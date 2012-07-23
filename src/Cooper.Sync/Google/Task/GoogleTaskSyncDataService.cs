using System;
using System.Collections.Generic;
using System.Linq;
using DotNetOpenAuth.OAuth2;
using Google.Apis.Tasks.v1;
using Google.Apis.Tasks.v1.Data;
using CodeSharp.Core;
using CodeSharp.Core.Services;
using GoogleTask = Google.Apis.Tasks.v1.Data.Task;

namespace Cooper.Sync
{
    public interface IGoogleTaskSyncDataService : IGoogleSyncService, ISyncDataService<GoogleTaskSyncData, TaskSyncData>
    {
        TaskList GetDefaultTaskList(IAuthorizationState token, out bool isDefaultTaskListExist);
    }
    public class GoogleTaskSyncDataService : IGoogleTaskSyncDataService
    {
        #region Private Variables

        private IAuthorizationState _token;
        private TasksService _googleTaskService;
        private IExternalServiceProvider _externalServiceProvider;
        private ILog _logger;

        #endregion

        public GoogleTaskSyncDataService(IExternalServiceProvider externalServiceProvider, ILoggerFactory loggerFactory)
        {
            _externalServiceProvider = externalServiceProvider;
            _logger = loggerFactory.Create(GetType());
        }

        public IList<GoogleTaskSyncData> GetSyncDataList()
        {
            bool isDefaultTaskListExist = false;

            _logger.InfoFormat("Calling method GetDefaultTaskList in GoogleTaskSyncDataService.GetSyncDataList, time:{0}", DateTime.Now);
            var taskList = GetDefaultTaskList(_token, out isDefaultTaskListExist);

            bool isFromDefault = true;
            var listRequest = _googleTaskService.Tasks.List(taskList.Id);
            listRequest.MaxResults = GoogleSyncSettings.DefaultMaxTaskCount.ToString();
            var googleTasks = listRequest.Fetch().Items;

            if (!isDefaultTaskListExist)
            {
                //默认的不存在，不从Google默认的任务列表读取任务
                //googleTasks = _googleTaskService.Tasks.List("@default").Fetch().Items;
                isFromDefault = false;
            }

            List<GoogleTaskSyncData> items = new List<GoogleTaskSyncData>();

            if (googleTasks != null && googleTasks.Count() > 0)
            {
                foreach (var googleTask in googleTasks)
                {
                    if (googleTask.Deleted == null || !googleTask.Deleted.Value)
                    {
                        var syncData = new GoogleTaskSyncData(googleTask);
                        syncData.IsFromDefault = isFromDefault;
                        items.Add(syncData);
                    }
                }
            }

            return items;
        }
        public GoogleTaskSyncData CreateFrom(TaskSyncData syncDataSource)
        {
            var task = new GoogleTask();

            task.Title = syncDataSource.Subject;
            task.Notes = syncDataSource.Body;
            if (syncDataSource.DueTime != null)
            {
                var dueTime = syncDataSource.DueTime.Value;
                var dueTimeUtcFormat = new DateTime(dueTime.Year, dueTime.Month, dueTime.Day, 0, 0, 0, DateTimeKind.Utc);
                task.Due = Rfc3339DateTime.ToString(dueTimeUtcFormat);
            }

            if (syncDataSource.IsCompleted)
            {
                task.Status = "completed";
            }
            else
            {
                task.Status = "needsAction";
            }

            return new GoogleTaskSyncData(task);
        }
        public void UpdateSyncData(GoogleTaskSyncData syncData, TaskSyncData syncDataSource)
        {
            syncData.GoogleTask.Title = syncDataSource.Subject;
            syncData.GoogleTask.Notes = syncDataSource.Body;
            if (syncDataSource.DueTime != null)
            {
                var dueTime = syncDataSource.DueTime.Value;
                var dueTimeUtcFormat = new DateTime(dueTime.Year, dueTime.Month, dueTime.Day, 0, 0, 0, DateTimeKind.Utc);
                syncData.GoogleTask.Due = Rfc3339DateTime.ToString(dueTimeUtcFormat);
            }
            else
            {
                syncData.GoogleTask.Due = null;
            }

            if (syncDataSource.IsCompleted)
            {
                syncData.GoogleTask.Status = "completed";
                var currentDate = DateTime.Now.Date;
                var currentDateUtcFormat = new DateTime(currentDate.Year, currentDate.Month, currentDate.Day, 0, 0, 0, DateTimeKind.Utc);
                syncData.GoogleTask.Completed = Rfc3339DateTime.ToString(currentDateUtcFormat);
            }
            else
            {
                syncData.GoogleTask.Status = "needsAction";
                syncData.GoogleTask.Completed = null;
            }
        }

        public void SetGoogleToken(IAuthorizationState token)
        {
            _token = token;
        }
        public TaskList GetDefaultTaskList(IAuthorizationState token, out bool isDefaultTaskListExist)
        {
            _logger.InfoFormat("GetDefaultTaskList method is entered, time:{0}", DateTime.Now);

            TaskList taskList = null;

            try
            {
                isDefaultTaskListExist = false;
                SetGoogleToken(token);
                _googleTaskService = _externalServiceProvider.GetGoogleTaskService(_token);

                var listRequest = _googleTaskService.Tasklists.List();
                listRequest.MaxResults = GoogleSyncSettings.DefaultMaxTaskListCount.ToString();
                var totalTaskLists = listRequest.Fetch().Items;

                _logger.InfoFormat("----返回的CooperTaskList的总个数:{0}，明细如下：--------", totalTaskLists.Count());
                foreach (var list in totalTaskLists)
                {
                    _logger.InfoFormat("--------TaskList title:{0}, id:{1}, last update time(RFC 3339 time format):{2}", list.Title, list.Id, list.Updated);
                }

                var defaultTaskListList = totalTaskLists.Where(x => x.Title == GoogleSyncSettings.DefaultTaskListName).ToList();
                var totalDefaultTaskListCount = defaultTaskListList.Count();
                _logger.InfoFormat("----默认CooperTaskList的总个数:{0}----", totalDefaultTaskListCount);

                if (totalDefaultTaskListCount == 0)
                {
                    taskList = _googleTaskService.Tasklists.Insert(new TaskList() { Title = GoogleSyncSettings.DefaultTaskListName }).Fetch();
                    _logger.Info("----默认CooperTaskList不存在，故一个默认的CooperTaskList已被创建。");
                }
                else
                {
                    //如果默认的任务列表多余1个，则删除多余的默认任务列表，只保留第一个默认任务列表
                    if (totalDefaultTaskListCount > 1)
                    {
                        _logger.Error("----开始删除多余的默认CooperTaskList");
                        int totalDeletedTaskListCount = 0;
                        var taskListIdList = defaultTaskListList.Select(x => x.Id).ToList();
                        for (int index = 1; index < totalDefaultTaskListCount; index++)
                        {
                            //删除任务列表
                            var list = defaultTaskListList[index];
                            _googleTaskService.Tasklists.Delete(taskListIdList[index]).Fetch();
                            _logger.InfoFormat("----删除了一个多余的默认CooperTaskList, title:{0}, id:{1}, last update time(RFC 3339 time format):{2}", list.Title, list.Id, list.Updated);
                            totalDeletedTaskListCount++;
                        }

                        _logger.InfoFormat("----被删除的CooperTaskList总个数:{0}", totalDeletedTaskListCount);
                    }

                    taskList = defaultTaskListList.First();
                    isDefaultTaskListExist = true;
                }
            }
            catch (Exception ex)
            {
                _logger.ErrorFormat("尝试获取默认任务列表时出现异常：", ex);
                throw;
            }

            _logger.InfoFormat("GetDefaultTaskList method is exited, time:{0}", DateTime.Now);

            return taskList;
        }
    }
}
