//Copyright (c) CodeSharp.  All rights reserved. - http://www.icodesharp.com/

using Cooper.Model.Accounts;

namespace Cooper.Model.AddressBooks
{
    /// <summary>个人通讯簿模型
    /// <remarks>
    /// 个人通讯簿管理与个人相关的联系人
    /// </remarks>
    /// </summary>
    public class PersonalAddressBook : AddressBook
    {
        protected PersonalAddressBook() : base() { }
        public PersonalAddressBook(string name, Account owner) : base(name)
        {
            Assert.IsValid(owner);
            this.OwnerAccountId = owner.ID;
        }

        /// <summary>通讯簿拥有者账号ID
        /// </summary>
        public int OwnerAccountId { get; private set; }
    }
}