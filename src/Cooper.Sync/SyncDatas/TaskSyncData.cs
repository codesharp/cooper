using System;

namespace Cooper.Sync
{
    /// <summary>
    /// 表示Cooper任务与外部系统同步数据时的数据对象
    /// </summary>
    public class TaskSyncData : ISyncData
    {
        /// <summary>
        /// 唯一标识
        /// </summary>
        public string Id { get; set; }
        /// <summary>获取标题/主题
        /// </summary>
        public string Subject { get; set; }
        /// <summary>获取内容/描述
        /// </summary>
        public string Body { get; set; }
        /// <summary>获取截止时间
        /// </summary>
        public DateTime? DueTime { get; set; }
        /// <summary>获取创建时间
        /// </summary>
        public DateTime CreateTime { get; set; }
        /// <summary>获取最后更新时间
        /// </summary>
        public DateTime LastUpdateLocalTime { get; set; }
        /// <summary>是否完成
        /// </summary>
        public bool IsCompleted { get; set; }
        /// <summary>获取基本优先级
        /// </summary>
        public TaskSyncDataPriority Priority { get; set; }

        public string SyncId { get; set; }
        public int SyncType { get; set; }
        public bool IsFromDefault { get; set; }
    }
    /// <summary>任务基本优先级，描述大概过多久时间开始执行任务
    /// <remarks>与DueTime没有直接联系</remarks>
    /// </summary>
    public enum TaskSyncDataPriority
    {
        /// <summary>0 今天，马上
        /// </summary>
        Today = 0,
        /// <summary>1 尽快
        /// </summary>
        Upcoming = 1,
        /// <summary>2 迟些再说
        /// </summary>
        Later = 2
    }
}