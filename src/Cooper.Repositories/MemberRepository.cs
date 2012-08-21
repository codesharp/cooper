//Copyright (c) CodeSharp.  All rights reserved. - http://www.icodesharp.com/

using System.Collections.Generic;
using CodeSharp.Core.Castles;
using Cooper.Model.Teams;
using NHibernate.Criterion;

namespace Cooper.Repositories
{
    public class MemberRepository : NHibernateRepositoryBase<int, Member>, IMemberRepository
    {
        public IEnumerable<Member> FindBy(Team team)
        {
            return this.GetSession()
                .CreateCriteria<Member>()
                .Add(Expression.Eq("TeamId", team.ID))
                .List<Member>();
        }
        public Member FindBy(Team team, string email)
        {
            return this.FindOne(
                Expression.And(
                    Expression.Eq("TeamId", team.ID),
                    Expression.Eq("Email", email))
                );
        }
    }
}
