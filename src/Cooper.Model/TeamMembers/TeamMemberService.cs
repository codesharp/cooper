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
    /// <summary>团队成员领域服务定义
    /// </summary>
    public interface ITeamMemberService
    {
        /// <summary>创建成员
        /// </summary>
        /// <param name="teamMember"></param>
        void Create(TeamMember teamMember);
        /// <summary>更新成员
        /// </summary>
        /// <param name="teamMember"></param>
        void Update(TeamMember teamMember);
        /// <summary>删除成员
        /// </summary>
        /// <param name="teamMember"></param>
        void Delete(TeamMember teamMember);
        /// <summary>根据标识获取成员
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        TeamMember GetTeamMember(int id);
        /// <summary>获取指定团队的所有成员
        /// </summary>
        /// <param name="team"></param>
        /// <returns></returns>
        IEnumerable<TeamMember> GetTeamMembersByTeam(Team team);
    }
    /// <summary>团队成员领域服务
    /// </summary>
    [Transactional]
    public class TeamMemberService : ITeamMemberService
    {
        private static ITeamMemberRepository _repository;
        private ILog _log;

        static TeamMemberService()
        {
            _repository = RepositoryFactory.GetRepository<ITeamMemberRepository, int, TeamMember>();
        }
        public TeamMemberService(ILoggerFactory factory)
        {
            this._log = factory.Create(typeof(TeamMemberService));
        }

        #region ITeamMemberService Members
        [Transaction(TransactionMode.Requires)]
        void ITeamMemberService.Create(TeamMember teamMember)
        {
            _repository.Add(teamMember);
            if (this._log.IsInfoEnabled)
                this._log.InfoFormat("新增团队成员#{0}|{1}|{2}", teamMember.ID, teamMember.Name, teamMember.Email);
        }
        [Transaction(TransactionMode.Requires)]
        void ITeamMemberService.Update(TeamMember teamMember)
        {
            _repository.Update(teamMember);
        }
        [Transaction(TransactionMode.Requires)]
        void ITeamMemberService.Delete(TeamMember teamMember)
        {
            _repository.Remove(teamMember);
            if (this._log.IsInfoEnabled)
                this._log.InfoFormat("删除团队成员#{0}", teamMember.ID);
        }
        TeamMember ITeamMemberService.GetTeamMember(int id)
        {
            return _repository.FindBy(id);
        }
        IEnumerable<TeamMember> ITeamMemberService.GetTeamMembersByTeam(Team team)
        {
            return _repository.FindBy(team);
        }
        #endregion
    }
}
