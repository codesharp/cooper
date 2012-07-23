using System.Collections.Generic;
using Google.Apis.Calendar.v3;
using Google.Apis.Tasks.v1;
using Google.Apis.Util;

namespace Cooper.Sync
{
    public static class GoogleSyncSettings
    {
        private static readonly IList<string> _scopes;

        static GoogleSyncSettings()
        {
            _scopes = new List<string>();
            _scopes.Add(UserMailScope);
            _scopes.Add(UserProfileScope);
            _scopes.Add(TaskScope);
            _scopes.Add(CalendarScope);
            _scopes.Add(CalendarScope2);
            _scopes.Add(ContactScope);
            _scopes.Add(ContactScope2);
        }

        public static string ApplicationName = "Cooper Task Management";
        public static string ClientIdentifier;
        public static string ClientSecret;
        public static string RedirectUri = "urn:ietf:wg:oauth:2.0:oob";

        public static IEnumerable<string> DefaultSyncScopes { get { return _scopes; } }
        public static readonly string UserMailScope = "https://www.googleapis.com/auth/userinfo.email";
        public static readonly string UserProfileScope = "https://www.googleapis.com/auth/userinfo.profile";
        public static readonly string TaskScope = TasksService.Scopes.Tasks.GetStringValue();
        public static readonly string CalendarScope = CalendarService.Scopes.Calendar.GetStringValue();
        public static readonly string CalendarScope2 = "https://www.google.com/calendar/feeds";
        public static readonly string ContactScope = "https://www.google.com/m8/feeds/contacts/default/full";
        public static readonly string ContactGroupScope = "https://www.google.com/m8/feeds/groups/default/full";
        public static readonly string ContactScope2 = "https://www.google.com/m8/feeds";

        public static readonly string DefaultTaskListName = "CooperTaskList";
        public static readonly string DefaultCalendarName = "CooperCalendar";
        public static readonly string DefaultContactGroupName = "Cooper Contacts";

        // 表示默认导入日历中从现在开始往后一个月的日历事件
        public static readonly int DefaultCalendarImportTimeMonths = 3;

        public static readonly int DefaultMaxTaskListCount = 100000000;
        public static readonly long DefaultMaxCalendarCount = 100000000;
        public static readonly int DefaultMaxTaskCount = 100000000;
        public static readonly long DefaultMaxCalendarEventCount = 100000000;
        public static readonly int DefaultMaxContactCount = 100000000;
        public static readonly int DefaultMaxContactGroupCount = 10000;
    }
}
