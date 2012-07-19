//Copyright (c) CodeSharp.  All rights reserved. - http://www.codesharp.cn/

using System.Collections.Generic;
using System.Linq;
using CodeSharp.Core.DomainBase;

namespace Cooper.Model.Accounts
{
    /// <summary>GitHub帐号连接
    /// </summary>
    public class GitHubConnection : AccountConnection
    {
        protected GitHubConnection() : base() { }//由于NH
        public GitHubConnection(string name, string token, Account account) : base(name, token, account) { }
    }
}