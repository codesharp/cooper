using System.Collections.Generic;
using System.Linq;

namespace Cooper.Sync
{
    /// <summary>
    /// 同步结果接口定义
    /// </summary>
    /// <typeparam name="TLocalData">同步的本地数据的类型</typeparam>
    public interface ISyncResult<TLocalData> where TLocalData : class, ISyncData
    {
        IList<TLocalData> LocalDatasToCreate { get; }
        IList<TLocalData> LocalDatasToUpdate { get; }
        IList<TLocalData> LocalDatasToDelete { get; }

        IList<ISyncData> SyncDatasToCreate { get; }
        IList<ISyncData> SyncDatasToUpdate { get; }
        IList<ISyncData> SyncDatasToDelete { get; }

        ISyncResult<TLocalData> MergeResult(ISyncResult<TLocalData> result);
    }
    /// <summary>
    /// 同步结果实现类
    /// </summary>
    public class SyncResult<TLocalData> : ISyncResult<TLocalData> where TLocalData : class, ISyncData
    {
        public SyncResult()
        {
            LocalDatasToCreate = new List<TLocalData>();
            LocalDatasToUpdate = new List<TLocalData>();
            LocalDatasToDelete = new List<TLocalData>();

            SyncDatasToCreate = new List<ISyncData>();
            SyncDatasToUpdate = new List<ISyncData>();
            SyncDatasToDelete = new List<ISyncData>();
        }

        /// <summary>
        /// 本地需要新增的数据
        /// </summary>
        public IList<TLocalData> LocalDatasToCreate { get; private set; }
        /// <summary>
        /// 本地需要更新的数据
        /// </summary>
        public IList<TLocalData> LocalDatasToUpdate { get; private set; }
        /// <summary>
        /// 本地需要删除的数据
        /// </summary>
        public IList<TLocalData> LocalDatasToDelete { get; private set; }

        /// <summary>
        /// 同步数据源需要新增的数据，因为数据类型可能有很多中，所以只能用接口表示
        /// </summary>
        public IList<ISyncData> SyncDatasToCreate { get; private set; }
        /// <summary>
        /// 同步数据源需要更新的数据，因为数据类型可能有很多中，所以只能用接口表示
        /// </summary>
        public IList<ISyncData> SyncDatasToUpdate { get; private set; }
        /// <summary>
        /// 同步数据源需要删除的数据，因为数据类型可能有很多中，所以只能用接口表示
        /// </summary>
        public IList<ISyncData> SyncDatasToDelete { get; private set; }

        /// <summary>
        /// 合并一个指定的同步结果
        /// </summary>
        public ISyncResult<TLocalData> MergeResult(ISyncResult<TLocalData> result)
        {
            if (result == null)
            {
                return this;
            }
            foreach (var localData in result.LocalDatasToCreate)
            {
                LocalDatasToCreate.Add(localData);
            }
            foreach (var localData in result.LocalDatasToUpdate)
            {
                var existLocalData = LocalDatasToUpdate.SingleOrDefault(x => x.Id == localData.Id);
                if (existLocalData != null)
                {
                    if (localData.LastUpdateLocalTime > existLocalData.LastUpdateLocalTime)
                    {
                        LocalDatasToUpdate.Remove(existLocalData);
                        LocalDatasToUpdate.Add(localData);
                    }
                }
                else
                {
                    LocalDatasToUpdate.Add(localData);
                }
            }
            foreach (var localData in result.LocalDatasToDelete)
            {
                if (!LocalDatasToDelete.Any(x => x.Id == localData.Id))
                {
                    LocalDatasToDelete.Add(localData);
                }
            }

            foreach (var syncData in result.SyncDatasToCreate)
            {
                SyncDatasToCreate.Add(syncData);
            }
            foreach (var syncData in result.SyncDatasToUpdate)
            {
                var existSyncData = SyncDatasToUpdate.SingleOrDefault(x => x.GetType() == syncData.GetType() && x.Id == syncData.Id);
                if (existSyncData != null)
                {
                    if (syncData.LastUpdateLocalTime > existSyncData.LastUpdateLocalTime)
                    {
                        SyncDatasToUpdate.Remove(existSyncData);
                        SyncDatasToUpdate.Add(syncData);
                    }
                }
                else
                {
                    SyncDatasToUpdate.Add(syncData);
                }
            }
            foreach (var syncData in result.SyncDatasToDelete)
            {
                if (!SyncDatasToDelete.Any(x => x.GetType() == syncData.GetType() && x.Id == syncData.Id))
                {
                    SyncDatasToDelete.Add(syncData);
                }
            }

            return this;
        }
    }
}
