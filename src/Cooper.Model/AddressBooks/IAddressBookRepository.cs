using System;
using System.Collections.Generic;
using CodeSharp.Core.RepositoryFramework;

namespace Cooper.Model.AddressBooks
{
    /// <summary>
    /// 通讯簿仓储
    /// </summary>
    public interface IAddressBookRepository : IRepository<int, AddressBook>
    {
    }
}