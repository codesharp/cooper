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
        /// <summary>获取所属团队的标识
        /// </summary>
        public int TeamId { get; private set; }
        /// <summary>创建时间
        /// </summary>
        public DateTime CreateTime { get; private set; }

        protected TeamMember() { this.CreateTime = DateTime.Now; }
        public TeamMember(string name, string email, Team team) : this()
        {
            Assert.IsValid(team);
            this.SetName(name);
            this.SetEmail(email);
            this.TeamId = team.ID;
        }

        /// <summary>设置名称
        /// <remarks>
        /// 长度应小于100
        /// </remarks>
        /// </summary>
        public void SetName(string name)
        {
            Assert.IsNotNullOrWhiteSpace(name);
            Assert.LessOrEqual(name.Length, 100);

            if (this.Name != name)
            {
                this.Name = name;
            }
        }
        /// <summary>设置Email
        /// <remarks>
        /// 长度应小于100
        /// </remarks>
        /// </summary>
        public void SetEmail(string email)
        {
            Assert.IsNotNullOrWhiteSpace(email);
            Assert.LessOrEqual(email.Length, 100);

            if (this.Email != email)
            {
                this.Email = email;
            }
        }
    }
}
