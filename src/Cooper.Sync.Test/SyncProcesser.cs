using System;
using System.Collections.Generic;
using System.Linq;
using Castle.Facilities.NHibernateIntegration;
using CodeSharp.Core;
using CodeSharp.Core.Services;
using Cooper.Model;
using Cooper.Model.Accounts;
using Cooper.Model.Tasks;
using CooperTask = Cooper.Model.Tasks.Task;

namespace Cooper.Sync.Test
{
    /// <summary>
    /// 数据同步处理器抽象基类
    /// </summary>
    public abstract class SyncProcesser
    {
        #region Private Members

        protected ILog _logger;
        protected Account _account;
        protected ITaskService _taskService;
        protected IAccountService _accountService;
        protected IAccountConnectionService _accountConnectionService;
        protected IAccountHelper _accountHelper;
        protected ISessionManager _sessionManager;
        protected IExternalServiceProvider _externalServiceProvider;

        #endregion

        #region Constructors

        public SyncProcesser(
            ILockHelper lockHelper,
            ILoggerFactory loggerFactory,
            ISessionManager sessionManager,
            IAccountHelper accountHelper,
            IAccountService accountService,
            IAccountConnectionService accountConnectionService,
            ITaskService taskService,
            IExternalServiceProvider externalServiceProvider
            )
        {
            //初始化同步锁
            lockHelper.Init<Account>();

            _logger = loggerFactory.Create(GetType());
            _sessionManager = sessionManager;
            _accountHelper = accountHelper;
            _accountService = accountService;
            _accountConnectionService = accountConnectionService;
            _taskService = taskService;
            _externalServiceProvider = externalServiceProvider;
        }

        #endregion

        #region Protected Methods

        /// <summary>
        /// 初始化Account，该方法必须在调用SyncTasks或SyncContacts之前调用
        /// </summary>
        protected void InitializeAccount(AccountConnection connection)
        {
            if (connection == null)
            {
                throw new ArgumentNullException("connection");
            }
            var account = _accountService.GetAccount(connection.AccountId);
            if (account == null)
            {
                throw new Exception(string.Format("账号连接【ID：{0}，AccountId：{1}】对应的账号不存在", connection.ID, connection.AccountId));
            }
            _account = account;
        }
        /// <summary>
        /// 实现同步任务的完整过程
        /// </summary>
        /// <param name="taskSyncServices">参与同步任务的具体服务</param>
        protected virtual void SyncTasks(IEnumerable<ISyncService<TaskSyncData, ISyncData, TaskSyncResult>> taskSyncServices)
        {
            _logger.Info("--------开始进行任务同步--------");

            if (taskSyncServices == null || taskSyncServices.Count() == 0)
            {
                _logger.Info("同步数据的服务个数为零，故同步操作未进行.");
                return;
            }

            //register all the sync services.
            var businessSyncService = DependencyResolver.Resolve<IBusinessSyncService>();
            foreach (var taskSyncService in taskSyncServices)
            {
                businessSyncService.RegisterTaskSyncService(taskSyncService);
            }

            var taskSyncDataList = GetTaskSyncDatas(_account);
            _logger.InfoFormat("同步前的本地任务数据, 总记录数:{0}，明细如下：", taskSyncDataList.Count());
            foreach (var taskSyncData in taskSyncDataList)
            {
                LogSyncData(taskSyncData);
            }

            var syncInfoList = GetSyncInfos(_account);
            _logger.InfoFormat("同步任务数据前的所有与当前帐号相关的任务同步信息，总数：{0}", syncInfoList.Count());
            foreach (var syncInfo in syncInfoList)
            {
                _logger.InfoFormat("任务同步信息, AccountId:{0}, LocalDataId:{1}, SyncDataId:{2}, SyncDataType:{3}", syncInfo.AccountId, syncInfo.LocalDataId, syncInfo.SyncDataId, syncInfo.SyncDataType);
            }

            _logger.InfoFormat("开始比较任务数据.");
            var result = businessSyncService.SyncTasks(taskSyncDataList, syncInfoList);
            _logger.InfoFormat("比较任务数据结束，比较结果：:");
            LogSyncResult(result);

            _logger.InfoFormat("开始持久化比较结果：");
            //持久化本地的任务数据
            PersistLocalTaskDatas(result, _account);
            //持久化外部同步的任务数据
            PersistSyncTaskDatas(result.SyncDatasToCreate, result.SyncDatasToUpdate, result.SyncDatasToDelete, _account);
            _logger.InfoFormat("持久化比较结果结束");

            _logger.Info("--------结束进行任务同步--------");
        }
        /// <summary>
        /// 实现同步联系人的完整过程
        /// </summary>
        /// <param name="contactSyncServices">参与同步联系人的具体服务</param>
        protected virtual void SyncContacts(IEnumerable<ISyncService<ContactSyncData, ISyncData, ContactSyncResult>> contactSyncServices)
        {
            _logger.InfoFormat("--------开始进行联系人同步--------");

            if (contactSyncServices == null || contactSyncServices.Count() == 0)
            {
                _logger.Info("同步数据的服务个数为零，故同步操作未进行.");
                return;
            }

            //register all the sync services.
            var businessSyncService = DependencyResolver.Resolve<IBusinessSyncService>();
            foreach (var contactSyncService in contactSyncServices)
            {
                businessSyncService.RegisterContactSyncService(contactSyncService);
            }

            var contactSyncDataList = GetContactSyncDatas(_account);
            _logger.InfoFormat("同步前的本地联系人数据, 总记录数:{0}，明细如下：", contactSyncDataList.Count());
            foreach (var contactSyncData in contactSyncDataList)
            {
                LogSyncData(contactSyncData);
            }

            var syncInfoList = GetSyncInfos(_account);
            _logger.InfoFormat("同步联系人数据前的所有与当前帐号相关的联系人同步信息，总数：{0}", syncInfoList.Count());
            foreach (var syncInfo in syncInfoList)
            {
                _logger.InfoFormat("联系人同步信息, AccountId:{0}, LocalDataId:{1}, SyncDataId:{2}, SyncDataType:{3}", syncInfo.AccountId, syncInfo.LocalDataId, syncInfo.SyncDataId, syncInfo.SyncDataType);
            }

            _logger.InfoFormat("开始比较联系人数据.");
            var result = businessSyncService.SyncContacts(contactSyncDataList, syncInfoList);
            _logger.InfoFormat("结束比较联系人数据，比较结果:");
            LogSyncResult(result);

            _logger.InfoFormat("开始处理比较结果：");
            //持久化本地的l联系人数据
            ProcessLocalContactDatas(result, _account);
            //持久化外部同步的联系人数据
            PersistSyncContactDatas(result.SyncDatasToCreate, result.SyncDatasToUpdate, result.SyncDatasToDelete, _account);
            _logger.InfoFormat("处理比较结果结束");

            _logger.InfoFormat("--------结束进行联系人同步--------");
        }
        /// <summary>
        /// 持久化外部同步的任务数据
        /// </summary>
        /// <param name="syncDatasToCreate">需要新增的数据</param>
        /// <param name="syncDatasToUpdate">需要更新的数据</param>
        /// <param name="syncDatasToDelete">需要删除的数据</param>
        /// <param name="account">当前同步账号</param>
        protected abstract void PersistSyncTaskDatas(IEnumerable<ISyncData> syncDatasToCreate, IEnumerable<ISyncData> syncDatasToUpdate, IEnumerable<ISyncData> syncDatasToDelete, Account account);
        /// <summary>
        /// 持久化外部同步的联系人数据
        /// </summary>
        /// <param name="syncDatasToCreate">需要新增的数据</param>
        /// <param name="syncDatasToUpdate">需要更新的数据</param>
        /// <param name="syncDatasToDelete">需要删除的数据</param>
        /// <param name="account">当前同步账号</param>
        protected abstract void PersistSyncContactDatas(IEnumerable<ISyncData> syncDatasToCreate, IEnumerable<ISyncData> syncDatasToUpdate, IEnumerable<ISyncData> syncDatasToDelete, Account account);

        /// <summary>
        /// 插入一条同步信息
        /// </summary>
        protected void InsertSyncInfo(SyncInfo syncInfo)
        {
            var session = _sessionManager.OpenSession();
            var sqlFormat = "insert into Cooper_SyncInfo (AccountId,LocalDataId,SyncDataId,SyncDataType) values ({0},'{1}','{2}',{3})";
            var sql = string.Format(sqlFormat, syncInfo.AccountId, syncInfo.LocalDataId, syncInfo.SyncDataId, syncInfo.SyncDataType);
            var query = session.CreateSQLQuery(sql);
            query.ExecuteUpdate();
        }
        /// <summary>
        /// 删除一条同步信息
        /// </summary>
        protected void DeleteSyncInfo(SyncInfo syncInfo)
        {
            var session = _sessionManager.OpenSession();
            var sqlFormat = "delete from Cooper_SyncInfo where AccountId={0} and LocalDataId='{1}' and SyncDataId='{2}' and SyncDataType={3}";
            var sql = string.Format(sqlFormat, syncInfo.AccountId, syncInfo.LocalDataId, syncInfo.SyncDataId, syncInfo.SyncDataType);
            var query = session.CreateSQLQuery(sql);
            query.ExecuteUpdate();
        }
        /// <summary>
        /// 设置任务的最后更新时间
        /// </summary>
        protected void UpdateTaskLastUpdateTime(long taskId, DateTime lastUpdateTime)
        {
            Cooper.Model.Tasks.Task task = _taskService.GetTask(taskId);
            task.SetLastUpdateTime(lastUpdateTime);
            _taskService.Update(task);
        }
        /// <summary>
        /// 设置联系人的最后更新时间
        /// </summary>
        protected void UpdateContactLastUpdateTime(int contactId, DateTime lastUpdateTime)
        {
            //TODO
            //Cooper.Model.Tasks.Contacter contacter = _contactService.GetTask(contactId);
            //contacter.SetLastUpdateTime(lastUpdateTime);
            //_contactService.Update(contacter);
        }

        #endregion

        #region Helper Methods

        /// <summary>
        /// 持久化本地需要增删改的任务
        /// </summary>
        private void PersistLocalTaskDatas(TaskSyncResult taskSyncResult, Account account)
        {
            if (taskSyncResult.LocalDatasToCreate.Count() == 0 && taskSyncResult.LocalDatasToUpdate.Count() == 0 && taskSyncResult.LocalDatasToDelete.Count() == 0)
            {
                return;
            }
            //处理在本地需要新增的任务
            foreach (var taskData in taskSyncResult.LocalDatasToCreate)
            {
                //TODO， 以后下面这三步需要放在一个Transaction中实现

                //创建任务
                CooperTask task = new CooperTask(account);

                task.SetSubject(taskData.Subject ?? string.Empty);
                task.SetBody(FormatTaskBody(taskData.Body));
                task.SetDueTime(FormatTaskDueTime(taskData.DueTime));
                if (taskData.IsCompleted)
                {
                    task.MarkAsCompleted();
                }
                else
                {
                    task.MarkAsInCompleted();
                }
                task.SetPriority(ConvertToPriority(taskData.Priority));

                _taskService.Create(task);

                //任务创建后更新最后更新时间，更新为和这条任务关联的外部系统任务的最后更新时间
                task.SetLastUpdateTime(taskData.LastUpdateLocalTime);
                _taskService.Update(task);

                //创建同步信息
                if (taskData.IsFromDefault)
                {
                    SyncInfo syncInfo = new SyncInfo();
                    syncInfo.AccountId = account.ID;
                    syncInfo.LocalDataId = task.ID.ToString();
                    syncInfo.SyncDataId = taskData.SyncId;
                    syncInfo.SyncDataType = taskData.SyncType;
                    InsertSyncInfo(syncInfo);
                }
            }

            //处理在本地需要更新的任务
            foreach (var taskData in taskSyncResult.LocalDatasToUpdate)
            {
                //更新任务
                CooperTask task = _taskService.GetTask(long.Parse(taskData.Id));

                task.SetSubject(taskData.Subject ?? string.Empty);
                task.SetBody(FormatTaskBody(taskData.Body));
                task.SetDueTime(FormatTaskDueTime(taskData.DueTime));
                if (taskData.IsCompleted)
                {
                    task.MarkAsCompleted();
                }
                else
                {
                    task.MarkAsInCompleted();
                }
                task.SetPriority(ConvertToPriority(taskData.Priority));

                _taskService.Update(task);

                //任务更新后更新最后更新时间，更新为和这条任务关联的外部系统任务的最后更新时间
                task.SetLastUpdateTime(taskData.LastUpdateLocalTime);
                _taskService.Update(task);
            }

            //暂时去掉删除本地数据的功能
            ////处理在本地需要删除的任务
            //foreach (var taskData in taskSyncResult.LocalDatasToDelete)
            //{
            //    Cooper.Model.Tasks.Task task = _taskService.GetTask(long.Parse(taskData.Id));
            //    _taskService.Delete(task);

            //    //删除同步信息
            //    SyncInfo syncInfo = new SyncInfo();
            //    syncInfo.AccountId = account.ID;
            //    syncInfo.LocalDataId = task.ID.ToString();
            //    syncInfo.SyncDataId = taskData.SyncId;
            //    syncInfo.SyncDataType = taskData.SyncType;
            //    DeleteSyncInfo(syncInfo);
            //}
        }
        /// <summary>
        /// 持久化本地需要增删改的联系人
        /// </summary>
        private void ProcessLocalContactDatas(ContactSyncResult contactSyncResult, Account account)
        {
            //TODO，由于联系人功能暂时还未做好，所以这里先不实现

            if (contactSyncResult.LocalDatasToCreate.Count() == 0 && contactSyncResult.LocalDatasToUpdate.Count() == 0 && contactSyncResult.LocalDatasToDelete.Count() == 0)
            {
                return;
            }
            //处理在本地需要新增的联系人
            foreach (var contactData in contactSyncResult.LocalDatasToCreate)
            {

            }

            //处理在本地需要更新的联系人
            foreach (var contactData in contactSyncResult.LocalDatasToUpdate)
            {

            }

            //处理在本地需要删除的联系人
            foreach (var contactData in contactSyncResult.LocalDatasToDelete)
            {

            }
        }

        private string FormatTaskBody(string bodyToFormat)
        {
            string formattedBody = bodyToFormat;

            if (formattedBody == null)
            {
                formattedBody = string.Empty;
            }
            else if (formattedBody.Length > 5000)
            {
                formattedBody = formattedBody.Substring(5000);
            }

            return formattedBody;
        }
        private DateTime? FormatTaskDueTime(DateTime? dueTimeToFormat)
        {
            DateTime? formattedDueTime = dueTimeToFormat;

            if (formattedDueTime != null && formattedDueTime.Value < new DateTime(1753, 1, 1))
            {
                formattedDueTime = null;
            }

            return formattedDueTime;
        }

        /// <summary>
        /// 将任务数据的同步结果进行日志记录
        /// </summary>
        private void LogSyncResult<T>(ISyncResult<T> syncResult) where T : class, ISyncData
        {
            _logger.InfoFormat("----LocalDatasToCreate, Count:{0}, Details:", syncResult.LocalDatasToCreate.Count());
            foreach (var taskData in syncResult.LocalDatasToCreate)
            {
                LogToCreatedSyncData(taskData);
            }

            _logger.InfoFormat("----LocalDatasToUpdate, Count:{0}, Details:", syncResult.LocalDatasToUpdate.Count());
            foreach (var taskData in syncResult.LocalDatasToUpdate)
            {
                LogSyncData(taskData);
            }

            _logger.InfoFormat("----LocalDatasToDelete, Count:{0}, Details:", syncResult.LocalDatasToDelete.Count());
            foreach (var taskData in syncResult.LocalDatasToDelete)
            {
                LogSyncData(taskData);
            }

            _logger.InfoFormat("----SyncDatasToCreate, Count:{0}, Details:", syncResult.SyncDatasToCreate.Count());
            foreach (var syncData in syncResult.SyncDatasToCreate)
            {
                LogToCreatedSyncData(syncData);
            }

            _logger.InfoFormat("----SyncDatasToUpdate, Count:{0}, Details:", syncResult.SyncDatasToUpdate.Count());
            foreach (var syncData in syncResult.SyncDatasToUpdate)
            {
                LogSyncData(syncData);
            }

            _logger.InfoFormat("----SyncDatasToDelete, Count:{0}, Details:", syncResult.SyncDatasToDelete.Count());
            foreach (var syncData in syncResult.SyncDatasToDelete)
            {
                LogSyncData(syncData);
            }
        }
        private void LogToCreatedSyncData(ISyncData syncData)
        {
            if (syncData != null)
            {
                _logger.InfoFormat("--------Data Type:{0}, Subject:{1}, SyncId:{2}, SyncType:{3}, IsFromDefault:{4}",
                    syncData.GetType().Name,
                    syncData.Subject,
                    syncData.SyncId,
                    syncData.SyncType,
                    syncData.IsFromDefault);
            }
        }
        private void LogSyncData(ISyncData syncData)
        {
            if (syncData != null)
            {
                _logger.InfoFormat("--------Data Type:{0}, Id:{1}, Subject:{2}, SyncId:{3}, SyncType:{4}, IsFromDefault:{5}, LastUpdateLocalTime:{6}",
                    syncData.GetType().Name,
                    syncData.Id,
                    syncData.Subject,
                    syncData.SyncId,
                    syncData.SyncType,
                    syncData.IsFromDefault,
                    syncData.LastUpdateLocalTime);
            }
        }
        private bool SafeAction(string actionName, Action action)
        {
            bool success = false;

            try
            {
                action();
                success = true;
            }
            catch (Exception ex)
            {
                success = false;
                _logger.Error(string.Format("{0} has exception.", actionName), ex);
            }

            return success;
        }

        /// <summary>
        /// 获取帐号的所有任务
        /// </summary>
        private IEnumerable<TaskSyncData> GetTaskSyncDatas(Account account)
        {
            List<TaskSyncData> dataList = new List<TaskSyncData>();

            var tasks = _taskService.GetTasks(account);
            foreach (var task in tasks)
            {
                dataList.Add(CreateFromTask(task));
            }

            return dataList;
        }
        /// <summary>
        /// 获取账号的所有联系人信息，目前还未实现
        /// </summary>
        private IEnumerable<ContactSyncData> GetContactSyncDatas(Account account)
        {
            List<ContactSyncData> dataList = new List<ContactSyncData>();

            //TODO
            //var tasks = _taskService.GetTasks(account);
            //foreach (var task in tasks)
            //{
            //    dataList.Add(CreateFromTask(task));
            //}

            return dataList;
        }
        /// <summary>
        /// 获取账号的所有同步信息
        /// </summary>
        private IEnumerable<SyncInfo> GetSyncInfos(Account account)
        {
            var session = _sessionManager.OpenSession();
            var sql = "select AccountId,LocalDataId,SyncDataId,SyncDataType from Cooper_SyncInfo where AccountId=" + account.ID;
            var query = session.CreateSQLQuery(sql);
            var objectArrayList = query.List();
            List<SyncInfo> syncInfoList = new List<SyncInfo>();

            foreach (object[] objectArray in objectArrayList)
            {
                SyncInfo syncInfo = new SyncInfo();
                syncInfo.AccountId = int.Parse(objectArray[0].ToString());
                syncInfo.LocalDataId = objectArray[1].ToString();
                syncInfo.SyncDataId = objectArray[2].ToString();
                syncInfo.SyncDataType = int.Parse(objectArray[3].ToString());
                syncInfoList.Add(syncInfo);
            }

            return syncInfoList;
        }
        /// <summary>
        /// 将Task转换为TaskSyncData
        /// </summary>
        private TaskSyncData CreateFromTask(Cooper.Model.Tasks.Task task)
        {
            TaskSyncData data = new TaskSyncData();
            data.Id = task.ID.ToString();
            data.Subject = task.Subject;
            data.Body = task.Body;
            data.DueTime = task.DueTime;
            data.IsCompleted = task.IsCompleted;
            data.Priority = ConvertToSyncDataPriority(task.Priority);
            data.CreateTime = task.CreateTime;
            data.LastUpdateLocalTime = task.LastUpdateTime;
            return data;
        }
        private TaskSyncDataPriority ConvertToSyncDataPriority(Cooper.Model.Tasks.Priority priority)
        {
            if (priority == Cooper.Model.Tasks.Priority.Today)
            {
                return TaskSyncDataPriority.Today;
            }
            else if (priority == Cooper.Model.Tasks.Priority.Upcoming)
            {
                return TaskSyncDataPriority.Upcoming;
            }
            else if (priority == Cooper.Model.Tasks.Priority.Later)
            {
                return TaskSyncDataPriority.Later;
            }
            return TaskSyncDataPriority.Today;
        }
        private Cooper.Model.Tasks.Priority ConvertToPriority(TaskSyncDataPriority priority)
        {
            if (priority == TaskSyncDataPriority.Today)
            {
                return Cooper.Model.Tasks.Priority.Today;
            }
            else if (priority == TaskSyncDataPriority.Upcoming)
            {
                return Cooper.Model.Tasks.Priority.Upcoming;
            }
            else if (priority == TaskSyncDataPriority.Later)
            {
                return Cooper.Model.Tasks.Priority.Later;
            }
            return Cooper.Model.Tasks.Priority.Today;
        }

        #endregion
    }
}
