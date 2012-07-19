//Copyright (c) CodeSharp.  All rights reserved. - http://www.codesharp.cn/

using CodeSharp.Core.RepositoryFramework;

namespace Cooper.Model.Accounts
{
    /// <summary>帐号仓储
    /// </summary>
    public interface IAccountRepository : IRepository<int, Account>
    {
        Account FindBy(string name);
    }
}