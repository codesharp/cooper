using System;
using System.IO;
using System.Net;
using System.Text;

namespace CooperDemo.Infrastructure
{
    public class HttpWebRequestHelper
    {
        /// <summary>
        /// 同步发送HttpRequest
        /// </summary>
        /// <param name="cookie"></param>
        /// <param name="url"></param>
        /// <param name="postData"></param>
        /// <returns></returns>
        public static HttpWebResponse SendHttpPostRequest(Cookie cookie, string url, string postData)
        {
            //解决https下的证书问题
            HttpRequestCredentialHelper.SetDefaultCredentialValidationLogic();

            var request = HttpWebRequest.Create(url) as HttpWebRequest;

            //设置请求类型为POST
            request.Method = "POST";

            //设置Post的数据
            if (!string.IsNullOrEmpty(postData))
            {
                request.ContentLength = postData.Length;
                request.ContentType = "application/x-www-form-urlencoded";
                using (var writer = new StreamWriter(request.GetRequestStream()))
                {
                    writer.Write(postData);
                    writer.Close();
                }
            }

            //将Cookie放入请求，以让服务器知道当前用户的身份
            var container = new CookieContainer();
            request.CookieContainer = container;
            if (cookie != null)
            {
                container.SetCookies(new Uri(Constants.ROOT_URL), string.Format("{0}={1}", cookie.Name, cookie.Value));
                var logger = DependencyResolver.Resolve<ILoggerFactory>().Create(typeof(HttpWebRequestHelper));
                logger.InfoFormat("HttpWebRequest CookieName:{0}, Value:{1}", cookie.Name, cookie.Value);
            }

            return request.GetResponse() as HttpWebResponse;
        }
        /// <summary>
        /// 异步发送HttpRequest
        /// </summary>
        /// <param name="cookie"></param>
        /// <param name="url"></param>
        /// <param name="postData"></param>
        /// <param name="callback"></param>
        public static void SendHttpPostRequest(Cookie cookie, string url, string postData, Action<HttpWebResponse> callback)
        {
            //解决https下的证书问题
            HttpRequestCredentialHelper.SetDefaultCredentialValidationLogic();

            var request = HttpWebRequest.Create(url) as HttpWebRequest;

            //设置请求类型为POST
            request.Method = "POST";

            //设置Post的数据
            if (!string.IsNullOrEmpty(postData))
            {
                request.ContentLength = postData.Length;
                request.ContentType = "application/x-www-form-urlencoded";
                using (var writer = new StreamWriter(request.GetRequestStream()))
                {
                    writer.Write(postData);
                    writer.Close();
                }
            }

            //将Cookie放入请求，以让服务器知道当前用户的身份
            var container = new CookieContainer();
            request.CookieContainer = container;
            if (cookie != null)
            {
                container.SetCookies(new Uri(Constants.ROOT_URL), string.Format("{0}={1}", cookie.Name, cookie.Value));
                var logger = DependencyResolver.Resolve<ILoggerFactory>().Create(typeof(HttpWebRequestHelper));
                logger.InfoFormat("HttpWebRequest CookieName:{0}, Value:{1}", cookie.Name, cookie.Value);
            }

            //异步发送请求
            request.BeginGetResponse(new AsyncCallback(asyncResult =>
            {
                var httpRequest = asyncResult.AsyncState as HttpWebRequest;
                using (var response = httpRequest.EndGetResponse(asyncResult) as HttpWebResponse)
                {
                    callback(response);
                }
            }), request);
        }
        /// <summary>
        /// 返回指定HttpWebResponse的文本信息
        /// </summary>
        /// <param name="response"></param>
        /// <returns></returns>
        public static string GetTextFromResponse(HttpWebResponse response)
        {
            Encoding encoding = Encoding.UTF8;
            string charset = response.CharacterSet;
            if (!string.IsNullOrEmpty(charset))
            {
                encoding = Encoding.GetEncoding(charset);
            }

            string result;

            using (StreamReader reader = new StreamReader(response.GetResponseStream(), encoding))
            {
                result = reader.ReadToEnd();
                reader.Close();
            }

            return result;
        }
    }
}

