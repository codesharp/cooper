using System.Collections.Generic;

namespace Cooper.Sync
{
    /// <summary>
    /// 一个接口，用于定义某个TSyncData与外部数据源TSyncDataSource进行同步时的相关原子操作
    /// </summary>
    public interface ISyncDataService<TSyncData, TSyncDataSource>
        where TSyncData : ISyncData
        where TSyncDataSource : ISyncData
    {
        IList<TSyncData> GetSyncDataList();
        TSyncData CreateFrom(TSyncDataSource syncDataSource);
        void UpdateSyncData(TSyncData syncData, TSyncDataSource syncDataSource);
    }
}
