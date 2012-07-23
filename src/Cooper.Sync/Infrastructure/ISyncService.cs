using System.Collections.Generic;
namespace Cooper.Sync
{
    /// <summary>
    /// 通用数据同步服务接口定义
    /// </summary>
    public interface ISyncService<TLocalData, out TSyncData, out TSyncResult>
        where TLocalData : class, ISyncData
        where TSyncData : ISyncData
        where TSyncResult : class, ISyncResult<TLocalData>, new()
    {
        /// <summary>
        /// 从外部数据源获取需要导入的数据
        /// </summary>
        IEnumerable<TLocalData> GetDataFromSyncSource();
        /// <summary>
        /// 双向同步数据，返回同步后的结果
        /// </summary>
        TSyncResult ProcessTwoWaySynchronization(IEnumerable<TLocalData> localDataList, IEnumerable<SyncInfo> syncInfos);
    }
}