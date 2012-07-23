//Copyright (c) CodeSharp.  All rights reserved. - http://www.icodesharp.com/

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