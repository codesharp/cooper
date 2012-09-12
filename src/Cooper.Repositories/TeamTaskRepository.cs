//Copyright (c) CodeSharp.  All rights reserved. - http://www.icodesharp.com/

using System.Collections.Generic;
using CodeSharp.Core.Castles;
using Cooper.Model;
using Cooper.Model.Accounts;
using Cooper.Model.Teams;
using NHibernate.Criterion;

namespace Cooper.Repositories
{
    public class TeamTaskRepository : NHibernateRepositoryBase<long, Task>, ITaskRepository
    {
        public IEnumerable<Task> FindBy(Team team)
        {
            return this.GetSession()
                .CreateCriteria<Task>()
                .Add(Expression.Eq("TeamId", team.ID))
                .List<Task>();
        }
        public IEnumerable<Task> FindByTag(Team team, string tag)
        {
            return this.GetSession()
                .CreateCriteria<Task>()
                .Add(Expression.Eq("TeamId", team.ID))
                .Add(Expression.Like("_tagList._serializedValue", string.Format("{1}{0}{1}", tag, StringList.Seperator), MatchMode.Anywhere))
                .List<Task>();
        }
        public IEnumerable<Task> FindByTag(Team team, bool isCompleted, string tag)
        {
            return this.GetSession()
                .CreateCriteria<Task>()
                .Add(Expression.Eq("TeamId", team.ID))
                .Add(Expression.Eq("IsCompleted", isCompleted))
                .Add(Expression.Like("_tagList._serializedValue", string.Format("{1}{0}{1}", tag, StringList.Seperator), MatchMode.Anywhere))
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
                .List<Task>();
        }
        public IEnumerable<Task> FindBy(Team team, Member member)
        {
            return this.GetSession()
                .CreateCriteria<Task>()
                .Add(Expression.Eq("TeamId", team.ID))
                .Add(Expression.Eq("AssigneeId", member.ID))
                .List<Task>();
        }
        public IEnumerable<Task> FindBy(Team team, Member member, bool isCompleted)
        {
            return this.GetSession()
                .CreateCriteria<Task>()
                .Add(Expression.Eq("TeamId", team.ID))
                .Add(Expression.Eq("AssigneeId", member.ID))
                .Add(Expression.Eq("IsCompleted", isCompleted))
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
    }
}
