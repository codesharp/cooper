//Copyright (c) CodeSharp.  All rights reserved. - http://www.icodesharp.com/

using System.Collections.Generic;
using System.Linq;
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
        public Member FindMemberBy(Team team, string email)
        {
            return this.GetSession()
                .CreateCriteria<Member>()
                .Add(Expression.Eq("TeamId", team.ID))
                .Add(Expression.Eq("Email", email))
                .UniqueResult<Member>();
        }
        public Member FindMemberBy(Team team, Account account)
        {
            return this.GetSession()
                .CreateCriteria<Member>()
                .Add(Expression.Eq("TeamId", team.ID))
                .Add(Expression.Eq("AssociatedAccountId", account.ID))
                .UniqueResult<Member>();
        }
        public IEnumerable<Member> FindUnassociatedMembersBy(string email)
        {
            return this.GetSession()
                .CreateCriteria<Member>()
                .Add(Expression.IsNotNull("TeamId"))
                .Add(Expression.IsNull("AssociatedAccountId"))
                .Add(Expression.Eq("Email", email))
                .List<Member>();
        }
    }
}
