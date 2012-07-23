using DotNetOpenAuth.OAuth2;

namespace Cooper.Sync
{
    public interface IGoogleContactSyncService : IGoogleSyncService, ISyncService<ContactSyncData, GoogleContactSyncData, ContactSyncResult>
    {
    }
    public class GoogleContactSyncService : SyncService<ContactSyncData, GoogleContactSyncData, ContactSyncResult>, IGoogleContactSyncService
    {
        private IGoogleContactSyncDataService _googleContactSyncDataService;

        public GoogleContactSyncService(ISyncDataService<ContactSyncData, GoogleContactSyncData> localDataService, IGoogleContactSyncDataService syncDataService)
            : base(localDataService, syncDataService)
        {
            AllowAutoCreateSyncInfo = true;
            _googleContactSyncDataService = syncDataService;
        }

        protected override int GetSyncDataType()
        {
            return (int)SyncDataType.GoogleContact;
        }

        public void SetGoogleToken(IAuthorizationState token)
        {
            _googleContactSyncDataService.SetGoogleToken(token);
        }
    }
}