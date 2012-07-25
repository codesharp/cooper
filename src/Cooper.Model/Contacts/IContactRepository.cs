//Copyright (c) CodeSharp.  All rights reserved. - http://www.icodesharp.com/

using System.Collections.Generic;
using CodeSharp.Core.RepositoryFramework;
using Cooper.Model.AddressBooks;
using Cooper.Model.ContactGroups;

namespace Cooper.Model.Contacts
{
    /// <summary>
    /// 联系人仓储
    /// </summary>
    public interface IContactRepository : IRepository<int, Contact>
    {
        IEnumerable<Contact> FindBy(AddressBook addressBook);
    }
}