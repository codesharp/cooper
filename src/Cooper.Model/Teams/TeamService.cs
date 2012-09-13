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

        /// <summary>向团队添加一个FullMember
        /// <remarks>
        /// 团队内成员的Email和关联账号必须唯一，会做并发控制
        /// </remarks>
        /// </summary>
        /// <param name="name"></param>
        /// <param name="email"></param>
        /// <param name="team"></param>
        /// <returns></returns>
        FullMember AddFullMember(string name, string email, Team team);
        /// <summary>向团队添加一个FullMember，并自动关联到指定账号
        /// <remarks>
        /// 团队内成员的Email和关联账号必须唯一，会做并发控制
        /// </remarks>
        /// </summary>
        /// <param name="name"></param>
        /// <param name="email"></param>
        /// <param name="team"></param>
        /// <param name="associateAccount"></param>
        /// <returns></returns>
        FullMember AddFullMember(string name, string email, Team team, Account associateAccount);
        /// <summary>向团队添加一个GuestMember
        /// <remarks>
        /// 团队内成员的Email和关联账号必须唯一，会做并发控制
        /// </remarks>
        /// </summary>
        /// <param name="name"></param>
        /// <param name="email"></param>
        /// <param name="team"></param>
        /// <returns></returns>
        GuestMember AddGuestMember(string name, string email, Team team);
        /// <summary>向团队添加一个GuestMember，并自动关联到指定账号
        /// <remarks>
        /// 团队内成员的Email和关联账号必须唯一，会做并发控制
        /// </remarks>
        /// </summary>
        /// <param name="name"></param>
        /// <param name="email"></param>
        /// <param name="team"></param>
        /// <param name="associateAccount"></param>
        /// <returns></returns>
        GuestMember AddGuestMember(string name, string email, Team team, Account associateAccount);
        /// <summary>将一个指定的团队成员从团队移除
        /// </summary>
        /// <param name="member"></param>
        /// <param name="team"></param>
        void RemoveMember(Member member, Team team);
        /// <summary>为团队成员设置关联账号
        /// <remarks>
        /// 团队内成员的关联账号必须唯一，会做并发控制
        /// </remarks>
        /// </summary>
        /// <param name="member"></param>
        /// <param name="account"></param>
        void AssociateMemberAccount(Team team, Member member, Account account);
        /// <summary>为团队成员取消关联账号
        /// </summary>
        /// <param name="member"></param>
        void UnassociateMemberAccount(Team team, Member member);
        /// <summary>获取与指定邮箱相符的所有还未与Account建立关联的团队成员所属的团队
        /// </summary>
        /// <param name="email"></param>
        /// <returns></returns>
        IEnumerable<Team> GetUnassociatedTeams(string email);
        /// <summary>往团队新增一个项目
        /// </summary>
        /// <param name="name"></param>
        /// <param name="team"></param>
        /// <returns></returns>
        Project AddProject(string name, Team team);
        /// <summary>将一个指定的项目从团队移除
        /// </summary>
        /// <param name="project"></param>
        /// <param name="team"></param>
        void RemoveProject(Project project, Team team);
        /// <summary>获取团队内所有任务的所有不重复标签
        /// </summary>
        /// <param name="team"></param>
        /// <returns></returns>
        IEnumerable<string> GetTagsByTeam(Team team);
    }
    /// <summary>团队领域服务
    /// </summary>
    [Transactional]
    public class TeamService : ITeamService
    {
        private static ITeamRepository _teamRepository;
        private static ITaskRepository _taskRepository;
        private ILockHelper _locker;
        private ILog _log;

        static TeamService()
        {
            _teamRepository = RepositoryFactory.GetRepository<ITeamRepository, int, Team>();
            _taskRepository = RepositoryFactory.GetRepository<ITaskRepository, long, Task>();
        }
        public TeamService(ILoggerFactory factory, ILockHelper locker)
        {
            this._log = factory.Create(typeof(TeamService));
            this._locker = locker;
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
        FullMember ITeamService.AddFullMember(string name, string email, Team team)
        {
            return (this as ITeamService).AddFullMember(name, email, team, null);
        }
        [Transaction(TransactionMode.Requires)]
        FullMember ITeamService.AddFullMember(string name, string email, Team team, Account associateAccount)
        {
            return AddMemberToTeam(new FullMember(name, email, team), team, associateAccount) as FullMember;
        }
        [Transaction(TransactionMode.Requires)]
        GuestMember ITeamService.AddGuestMember(string name, string email, Team team)
        {
            return (this as ITeamService).AddGuestMember(name, email, team, null);
        }
        [Transaction(TransactionMode.Requires)]
        GuestMember ITeamService.AddGuestMember(string name, string email, Team team, Account associateAccount)
        {
            return AddMemberToTeam(new GuestMember(name, email, team), team, associateAccount) as GuestMember;
        }
        [Transaction(TransactionMode.Requires)]
        void ITeamService.AssociateMemberAccount(Team team, Member member, Account account)
        {
            Assert.IsValid(team);
            Assert.IsValid(member);
            Assert.IsValid(account);
            var memberToAssociateAccount = team.GetMember(member.ID);
            Assert.IsNotNull(memberToAssociateAccount);

            //HACK:为了确保设置成员的关联账号时，账号必须在团队内唯一，所以这里通过锁来进行同步控制
            this._locker.Require<Member>();
            Assert.IsNull(_teamRepository.FindMemberBy(team, account));

            memberToAssociateAccount.Associate(account);
            _teamRepository.Update(team);

            _log.InfoFormat("将团队【id:{0},name:{1}】中的成员【id:{2},name:{3},email:{4}】与账号【id:{5},name:{6}】建立了关联",
                team.ID,
                team.Name,
                member.ID,
                member.Name,
                member.Email,
                account.ID,
                account.Name);
        }
        [Transaction(TransactionMode.Requires)]
        void ITeamService.UnassociateMemberAccount(Team team, Member member)
        {
            Assert.IsValid(team);
            Assert.IsValid(member);

            var memberToUnAssociateAccount = team.GetMember(member.ID);
            Assert.IsNotNull(memberToUnAssociateAccount);

            memberToUnAssociateAccount.Associate(null);
            _teamRepository.Update(team);

            _log.InfoFormat("团队【id:{0},name:{1}】中的成员【id:{2},name:{3},email:{4}】的账号关联信息被取消",
                team.ID,
                team.Name,
                member.ID,
                member.Name,
                member.Email);
        }
        [Transaction(TransactionMode.Requires)]
        void ITeamService.RemoveMember(Member member, Team team)
        {
            Assert.IsValid(member);
            Assert.IsValid(team);

            //将团队成员从团队中移除
            team.RemoveMember(member);
            _teamRepository.Update(team);

            //将团队内由该成员发表的所有评论的作者信息清空
            var teamTasks = _taskRepository.FindBy(team);
            foreach (var task in teamTasks)
            {
                task.Comments
                    .Where(x => x.Creator != null && x.Creator.ID == member.ID)
                    .ForEach(comment => comment.SetCreatorAsNull());
                _taskRepository.Update(task);
            }

            //将分配给团队成员的所有任务收回
            var memberTasks = _taskRepository.FindBy(team, member);
            foreach (var task in memberTasks)
            {
                task.RemoveAssignee();
                _taskRepository.Update(task);
            }
        }
        IEnumerable<Team> ITeamService.GetUnassociatedTeams(string email)
        {
            Assert.IsNotNullOrWhiteSpace(email);
            return _teamRepository.FindUnassociatedTeamsBy(email);
        }
        [Transaction(TransactionMode.Requires)]
        Project ITeamService.AddProject(string name, Team team)
        {
            Assert.IsValidKey(name);
            Assert.IsValid(team);

            var project = team.AddProject(name);
            _teamRepository.Update(team);

            return project;
        }
        [Transaction(TransactionMode.Requires)]
        void ITeamService.RemoveProject(Project project, Team team)
        {
            Assert.IsValid(project);
            Assert.IsValid(team);

            //将团项目从团队中移除
            team.RemoveProject(project);
            _teamRepository.Update(team);

            //将与该项目相关的所有任务对该项目解除关联
            var projectTasks = _taskRepository.FindBy(team, project);
            foreach (var task in projectTasks)
            {
                task.RemoveFromProject(project);
                _taskRepository.Update(task);
            }
        }
        IEnumerable<string> ITeamService.GetTagsByTeam(Team team)
        {
            var tasks = _taskRepository.FindNotEmptyTagTasks(team);
            var tags = new List<string>();

            foreach (var task in tasks)
            {
                foreach (var tag in task.Tags)
                {
                    if (!tags.Any(x => StringHelper.CompareStringIgnoreCaseAndWidth(x, tag) == 0))
                    {
                        tags.Add(tag);
                    }
                }
            }

            return tags;
        }
        #endregion

        [Transaction(TransactionMode.Requires)]
        protected virtual Member AddMemberToTeam(Member member, Team team, Account associatedAccount)
        {
            Assert.IsNotNull(member);
            Assert.IsValidKey(member.Email);
            Assert.IsValidKey(member.Name);
            Assert.IsValid(team);
            Assert.AreEqual(team.ID, member.TeamId);

            //HACK:为了确保新增成员时，团队内成员的Email以及成员关联的账号都唯一，所以这里通过锁来进行同步控制
            this._locker.Require<Member>();
            Assert.IsNull(_teamRepository.FindMemberBy(team, member.Email));
            if (associatedAccount != null)
            {
                Assert.IsNull(_teamRepository.FindMemberBy(team, associatedAccount));
                member.Associate(associatedAccount);
            }

            team.AddMember(member);
            _teamRepository.Update(team);

            return member;
        }
    }
}
