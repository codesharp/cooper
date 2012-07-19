//Copyright (c) CodeSharp.  All rights reserved. - http://www.codesharp.cn/

using System.Collections.Generic;
using System.Linq;
using CodeSharp.Core.DomainBase;

namespace Cooper.Model.Accounts
{
    /// <summary>Google帐号连接
    /// </summary>
    public class GoogleConnection : AccountConnection
    {
        protected GoogleConnection() : base() { }//由于NH
        public GoogleConnection(string name, string token, Account account) : base(name, token, account) { }
    }
}