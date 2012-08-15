//Copyright (c) CodeSharp.  All rights reserved. - http://www.icodesharp.com/

using System.Collections.Generic;
using CodeSharp.Core.Castles;
using Cooper.Model.Teams;
using NHibernate.Criterion;

namespace Cooper.Repositories
{
    public class TeamMemberRepository : NHibernateRepositoryBase<int, TeamMember>, ITeamMemberRepository
    {
        public IEnumerable<TeamMember> FindBy(Team team)
        {
            return this.GetSession()
                .CreateCriteria<TeamMember>()
                .Add(Expression.Eq("TeamId", team.ID))
                .List<TeamMember>();
        }
    }
}
