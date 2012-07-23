using System;
using GoogleCalendarEvent = Google.Apis.Calendar.v3.Data.Event;

namespace Cooper.Sync
{
    public class GoogleCalendarEventSyncData : ISyncData
    {
        public GoogleCalendarEvent GoogleCalendarEvent { get; private set; }

        public GoogleCalendarEventSyncData(GoogleCalendarEvent calendarEvent)
        {
            GoogleCalendarEvent = calendarEvent;
            IsFromDefault = true;
        }

        public string Id
        {
            get
            {
                return GoogleCalendarEvent.Id;
            }
        }
        public string Subject
        {
            get { return GoogleCalendarEvent.Summary; }
        }
        public DateTime LastUpdateLocalTime
        {
            get
            {
                return Utils.IsDateTime(GoogleCalendarEvent.Updated) ? Rfc3339DateTime.Parse(GoogleCalendarEvent.Updated).ToLocalTime() : DateTime.MinValue;
            }
            set
            { }
        }

        public string SyncId { get; set; }
        public int SyncType { get; set; }
        public bool IsFromDefault { get; set; }
    }
}
