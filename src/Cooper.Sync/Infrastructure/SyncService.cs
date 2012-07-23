using System;
using System.Collections.Generic;
using System.Linq;
using CodeSharp.Core;
using CodeSharp.Core.Services;

namespace Cooper.Sync
{
    /// <summary>
    /// 通用数据同步服务实现类，实现本地数据与外部系统数据之间的双向同步的过程逻辑
    /// </summary>
    public abstract class SyncService<TLocalData, TSyncData, TSyncResult> : ISyncService<TLocalData, TSyncData, TSyncResult>
        where TLocalData : class, ISyncData
        where TSyncData : ISyncData
        where TSyncResult : class, ISyncResult<TLocalData>, new()
    {
        #region Private Variables

        private ISyncDataService<TLocalData, TSyncData> _localDataService;
        private ISyncDataService<TSyncData, TLocalData> _syncDataService;
        private ILog _logger;

        #endregion

        public SyncService(ISyncDataService<TLocalData, TSyncData> localDataService, ISyncDataService<TSyncData, TLocalData> syncDataService)
        {
            _localDataService = localDataService;
            _syncDataService = syncDataService;
            AllowAutoCreateSyncInfo = false;
            _logger = DependencyResolver.Resolve<ILoggerFactory>().Create(GetType());
        }

        protected abstract int GetSyncDataType();
        protected bool AllowAutoCreateSyncInfo { get; set; }

        /// <summary>
        /// 单向数据同步，将外部数据导入到任务系统
        /// </summary>
        public IEnumerable<TLocalData> GetDataFromSyncSource()
        {
            List<TLocalData> localDataList = new List<TLocalData>();
            IList<TSyncData> syncDataList = _syncDataService.GetSyncDataList();

            foreach (var syncData in syncDataList)
            {
                var localData = _localDataService.CreateFrom(syncData);
                localDataList.Add(localData);
            }

            return localDataList;
        }
        /// <summary>
        /// 双向数据同步
        /// </summary>
        public TSyncResult ProcessTwoWaySynchronization(IEnumerable<TLocalData> localDataList, IEnumerable<SyncInfo> syncInfos)
        {
            TSyncResult syncResult = new TSyncResult();
            int syncDataType = GetSyncDataType();
            IList<TSyncData> syncDataList = _syncDataService.GetSyncDataList();

            //先做日志记录，方便查找问题
            _logger.InfoFormat("----外部同步数据, 总条数:{0}, 数据类型:{1}, 明细如下:", syncDataList.Count(), typeof(TSyncData).FullName);
            foreach (var syncData in syncDataList)
            {
                LogSyncData(syncData);
            }

            //1. 以TLocalData为出发点，同步TSyncData
            foreach (var localData in localDataList)
            {
                IEnumerable<SyncInfo> filterSyncInfos = syncInfos.Where(x => x.LocalDataId == localData.Id && x.SyncDataType == syncDataType);

                if (filterSyncInfos.Count() == 0)
                {
                    if (AllowAutoCreateSyncInfo)
                    {
                        //新增TSyncData
                        var syncData = _syncDataService.CreateFrom(localData);
                        syncData.SyncId = localData.Id;
                        syncData.SyncType = syncDataType;
                        syncResult.SyncDatasToCreate.Add(syncData);
                    }
                }
                else
                {
                    foreach (var syncInfo in filterSyncInfos)
                    {
                        var syncData = syncDataList.SingleOrDefault(x => x.Id == syncInfo.SyncDataId);
                        if (syncData == null)
                        {
                            //删除localData
                            localData.SyncId = syncInfo.SyncDataId;
                            localData.SyncType = syncDataType;
                            syncResult.LocalDatasToDelete.Add(localData);
                        }
                        else if (syncData.IsFromDefault)
                        {
                            //比较最后更新时间，用晚更新的数据覆盖早更新的数据，比较时只精确到秒
                            DateTime localDataLastUpdateLocalTime =
                                new DateTime(
                                    localData.LastUpdateLocalTime.Year,
                                    localData.LastUpdateLocalTime.Month,
                                    localData.LastUpdateLocalTime.Day,
                                    localData.LastUpdateLocalTime.Hour,
                                    localData.LastUpdateLocalTime.Minute,
                                    localData.LastUpdateLocalTime.Second
                                    );
                            DateTime syncDataLastUpdateLocalTime =
                                new DateTime(
                                    syncData.LastUpdateLocalTime.Year,
                                    syncData.LastUpdateLocalTime.Month,
                                    syncData.LastUpdateLocalTime.Day,
                                    syncData.LastUpdateLocalTime.Hour,
                                    syncData.LastUpdateLocalTime.Minute,
                                    syncData.LastUpdateLocalTime.Second
                                    );

                            if (localDataLastUpdateLocalTime > syncDataLastUpdateLocalTime)
                            {
                                //更新TSyncData
                                _syncDataService.UpdateSyncData(syncData, localData);
                                syncData.SyncId = localData.Id;
                                syncResult.SyncDatasToUpdate.Add(syncData);
                            }
                            else if (localDataLastUpdateLocalTime < syncDataLastUpdateLocalTime)
                            {
                                //更新TLocalData
                                _localDataService.UpdateSyncData(localData, syncData);
                                localData.SyncId = syncData.Id;
                                localData.LastUpdateLocalTime = syncData.LastUpdateLocalTime;
                                syncResult.LocalDatasToUpdate.Add(localData);
                            }
                        }
                    }
                }
            }

            //2. 以TSyncData为出发点，同步TLocalData
            foreach (var syncData in syncDataList)
            {
                //先获取同步信息
                IEnumerable<SyncInfo> filterSyncInfos = syncInfos.Where(x => x.SyncDataId == syncData.Id && x.SyncDataType == syncDataType);

                if (filterSyncInfos.Count() == 0)
                {
                    //新增TLocalData
                    var localData = _localDataService.CreateFrom(syncData);
                    localData.SyncId = syncData.Id;
                    localData.SyncType = syncDataType;
                    localData.LastUpdateLocalTime = syncData.LastUpdateLocalTime;
                    localData.IsFromDefault = syncData.IsFromDefault;
                    syncResult.LocalDatasToCreate.Add(localData);
                }
                else
                {
                    foreach (var syncInfo in filterSyncInfos)
                    {
                        var localData = localDataList.SingleOrDefault(x => x.Id == syncInfo.LocalDataId);
                        if (localData == null)
                        {
                            //删除TSyncData
                            syncData.SyncId = syncInfo.LocalDataId;
                            syncData.SyncType = syncDataType;
                            syncResult.SyncDatasToDelete.Add(syncData);
                        }
                    }
                }
            }

            return syncResult;
        }

        private void LogSyncData(ISyncData syncData)
        {
            if (syncData != null)
            {
                _logger.InfoFormat("--------Data Type:{0}, Id:{1}, Subject:{2}, SyncId:{3}, SyncType:{4}, IsFromDefault:{5}, LastUpdateLocalTime:{6}",
                    syncData.GetType().Name,
                    syncData.Id,
                    syncData.Subject,
                    syncData.SyncId,
                    syncData.SyncType,
                    syncData.IsFromDefault,
                    syncData.LastUpdateLocalTime);
            }
        }
    }
}