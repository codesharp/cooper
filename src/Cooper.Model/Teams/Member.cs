//Copyright (c) CodeSharp.  All rights reserved. - http://www.icodesharp.com/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using CodeSharp.Core.DomainBase;
using Cooper.Model.Accounts;

namespace Cooper.Model.Teams
{
    /// <summary>团队成员模型
    /// </summary>
    public class Member : EntityBase<int>
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
        /// <summary>获取关联账号的标识
        /// </summary>
        public int? AssociatedAccountId { get; private set; }
        /// <summary>创建时间
        /// </summary>
        public DateTime CreateTime { get; private set; }

        protected Member() { this.CreateTime = DateTime.Now; }
        internal Member(string name, string email, Team team) : this()
        {
            Assert.IsValid(team);
            this.SetName(name);
            this.SetEmail(email);
            this.TeamId = team.ID;
        }

        /// <summary>设置名称
        /// <remarks>
        /// 长度应小于255
        /// </remarks>
        /// </summary>
        public void SetName(string name)
        {
            Assert.IsValidKey(name);

            if (this.Name != name)
            {
                this.Name = name;
            }
        }
        /// <summary>设置Email
        /// <remarks>
        /// 长度应小于255
        /// </remarks>
        /// </summary>
        internal void SetEmail(string email)
        {
            Assert.IsValidKey(email);

            if (this.Email != email)
            {
                this.Email = email;
            }
        }
        /// <summary>设置关联账号
        /// </summary>
        /// <param name="account"></param>
        public void Associate(Account account)
        {
            if (account != null)
            {
                Assert.IsValid(account);
                this.AssociatedAccountId = account.ID;
            }
            else
            {
                this.AssociatedAccountId = null;
            }
        }
    }
}
