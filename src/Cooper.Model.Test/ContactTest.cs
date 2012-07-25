//Copyright (c) CodeSharp.  All rights reserved. - http://www.icodesharp.com/

using System;
using System.Linq;
using Cooper.Model.AddressBooks;
using Cooper.Model.Contacts;
using NUnit.Framework;

namespace Cooper.Model.Test
{
    [TestFixture]
    [Microsoft.VisualStudio.TestTools.UnitTesting.TestClass]
    public class ContactTest : TestBase
    {
        [Test]
        [Microsoft.VisualStudio.TestTools.UnitTesting.TestMethod]
        public void TotalTest()
        {
            CreateContact();
            UpdateContact();
            DeleteContact();
            GetContacts();
        }

        [Test]
        [Microsoft.VisualStudio.TestTools.UnitTesting.TestMethod]
        public void CreateContact()
        {
            var personalAddressBook = this.CreatePersonalAddressBook();
            var contact = new Contact(personalAddressBook, "hewang.txh", "hewang.txh@taobao.com");

            contact.SetPhone("+8613777891550");

            Assert.AreNotEqual(DateTime.MinValue, contact.CreateTime);

            this._contactService.Create(contact);
            Assert.Greater(contact.ID, 0);

            this.Evict(contact);

            var contact2 = this._contactService.GetContact(contact.ID) as Contact;

            Assert.AreEqual(contact.FullName, contact2.FullName);
            Assert.AreEqual(contact.Email, contact2.Email);
            Assert.AreEqual(contact.Phone, contact2.Phone);
            Assert.AreEqual(FormatTime(contact.CreateTime), FormatTime(contact2.CreateTime));
        }
        [Test]
        [Microsoft.VisualStudio.TestTools.UnitTesting.TestMethod]
        public void UpdateContact()
        {
            var personalAddressBook = this.CreatePersonalAddressBook();
            var contact = new Contact(personalAddressBook, "hewang.txh", "hewang.txh@taobao.com");

            contact.SetPhone("+8613777891550");

            Assert.AreNotEqual(DateTime.MinValue, contact.CreateTime);

            this._contactService.Create(contact);
            Assert.Greater(contact.ID, 0);

            this.Evict(contact);

            var contact2 = this._contactService.GetContact(contact.ID) as Contact;

            contact2.SetFullName(contact.FullName + "_updated");
            contact2.SetEmail(contact.Email + "_updated");
            contact2.SetPhone(contact.Phone + "_updated");

            this._contactService.Update(contact2);

            this.Evict(contact2);

            var contact3 = this._contactService.GetContact(contact2.ID) as Contact;

            Assert.AreEqual(contact2.FullName, contact3.FullName);
            Assert.AreEqual(contact2.Email, contact3.Email);
            Assert.AreEqual(contact2.Phone, contact3.Phone);
        }
        [Test]
        [Microsoft.VisualStudio.TestTools.UnitTesting.TestMethod]
        public void DeleteContact()
        {
            var personalAddressBook = this.CreatePersonalAddressBook();
            var contact = new Contact(personalAddressBook, "hewang.txh", "hewang.txh@taobao.com");

            this._contactService.Create(contact);
            this.Evict(contact);

            var contact2 = this._contactService.GetContact(contact.ID) as Contact;

            this._contactService.Delete(contact2);

            this.Evict(contact2);

            var contact3 = this._contactService.GetContact(contact2.ID) as Contact;

            Assert.IsNull(contact3);
        }
        [Test]
        [Microsoft.VisualStudio.TestTools.UnitTesting.TestMethod]
        public void GetContacts()
        {
            var personalAddressBook = this.CreatePersonalAddressBook();
            var contact1 = new Contact(personalAddressBook, "hewang.txh", "hewang.txh@taobao.com");
            var contact2 = new Contact(personalAddressBook, "hewang.txh", "hewang.txh@taobao.com");
            var contact3 = new Contact(personalAddressBook, "hewang.txh", "hewang.txh@taobao.com");

            this._contactService.Create(contact1);
            this._contactService.Create(contact2);
            this._contactService.Create(contact3);

            this.Evict(contact1);
            this.Evict(contact2);
            this.Evict(contact3);

            var contacts = this._contactService.GetContacts(personalAddressBook);

            Assert.AreEqual(3, contacts.Count());
        }

        private PersonalAddressBook CreatePersonalAddressBook()
        {
            var account = this.CreateAccount();
            var addressBook = new PersonalAddressBook("我的通讯簿", account);
            this._addressBookService.Create(addressBook);
            return addressBook;
        }
    }
}