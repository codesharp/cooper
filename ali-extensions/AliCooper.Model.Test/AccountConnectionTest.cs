//Copyright (c) CodeSharp.  All rights reserved. - http://www.codesharp.cn/

using System;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using AliCooper.Model.Accounts;

namespace AliCooper.Model.Test
{
    [TestFixture]
    [Microsoft.VisualStudio.TestTools.UnitTesting.TestClass]
    public class AccountConnectionTest : TestBase
    {
        private static readonly string _token = "abcdefg";
        [Test]
        [Microsoft.VisualStudio.TestTools.UnitTesting.TestMethod]
        public void CreateArk()
        {
            var u = this.RandomUser();
            var c = new ArkConnection(u, _token, this.CreateAccount());
            this._accountConnectionService.Create(c);
            Assert.Greater(c.ID, 0);

            var c2 = this._accountConnectionService.GetConnection<ArkConnection>(u);
            Assert.IsNotNull(c2);
            Assert.AreEqual(c2.ID, c.ID);
            Assert.AreEqual(c2.Name, c.Name);
        }
    }
}