//Copyright (c) CodeSharp.  All rights reserved. - http://www.icodesharp.com/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using CodeSharp.Core.RepositoryFramework;
using Cooper.Model.Accounts;

namespace Cooper.Model.Teams
{
    /// <summary>团队任务仓储
    /// </summary>
    public interface ITaskRepository : IRepository<long, Task>
    {
        IEnumerable<Task> FindBy(Team team);
        IEnumerable<Task> FindBy(Team team, Account account);
        IEnumerable<Task> FindBy(Team team, Account account, bool isCompleted);
        IEnumerable<Task> FindBy(Project project);
        IEnumerable<Task> FindBy(Team team, Project project, Account account);
        IEnumerable<Task> FindBy(Team team, Project project, Account account, bool isCompleted);
        IEnumerable<Task> FindBy(Member member);
    }
}
