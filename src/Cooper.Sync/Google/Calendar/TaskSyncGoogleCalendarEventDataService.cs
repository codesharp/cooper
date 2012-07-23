using System.Collections.Generic;

namespace Cooper.Sync
{
    public class TaskSyncGoogleCalendarEventDataService : ISyncDataService<TaskSyncData, GoogleCalendarEventSyncData>
    {
        public IList<TaskSyncData> GetSyncDataList()
        {
            return new List<TaskSyncData>();
        }

        public TaskSyncData CreateFrom(GoogleCalendarEventSyncData syncDataSource)
        {
            TaskSyncData taskSyncData = new TaskSyncData();

            taskSyncData.Subject = syncDataSource.GoogleCalendarEvent.Summary;
            taskSyncData.Body = syncDataSource.GoogleCalendarEvent.Description;
            
            if (!string.IsNullOrEmpty(syncDataSource.GoogleCalendarEvent.End.DateTime))
            {
                var end = Rfc3339DateTime.Parse(syncDataSource.GoogleCalendarEvent.End.DateTime);
                end = end.ToLocalTime();
                taskSyncData.DueTime = end;
            }

            return taskSyncData;
        }

        public void UpdateSyncData(TaskSyncData syncData, GoogleCalendarEventSyncData syncDataSource)
        {
            syncData.Subject = syncDataSource.GoogleCalendarEvent.Summary;
            syncData.Body = syncDataSource.GoogleCalendarEvent.Description;

            if (!string.IsNullOrEmpty(syncDataSource.GoogleCalendarEvent.End.DateTime))
            {
                var end = Rfc3339DateTime.Parse(syncDataSource.GoogleCalendarEvent.End.DateTime);
                end = end.ToLocalTime();
                syncData.DueTime = end;
            }
            else
            {
                syncData.DueTime = null;
            }
        }
    }
}
