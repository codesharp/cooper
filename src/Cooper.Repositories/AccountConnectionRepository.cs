//Copyright (c) CodeSharp.  All rights reserved. - http://www.codesharp.cn/

using System;
using System.Collections.Generic;
using Cooper.Model;
using CodeSharp.Core.Castles;
using Cooper.Model.Accounts;
using NHibernate.Criterion;

namespace Cooper.Repositories
{
    public class AccountConnectionRepository : NHibernateRepositoryBase<int, AccountConnection>, IAccountConnectionRepository
    {
        public AccountConnection FindBy(Type type, string name)
        {
            return this.GetSession()
                .CreateCriteria(type)
                .Add(Expression.Eq("Name", name))
                .UniqueResult() as AccountConnection;
        }

        public IEnumerable<AccountConnection> FindBy(Account account)
        {
            return this.FindAll(Expression.Eq("AccountId", account.ID));
        }
    }
}
