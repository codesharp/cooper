using System;
using Google.Contacts;

namespace Cooper.Sync
{
    public class GoogleContactSyncData : ISyncData
    {
        public GoogleContactSyncData(Contact contact)
        {
            Contact = contact;
            IsFromDefault = true;
        }

        public Contact Contact { get; private set; }
        public string Id
        {
            get
            {
                return Contact.Id;
            }
        }
        public string Subject
        {
            get { return Contact.Name != null ? Contact.Name.FullName : null; }
        }
        public DateTime LastUpdateLocalTime
        {
            get
            {
                return Contact.Updated.ToLocalTime();
            }
            set
            { }
        }

        public string SyncId { get; set; }
        public int SyncType { get; set; }
        public bool IsFromDefault { get; set; }
    }
}
