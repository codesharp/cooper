using CodeSharp.Core;
using DotNetOpenAuth.OAuth2;
using Google.Apis.Authentication.OAuth2;
using Google.Apis.Authentication.OAuth2.DotNetOpenAuth;
using Google.Apis.Calendar.v3;
using Google.Apis.Tasks.v1;
using Google.Contacts;
using Google.GData.Client;

namespace Cooper.Sync
{
    /// <summary>
    /// 定义接口，用于提供相关的外部服务
    /// </summary>
    public interface IExternalServiceProvider
    {
        /// <summary>
        /// 获取Google管理任务的服务
        /// </summary>
        TasksService GetGoogleTaskService(IAuthorizationState token);
        /// <summary>
        /// 获取Google管理日历的服务
        /// </summary>
        CalendarService GetGoogleCalendarService(IAuthorizationState token);
        /// <summary>
        /// 获取Google管理联系人的服务
        /// </summary>
        ContactsRequest GetGoogleContactRequest(IAuthorizationState token);
    }

    [Component]
    public class ExternalServiceProvider : IExternalServiceProvider
    {
        public TasksService GetGoogleTaskService(IAuthorizationState token)
        {
            return new TasksService(new OAuth2Authenticator<NativeApplicationClient>(GetNativeApplicationClient(), client => token));
        }
        public CalendarService GetGoogleCalendarService(IAuthorizationState token)
        {
            return new CalendarService(new OAuth2Authenticator<NativeApplicationClient>(GetNativeApplicationClient(), client => token));
        }
        public ContactsRequest GetGoogleContactRequest(IAuthorizationState token)
        {
            return new ContactsRequest(new RequestSettings(GoogleSyncSettings.ApplicationName, CreateOAuth2Parameters(token)));
        }

        private OAuth2Parameters CreateOAuth2Parameters(IAuthorizationState state)
        {
            var parameters = new OAuth2Parameters()
            {
                ClientId = GoogleSyncSettings.ClientIdentifier,
                ClientSecret = GoogleSyncSettings.ClientSecret,
                RedirectUri = GoogleSyncSettings.RedirectUri,
                RefreshToken = state.RefreshToken,
                Scope = GoogleSyncSettings.ContactScope + " " + GoogleSyncSettings.ContactGroupScope
            };
            OAuthUtil.RefreshAccessToken(parameters);

            return parameters;
        }
        private NativeApplicationClient GetNativeApplicationClient()
        {
            return new NativeApplicationClient(GoogleAuthenticationServer.Description)
            {
                ClientIdentifier = GoogleSyncSettings.ClientIdentifier,
                ClientSecret = GoogleSyncSettings.ClientSecret
            };
        }
    }
}
