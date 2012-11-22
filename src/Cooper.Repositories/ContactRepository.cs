//Copyright (c) CodeSharp.  All rights reserved. - http://www.icodesharp.com/

using System;
using System.Collections.Generic;
using CodeSharp.Core.Castles;
using Cooper.Model.Contacts;
using NHibernate.Criterion;

namespace Cooper.Repositories
{
    public class ContactRepository : NHibernateRepositoryBase<Guid, Contact>, IContactRepository
    {
        #region IContactRepository Members

        public IEnumerable<Contact> FindBy(AddressBook addressBook)
        {
            return this.GetSession()
                .CreateCriteria<Contact>()
                .Add(Expression.Eq("AddressBookId", addressBook.ID))
                .List<Contact>();
        }
        public IEnumerable<Contact> FindBy(ContactGroup contactGroup)
        {
            return this.GetSession()
                .CreateCriteria<Contact>()
                .Add(Expression.Eq("GroupId", contactGroup.ID))
                .List<Contact>();
        }

        #endregion
    }
}
