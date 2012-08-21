//Copyright (c) CodeSharp.  All rights reserved. - http://www.icodesharp.com/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using CodeSharp.Core.RepositoryFramework;

namespace Cooper.Model.Teams
{
    /// <summary>团队成员仓储
    /// </summary>
    public interface IMemberRepository : IRepository<int, Member>
    {
        Member FindBy(Team team, string email);
    }
}
