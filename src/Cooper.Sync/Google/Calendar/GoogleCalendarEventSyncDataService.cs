using System;
using System.Collections.Generic;
using System.Linq;
using DotNetOpenAuth.OAuth2;
using Google.Apis.Calendar.v3;
using Google.Apis.Calendar.v3.Data;
using CodeSharp.Core;
using CodeSharp.Core.Services;

namespace Cooper.Sync
{
    public interface IGoogleCalendarEventSyncDataService : IGoogleSyncService, ISyncDataService<GoogleCalendarEventSyncData, TaskSyncData>
    {
        Calendar GetDefaultCalendar(IAuthorizationState token, out bool isDefaultTaskListExist);
    }
    public class GoogleCalendarEventSyncDataService : IGoogleCalendarEventSyncDataService
    {
        #region Private Variables

        private ILog _logger;
        private IAuthorizationState _token;
        private CalendarService _calendarService;
        private IExternalServiceProvider _externalServiceProvider;

        #endregion

        public GoogleCalendarEventSyncDataService(IExternalServiceProvider externalServiceProvider, ILoggerFactory loggerFactory)
        {
            _externalServiceProvider = externalServiceProvider;
            _logger = loggerFactory.Create(GetType());
        }

        public IList<GoogleCalendarEventSyncData> GetSyncDataList()
        {
            bool isDefaultCalendarExist = false;
            var defaultCalendar = GetDefaultCalendar(_token, out isDefaultCalendarExist);

            DateTime startTime = DateTime.UtcNow.Date;
            DateTime endTime = startTime.AddMonths(GoogleSyncSettings.DefaultCalendarImportTimeMonths);

            bool isFromDefault = true;
            var listRequest = _calendarService.Events.List(defaultCalendar.Id);
            listRequest.MaxResults = GoogleSyncSettings.DefaultMaxCalendarEventCount;
            var evnts = listRequest.Fetch().Items;

            if (!isDefaultCalendarExist)
            {
                //默认的不存在，不从Google默认的日历读取任务
                //evnts = _calendarService.Events.List("primary").Fetch().Items;
                isFromDefault = false;
            }

            List<GoogleCalendarEventSyncData> items = new List<GoogleCalendarEventSyncData>();

            if (evnts != null && evnts.Count() > 0)
            {
                foreach (var evnt in evnts)
                {
                    //这里目前没有找到可以根据开始日期和结束日期查询的接口，
                    //只能查处所有当前日历的事件，然后在内存过滤
                    DateTime result;
                    if (Rfc3339DateTime.TryParse(evnt.End.DateTime, out result))
                    {
                        var end = result.ToLocalTime();
                        if (end >= startTime && end <= endTime)
                        {
                            var syncData = new GoogleCalendarEventSyncData(evnt);
                            syncData.IsFromDefault = isFromDefault;
                            items.Add(syncData);
                        }
                    }
                }
            }

            return items;
        }
        public GoogleCalendarEventSyncData CreateFrom(TaskSyncData syncDataSource)
        {
            var calendarEvent = new Event();
            calendarEvent.Summary = syncDataSource.Subject;
            calendarEvent.Description = syncDataSource.Body;

            DateTime dueTime;
            if (syncDataSource.DueTime != null)
            {
                dueTime = syncDataSource.DueTime.Value;
            }
            else
            {
                dueTime = syncDataSource.CreateTime;
            }

            var currentDate = dueTime.Date;
            var startTime = new DateTime(currentDate.Year, currentDate.Month, currentDate.Day, 0, 0, 0, DateTimeKind.Utc);
            var endTime = startTime.AddHours(1);
            string start = Rfc3339DateTime.ToString(startTime);
            string end = Rfc3339DateTime.ToString(endTime);
            calendarEvent.Start = new EventDateTime() { DateTime = start };
            calendarEvent.End = new EventDateTime() { DateTime = end };

            return new GoogleCalendarEventSyncData(calendarEvent);
        }
        public void UpdateSyncData(GoogleCalendarEventSyncData syncData, TaskSyncData syncDataSource)
        {
            syncData.GoogleCalendarEvent.Summary = syncDataSource.Subject;
            syncData.GoogleCalendarEvent.Description = syncDataSource.Body;
        }

        public void SetGoogleToken(IAuthorizationState token)
        {
            _token = token;
        }
        public Calendar GetDefaultCalendar(IAuthorizationState token, out bool isDefaultCalendarExist)
        {
            _logger.InfoFormat("GetDefaultCalendar method is entered, time:{0}", DateTime.Now);

            Calendar defaultCalendar = null;

            try
            {
                isDefaultCalendarExist = false;
                SetGoogleToken(token);
                _calendarService = _externalServiceProvider.GetGoogleCalendarService(_token);

                var listRequest = _calendarService.CalendarList.List();
                listRequest.MaxResults = GoogleSyncSettings.DefaultMaxCalendarCount;
                var calendars = listRequest.Fetch().Items;

                _logger.InfoFormat("----返回的CooperCalendar的总个数:{0}，明细如下：--------", calendars.Count());
                foreach (var calendar in calendars)
                {
                    _logger.InfoFormat("--------calendar summary:{0}, id:{1}", calendar.Summary, calendar.Id);
                }

                var defaultCalendars = calendars.Where(x => x.Summary == GoogleSyncSettings.DefaultCalendarName).ToList();
                var totalDefaultCalendarCount = defaultCalendars.Count();
                _logger.InfoFormat("----默认CooperCalendar的总个数:{0}----", totalDefaultCalendarCount);

                if (totalDefaultCalendarCount == 0)
                {
                    defaultCalendar = _calendarService.Calendars.Insert(new Calendar { Summary = GoogleSyncSettings.DefaultCalendarName }).Fetch();
                    _logger.Info("----默认CooperCalendar不存在，故一个默认的CooperCalendar已被创建。");
                }
                else
                {
                    //如果默认的日历多余1个，则删除多余的默认日历，只保留第一个默认日历
                    if (totalDefaultCalendarCount > 1)
                    {
                        _logger.Error("----开始删除多余的默认CooperCalendar");
                        int totalDeletedCalendarCount = 0;
                        var calendarIdList = defaultCalendars.Select(x => x.Id).ToList();
                        for (int index = 1; index < totalDefaultCalendarCount; index++)
                        {
                            //删除多余的默认日历
                            var calendar = defaultCalendars[index];
                            _calendarService.Calendars.Delete(calendarIdList[index]).Fetch();
                            _logger.InfoFormat("----删除了一个多余的默认CooperCalendar, title:{0}, id:{1}", calendar.Summary, calendar.Id);
                            totalDeletedCalendarCount++;
                        }

                        _logger.InfoFormat("----被删除的CooperCalendar总个数:{0}", totalDeletedCalendarCount);
                    }

                    defaultCalendar = _calendarService.Calendars.Get(defaultCalendars.First().Id).Fetch();
                    isDefaultCalendarExist = true;
                }
            }
            catch (Exception ex)
            {
                _logger.ErrorFormat("尝试获取默认日历时出现异常：", ex);
                throw;
            }

            _logger.InfoFormat("GetDefaultCalendar method is exited, time:{0}", DateTime.Now);

            return defaultCalendar;
        }
    }
}
