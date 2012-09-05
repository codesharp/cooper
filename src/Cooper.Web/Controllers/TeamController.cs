﻿//Copyright (c) CodeSharp.  All rights reserved. - http://www.icodesharp.com/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using CodeSharp.Core.Services;
using Cooper.Model.Accounts;
using Cooper.Model.Tasks;
using Teams=Cooper.Model.Teams;

namespace Cooper.Web.Controllers
{
    /// <summary>团队
    /// </summary>
    public class TeamController : TaskController
    {
        private Teams.ITeamService _teamService;
        private Teams.ITaskService _teamTaskService;
        protected IAccountConnectionService _accountConnectionService;

        public TeamController(ILoggerFactory factory
            , IAccountService accountService
            , IAccountConnectionService accountConnectionService
            , ITaskService taskService
            , ITaskFolderService taskFolderService
            , IFetchTaskHelper fetchTaskHelper
            , Teams.ITeamService teamService
            , Teams.ITaskService teamTaskService)
            : base(factory
            , accountService
            , taskService
            , taskFolderService
            , fetchTaskHelper)
        {
            this._accountConnectionService = accountConnectionService;
            this._teamService = teamService;
            this._teamTaskService = teamTaskService;
        }

        public ActionResult Index(string teamId, string projectId, string memberId)
        {
            ViewBag.Account = this.Parse(this.Context.Current);
            ViewBag.TeamId = teamId;
            ViewBag.ProjectId = projectId;
            ViewBag.MemberId = memberId;
            return View();
        }

        #region 各种列表排序模式
        //优先级列表模式 所有任务
        [HttpPost]
        public ActionResult GetByPriority(string teamId, string projectId, string memberId)
        {
            return this.GetBy(teamId
                , projectId
                , memberId
                , this.GetTasksByAccount
                , this.GetTasksByMember
                , this.GetTasksByProject
                , this.ParseSortsByPriority
                , this.ParseSortsByPriority);
        }
        //优先级列表模式 不含已经完成
        [HttpPost]
        public ActionResult GetIncompletedByPriority(string teamId, string projectId, string memberId)
        {
            return this.GetBy(teamId
                , projectId
                , memberId
                , this.GetIncompletedTasksByAccount
                , this.GetIncompletedTasksByMember
                , this.GetIncompletedTasksByProject
                , this.ParseSortsByPriority
                , this.ParseSortsByPriority);
        }
        //DueTime列表模式
        [HttpPost]
        public ActionResult GetByDueTime(string teamId, string projectId, string memberId)
        {
            return this.GetBy(teamId
                , projectId
                , memberId
                , this.GetTasksByAccount
                , this.GetTasksByMember
                , this.GetTasksByProject
                , this.ParseSortsByDueTime
                , this.ParseSortsByDueTime);
        }
        //UNDONE:Assignee排序模式
        [HttpPost]
        public ActionResult GetByAssignee(string teamId, string projectId)
        {
            return this.GetBy(teamId
                , projectId
                , null
                , this.GetTasksByAccount
                , this.GetTasksByMember
                , this.GetTasksByProject
                , this.ParseSortsByDueTime
                , this.ParseSortsByDueTime);
        }
        #endregion

        #region 团队相关api 只有团队FullMember可执行变更操作
        [HttpGet]
        public ActionResult GetTeams()
        {
            return Json(this._teamService.GetTeamsByAccount(this.Context.Current).Select(o => this.Parse(o)), JsonRequestBehavior.AllowGet);
        }
        [HttpPost]
        public ActionResult CreateTeam(string name, string memberName, string memberEmail)
        {
            var a = this.Context.Current;
            var t = new Teams.Team(name);
            this._teamService.Create(t);

            //HACK:创建team同时将当前用户加入到该team
            var m = this._teamService.AddFullMember(
                !string.IsNullOrWhiteSpace(memberName) ? memberName : a.Name
                , !string.IsNullOrWhiteSpace(memberEmail) ? memberEmail : memberEmail
                , t, a);
            return Json(t.ID);
        }
        [HttpPut]
        public ActionResult UpdateTeam(string id, string name)
        {
            var t = this.GetTeamOfFullMember(id);
            t.SetName(name);
            this._teamService.Update(t);
            return Json(true);
        }
        [HttpPost]
        public ActionResult CreateProject(string teamId, string name)
        {
            return Json(this._teamService.AddProject(name, this.GetTeamOfFullMember(teamId)).ID);
        }
        [HttpPut]
        public ActionResult UpdateProject(string teamId, string projectId, string name)
        {
            var t = this.GetTeamOfFullMember(teamId);
            var p = this.GetProject(t, projectId);
            p.SetName(name);
            this._teamService.Update(t);
            return Json(true);
        }
        [HttpPost]
        public ActionResult DeleteProject(string teamId, string projectId)
        {
            var t = this.GetTeamOfFullMember(teamId);
            var p = this.GetProject(t, projectId);
            this._teamService.RemoveProject(p, t);
            return Json(true);
        }
        [HttpPost]
        public ActionResult CreateMember(string teamId, string name, string email)
        {
            //HACK:创建Member时自动根据Email获取账号进行关联
            var c = this.GetDefaultConnectionByEmail(email);
            var a = c != null
                ? this._accountService.GetAccount(c.AccountId)
                : null;
            return Json(a != null
                ? this._teamService.AddFullMember(name, email, this.GetTeamOfFullMember(teamId), a).ID
                : this._teamService.AddFullMember(name, email, this.GetTeamOfFullMember(teamId)).ID);
        }
        [HttpPut]
        public ActionResult UpdateMember(string teamId, string memberId, string name, string email)
        {
            var t = this.GetTeamOfFullMember(teamId);
            var m = this.GetCurrentMember(t);//UNDONE:只允许修改自己的？
            m.SetName(name);
            //UNDONE:还未支持修改Member的Email
            this._teamService.Update(t);
            return Json(true);
        }
        [HttpPost]//[HttpDelete]//需要路由支持 team/{teamId}/member/{memberId}
        public ActionResult DeleteMember(string teamId, string memberId)
        {
            var t = this.GetTeamOfFullMember(teamId);
            var m = this.GetMember(t, memberId);
            this._teamService.RemoveMember(m, t);
            return Json(true);
        }
        #endregion

        /// <summary>用于接收终端的变更同步数据，按team同步
        /// </summary>
        /// <param name="teamId">团队标识</param>
        /// <param name="projectId">项目标识，可以为空</param>
        /// <param name="memberId">成员标识，可以为空</param>
        /// <param name="changes"></param>
        /// <param name="by"></param>
        /// <param name="sorts"></param>
        /// <returns></returns>
        [HttpPost]
        [ValidateInput(false)]
        public ActionResult Sync(string teamId, string projectId, string memberId, string changes, string by, string sorts)
        {
            var a = this.Context.Current;
            var team = this.GetTeamOfCurrentAccount(teamId);
            var project = string.IsNullOrWhiteSpace(projectId) ? null : this.GetProject(team, projectId);
            var member = string.IsNullOrWhiteSpace(memberId) ? null : this.GetMember(team, memberId);
            var currentMember = this.GetCurrentMember(team);

            return Json(this.Sync(changes, by, sorts
                , () =>
                {
                    var t = new Teams.Task(currentMember, team);
                    if (project != null)
                        t.AddToProject(project);
                    if (member != null)
                        t.AssignTo(member);
                    return t;
                }
                , o =>
                {
                    var task = o as Teams.Task;
                    //创建者或执行人
                    Assert.IsTrue(this.IsCreator(team, task, a) || this.IsAssignee(task, currentMember));
                }
                , () => project == null && string.IsNullOrWhiteSpace(memberId)
                , o => this.GetSortKey(team, o)
                , o =>
                {
                    //HACK:若有memberId则认为是查看member视图，此时不对排序数据做保存
                    if (project != null)
                    {
                        project[by] = o;
                        this._teamService.Update(team);
                    }
                }));
        }
        protected override void ApplyUpdate(Task t, ChangeLog c)
        {
            base.ApplyUpdate(t, c);

            var teamTask = t as Teams.Task;
            var team = this.GetTeamOfCurrentAccount<Teams.Member>(teamTask.TeamId);//普通成员即可

            switch (c.Name.ToLower())
            {
                case "assigneeid":
                    if (string.IsNullOrWhiteSpace(c.Value))
                        teamTask.RemoveAssignee();
                    else
                        teamTask.AssignTo(this.GetMember(team, c.Value));
                    break;
                case "projects":
                    var p = this.GetProject(team, c.Value);
                    if (c.Type == ChangeType.Insert)
                        teamTask.AddToProject(p);
                    else if (c.Type == ChangeType.Delete)
                        teamTask.RemoveFromProject(p);
                    break;
                case "comments":
                    if (c.Type == ChangeType.Insert)
                        teamTask.AddComment(this.GetCurrentMember(team), c.Value);
                    //UNDONE:目前暂不支持删除评论
                    break;
            }
        }

        /// <summary>根据Email获取默认账号连接，用于Email与账号间的关联依据，默认取Google连接
        /// </summary>
        /// <param name="email"></param>
        /// <returns></returns>
        protected virtual AccountConnection GetDefaultConnectionByEmail(string email)
        {
            return this._accountConnectionService.GetConnection<GoogleConnection>(email);
        }
        /// <summary>根据账号获取默认Email信息，默认取Google连接
        /// </summary>
        /// <param name="a"></param>
        /// <returns></returns>
        protected virtual string GetDefaultEmail(Account a)
        {
            var google = this._accountConnectionService
                .GetConnections(a)
                .FirstOrDefault(o => o is GoogleConnection) as GoogleConnection;
            return google != null ? google.Name : null;
        }
        /// <summary>转换客户端在团队模块中使用的账号信息，可重载定制Name和Email，默认取Google连接信息
        /// </summary>
        /// <param name="a"></param>
        /// <returns></returns>
        protected virtual AccountInfo Parse(Account a)
        {
            return new AccountInfo()
            {
                ID = a.ID.ToString(),
                Name = a.Name,
                Email = this.GetDefaultEmail(a) ?? string.Empty
            };
        }

        //获取当前用户所在的team
        private Teams.Team GetTeamOfCurrentAccount(string teamId)
        {
            int id;
            int.TryParse(teamId, out id);
            return this.GetTeamOfCurrentAccount<Teams.Member>(id);
        }
        //获取当前用户以FullMember身份所在的team
        private Teams.Team GetTeamOfFullMember(string teamId)
        {
            int id;
            int.TryParse(teamId, out id);
            return this.GetTeamOfCurrentAccount<Teams.FullMember>(id);
        }
        private Teams.Team GetTeamOfCurrentAccount<T>(int teamId) where T : Teams.Member
        {
            var t = this.GetTeam(teamId);//权限在之后验证
            if (!this.IsTeamOfCurrentAccount<T>(t))
                throw new CooperknownException(this.Lang().you_are_not_the_member_of_team);
            return t;
        }
        //GetTeam未作权限控制，少数匿名场景使用
        private Teams.Team GetTeam(string teamId)
        {
            int id;
            int.TryParse(teamId, out id);
            return this.GetTeam(id);
        }
        private Teams.Team GetTeam(int teamId)
        {
            var t = this._teamService.GetTeam(teamId);
            if (t == null)
                throw new CooperknownException(this.Lang().team_not_found);
            return t;
        }
        private Teams.Project GetProject(Teams.Team team, string projectId)
        {
            int id;
            var p = int.TryParse(projectId, out id) ? team.GetProject(id) : null;
            if (p == null)
                throw new CooperknownException(this.Lang().project_not_found);
            if (p.TeamId != team.ID)
                throw new CooperknownException(this.Lang().project_not_match_team);
            return p;
        }
        private Teams.Member GetMember(Teams.Team team, string memberId)
        {
            int id;
            int.TryParse(memberId, out id);
            return this.GetMember(team, id);
        }
        private Teams.Member GetMember(Teams.Team team, int memberId)
        {
            var m = team.GetMember(memberId);
            if (m == null)
                throw new CooperknownException(this.Lang().member_not_found);
            if (m.TeamId != team.ID)
                throw new CooperknownException(this.Lang().member_not_match_team);
            return m;
        }
        //获取当前用户对应的成员信息
        private Teams.Member GetCurrentMember(Teams.Team team)
        {
            var m = team.Members.FirstOrDefault(o =>
                o.AssociatedAccountId.HasValue && o.AssociatedAccountId.Value == this.Context.Current.ID);
            if (m == null)
                throw new CooperknownException(this.Lang().you_are_not_the_member_of_team);
            return m;
        }
        //判断当前用户是否是指定团队的成员
        private bool IsTeamOfCurrentAccount(Teams.Team team)
        {
            return this.IsTeamOfCurrentAccount<Teams.Member>(team);
        }
        private bool IsTeamOfCurrentAccount<T>(Teams.Team team) where T : Teams.Member
        {
            var a = this.Context.Current;
            return team.Members.Any(o =>
                o.AssociatedAccountId.HasValue && o.AssociatedAccountId == a.ID && o is T);
        }
        private bool IsCreator(Teams.Team team, Teams.Task task, Account a)
        {
            var m = team.GetMember(task.CreatorMemberId);
            //UNDONE:由于Member可能被删除，teamtask可能找不到创建者？
            if (m == null || !m.AssociatedAccountId.HasValue)
                return false;
            var account = _accountService.GetAccount(m.AssociatedAccountId.Value);
            return account != null && account.ID == a.ID;
        }
        private bool IsAssignee(TeamTaskInfo task, Account a)
        {
            return task.Assignee != null && task.Assignee.accountId == a.ID.ToString();
        }
        private bool IsAssignee(Teams.Task task, Teams.Member member)
        {
            return task.AssigneeId.HasValue && task.AssigneeId == member.ID;
        }
        private IEnumerable<Teams.Task> GetTasksByAccount(Teams.Team team, Account account)
        {
            return this._teamTaskService.GetTasksByAccount(team, account);
        }
        private IEnumerable<Teams.Task> GetIncompletedTasksByAccount(Teams.Team team, Account account)
        {
            return this._teamTaskService.GetIncompletedTasksByAccount(team, account);
        }
        private IEnumerable<Teams.Task> GetTasksByMember(Teams.Member member)
        {
            return this._teamTaskService.GetTasksByMember(member);
        }
        private IEnumerable<Teams.Task> GetIncompletedTasksByMember(Teams.Member member)
        {
            return this._teamTaskService.GetIncompletedTasksByMember(member);
        }
        private IEnumerable<Teams.Task> GetTasksByProject(Teams.Project p)
        {
            return this._teamTaskService.GetTasksByProject(p);
        }
        private IEnumerable<Teams.Task> GetIncompletedTasksByProject(Teams.Project p)
        {
            return this._teamTaskService.GetIncompletedTasksByProject(p);
        }
        private TeamInfo Parse(Teams.Team team)
        {
            return new TeamInfo()
            {
                id = team.ID.ToString(),
                name = team.Name,
                projects = team.Projects.Select(o => this.Parse(o)).ToArray(),
                members = team.Members.Select(o => this.Parse(o)).ToArray()
            };
        }
        private TeamProjectInfo Parse(Teams.Project project)
        {
            return new TeamProjectInfo()
            {
                id = project.ID.ToString(),
                name = project.Name
            };
        }
        private TeamMemberInfo Parse(Teams.Member member)
        {
            return new TeamMemberInfo()
            {
                id = member.ID.ToString(),
                name = member.Name,
                email = member.Email,
                accountId = member.AssociatedAccountId.HasValue ? member.AssociatedAccountId.Value.ToString() : null
            };
        }
        private TeamTaskCommentInfo Parse(Teams.Comment comment)
        {
            return new TeamTaskCommentInfo()
            {
                body = comment.Body,
                creator = this.Parse(comment.Creator),
                createTime = comment.CreateTime.ToString("yyyy-MM-dd HH:mm:ss")
            };
        }
        private TeamTaskInfo[] Parse(IEnumerable<Teams.Task> tasks, Teams.Team team)
        {
            var a = this.Context.Current;
            Teams.Member m;
            //TODO:改用automapper做实体映射
            return this.ParseTasks(() => new TeamTaskInfo(), (task, taskInfo) =>
            {
                var teamTask = task as Teams.Task;
                var teamTaskInfo = taskInfo as TeamTaskInfo;
                //创建人
                if ((m = team.GetMember(teamTask.CreatorMemberId)) != null)
                    teamTaskInfo.Creator = this.Parse(m);
                //执行人
                if (teamTask.AssigneeId.HasValue
                    && (m = team.GetMember(teamTask.AssigneeId.Value)) != null)
                    teamTaskInfo.Assignee = this.Parse(m);
                //项目列表
                teamTaskInfo.Projects = teamTask.ProjectIds.Select(o => this.Parse(team.GetProject(o.ID))).ToArray();
                //是否可编辑 创建者或被分配者（执行人）
                teamTaskInfo.Editable = this.IsCreator(team, teamTask, a) || this.IsAssignee(teamTaskInfo, a);
                //评论
                teamTaskInfo.Comments = teamTask.Comments.Select(o => this.Parse(o)).ToArray();
            }, tasks.Select(o => o as Task)
            .ToArray())
            .Select(o => o as TeamTaskInfo)
            .ToArray();
        }

        private ActionResult GetBy(string teamId
            , string projectId
            , string memberId
            , Func<Teams.Team, Account, IEnumerable<Teams.Task>> taskByAccount//获取用户在指定team任务
            , Func<Teams.Member, IEnumerable<Teams.Task>> taskByMember//获取成员在指定team任务
            , Func<Teams.Project, IEnumerable<Teams.Task>> taskByProject//获取项目内的所有任务
            , Func<Account, Teams.Team, TaskInfo[], Sort[]> sortsOfAccount//获取用户在指定team的排序信息
            , Func<Teams.Project, TaskInfo[], Sort[]> sortsOfProject)//获取项目的排序信息
        {
            var a = this.Context.Current;
            var team = this.GetTeam(teamId);//允许匿名访问
            var project = !string.IsNullOrWhiteSpace(projectId)
                ? this.GetProject(team, projectId)
                : null;
            var member = !string.IsNullOrWhiteSpace(memberId)
                ? this.GetMember(team, memberId)
                : null;

            TeamTaskInfo[] tasks;
            Sort[] sorts = null;
            //当前用户是该团队成员则处于可编辑状态
            var editable = this.IsTeamOfCurrentAccount(team);

            if (project != null)
            {
                tasks = this.Parse(taskByProject(project), team);
                sorts = sortsOfProject(project, tasks);
            }
            else if (member != null)
            {
                //editable = false;
                tasks = this.Parse(taskByMember(member), team);
                //HACK:查询member任务时使用member对应账号的排序
                if (member.AssociatedAccountId.HasValue)
                {
                    //查看指定成员任务时只能编辑属于当前用户的成员
                    //editable = member.AssociatedAccountId == current.ID;
                    sorts = sortsOfAccount(this._accountService
                        .GetAccount(member.AssociatedAccountId.Value), team, tasks);
                }
                else
                    sorts = sortsOfAccount(a, team, tasks);
            }
            else
            {
                tasks = this.Parse(taskByAccount(team, a), team);
                sorts = sortsOfAccount(a, team, tasks);
            }

            return Json(new
            {
                Editable = editable,
                List = tasks,
                Sorts = sorts ?? _emptySorts,
            });
        }
        private Sort[] ParseSortsByPriority(Account account, Teams.Team team, params TaskInfo[] tasks)
        {
            return this.ParseSortsByPriority(this.GetSorts(account, team, SORT_PRIORITY), tasks);
        }
        private Sort[] ParseSortsByPriority(Teams.Project project, params TaskInfo[] tasks)
        {
            return this.ParseSortsByPriority(this.GetSorts(project, SORT_PRIORITY), tasks);
        }
        private Sort[] ParseSortsByDueTime(Account account, Teams.Team team, params TaskInfo[] tasks)
        {
            return this.ParseSortsByDueTime(this.GetSorts(account, team, SORT_DUETIME), tasks);
        }
        private Sort[] ParseSortsByDueTime(Teams.Project project, params TaskInfo[] tasks)
        {
            return this.ParseSortsByDueTime(this.GetSorts(project, SORT_DUETIME), tasks);
        }
        private Sort[] GetSorts(Account a, Teams.Team t, string by)
        {
            return this.GetSorts(a, this.GetSortKey(t, by));
        }
        private Sort[] GetSorts(Teams.Project p, string by)
        {
            return !string.IsNullOrWhiteSpace(p[by])
                ? _serializer.JsonDeserialize<Sort[]>(p[by])
                : _emptySorts;
        }
        private string GetSortKey(Teams.Team t, string by)
        {
            return by + "_" + t.ID;
        }
    }

    #region 以下是面向终端的Team相关数据模型设计
    public class AccountInfo
    {
        public string ID { get; set; }
        public string Name { get; set; }
        public string Email { get; set; }
    }
    public class TeamInfo
    {
        public string id { get; set; }
        public string name { get; set; }

        public TeamMemberInfo[] members { get; set; }
        public TeamProjectInfo[] projects { get; set; }
    }
    public class TeamMemberInfo
    {
        public string id { get; set; }
        public string name { get; set; }
        public string email { get; set; }
        public string accountId { get; set; }
    }
    public class TeamProjectInfo
    {
        public string id { get; set; }
        public string name { get; set; }
    }
    public class TeamTaskInfo : TaskInfo
    {
        public TeamMemberInfo Creator { get; set; }
        public TeamMemberInfo Assignee { get; set; }
        public TeamProjectInfo[] Projects { get; set; }
        public TeamTaskCommentInfo[] Comments { get; set; }
        public TeamTaskInfo() : base() { }
    }
    /// <summary>团队任务评论信息
    /// </summary>
    public class TeamTaskCommentInfo
    {
        public TeamMemberInfo creator { get; set; }
        public string body { get; set; }
        /// <summary>格式yyyy-MM-dd HH:mm:ss
        /// </summary>
        public string createTime { get; set; }
    }
    #endregion
}