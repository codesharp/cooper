using System;
using System.Collections.Generic;
using CodeSharp.Core.RepositoryFramework;

namespace Cooper.Model.ContactGroups
{
    /// <summary>
    /// 联系人组仓储
    /// </summary>
    public interface IContactGroupRepository : IRepository<int, ContactGroup>
    {
    }
}