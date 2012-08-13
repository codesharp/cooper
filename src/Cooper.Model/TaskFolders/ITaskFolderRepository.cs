//Copyright (c) CodeSharp.  All rights reserved. - http://www.icodesharp.com/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using CodeSharp.Core.RepositoryFramework;

namespace Cooper.Model.Tasks
{
    public interface ITaskFolderRepository : IRepository<int, TaskFolder>
    {
        IEnumerable<PersonalTaskFolder> FindBy(Accounts.Account account);
    }
}
