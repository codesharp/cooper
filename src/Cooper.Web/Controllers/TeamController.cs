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

        //优先级列表模式 所有任务
        [HttpPost]
        public ActionResult GetByPriority(string teamId, string projectId)
        {
            return this.GetBy(teamId
                , projectId
                , this.GetTasksByTeam
                , this.GetTasksByProject
                , this.ParseSortsByPriority
                , this.ParseSortsByPriority);
        }
        //优先级列表模式 不含已经完成
        [HttpPost]
        public ActionResult GetIncompletedByPriority(string teamId, string projectId)
        {
            return this.GetBy(teamId
                , projectId
                , this.GetIncompletedTasksByTeam
                , this.GetIncompletedTasksByProject
                , this.ParseSortsByPriority
                , this.ParseSortsByPriority);
        }
        //DueTime列表模式
        [HttpPost]
        public ActionResult GetByDueTime(string teamId, string projectId)
        {
            return this.GetBy(teamId
                , projectId
                , this.GetTasksByTeam
                , this.GetTasksByProject
                , this.ParseSortsByDueTime
                , this.ParseSortsByDueTime);
        }

        //团队相关api
        private static IList<TeamInfo> _teams = new List<TeamInfo>() { GetTeam("1"), GetTeam("2"), GetTeam("3"), GetTeam("4") };
        [HttpGet]
        public ActionResult GetTeams()
        {
            //return Json(this.Parse(this._teamService.GetTeamsByAccount(this.Context.Current)), JsonRequestBehavior.AllowGet);
            return Json(_teams, JsonRequestBehavior.AllowGet);
        }
        [HttpPost]
        public ActionResult CreateTeam(string name)
        {
            //var t = new Teams.Team(name);
            //TODO:将当前用户加入到该team
            //this._teamService.Create(t);
            //return Json(t.ID);

            var t = GetTeam((_teams.Count + 1).ToString());
            t.name = name;
            _teams.Add(t);
            return Json(t.id);
        }
        [HttpPut]
        public ActionResult UpdateTeam(string id, string name)
        {
            var t = _teams.Single(o => o.id == id);
            t.name = name;
            return Json(true);
        }
        [HttpPost]
        public ActionResult CreateProject(string teamId, string name)
        {
            var t = _teams.Single(o => o.id == teamId);
            var p = GetProject((t.projects.Length + 1).ToString());
            p.name = name;
            var all = t.projects.ToList();
            all.Add(p);
            t.projects = all.ToArray();
            return Json(p.id);
        }
        [HttpPost]
        public ActionResult CreateMember(string teamId, string name, string email)
        {
            var t = _teams.Single(o => o.id == teamId);
            var m = GetMember((t.members.Length + 1).ToString());
            m.name = name;
            m.email = email;
            var all = t.members.ToList();
            all.Add(m);
            t.members = all.ToArray();
            return Json(m.id);
        }
        [HttpPost]
        //[HttpDelete]//需要路由支持 team/{teamId}/member/{memberId}
        public ActionResult DeleteMember(string teamId, string memberId)
        {
            var t = _teams.Single(o => o.id == teamId);
            var all = t.members.ToList();
            all.Remove(t.members.Single(o => o.id == memberId));
            t.members = all.ToArray();
            return Json(true);
        }

        /// <summary>用于接收终端的变更同步数据，按team同步
        /// </summary>
        /// <param name="teamId">团队标识</param>
        /// <param name="changes"></param>
        /// <param name="by"></param>
        /// <param name="sorts"></param>
        /// <returns></returns>
        [HttpPost]
        [ValidateInput(false)]
        public ActionResult Sync(string teamId, string projectId, string changes, string by, string sorts)
        {
            var team = GetTeam(teamId);
            Assert.IsNotNull(team);
            var project = GetProject(projectId);

            return Json(this.Sync(changes, by, sorts
                , o => { if (team != null) { } if (project != null) { } }
                , () => project == null
                , o => this.GetSortKey(team, o)
                , o => { }));
        }
        protected override void ApplyUpdate(Task t, ChangeLog c)
        {
            base.ApplyUpdate(t, c);

            var teamTask = t as Teams.Task;

            switch (c.Name.ToLower())
            {
                case "assigneeid":
                    //teamTask.AssignTo()
                    break;
                case "projects":
                    //teamTask.AddToProject
                    break;
            }
        }

        #region Mocks
        private static TeamInfo GetTeam(string teamId)
        {
            return new TeamInfo()
            {
                id = teamId,
                name = "CooperTestTeam-" + teamId,
                members = new TeamMemberInfo[] { GetMember("1"), GetMember("2"), GetMember("3") },
                projects = new TeamProjectInfo[] { GetProject("1"), GetProject("2"), GetProject("3") }
            };
        }
        private static TeamProjectInfo GetProject(string projectId)
        {
            int id;
            return int.TryParse(projectId, out id) ? new TeamProjectInfo() { id = projectId, name = "Project-" + projectId } : null;
        }
        private static TeamMemberInfo GetMember(string mId) { return new TeamMemberInfo() { id = mId, name = "Member-" + mId, email = "xwwwx@gmail.com" }; }
        private IEnumerable<Task> GetTasksByTeam(Account a, TeamInfo t) { return new List<Task>(); }
        private IEnumerable<Task> GetIncompletedTasksByTeam(Account a, TeamInfo t) { return new List<Task>(); }
        private IEnumerable<Task> GetTasksByProject(TeamProjectInfo p) { return new List<Task>(); }
        private IEnumerable<Task> GetIncompletedTasksByProject(TeamProjectInfo p) { return new List<Task>(); }
        #endregion

        private TeamInfo Parse(Teams.Team o)
        {
            return new TeamInfo()
            {
                id = o.ID.ToString(),
                name = o.Name,
                projects = this._teamProjectService.GetProjectsByTeam(o).Select(p => this.Parse(p)).ToArray(),
                //members
            };
        }
        private TeamProjectInfo Parse(Teams.Project project)
        {
            return new TeamProjectInfo()
            {
                id = project.ID.ToString(),
                name = project.Name,
                Extensions = new Dictionary<string, string>()
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
            , Func<Account, TeamInfo, IEnumerable<Task>> taskByTeam//没有projectId时的获取用户在团队内的任务
            , Func<TeamProjectInfo, IEnumerable<Task>> taskByProject//获取项目内的所有任务
            , Func<Account, TeamInfo, TaskInfo[], Sort[]> sortByTeam
            , Func<TeamProjectInfo, TaskInfo[], Sort[]> sortByProject)
        {
            var account = this.Context.Current;
            var team = GetTeam(teamId);
            var project = GetProject(projectId);
            var editable = true;//是否是非团队内成员浏览

            var tasks = project == null
                ? this.ParseTasks(taskByTeam(account, team).ToArray())
                : this.ParseTasks(taskByProject(project).ToArray());

            return Json(new
            {
                Editable = editable,
                List = tasks,
                Sorts = project == null
                    ? sortByTeam(account, team, tasks)
                    : sortByProject(project, tasks),
            });
        }
        
        #region Sorts
        private Sort[] ParseSortsByPriority(Account account, TeamInfo team, params TaskInfo[] tasks)
        {
            return this.ParseSortsByPriority(this.GetSorts(account, team, SORT_PRIORITY), tasks);
        }
        private Sort[] ParseSortsByPriority(TeamProjectInfo project, params TaskInfo[] tasks)
        {
            return this.ParseSortsByPriority(this.GetSorts(project, SORT_PRIORITY), tasks);
        }
        private Sort[] ParseSortsByDueTime(Account account, TeamInfo team, params TaskInfo[] tasks)
        {
            return this.ParseSortsByDueTime(this.GetSorts(account, team, SORT_DUETIME), tasks);
        }
        private Sort[] ParseSortsByDueTime(TeamProjectInfo project, params TaskInfo[] tasks)
        {
            return this.ParseSortsByDueTime(this.GetSorts(project, SORT_DUETIME), tasks);
        }
        private Sort[] GetSorts(Account a, TeamInfo t, string by)
        {
            return this.GetSorts(a, this.GetSortKey(t, by));
        }
        private Sort[] GetSorts(TeamProjectInfo p, string by)
        {
            return !string.IsNullOrWhiteSpace(p.Extensions[by])
                ? _serializer.JsonDeserialize<Sort[]>(p.Extensions[by])
                : _empty;
        }
        private string GetSortKey(TeamInfo t, string by)
        {
            return by + "_" + t.id;
        }
        #endregion

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
            public IDictionary<string, string> Extensions { get; set; }
        }
    }
}