//Copyright (c) CodeSharp.  All rights reserved. - http://www.icodesharp.com/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using CodeSharp.Core.RepositoryFramework;
using Cooper.Model.Accounts;

namespace Cooper.Model.Tasks
{
    /// <summary>任务仓储
    /// </summary>
    public interface IPersonalTaskRepository : IRepository<long, PersonalTask>
    {
        IEnumerable<PersonalTask> FindBy(Account account);
        IEnumerable<PersonalTask> FindByTag(Account account, string tag);
        IEnumerable<PersonalTask> FindByTag(Account account, bool isCompleted, string tag);
        //folder为空则查找不属于任何folder的任务
        IEnumerable<PersonalTask> FindBy(Account account, TaskFolder folder);
        IEnumerable<PersonalTask> FindBy(Account account, bool isCompleted);
        IEnumerable<PersonalTask> FindBy(Account account, bool isCompleted, TaskFolder folder);
    }
}
