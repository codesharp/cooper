using System;
using System.Collections.Generic;
using CodeSharp.Core.RepositoryFramework;

namespace Cooper.Model.Contacts
{
    /// <summary>
    /// 联系人仓储
    /// </summary>
    public interface IContactRepository : IRepository<int, Contact>
    {
    }
}