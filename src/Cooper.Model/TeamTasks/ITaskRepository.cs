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
        IEnumerable<Task> FindByTag(Team team, string tag);
        IEnumerable<Task> FindByTag(Team team, bool isCompleted, string tag);
        IEnumerable<Task> FindBy(Team team, Account account);
        IEnumerable<Task> FindBy(Team team, Account account, bool isCompleted);
        IEnumerable<Task> FindBy(Team team, Project project);
        IEnumerable<Task> FindBy(Team team, Project project, bool isCompleted);
        IEnumerable<Task> FindBy(Team team, Member member);
        IEnumerable<Task> FindBy(Team team, Member member, bool isCompleted);
        IEnumerable<Task> FindTrashedTasksBy(Team team);
        IEnumerable<Task> FindNotEmptyTagTasks(Team team);
    }
}
