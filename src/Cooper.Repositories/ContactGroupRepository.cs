//Copyright (c) CodeSharp.  All rights reserved. - http://www.icodesharp.com/

using System.Collections.Generic;
using CodeSharp.Core.Castles;
using Cooper.Model.AddressBooks;
using Cooper.Model.ContactGroups;
using NHibernate.Criterion;

namespace Cooper.Repositories
{
    public class ContactGroupRepository : NHibernateRepositoryBase<int, ContactGroup>, IContactGroupRepository
    {
        #region IContactGroupRepository Members

        public IEnumerable<ContactGroup> FindBy(AddressBook addressBook)
        {
            return this.GetSession()
                .CreateCriteria<ContactGroup>()
                .Add(Expression.Eq("AddressBookId", addressBook.ID))
                .List<ContactGroup>();
        }

        #endregion
    }
}
