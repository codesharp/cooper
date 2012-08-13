//Copyright (c) CodeSharp.  All rights reserved. - http://www.icodesharp.com/

using System.Collections.Generic;
using System.Linq;
using CodeSharp.Core.DomainBase;

namespace Cooper.Model.Accounts
{
    /// <summary>Weibo帐号连接
    /// </summary>
    public class WeiboConnection : AccountConnection
    {
        protected WeiboConnection() : base() { }//由于NH
        public WeiboConnection(string name, string token, Account account) : base(name, token, account) { }
    }
}