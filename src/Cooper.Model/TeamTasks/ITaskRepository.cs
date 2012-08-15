//Copyright (c) CodeSharp.  All rights reserved. - http://www.icodesharp.com/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using CodeSharp.Core.RepositoryFramework;

namespace Cooper.Model.Teams
{
    /// <summary>团队任务仓储
    /// </summary>
    public interface ITaskRepository : IRepository<long, Task>
    {
        IEnumerable<Task> FindBy(Team team);
        IEnumerable<Task> FindBy(Project project);
        IEnumerable<Task> FindBy(TeamMember teamMember);
    }
}
