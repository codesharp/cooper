//Copyright (c) CodeSharp.  All rights reserved. - http://www.icodesharp.com/

using System.Collections.Generic;
using CodeSharp.Core.Castles;
using Cooper.Model;
using Cooper.Model.Accounts;
using Cooper.Model.Teams;
using NHibernate.Criterion;
using NHibernate.SqlCommand;

namespace Cooper.Repositories
{
    public class TeamTaskRepository : NHibernateRepositoryBase<long, Task>, ITaskRepository
    {
        public override IEnumerable<Task> FindAll()
        {
            return base.FindAll(IsNotTrashedCriteria);
        }
        public override Task FindBy(long key)
        {
            var task = base.FindBy(key);
            return task == null ? null : task.IsTrashed ? null : task;
        }
        public override IEnumerable<Task> FindBy(params long[] keys)
        {
            return base.FindAll(
                Expression.In("ID", keys),
                IsNotTrashedCriteria);
        }
        public IEnumerable<Task> FindBy(Team team)
        {
            return this.GetSession()
                .CreateCriteria<Task>()
                .Add(Expression.Eq("TeamId", team.ID))
                .Add(IsNotTrashedCriteria)
                .List<Task>();
        }
        public IEnumerable<Task> FindByTag(Team team, string tag)
        {
            return this.GetSession()
                .CreateCriteria<Task>()
                .Add(Expression.Eq("TeamId", team.ID))
                .Add(Expression.Like("_tagList._serializedValue", string.Format("{1}{0}{1}", tag, StringList.Seperator), MatchMode.Anywhere))
                .Add(IsNotTrashedCriteria)
                .List<Task>();
        }
        public IEnumerable<Task> FindByTag(Team team, bool isCompleted, string tag)
        {
            return this.GetSession()
                .CreateCriteria<Task>()
                .Add(Expression.Eq("TeamId", team.ID))
                .Add(Expression.Eq("IsCompleted", isCompleted))
                .Add(Expression.Like("_tagList._serializedValue", string.Format("{1}{0}{1}", tag, StringList.Seperator), MatchMode.Anywhere))
                .Add(IsNotTrashedCriteria)
                .List<Task>();
        }
        public IEnumerable<Task> FindByKey(Team team, string key)
        {
            return this.GetSession()
                .CreateCriteria<Task>()
                .CreateAlias("Comments", "comments", JoinType.LeftOuterJoin)
                .Add(Expression.Eq("TeamId", team.ID))
                .Add(Expression.Or(
                        Expression.Or(
                            Expression.Like("Subject", key, MatchMode.Anywhere),
                            Expression.Like("Body", key, MatchMode.Anywhere)),
                        Expression.Like("comments.Body", key, MatchMode.Anywhere)))
                .Add(IsNotTrashedCriteria)
                .List<Task>();
        }
        public IEnumerable<Task> FindByKey(Team team, bool isCompleted, string key)
        {
            return this.GetSession()
                .CreateCriteria<Task>()
                .CreateAlias("Comments", "comments", JoinType.LeftOuterJoin)
                .Add(Expression.Eq("TeamId", team.ID))
                .Add(Expression.Eq("IsCompleted", isCompleted))
                .Add(Expression.Or(
                        Expression.Or(
                            Expression.Like("Subject", key, MatchMode.Anywhere),
                            Expression.Like("Body", key, MatchMode.Anywhere)),
                        Expression.Like("comments.Body", key, MatchMode.Anywhere)))
                .Add(IsNotTrashedCriteria)
                .List<Task>();
        }
        public IEnumerable<Task> FindBy(Team team, Account account)
        {
            var criteria = BuildCreatorAndAssigneeCriteria(team, account);
            if (criteria != null)
            {
                return this.GetSession()
                    .CreateCriteria<Task>()
                    .Add(Expression.Eq("TeamId", team.ID))
                    .Add(criteria)
                    .Add(IsNotTrashedCriteria)
                    .List<Task>();
            }
            return new List<Task>();
        }
        public IEnumerable<Task> FindBy(Team team, Account account, bool isCompleted)
        {
            var criteria = BuildCreatorAndAssigneeCriteria(team, account);
            if (criteria != null)
            {
                return this.GetSession()
                    .CreateCriteria<Task>()
                    .Add(Expression.Eq("TeamId", team.ID))
                    .Add(Expression.Eq("IsCompleted", isCompleted))
                    .Add(criteria)
                    .Add(IsNotTrashedCriteria)
                    .List<Task>();
            }
            return new List<Task>();
        }
        public IEnumerable<Task> FindBy(Team team, Project project)
        {
            return this.GetSession()
                .CreateCriteria<Task>()
                .Add(Expression.Eq("TeamId", team.ID))
                .CreateAlias("ProjectIds", "projectIds")
                .Add(Expression.Eq("projectIds.ID", project.ID))
                .Add(IsNotTrashedCriteria)
                .List<Task>();
        }
        public IEnumerable<Task> FindBy(Team team, Project project, bool isCompleted)
        {
            return this.GetSession()
                .CreateCriteria<Task>()
                .CreateAlias("ProjectIds", "projectIds")
                .Add(Expression.Eq("TeamId", team.ID))
                .Add(Expression.Eq("IsCompleted", isCompleted))
                .Add(Expression.Eq("projectIds.ID", project.ID))
                .Add(IsNotTrashedCriteria)
                .List<Task>();
        }
        public IEnumerable<Task> FindBy(Team team, Member member)
        {
            return this.GetSession()
                .CreateCriteria<Task>()
                .Add(Expression.Eq("TeamId", team.ID))
                .Add(Expression.Eq("AssigneeId", member.ID))
                .Add(IsNotTrashedCriteria)
                .List<Task>();
        }
        public IEnumerable<Task> FindBy(Team team, Member member, bool isCompleted)
        {
            return this.GetSession()
                .CreateCriteria<Task>()
                .Add(Expression.Eq("TeamId", team.ID))
                .Add(Expression.Eq("AssigneeId", member.ID))
                .Add(Expression.Eq("IsCompleted", isCompleted))
                .Add(IsNotTrashedCriteria)
                .List<Task>();
        }
        public IEnumerable<Task> FindTrashedTasksBy(Team team)
        {
            return this.GetSession()
                .CreateCriteria<Task>()
                .Add(Expression.Eq("TeamId", team.ID))
                .Add(Expression.Eq("IsTrashed", true))
                .List<Task>();
        }
        public IEnumerable<Task> FindNotEmptyTagTasks(Team team)
        {
            return this.GetSession()
                .CreateCriteria<Task>()
                .Add(Expression.Eq("TeamId", team.ID))
                .Add(Expression.Not(Expression.Or(Expression.IsNull("_tagList._serializedValue"), Expression.Eq("_tagList._serializedValue", string.Empty))))
                .Add(IsNotTrashedCriteria)
                .List<Task>();
        }

        private AbstractCriterion BuildCreatorAndAssigneeCriteria(Team team, Account account)
        {
            AbstractCriterion criteria = null;
            var member = team.GetMember(account);
            if (member != null)
            {
                criteria = Expression.Or(Expression.Eq("CreatorMemberId", member.ID), Expression.Eq("AssigneeId", member.ID));
            }
            return criteria;
        }
        private ICriterion IsNotTrashedCriteria
        {
            get
            {
                return Expression.Or(Expression.Eq("IsTrashed", false), Expression.IsNull("IsTrashed"));
            }
        }
    }
}
