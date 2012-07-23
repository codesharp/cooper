using DotNetOpenAuth.OAuth2;

namespace Cooper.Sync
{
    public interface IGoogleCalendarEventSyncService : IGoogleSyncService, ISyncService<TaskSyncData, GoogleCalendarEventSyncData, TaskSyncResult>
    {
    }
    public class GoogleCalendarEventSyncService : SyncService<TaskSyncData, GoogleCalendarEventSyncData, TaskSyncResult>, IGoogleCalendarEventSyncService
    {
        private IGoogleCalendarEventSyncDataService _googleCalendarEventSyncDataService;

        public GoogleCalendarEventSyncService(ISyncDataService<TaskSyncData, GoogleCalendarEventSyncData> localDataService, IGoogleCalendarEventSyncDataService syncDataService)
            : base(localDataService, syncDataService)
        {
            _googleCalendarEventSyncDataService = syncDataService;
        }

        protected override int GetSyncDataType()
        {
            return (int)SyncDataType.GoogleCalendarEvent;
        }

        public void SetGoogleToken(IAuthorizationState token)
        {
            _googleCalendarEventSyncDataService.SetGoogleToken(token);
        }
    }
}