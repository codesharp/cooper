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
    public class TaskRepository : CodeSharp.Core.Castles.NHibernateRepositoryBase<long, Task>, ITaskRepository
    {
        #region ITaskRepository Members

        public IEnumerable<Task> FindBy(Account account)
        {
            return this.FindAll(Expression.Eq("CreatorAccountId", account.ID));
        }

        public IEnumerable<Task> FindBy(Account account, TaskFolder folder)
        {
            return this.FindAll(Expression.Eq("CreatorAccountId", account.ID)
                , folder == null
                ? Expression.IsNull("TaskFolderId")
                : Expression.Eq("TaskFolderId", folder.ID));
        }

        public IEnumerable<Task> FindBy(Account account, bool isCompleted)
        {
            return this.FindAll(Expression.Eq("CreatorAccountId", account.ID), Expression.Eq("IsCompleted", isCompleted));
        }

        public IEnumerable<Task> FindBy(Account account, bool isCompleted, TaskFolder folder)
        {
            return this.FindAll(Expression.Eq("CreatorAccountId", account.ID)
                , Expression.Eq("IsCompleted", isCompleted)
                , folder == null
                ? Expression.IsNull("TaskFolderId")
                : Expression.Eq("TaskFolderId", folder.ID));
        }

        #endregion
    }
}
