//Copyright (c) CodeSharp.  All rights reserved. - http://www.icodesharp.com/

using System.Collections.Generic;
using CodeSharp.Core.RepositoryFramework;
using Cooper.Model.Accounts;

namespace Cooper.Model.Contacts
{
    /// <summary>
    /// 通讯簿仓储
    /// </summary>
    public interface IAddressBookRepository : IRepository<int, AddressBook>
    {
        IEnumerable<PersonalAddressBook> FindBy(Account account);
        IEnumerable<SystemAddressBook> FindAllSystemAddressBooks();
    }
}