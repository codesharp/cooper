//Copyright (c) CodeSharp.  All rights reserved. - http://www.icodesharp.com/

using System;
using System.Collections.Generic;
using Cooper.Model;
using CodeSharp.Core.Castles;
using Cooper.Model.Accounts;
using NHibernate.Criterion;
using NHibernate;

namespace Cooper.Repositories
{
    public class AccountRepository : NHibernateRepositoryBase<int, Account>, IAccountRepository
    {
        public Account FindBy(string name)
        {
            return this.FindOne(Expression.Eq("Name", name));
        }
    }
}
