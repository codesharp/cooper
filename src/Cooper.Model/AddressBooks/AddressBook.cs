using System;
using CodeSharp.Core.DomainBase;

namespace Cooper.Model.AddressBooks
{
    /// <summary>通讯簿模型
    /// <remarks>
    /// 表示一个通用的通讯簿，支持父子通讯簿
    /// </remarks>
    /// </summary>
    public class AddressBook : EntityBase<int>, IAggregateRoot
    {
        public AddressBook()
        {
            this.CreateTime = this.LastUpdateTime = DateTime.Now;
        }

        /// <summary>通讯簿名称
        /// <remarks>
        /// 所有通讯簿名称不能重名
        /// </remarks>
        /// </summary>
        public virtual string Name { get; private set; }
        /// <summary>父通讯簿
        /// </summary>
        public virtual AddressBook Parent { get; private set; }
        /// <summary>创建时间
        /// </summary>
        public virtual DateTime CreateTime { get; private set; }
        /// <summary>最后更新时间
        /// </summary>
        public virtual DateTime LastUpdateTime { get; private set; }

        /// <summary>设置通讯簿名称
        /// <remarks>
        /// 长度应小于200
        /// </remarks>
        /// </summary>
        public virtual void SetName(string name)
        {
            Assert.IsNotNullOrWhiteSpace(name);
            Assert.LessOrEqual(name.Length, 200);

            if (this.Name != name)
            {
                this.Name = name;
                this.MakeChange();
            }
        }
        /// <summary>设置父通讯簿
        /// <remarks>
        /// 父通讯簿不能为自己，也不能将其某个子的通讯簿设置为其父通讯簿
        /// </remarks>
        /// </summary>
        public virtual void SetParent(AddressBook parent)
        {
            Assert.IsNotNull(parent);
            Assert.AreNotEqual(this.ID, parent.ID);

            var parentParent = parent.Parent;
            while (parentParent != null)
            {
                Assert.AreNotEqual(this.ID, parentParent.ID);
                parentParent = parentParent.Parent;
            }

            this.Parent = parent;
            this.MakeChange();
        }

        private void MakeChange()
        {
            this.LastUpdateTime = DateTime.Now;
        }
    }
    /// <summary>个人通讯簿模型
    /// <remarks>
    /// 个人通讯簿管理与个人相关的联系人
    /// </remarks>
    /// </summary>
    public class PersonalAddressBook : AddressBook
    {
        /// <summary>
        /// 通讯簿拥有者账号ID
        /// </summary>
        public virtual int OwnerAccountId { get; private set; }
    }
    /// <summary>系统通讯簿模型
    /// <remarks>
    /// 表示由系统进行管理的通讯簿，通常表示企业或组织内全局的联系人通讯簿，
    /// 如企业的全球通讯簿可以用系统通讯簿来实现
    /// </remarks>
    /// </summary>
    public class SystemAddressBook : AddressBook
    {
    }
}