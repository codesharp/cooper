using System;

namespace Cooper.Sync
{
    /// <summary>
    /// 表示Cooper联系人与外部系统同步数据时的数据对象
    /// </summary>
    public class ContactSyncData : ISyncData
    {
        /// <summary>
        /// 唯一标识
        /// </summary>
        public string Id { get; set; }
        /// <summary>
        /// 联系人对应的帐号，可以为空
        /// </summary>
        public int? AccountId { get; set; }
        /// <summary>
        /// 联系人所属通讯簿ID
        /// </summary>
        public int AddressBookId { get; set; }
        /// <summary>
        /// 联系人全名称
        /// </summary>
        public string FullName { get; set; }
        /// <summary>
        /// 联系人Email
        /// </summary>
        public string Email { get; set; }
        /// <summary>
        /// 联系人电话
        /// </summary>
        public string Phone { get; set; }
        /// <summary>
        /// 创建时间
        /// </summary>
        public DateTime CreateTime { get; set; }
        /// <summary>
        /// 最后更新时间
        /// </summary>
        public DateTime LastUpdateLocalTime { get; set; }

        public string SyncId { get; set; }
        public int SyncType { get; set; }
        public bool IsFromDefault { get; set; }

        public string Subject
        {
            get { return FullName; }
        }
    }
}