//Copyright (c) CodeSharp.  All rights reserved. - http://www.icodesharp.com/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using CodeSharp.Core.DomainBase;

namespace Cooper.Model.Teams
{
    /// <summary>团队成员模型
    /// </summary>
    public class TeamMember : EntityBase<int>, IAggregateRoot
    {
        /// <summary>获取团队成员显示的名字
        /// </summary>
        public string Name { get; private set; }
        /// <summary>获取团队成员的Email
        /// </summary>
        public string Email { get; private set; }
    }
}
