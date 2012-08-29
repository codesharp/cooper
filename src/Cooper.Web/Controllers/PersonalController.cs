//Copyright (c) CodeSharp.  All rights reserved. - http://www.icodesharp.com/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using CodeSharp.Core.Services;
using Cooper.Model.Tasks;
using CodeSharp.Core.Utils;
using Cooper.Model.Accounts;
using CodeSharp.Core;

namespace Cooper.Web.Controllers
{
    //个人任务
    public class PersonalController : TaskController
    {
        private IAccountConnectionService _accountConnectionService;
        private IPersonalTaskService _personalTaskService;

        public PersonalController(ILoggerFactory factory
            , ITaskService taskService
            , IPersonalTaskService personalTaskService
            , ITaskFolderService taskFolderService
            , IFetchTaskHelper fetchTaskHelper
            , IAccountService accountService
            , IAccountConnectionService accountConnectionService)
            : base(factory
            , accountService
            , taskService
            , taskFolderService
            , fetchTaskHelper)
        {
            this._personalTaskService = personalTaskService;
            this._accountConnectionService = accountConnectionService;
        }

        public virtual ActionResult Index()
        {
            //判断是否移动
            if (Request.Browser.IsMobileDevice)
                return Redirect("~/hybrid/index.htm#taskListPage");

            this.Prepare();
            return View();
        }
        public ActionResult Full()
        {
            this.Prepare();
            return View();
        }
        public ActionResult Mini()
        {
            this.Prepare();
            return View();
        }

        //优先级列表模式 所有任务
        [HttpPost]
        public ActionResult GetByPriority(string tasklistId, string taskFolderId)
        {
            return this.GetBy(taskFolderId ?? tasklistId
                , this._personalTaskService.GetTasksNotBelongAnyFolder
                , this._personalTaskService.GetTasks
                , this.ParseSortsByPriority
                , this.ParseSortsByPriority);
        }
        //优先级列表模式 不含已经完成
        [HttpPost]
        public ActionResult GetIncompletedByPriority(string tasklistId, string taskFolderId)
        {
            return this.GetBy(taskFolderId ?? tasklistId
                , this._personalTaskService.GetIncompletedTasksAndNotBelongAnyFolder
                , this._personalTaskService.GetIncompletedTasks
                , this.ParseSortsByPriority
                , this.ParseSortsByPriority);
        }
        //DueTime列表模式
        [HttpPost]
        public ActionResult GetByDueTime(string tasklistId, string taskFolderId)
        {
            return this.GetBy(taskFolderId ?? tasklistId
                , this._personalTaskService.GetTasksNotBelongAnyFolder
                , this._personalTaskService.GetTasks
                , this.ParseSortsByDueTime
                , this.ParseSortsByDueTime);
        }

        #region TaskFolder处理
        //返回所有可用的folder id|name
        public ActionResult GetTasklists() { return this.GetTaskFolders(); }
        public ActionResult GetTaskFolders()
        {
            var dict = this._fetchTaskHelper.GetFetchTaskFolders(this.Context.Current);
            this._taskFolderService
                .GetTaskFolders(this.Context.Current)
                .ToList()
                .ForEach(o => dict.Add(o.ID.ToString(), o.Name));
            return Json(dict);
        }
        [HttpPost]
        [ValidateInput(false)]
        public ActionResult CreateTasklist(string name, string type) { return this.CreateTaskFolder(name, type); }
        /// <summary>创建任务夹
        /// </summary>
        /// <param name="name"></param>
        /// <param name="type">任务表类型，如：personal，team，project...</param>
        /// <returns></returns>
        [HttpPost]
        [ValidateInput(false)]
        public ActionResult CreateTaskFolder(string name, string type)
        {
            var list = new PersonalTaskFolder(name, this.Context.Current);
            this._taskFolderService.Create(list);
            return Json(list.ID);
        }
        /// <summary>批量创建任务夹
        /// </summary>
        /// <param name="data">[{ID:"",Name:"",Type:""}]</param>
        /// <returns>返回id变更修正</returns>
        [HttpPost]
        [ValidateInput(false)]
        public ActionResult CreateTaskFolders(string data)
        {
            Assert.IsNotNullOrWhiteSpace(data);
            var collects = new List<Correction>();
            var all = _serializer.JsonDeserialize<TaskFolderInfo[]>(data);
            foreach (var l in all)
            {
                try
                {
                    var list = new PersonalTaskFolder(l.Name, this.Context.Current);
                    this._taskFolderService.Create(list);
                    collects.Add(new Correction() { NewId = list.ID.ToString(), OldId = l.ID.ToString() });
                }
                catch (Exception e)
                {
                    this._log.Error(string.Format("创建任务表时异常：{0}|{1}|{2}", l.ID, l.Name, l.Type), e);
                }
            }
            return Json(collects);
        }
        [HttpPost]
        public ActionResult DeleteTaskFolder(string id)
        {
            var list = this.GetTaskFolder(id);
            if (list != null)
                this._taskFolderService.Delete(list);
            return Json(true);
        }
        #endregion

        /// <summary>用于接收终端的变更同步数据，按folder同步
        /// </summary>
        /// <param name="tasklistId">兼容</param>
        /// <param name="taskFolderId">允许为空，客户端在同步数据前应先确保folder已经创建</param>
        /// <param name="changes"></param>
        /// <param name="by"></param>
        /// <param name="sorts"></param>
        /// <returns></returns>
        [HttpPost]
        [ValidateInput(false)]
        public ActionResult Sync(string tasklistId, string taskFolderId, string changes, string by, string sorts)
        {
            //Fetch模式不支持同步变更
            Assert.IsFalse(this._fetchTaskHelper.IsFetchTaskFolder(taskFolderId ?? tasklistId));
            var folder = this.GetTaskFolder(taskFolderId ?? tasklistId);
            var a = this.Context.Current;
            return Json(this.Sync(changes, by, sorts
                , () =>
                {
                    var o = new PersonalTask(a);
                    if (folder != null)
                        o.SetTaskFolder(folder);
                    return o;
                }
                , o =>
                {
                    var task = o as PersonalTask;
                    //只有创建者才能更新个人任务
                    Assert.AreEqual(task.CreatorAccountId, a.ID);
                }
                , () => folder == null
                , o => o
                , o => { folder[by] = o; this._taskFolderService.Update(folder); }));
        }

        protected override void ApplyUpdate(Task t, ChangeLog c)
        {
            base.ApplyUpdate(t, c);
            //目前暂不支持修改folder
            if (c.Name.Equals("taskfolderid", StringComparison.InvariantCultureIgnoreCase))
                (t as PersonalTask).SetTaskFolder(this.GetTaskFolder(c.Value));
        }

        private ActionResult GetBy(string folderId
            , Func<Account, IEnumerable<Task>> func1//没有folder时的获取
            , Func<Account, TaskFolder, IEnumerable<Task>> func2//folder内的所有任务
            , Func<Account, TaskInfo[], Sort[]> func3
            , Func<Account, TaskFolder, TaskInfo[], Sort[]> func4)
        {
            var folder = this.GetTaskFolder(folderId);
            var account = this.Context.Current;

            TaskInfo[] tasks = null;

            var editable = folder != null
                || (tasks = this._fetchTaskHelper.FetchTasks(account, folderId)) == null;

            tasks = folder == null
                ? (tasks ?? this.ParseTasks(func1(account).ToArray()))
                : this.ParseTasks(func2(account, folder).ToArray());

            return Json(new
            {
                Editable = editable,
                List = tasks,
                Sorts = folder == null
                    ? func3(account, tasks)
                    : func4(account, folder, tasks),
            });
        }
        private TaskFolder GetTaskFolder(string taskFolderId)
        {
            int listId;
            var list = int.TryParse(taskFolderId, out listId) ? this._taskFolderService.GetTaskFolder(listId) : null;
            //HACK:个人任务列表只有拥有者能查看
            if (list != null && list is PersonalTaskFolder)
                Assert.AreEqual(this.Context.Current.ID, (list as PersonalTaskFolder).OwnerAccountId);
            return list;
        }
        private Sort[] ParseSortsByPriority(Account account, params TaskInfo[] tasks)
        {
            return this.ParseSortsByPriority(this.GetSorts(account, SORT_PRIORITY), tasks);
        }
        private Sort[] ParseSortsByPriority(Account account, TaskFolder folder, params TaskInfo[] tasks)
        {
            return this.ParseSortsByPriority(this.GetSorts(folder, SORT_PRIORITY), tasks);
        }
        private Sort[] ParseSortsByDueTime(Account account, params TaskInfo[] tasks)
        {
            return this.ParseSortsByDueTime(this.GetSorts(account, SORT_DUETIME), tasks);
        }
        private Sort[] ParseSortsByDueTime(Account account, TaskFolder folder, params TaskInfo[] tasks)
        {
            return this.ParseSortsByDueTime(this.GetSorts(folder, SORT_DUETIME), tasks);
        }
        private Sort[] GetSorts(TaskFolder a, string by)
        {
            return !string.IsNullOrWhiteSpace(a[by])
                ? _serializer.JsonDeserialize<Sort[]>(a[by])
                : _emptySorts;
        }
        private void Prepare()
        {
            var a = this.Context.Current;
            ViewBag.Connections = this._accountConnectionService.GetConnections(a);
            ViewBag.TaskFolders = this._taskFolderService.GetTaskFolders(a);
            ViewBag.FetchTaskFolders = this._fetchTaskHelper.GetFetchTaskFolders(a);
        }
    }
}