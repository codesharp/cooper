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
        public PersonalController(ILoggerFactory factory
            , ITaskService taskService
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

        #region 各类显示模式数据获取
        //优先级列表模式 所有任务
        [HttpPost]
        public ActionResult GetByPriority(string tasklistId, string taskFolderId)
        {
            return this.GetBy(taskFolderId ?? tasklistId
                , this._taskService.GetTasksNotBelongAnyFolder
                , this._taskService.GetTasks
                , this.ParseSortsByPriority
                , this.ParseSortsByPriority);
        }
        //优先级列表模式 不含已经完成
        [HttpPost]
        public ActionResult GetIncompletedByPriority(string tasklistId, string taskFolderId)
        {
            return this.GetBy(taskFolderId ?? tasklistId
                , this._taskService.GetIncompletedTasksAndNotBelongAnyFolder
                , this._taskService.GetIncompletedTasks
                , this.ParseSortsByPriority
                , this.ParseSortsByPriority);
        }
        //DueTime列表模式
        [HttpPost]
        public ActionResult GetByDueTime(string tasklistId, string taskFolderId)
        {
            return this.GetBy(taskFolderId ?? tasklistId
                , this._taskService.GetTasksNotBelongAnyFolder
                , this._taskService.GetTasks
                , this.ParseSortsByDueTime
                , this.ParseSortsByDueTime);
        }
        #endregion

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

        private void Prepare()
        {
            var a = this.Context.Current;
            ViewBag.Connections = this._accountConnectionService.GetConnections(a);
            ViewBag.TaskFolders = this._taskFolderService.GetTaskFolders(a);
            ViewBag.FetchTaskFolders = this._fetchTaskHelper.GetFetchTaskFolders(a);
        }
    }
}