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
        /// <param name="name"></param>
        /// <param name="email"></param>
        /// <param name="team"></param>
        /// <returns></returns>
        Member AddMember(string name, string email, Team team);
        /// <summary>新增一个指定的成员，并自动关联到指定账号
        /// <remarks>
        /// 团队内成员的Email和关联账号必须唯一，会做并发控制
        /// </remarks>
        /// </summary>
        /// <param name="name"></param>
        /// <param name="email"></param>
        /// <param name="team"></param>
        /// <param name="associateAccount"></param>
        /// <returns></returns>
        Member AddMember(string name, string email, Team team, Account associateAccount);
        /// <summary>新增一个指定的成员，并指定成员类型
        /// <remarks>
        /// 团队内成员的Email和关联账号必须唯一，会做并发控制
        /// </remarks>
        /// </summary>
        /// <param name="name"></param>
        /// <param name="email"></param>
        /// <param name="team"></param>
        /// <param name="memberType"></param>
        /// <returns></returns>
        Member AddMember(string name, string email, Team team, MemberType memberType);
        /// <summary>新增一个指定的成员，并指定成员类型和自动关联到指定账号
        /// <remarks>
        /// 团队内成员的Email和关联账号必须唯一，会做并发控制
        /// </remarks>
        /// </summary>
        /// <param name="name"></param>
        /// <param name="email"></param>
        /// <param name="team"></param>
        /// <param name="memberType"></param>
        /// <param name="associateAccount"></param>
        /// <returns></returns>
        Member AddMember(string name, string email, Team team, MemberType memberType, Account associateAccount);
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
        void AssociateMemberAccount(Member member, Account account);
        /// <summary>为团队成员取消关联账号
        /// </summary>
        /// <param name="member"></param>
        void UnAssociateMemberAccount(Member member);
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
        Member ITeamService.AddMember(string name, string email, Team team)
        {
            return (this as ITeamService).AddMember(name, email, team, null);
        }
        [Transaction(TransactionMode.Requires)]
        Member ITeamService.AddMember(string name, string email, Team team, Account associateAccount)
        {
            return (this as ITeamService).AddMember(name, email, team, MemberType.FullMember, associateAccount);
        }
        [Transaction(TransactionMode.Requires)]
        Member ITeamService.AddMember(string name, string email, Team team, MemberType memberType)
        {
            return (this as ITeamService).AddMember(name, email, team, memberType, null);
        }
        [Transaction(TransactionMode.Requires)]
        Member ITeamService.AddMember(string name, string email, Team team, MemberType memberType, Account associateAccount)
        {
            Assert.IsValidKey(name);
            Assert.IsValidKey(email);
            Assert.IsValid(team);
            if (associateAccount != null)
            {
                Assert.IsValid(associateAccount);
            }

            //HACK:发现在测试
            //TeamTest.AddTeamMemberWithDuplicateEmailTest
            //TeamTest.AddTeamMemberWithDuplicateAccountTest
            //TeamTest.AssociateTeamMemberWithDuplicateAccountTest
            //这三个并发测试的时候，如果这三个测试用例中的任何一个和TeamTest中的其他任何一个测试用例搭配，
            //并且将并发测试用例放后面，然后再TeamTest这个类的级别“Run Test”，会出现以下异常：
            //Illegal attempt to associate a collection with two open sessions
            //经过分析，原因是当前的AddMember方法接收的team参数是在外面的session创建的
            //然后在AddMember时，通过LazyLoad加载team.Members集合时，就会抛出上面的异常；
            //奇怪的是，这种异常只有在通过AssertParallel进行并发测试的时候才会出现，单个顺序的方式调用AddMember方法都不会出现该问题；
            //目前为了解决该问题又不影响功能，采用方法内部再获取属于当前Session的Team对象，然后通过获取到的team对象来AddMember；
            //其实通过测试发现，获取到的对象与外面传入的team对象的引用地址不同，也说明不是从同一个Session拿出来的；
            //最后，对于这个问题，还需要仔细分析，肯定还有更合理的解决方法
            var teamToAddMember = _teamRepository.FindBy(team.ID);

            //HACK:为了确保新增成员时，团队内成员的Email以及成员关联的账号都唯一，所以这里通过锁来进行同步控制
            this._locker.Require<Member>();
            Assert.IsNull(_teamRepository.FindMemberBy(teamToAddMember, email));
            if (associateAccount != null)
            {
                Assert.IsNull(_teamRepository.FindMemberBy(teamToAddMember, associateAccount));
            }

            var member = teamToAddMember.AddMember(name, email, memberType, associateAccount);
            _teamRepository.Update(teamToAddMember);

            return member;
        }
        [Transaction(TransactionMode.Requires)]
        void ITeamService.AssociateMemberAccount(Member member, Account account)
        {
            Assert.IsValid(member);
            Assert.IsValid(account);

            var team = _teamRepository.FindBy(member.TeamId);
            Assert.IsNotNull(team);
            var memberToAssociateAccount = team.GetMember(member.ID);
            Assert.IsNotNull(memberToAssociateAccount);

            //HACK:为了确保设置成员的关联账号时，账号必须在团队内唯一，所以这里通过锁来进行同步控制
            this._locker.Require<Member>();
            Assert.IsNull(_teamRepository.FindMemberBy(team, account));

            memberToAssociateAccount.Associate(account);
            _teamRepository.Update(team);
        }
        [Transaction(TransactionMode.Requires)]
        void ITeamService.UnAssociateMemberAccount(Member member)
        {
            Assert.IsValid(member);

            var team = _teamRepository.FindBy(member.TeamId);
            Assert.IsNotNull(team);
            var memberToUnAssociateAccount = team.GetMember(member.ID);
            Assert.IsNotNull(memberToUnAssociateAccount);

            memberToUnAssociateAccount.Associate(null);
            _teamRepository.Update(team);
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
        #endregion
    }
}
