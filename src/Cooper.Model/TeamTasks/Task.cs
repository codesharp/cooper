//Copyright (c) CodeSharp.  All rights reserved. - http://www.icodesharp.com/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Cooper.Model.Accounts;

namespace Cooper.Model.Teams
{
    /// <summary>团队任务核心模型
    /// </summary>
    public class Task : Cooper.Model.Tasks.Task
    {
        private IList<Project> _projects = new List<Project>();
        private IList<Comment> _comments = new List<Comment>();

        /// <summary>获取所属团队的标识
        /// </summary>
        public int TeamId { get; private set; }
        /// <summary>获取当前任务分配给的团队成员的标识
        /// <remarks>
        /// 团队任务是面向团队成员进行分配，而不是直接分配到账号
        /// </remarks>
        /// </summary>
        public int? AssigneeId { get; private set; }
        /// <summary>获取当前团队任务所属的项目
        /// <remarks>
        /// 一个团队任务可以属于多个项目
        /// </remarks>
        /// </summary>
        public IEnumerable<Project> Projects { get { return _projects; } }
        /// <summary>获取当前团队任务的所有评论
        /// </summary>
        public IEnumerable<Comment> Comments { get { return _comments; } }

        protected Task() : base()
        { }
        public Task(Account creator, Team team) : base(creator)
        {
            Assert.IsValid(creator);
            Assert.IsValid(team);
            this.TeamId = team.ID;
        }

        /// <summary>将任务分配给指定的团队成员
        /// </summary>
        public void AssignTo(Member member)
        {
            Assert.IsValid(member);
            Assert.AreEqual(this.TeamId, member.TeamId);
            this.AssigneeId = member.ID;
        }
        /// <summary>移除当前任务的Assignee
        /// </summary>
        public void RemoveAssignee()
        {
            this.AssigneeId = null;
        }
        /// <summary>将任务添加到指定项目
        /// <remarks>
        /// 如果任务已经在该项目，则不做处理。
        /// </remarks>
        /// </summary>
        public void AddToProject(Project project)
        {
            Assert.IsValid(project);
            Assert.AreEqual(this.TeamId, project.TeamId);
            if (!_projects.Any(x => x.ID == project.ID))
            {
                _projects.Add(project);
            }
        }
        /// <summary>将任务从指定项目移除
        /// <remarks>
        /// 如果任务不在该项目中，则不做处理
        /// </remarks>
        /// </summary>
        public void RemoveFromProject(Project project)
        {
            Assert.IsValid(project);
            Assert.AreEqual(this.TeamId, project.TeamId);
            var projectToRemove = _projects.SingleOrDefault(x => x.ID == project.ID);
            if (projectToRemove != null)
            {
                _projects.Remove(projectToRemove);
            }
        }
        /// <summary>根据评论标识获取评论
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public Comment GetComment(int id)
        {
            return _comments.SingleOrDefault(x => x.ID == id);
        }
        /// <summary>添加评论
        /// </summary>
        /// <param name="creator"></param>
        /// <param name="body"></param>
        public void AddComment(Member creator, string body)
        {
            Assert.IsValid(creator);
            Assert.AreEqual(this.TeamId, creator.TeamId);
            Assert.IsNotNullOrWhiteSpace(body);
            _comments.Add(new Comment(creator, body));
        }
        /// <summary>移除评论
        /// </summary>
        /// <param name="comment"></param>
        public void RemoveComment(Comment comment)
        {
            Assert.IsValid(comment);
            var commentToRemove = _comments.SingleOrDefault(x => x.ID == comment.ID);
            if (commentToRemove != null)
            {
                _comments.Remove(commentToRemove);
            }
        }
    }
}
