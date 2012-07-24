using System;
using CodeSharp.Core.DomainBase;
using Cooper.Model.Accounts;
using Cooper.Model.AddressBooks;
using Cooper.Model.ContactGroups;

namespace Cooper.Model.Contacts
{
    /// <summary>联系人模型
    /// <remarks>
    /// 描述通讯录中的一个联系人，联系人是任务协同的主要参与者；
    /// 目前仅描述联系人基本属性和最小关联属性，一个联系人必须属于一个通讯录
    /// </remarks>
    /// </summary>
    public class Contact : EntityBase<int>, IAggregateRoot
    {
        public Contact(AddressBook addressBook)
        {
            Assert.IsNotNull(addressBook);
            AddressBookId = addressBook.ID;
            this.CreateTime = this.LastUpdateTime = DateTime.Now;
        }

        /// <summary>
        /// 关联的账号，可以为空
        /// </summary>
        public virtual int? AccountId { get; private set; }
        /// <summary>
        /// 所属通讯簿ID
        /// </summary>
        public virtual int AddressBookId { get; private set; }
        /// <summary>
        /// 所属的联系人组ID
        /// </summary>
        public virtual int? GroupId { get; private set; }

        /// <summary>
        /// 全名
        /// </summary>
        public virtual string FullName { get; private set; }
        /// <summary>
        /// 邮箱
        /// </summary>
        public virtual string Email { get; private set; }
        /// <summary>
        /// 联系电话
        /// </summary>
        public virtual string Phone { get; private set; }
        /// <summary>
        /// 创建时间
        /// </summary>
        public virtual DateTime CreateTime { get; private set; }
        /// <summary>
        /// 最后更新时间
        /// </summary>
        public virtual DateTime LastUpdateTime { get; private set; }

        /// <summary>设置关联账号
        /// </summary>
        public virtual void SetAccount(Account account)
        {
            Assert.IsNotNull(account);
            Assert.Greater(account.ID, 0);

            if (this.AccountId != null && this.AccountId.Value == account.ID)
            {
                return;
            }

            this.AccountId = account.ID;
            this.MakeChange();
        }
        /// <summary>设置关联的联系人组
        /// </summary>
        public virtual void SetGroup(ContactGroup group)
        {
            Assert.IsNotNull(group);
            Assert.Greater(group.ID, 0);

            if (this.GroupId != null && this.GroupId.Value == group.ID)
            {
                return;
            }

            this.GroupId = group.ID;
            this.MakeChange();
        }
        /// <summary>设置全名
        /// <remarks>
        /// 长度应小于200
        /// </remarks>
        /// </summary>
        public virtual void SetFullName(string fullName)
        {
            Assert.IsNotNullOrWhiteSpace(fullName);
            Assert.LessOrEqual(fullName.Length, 200);

            if (this.FullName != fullName)
            {
                this.FullName = fullName;
                this.MakeChange();
            }
        }
        /// <summary>设置邮箱
        /// </summary>
        public virtual void SetEmail(string email)
        {
            Assert.IsNotNullOrWhiteSpace(email);

            if (this.Email != email)
            {
                this.Email = email;
                this.MakeChange();
            }
        }
        /// <summary>设置联系电话
        /// </summary>
        public virtual void SetPhone(string phone)
        {
            if (this.Phone != phone)
            {
                this.Phone = phone;
                this.MakeChange();
            }
        }
        //UNDONE:允许外部直接更改LastUpdateTime可能对后期业务设计带来隐患，
        //目前需要这样做是因为与外部系统同步联系人的需要
        /// <summary>设置最后更新时间
        /// </summary>
        public virtual void SetLastUpdateTime(DateTime lastUpdateTime)
        {
            this.LastUpdateTime = lastUpdateTime;
        }

        private void MakeChange()
        {
            this.LastUpdateTime = DateTime.Now;
        }
    }
}