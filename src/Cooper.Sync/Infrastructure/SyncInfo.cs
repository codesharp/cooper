namespace Cooper.Sync
{
    /// <summary>
    /// 同步信息，描述任务系统与某个外部同步数据之间的同步信息
    /// </summary>
    public class SyncInfo
    {
        /// <summary>
        /// 账号ID
        /// </summary>
        public int AccountId { get; set; }
        /// <summary>
        /// 本地数据的ID
        /// </summary>
        public string LocalDataId { get; set; }
        /// <summary>
        /// 外部同步数据的ID
        /// </summary>
        public string SyncDataId { get; set; }
        /// <summary>
        /// 外部同步数据的类型
        /// </summary>
        public int SyncDataType { get; set; }
    }
}