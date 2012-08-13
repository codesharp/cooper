//Copyright (c) CodeSharp.  All rights reserved. - http://www.icodesharp.com/

using System.Collections.Generic;
using CodeSharp.Core.RepositoryFramework;
using Cooper.Model.Contacts;

namespace Cooper.Model.Contacts
{
    /// <summary>
    /// 联系人组仓储
    /// </summary>
    public interface IContactGroupRepository : IRepository<int, ContactGroup>
    {
        IEnumerable<ContactGroup> FindBy(AddressBook addressBook);
    }
}