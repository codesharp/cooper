//Copyright (c) CodeSharp.  All rights reserved. - http://www.icodesharp.com/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using CodeSharp.Core.DomainBase;

namespace Cooper.Model.Teams
{
    /// <summary>团队项目模型
    /// </summary>
    public class Project : EntityBase<int>, IAggregateRoot
    {
        /// <summary>获取团队项目名称
        /// </summary>
        public string Name { get; private set; }
    }
}
