//Copyright (c) CodeSharp.  All rights reserved. - http://www.icodesharp.com/

using System.Collections.Generic;
using CodeSharp.Core.Castles;
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
        public IEnumerable<Task> FindBy(Project project)
        {
            return this.GetSession()
                .CreateCriteria<Task>()
                .CreateAlias("Projects", "projects")
                .Add(Expression.Eq("projects.ID", project.ID))
                .List<Task>();
        }
        public IEnumerable<Task> FindBy(TeamMember teamMember)
        {
            return this.GetSession()
                .CreateCriteria<Task>()
                .Add(Expression.Eq("AssigneeId", teamMember.ID))
                .List<Task>();
        }
    }
}
