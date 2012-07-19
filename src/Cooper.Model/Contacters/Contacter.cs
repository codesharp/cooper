using System;
using Taobao.Infrastructure.DomainBase;

namespace Taobao.Cooper.Model
{
    /// <summary>
    /// 联系人
    /// </summary>
    public class Contacter : EntityBase<int>, IAggregateRoot
    {
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
        public DateTime LastUpdateTime { get; set; }
    }
}