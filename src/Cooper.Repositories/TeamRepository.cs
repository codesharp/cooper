//Copyright (c) CodeSharp.  All rights reserved. - http://www.icodesharp.com/

using System.Collections.Generic;
using CodeSharp.Core.Castles;
using Cooper.Model.Accounts;
using Cooper.Model.Teams;

namespace Cooper.Repositories
{
    public class TeamRepository : NHibernateRepositoryBase<int, Team>, ITeamRepository
    {
        public IEnumerable<Team> FindBy(Account account)
        {
            //TODO
            return null;
        }
    }
}
