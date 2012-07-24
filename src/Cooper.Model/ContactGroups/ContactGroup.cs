﻿//Copyright (c) CodeSharp.  All rights reserved. - http://www.icodesharp.com/

using System;
using System.Collections.Generic;
using CodeSharp.Core.DomainBase;
using Cooper.Model.AddressBooks;
using Cooper.Model.Contacts;

namespace Cooper.Model.ContactGroups
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
        /// <summary>联系人组关联的联系人ID的集合，用一个拼接字符串表示，以分号分隔
        /// </summary>
        public string ContactIds { get; private set; }
        /// <summary>创建时间
        /// </summary>
        public DateTime CreateTime { get; private set; }

        /// <summary>设置名称
        /// <remarks>
        /// 长度应小于100
        /// </remarks>
        /// </summary>
        public void SetName(string name)
        {
            Assert.IsNotNullOrWhiteSpace(name);
            Assert.LessOrEqual(name.Length, 100);

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
            string contactId = contact.ID.ToString();

            List<string> contactIdList = new List<string>();

            if (!string.IsNullOrWhiteSpace(this.ContactIds))
            {
                contactIdList.AddRange(this.ContactIds.Split(';'));
            }

            if (!contactIdList.Contains(contactId))
            {
                contactIdList.Add(contactId);
                this.ContactIds = string.Join(";", contactIdList.ToArray());
            }
        }
        /// <summary>将指定联系人从组移除
        /// </summary>
        public void RemoveContact(Contact contact)
        {
            Assert.IsNotNull(contact);
            string contactId = contact.ID.ToString();

            List<string> contactIdList = new List<string>();

            if (!string.IsNullOrWhiteSpace(this.ContactIds))
            {
                contactIdList.AddRange(this.ContactIds.Split(';'));
            }

            if (contactIdList.Contains(contactId))
            {
                contactIdList.Remove(contactId);
                this.ContactIds = string.Join(";", contactIdList.ToArray());
            }
        }
    }
}