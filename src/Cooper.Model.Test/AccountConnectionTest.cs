//Copyright (c) CodeSharp.  All rights reserved. - http://www.icodesharp.com/

using System;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using Cooper.Model.Accounts;

namespace Cooper.Model.Test
{
    [TestFixture]
    [Microsoft.VisualStudio.TestTools.UnitTesting.TestClass]
    public class AccountConnectionTest : TestBase
    {
        private static readonly string _token = "abcdefg";
        [Test]
        [Microsoft.VisualStudio.TestTools.UnitTesting.TestMethod]
        public void CreateGoole()
        {
            var u = this.RandomString();
            var c = new GoogleConnection(u, _token, this.CreateAccount());
            this._accountConnectionService.Create(c);
            Assert.Greater(c.ID, 0);

            var c2 = this._accountConnectionService.GetConnection<GoogleConnection>(u);
            Assert.IsNotNull(c2);
            Assert.AreEqual(c2.ID, c.ID);
            Assert.AreEqual(c2.Name, c.Name);
        }
        [Test]
        [Microsoft.VisualStudio.TestTools.UnitTesting.TestMethod]
        public void CreateGit()
        {
            var u = this.RandomString();
            var c = new GitHubConnection(u, _token, this.CreateAccount());
            this._accountConnectionService.Create(c);
            Assert.Greater(c.ID, 0);

            var c2 = this._accountConnectionService.GetConnection<GitHubConnection>(u);
            Assert.IsNotNull(c2);
            Assert.AreEqual(c2.ID, c.ID);
            Assert.AreEqual(c2.Name, c.Name);
        }
        [Test]
        [Category("concurrent")]
        [Microsoft.VisualStudio.TestTools.UnitTesting.TestMethod]
        public void CreateDuplicate()
        {
            var a = this.CreateAccount();
            var u = this.RandomString();
            //每个类型的连接内用户名不可重复
            this.AssertParallel(() => this._accountConnectionService.Create(new GoogleConnection(u, _token, a)), 4, 1);
            this.AssertParallel(() => this._accountConnectionService.Create(new GitHubConnection(u, _token, a)), 4, 1);
        }
        [Test]
        [Microsoft.VisualStudio.TestTools.UnitTesting.TestMethod]
        public void Get()
        {
            var a = this.CreateAccount();
            this._accountConnectionService.Create(new GoogleConnection(this.RandomString(), _token, a));
            this._accountConnectionService.Create(new GitHubConnection(this.RandomString(), _token, a));
            Assert.AreEqual(2, this._accountConnectionService.GetConnections(a).Count());
            Assert.DoesNotThrow(() => this._accountConnectionService.GetConnections(a).First(o => o is GoogleConnection));
            Assert.DoesNotThrow(() => this._accountConnectionService.GetConnections(a).First(o => o is GitHubConnection));
        }
    }
}