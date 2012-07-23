using DotNetOpenAuth.OAuth2;

namespace Cooper.Sync
{
    public interface IGoogleSyncService
    {
        void SetGoogleToken(IAuthorizationState token);
    }
}
