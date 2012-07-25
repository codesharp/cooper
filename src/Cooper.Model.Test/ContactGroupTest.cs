//Copyright (c) CodeSharp.  All rights reserved. - http://www.icodesharp.com/

using System;
using System.Linq;
using Cooper.Model.AddressBooks;
using Cooper.Model.ContactGroups;
using Cooper.Model.Contacts;
using NUnit.Framework;

namespace Cooper.Model.Test
{
    [TestFixture]
    [Microsoft.VisualStudio.TestTools.UnitTesting.TestClass]
    public class ContactGroupTest : TestBase
    {
        [Test]
        [Microsoft.VisualStudio.TestTools.UnitTesting.TestMethod]
        public void TotalTest()
        {
            CreateContactGroup();
            UpdateContactGroup();
            DeleteContactGroup();
            GetContactGroups();
        }

        [Test]
        [Microsoft.VisualStudio.TestTools.UnitTesting.TestMethod]
        public void CreateContactGroup()
        {
            var personalAddressBook = this.CreatePersonalAddressBook();
            var contactGroup = new ContactGroup(personalAddressBook, "Contact Group1");

            Assert.AreNotEqual(DateTime.MinValue, contactGroup.CreateTime);

            this._contactGroupService.Create(contactGroup);
            Assert.Greater(contactGroup.ID, 0);

            this.Evict(contactGroup);

            var contactGroup2 = this._contactGroupService.GetContactGroup(contactGroup.ID) as ContactGroup;

            Assert.AreEqual(contactGroup.Name, contactGroup2.Name);
            Assert.AreEqual(contactGroup.AddressBookId, personalAddressBook.ID);
            Assert.AreEqual(FormatTime(contactGroup.CreateTime), FormatTime(contactGroup2.CreateTime));
        }
        [Test]
        [Microsoft.VisualStudio.TestTools.UnitTesting.TestMethod]
        public void UpdateContactGroup()
        {
            var personalAddressBook = this.CreatePersonalAddressBook();
            var contactGroup = new ContactGroup(personalAddressBook, "Contact Group1");

            Assert.AreNotEqual(DateTime.MinValue, contactGroup.CreateTime);

            this._contactGroupService.Create(contactGroup);
            Assert.Greater(contactGroup.ID, 0);

            this.Evict(contactGroup);

            var contactGroup2 = this._contactGroupService.GetContactGroup(contactGroup.ID) as ContactGroup;

            Assert.AreEqual(contactGroup.Name, contactGroup2.Name);
            Assert.AreEqual(contactGroup.AddressBookId, personalAddressBook.ID);
            Assert.AreEqual(FormatTime(contactGroup.CreateTime), FormatTime(contactGroup2.CreateTime));

            contactGroup2.SetName(contactGroup.Name + "_updated");

            this._contactGroupService.Update(contactGroup2);
            this.Evict(contactGroup2);
            var contactGroup3 = this._contactGroupService.GetContactGroup(contactGroup2.ID) as ContactGroup;

            Assert.AreEqual(contactGroup2.Name, contactGroup3.Name);

            var contact1 = CreateContact(personalAddressBook);
            var contact2 = CreateContact(personalAddressBook);
            contactGroup3.AddContact(contact1);
            contactGroup3.AddContact(contact2);

            this._contactGroupService.Update(contactGroup3);
            this.Evict(contactGroup3);
            var contactGroup4 = this._contactGroupService.GetContactGroup(contactGroup3.ID) as ContactGroup;
            Assert.AreEqual(2, contactGroup4.Contacts.Count());

            contactGroup4.RemoveContact(contact2);

            this._contactGroupService.Update(contactGroup4);
            this.Evict(contactGroup4);
            var contactGroup5 = this._contactGroupService.GetContactGroup(contactGroup4.ID) as ContactGroup;
            Assert.AreEqual(1, contactGroup5.Contacts.Count());
        }
        [Test]
        [Microsoft.VisualStudio.TestTools.UnitTesting.TestMethod]
        public void DeleteContactGroup()
        {
            var personalAddressBook = this.CreatePersonalAddressBook();
            var contactGroup = new ContactGroup(personalAddressBook, "Contact Group1");

            Assert.AreNotEqual(DateTime.MinValue, contactGroup.CreateTime);

            this._contactGroupService.Create(contactGroup);
            Assert.Greater(contactGroup.ID, 0);

            this.Evict(contactGroup);

            var contactGroup2 = this._contactGroupService.GetContactGroup(contactGroup.ID) as ContactGroup;

            this._contactGroupService.Delete(contactGroup2);

            this.Evict(contactGroup2);

            var contactGroup3 = this._contactGroupService.GetContactGroup(contactGroup2.ID) as ContactGroup;

            Assert.IsNull(contactGroup3);
        }
        [Test]
        [Microsoft.VisualStudio.TestTools.UnitTesting.TestMethod]
        public void GetContactGroups()
        {
            var personalAddressBook = this.CreatePersonalAddressBook();
            var contactGroup1 = new ContactGroup(personalAddressBook, "Contact Group1");
            var contactGroup2 = new ContactGroup(personalAddressBook, "Contact Group1");
            var contactGroup3 = new ContactGroup(personalAddressBook, "Contact Group1");

            this._contactGroupService.Create(contactGroup1);
            this._contactGroupService.Create(contactGroup2);
            this._contactGroupService.Create(contactGroup3);

            this.Evict(contactGroup1);
            this.Evict(contactGroup2);
            this.Evict(contactGroup3);

            var contactGroups = this._contactGroupService.GetContactGroups(personalAddressBook);

            Assert.AreEqual(3, contactGroups.Count());
        }

        private PersonalAddressBook CreatePersonalAddressBook()
        {
            var account = this.CreateAccount();
            var addressBook = new PersonalAddressBook("我的通讯簿", account);
            this._addressBookService.Create(addressBook);
            return addressBook;
        }
        private Contact CreateContact(AddressBook addressBook)
        {
            var contact = new Contact(addressBook, "hewang.txh", "hewang.txh@taobao.com");
            this._contactService.Create(contact);
            return contact;
        }
    }
}