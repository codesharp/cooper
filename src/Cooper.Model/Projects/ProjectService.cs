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
    /// <summary>团队项目领域服务定义
    /// </summary>
    public interface IProjectService
    {
        /// <summary>创建项目
        /// </summary>
        /// <param name="project"></param>
        void Create(Project project);
        /// <summary>更新项目
        /// </summary>
        /// <param name="project"></param>
        void Update(Project project);
        /// <summary>删除项目
        /// </summary>
        /// <param name="project"></param>
        void Delete(Project project);
        /// <summary>根据标识获取项目
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        Project GetProject(int id);
        /// <summary>获取指定团队的所有项目
        /// </summary>
        /// <param name="team"></param>
        /// <returns></returns>
        IEnumerable<Project> GetProjectsByTeam(Team team);
    }
    /// <summary>团队项目领域服务
    /// </summary>
    [Transactional]
    public class ProjectService : IProjectService
    {
        private static IProjectRepository _repository;
        private ILog _log;

        static ProjectService()
        {
            _repository = RepositoryFactory.GetRepository<IProjectRepository, int, Project>();
        }
        public ProjectService(ILoggerFactory factory)
        {
            this._log = factory.Create(typeof(ProjectService));
        }

        #region IProjectService Members
        [Transaction(TransactionMode.Requires)]
        void IProjectService.Create(Project project)
        {
            _repository.Add(project);
            if (this._log.IsInfoEnabled)
                this._log.InfoFormat("新增团队项目#{0}|{1}", project.ID, project.Name);
        }
        [Transaction(TransactionMode.Requires)]
        void IProjectService.Update(Project project)
        {
            _repository.Update(project);
        }
        [Transaction(TransactionMode.Requires)]
        void IProjectService.Delete(Project project)
        {
            _repository.Remove(project);
            if (this._log.IsInfoEnabled)
                this._log.InfoFormat("删除团队项目#{0}", project.ID);
        }
        Project IProjectService.GetProject(int id)
        {
            return _repository.FindBy(id);
        }
        IEnumerable<Project> IProjectService.GetProjectsByTeam(Team team)
        {
            return _repository.FindBy(team);
        }
        #endregion
    }
}
