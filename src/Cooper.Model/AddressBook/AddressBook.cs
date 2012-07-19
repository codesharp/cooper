using Taobao.Infrastructure.DomainBase;

namespace Taobao.Cooper.Model
{
    /// <summary>
    /// 通讯簿
    /// </summary>
    public class AddressBook : EntityBase<int>, IAggregateRoot
    {
        /// <summary>
        /// 父通讯簿ID
        /// </summary>
        public int? ParentId { get; set; }
        /// <summary>
        /// 通讯簿名称
        /// </summary>
        public string Name { get; set; }
    }
}