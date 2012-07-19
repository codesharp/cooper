//Copyright (c) CodeSharp.  All rights reserved. - http://www.codesharp.cn/

using System;
using System.Collections.Generic;
using CodeSharp.Core.RepositoryFramework;

namespace Cooper.Model.Accounts
{
    /// <summary>外部帐号连接仓储
    /// </summary>
    public interface IAccountConnectionRepository : IRepository<int, AccountConnection>
    {
        AccountConnection FindBy(Type type, string name);
        IEnumerable<AccountConnection> FindBy(Account account);
    }
}