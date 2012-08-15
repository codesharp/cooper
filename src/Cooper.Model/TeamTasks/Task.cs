//Copyright (c) CodeSharp.  All rights reserved. - http://www.icodesharp.com/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Cooper.Model.Teams
{
    /// <summary>团队任务核心模型
    /// </summary>
    public class Task : Cooper.Model.Tasks.Task
    {
        /// <summary>获取被分配的团队成员标识
        /// <remarks>
        /// 团队任务是面向团队成员进行分配，而不是直接分配到账号
        /// </remarks>
        /// </summary>
        public int? Assignee { get; private set; }
    }
}
