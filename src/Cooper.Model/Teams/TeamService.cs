//Copyright (c) CodeSharp.  All rights reserved. - http://www.icodesharp.com/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using CodeSharp.Core.RepositoryFramework;
using Castle.Services.Transaction;
using Cooper.Model.Accounts;
using CodeSharp.Core;
using CodeSharp.Core.Services;

namespace Cooper.Model.Teams
{
    /// <summary>团队领域服务定义
    /// </summary>
    public interface ITeamService
    {
        /// <summary>创建团队
        /// </summary>
        /// <param name="team"></param>
        void Create(Team team);
        /// <summary>更新团队
        /// </summary>
        /// <param name="team"></param>
        void Update(Team team);
        /// <summary>删除团队
        /// </summary>
        /// <param name="team"></param>
        void Delete(Team team);
        /// <summary>根据标识获取团队
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        Team GetTeam(int id);
        /// <summary>获取指定账号的所有相关团队
        /// </summary>
        /// <param name="account"></param>
        /// <returns></returns>
        IEnumerable<Team> GetTeamsByAccount(Account account);

        /// <summary>将一个指定的团队成员添加到团队
        /// </summary>
        /// <param name="member"></param>
        void AddMember(TeamMember member);
        /// <summary>更新一个指定的团队成员
        /// </summary>
        /// <param name="member"></param>
        void UpdateMember(TeamMember member);
        /// <summary>将一个指定的团队成员从团队移除
        /// </summary>
        /// <param name="member"></param>
        void RemoveMember(TeamMember member);
        /// <summary>根据标识获取成员
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        TeamMember GetMember(int id);

        /// <summary>将一个指定的项目添加到团队
        /// </summary>
        /// <param name="project"></param>
        void AddProject(Project project);
        /// <summary>更新一个指定的项目
        /// </summary>
        /// <param name="project"></param>
        void UpdateProject(Project project);
        /// <summary>将一个指定的项目从团队移除
        /// </summary>
        /// <param name="project"></param>
        void RemoveProject(Project project);
        /// <summary>根据标识获取项目
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        Project GetProject(int id);
    }
    /// <summary>团队领域服务
    /// </summary>
    [Transactional]
    public class TeamService : ITeamService
    {
        private static ITeamRepository _teamRepository;
        private static ITaskRepository _taskRepository;
        private static ITeamMemberRepository _memberRepository;
        private static IProjectRepository _projectRepository;
        private ILog _log;

        static TeamService()
        {
            _teamRepository = RepositoryFactory.GetRepository<ITeamRepository, int, Team>();
            _memberRepository = RepositoryFactory.GetRepository<ITeamMemberRepository, int, TeamMember>();
            _taskRepository = RepositoryFactory.GetRepository<ITaskRepository, long, Task>();
            _projectRepository = RepositoryFactory.GetRepository<IProjectRepository, int, Project>();
        }
        public TeamService(ILoggerFactory factory)
        {
            this._log = factory.Create(typeof(TeamService));
        }

        #region ITeamService Members
        [Transaction(TransactionMode.Requires)]
        void ITeamService.Create(Team team)
        {
            _teamRepository.Add(team);
            if (this._log.IsInfoEnabled)
                this._log.InfoFormat("新增团队#{0}|{1}", team.ID, team.Name);
        }
        [Transaction(TransactionMode.Requires)]
        void ITeamService.Update(Team team)
        {
            _teamRepository.Update(team);
        }
        [Transaction(TransactionMode.Requires)]
        void ITeamService.Delete(Team team)
        {
            _teamRepository.Remove(team);
            if (this._log.IsInfoEnabled)
                this._log.InfoFormat("删除团队#{0}", team.ID);
        }
        Team ITeamService.GetTeam(int id)
        {
            return _teamRepository.FindBy(id);
        }
        IEnumerable<Team> ITeamService.GetTeamsByAccount(Account account)
        {
            return _teamRepository.FindBy(account);
        }

        [Transaction(TransactionMode.Requires)]
        void ITeamService.AddMember(TeamMember member)
        {
            var team = _teamRepository.FindBy(member.TeamId);
            team.AddMember(member);
            _teamRepository.Update(team);
        }
        [Transaction(TransactionMode.Requires)]
        void ITeamService.UpdateMember(TeamMember member)
        {
            Assert.IsValid(member);
            var team = _teamRepository.FindBy(member.TeamId);
            Assert.IsFalse(team.IsMemberEmailDuplicated(member));
            _memberRepository.Update(member);
        }
        [Transaction(TransactionMode.Requires)]
        void ITeamService.RemoveMember(TeamMember member)
        {
            //先将团队成员从团队中移除
            var team = _teamRepository.FindBy(member.TeamId);
            team.RemoveMember(member);
            _teamRepository.Update(team);
            _memberRepository.Remove(member);

            //再将分配给团队成员的所有任务收回
            foreach (var task in member.AssignedTasks)
            {
                task.RemoveAssignee();
                _taskRepository.Update(task);
            }
        }
        TeamMember ITeamService.GetMember(int id)
        {
            return _memberRepository.FindBy(id);
        }

        [Transaction(TransactionMode.Requires)]
        void ITeamService.AddProject(Project project)
        {
            var team = _teamRepository.FindBy(project.TeamId);
            team.AddProject(project);
            _teamRepository.Update(team);
        }
        [Transaction(TransactionMode.Requires)]
        void ITeamService.UpdateProject(Project project)
        {
            Assert.IsValid(project);
            var team = _teamRepository.FindBy(project.TeamId);
            _projectRepository.Update(project);
        }
        [Transaction(TransactionMode.Requires)]
        void ITeamService.RemoveProject(Project project)
        {
            var team = _teamRepository.FindBy(project.TeamId);
            team.RemoveProject(project);
            _teamRepository.Update(team);
            _projectRepository.Remove(project);
        }
        Project ITeamService.GetProject(int id)
        {
            return _projectRepository.FindBy(id);
        }
        #endregion
    }
}
