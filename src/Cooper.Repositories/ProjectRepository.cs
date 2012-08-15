//Copyright (c) CodeSharp.  All rights reserved. - http://www.icodesharp.com/

using System.Collections.Generic;
using CodeSharp.Core.Castles;
using Cooper.Model.Teams;
using NHibernate.Criterion;

namespace Cooper.Repositories
{
    public class ProjectRepository : NHibernateRepositoryBase<int, Project>, IProjectRepository
    {
        public IEnumerable<Project> FindBy(Team team)
        {
            return this.GetSession()
                .CreateCriteria<Project>()
                .Add(Expression.Eq("TeamId", team.ID))
                .List<Project>();
        }
    }
}
