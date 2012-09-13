//Copyright (c) CodeSharp.  All rights reserved. - http://www.icodesharp.com/

using System.Collections.Generic;
using System.Linq;
using Cooper.Model;
using Cooper.Model.Accounts;
using Cooper.Model.Tasks;
using NHibernate.Criterion;

namespace Cooper.Repositories
{
    public class PersonalTaskRepository : CodeSharp.Core.Castles.NHibernateRepositoryBase<long, PersonalTask>, IPersonalTaskRepository
    {
        #region IPersonalTaskRepository Members

        public override IEnumerable<PersonalTask> FindAll()
        {
            return base.FindAll(Expression.Eq("IsTrashed", false));
        }
        public override PersonalTask FindBy(long key)
        {
            var task = base.FindBy(key);
            return task == null ? null : task.IsTrashed ? null : task;
        }
        public override IEnumerable<PersonalTask> FindBy(params long[] keys)
        {
            return base.FindAll(
                Expression.In("ID", keys),
                Expression.Eq("IsTrashed", false));
        }
        public IEnumerable<PersonalTask> FindBy(Account account)
        {
            return this.FindAll(
                Expression.Eq("CreatorAccountId", account.ID),
                Expression.Eq("IsTrashed", false));
        }
        public IEnumerable<PersonalTask> FindByTag(Account account, string tag)
        {
            return this.FindAll(
                Expression.Eq("CreatorAccountId", account.ID),
                Expression.Like("_tagList._serializedValue", string.Format("{1}{0}{1}", tag, StringList.Seperator), MatchMode.Anywhere),
                Expression.Eq("IsTrashed", false));
        }
        public IEnumerable<PersonalTask> FindByTag(Account account, bool isCompleted, string tag)
        {
            return this.FindAll(
                Expression.Eq("CreatorAccountId", account.ID),
                Expression.Eq("IsCompleted", isCompleted),
                Expression.Like("_tagList._serializedValue", string.Format("{1}{0}{1}", tag, StringList.Seperator), MatchMode.Anywhere),
                Expression.Eq("IsTrashed", false));
        }
        public IEnumerable<PersonalTask> FindBy(Account account, TaskFolder folder)
        {
            return this.FindAll(Expression.Eq("CreatorAccountId", account.ID)
                , folder == null
                ? Expression.IsNull("TaskFolderId")
                : Expression.Eq("TaskFolderId", folder.ID),
                Expression.Eq("IsTrashed", false));
        }
        public IEnumerable<PersonalTask> FindBy(Account account, bool isCompleted)
        {
            return this.FindAll(
                Expression.Eq("CreatorAccountId", account.ID),
                Expression.Eq("IsCompleted", isCompleted),
                Expression.Eq("IsTrashed", false));
        }
        public IEnumerable<PersonalTask> FindBy(Account account, bool isCompleted, TaskFolder folder)
        {
            return this.FindAll(
                Expression.Eq("CreatorAccountId", account.ID),
                Expression.Eq("IsCompleted", isCompleted)
                , folder == null
                ? Expression.IsNull("TaskFolderId")
                : Expression.Eq("TaskFolderId", folder.ID),
                Expression.Eq("IsTrashed", false));
        }
        public IEnumerable<PersonalTask> FindTrashedTasksBy(Account account)
        {
            return this.FindAll(
                Expression.Eq("CreatorAccountId", account.ID),
                Expression.Eq("IsTrashed", true));
        }

        #endregion
    }
}
