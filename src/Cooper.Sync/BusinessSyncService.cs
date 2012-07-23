using System;
using System.Collections.Generic;
using CodeSharp.Core;
using CodeSharp.Core.Services;

namespace Cooper.Sync
{
    /// <summary>
    /// 定义Cooper任务系统与外部系统同步数据的接口
    /// </summary>
    public interface IBusinessSyncService
    {
        /// <summary>
        /// 注册同步任务的服务
        /// </summary>
        void RegisterTaskSyncService(ISyncService<TaskSyncData, ISyncData, TaskSyncResult> taskSyncService);
        /// <summary>
        /// 注册同步联系人的服务
        /// </summary>
        void RegisterContactSyncService(ISyncService<ContactSyncData, ISyncData, ContactSyncResult> contactSyncService);

        /// <summary>
        /// 对某个帐号的任务与外部系统（如Google）进行双向同步
        /// 该接口会在用户人工选择同步数据时被调用；
        /// </summary>
        TaskSyncResult SyncTasks(IEnumerable<TaskSyncData> tasks, IEnumerable<SyncInfo> syncInfos);
        /// <summary>
        /// 对某个帐号的联系人与外部系统（如Google）进行双向同步
        /// 该接口会在用户人工选择同步数据时被调用；
        /// </summary>
        ContactSyncResult SyncContacts(IEnumerable<ContactSyncData> contacts, IEnumerable<SyncInfo> syncInfos);
    }

    /// <summary>
    /// Cooper任务系统与外部系统同步数据的服务，目前支持任务以及联系人与外部系统同步。
    /// </summary>
    public class BusinessSyncService : IBusinessSyncService
    {
        private List<ISyncService<TaskSyncData, ISyncData, TaskSyncResult>> _taskSyncServices;
        private List<ISyncService<ContactSyncData, ISyncData, ContactSyncResult>> _contactSyncServices;
        private ILog _logger;

        public BusinessSyncService(ILoggerFactory loggerFactory)
        {
            _taskSyncServices = new List<ISyncService<TaskSyncData, ISyncData, TaskSyncResult>>();
            _contactSyncServices = new List<ISyncService<ContactSyncData, ISyncData, ContactSyncResult>>();
            _logger = loggerFactory.Create(GetType());
        }

        /// <summary>
        /// 注册与任务系统的任务进行数据同步的服务
        /// </summary>
        public void RegisterTaskSyncService(ISyncService<TaskSyncData, ISyncData, TaskSyncResult> taskSyncService)
        {
            _taskSyncServices.Add(taskSyncService);
        }
        /// <summary>
        /// 注册与任务系统的联系人进行数据同步的服务
        /// </summary>
        public void RegisterContactSyncService(ISyncService<ContactSyncData, ISyncData, ContactSyncResult> contactSyncService)
        {
            _contactSyncServices.Add(contactSyncService);
        }

        /// <summary>
        /// 根据通过RegisterTaskSyncService方法注册的同步服务，对任务进行双向同步
        /// </summary>
        /// <param name="tasks">与某个账号相关的所有任务</param>
        /// <param name="syncInfos">与某个账号相关的所有同步信息</param>
        /// <returns>同步结果，包含本地任务数据的增删改，以及外部数据源数据的增删改</returns>
        public TaskSyncResult SyncTasks(IEnumerable<TaskSyncData> tasks, IEnumerable<SyncInfo> syncInfos)
        {
            TaskSyncResult syncResult = new TaskSyncResult();

            foreach (var taskSyncService in _taskSyncServices)
            {
                TaskSyncResult result = null;
                bool success = false;
                try
                {
                    _logger.InfoFormat("----开始使用同步服务【{0}】比对任务数据", taskSyncService.GetType().FullName);
                    result = taskSyncService.ProcessTwoWaySynchronization(tasks, syncInfos);
                    _logger.InfoFormat("----结束使用同步服务【{0}】比对任务数据", taskSyncService.GetType().FullName);
                    success = true;
                }
                catch (Exception ex)
                {
                    _logger.Error(string.Format("{0}.ProcessTwoWaySynchronization has exception.", taskSyncService.GetType().Name), ex);
                }

                if (success)
                {
                    syncResult.MergeResult(result);
                }
            }

            return syncResult;
        }
        /// <summary>
        /// 根据通过RegisterContactSyncService方法注册的同步服务，对任联系人进行双向同步
        /// </summary>
        /// <param name="contacts">与某个账号相关的所有联系人</param>
        /// <param name="syncInfos">与某个账号相关的所有同步信息</param>
        /// <returns>同步结果，包含本地联系人数据的增删改，以及外部数据源数据的增删改</returns>
        public ContactSyncResult SyncContacts(IEnumerable<ContactSyncData> contacts, IEnumerable<SyncInfo> syncInfos)
        {
            ContactSyncResult syncResult = new ContactSyncResult();

            foreach (var contactSyncService in _contactSyncServices)
            {
                ContactSyncResult result = null;
                bool success = false;
                try
                {
                    result = contactSyncService.ProcessTwoWaySynchronization(contacts, syncInfos);
                    success = true;
                }
                catch (Exception ex)
                {
                    _logger.Error(string.Format("{0}.ProcessTwoWaySynchronization has exception.", contactSyncService.GetType().Name), ex);
                }

                if (success)
                {
                    syncResult.MergeResult(result);
                }
            }

            return syncResult;
        }
    }
}
