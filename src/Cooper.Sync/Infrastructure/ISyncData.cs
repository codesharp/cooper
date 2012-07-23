using System;

namespace Cooper.Sync
{
    /// <summary>
    /// 表示某个被同步的数据
    /// </summary>
    public interface ISyncData
    {
        string Id { get; }
        string Subject { get; }
        string SyncId { get; set; }
        int SyncType { get; set; }
        bool IsFromDefault { get; set; }
        DateTime LastUpdateLocalTime { get; set; }
    }
}
