//Copyright (c) CodeSharp.  All rights reserved. - http://www.icodesharp.com/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using CodeSharp.Core.RepositoryFramework;

namespace Cooper.Model.Tasks
{
    /// <summary>任务列表仓储
    /// </summary>
    public interface ITasklistRepository : IRepository<int, Tasklist>
    {
        IEnumerable<PersonalTasklist> FindBy(Accounts.Account account);
    }
}
