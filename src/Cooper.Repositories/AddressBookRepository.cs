//Copyright (c) CodeSharp.  All rights reserved. - http://www.icodesharp.com/

using System.Collections.Generic;
using CodeSharp.Core.Castles;
using Cooper.Model.Accounts;
using Cooper.Model.AddressBooks;
using NHibernate.Criterion;

namespace Cooper.Repositories
{
    public class AddressBookRepository : NHibernateRepositoryBase<int, AddressBook>, IAddressBookRepository
    {
        #region IAddressBookRepository Members

        public IEnumerable<PersonalAddressBook> FindBy(Account account)
        {
            return this.GetSession()
                .CreateCriteria<PersonalAddressBook>()
                .Add(Expression.Eq("OwnerAccountId", account.ID))
                .List<PersonalAddressBook>();
        }

        public IEnumerable<SystemAddressBook> FindAllSystemAddressBooks()
        {
            return this.GetSession()
                .CreateCriteria<SystemAddressBook>()
                .List<SystemAddressBook>();
        }

        #endregion
    }
}
