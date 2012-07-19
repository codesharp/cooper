using System;
using System.Collections.Generic;
using Taobao.Infrastructure.RepositoryFramework;

namespace Taobao.Cooper.Model
{
    /// <summary>
    /// 联系人仓储
    /// </summary>
    public interface IContactRepository : IRepository<int, Contacter>
    {
        IEnumerable<Contacter> GetContactsByAccount(Account account);
    }
}