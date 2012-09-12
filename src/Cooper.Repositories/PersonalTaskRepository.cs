//Copyright (c) CodeSharp.  All rights reserved. - http://www.icodesharp.com/

using System.Collections.Generic;
using Cooper.Model.Accounts;
using Cooper.Model.Tasks;
using NHibernate.Criterion;

namespace Cooper.Repositories
{
    public class PersonalTaskRepository : CodeSharp.Core.Castles.NHibernateRepositoryBase<long, PersonalTask>, IPersonalTaskRepository
    {
        #region IPersonalTaskRepository Members

        public IEnumerable<PersonalTask> FindBy(Account account)
        {
            return this.FindAll(Expression.Eq("CreatorAccountId", account.ID));
        }
        public IEnumerable<PersonalTask> FindByTag(Account account, string tag)
        {
            return this.FindAll(Expression.Eq("CreatorAccountId", account.ID), Expression.Like("_tags", string.Format("${0}$", tag), MatchMode.Anywhere));
        }
        public IEnumerable<PersonalTask> FindByTag(Account account, bool isCompleted, string tag)
        {
            return this.FindAll(Expression.Eq("CreatorAccountId", account.ID), Expression.Eq("IsCompleted", isCompleted), Expression.Like("_tags", string.Format("${0}$", tag), MatchMode.Anywhere));
        }
        public IEnumerable<PersonalTask> FindBy(Account account, TaskFolder folder)
        {
            return this.FindAll(Expression.Eq("CreatorAccountId", account.ID)
                , folder == null
                ? Expression.IsNull("TaskFolderId")
                : Expression.Eq("TaskFolderId", folder.ID));
        }
        public IEnumerable<PersonalTask> FindBy(Account account, bool isCompleted)
        {
            return this.FindAll(Expression.Eq("CreatorAccountId", account.ID), Expression.Eq("IsCompleted", isCompleted));
        }
        public IEnumerable<PersonalTask> FindBy(Account account, bool isCompleted, TaskFolder folder)
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
