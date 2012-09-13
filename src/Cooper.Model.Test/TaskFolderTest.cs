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
    public class TaskFolderTest : TestBase
    {
        [Test]
        [Microsoft.VisualStudio.TestTools.UnitTesting.TestMethod]
        public void Create_Personal()
        {
            var a = this.CreateAccount();
            var folder = new PersonalTaskFolder("default", a);
            Assert.Throws(typeof(AssertionException), () => folder.SetName(null));
            this._taskFolderService.Create(folder);
            
            this.Evict(folder);

            var folder2 = this._taskFolderService.GetTaskFolder(folder.ID);
            Assert.IsInstanceOf<PersonalTaskFolder>(folder2);
            Assert.AreEqual(folder.Name, folder2.Name);
            Assert.AreEqual(a.ID, (folder2 as PersonalTaskFolder).OwnerAccountId);
        }
        [Test]
        [Microsoft.VisualStudio.TestTools.UnitTesting.TestMethod]
        public void Update_Personal()
        {
            var a = this.CreateAccount();
            var folder = new PersonalTaskFolder("default", a);
            this._taskFolderService.Create(folder);

            this.Evict(folder);

            folder = this._taskFolderService.GetTaskFolder(folder.ID) as PersonalTaskFolder;
            folder.SetName("abc");
            this._taskFolderService.Update(folder);

            this.Evict(folder);
            folder = this._taskFolderService.GetTaskFolder(folder.ID) as PersonalTaskFolder;
            Assert.AreEqual("abc", folder.Name);
        }
        [Test]
        [Microsoft.VisualStudio.TestTools.UnitTesting.TestMethod]
        public void Extension()
        {
            var a = this.CreateAccount();
            var folder = new PersonalTaskFolder("default", a);
            folder.Settings["key"] = "abc";
            this._taskFolderService.Create(folder);

            this.Evict(folder);

            folder = this._taskFolderService.GetTaskFolder(folder.ID) as PersonalTaskFolder;
            Assert.AreEqual("abc", folder.Settings["key"]);
        }

        [Test]
        [Microsoft.VisualStudio.TestTools.UnitTesting.TestMethod]
        public void Get()
        {
            var a = this.CreateAccount();
            var all = this._taskFolderService.GetTaskFolders(a);
            
            Assert.AreEqual(0, all.Count());

            this._taskFolderService.Create(new PersonalTaskFolder("default", a));
            this._taskFolderService.Create(new PersonalTaskFolder("default", a));
            all = this._taskFolderService.GetTaskFolders(a);
            Assert.AreEqual(2, all.Count());
            Assert.IsInstanceOf<PersonalTaskFolder>(all.First());
        }
    }
}