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
    /// <summary>团队任务领域服务定义
    /// </summary>
    public interface ITaskService
    {
        /// <summary>创建任务
        /// </summary>
        /// <param name="task"></param>
        void Create(Task task);
        /// <summary>更新任务
        /// </summary>
        /// <param name="task"></param>
        void Update(Task task);
        /// <summary>删除任务
        /// </summary>
        /// <param name="task"></param>
        void Delete(Task task);
        /// <summary>根据标识获取任务
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        Task GetTask(long id);
        /// <summary>获取指定团队中分配给指定账号的或由该账号创建的任务
        /// </summary>
        /// <param name="team"></param>
        /// <param name="account"></param>
        /// <returns></returns>
        IEnumerable<Task> GetTasksByAccount(Team team, Account account);
        /// <summary>获取指定团队中分配给指定账号的或由该账号创建的未完成的任务
        /// </summary>
        /// <param name="team"></param>
        /// <param name="account"></param>
        /// <returns></returns>
        IEnumerable<Task> GetIncompletedTasksByAccount(Team team, Account account);
        /// <summary>获取指定团队中指定Tag相关的任务
        /// </summary>
        /// <param name="team"></param>
        /// <param name="tag"></param>
        /// <returns></returns>
        IEnumerable<Task> GetTasksByTag(Team team, string tag);
        /// <summary>获取指定团队中指定Tag相关的未完成的任务
        /// </summary>
        /// <param name="team"></param>
        /// <param name="tag"></param>
        /// <returns></returns>
        IEnumerable<Task> GetIncompletedTasksByTag(Team team, string tag);
        /// <summary>获取指定项目的所有任务
        /// </summary>
        /// <param name="project"></param>
        /// <returns></returns>
        IEnumerable<Task> GetTasksByProject(Project project);
        /// <summary>获取指定项目的所有未完成任务
        /// </summary>
        /// <param name="project"></param>
        /// <returns></returns>
        IEnumerable<Task> GetIncompletedTasksByProject(Project project);
        /// <summary>获取分配给指定团队成员的所有该团队成员所属团队的所有任务
        /// </summary>
        /// <param name="member"></param>
        /// <returns></returns>
        IEnumerable<Task> GetTasksByMember(Member member);
        /// <summary>获取分配给指定团队成员的所有该团队成员所属团队的所有未完成任务
        /// </summary>
        /// <param name="member"></param>
        /// <returns></returns>
        IEnumerable<Task> GetIncompletedTasksByMember(Member member);
        /// <summary>获取指定团队内已废弃的任务
        /// </summary>
        /// <param name="team"></param>
        /// <returns></returns>
        IEnumerable<Task> GetTrashedTasks(Team team);
    }
    /// <summary>团队任务领域服务
    /// </summary>
    [Transactional]
    public class TaskService : ITaskService
    {
        private static ITaskRepository _repository;
        private static ITeamRepository _teamRepository;
        private ILog _log;

        static TaskService()
        {
            _repository = RepositoryFactory.GetRepository<ITaskRepository, long, Task>();
            _teamRepository = RepositoryFactory.GetRepository<ITeamRepository, int, Team>();
        }
        public TaskService(ILoggerFactory factory)
        {
            this._log = factory.Create(typeof(TaskService));
        }

        #region ITaskService Members
        [Transaction(TransactionMode.Requires)]
        void ITaskService.Create(Task task)
        {
            _repository.Add(task);
            if (this._log.IsInfoEnabled)
                this._log.InfoFormat("新增团队任务#{0}|{1}|{2}", task.ID, task.Subject, task.CreatorMemberId);
        }
        [Transaction(TransactionMode.Requires)]
        void ITaskService.Update(Task task)
        {
            _repository.Update(task);
        }
        [Transaction(TransactionMode.Requires)]
        void ITaskService.Delete(Task task)
        {
            task.MarkAsTrashed();
            _repository.Update(task);
            if (this._log.IsInfoEnabled)
                this._log.InfoFormat("废弃团队任务#{0}", task.ID);
        }
        Task ITaskService.GetTask(long id)
        {
            return _repository.FindBy(id);
        }
        IEnumerable<Task> ITaskService.GetTasksByAccount(Team team, Account account)
        {
            return _repository.FindBy(team, account);
        }
        IEnumerable<Task> ITaskService.GetIncompletedTasksByAccount(Team team, Account account)
        {
            return _repository.FindBy(team, account, false);
        }
        IEnumerable<Task> ITaskService.GetTasksByTag(Team team, string tag)
        {
            return _repository.FindByTag(team, tag);
        }
        IEnumerable<Task> ITaskService.GetIncompletedTasksByTag(Team team, string tag)
        {
            return _repository.FindByTag(team, false, tag);
        }
        IEnumerable<Task> ITaskService.GetTasksByProject(Project project)
        {
            var team = _teamRepository.FindBy(project.TeamId);
            Assert.IsNotNull(team);
            return _repository.FindBy(team, project);
        }
        IEnumerable<Task> ITaskService.GetIncompletedTasksByProject(Project project)
        {
            var team = _teamRepository.FindBy(project.TeamId);
            Assert.IsNotNull(team);
            return _repository.FindBy(team, project, false);
        }
        IEnumerable<Task> ITaskService.GetTasksByMember(Member member)
        {
            var team = _teamRepository.FindBy(member.TeamId);
            Assert.IsNotNull(team);
            return _repository.FindBy(team, member);
        }
        IEnumerable<Task> ITaskService.GetIncompletedTasksByMember(Member member)
        {
            var team = _teamRepository.FindBy(member.TeamId);
            Assert.IsNotNull(team);
            return _repository.FindBy(team, member, false);
        }
        IEnumerable<Task> ITaskService.GetTrashedTasks(Team team)
        {
            return _repository.FindTrashedTasksBy(team);
        }
        #endregion
    }
}
