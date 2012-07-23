//Copyright (c) CodeSharp.  All rights reserved. - http://www.icodesharp.com/

using System;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using Cooper.Model.Tasks;
using Cooper.Model.Accounts;

namespace Cooper.Model.Test
{
    [TestFixture]
    [Microsoft.VisualStudio.TestTools.UnitTesting.TestClass]
    public class TasklistTest : TestBase
    {
        [Test]
        [Microsoft.VisualStudio.TestTools.UnitTesting.TestMethod]
        public void Create_PersonalTasklist()
        {
            var a = this.CreateAccount();
            var list = new PersonalTasklist("default", a);
            Assert.Throws(typeof(AssertionException), () => list.SetName(null));
            this._tasklistService.Create(list);
            
            this.Evict(list);

            var list2 = this._tasklistService.GetTasklist(list.ID);
            Assert.IsInstanceOf<PersonalTasklist>(list2);
            Assert.AreEqual(list.Name, list2.Name);
            Assert.AreEqual(a.ID, (list2 as PersonalTasklist).OwnerAccountId);
        }
        [Test]
        [Microsoft.VisualStudio.TestTools.UnitTesting.TestMethod]
        public void Update_PersonalTasklist()
        {
            var a = this.CreateAccount();
            var list = new PersonalTasklist("default", a);
            this._tasklistService.Create(list);

            this.Evict(list);

            list = this._tasklistService.GetTasklist(list.ID) as PersonalTasklist;
            list.SetName("abc");
            this._tasklistService.Update(list);

            this.Evict(list);
            list = this._tasklistService.GetTasklist(list.ID) as PersonalTasklist;
            Assert.AreEqual("abc", list.Name);
        }

        [Test]
        [Microsoft.VisualStudio.TestTools.UnitTesting.TestMethod]
        public void GetTasklist()
        {
            var a = this.CreateAccount();
            var all = this._tasklistService.GetTasklists(a);
            
            Assert.AreEqual(0, all.Count());

            this._tasklistService.Create(new PersonalTasklist("default", a));
            this._tasklistService.Create(new PersonalTasklist("default", a));
            all = this._tasklistService.GetTasklists(a);
            Assert.AreEqual(2, all.Count());
            Assert.IsInstanceOf<PersonalTasklist>(all.First());
        }
    }
}