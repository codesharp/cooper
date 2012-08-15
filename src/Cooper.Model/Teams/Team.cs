//Copyright (c) CodeSharp.  All rights reserved. - http://www.icodesharp.com/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using CodeSharp.Core.DomainBase;

namespace Cooper.Model.Teams
{
    /// <summary>团队模型
    /// </summary>
    public class Team : EntityBase<int>, IAggregateRoot
    {
        /// <summary>获取团队名称
        /// </summary>
        public string Name { get; private set; }
        /// <summary>创建时间
        /// </summary>
        public DateTime CreateTime { get; private set; }

        protected Team() { this.CreateTime = DateTime.Now; }
        public Team(string name) : this()
        {
            this.SetName(name);
        }

        /// <summary>设置名称
        /// <remarks>
        /// 长度应小于200
        /// </remarks>
        /// </summary>
        public void SetName(string name)
        {
            Assert.IsNotNullOrWhiteSpace(name);
            Assert.LessOrEqual(name.Length, 200);

            if (this.Name != name)
            {
                this.Name = name;
            }
        }
    }
}
