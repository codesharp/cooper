//Copyright (c) CodeSharp.  All rights reserved. - http://www.codesharp.cn/

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
    public class AccountTest : TestBase
    {
        private static readonly string _token = "abcdefg";
        [SetUp]
        public void SetUp()
        {
            this.Idle(1);
        }

        [Test]
        [Microsoft.VisualStudio.TestTools.UnitTesting.TestMethod]
        public void Account()
        {
            var a = new Account(this.RandomUser());
            a.SetName("houkun");
            Assert.AreEqual("houkun", a.Name);
            Assert.Catch(() => a.SetName(null));
            Assert.Catch(() => a.SetName(string.Empty));
            Assert.Catch(() => a.SetName("  "));

            a.SetPassword("houkun");
            Assert.Catch(() => a.SetPassword(null));
            Assert.Catch(() => a.SetPassword("  "));
        }

        [Test]
        [Microsoft.VisualStudio.TestTools.UnitTesting.TestMethod]
        public void Profile()
        {
            var a = new Account(this.RandomUser());
            Assert.IsNullOrEmpty(a.GetProfile("test"));
            a.SetProfile("test", "test");
            Assert.AreEqual("test", a.GetProfile("test"));

            this._accountService.Create(a);

            this.Evict(a);

            var a2 = this._accountService.GetAccount(a.ID);
            Assert.AreEqual("test", a2.GetProfile("test"));
        }
        [Test]
        [Microsoft.VisualStudio.TestTools.UnitTesting.TestMethod]
        public void ProfileUpdate()
        {
            var a = new Account(this.RandomUser());
            //未初始化profile
            this._accountService.Create(a);
            //此时才初始化profile
            a.SetProfile("test", "test");
            this._accountService.Update(a);

            this.Evict(a);

            var a2 = this._accountService.GetAccount(a.ID);
            Assert.AreEqual("test", a2.GetProfile("test"));
        }

        [Test]
        [Microsoft.VisualStudio.TestTools.UnitTesting.TestMethod]
        public void CreateDuplicate()
        {
            var name = this.RandomUser();
            //不可重复
            this.AssertParallel(() => this._accountService.Create(new Account(name)), 4, 1);
            Assert.Catch(typeof(AssertionException), () => this._accountService.Create(new Account(name)));
        }
        [Test]
        [Microsoft.VisualStudio.TestTools.UnitTesting.TestMethod]
        public void UpdateDuplicate()
        {
            var a = new Account(this.RandomUser());
            this._accountService.Create(a);
            //不可重复
            a.SetName(this.RandomUser());
            this.AssertParallel(() => this._accountService.Update(a), 4, 1);

            //不可与其他重复
            var a2 = new Account(this.RandomUser());
            this._accountService.Create(a2);
            a.SetName(a2.Name);
            Assert.Catch(typeof(AssertionException), () => this._accountService.Update(a));
        }

        [Test(Description = "根据连接创建账号")]
        [Microsoft.VisualStudio.TestTools.UnitTesting.TestMethod]
        public void CreateBy()
        {
            var u = this.RandomUser();
            var a = this._accountHelper.CreateBy<GoogleConnection>(u, _token);
            Assert.IsNotNull(this._accountConnectionService.GetConnection<GoogleConnection>(u));
            Assert.AreEqual(a.ID, this._accountConnectionService.GetConnection<GoogleConnection>(u).AccountId);
            Assert.AreEqual(1, this._accountConnectionService.GetConnections(a).Count());
        }
        [Test(Description = "根据连接创建账号，并发")]
        [Microsoft.VisualStudio.TestTools.UnitTesting.TestMethod]
        public void CreateByDuplicate()
        {
            var u = this.RandomUser();
            this.AssertParallel(() => this._accountHelper.CreateBy<GoogleConnection>(u, _token), 4, 1);
        }
        [Test(Description = "模拟外部连接登录逻辑")]
        [Microsoft.VisualStudio.TestTools.UnitTesting.TestMethod]
        public void LoginBy()
        {
            //实际编写时，根据不同类型获得外部账号标识
            var u = this.RandomUser();
            //先尝试获取账号连接
            this._accountConnectionService.GetConnection<GoogleConnection>(u);
            if (u != null) return;
            //创建账号
            var a = this._accountHelper.CreateBy<GoogleConnection>(u, _token);
        }
    }
}
