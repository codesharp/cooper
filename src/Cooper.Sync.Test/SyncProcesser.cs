using System;
using System.Collections.Generic;
using System.Linq;
using Castle.Facilities.NHibernateIntegration;
using CodeSharp.Core;
using CodeSharp.Core.Services;
using Cooper.Model;
using Cooper.Model.Accounts;
using Cooper.Model.Tasks;
using DotNetOpenAuth.OAuth2;
using Google.Apis.Calendar.v3;
using Google.Apis.Calendar.v3.Data;
using Google.Apis.Tasks.v1;
using Google.Apis.Tasks.v1.Data;
using Google.Contacts;
using Google.GData.Client;
using Google.GData.Contacts;
using CooperTask = Cooper.Model.Tasks.Task;

namespace Cooper.Sync.Test
{
    /// <summary>
    /// 数据同步处理器接口定义，该处理器负责实现整个同步过程
    /// </summary>
    public interface ISyncProcesser
    {
        /// <summary>
        /// 根据指定的同步服务同步数据
        /// </summary>
        void SyncTasksAndContacts(int connectionId, IList<ISyncService<TaskSyncData, ISyncData, TaskSyncResult>> taskSyncServices, IList<ISyncService<ContactSyncData, ISyncData, ContactSyncResult>> contactSyncServices);
        /// <summary>
        /// 根据默认支持的同步服务来同步任务与联系人
        /// </summary>
        void SyncTasksAndContacts(int connectionId);
    }

    /// <summary>
    /// 数据同步处理器实现类
    /// </summary>
    [Component]
    public class SyncProcesser : ISyncProcesser
    {
        #region Private Members

        private ILog _logger;
        private ISessionManager _sessionManager;
        private Account _account;
        private GoogleConnection _googleConnection;

        private ITaskService _taskService;
        private IAccountService _accountService;
        private IAccountConnectionService _accountConnectionService;
        private IAccountHelper _accountHelper;
        private IExternalServiceProvider _externalServiceProvider;
        private IGoogleTokenService _googleTokenService;
        private IList<ISyncService<TaskSyncData, ISyncData, TaskSyncResult>> _defaultTaskSyncServices;
        private IList<ISyncService<ContactSyncData, ISyncData, ContactSyncResult>> _defaultContactSyncServices;

        #endregion

        #region Constructors

        public SyncProcesser()
        {
            //初始化同步锁
            DependencyResolver.Resolve<ILockHelper>().Init<Account>();
            DependencyResolver.Resolve<ILockHelper>().Init<GoogleConnection>();

            _logger = DependencyResolver.Resolve<ILoggerFactory>().Create(GetType());
            _sessionManager = DependencyResolver.Resolve<ISessionManager>();

            _accountHelper = DependencyResolver.Resolve<IAccountHelper>();
            _accountService = DependencyResolver.Resolve<IAccountService>();
            _accountConnectionService = DependencyResolver.Resolve<IAccountConnectionService>();
            _taskService = DependencyResolver.Resolve<ITaskService>();

            _externalServiceProvider = DependencyResolver.Resolve<IExternalServiceProvider>();
            _googleTokenService = DependencyResolver.Resolve<IGoogleTokenService>();

            _defaultTaskSyncServices = new List<ISyncService<TaskSyncData, ISyncData, TaskSyncResult>>();

            _defaultTaskSyncServices.Add(DependencyResolver.Resolve<IGoogleTaskSyncService>());
            _defaultTaskSyncServices.Add(DependencyResolver.Resolve<IGoogleCalendarEventSyncService>());

            _defaultContactSyncServices = new List<ISyncService<ContactSyncData, ISyncData, ContactSyncResult>>();
            //暂时不对联系人进行同步
            //_defaultContactSyncServices.Add(DependencyResolver.Resolve<IGoogleContactSyncService>());
        }

        #endregion

        #region Public Methods

        /// <summary>
        /// 根据默认支持的同步服务来同步任务与联系人
        /// </summary>
        public void SyncTasksAndContacts(int connectionId)
        {
            SyncTasksAndContacts(connectionId, _defaultTaskSyncServices, _defaultContactSyncServices);
        }
        /// <summary>
        /// 根据指定的同步服务同步数据
        /// </summary>
        public void SyncTasksAndContacts(int connectionId, IList<ISyncService<TaskSyncData, ISyncData, TaskSyncResult>> taskSyncServices, IList<ISyncService<ContactSyncData, ISyncData, ContactSyncResult>> contactSyncServices)
        {
            _logger.InfoFormat("开始同步任务及联系人, connectionId:{0}", connectionId);

            try
            {
                InitializeAccountAndConnection(connectionId);
                SyncTasks(taskSyncServices);
                SyncContacts(contactSyncServices);
            }
            catch (Exception ex)
            {
                _logger.Error(ex);
            }
            finally
            {
                _logger.InfoFormat("结束同步任务及联系人, connectionId:{0}", connectionId);
            }
        }

        #endregion

        #region Helper Methods

        #region 处理同步结果相关函数

        /// <summary>
        /// 处理任务同步结果
        /// </summary>
        /// <param name="taskSyncResult">任务同步结果</param>
        /// <param name="account">任务所属账号</param>
        private void ProcessTaskSyncResult(TaskSyncResult taskSyncResult, Account account)
        {
            //处理本地任务
            ProcessLocalTaskDatas(taskSyncResult, account);

            var tasksToCreate = taskSyncResult.SyncDatasToCreate.Where(x => x.GetType() == typeof(GoogleTaskSyncData)).Cast<GoogleTaskSyncData>();
            var tasksToUpdate = taskSyncResult.SyncDatasToUpdate.Where(x => x.GetType() == typeof(GoogleTaskSyncData)).Cast<GoogleTaskSyncData>();
            var tasksToDelete = taskSyncResult.SyncDatasToDelete.Where(x => x.GetType() == typeof(GoogleTaskSyncData)).Cast<GoogleTaskSyncData>();
            var calendarEventsToCreate = taskSyncResult.SyncDatasToCreate.Where(x => x.GetType() == typeof(GoogleCalendarEventSyncData)).Cast<GoogleCalendarEventSyncData>();
            var calendarEventsToUpdate = taskSyncResult.SyncDatasToUpdate.Where(x => x.GetType() == typeof(GoogleCalendarEventSyncData)).Cast<GoogleCalendarEventSyncData>();
            var calendarEventsToDelete = taskSyncResult.SyncDatasToDelete.Where(x => x.GetType() == typeof(GoogleCalendarEventSyncData)).Cast<GoogleCalendarEventSyncData>();

            if (tasksToCreate.Count() > 0 || tasksToUpdate.Count() > 0 || tasksToDelete.Count() > 0 || calendarEventsToCreate.Count() > 0 || calendarEventsToUpdate.Count() > 0 || calendarEventsToDelete.Count() > 0)
            {
                var googleToken = GetGoogleUserToken();
                //处理Google Task
                ProcessGoogleTaskDatas(account, googleToken, tasksToCreate, tasksToUpdate, tasksToDelete);
                //处理Google Calender Event
                ProcessGoogleCalendarEventDatas(account, googleToken, calendarEventsToCreate, calendarEventsToUpdate, calendarEventsToDelete);
            }
        }
        /// <summary>
        /// 处理联系人同步结果
        /// </summary>
        /// <param name="contactSyncResult">联系人同步结果</param>
        /// <param name="account">联系人所属账号</param>
        private void ProcessContactSyncResult(ContactSyncResult contactSyncResult, Account account)
        {
            //处理本地联系人
            ProcessLocalContactDatas(contactSyncResult, account);

            var googleContactsToCreate = contactSyncResult.SyncDatasToCreate.Where(x => x.GetType() == typeof(GoogleContactSyncData)).Cast<GoogleContactSyncData>();
            var googleContactsToUpdate = contactSyncResult.SyncDatasToUpdate.Where(x => x.GetType() == typeof(GoogleContactSyncData)).Cast<GoogleContactSyncData>();
            var googleContactsToDelete = contactSyncResult.SyncDatasToDelete.Where(x => x.GetType() == typeof(GoogleContactSyncData)).Cast<GoogleContactSyncData>();

            //处理Google联系人
            if (googleContactsToCreate.Count() > 0 || googleContactsToUpdate.Count() > 0 || googleContactsToDelete.Count() > 0)
            {
                var googleToken = GetGoogleUserToken();
                ProcessGoogleContactDatas(account, googleToken, googleContactsToCreate, googleContactsToUpdate, googleContactsToDelete);
            }
        }

        #region 处理本地任务数据的同步结果

        /// <summary>
        /// 持久化本地需要增删改的任务
        /// </summary>
        private void ProcessLocalTaskDatas(TaskSyncResult taskSyncResult, Account account)
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

        #endregion

        #region 处理与Google相关数据的同步结果

        /// <summary>
        /// 持久化Google Task
        /// </summary>
        private void ProcessGoogleTaskDatas(Account account, IAuthorizationState token, IEnumerable<GoogleTaskSyncData> datasToCreate, IEnumerable<GoogleTaskSyncData> datasToUpdate, IEnumerable<GoogleTaskSyncData> datasToDelete)
        {
            if (datasToCreate.Count() == 0 && datasToUpdate.Count() == 0 && datasToDelete.Count() == 0)
            {
                return;
            }
            var googleTaskService = _externalServiceProvider.GetGoogleTaskService(token);
            bool isDefaultTaskListExist = false;
            var taskList = DependencyResolver.Resolve<IGoogleTaskSyncDataService>().GetDefaultTaskList(token, out isDefaultTaskListExist);
            foreach (var dataToCreate in datasToCreate)
            {
                CreateGoogleTask(account, dataToCreate, googleTaskService, taskList);
            }

            foreach (var dataToUpdate in datasToUpdate)
            {
                UpdateGoogleTask(dataToUpdate, googleTaskService, taskList);
            }

            foreach (var dataToDelete in datasToDelete)
            {
                DeleteGoogleTask(account, dataToDelete, googleTaskService, taskList);
            }
        }
        /// <summary>
        /// 持久化Google Calendar Event
        /// </summary>
        private void ProcessGoogleCalendarEventDatas(Account account, IAuthorizationState token, IEnumerable<GoogleCalendarEventSyncData> datasToCreate, IEnumerable<GoogleCalendarEventSyncData> datasToUpdate, IEnumerable<GoogleCalendarEventSyncData> datasToDelete)
        {
            if (datasToCreate.Count() == 0 && datasToUpdate.Count() == 0 && datasToDelete.Count() == 0)
            {
                return;
            }
            var googleCalendarService = _externalServiceProvider.GetGoogleCalendarService(token);
            bool isDefaultCalendarExist = false;
            var defaultCalendar = DependencyResolver.Resolve<IGoogleCalendarEventSyncDataService>().GetDefaultCalendar(token, out isDefaultCalendarExist);
            foreach (var dataToCreate in datasToCreate)
            {
                CreateGoogleCalendarEvent(account, dataToCreate, googleCalendarService, defaultCalendar);
            }

            foreach (var dataToUpdate in datasToUpdate)
            {
                UpdateGoogleCalendarEvent(dataToUpdate, googleCalendarService, defaultCalendar);
            }

            foreach (var dataToDelete in datasToDelete)
            {
                DeleteGoogleCalendarEvent(account, dataToDelete, googleCalendarService, defaultCalendar);
            }
        }
        /// <summary>
        /// 持久化Google Contact
        /// </summary>
        private void ProcessGoogleContactDatas(Account account, IAuthorizationState token, IEnumerable<GoogleContactSyncData> datasToCreate, IEnumerable<GoogleContactSyncData> datasToUpdate, IEnumerable<GoogleContactSyncData> datasToDelete)
        {
            if (datasToCreate.Count() == 0 && datasToUpdate.Count() == 0 && datasToDelete.Count() == 0)
            {
                return;
            }

            var googleContactRequest = _externalServiceProvider.GetGoogleContactRequest(token);
            bool isDefaultContactGroupExist = false;
            var defaultContactGroup = DependencyResolver.Resolve<IGoogleContactSyncDataService>().GetDefaultContactGroup(token, out isDefaultContactGroupExist);
            foreach (var dataToCreate in datasToCreate)
            {
                CreateGoogleContact(account, dataToCreate, googleContactRequest, defaultContactGroup);
            }

            foreach (var dataToUpdate in datasToUpdate)
            {
                UpdateGoogleContact(dataToUpdate, googleContactRequest);
            }

            foreach (var dataToDelete in datasToDelete)
            {
                DeleteGoogleContact(account, dataToDelete, googleContactRequest);
            }
        }

        #endregion

        #region 持久化Google相关数据的函数

        /// <summary>
        /// 创建Google Task
        /// </summary>
        private void CreateGoogleTask(Account account, GoogleTaskSyncData taskData, TasksService googleTaskService, TaskList defaultTaskList)
        {
            global::Google.Apis.Tasks.v1.Data.Task googleTask = null;
            bool success = false;

            try
            {
                //创建Google Task
                googleTask = googleTaskService.Tasks.Insert(taskData.GoogleTask, defaultTaskList.Id).Fetch();
                _logger.InfoFormat("新增Google任务#{0}|{1}|{2}", googleTask.Id, googleTask.Title, account.ID);
                success = true;
            }
            catch (Exception ex)
            {
                _logger.Error("CreateGoogleTask has exception.", ex);
            }

            if (success)
            {
                //更新任务最后更新时间，确保与Google Task的最后更新时间一致
                UpdateTaskLastUpdateTime(long.Parse(taskData.SyncId), Rfc3339DateTime.Parse(googleTask.Updated).ToLocalTime());

                //创建同步信息
                if (defaultTaskList.Title == GoogleSyncSettings.DefaultTaskListName)
                {
                    SyncInfo syncInfo = new SyncInfo();
                    syncInfo.AccountId = account.ID;
                    syncInfo.LocalDataId = taskData.SyncId;
                    syncInfo.SyncDataId = googleTask.Id;
                    syncInfo.SyncDataType = taskData.SyncType;
                    InsertSyncInfo(syncInfo);
                }
            }
        }
        /// <summary>
        /// 创建Google Calendar Event
        /// </summary>
        private void CreateGoogleCalendarEvent(Account account, GoogleCalendarEventSyncData calendarEventData, CalendarService calendarService, Calendar defaultCalendar)
        {
            global::Google.Apis.Calendar.v3.Data.Event calendarEvent = null;
            bool success = false;

            try
            {
                //创建Google Calendar Event
                calendarEvent = calendarService.Events.Insert(calendarEventData.GoogleCalendarEvent, defaultCalendar.Id).Fetch();
                _logger.InfoFormat("新增Google日历事件#{0}|{1}|{2}", calendarEvent.Id, calendarEvent.Summary, account.ID);
                success = true;
            }
            catch (Exception ex)
            {
                _logger.Error("CreateGoogleCalendarEvent has exception.", ex);
            }

            if (success)
            {
                //更新任务最后更新时间，确保与Google Calendar Event的最后更新时间一致
                UpdateTaskLastUpdateTime(long.Parse(calendarEventData.SyncId), Rfc3339DateTime.Parse(calendarEvent.Updated).ToLocalTime());

                //创建同步信息
                if (defaultCalendar.Summary == GoogleSyncSettings.DefaultCalendarName)
                {
                    SyncInfo syncInfo = new SyncInfo();
                    syncInfo.AccountId = account.ID;
                    syncInfo.LocalDataId = calendarEventData.SyncId;
                    syncInfo.SyncDataId = calendarEvent.Id;
                    syncInfo.SyncDataType = calendarEventData.SyncType;
                    InsertSyncInfo(syncInfo);
                }
            }
        }
        /// <summary>
        /// 创建Google Contact
        /// </summary>
        private void CreateGoogleContact(Account account, GoogleContactSyncData contactData, ContactsRequest contactRequest, Group defaultContactGroup)
        {
            global::Google.Contacts.Contact contact = null;
            bool success = false;

            try
            {
                //设置联系人默认分组
                contactData.Contact.GroupMembership.Add(new GroupMembership() { HRef = defaultContactGroup.Id });
                //调用API新增联系人
                contact = contactRequest.Insert(new Uri(GoogleSyncSettings.ContactScope), contactData.Contact);
                _logger.InfoFormat("新增Google联系人#{0}|{1}|{2}", contact.Id, contactData.Subject, account.ID);
                success = true;
            }
            catch (Exception ex)
            {
                _logger.Error("CreateGoogleContact has exception.", ex);
            }

            if (success)
            {
                //更新联系人最后更新时间，确保与Google Contact的最后更新时间一致
                UpdateContactLastUpdateTime(int.Parse(contactData.SyncId), contact.Updated.ToLocalTime());

                //创建同步信息
                SyncInfo syncInfo = new SyncInfo();
                syncInfo.AccountId = account.ID;
                syncInfo.LocalDataId = contactData.SyncId;
                syncInfo.SyncDataId = contact.Id;
                syncInfo.SyncDataType = contactData.SyncType;
                InsertSyncInfo(syncInfo);
            }
        }

        /// <summary>
        /// 更新Google Task
        /// </summary>
        private void UpdateGoogleTask(GoogleTaskSyncData taskData, TasksService googleTaskService, TaskList defaultTaskList)
        {
            try
            {
                googleTaskService.Tasks.Update(taskData.GoogleTask, defaultTaskList.Id, taskData.Id).Fetch();
                _logger.InfoFormat("更新Google任务#{0}|{1}|{2}", taskData.Id, taskData.Subject, _account.ID);
                var updatedGoogleTask = googleTaskService.Tasks.Get(defaultTaskList.Id, taskData.Id).Fetch();
                UpdateTaskLastUpdateTime(long.Parse(taskData.SyncId), Rfc3339DateTime.Parse(updatedGoogleTask.Updated).ToLocalTime());
            }
            catch (Exception ex)
            {
                _logger.Error("UpdateGoogleTask has exception.", ex);
            }
        }
        /// <summary>
        /// 更新Google Calendar Event
        /// </summary>
        private void UpdateGoogleCalendarEvent(GoogleCalendarEventSyncData calendarEventData, CalendarService googleCalendarService, Calendar defaultCalendar)
        {
            try
            {
                googleCalendarService.Events.Update(calendarEventData.GoogleCalendarEvent, defaultCalendar.Id, calendarEventData.Id).Fetch();
                _logger.InfoFormat("更新Google日历事件#{0}|{1}|{2}", calendarEventData.Id, calendarEventData.Subject, _account.ID);
                var updatedGoogleCalendarEvent = googleCalendarService.Events.Get(defaultCalendar.Id, calendarEventData.Id).Fetch();
                UpdateTaskLastUpdateTime(long.Parse(calendarEventData.SyncId), Rfc3339DateTime.Parse(updatedGoogleCalendarEvent.Updated).ToLocalTime());
            }
            catch (Exception ex)
            {
                _logger.Error("UpdateGoogleCalendarEvent has exception.", ex);
            }
        }
        /// <summary>
        /// 更新Google Contact
        /// </summary>
        private void UpdateGoogleContact(GoogleContactSyncData contactData, ContactsRequest contactRequest)
        {
            try
            {
                ReplaceContactEditUrl(contactData.Contact);
                var updatedContact = contactRequest.Update(contactData.Contact);
                _logger.InfoFormat("更新Google联系人#{0}|{1}|{2}", contactData.Id, contactData.Subject, _account.ID);
                UpdateContactLastUpdateTime(int.Parse(contactData.SyncId), updatedContact.Updated.ToLocalTime());
            }
            catch (Exception ex)
            {
                _logger.Error("UpdateGoogleContact has exception.", ex);
            }
        }

        /// <summary>
        /// 删除Google Task
        /// </summary>
        private void DeleteGoogleTask(Account account, GoogleTaskSyncData taskData, TasksService googleTaskService, TaskList defaultTaskList)
        {
            bool success = false;

            try
            {
                googleTaskService.Tasks.Delete(defaultTaskList.Id, taskData.Id).Fetch();
                _logger.InfoFormat("删除Google任务#{0}|{1}|{2}", taskData.Id, taskData.Subject, account.ID);
                success = true;
            }
            catch (Exception ex)
            {
                _logger.Error("DeleteGoogleTask has exception.", ex);
            }

            if (success)
            {
                //删除同步信息
                SyncInfo syncInfo = new SyncInfo();
                syncInfo.AccountId = account.ID;
                syncInfo.LocalDataId = taskData.SyncId;
                syncInfo.SyncDataId = taskData.Id;
                syncInfo.SyncDataType = taskData.SyncType;
                DeleteSyncInfo(syncInfo);
            }
        }
        /// <summary>
        /// 删除Google Calendar Event
        /// </summary>
        private void DeleteGoogleCalendarEvent(Account account, GoogleCalendarEventSyncData calendarEventData, CalendarService googleCalendarService, Calendar defaultCalendar)
        {
            bool success = false;

            try
            {
                googleCalendarService.Events.Delete(defaultCalendar.Id, calendarEventData.Id).Fetch();
                _logger.InfoFormat("删除Google日历事件#{0}|{1}|{2}", calendarEventData.Id, calendarEventData.Subject, account.ID);
                success = true;
            }
            catch (Exception ex)
            {
                _logger.Error("DeleteGoogleCalendarEvent has exception.", ex);
            }

            if (success)
            {
                //删除同步信息
                SyncInfo syncInfo = new SyncInfo();
                syncInfo.AccountId = account.ID;
                syncInfo.LocalDataId = calendarEventData.SyncId;
                syncInfo.SyncDataId = calendarEventData.Id;
                syncInfo.SyncDataType = calendarEventData.SyncType;
                DeleteSyncInfo(syncInfo);
            }
        }
        /// <summary>
        /// 删除Google Contact
        /// </summary>
        private void DeleteGoogleContact(Account account, GoogleContactSyncData contactData, ContactsRequest contactRequest)
        {
            bool success = false;

            try
            {
                ReplaceContactEditUrl(contactData.Contact);
                contactRequest.Delete(contactData.Contact);
                _logger.InfoFormat("删除Google联系人#{0}|{1}|{2}", contactData.Id, contactData.Subject, account.ID);
                success = true;
            }
            catch (Exception ex)
            {
                _logger.Error("DeleteGoogleContact has exception.", ex);
            }

            if (success)
            {
                //删除同步信息
                SyncInfo syncInfo = new SyncInfo();
                syncInfo.AccountId = account.ID;
                syncInfo.LocalDataId = contactData.SyncId;
                syncInfo.SyncDataId = contactData.Id;
                syncInfo.SyncDataType = contactData.SyncType;
                DeleteSyncInfo(syncInfo);
            }
        }

        /// <summary>
        /// 这里要替换Contact的Edit的Uri的原因是：
        /// EditUri的默认值为：           https://www.google.com/m8/feeds/contacts/{userEmail}/full
        /// 而我们的Token针对的授权Uri是：https://www.google.com/m8/feeds/contacts/default/full
        /// 对于读取某个谷歌用户的联系人的Url，正常应该要用邮箱指定当前要获取谁的联系人，但是也可以指定为default，表示默认为当前Token对应的谷歌用户。
        /// 所以，对于读取联系人时，上面这两个地址都可以，我们选择了第二种方式，因为这样做可以不需要知道用户的邮箱，代码可以通用，不用针对每个谷歌用户的邮箱生成
        /// 一个专用于操作联系人的Token；
        /// 但是当要更新时，EditUri的默认值里包含的不是default,而是一个具体的Email，这导致我们之前用defaut的方式获取的Token无法做更新或删除谷歌联系人的操作
        /// Googel也搞了，只有在读取数据时具体邮箱才可以和default值等价，而在更新数时则不能。幸亏你提供了源代码，费了好大功夫调试你的代码才知道这个原因。
        /// </summary>
        private void ReplaceContactEditUrl(global::Google.Contacts.Contact contact)
        {
            var contactId = contact.Id;
            var updateContactUrl = GoogleSyncSettings.ContactScope + "/" + contactId.Substring(contactId.LastIndexOf('/') + 1);
            contact.ContactEntry.EditUri = new AtomUri(updateContactUrl);
        }

        #endregion

        /// <summary>
        /// 插入一条同步信息
        /// </summary>
        private void InsertSyncInfo(SyncInfo syncInfo)
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
        private void DeleteSyncInfo(SyncInfo syncInfo)
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
        private void UpdateTaskLastUpdateTime(long taskId, DateTime lastUpdateTime)
        {
            Cooper.Model.Tasks.Task task = _taskService.GetTask(taskId);
            task.SetLastUpdateTime(lastUpdateTime);
            _taskService.Update(task);
        }
        /// <summary>
        /// 设置联系人的最后更新时间
        /// </summary>
        private void UpdateContactLastUpdateTime(int contactId, DateTime lastUpdateTime)
        {
            //TODO
            //Cooper.Model.Tasks.Contacter contacter = _contactService.GetTask(contactId);
            //contacter.SetLastUpdateTime(lastUpdateTime);
            //_contactService.Update(contacter);
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

        #endregion

        /// <summary>
        /// 同步任务
        /// </summary>
        private void SyncTasks(IEnumerable<ISyncService<TaskSyncData, ISyncData, TaskSyncResult>> taskSyncServices)
        {
            if (taskSyncServices != null && taskSyncServices.Count() > 0)
            {
                if (_googleConnection != null)
                {
                    SyncTaskWithGoogle(taskSyncServices.Where(x => typeof(IGoogleSyncService).IsAssignableFrom(x.GetType())));
                }
            }
        }
        /// <summary>
        /// 同步联系人
        /// </summary>
        private void SyncContacts(IEnumerable<ISyncService<ContactSyncData, ISyncData, ContactSyncResult>> contactSyncServices)
        {
            if (contactSyncServices != null && contactSyncServices.Count() > 0)
            {
                if (_googleConnection != null)
                {
                    SyncContactWithGoogle(contactSyncServices.Where(x => typeof(IGoogleSyncService).IsAssignableFrom(x.GetType())));
                }
            }
        }

        private void SyncTaskWithGoogle(IEnumerable<ISyncService<TaskSyncData, ISyncData, TaskSyncResult>> googleTaskSyncServices)
        {
            _logger.Info("--------开始与Google进行任务同步--------");

            if (googleTaskSyncServices == null || googleTaskSyncServices.Count() == 0)
            {
                _logger.Info("因为没有注册的同步服务，故同步操作未进行.");
                return;
            }

            var googleToken = GetGoogleUserToken();

            if (googleToken == null)
            {
                _logger.ErrorFormat("Google Token无效，无法进行同步, google connectionId:{0}", _googleConnection.ID);
                return;
            }

            //set token for each google sync service.
            foreach (IGoogleSyncService taskSyncService in googleTaskSyncServices)
            {
                taskSyncService.SetGoogleToken(googleToken);
            }

            //register all the google sync services.
            var businessSyncService = DependencyResolver.Resolve<IBusinessSyncService>();
            foreach (var taskSyncService in googleTaskSyncServices)
            {
                businessSyncService.RegisterTaskSyncService(taskSyncService);
            }

            DoSyncTasks(businessSyncService);

            _logger.Info("--------结束与Google进行任务同步--------");
        }
        private void SyncContactWithGoogle(IEnumerable<ISyncService<ContactSyncData, ISyncData, ContactSyncResult>> googleContactSyncServices)
        {
            _logger.InfoFormat("--------开始与Google进行联系人同步--------");

            if (googleContactSyncServices == null || googleContactSyncServices.Count() == 0)
            {
                _logger.InfoFormat("因为没有注册的同步服务，故同步操作未进行.");
                return;
            }

            var googleToken = GetGoogleUserToken();

            if (googleToken == null)
            {
                _logger.ErrorFormat("Google Token无效，无法进行同步, google connectionId:{0}", _googleConnection.ID);
                return;
            }

            //set token for each google sync service.
            foreach (IGoogleSyncService contactSyncService in googleContactSyncServices)
            {
                contactSyncService.SetGoogleToken(googleToken);
            }

            //register all the google sync services.
            var businessSyncService = DependencyResolver.Resolve<IBusinessSyncService>();
            foreach (var contactSyncService in googleContactSyncServices)
            {
                businessSyncService.RegisterContactSyncService(contactSyncService);
            }

            DoSyncContacts(businessSyncService);

            _logger.InfoFormat("--------结束与Google进行联系人同步--------");
        }

        private void DoSyncTasks(IBusinessSyncService businessSyncService)
        {
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

            _logger.InfoFormat("开始处理比较结果：");
            ProcessTaskSyncResult(result, _account);
            _logger.InfoFormat("处理比较结果结束");
        }
        private void DoSyncContacts(IBusinessSyncService businessSyncService)
        {
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
            ProcessContactSyncResult(result, _account);
            _logger.InfoFormat("处理比较结果结束");
        }

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

                if (connection.GetType() == typeof(GoogleConnection))
                {
                    _googleConnection = connection as GoogleConnection;
                }
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
