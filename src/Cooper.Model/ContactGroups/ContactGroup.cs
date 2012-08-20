//Copyright (c) CodeSharp.  All rights reserved. - http://www.icodesharp.com/

using System;
using System.Collections.Generic;
using System.Linq;
using CodeSharp.Core.DomainBase;
using Cooper.Model.Contacts;

namespace Cooper.Model.Contacts
{
    /// <summary>联系人组
    /// <remarks>
    /// 一个联系人组用于对一个通讯簿中的所有联系人进行分组；
    /// 联系人组只在显示联系人时使用，联系人本身不必关心联系人组的存在，
    /// 所以，由联系人组来维护其所关联的联系人，联系人与组的关系是多对多；
    /// </remarks>
    /// </summary>
    public class ContactGroup : EntityBase<int>, IAggregateRoot
    {
        private IList<Contact> _contacts = new List<Contact>();

        protected ContactGroup() { this.CreateTime = DateTime.Now; }
        public ContactGroup(AddressBook addressBook, string name) : this()
        {
            Assert.IsNotNull(addressBook);

            this.AddressBookId = addressBook.ID;
            this.SetName(name);
        }

        /// <summary>名称，不能重名
        /// </summary>
        public string Name { get; private set; }
        /// <summary>所属通讯簿ID
        /// </summary>
        public int AddressBookId { get; private set; }
        /// <summary>关联的联系人
        /// </summary>
        public IEnumerable<Contact> Contacts { get { return _contacts; } }
        /// <summary>创建时间
        /// </summary>
        public DateTime CreateTime { get; private set; }

        /// <summary>设置名称
        /// <remarks>
        /// 长度应小于255
        /// </remarks>
        /// </summary>
        public void SetName(string name)
        {
            Assert.IsValidKey(name);

            if (this.Name != name)
            {
                this.Name = name;
            }
        }
        /// <summary>将指定联系人添加到组
        /// </summary>
        public void AddContact(Contact contact)
        {
            Assert.IsNotNull(contact);
            Assert.AreEqual(this.AddressBookId, contact.AddressBookId);

            if (!_contacts.Any(x => x.ID == contact.ID))
            {
                _contacts.Add(contact);
            }
        }
        /// <summary>将指定联系人从组移除
        /// </summary>
        public void RemoveContact(Contact contact)
        {
            Assert.IsNotNull(contact);

            var contactToRemove = _contacts.SingleOrDefault(x => x.ID == contact.ID);
            if (contactToRemove != null)
            {
                _contacts.Remove(contactToRemove);
            }
        }
    }
}