using System.Collections.Generic;

namespace Cooper.Sync
{
    public class ContactSyncGoogleContactDataService : ISyncDataService<ContactSyncData, GoogleContactSyncData>
    {
        public IList<ContactSyncData> GetSyncDataList()
        {
            return new List<ContactSyncData>();
        }

        public ContactSyncData CreateFrom(GoogleContactSyncData syncDataSource)
        {
            ContactSyncData contactSyncData = new ContactSyncData();

            contactSyncData.FullName = syncDataSource.Contact.Name.FullName;
            contactSyncData.Email = syncDataSource.Contact.Emails.Count > 0 ? syncDataSource.Contact.Emails[0].Address : null;
            contactSyncData.Phone = syncDataSource.Contact.Phonenumbers.Count > 0 ? syncDataSource.Contact.Phonenumbers[0].Value : null;

            return contactSyncData;

            //var contacterId = _contactService.CreateContacter(
            //    new Contacter
            //    {
            //        AccountId = accountId,
            //        AddressBookId = 0, //set with the default personal addressbook id.
            //        FullName = syncDataSource.Contact.Name.FullName,
            //        Email = syncDataSource.Contact.Emails.Count > 0 ? syncDataSource.Contact.Emails[0].Address : null,
            //        Phone = syncDataSource.Contact.Phonenumbers.Count > 0 ? syncDataSource.Contact.Phonenumbers[0].Value : null,
            //        CreateTime = DateTime.Now
            //    });

            //return contacterId.ToString();
        }

        public void UpdateSyncData(ContactSyncData syncData, GoogleContactSyncData syncDataSource)
        {
            syncData.FullName = syncDataSource.Contact.Name.FullName;
            syncData.Email = syncDataSource.Contact.Emails.Count > 0 ? syncDataSource.Contact.Emails[0].Address : null;
            syncData.Phone = syncDataSource.Contact.Phonenumbers.Count > 0 ? syncDataSource.Contact.Phonenumbers[0].Value : null;
        }
    }
}
