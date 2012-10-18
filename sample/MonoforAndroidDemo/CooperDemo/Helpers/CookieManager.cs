using System.Collections.Generic;
using System.Net;
using Android.Content;
using CooperDemo.Infrastructure;

namespace CooperDemo
{
    public class CookieManager
    {
        public static void SaveCookie(ISharedPreferences sharedPreferences, string cookieKey, Cookie cookie)
        {
            var editor = sharedPreferences.Edit();
            var cookieValueItems = new List<string>();
            var itemFormat = "{0}:{1}";

            cookieValueItems.Clear();

            cookieValueItems.Add(string.Format(itemFormat, "Domain", cookie.Domain));
            cookieValueItems.Add(string.Format(itemFormat, "Name", cookie.Name));
            cookieValueItems.Add(string.Format(itemFormat, "Value", cookie.Value));
            cookieValueItems.Add(string.Format(itemFormat, "Expires", cookie.Expires.ToString()));
            cookieValueItems.Add(string.Format(itemFormat, "Comment", cookie.Comment));
            cookieValueItems.Add(string.Format(itemFormat, "CommentUri", cookie.CommentUri != null ? cookie.CommentUri.AbsoluteUri : null));
            cookieValueItems.Add(string.Format(itemFormat, "Discard", cookie.Discard));
            cookieValueItems.Add(string.Format(itemFormat, "HttpOnly", cookie.HttpOnly));
            cookieValueItems.Add(string.Format(itemFormat, "Path", cookie.Path));
            cookieValueItems.Add(string.Format(itemFormat, "Port", cookie.Port));
            cookieValueItems.Add(string.Format(itemFormat, "Secure", cookie.Secure));
            cookieValueItems.Add(string.Format(itemFormat, "Version", cookie.Version));

            editor.PutString(cookieKey, string.Join("|", cookieValueItems.ToArray()));
            editor.Commit();

            var logger = DependencyResolver.Resolve<ILoggerFactory>().Create(typeof(CookieManager));
            logger.InfoFormat("Saved Cookie. Domain:{0}, Name:{1}, Value:{2}, Expires:{3}", cookie.Domain, cookie.Name, cookie.Value, cookie.Expires);
        }
        public static Cookie GetCookie(ISharedPreferences sharedPreferences, string cookieKey)
        {
            var cookieString = sharedPreferences.GetString(cookieKey, null);

            if (!string.IsNullOrEmpty(cookieString))
            {
                var cookie = new Cookie();
                var entries = cookieString.Split('|');
                var cookieType = typeof(Cookie);
                foreach (var entry in entries)
                {
                    var index = entry.IndexOf(':');
                    var propertyName = entry.Substring(0, index);
                    var propertyValue = entry.Substring(index + 1);
                    var property = cookieType.GetProperty(propertyName);
                    property.SetValue(cookie, Utils.ConvertType(propertyValue, property.PropertyType), null);
                }

                return cookie;
            }

            return null;
        }
    }
}

