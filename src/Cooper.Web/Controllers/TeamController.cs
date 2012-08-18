using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using CodeSharp.Core.Services;
using Cooper.Model.Tasks;
using Cooper.Model.Accounts;

namespace Cooper.Web.Controllers
{
    //团队
    public class TeamController : TaskController
    {
        public TeamController(ILoggerFactory factory
            , IAccountService accountService
            , ITaskService taskService
            , ITaskFolderService taskFolderService
            , IFetchTaskHelper fetchTaskHelper)
            : base(factory
            , accountService
            , taskService
            , taskFolderService
            , fetchTaskHelper) { }

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

        private static IList<Team> _teams = new List<Team>() { GetTeam("1"), GetTeam("2"), GetTeam("3"), GetTeam("4") };
        public ActionResult GetTeams()
        {
            return Json(_teams, JsonRequestBehavior.AllowGet);
        }
        public ActionResult CreateTeam(string name)
        {
            var t = GetTeam((_teams.Count + 1).ToString());
            t.name = name;
            _teams.Add(t);
            return Json(t.id);
        }
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
        public ActionResult CreateMember(string teamId, string name)
        {
            var t = _teams.Single(o => o.id == teamId);
            var m = GetMember((t.members.Length + 1).ToString());
            m.name = name;
            var all = t.members.ToList();
            all.Add(m);
            t.members = all.ToArray();
            return Json(m.id);
        }
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
            //设置project
            if (c.Name.Equals("projects", StringComparison.InvariantCultureIgnoreCase)) { }
        }

        //临时
        private static Team GetTeam(string teamId)
        {
            return new Team()
            {
                id = teamId,
                name = "CooperTestTeam-" + teamId,
                members = new TeamMember[] { GetMember("1"), GetMember("2"), GetMember("3") },
                projects = new Project[] { GetProject("1"), GetProject("2"), GetProject("3") }
            };
        }
        private static Project GetProject(string projectId)
        {
            int id;
            return int.TryParse(projectId, out id) ? new Project() { id = projectId, name = "Project-" + projectId } : null;
        }
        private static TeamMember GetMember(string mId) { return new TeamMember() { id = mId, name = "Member-" + mId, email = "xwwwx@gmail.com" }; }
        private IEnumerable<Task> GetTasksByTeam(Account a, Team t) { return new List<Task>(); }
        private IEnumerable<Task> GetIncompletedTasksByTeam(Account a, Team t) { return new List<Task>(); }
        private IEnumerable<Task> GetTasksByProject(Project p) { return new List<Task>(); }
        private IEnumerable<Task> GetIncompletedTasksByProject(Project p) { return new List<Task>(); }

        private ActionResult GetBy(string teamId
            , string projectId
            , Func<Account, Team, IEnumerable<Task>> taskByTeam//没有projectId时的获取用户在团队内的任务
            , Func<Project, IEnumerable<Task>> taskByProject//获取项目内的所有任务
            , Func<Account, Team, TaskInfo[], Sort[]> sortByTeam
            , Func<Project, TaskInfo[], Sort[]> sortByProject)
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
        private Sort[] ParseSortsByPriority(Account account, Team team, params TaskInfo[] tasks)
        {
            return this.ParseSortsByPriority(this.GetSorts(account, team, SORT_PRIORITY), tasks);
        }
        private Sort[] ParseSortsByPriority(Project project, params TaskInfo[] tasks)
        {
            return this.ParseSortsByPriority(this.GetSorts(project, SORT_PRIORITY), tasks);
        }
        private Sort[] ParseSortsByDueTime(Account account, Team team, params TaskInfo[] tasks)
        {
            return this.ParseSortsByDueTime(this.GetSorts(account, team, SORT_DUETIME), tasks);
        }
        private Sort[] ParseSortsByDueTime(Project project, params TaskInfo[] tasks)
        {
            return this.ParseSortsByDueTime(this.GetSorts(project, SORT_DUETIME), tasks);
        }
        private Sort[] GetSorts(Account a, Team t, string by)
        {
            return this.GetSorts(a, this.GetSortKey(t, by));
        }
        private Sort[] GetSorts(Project p, string by)
        {
            return _empty;
            //return !string.IsNullOrWhiteSpace(p[key])
            //    ? _serializer.JsonDeserialize<Sort[]>(p[key])
            //    : _empty;
        }
        private string GetSortKey(Team t, string by)
        {
            return by + "_" + t.id;
        }

        public class Team
        {
            public string id { get; set; }
            public string name { get; set; }

            public TeamMember[] members { get; set; }
            public Project[] projects { get; set; }
        }
        public class TeamMember
        {
            public string id { get; set; }
            public string name { get; set; }
            public string email { get; set; }
        }
        public class Project
        {
            public string id { get; set; }
            public string name { get; set; }
        }
    }
}
