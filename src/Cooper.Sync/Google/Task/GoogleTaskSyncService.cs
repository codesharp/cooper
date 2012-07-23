using DotNetOpenAuth.OAuth2;

namespace Cooper.Sync
{
    public interface IGoogleTaskSyncService : IGoogleSyncService, ISyncService<TaskSyncData, GoogleTaskSyncData, TaskSyncResult>
    {
    }
    public class GoogleTaskSyncService : SyncService<TaskSyncData, GoogleTaskSyncData, TaskSyncResult>, IGoogleTaskSyncService
    {
        private IGoogleTaskSyncDataService _googleTaskSyncDataService;

        public GoogleTaskSyncService(ISyncDataService<TaskSyncData, GoogleTaskSyncData> localDataService, IGoogleTaskSyncDataService syncDataService)
            : base(localDataService, syncDataService)
        {
            AllowAutoCreateSyncInfo = true;
            _googleTaskSyncDataService = syncDataService;
        }

        protected override int GetSyncDataType()
        {
            return (int)SyncDataType.GoogleTask;
        }

        public void SetGoogleToken(IAuthorizationState token)
        {
            _googleTaskSyncDataService.SetGoogleToken(token);
        }
    }
}