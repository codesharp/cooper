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
    /// <remarks>
    /// 团队成员在目前的Cooper模型中，完全由团队模型管理，团队成员的概念只存在于团队上下文。
    /// 而目前仍然将团队成员设计为独立聚合根，是因为团队成员会直接和任务关联；
    /// 考虑到聚合根不能引用其他聚合根内的实体，所以将团队成员设计为聚合根；
    /// </remarks>
    /// </summary>
    public class Member : EntityBase<int>, IAggregateRoot
    {
        private IList<Task> _assignedTasks = new List<Task>();

        /// <summary>获取团队成员显示的名字
        /// </summary>
        public string Name { get; private set; }
        /// <summary>获取团队成员的Email
        /// </summary>
        public string Email { get; private set; }
        /// <summary>获取所属团队的标识
        /// </summary>
        public int TeamId { get; private set; }
        /// <summary>获取分配给当前团队成员的任务
        /// </summary>
        public IEnumerable<Task> AssignedTasks { get { return _assignedTasks; } }
        /// <summary>获取关联账号的标识
        /// </summary>
        public int? AssociatedAccountId { get; private set; }
        /// <summary>创建时间
        /// </summary>
        public DateTime CreateTime { get; private set; }

        protected Member() { this.CreateTime = DateTime.Now; }
        public Member(string name, string email, Team team) : this()
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
        public void SetEmail(string email)
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
        public void SetAssociateAccount(Account account)
        {
            Assert.IsValid(account);
            this.AssociatedAccountId = account.ID;
        }
    }
}
