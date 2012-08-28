//Copyright (c) CodeSharp.  All rights reserved. - http://www.icodesharp.com/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using CodeSharp.Core.RepositoryFramework;
using Cooper.Model.Accounts;

namespace Cooper.Model.Teams
{
    /// <summary>团队仓储
    /// </summary>
    public interface ITeamRepository : IRepository<int, Team>
    {
        IEnumerable<Team> FindBy(Account account);
        Member FindMemberBy(Team team, string email);
        Member FindMemberBy(Team team, Account account);
        IEnumerable<Member> FindUnassociatedMembersBy(string email);
    }
}
