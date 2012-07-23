namespace Cooper.Sync
{
    /// <summary>
    /// 描述外部同步数据类型的枚举
    /// </summary>
    public enum SyncDataType
    {
        /// <summary>
        /// Exchange任务
        /// </summary>
        ExchangeTask = 1,
        /// <summary>
        /// Exchange日历事件
        /// </summary>
        ExchangeCalendarEvent = 2,
        /// <summary>
        /// Exchange联系人
        /// </summary>
        ExchangeContact = 3,
        /// <summary>
        /// Google任务
        /// </summary>
        GoogleTask = 4,
        /// <summary>
        /// Google日历事件
        /// </summary>
        GoogleCalendarEvent = 5,
        /// <summary>
        /// Google联系人
        /// </summary>
        GoogleContact = 6,
        /// <summary>
        /// 阿里云日历事件
        /// </summary>
        AliyunCalendarEvent = 7,
        /// <summary>
        /// 阿里云邮箱联系人
        /// </summary>
        AliyunContact = 8,
    }
}