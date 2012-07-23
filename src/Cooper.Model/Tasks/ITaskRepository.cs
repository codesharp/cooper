﻿//Copyright (c) CodeSharp.  All rights reserved. - http://www.icodesharp.com/

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
    public interface ITaskRepository : IRepository<long, Task>
    {
        IEnumerable<Task> FindBy(Account account);
        IEnumerable<Task> FindBy(Account account, Tasklist tasklist);
        IEnumerable<Task> FindBy(Account account, bool isCompleted);
        IEnumerable<Task> FindBy(Account account, bool isCompleted, Tasklist tasklist);
    }
}
