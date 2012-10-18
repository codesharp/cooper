using System;
using System.IO;
using System.Net;
using System.Text;

namespace CooperDemo.Infrastructure
{
    public class HttpWebRequestHelper
    {
        /// <summary>
        /// ͬ������HttpRequest
        /// </summary>
        /// <param name="cookie"></param>
        /// <param name="url"></param>
        /// <param name="postData"></param>
        /// <returns></returns>
        public static HttpWebResponse SendHttpPostRequest(Cookie cookie, string url, string postData)
        {
            //���https�µ�֤������
            HttpRequestCredentialHelper.SetDefaultCredentialValidationLogic();

            var request = HttpWebRequest.Create(url) as HttpWebRequest;

            //������������ΪPOST
            request.Method = "POST";

            //����Post������
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

            //��Cookie�����������÷�����֪����ǰ�û������
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
        /// �첽����HttpRequest
        /// </summary>
        /// <param name="cookie"></param>
        /// <param name="url"></param>
        /// <param name="postData"></param>
        /// <param name="callback"></param>
        public static void SendHttpPostRequest(Cookie cookie, string url, string postData, Action<HttpWebResponse> callback)
        {
            //���https�µ�֤������
            HttpRequestCredentialHelper.SetDefaultCredentialValidationLogic();

            var request = HttpWebRequest.Create(url) as HttpWebRequest;

            //������������ΪPOST
            request.Method = "POST";

            //����Post������
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

            //��Cookie�����������÷�����֪����ǰ�û������
            var container = new CookieContainer();
            request.CookieContainer = container;
            if (cookie != null)
            {
                container.SetCookies(new Uri(Constants.ROOT_URL), string.Format("{0}={1}", cookie.Name, cookie.Value));
                var logger = DependencyResolver.Resolve<ILoggerFactory>().Create(typeof(HttpWebRequestHelper));
                logger.InfoFormat("HttpWebRequest CookieName:{0}, Value:{1}", cookie.Name, cookie.Value);
            }

            //�첽��������
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
        /// ����ָ��HttpWebResponse���ı���Ϣ
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

