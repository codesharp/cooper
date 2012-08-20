//Copyright (c) CodeSharp.  All rights reserved. - http://www.icodesharp.com/

using System.Collections.Generic;
using CodeSharp.Core.Castles;
using Cooper.Model.Accounts;
using Cooper.Model.Teams;
using NHibernate.Criterion;

namespace Cooper.Repositories
{
    public class TeamRepository : NHibernateRepositoryBase<int, Team>, ITeamRepository
    {
        public IEnumerable<Team> FindBy(Account account)
        {
            return this.GetSession()
                .CreateCriteria<Team>()
                .CreateAlias("Members", "members")
                .Add(Expression.Eq("members.AssociatedAccountId", account.ID))
                .List<Team>();
        }
    }
}
