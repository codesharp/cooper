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
    //团队
    public class TeamController : TaskController
    {
        private Teams.ITeamService _teamService;
        private Teams.IProjectService _teamProjectService;
        private Teams.ITaskService _teamTaskService;
        public TeamController(ILoggerFactory factory
            , IAccountService accountService
            , ITaskService taskService
            , ITaskFolderService taskFolderService
            , IFetchTaskHelper fetchTaskHelper
            , Teams.ITeamService teamService
            , Teams.IProjectService teamProjectService
            , Teams.ITaskService teamTaskService)
            : base(factory
            , accountService
            , taskService
            , taskFolderService
            , fetchTaskHelper)
        {
            this._teamService = teamService;
            this._teamProjectService = teamProjectService;
            this._teamTaskService = teamTaskService;
        }

        public ActionResult Index(string teamId, string projectId, string memberId)
        {
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
                , this.GetTasksByMember
                , this.GetTasksByProject
                , this.ParseSortsByDueTime
                , this.ParseSortsByDueTime);
        }
        #endregion

        #region 团队相关api
        [HttpGet]
        public ActionResult GetTeams()
        {
            return Json(this._teamService.GetTeamsByAccount(this.Context.Current).Select(o => this.Parse(o)), JsonRequestBehavior.AllowGet);
        }
        [HttpPost]
        public ActionResult CreateTeam(string name)
        {
            var t = new Teams.Team(name);
            //TODO:将当前用户加入到该team
            this._teamService.Create(t);
            return Json(t.ID);
        }
        [HttpPut]
        public ActionResult UpdateTeam(string id, string name)
        {
            var t = this.GetTeamByCurrent(id);
            t.SetName(name);
            this._teamService.Update(t);
            return Json(true);
        }
        [HttpPost]
        public ActionResult CreateProject(string teamId, string name)
        {
            var t = this.GetTeam(teamId);
            var p = new Teams.Project(name, false, t);
            this._teamProjectService.Create(p);
            return Json(p.ID);
        }
        [HttpPost]
        public ActionResult CreateMember(string teamId, string name, string email)
        {
            var t = this.GetTeam(teamId);
            var m = new Teams.TeamMember(name, email, t);
            //UNDONE:新建Member
            return Json(m.ID);
        }
        [HttpPost]
        //[HttpDelete]//需要路由支持 team/{teamId}/member/{memberId}
        public ActionResult DeleteMember(string teamId, string memberId)
        {
            //UNDONE:删除Member
            return Json(true);
        }
        #endregion

        /// <summary>用于接收终端的变更同步数据，按team同步
        /// </summary>
        /// <param name="teamId">团队标识</param>
        /// <param name="projectId">项目标识，可以为空</param>
        /// <param name="changes"></param>
        /// <param name="by"></param>
        /// <param name="sorts"></param>
        /// <returns></returns>
        [HttpPost]
        [ValidateInput(false)]
        public ActionResult Sync(string teamId, string projectId, string changes, string by, string sorts)
        {
            var team = this.GetTeamByCurrent(teamId);
            var project = string.IsNullOrWhiteSpace(projectId) ? null : this.GetProject(team, projectId);

            return Json(this.Sync(changes, by, sorts
                , () =>
                {
                    var t = new Teams.Task(this.Context.Current, team);
                    if (project != null)
                        t.AddToProject(project);
                    return t;
                }
                , () => project == null
                , o => this.GetSortKey(team, o)
                , o => { project[by] = o; this._teamProjectService.Update(project); }));
        }
        protected override void ApplyUpdate(Task t, ChangeLog c)
        {
            base.ApplyUpdate(t, c);

            var teamTask = t as Teams.Task;

            switch (c.Name.ToLower())
            {
                case "assigneeid":
                    //UNDONE:teamTask.AssignTo()
                    break;
                case "projects":
                    if (c.Type == ChangeType.Insert)
                        teamTask.AddToProject(this._teamProjectService.GetProject(int.Parse(c.Value)));
                    else if (c.Type == ChangeType.Delete)
                        teamTask.RemoveFromProject(this._teamProjectService.GetProject(int.Parse(c.Value)));
                    break;
            }
        }

        private Teams.Team GetTeam(string teamId)
        {
            int id;
            var t = int.TryParse(teamId, out id)
                ? this._teamService.GetTeam(id)
                : null;
            if (t == null)
                throw new CooperknownException(this.Lang().team_not_found);
            return t;
        }
        private Teams.Team GetTeamByCurrent(string teamId)
        {
            var t = this.GetTeam(teamId);
            //TODO:验证当前用户是该team的成员
            return t;
        }
        private Teams.Project GetProject(Teams.Team team, string projectId)
        {
            int id;
            var p = int.TryParse(projectId, out id)
                ? this._teamProjectService.GetProject(id)
                : null;
            if (p == null)
                throw new CooperknownException(this.Lang().project_not_found);
            if (p.TeamId != team.ID)
                throw new CooperknownException(this.Lang().project_not_match_team);
            return p;
        }
        private Teams.TeamMember GetMember(Teams.Team team, string memberId)
        {
            int id;
            int.TryParse(memberId, out id);
            Teams.TeamMember m = null;
            if (m == null)
                throw new CooperknownException(this.Lang().member_not_found);
            if (m.TeamId != team.ID)
                throw new CooperknownException(this.Lang().member_not_match_team);
            return m;
        }
        //UNDONE:团队任务查询
        private IEnumerable<Teams.Task> GetTasksByMember(Teams.TeamMember member) { return new List<Teams.Task>(); }
        private IEnumerable<Teams.Task> GetIncompletedTasksByMember(Teams.TeamMember member) { return new List<Teams.Task>(); }
        private IEnumerable<Teams.Task> GetTasksByProject(Teams.Project p) { return new List<Teams.Task>(); }
        private IEnumerable<Teams.Task> GetIncompletedTasksByProject(Teams.Project p) { return new List<Teams.Task>(); }
        private TeamInfo Parse(Teams.Team team)
        {
            return new TeamInfo()
            {
                id = team.ID.ToString(),
                name = team.Name,
                //UNDONE:teaminfo填充完整
                projects = new TeamProjectInfo[] { }, //this._teamProjectService.GetProjectsByTeam(team).Select(p => this.Parse(p)).ToArray(),
                members = new TeamMemberInfo[] { }
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
        private TeamMemberInfo Parse(Teams.TeamMember member)
        {
            return new TeamMemberInfo()
            {
                id = member.ID.ToString(),
                name = member.Name,
                email = member.Email
            };
        }

        private ActionResult GetBy(string teamId
            , string projectId
            , string memberId
            , Func<Teams.TeamMember, IEnumerable<Teams.Task>> taskByMember//获取成员在指定team任务
            , Func<Teams.Project, IEnumerable<Teams.Task>> taskByProject//获取项目内的所有任务
            , Func<Account, Teams.Team, TaskInfo[], Sort[]> sortByTeam
            , Func<Teams.Project, TaskInfo[], Sort[]> sortByProject)
        {
            var account = this.Context.Current;
            var team = this.GetTeam(teamId);
            var project = string.IsNullOrWhiteSpace(projectId) ? null : this.GetProject(team, projectId);
            //UNDONE:memberId为空则取当前用户在团队内的member信息
            var member = string.IsNullOrWhiteSpace(memberId) ? null : this.GetMember(team, memberId);

            var editable = true;//TODO:是否是非团队内成员浏览

            //TODO:改用automapper做实体映射
            var tasks = project == null
                ? this.Parse(taskByMember(member), team)
                : this.Parse(taskByProject(project), team);

            return Json(new
            {
                Editable = editable,
                List = tasks,
                Sorts = project == null
                    ? sortByTeam(account, team, tasks)
                    : sortByProject(project, tasks),
            });
        }
        private TeamTaskInfo[] Parse(IEnumerable<Teams.Task> tasks, Teams.Team team)
        {
            return this.ParseTasks(() => new TeamTaskInfo(), (task, taskInfo) =>
            {
                var teamTask = task as Teams.Task;
                var teamTaskInfo = taskInfo as TeamTaskInfo;
                //UNDONE:team相关数据填充
                //teamTaskInfo.Assignee
            }, tasks.Select(o => o as Task)
            .ToArray())
            .Select(o => o as TeamTaskInfo)
            .ToArray();
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
                : _empty;
        }
        private string GetSortKey(Teams.Team t, string by)
        {
            return by + "_" + t.ID;
        }
    }

    #region 以下是面向终端的Team相关数据模型设计
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
    }
    public class TeamProjectInfo
    {
        public string id { get; set; }
        public string name { get; set; }
    }
    public class TeamTaskInfo : TaskInfo
    {
        public TeamMemberInfo Assignee { get; set; }
        public TeamProjectInfo[] Projects { get; set; }
        public TeamTaskInfo() : base() { }
    }
    #endregion
}