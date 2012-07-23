//Copyright (c) CodeSharp.  All rights reserved. - http://www.icodesharp.com/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NHibernate.Criterion;
using Cooper.Model.Accounts;
using Cooper.Model.Tasks;

namespace Cooper.Repositories
{
    public class TasklistRepository : CodeSharp.Core.Castles.NHibernateRepositoryBase<int, Tasklist>, ITasklistRepository
    {
        #region ITasklistRepository Members

        public new IEnumerable<PersonalTasklist> FindBy(Account account)
        {
            return this.GetSession()
                .CreateCriteria<PersonalTasklist>()
                .Add(Expression.Eq("OwnerAccountId", account.ID))
                .List<PersonalTasklist>();
        }

        #endregion
    }
}
