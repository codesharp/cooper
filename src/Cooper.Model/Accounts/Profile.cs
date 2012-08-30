//Copyright (c) CodeSharp.  All rights reserved. - http://www.icodesharp.com/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using CodeSharp.Core.DomainBase;
using CodeSharp.Core.Utils;

namespace Cooper.Model.Accounts
{
    /// <summary>个人账号设置
    /// </summary>
    public class Profile : ExtensiableEntityBase<int>
    {
        private Account _account { get; set; }

        protected Profile() { }
        internal Profile(Account account)
            : this()
        {
            this._account = account;
        }
    }
}