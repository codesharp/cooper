//Copyright (c) CodeSharp.  All rights reserved. - http://www.icodesharp.com/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using CodeSharp.Core.RepositoryFramework;

namespace Cooper.Model.Teams
{
    /// <summary>团队项目仓储
    /// </summary>
    public interface IProjectRepository : IRepository<int, Project>
    {
    }
}
