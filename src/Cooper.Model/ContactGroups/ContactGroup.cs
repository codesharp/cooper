using System;
using CodeSharp.Core.DomainBase;
using Cooper.Model.AddressBooks;

namespace Cooper.Model.ContactGroups
{
    /// <summary>联系人组
    /// <remarks>
    /// 一个联系人组用于对一个通讯簿中的所有联系人进行分组
    /// </remarks>
    /// </summary>
    public class ContactGroup : EntityBase<int>, IAggregateRoot
    {
        public ContactGroup(AddressBook addressBook)
        {
            Assert.IsNotNull(addressBook);
            AddressBookId = addressBook.ID;
            this.CreateTime = this.LastUpdateTime = DateTime.Now;
        }

        /// <summary>名称，不能重名
        /// </summary>
        public virtual string Name { get; private set; }
        /// <summary>所属通讯簿ID
        /// </summary>
        public virtual int AddressBookId { get; private set; }
        /// <summary>创建时间
        /// </summary>
        public virtual DateTime CreateTime { get; private set; }
        /// <summary>最后更新时间
        /// </summary>
        public virtual DateTime LastUpdateTime { get; private set; }

        /// <summary>设置名称
        /// <remarks>
        /// 长度应小于100
        /// </remarks>
        /// </summary>
        public virtual void SetName(string name)
        {
            Assert.IsNotNullOrWhiteSpace(name);
            Assert.LessOrEqual(name.Length, 100);

            if (this.Name != name)
            {
                this.Name = name;
                this.MakeChange();
            }
        }

        private void MakeChange()
        {
            this.LastUpdateTime = DateTime.Now;
        }
    }
}