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

namespace Cooper.Sync.Test
{
    /// <summary>
    /// Google数据同步处理器接口定义，该处理器负责实现整个同步过程
    /// </summary>
    public interface IGoogleSyncProcesser
    {
        /// <summary>
        /// 根据默认支持的同步服务来同步任务与联系人
        /// </summary>
        void SyncTasksAndContacts(int connectionId);
        /// <summary>
        /// 根据指定的同步服务同步数据
        /// </summary>
        void SyncTasksAndContacts(int connectionId, IEnumerable<ISyncService<TaskSyncData, ISyncData, TaskSyncResult>> taskSyncServices, IEnumerable<ISyncService<ContactSyncData, ISyncData, ContactSyncResult>> contactSyncServices);
    }

    /// <summary>
    /// 数据同步处理器实现类
    /// </summary>
    [Component]
    public class GoogleSyncProcesser : SyncProcesser, IGoogleSyncProcesser
    {
        #region Private Variables

        private IGoogleTaskSyncService _googleTaskSyncService;
        private IGoogleCalendarEventSyncService _googleCalendarEventSyncService;
        private IGoogleContactSyncService _googleContactSyncService;
        private IGoogleTokenService _googleTokenService;
        private IAuthorizationState _token;

        #endregion

        #region Constructors

        public GoogleSyncProcesser(
            ILockHelper lockHelper,
            ILoggerFactory loggerFactory,
            ISessionManager sessionManager,
            IAccountHelper accountHelper,
            IAccountService accountService,
            IAccountConnectionService accountConnectionService,
            ITaskService taskService,
            IExternalServiceProvider externalServiceProvider,
            IGoogleTokenService googleTokenService,
            IGoogleTaskSyncService googleTaskSyncService,
            IGoogleCalendarEventSyncService googleCalendarEventSyncService,
            IGoogleContactSyncService googleContactSyncService)
            : base(lockHelper, loggerFactory, sessionManager, accountHelper, accountService, accountConnectionService, taskService, externalServiceProvider)
        {
            lockHelper.Init<GoogleConnection>();
            _googleTokenService = googleTokenService;
            _googleTaskSyncService = googleTaskSyncService;
            _googleCalendarEventSyncService = googleCalendarEventSyncService;
            _googleContactSyncService = googleContactSyncService;
        }

        #endregion

        #region Public Methods

        /// <summary>
        /// 根据默认支持的同步服务来同步任务与联系人
        /// </summary>
        public void SyncTasksAndContacts(int connectionId)
        {
            SyncTasksAndContacts(
                connectionId,
                new List<ISyncService<TaskSyncData, ISyncData, TaskSyncResult>> { _googleTaskSyncService, _googleCalendarEventSyncService },
                null);
        }
        /// <summary>
        /// 根据给定的同步服务来同步任务与联系人
        /// </summary>
        public void SyncTasksAndContacts(int connectionId, IEnumerable<ISyncService<TaskSyncData, ISyncData, TaskSyncResult>> taskSyncServices, IEnumerable<ISyncService<ContactSyncData, ISyncData, ContactSyncResult>> contactSyncServices)
        {
            if (InitializeAccountAndConnection(connectionId))
            {
                if (taskSyncServices != null && taskSyncServices.Count() > 0)
                {
                    SyncTasks(taskSyncServices);
                }
                if (contactSyncServices != null && contactSyncServices.Count() > 0)
                {
                    SyncContacts(contactSyncServices);
                }
            }
        }

        #endregion

        #region Override Methods

        protected override void SyncTasks(IEnumerable<ISyncService<TaskSyncData, ISyncData, TaskSyncResult>> taskSyncServices)
        {
            _logger.Info("------------开始与Google同步任务------------");

            try
            {
                foreach (IGoogleSyncService taskSyncService in taskSyncServices)
                {
                    taskSyncService.SetGoogleToken(_token);
                }
                base.SyncTasks(taskSyncServices);
            }
            catch (Exception ex)
            {
                _logger.Error(ex);
            }
            finally
            {
                _logger.Info("------------结束与Google同步任务------------");
            }
        }
        protected override void SyncContacts(IEnumerable<ISyncService<ContactSyncData, ISyncData, ContactSyncResult>> contactSyncServices)
        {
            _logger.Info("------------开始与Google同步联系人------------");

            try
            {
                foreach (IGoogleSyncService contactSyncService in contactSyncServices)
                {
                    contactSyncService.SetGoogleToken(_token);
                }
                SyncContacts(contactSyncServices);
            }
            catch (Exception ex)
            {
                _logger.Error(ex);
            }
            finally
            {
                _logger.Info("------------结束与Google同步联系人------------");
            }
        }
        protected override void PersistSyncTaskDatas(IEnumerable<ISyncData> syncDatasToCreate, IEnumerable<ISyncData> syncDatasToUpdate, IEnumerable<ISyncData> syncDatasToDelete, Account account)
        {
            var tasksToCreate = syncDatasToCreate.Where(x => x.GetType() == typeof(GoogleTaskSyncData)).Cast<GoogleTaskSyncData>();
            var tasksToUpdate = syncDatasToUpdate.Where(x => x.GetType() == typeof(GoogleTaskSyncData)).Cast<GoogleTaskSyncData>();
            var tasksToDelete = syncDatasToDelete.Where(x => x.GetType() == typeof(GoogleTaskSyncData)).Cast<GoogleTaskSyncData>();
            var calendarEventsToCreate = syncDatasToCreate.Where(x => x.GetType() == typeof(GoogleCalendarEventSyncData)).Cast<GoogleCalendarEventSyncData>();
            var calendarEventsToUpdate = syncDatasToUpdate.Where(x => x.GetType() == typeof(GoogleCalendarEventSyncData)).Cast<GoogleCalendarEventSyncData>();
            var calendarEventsToDelete = syncDatasToDelete.Where(x => x.GetType() == typeof(GoogleCalendarEventSyncData)).Cast<GoogleCalendarEventSyncData>();

            if (tasksToCreate.Count() > 0 || tasksToUpdate.Count() > 0 || tasksToDelete.Count() > 0)
            {
                PersistGoogleTaskDatas(tasksToCreate, tasksToUpdate, tasksToDelete);
            }
            if (calendarEventsToCreate.Count() > 0 || calendarEventsToUpdate.Count() > 0 || calendarEventsToDelete.Count() > 0)
            {
                PersistGoogleCalendarEventDatas(calendarEventsToCreate, calendarEventsToUpdate, calendarEventsToDelete);
            }
        }
        protected override void PersistSyncContactDatas(IEnumerable<ISyncData> syncDatasToCreate, IEnumerable<ISyncData> syncDatasToUpdate, IEnumerable<ISyncData> syncDatasToDelete, Account account)
        {
            var googleContactsToCreate = syncDatasToCreate.Where(x => x.GetType() == typeof(GoogleContactSyncData)).Cast<GoogleContactSyncData>();
            var googleContactsToUpdate = syncDatasToUpdate.Where(x => x.GetType() == typeof(GoogleContactSyncData)).Cast<GoogleContactSyncData>();
            var googleContactsToDelete = syncDatasToDelete.Where(x => x.GetType() == typeof(GoogleContactSyncData)).Cast<GoogleContactSyncData>();

            if (googleContactsToCreate.Count() > 0 || googleContactsToUpdate.Count() > 0 || googleContactsToDelete.Count() > 0)
            {
                PersistGoogleContactDatas(googleContactsToCreate, googleContactsToUpdate, googleContactsToDelete);
            }
        }

        #endregion

        #region Helper Methods

        /// <summary>
        /// 初始化Account以及Connection
        /// </summary>
        private bool InitializeAccountAndConnection(int connectionId)
        {
            var connection = _accountConnectionService.GetConnection(connectionId) as GoogleConnection;
            if (connection == null)
            {
                _logger.WarnFormat("无效的Google账号连接，连接ID：{0}", connectionId);
                return false;
            }

            InitializeAccount(connection);
            RefreshConnectionToken(connection);

            return true;
        }
        /// <summary>
        /// 刷新GoogleToken信息
        /// </summary>
        private void RefreshConnectionToken(GoogleConnection connection)
        {
            _token = _googleTokenService.DeserializeToken(connection.Token);
            _googleTokenService.RefreshToken(_token);

            connection.SetToken(_googleTokenService.SerializeToken(_token));
            _accountConnectionService.Update(connection);
        }

        #region 持久化Google相关数据的函数

        #region Google Task

        /// <summary>
        /// 持久化Google Task
        /// </summary>
        private void PersistGoogleTaskDatas(IEnumerable<GoogleTaskSyncData> datasToCreate, IEnumerable<GoogleTaskSyncData> datasToUpdate, IEnumerable<GoogleTaskSyncData> datasToDelete)
        {
            var googleTaskService = _externalServiceProvider.GetGoogleTaskService(_token);
            bool isDefaultTaskListExist = false;
            var taskList = DependencyResolver.Resolve<IGoogleTaskSyncDataService>().GetDefaultTaskList(_token, out isDefaultTaskListExist);

            foreach (var dataToCreate in datasToCreate)
            {
                CreateGoogleTask(dataToCreate, googleTaskService, taskList);
            }

            foreach (var dataToUpdate in datasToUpdate)
            {
                UpdateGoogleTask(dataToUpdate, googleTaskService, taskList);
            }

            foreach (var dataToDelete in datasToDelete)
            {
                DeleteGoogleTask(dataToDelete, googleTaskService, taskList);
            }
        }
        /// <summary>
        /// 创建Google Task
        /// </summary>
        private void CreateGoogleTask(GoogleTaskSyncData taskData, TasksService googleTaskService, TaskList defaultTaskList)
        {
            global::Google.Apis.Tasks.v1.Data.Task googleTask = null;
            bool success = false;

            try
            {
                //创建Google Task
                googleTask = googleTaskService.Tasks.Insert(taskData.GoogleTask, defaultTaskList.Id).Fetch();
                _logger.InfoFormat("新增Google任务#{0}|{1}|{2}", googleTask.Id, googleTask.Title, _account.ID);
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
                    syncInfo.AccountId = _account.ID;
                    syncInfo.LocalDataId = taskData.SyncId;
                    syncInfo.SyncDataId = googleTask.Id;
                    syncInfo.SyncDataType = taskData.SyncType;
                    InsertSyncInfo(syncInfo);
                }
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
        /// 删除Google Task
        /// </summary>
        private void DeleteGoogleTask(GoogleTaskSyncData taskData, TasksService googleTaskService, TaskList defaultTaskList)
        {
            bool success = false;

            try
            {
                googleTaskService.Tasks.Delete(defaultTaskList.Id, taskData.Id).Fetch();
                _logger.InfoFormat("删除Google任务#{0}|{1}|{2}", taskData.Id, taskData.Subject, _account.ID);
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
                syncInfo.AccountId = _account.ID;
                syncInfo.LocalDataId = taskData.SyncId;
                syncInfo.SyncDataId = taskData.Id;
                syncInfo.SyncDataType = taskData.SyncType;
                DeleteSyncInfo(syncInfo);
            }
        }

        #endregion

        #region Google Calendar Event

        /// <summary>
        /// 持久化Google Calendar Event
        /// </summary>
        private void PersistGoogleCalendarEventDatas(IEnumerable<GoogleCalendarEventSyncData> datasToCreate, IEnumerable<GoogleCalendarEventSyncData> datasToUpdate, IEnumerable<GoogleCalendarEventSyncData> datasToDelete)
        {
            var googleCalendarService = _externalServiceProvider.GetGoogleCalendarService(_token);
            bool isDefaultCalendarExist = false;
            var defaultCalendar = DependencyResolver.Resolve<IGoogleCalendarEventSyncDataService>().GetDefaultCalendar(_token, out isDefaultCalendarExist);

            foreach (var dataToCreate in datasToCreate)
            {
                CreateGoogleCalendarEvent(dataToCreate, googleCalendarService, defaultCalendar);
            }

            foreach (var dataToUpdate in datasToUpdate)
            {
                UpdateGoogleCalendarEvent(dataToUpdate, googleCalendarService, defaultCalendar);
            }

            foreach (var dataToDelete in datasToDelete)
            {
                DeleteGoogleCalendarEvent(dataToDelete, googleCalendarService, defaultCalendar);
            }
        }
        /// <summary>
        /// 创建Google Calendar Event
        /// </summary>
        private void CreateGoogleCalendarEvent(GoogleCalendarEventSyncData calendarEventData, CalendarService calendarService, Calendar defaultCalendar)
        {
            global::Google.Apis.Calendar.v3.Data.Event calendarEvent = null;
            bool success = false;

            try
            {
                //创建Google Calendar Event
                calendarEvent = calendarService.Events.Insert(calendarEventData.GoogleCalendarEvent, defaultCalendar.Id).Fetch();
                _logger.InfoFormat("新增Google日历事件#{0}|{1}|{2}", calendarEvent.Id, calendarEvent.Summary, _account.ID);
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
                    syncInfo.AccountId = _account.ID;
                    syncInfo.LocalDataId = calendarEventData.SyncId;
                    syncInfo.SyncDataId = calendarEvent.Id;
                    syncInfo.SyncDataType = calendarEventData.SyncType;
                    InsertSyncInfo(syncInfo);
                }
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
        /// 删除Google Calendar Event
        /// </summary>
        private void DeleteGoogleCalendarEvent(GoogleCalendarEventSyncData calendarEventData, CalendarService googleCalendarService, Calendar defaultCalendar)
        {
            bool success = false;

            try
            {
                googleCalendarService.Events.Delete(defaultCalendar.Id, calendarEventData.Id).Fetch();
                _logger.InfoFormat("删除Google日历事件#{0}|{1}|{2}", calendarEventData.Id, calendarEventData.Subject, _account.ID);
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
                syncInfo.AccountId = _account.ID;
                syncInfo.LocalDataId = calendarEventData.SyncId;
                syncInfo.SyncDataId = calendarEventData.Id;
                syncInfo.SyncDataType = calendarEventData.SyncType;
                DeleteSyncInfo(syncInfo);
            }
        }

        #endregion

        #region Google Contact

        /// <summary>
        /// 持久化Google Contact
        /// </summary>
        private void PersistGoogleContactDatas(IEnumerable<GoogleContactSyncData> datasToCreate, IEnumerable<GoogleContactSyncData> datasToUpdate, IEnumerable<GoogleContactSyncData> datasToDelete)
        {
            var googleContactRequest = _externalServiceProvider.GetGoogleContactRequest(_token);
            bool isDefaultContactGroupExist = false;
            var defaultContactGroup = DependencyResolver.Resolve<IGoogleContactSyncDataService>().GetDefaultContactGroup(_token, out isDefaultContactGroupExist);

            foreach (var dataToCreate in datasToCreate)
            {
                CreateGoogleContact(dataToCreate, googleContactRequest, defaultContactGroup);
            }

            foreach (var dataToUpdate in datasToUpdate)
            {
                UpdateGoogleContact(dataToUpdate, googleContactRequest);
            }

            foreach (var dataToDelete in datasToDelete)
            {
                DeleteGoogleContact(dataToDelete, googleContactRequest);
            }
        }
        /// <summary>
        /// 创建Google Contact
        /// </summary>
        private void CreateGoogleContact(GoogleContactSyncData contactData, ContactsRequest contactRequest, Group defaultContactGroup)
        {
            global::Google.Contacts.Contact contact = null;
            bool success = false;

            try
            {
                //设置联系人默认分组
                contactData.Contact.GroupMembership.Add(new GroupMembership() { HRef = defaultContactGroup.Id });
                //调用API新增联系人
                contact = contactRequest.Insert(new Uri(GoogleSyncSettings.ContactScope), contactData.Contact);
                _logger.InfoFormat("新增Google联系人#{0}|{1}|{2}", contact.Id, contactData.Subject, _account.ID);
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
                syncInfo.AccountId = _account.ID;
                syncInfo.LocalDataId = contactData.SyncId;
                syncInfo.SyncDataId = contact.Id;
                syncInfo.SyncDataType = contactData.SyncType;
                InsertSyncInfo(syncInfo);
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
        /// 删除Google Contact
        /// </summary>
        private void DeleteGoogleContact(GoogleContactSyncData contactData, ContactsRequest contactRequest)
        {
            bool success = false;

            try
            {
                ReplaceContactEditUrl(contactData.Contact);
                contactRequest.Delete(contactData.Contact);
                _logger.InfoFormat("删除Google联系人#{0}|{1}|{2}", contactData.Id, contactData.Subject, _account.ID);
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
                syncInfo.AccountId = _account.ID;
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

        #endregion

        #endregion
    }
}
