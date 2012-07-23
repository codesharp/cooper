using System;
using System.Security.Authentication;
using Cooper.Sync.Google.Helpers;
using DotNetOpenAuth.OAuth2;
using Google.Apis.Authentication.OAuth2;
using Google.Apis.Authentication.OAuth2.DotNetOpenAuth;
using Google.Apis.Oauth2.v2;

namespace Cooper.Sync
{
    /// <summary>
    /// 定义与Google Token相关的服务接口
    /// </summary>
    public interface IGoogleTokenService
    {
        /// <summary>
        /// 请求新的OAuth2 Token
        /// </summary>
        /// <param name="scopes">Token访问API的权限范围</param>
        /// <returns>Token对象</returns>
        IAuthorizationState GetNewAuthToken(params string[] scopes);
        /// <summary>
        /// 刷新OAuth2 Token
        /// </summary>
        void RefreshToken(IAuthorizationState token);
        /// <summary>
        /// 根据Token获取谷歌帐号信息
        /// </summary>
        /// <returns>谷歌帐号</returns>
        string GetAccountNameFromToken(IAuthorizationState token);
        /// <summary>
        /// 将Token序列化为json
        /// </summary>
        string SerializeToken(IAuthorizationState token);
        /// <summary>
        /// 从json串还原token对象
        /// </summary>
        IAuthorizationState DeserializeToken(string tokenJson);
    }
    public class GoogleTokenService : IGoogleTokenService
    {
        private CodeSharp.Core.Utils.Serializer _serializer = new CodeSharp.Core.Utils.Serializer();

        public IAuthorizationState GetNewAuthToken(params string[] scopes)
        {
            IAuthorizationState state = new AuthorizationState(scopes);
            var client = GetNativeApplicationClient();
            string authCode = new LoopbackServerAuthorizationFlow().RetrieveAuthorization(client, state);

            if (string.IsNullOrEmpty(authCode))
            {
                throw new AuthenticationException("The authentication request was cancelled by the user.");
            }

            return client.ProcessUserAuthorization(authCode, state);
        }
        public void RefreshToken(IAuthorizationState token)
        {
            var client = GetNativeApplicationClient();
            client.RefreshToken(token);
        }
        public string GetAccountNameFromToken(IAuthorizationState token)
        {
            var client = GetNativeApplicationClient();
            var oauth2Service = new Oauth2Service(new OAuth2Authenticator<NativeApplicationClient>(client, authProvider => token));
            return oauth2Service.Userinfo.Get().Fetch().Email;
        }
        public string SerializeToken(IAuthorizationState token)
        {
            if (token == null)
            {
                throw new ArgumentNullException("token");
            }

            var tokenData = new GoogkeTokenData();
            tokenData.access_token = token.AccessToken;
            tokenData.token_type = "Bearer"; //目前总是为Bearer
            tokenData.expires_in = 0; //目前忽略该属性
            tokenData.id_token = null;
            tokenData.refresh_token = token.RefreshToken;

            //序列化
            return _serializer.JsonSerialize(tokenData);
        }
        public IAuthorizationState DeserializeToken(string tokenJson)
        {
            if (tokenJson == null)
            {
                throw new ArgumentNullException("tokenJson");
            }
            try
            {
                var tokenData = _serializer.JsonDeserialize<GoogkeTokenData>(tokenJson);
                var state = new AuthorizationState(GoogleSyncSettings.DefaultSyncScopes);

                state.AccessToken = tokenData.access_token;
                state.RefreshToken = tokenData.refresh_token;

                return state;
            }
            catch
            {
                //Log error, TODO
                return null;
            }
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

    public class GoogkeTokenData
    {
        public string access_token { get; set; }
        public string token_type { get; set; }
        public int expires_in { get; set; }
        public string id_token { get; set; }
        public string refresh_token { get; set; }
    }
    //格式如下：
    //{
    //  "access_token" : "ya29.AHES6ZSBkg3BVc0U2DiPQspEo-ifIiRPW-erfHuEsYgpXg",
    //  "token_type" : "Bearer",
    //  "expires_in" : 3600,
    //  "id_token" : "eyJhbGciOiJSUzI1NiJ9.eyJpc3MiOiJhY2NvdW50cy5nb29nbGUuY29tIiwiYXVkIjoiMjM0OTE5MDI4MjcyLWdzbW1uZzA2bmhlb2loNGFqcDYwb3E4czMzYXQxb3MwLmFwcHMuZ29vZ2xldXNlcmNvbnRlbnQuY29tIiwiY2lkIjoiMjM0OTE5MDI4MjcyLWdzbW1uZzA2bmhlb2loNGFqcDYwb3E4czMzYXQxb3MwLmFwcHMuZ29vZ2xldXNlcmNvbnRlbnQuY29tIiwiaWQiOiIxMDQ0MTM5Mzc1Mzg5MjYwNDk2ODgiLCJlbWFpbCI6InR4aF83MjNAMTYzLmNvbSIsInZlcmlmaWVkX2VtYWlsIjoidHJ1ZSIsInRva2VuX2hhc2giOiJZbk5CUzc5dHhzQkJ5azJsdFYtR253IiwiaWF0IjoxMzQxODE3NzgxLCJleHAiOjEzNDE4MjE2ODF9.qydWJ5ffoGdxi1fDLDpOl0zuC1lGyYfBCKaAjAo4cyaDYJdigAZ8qwimyBECCwxx2uFulYZW7NY96C8SjT35YfkCs3sbqF_XQTnPbJ9NdLF8Cb46BvmMWLLotNass3yaZ8iTaISQqxMVbm7xX4C85hA8LeorhVxoSpoqd6sOSis",
    //  "refresh_token" : "1/reG3cCBY8QTlHb-uiJ2koINxNzXaJUU2_hRkm67eg74"
    //}
}
