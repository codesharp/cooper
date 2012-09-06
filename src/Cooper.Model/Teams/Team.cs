//Copyright (c) CodeSharp.  All rights reserved. - http://www.icodesharp.com/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using CodeSharp.Core.DomainBase;
using Cooper.Model.Accounts;

namespace Cooper.Model.Teams
{
    /// <summary>团队模型
    /// </summary>
    public class Team : EntityBase<int>, IAggregateRoot
    {
        private IList<Member> _members = new List<Member>();
        private IList<Project> _projects = new List<Project>();

        /// <summary>获取团队名称
        /// </summary>
        public string Name { get; private set; }
        /// <summary>创建时间
        /// </summary>
        public DateTime CreateTime { get; private set; }
        /// <summary>获取团队的所有成员
        /// </summary>
        public IEnumerable<Member> Members { get { return _members; } }
        /// <summary>获取团队的所有项目
        /// </summary>
        public IEnumerable<Project> Projects { get { return _projects; } }

        protected Team() { this.CreateTime = DateTime.Now; }
        public Team(string name) : this()
        {
            this.SetName(name);
        }

        /// <summary>根据成员标识获取成员
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public Member GetMember(int id)
        {
            return _members.SingleOrDefault(x => x.ID == id);
        }
        /// <summary>根据成员标识获取成员
        /// </summary>
        /// <param name="email"></param>
        /// <returns></returns>
        public Member GetMember(string email)
        {
            return _members.SingleOrDefault(x => x.Email == email);
        }
		/// <summary>根据账号获取成员
        /// </summary>
        /// <param name="account"></param>
        /// <returns></returns>
		public Member GetMember(Account account)
		{
			return this._members.SingleOrDefault (x => x.AssociatedAccountId != null && x.AssociatedAccountId.Value == account.ID);
		}
        /// <summary>根据项目标识获取项目
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public Project GetProject(int id)
        {
            return _projects.SingleOrDefault(x => x.ID == id);
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
        /// <summary>往团队中添加一个新成员
        /// </summary>
        /// <param name="member"></param>
        /// <returns></returns>
        internal void AddMember(Member member)
        {
            Assert.IsNotNull(member);
            Assert.IsValidKey(member.Email);
            Assert.IsValidKey(member.Name);
            Assert.AreEqual(this.ID, member.TeamId);
            Assert.IsFalse(_members.Any(x => x.Email == member.Email));

            _members.Add(member);
        }
        /// <summary>从团队中移除一个成员
        /// </summary>
        /// <param name="member"></param>
        internal void RemoveMember(Member member)
        {
            Assert.IsValid(member);
            Assert.AreEqual(this.ID, member.TeamId);
            Assert.IsTrue(IsMemberExist(member));
            _members.Remove(_members.Single(x => x.ID == member.ID));
        }
        /// <summary>往团队中添加一个新项目
        /// </summary>
        /// <param name="name"></param>
        internal Project AddProject(string name)
        {
            var project = new Project(name, this);
            _projects.Add(project);
            return project;
        }
        /// <summary>从团队中移除一个项目
        /// </summary>
        /// <param name="project"></param>
        internal void RemoveProject(Project project)
        {
            Assert.IsValid(project);
            Assert.AreEqual(this.ID, project.TeamId);
            var projectToRemove = _projects.SingleOrDefault(x => x.ID == project.ID);
            Assert.IsNotNull(projectToRemove);
            _projects.Remove(projectToRemove);
        }

        /// <summary>判断指定的团队成员是否在当前团队中存在
        /// <remarks>
        /// 成员ID或Email相同认为是同一个团队成员
        /// </remarks>
        /// </summary>
        /// <param name="member"></param>
        /// <returns></returns>
        public bool IsMemberExist(Member member)
        {
            return _members.Any(x => x.ID == member.ID) || _members.Any(x => x.Email == member.Email);
        }
    }
}
