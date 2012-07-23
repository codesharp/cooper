using System;
using GoogleTask = Google.Apis.Tasks.v1.Data.Task;

namespace Cooper.Sync
{
    public class GoogleTaskSyncData : ISyncData
    {
        public GoogleTaskSyncData(GoogleTask googleTask)
        {
            GoogleTask = googleTask;
            IsFromDefault = true;
        }

        public GoogleTask GoogleTask { get; private set; }

        public string Id
        {
            get
            {
                return GoogleTask.Id;
            }
        }
        public string Subject
        {
            get { return GoogleTask.Title; }
        }
        public DateTime LastUpdateLocalTime
        {
            get
            {
                return Utils.IsDateTime(GoogleTask.Updated) ? Rfc3339DateTime.Parse(GoogleTask.Updated).ToLocalTime() : DateTime.MinValue;
            }
            set
            { }
        }

        public string SyncId { get; set; }
        public int SyncType { get; set; }
        public bool IsFromDefault { get; set; }
    }
}
