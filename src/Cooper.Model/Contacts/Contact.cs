//Copyright (c) CodeSharp.  All rights reserved. - http://www.icodesharp.com/

using System;
using CodeSharp.Core.DomainBase;
using Cooper.Model.Accounts;
using Cooper.Model.Contacts;

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
        protected Contact() { this.CreateTime = this.LastUpdateTime = DateTime.Now; }
        public Contact(AddressBook addressBook, string fullName, string email) : this()
        {
            Assert.IsNotNull(addressBook);

            this.AddressBookId = addressBook.ID;
            this.SetFullName(fullName);
            this.SetEmail(email);
        }

        /// <summary>关联的账号，可以为空
        /// </summary>
        public int? AccountId { get; private set; }
        /// <summary>所属通讯簿ID
        /// </summary>
        public int AddressBookId { get; private set; }
        /// <summary>全名
        /// </summary>
        public string FullName { get; private set; }
        /// <summary>邮箱
        /// </summary>
        public string Email { get; private set; }
        /// <summary>联系电话
        /// </summary>
        public string Phone { get; private set; }
        /// <summary>创建时间
        /// </summary>
        public DateTime CreateTime { get; private set; }
        /// <summary>最后更新时间
        /// </summary>
        public DateTime LastUpdateTime { get; private set; }

        /// <summary>设置关联账号
        /// </summary>
        public void SetAccount(Account account)
        {
            Assert.IsNotNull(account);

            if (this.AccountId != null && this.AccountId.Value == account.ID) return;
            this.AccountId = account.ID;
            this.MakeChange();
        }
        /// <summary>设置全名
        /// <remarks>
        /// 长度应小于200
        /// </remarks>
        /// </summary>
        public void SetFullName(string fullName)
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
        /// <remarks>
        /// 长度应小于100
        /// </remarks>
        /// </summary>
        public void SetEmail(string email)
        {
            Assert.IsNotNullOrWhiteSpace(email);
            Assert.LessOrEqual(email.Length, 100);

            if (this.Email != email)
            {
                this.Email = email;
                this.MakeChange();
            }
        }
        /// <summary>设置联系电话
        /// <remarks>
        /// 长度应小于100
        /// </remarks>
        /// </summary>
        public void SetPhone(string phone)
        {
            if (phone != null)
            {
                Assert.LessOrEqual(phone.Length, 100);
            }

            if (this.Phone != phone)
            {
                this.Phone = phone;
                this.MakeChange();
            }
        }

        private void MakeChange()
        {
            this.LastUpdateTime = DateTime.Now;
        }
    }
}