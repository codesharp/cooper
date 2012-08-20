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
        internal void AddMember(Member member)
        {
            Assert.AreEqual(0, member.ID);
            Assert.AreEqual(this.ID, member.TeamId);
            Assert.IsFalse(IsMemberExist(member));
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
        /// <param name="project"></param>
        internal void AddProject(Project project)
        {
            Assert.AreEqual(0, project.ID);
            Assert.AreEqual(this.ID, project.TeamId);
            _projects.Add(project);
        }
        /// <summary>从团队中移除一个项目
        /// </summary>
        /// <param name="project"></param>
        internal void RemoveProject(Project project)
        {
            Assert.IsValid(project);
            Assert.AreEqual(this.ID, project.TeamId);
            _projects.Remove(_projects.Single(x => x.ID == project.ID));
        }

        /// <summary>判断指定的团队成员是否和团队内的其他成员的Email重复
        /// </summary>
        /// <param name="member"></param>
        /// <returns></returns>
        public bool IsMemberEmailDuplicated(Member member)
        {
            foreach (var existingMember in _members)
            {
                if (existingMember.ID != member.ID && existingMember.Email == member.Email)
                {
                    return true;
                }
            }
            return false;
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
