using System;
using System.Text.RegularExpressions;

namespace Cooper.Sync
{
    /// <summary>
    /// 工具类，提供一些实用工具方法
    /// </summary>
    public class Utils
    {
        public static bool IsDateTime(string dateTimeString)
        {
            if (string.IsNullOrWhiteSpace(dateTimeString))
            {
                return false;
            }

            DateTime tempTime;
            return DateTime.TryParse(dateTimeString, out tempTime);
        }
        public static TimeSpan GetCurrentTimeUtcOffset()
        {
            return System.TimeZone.CurrentTimeZone.GetUtcOffset(DateTime.Now);
        }
    }

    public static class StringHelpers
    {
        public static string StripHTML(string html)
        {
            if (html == null)
            {
                return null;
            }
            var reg = new Regex("<[^>]+>", RegexOptions.IgnoreCase);
            return reg.Replace(html, "").Trim(' ', '\n', '\r');
        }
    }
}
