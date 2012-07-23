using System;
using System.Collections.Generic;

namespace Cooper.Sync
{
    public class TaskSyncGoogleTaskDataService : ISyncDataService<TaskSyncData, GoogleTaskSyncData>
    {
        public IList<TaskSyncData> GetSyncDataList()
        {
            return new List<TaskSyncData>();
        }
        public TaskSyncData CreateFrom(GoogleTaskSyncData syncDataSource)
        {
            DateTime? dueTime = null;
            if (Utils.IsDateTime(syncDataSource.GoogleTask.Due))
            {
                dueTime = Rfc3339DateTime.Parse(syncDataSource.GoogleTask.Due);
            }

            TaskSyncData taskSyncData = new TaskSyncData();

            taskSyncData.Subject = syncDataSource.GoogleTask.Title;
            taskSyncData.Body = syncDataSource.GoogleTask.Notes;

            if (dueTime != null)
            {
                var dueTimeLocalFormat = new DateTime(dueTime.Value.Year, dueTime.Value.Month, dueTime.Value.Day, 0, 0, 0, DateTimeKind.Local);
                taskSyncData.DueTime = dueTimeLocalFormat;
            }
            else
            {
                taskSyncData.DueTime = null;
            }

            if (syncDataSource.GoogleTask.Status == "needsAction")
            {
                taskSyncData.IsCompleted = false;
            }
            else if (syncDataSource.GoogleTask.Status == "completed")
            {
                taskSyncData.IsCompleted = true;
            }

            return taskSyncData;
        }
        public void UpdateSyncData(TaskSyncData syncData, GoogleTaskSyncData syncDataSource)
        {
            syncData.Subject = syncDataSource.GoogleTask.Title;
            syncData.Body = syncDataSource.GoogleTask.Notes;

            DateTime? dueTime = null;
            if (Utils.IsDateTime(syncDataSource.GoogleTask.Due))
            {
                dueTime = Rfc3339DateTime.Parse(syncDataSource.GoogleTask.Due);
            }
            if (dueTime != null)
            {
                var dueTimeLocalFormat = new DateTime(dueTime.Value.Year, dueTime.Value.Month, dueTime.Value.Day, 0, 0, 0, DateTimeKind.Local);
                syncData.DueTime = dueTimeLocalFormat;
            }
            else
            {
                syncData.DueTime = null;
            }

            if (syncDataSource.GoogleTask.Status == "needsAction")
            {
                syncData.IsCompleted = false;
            }
            else if (syncDataSource.GoogleTask.Status == "completed")
            {
                syncData.IsCompleted = true;
            }
        }
    }
}
