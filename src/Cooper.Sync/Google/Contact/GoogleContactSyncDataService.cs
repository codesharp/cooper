using System;
using System.Collections.Generic;
using System.Linq;
using DotNetOpenAuth.OAuth2;
using Google.Contacts;
using Google.GData.Client;
using Google.GData.Contacts;
using Google.GData.Extensions;
using CodeSharp.Core;
using CodeSharp.Core.Services;

namespace Cooper.Sync
{
    public interface IGoogleContactSyncDataService : IGoogleSyncService, ISyncDataService<GoogleContactSyncData, ContactSyncData>
    {
        Group GetDefaultContactGroup(IAuthorizationState token, out bool isDefaultContactGroupExist);
    }

    public class GoogleContactSyncDataService : IGoogleContactSyncDataService
    {
        private IAuthorizationState _token;
        private IExternalServiceProvider _externalServiceProvider;
        private ILog _logger;

        public GoogleContactSyncDataService(IExternalServiceProvider externalServiceProvider, ILoggerFactory loggerFactory)
        {
            _externalServiceProvider = externalServiceProvider;
            _logger = loggerFactory.Create(GetType());
        }

        public IList<GoogleContactSyncData> GetSyncDataList()
        {
            var contactRequest = _externalServiceProvider.GetGoogleContactRequest(_token);
            contactRequest.Settings.AutoPaging = false;
            contactRequest.Settings.Maximum = GoogleSyncSettings.DefaultMaxContactCount;

            var isDefaultContactGroupExist = false;
            var defaultContactGroup = GetDefaultContactGroup(_token, out isDefaultContactGroupExist);
            var contactsQuery = new ContactsQuery(GoogleSyncSettings.ContactScope) { Group = defaultContactGroup.Id };

            List<GoogleContactSyncData> items = new List<GoogleContactSyncData>();

            foreach (Contact contact in contactRequest.Get<Contact>(contactsQuery).Entries)
            {
                if (!contact.Deleted)
                {
                    items.Add(new GoogleContactSyncData(contact));
                }
            }

            return items;
        }
        public GoogleContactSyncData CreateFrom(ContactSyncData syncDataSource)
        {
            var contact = new Contact();

            contact.Name = new Name()
            {
                FullName = syncDataSource.FullName
            };

            if (!string.IsNullOrEmpty(syncDataSource.Email))
            {
                contact.Emails.Add(new EMail()
                {
                    Primary = true,
                    Rel = ContactsRelationships.IsWork,
                    Address = syncDataSource.Email
                });
            }

            if (!string.IsNullOrEmpty(syncDataSource.Phone))
            {
                contact.Phonenumbers.Add(new PhoneNumber()
                {
                    Primary = true,
                    Rel = ContactsRelationships.IsWork,
                    Value = syncDataSource.Phone,
                });
            }

            return new GoogleContactSyncData(contact);
        }
        public void UpdateSyncData(GoogleContactSyncData googleSyncData, ContactSyncData syncDataSource)
        {
            googleSyncData.Contact.Name.FullName = syncDataSource.FullName;
        }

        public void SetGoogleToken(IAuthorizationState token)
        {
            _token = token;
        }

        public Group GetDefaultContactGroup(IAuthorizationState token, out bool isDefaultContactGroupExist)
        {
            _logger.InfoFormat("GetDefaultContactGroup method is entered, time:{0}", DateTime.Now);

            Group group = null;

            try
            {
                var request = _externalServiceProvider.GetGoogleContactRequest(token);

                isDefaultContactGroupExist = false;
                SetGoogleToken(token);
                var totalGroups = GetAllContactGroups(request);

                _logger.InfoFormat("----返回的Cooper Contacts Group的总个数:{0}，明细如下：--------", totalGroups.Count());
                foreach (var currentGroup in totalGroups)
                {
                    _logger.InfoFormat("--------Group title:{0}, id:{1}, last update time:{2}", currentGroup.Title, currentGroup.Id, currentGroup.Updated);
                }

                var defaultGroups = totalGroups.Where(x => x.Title == GoogleSyncSettings.DefaultContactGroupName).ToList();
                var totalDefaultGroupCount = defaultGroups.Count();
                _logger.InfoFormat("----默认Cooper Contacts Group的总个数:{0}----", totalDefaultGroupCount);

                if (totalDefaultGroupCount == 0)
                {
                    group = CreateContactGroup(request, GoogleSyncSettings.DefaultContactGroupName);
                    _logger.Info("----默认Cooper Contacts Group不存在，故一个默认的Cooper Contacts Group已被创建。");
                }
                else
                {
                    //如果默认的联系人组多余1个，则删除多余的默认联系人组，只保留第一个默认联系人组
                    if (totalDefaultGroupCount > 1)
                    {
                        _logger.Error("----开始删除多余的默认Cooper Contacts Group");
                        int totalDeletedGroupCount = 0;
                        var groupIdList = defaultGroups.Select(x => x.Id).ToList();
                        for (int index = 1; index < totalDefaultGroupCount; index++)
                        {
                            //删除多余的联系人组
                            var currentGroup = defaultGroups[index];
                            DeleteContactGroup(request, currentGroup);
                            _logger.InfoFormat("----删除了一个多余的默认Cooper Contacts Group, title:{0}, id:{1}, last update time:{2}", currentGroup.Title, currentGroup.Id, currentGroup.Updated);
                            totalDeletedGroupCount++;
                        }

                        _logger.InfoFormat("----被删除的Cooper Contacts Group总个数:{0}", totalDeletedGroupCount);
                    }

                    group = defaultGroups.First();
                    isDefaultContactGroupExist = true;
                }
            }
            catch (Exception ex)
            {
                _logger.ErrorFormat("尝试获取默认Cooper Contacts Group时出现异常：", ex);
                throw;
            }

            _logger.InfoFormat("GetDefaultContactGroup method is exited, time:{0}", DateTime.Now);

            return group;
        }

        private IList<Group> GetAllContactGroups(ContactsRequest request)
        {
            request.Settings.AutoPaging = false;
            request.Settings.Maximum = GoogleSyncSettings.DefaultMaxContactGroupCount;
            return request.GetGroups().Entries.ToList();
        }
        private Group CreateContactGroup(ContactsRequest request, string name)
        {
            return request.Insert(new Uri(GoogleSyncSettings.ContactGroupScope), new Group() { Title = name });
        }
        private void DeleteContactGroup(ContactsRequest request, Group group)
        {
            var groupId = group.Id.Substring(group.Id.LastIndexOf('/') + 1);
            var contactGroupUrl = GoogleSyncSettings.ContactGroupScope + "/" + groupId;
            group.GroupEntry.EditUri = new AtomUri(contactGroupUrl);
            request.Delete(group);
        }
    }
}
