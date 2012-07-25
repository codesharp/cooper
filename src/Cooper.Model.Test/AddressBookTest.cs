//Copyright (c) CodeSharp.  All rights reserved. - http://www.icodesharp.com/

using System;
using System.Collections.Generic;
using System.Linq;
using Cooper.Model.AddressBooks;
using NUnit.Framework;

namespace Cooper.Model.Test
{
    [TestFixture]
    [Microsoft.VisualStudio.TestTools.UnitTesting.TestClass]
    public class AddressBookTest : TestBase
    {
        [Test]
        [Microsoft.VisualStudio.TestTools.UnitTesting.TestMethod]
        public void TotalTest()
        {
            CreatePersonalAddressBook();
            CreateSystemAddressBook();
            UpdatePersonalAddressBook();
            DeletePersonalAddressBook();
            GetPersonalAddressBooks();
            GetSystemAddressBooks();
        }

        [Test]
        [Microsoft.VisualStudio.TestTools.UnitTesting.TestMethod]
        public void CreatePersonalAddressBook()
        {
            var account = this.CreateAccount();
            var addressBook = new PersonalAddressBook("我的通讯簿", account);

            Assert.AreNotEqual(DateTime.MinValue, addressBook.CreateTime);

            this._addressBookService.Create(addressBook);
            Assert.Greater(addressBook.ID, 0);

            this.Evict(addressBook);

            var addressBook2 = this._addressBookService.GetAddressBook(addressBook.ID) as PersonalAddressBook;

            Assert.AreEqual(addressBook.Name, addressBook2.Name);
            Assert.AreEqual(addressBook.OwnerAccountId, addressBook2.OwnerAccountId);
            Assert.AreEqual(FormatTime(addressBook.CreateTime), FormatTime(addressBook2.CreateTime));
        }
        [Test]
        [Microsoft.VisualStudio.TestTools.UnitTesting.TestMethod]
        public void CreateSystemAddressBook()
        {
            var addressBook = new SystemAddressBook("系统通讯簿");

            Assert.AreNotEqual(DateTime.MinValue, addressBook.CreateTime);

            this._addressBookService.Create(addressBook);
            Assert.Greater(addressBook.ID, 0);

            this.Evict(addressBook);

            var addressBook2 = this._addressBookService.GetAddressBook(addressBook.ID) as SystemAddressBook;

            Assert.AreEqual(addressBook.Name, addressBook2.Name);
            Assert.AreEqual(FormatTime(addressBook.CreateTime), FormatTime(addressBook2.CreateTime));

            var childAddressBook = new SystemAddressBook("系统子通讯簿");

            Assert.AreNotEqual(DateTime.MinValue, addressBook.CreateTime);

            childAddressBook.SetParent(addressBook);
            Assert.AreEqual(childAddressBook.Parent.ID, addressBook.ID);

            this._addressBookService.Create(childAddressBook);
            Assert.Greater(childAddressBook.ID, 0);

            this.Evict(childAddressBook);

            var childAddressBook2 = this._addressBookService.GetAddressBook(childAddressBook.ID) as SystemAddressBook;

            Assert.AreEqual(childAddressBook.Name, childAddressBook2.Name);
            Assert.AreEqual(childAddressBook.Parent.ID, childAddressBook2.Parent.ID);
            Assert.AreEqual(FormatTime(childAddressBook.CreateTime), FormatTime(childAddressBook2.CreateTime));
        }
        [Test]
        [Microsoft.VisualStudio.TestTools.UnitTesting.TestMethod]
        public void UpdatePersonalAddressBook()
        {
            var account = this.CreateAccount();
            var addressBook = new PersonalAddressBook("我的通讯簿", account);

            Assert.AreNotEqual(DateTime.MinValue, addressBook.CreateTime);

            this._addressBookService.Create(addressBook);
            Assert.Greater(addressBook.ID, 0);

            this.Evict(addressBook);

            var addressBook2 = this._addressBookService.GetAddressBook(addressBook.ID) as PersonalAddressBook;

            addressBook2.SetName(addressBook2.Name + "_updated");
            this._addressBookService.Update(addressBook2);

            this.Evict(addressBook2);

            var addressBook3 = this._addressBookService.GetAddressBook(addressBook2.ID) as PersonalAddressBook;

            Assert.AreEqual(addressBook2.Name, addressBook3.Name);
        }
        [Test]
        [Microsoft.VisualStudio.TestTools.UnitTesting.TestMethod]
        public void DeletePersonalAddressBook()
        {
            var account = this.CreateAccount();
            var addressBook = new PersonalAddressBook("我的通讯簿", account);

            Assert.AreNotEqual(DateTime.MinValue, addressBook.CreateTime);

            this._addressBookService.Create(addressBook);
            Assert.Greater(addressBook.ID, 0);

            this.Evict(addressBook);

            var addressBook2 = this._addressBookService.GetAddressBook(addressBook.ID) as PersonalAddressBook;

            this._addressBookService.Delete(addressBook2);

            this.Evict(addressBook2);

            var addressBook3 = this._addressBookService.GetAddressBook(addressBook2.ID) as PersonalAddressBook;

            Assert.IsNull(addressBook3);
        }
        [Test]
        [Microsoft.VisualStudio.TestTools.UnitTesting.TestMethod]
        public void GetPersonalAddressBooks()
        {
            var account = this.CreateAccount();
            var personalAddressBook1 = new PersonalAddressBook("我的通讯簿1", account);
            var personalAddressBook2 = new PersonalAddressBook("我的通讯簿2", account);
            var personalAddressBook3 = new PersonalAddressBook("我的通讯簿3", account);

            this._addressBookService.Create(personalAddressBook1);
            this._addressBookService.Create(personalAddressBook2);
            this._addressBookService.Create(personalAddressBook3);

            this.Evict(personalAddressBook1);
            this.Evict(personalAddressBook2);
            this.Evict(personalAddressBook3);

            var addressBooks = this._addressBookService.GetAddressBooks(account);

            Assert.AreEqual(3, addressBooks.Count());
        }
        [Test]
        [Microsoft.VisualStudio.TestTools.UnitTesting.TestMethod]
        public void GetSystemAddressBooks()
        {
            var account = this.CreateAccount();
            var radomId1 = Guid.NewGuid().ToString();
            var radomId2 = Guid.NewGuid().ToString();
            var radomId3 = Guid.NewGuid().ToString();
            var systemAddressBook1 = new SystemAddressBook("系统通讯簿1" + radomId1);
            var systemAddressBook2 = new SystemAddressBook("系统通讯簿2" + radomId2);
            var systemAddressBook3 = new SystemAddressBook("系统通讯簿3" + radomId3);

            this._addressBookService.Create(systemAddressBook1);
            this._addressBookService.Create(systemAddressBook2);
            this._addressBookService.Create(systemAddressBook3);

            this.Evict(systemAddressBook1);
            this.Evict(systemAddressBook2);
            this.Evict(systemAddressBook3);

            var addressBooks = this._addressBookService.GetAllSystemAddressBooks();

            Assert.AreEqual(3, addressBooks.Where(x => new List<string> { "系统通讯簿1" + radomId1, "系统通讯簿2" + radomId2, "系统通讯簿3" + radomId3 }.Contains(x.Name)).Count());
        }
    }
}