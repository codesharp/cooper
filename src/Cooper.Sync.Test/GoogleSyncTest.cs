using System;
using System.Collections.Generic;
using System.Threading;
using CodeSharp.Core.Services;
using Cooper.Model.Accounts;
using DotNetOpenAuth.OAuth2;
using Google.Apis.Calendar.v3.Data;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using NUnit.Framework;
using CooperTask = Cooper.Model.Tasks.Task;
using GoogleTask = global::Google.Apis.Tasks.v1.Data.Task;
using MicrosoftAssert = Microsoft.VisualStudio.TestTools.UnitTesting.Assert;

namespace Cooper.Sync.Test
{
    [TestFixture]
    [TestClass]
    public class GoogleSyncTest : TestBase
    {
        private int _googleConnectionId = 18; //我的google账号连接

        /// <summary>
        /// 测试与Google同步的方法1
        /// </summary>
        [Test]
        [TestMethod]
        public void Test1_Sync_Tasks_And_CalendarEvents_With_Google()
        {
            Sync_Create_GoogleTask_AccordingWith_Task_Test();
            Sync_Update_GoogleTask_AccordingWith_Task_Test();
            Sync_Delete_GoogleTask_AccordingWith_Task_Test();
            Sync_Create_Task_AccordingWith_GoogleTask_Test();
            Sync_Update_Task_AccordingWith_GoogleTask_Test();

            Sync_Create_Task_AccordingWith_GoogleCalendarEvent_Test();
            Sync_Update_Task_AccordingWith_GoogleCalendarEvent_Test();
            Sync_Update_GoogleCalendarEvent_AccordingWith_Task_Test();
            Sync_Delete_GoogleCalendarEvent_AccordingWith_Task_Test();
        }

        #region Google 任务同步测试

        //由于并且任务系统不支持根据Google任务删除任务的功能，所以这个测试用例不需要写

        /// <summary>
        /// 测试同步时根据任务创建Google任务
        /// </summary>
        [Test]
        [TestMethod]
        public void Sync_Create_GoogleTask_AccordingWith_Task_Test()
        {
            _logger.Info("--------------开始执行测试：Sync_Create_GoogleTask_AccordingWith_Task_Test");
            InitializeAccountAndConnection(_googleConnectionId);

            var cooperTask = CreateCooperTask("cooper task", "description of task", DateTime.Now.Date.AddDays(2), false);
            MicrosoftAssert.IsNotNull(cooperTask);

            _syncProcessor.SyncTasksAndContacts(
                _googleConnectionId,
                new List<ISyncService<TaskSyncData, ISyncData, TaskSyncResult>>
                {
                    DependencyResolver.Resolve<IGoogleTaskSyncService>()
                },
                null);

            var syncInfo = GetSyncInfoByLocalDataId(_account.ID, cooperTask.ID.ToString(), SyncDataType.GoogleTask);
            MicrosoftAssert.IsNotNull(syncInfo);

            var googleTask = GetGoogleTask(syncInfo.SyncDataId);
            MicrosoftAssert.IsNotNull(googleTask);
            AssertTaskAndGoogleTaskAreEqual(cooperTask, googleTask);
        }
        /// <summary>
        /// 测试同步时根据任务更新Google任务
        /// </summary>
        [Test]
        [TestMethod]
        public void Sync_Update_GoogleTask_AccordingWith_Task_Test()
        {
            _logger.Info("--------------开始执行测试：Sync_Update_GoogleTask_AccordingWith_Task_Test");
            InitializeAccountAndConnection(_googleConnectionId);

            //首先创建
            var cooperTask = CreateCooperTask("cooper task or update test 0001", "description of task", DateTime.Now.Date.AddDays(2), false);
            MicrosoftAssert.IsNotNull(cooperTask);

            //同步
            _syncProcessor.SyncTasksAndContacts(
                _googleConnectionId,
                new List<ISyncService<TaskSyncData, ISyncData, TaskSyncResult>>
                {
                    DependencyResolver.Resolve<IGoogleTaskSyncService>()
                },
                null);

            //重新获取
            var syncInfo = GetSyncInfoByLocalDataId(_account.ID, cooperTask.ID.ToString(), SyncDataType.GoogleTask);
            MicrosoftAssert.IsNotNull(syncInfo);

            var googleTask = GetGoogleTask(syncInfo.SyncDataId);
            MicrosoftAssert.IsNotNull(googleTask);
            AssertTaskAndGoogleTaskAreEqual(cooperTask, googleTask);

            //更新Task
            cooperTask = UpdateCooperTask(cooperTask.ID, cooperTask.Subject + "_updated", cooperTask.Body + "_updated", cooperTask.DueTime.Value.Date.AddDays(1), true);
            UpdateTaskLastUpdateTime(cooperTask, Rfc3339DateTime.Parse(googleTask.Updated).ToLocalTime().AddSeconds(1));

            //同步
            _syncProcessor.SyncTasksAndContacts(
                _googleConnectionId,
                new List<ISyncService<TaskSyncData, ISyncData, TaskSyncResult>>
                {
                    DependencyResolver.Resolve<IGoogleTaskSyncService>()
                },
                null);

            //重新获取
            googleTask = GetGoogleTask(syncInfo.SyncDataId);

            //对比结果
            MicrosoftAssert.IsNotNull(googleTask);
            AssertTaskAndGoogleTaskAreEqual(cooperTask, googleTask);
        }
        /// <summary>
        /// 测试同步时根据任务删除Google任务
        /// </summary>
        [Test]
        [TestMethod]
        public void Sync_Delete_GoogleTask_AccordingWith_Task_Test()
        {
            _logger.Info("--------------开始执行测试：Sync_Delete_GoogleTask_AccordingWith_Task_Test");
            InitializeAccountAndConnection(_googleConnectionId);

            //首先创建
            var cooperTask = CreateCooperTask("cooper task", "description of task", DateTime.Now.Date.AddDays(2), false);
            MicrosoftAssert.IsNotNull(cooperTask);

            //同步
            _syncProcessor.SyncTasksAndContacts(
                _googleConnectionId,
                new List<ISyncService<TaskSyncData, ISyncData, TaskSyncResult>>
                {
                    DependencyResolver.Resolve<IGoogleTaskSyncService>()
                },
                null);

            //重新获取
            var syncInfo = GetSyncInfoByLocalDataId(_account.ID, cooperTask.ID.ToString(), SyncDataType.GoogleTask);
            MicrosoftAssert.IsNotNull(syncInfo);

            var googleTask = GetGoogleTask(syncInfo.SyncDataId);
            MicrosoftAssert.IsNotNull(googleTask);
            AssertTaskAndGoogleTaskAreEqual(cooperTask, googleTask);

            //删除本地Task
            DeleteCooperTask(cooperTask.ID);

            //同步
            _syncProcessor.SyncTasksAndContacts(
                _googleConnectionId,
                new List<ISyncService<TaskSyncData, ISyncData, TaskSyncResult>>
                {
                    DependencyResolver.Resolve<IGoogleTaskSyncService>()
                },
                null);

            //重新获取
            bool isGoogleTaskExist = IsGoogleTaskExist(syncInfo.SyncDataId);

            //Assert结果
            MicrosoftAssert.IsFalse(isGoogleTaskExist);
        }
        /// <summary>
        /// 测试同步时根据Google任务创建任务
        /// </summary>
        [Test]
        [TestMethod]
        public void Sync_Create_Task_AccordingWith_GoogleTask_Test()
        {
            _logger.Info("--------------开始执行测试：Sync_Create_Task_AccordingWith_GoogleTask_Test");
            InitializeAccountAndConnection(_googleConnectionId);

            var googleTask = CreateGoogleTask("cooper task 00001", "description of task", DateTime.Now.Date.AddDays(2), false);
            MicrosoftAssert.IsNotNull(googleTask);

            _syncProcessor.SyncTasksAndContacts(
                _googleConnectionId,
                new List<ISyncService<TaskSyncData, ISyncData, TaskSyncResult>>
                {
                    DependencyResolver.Resolve<IGoogleTaskSyncService>()
                },
                null);

            var syncInfo = GetSyncInfoBySyncDataId(_account.ID, googleTask.Id, SyncDataType.GoogleTask);
            MicrosoftAssert.IsNotNull(syncInfo);

            var cooperTask = GetCooperTask(long.Parse(syncInfo.LocalDataId));
            googleTask = GetGoogleTask(googleTask.Id);
            MicrosoftAssert.IsNotNull(cooperTask);
            AssertTaskAndGoogleTaskAreEqual(cooperTask, googleTask);
        }
        /// <summary>
        /// 测试同步时根据Google任务更新任务
        /// </summary>
        [Test]
        [TestMethod]
        public void Sync_Update_Task_AccordingWith_GoogleTask_Test()
        {
            _logger.Info("--------------开始执行测试：Sync_Update_Task_AccordingWith_GoogleTask_Test");
            InitializeAccountAndConnection(_googleConnectionId);

            var dueDate = DateTime.Now.Date.AddDays(2);
            var googleTask = CreateGoogleTask("cooper task 00001", "description of task", dueDate, false);
            MicrosoftAssert.IsNotNull(googleTask);

            _syncProcessor.SyncTasksAndContacts(
                _googleConnectionId,
                new List<ISyncService<TaskSyncData, ISyncData, TaskSyncResult>>
                {
                    DependencyResolver.Resolve<IGoogleTaskSyncService>()
                },
                null);

            var syncInfo = GetSyncInfoBySyncDataId(_account.ID, googleTask.Id, SyncDataType.GoogleTask);
            MicrosoftAssert.IsNotNull(syncInfo);

            var cooperTask = GetCooperTask(long.Parse(syncInfo.LocalDataId));
            googleTask = GetGoogleTask(googleTask.Id);
            MicrosoftAssert.IsNotNull(cooperTask);
            AssertTaskAndGoogleTaskAreEqual(cooperTask, googleTask);

            Thread.Sleep(1000 * 2);
            googleTask = UpdateGoogleTask(googleTask.Id, googleTask.Title + "_updated", googleTask.Notes + "_updated", dueDate.AddDays(1), true);

            _syncProcessor.SyncTasksAndContacts(
                _googleConnectionId,
                new List<ISyncService<TaskSyncData, ISyncData, TaskSyncResult>>
                {
                    DependencyResolver.Resolve<IGoogleTaskSyncService>()
                },
                null);

            cooperTask = GetCooperTask(long.Parse(syncInfo.LocalDataId));
            googleTask = GetGoogleTask(googleTask.Id);
            MicrosoftAssert.IsNotNull(cooperTask);
            AssertTaskAndGoogleTaskAreEqual(cooperTask, googleTask);
        }

        #endregion

        #region Google 日历事件同步测试

        //由于1）日历的事件不会主动从任务系统同步过去，2）并且任务系统不支持根据Google日历事件删除任务的功能
        //所以这两个测试用例不需要写

        /// <summary>
        /// 测试同步时根据Google日历事件创建任务
        /// </summary>
        [Test]
        [TestMethod]
        public void Sync_Create_Task_AccordingWith_GoogleCalendarEvent_Test()
        {
            _logger.Info("--------------开始执行测试：Sync_Create_Task_AccordingWith_GoogleCalendarEvent_Test");
            InitializeAccountAndConnection(_googleConnectionId);

            var googleCalendarEvent = CreateGoogleCalendarEvent("cooper calendar event", "description of event", DateTime.Now, DateTime.Now.AddHours(2));
            MicrosoftAssert.IsNotNull(googleCalendarEvent);

            _syncProcessor.SyncTasksAndContacts(
                _googleConnectionId,
                new List<ISyncService<TaskSyncData, ISyncData, TaskSyncResult>>
                {
                    DependencyResolver.Resolve<IGoogleCalendarEventSyncService>()
                },
                null);

            var syncInfo = GetSyncInfoBySyncDataId(_account.ID, googleCalendarEvent.Id, SyncDataType.GoogleCalendarEvent);
            MicrosoftAssert.IsNotNull(syncInfo);

            var cooperTask = GetCooperTask(long.Parse(syncInfo.LocalDataId));
            googleCalendarEvent = GetGoogleCalendarEvent(googleCalendarEvent.Id);
            MicrosoftAssert.IsNotNull(cooperTask);
            AssertTaskAndGoogleCalendarEventAreEqual(cooperTask, googleCalendarEvent);
        }
        /// <summary>
        /// 测试同步时根据Google日历事件更新任务
        /// </summary>
        [Test]
        [TestMethod]
        public void Sync_Update_Task_AccordingWith_GoogleCalendarEvent_Test()
        {
            _logger.Info("--------------开始执行测试：Sync_Update_Task_AccordingWith_GoogleCalendarEvent_Test");
            InitializeAccountAndConnection(_googleConnectionId);

            var googleCalendarEvent = CreateGoogleCalendarEvent("cooper calendar event", "description of event", DateTime.Now, DateTime.Now.AddHours(2));
            MicrosoftAssert.IsNotNull(googleCalendarEvent);

            _syncProcessor.SyncTasksAndContacts(
                _googleConnectionId,
                new List<ISyncService<TaskSyncData, ISyncData, TaskSyncResult>>
                {
                    DependencyResolver.Resolve<IGoogleCalendarEventSyncService>()
                },
                null);

            var syncInfo = GetSyncInfoBySyncDataId(_account.ID, googleCalendarEvent.Id, SyncDataType.GoogleCalendarEvent);
            MicrosoftAssert.IsNotNull(syncInfo);

            var cooperTask = GetCooperTask(long.Parse(syncInfo.LocalDataId));
            MicrosoftAssert.IsNotNull(cooperTask);
            AssertTaskAndGoogleCalendarEventAreEqual(cooperTask, googleCalendarEvent);

            Thread.Sleep(1000 * 2);
            googleCalendarEvent = UpdateGoogleCalendarEvent(googleCalendarEvent.Id, googleCalendarEvent.Summary + "_updated", googleCalendarEvent.Description + "_updated");

            _syncProcessor.SyncTasksAndContacts(
                _googleConnectionId,
                new List<ISyncService<TaskSyncData, ISyncData, TaskSyncResult>>
                {
                    DependencyResolver.Resolve<IGoogleCalendarEventSyncService>()
                },
                null);

            cooperTask = GetCooperTask(cooperTask.ID);
            googleCalendarEvent = GetGoogleCalendarEvent(googleCalendarEvent.Id);
            MicrosoftAssert.IsNotNull(cooperTask);
            AssertTaskAndGoogleCalendarEventAreEqual(cooperTask, googleCalendarEvent);
        }
        /// <summary>
        /// 测试同步时根据任务更新Google日历事件
        /// </summary>
        [Test]
        [TestMethod]
        public void Sync_Update_GoogleCalendarEvent_AccordingWith_Task_Test()
        {
            _logger.Info("--------------开始执行测试：Sync_Update_GoogleCalendarEvent_AccordingWith_Task_Test");
            InitializeAccountAndConnection(_googleConnectionId);

            var googleCalendarEvent = CreateGoogleCalendarEvent("cooper calendar event", "description of event", DateTime.Now, DateTime.Now.AddHours(2));
            MicrosoftAssert.IsNotNull(googleCalendarEvent);

            _syncProcessor.SyncTasksAndContacts(
                _googleConnectionId,
                new List<ISyncService<TaskSyncData, ISyncData, TaskSyncResult>>
                {
                    DependencyResolver.Resolve<IGoogleCalendarEventSyncService>()
                },
                null);

            var syncInfo = GetSyncInfoBySyncDataId(_account.ID, googleCalendarEvent.Id, SyncDataType.GoogleCalendarEvent);
            MicrosoftAssert.IsNotNull(syncInfo);

            var cooperTask = GetCooperTask(long.Parse(syncInfo.LocalDataId));
            MicrosoftAssert.IsNotNull(cooperTask);
            AssertTaskAndGoogleCalendarEventAreEqual(cooperTask, googleCalendarEvent);

            //更新Task
            cooperTask = UpdateCooperTask(cooperTask.ID, cooperTask.Subject + "_updated", cooperTask.Body + "_updated", cooperTask.DueTime.Value.Date.AddDays(1), true);
            var lastUpdateTime = Rfc3339DateTime.Parse(googleCalendarEvent.Updated).ToLocalTime();
            UpdateTaskLastUpdateTime(cooperTask, lastUpdateTime.AddSeconds(1));

            //同步
            _syncProcessor.SyncTasksAndContacts(
                _googleConnectionId,
                new List<ISyncService<TaskSyncData, ISyncData, TaskSyncResult>>
                {
                    DependencyResolver.Resolve<IGoogleCalendarEventSyncService>()
                },
                null);

            //重新获取
            cooperTask = GetCooperTask(cooperTask.ID);
            googleCalendarEvent = GetGoogleCalendarEvent(syncInfo.SyncDataId);

            //对比结果
            MicrosoftAssert.IsNotNull(googleCalendarEvent);
            MicrosoftAssert.AreEqual(cooperTask.Subject, googleCalendarEvent.Summary);
            MicrosoftAssert.AreEqual(cooperTask.Body, googleCalendarEvent.Description);
            MicrosoftAssert.AreEqual(FormatTime(cooperTask.LastUpdateTime), FormatTime(Rfc3339DateTime.Parse(googleCalendarEvent.Updated).ToLocalTime()));
        }
        /// <summary>
        /// 测试同步时根据任务删除Google日历事件
        /// </summary>
        [Test]
        [TestMethod]
        public void Sync_Delete_GoogleCalendarEvent_AccordingWith_Task_Test()
        {
            _logger.Info("--------------开始执行测试：Sync_Delete_GoogleCalendarEvent_AccordingWith_Task_Test");
            InitializeAccountAndConnection(_googleConnectionId);

            var googleCalendarEvent = CreateGoogleCalendarEvent("cooper calendar event", "description of event", DateTime.Now, DateTime.Now.AddHours(2));
            MicrosoftAssert.IsNotNull(googleCalendarEvent);

            _syncProcessor.SyncTasksAndContacts(
                _googleConnectionId,
                new List<ISyncService<TaskSyncData, ISyncData, TaskSyncResult>>
                {
                    DependencyResolver.Resolve<IGoogleCalendarEventSyncService>()
                },
                null);

            var syncInfo = GetSyncInfoBySyncDataId(_account.ID, googleCalendarEvent.Id, SyncDataType.GoogleCalendarEvent);
            MicrosoftAssert.IsNotNull(syncInfo);

            var cooperTask = GetCooperTask(long.Parse(syncInfo.LocalDataId));
            MicrosoftAssert.IsNotNull(cooperTask);
            AssertTaskAndGoogleCalendarEventAreEqual(cooperTask, googleCalendarEvent);

            //删除Task
            DeleteCooperTask(cooperTask.ID);

            //同步
            _syncProcessor.SyncTasksAndContacts(
                _googleConnectionId,
                new List<ISyncService<TaskSyncData, ISyncData, TaskSyncResult>>
                {
                    DependencyResolver.Resolve<IGoogleCalendarEventSyncService>()
                },
                null);

            //重新获取
            var isGoogleCalendarEventExist = IsGoogleCalendarEventExist(syncInfo.SyncDataId);

            //检查结果
            MicrosoftAssert.IsFalse(isGoogleCalendarEventExist);
        }

        #endregion

        #region Helper Methods

        /// <summary>
        /// 初始化Account以及Connection
        /// </summary>
        /// <param name="connectionId"></param>
        private void InitializeAccountAndConnection(int connectionId)
        {
            var connection = _accountConnectionService.GetConnection(connectionId);
            if (connection != null)
            {
                _account = _accountService.GetAccount(connection.AccountId);
                _googleConnection = connection as GoogleConnection;
            }
        }
        /// <summary>
        /// 获取账号的Google认证信息，即Token信息
        /// </summary>
        private IAuthorizationState GetGoogleUserToken()
        {
            if (_googleConnection != null)
            {
                try
                {
                    var token = _googleTokenService.DeserializeToken(_googleConnection.Token);

                    //这里总是刷新Token
                    _googleTokenService.RefreshToken(token);

                    _googleConnection.SetToken(_googleTokenService.SerializeToken(token));
                    _accountConnectionService.Update(_googleConnection);

                    return token;
                }
                catch (Exception ex)
                {
                    _logger.Error("GetGoogleUserToken has exception.", ex);
                }
            }
            return null;
        }

        private CooperTask CreateCooperTask(string subject, string body, DateTime? dueTime, bool isCompleted)
        {
            CooperTask task = new CooperTask(_account);

            task.SetSubject(subject);
            task.SetBody(body);
            task.SetDueTime(dueTime);
            if (isCompleted)
            {
                task.MarkAsCompleted();
            }
            else
            {
                task.MarkAsInCompleted();
            }

            _taskService.Create(task);

            return task;
        }
        private CooperTask UpdateCooperTask(long taskId, string subject, string body, DateTime? dueTime, bool isCompleted)
        {
            CooperTask task = _taskService.GetTask(taskId);

            task.SetSubject(subject);
            task.SetBody(body);
            task.SetDueTime(dueTime);
            if (isCompleted)
            {
                task.MarkAsCompleted();
            }
            else
            {
                task.MarkAsInCompleted();
            }

            _taskService.Update(task);

            return task;
        }
        private CooperTask GetCooperTask(long taskId)
        {
            return _taskService.GetTask(taskId);
        }
        private void DeleteCooperTask(long taskId)
        {
            _taskService.Delete(_taskService.GetTask(taskId));
        }
        /// <summary>
        /// 更新任务的最后更新时间
        /// </summary>
        private void UpdateTaskLastUpdateTime(CooperTask task, DateTime lastUpdateTime)
        {
            task.SetLastUpdateTime(lastUpdateTime);
            _taskService.Update(task);
        }

        private SyncInfo GetSyncInfoByLocalDataId(int accountId, string localDataId, SyncDataType syncDataType)
        {
            var sql = string.Format(
                "select AccountId,LocalDataId,SyncDataId,SyncDataType from Cooper_SyncInfo where AccountId={0} and LocalDataId='{1}' and SyncDataType={2}",
                accountId,
                localDataId,
                (int)syncDataType);

            var query = _sessionManager.OpenSession().CreateSQLQuery(sql);
            var objectArrayList = query.List();

            if (objectArrayList.Count > 0)
            {
                object[] objectArray = objectArrayList[0] as object[];

                SyncInfo syncInfo = new SyncInfo();
                syncInfo.AccountId = int.Parse(objectArray[0].ToString());
                syncInfo.LocalDataId = objectArray[1].ToString();
                syncInfo.SyncDataId = objectArray[2].ToString();
                syncInfo.SyncDataType = int.Parse(objectArray[3].ToString());

                return syncInfo;
            }

            return null;
        }
        private SyncInfo GetSyncInfoBySyncDataId(int accountId, string syncDataId, SyncDataType syncDataType)
        {
            var sql = string.Format(
                "select AccountId,LocalDataId,SyncDataId,SyncDataType from Cooper_SyncInfo where AccountId={0} and SyncDataId='{1}' collate Chinese_PRC_CS_AI and SyncDataType={2}",
                accountId,
                syncDataId,
                (int)syncDataType);

            var query = _sessionManager.OpenSession().CreateSQLQuery(sql);
            var objectArrayList = query.List();

            if (objectArrayList.Count > 0)
            {
                object[] objectArray = objectArrayList[0] as object[];

                SyncInfo syncInfo = new SyncInfo();
                syncInfo.AccountId = int.Parse(objectArray[0].ToString());
                syncInfo.LocalDataId = objectArray[1].ToString();
                syncInfo.SyncDataId = objectArray[2].ToString();
                syncInfo.SyncDataType = int.Parse(objectArray[3].ToString());

                return syncInfo;
            }

            return null;
        }

        private GoogleTask CreateGoogleTask(string subject, string body, DateTime dueTime, bool isCompleted)
        {
            var token = GetGoogleUserToken();
            var googleTaskService = _externalServiceProvider.GetGoogleTaskService(token);
            var isDefaultTaskListExist = false;
            var taskList = DependencyResolver.Resolve<IGoogleTaskSyncDataService>().GetDefaultTaskList(token, out isDefaultTaskListExist);

            var task = new GoogleTask();

            task.Title = subject;
            task.Notes = body;

            var dueTimeUtcFormat = new DateTime(dueTime.Year, dueTime.Month, dueTime.Day, 0, 0, 0, DateTimeKind.Utc);
            task.Due = Rfc3339DateTime.ToString(dueTimeUtcFormat);

            if (isCompleted)
            {
                task.Status = "completed";
            }
            else
            {
                task.Status = "needsAction";
            }

            task = googleTaskService.Tasks.Insert(task, taskList.Id).Fetch();

            return task;
        }
        private GoogleTask UpdateGoogleTask(string id, string subject, string body, DateTime dueTime, bool isCompleted)
        {
            var token = GetGoogleUserToken();
            var googleTaskService = _externalServiceProvider.GetGoogleTaskService(token);
            var isDefaultTaskListExist = false;
            var taskList = DependencyResolver.Resolve<IGoogleTaskSyncDataService>().GetDefaultTaskList(token, out isDefaultTaskListExist);

            var task = GetGoogleTask(id);

            task.Title = subject;
            task.Notes = body;

            var dueTimeUtcFormat = new DateTime(dueTime.Year, dueTime.Month, dueTime.Day, 0, 0, 0, DateTimeKind.Utc);
            task.Due = Rfc3339DateTime.ToString(dueTimeUtcFormat);

            if (isCompleted)
            {
                task.Status = "completed";
            }
            else
            {
                task.Status = "needsAction";
            }

            googleTaskService.Tasks.Update(task, taskList.Id, task.Id).Fetch();

            task = GetGoogleTask(id);

            return task;
        }
        private GoogleTask GetGoogleTask(string id)
        {
            var token = GetGoogleUserToken();
            var googleTaskService = _externalServiceProvider.GetGoogleTaskService(token);
            bool isDefaultTaskListExist = false;
            var defaultTaskList = DependencyResolver.Resolve<IGoogleTaskSyncDataService>().GetDefaultTaskList(token, out isDefaultTaskListExist);
            return googleTaskService.Tasks.Get(defaultTaskList.Id, id).Fetch();
        }
        private bool IsGoogleTaskExist(string id)
        {
            var task = GetGoogleTask(id);
            return task != null && task.Deleted != null && !task.Deleted.Value;
        }
        private void AssertTaskAndGoogleTaskAreEqual(CooperTask cooperTask, GoogleTask googleTask)
        {
            MicrosoftAssert.AreEqual(cooperTask.Subject, googleTask.Title);
            MicrosoftAssert.AreEqual(cooperTask.Body, googleTask.Notes);
            var dueDate = Rfc3339DateTime.Parse(googleTask.Due);
            MicrosoftAssert.AreEqual(cooperTask.DueTime, new DateTime(dueDate.Year, dueDate.Month, dueDate.Day));
            if (cooperTask.IsCompleted)
            {
                MicrosoftAssert.AreEqual("completed", googleTask.Status);
            }
            else
            {
                MicrosoftAssert.AreEqual("needsAction", googleTask.Status);
            }

            var lastUpdateTime = Rfc3339DateTime.Parse(googleTask.Updated).ToLocalTime();
            MicrosoftAssert.AreEqual(FormatTime(cooperTask.LastUpdateTime), FormatTime(lastUpdateTime));
        }

        private Event CreateGoogleCalendarEvent(string subject, string body, DateTime startTime, DateTime endTime)
        {
            var calendarEvent = new Event();
            calendarEvent.Summary = subject;
            calendarEvent.Description = body;

            var startTimeUtc = new DateTime(startTime.Year, startTime.Month, startTime.Day, startTime.Hour, startTime.Minute, startTime.Second, DateTimeKind.Utc);
            var endTimeUtc = new DateTime(endTime.Year, endTime.Month, endTime.Day, endTime.Hour, endTime.Minute, endTime.Second, DateTimeKind.Utc);
            string start = Rfc3339DateTime.ToString(startTimeUtc);
            string end = Rfc3339DateTime.ToString(endTimeUtc);
            calendarEvent.Start = new EventDateTime() { DateTime = start };
            calendarEvent.End = new EventDateTime() { DateTime = end };

            var token = GetGoogleUserToken();
            var googleCalendarService = _externalServiceProvider.GetGoogleCalendarService(token);
            bool isDefaultCalendarExist = false;
            var defaultCalendar = DependencyResolver.Resolve<IGoogleCalendarEventSyncDataService>().GetDefaultCalendar(token, out isDefaultCalendarExist);
            calendarEvent = googleCalendarService.Events.Insert(calendarEvent, defaultCalendar.Id).Fetch();

            calendarEvent = GetGoogleCalendarEvent(calendarEvent.Id);

            return calendarEvent;
        }
        private Event UpdateGoogleCalendarEvent(string id, string subject, string body)
        {
            var calendarEvent = GetGoogleCalendarEvent(id);

            calendarEvent.Summary = subject;
            calendarEvent.Description = body;

            var token = GetGoogleUserToken();
            var googleCalendarService = _externalServiceProvider.GetGoogleCalendarService(token);
            bool isDefaultCalendarExist = false;
            var defaultCalendar = DependencyResolver.Resolve<IGoogleCalendarEventSyncDataService>().GetDefaultCalendar(token, out isDefaultCalendarExist);
            googleCalendarService.Events.Update(calendarEvent, defaultCalendar.Id, calendarEvent.Id).Fetch();

            calendarEvent = GetGoogleCalendarEvent(calendarEvent.Id);

            return calendarEvent;
        }
        private Event GetGoogleCalendarEvent(string id)
        {
            var token = GetGoogleUserToken();
            var googleCalendarService = _externalServiceProvider.GetGoogleCalendarService(token);
            bool isDefaultCalendarExist = false;
            var defaultCalendar = DependencyResolver.Resolve<IGoogleCalendarEventSyncDataService>().GetDefaultCalendar(token, out isDefaultCalendarExist);
            var calendarEvent = googleCalendarService.Events.Get(defaultCalendar.Id, id).Fetch();

            return calendarEvent;
        }
        private bool IsGoogleCalendarEventExist(string id)
        {
            var calendarEvent = GetGoogleCalendarEvent(id);
            return calendarEvent != null && calendarEvent.Status == "confirmed";
        }
        private void AssertTaskAndGoogleCalendarEventAreEqual(CooperTask cooperTask, Event calendarEvent)
        {
            MicrosoftAssert.AreEqual(cooperTask.Subject, calendarEvent.Summary);
            MicrosoftAssert.AreEqual(cooperTask.Body, calendarEvent.Description);
            MicrosoftAssert.AreEqual(cooperTask.DueTime.Value.Date, Rfc3339DateTime.Parse(calendarEvent.End.DateTime).ToLocalTime().Date);
            MicrosoftAssert.AreEqual(FormatTime(cooperTask.LastUpdateTime), FormatTime(Rfc3339DateTime.Parse(calendarEvent.Updated).ToLocalTime()));
        }
        private DateTime FormatTime(DateTime originalTime)
        {
            return new DateTime(originalTime.Year,
                                originalTime.Month,
                                originalTime.Day,
                                originalTime.Hour,
                                originalTime.Minute,
                                originalTime.Second);
        }

        #endregion
    }
}
