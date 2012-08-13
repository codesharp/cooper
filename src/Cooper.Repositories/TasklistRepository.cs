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
    public class TaskFolderRepository : CodeSharp.Core.Castles.NHibernateRepositoryBase<int, TaskFolder>, ITaskFolderRepository
    {
        #region ITaskFolderRepository Members

        public IEnumerable<PersonalTaskFolder> FindBy(Account account)
        {
            return this.GetSession()
                .CreateCriteria<PersonalTaskFolder>()
                .Add(Expression.Eq("OwnerAccountId", account.ID))
                .List<PersonalTaskFolder>();
        }

        #endregion
    }
}
