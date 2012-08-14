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

        public virtual ActionResult Index()
        {
            return View();
        }

        #region 各类显示模式数据获取
        //优先级列表模式 所有任务
        [HttpPost]
        public ActionResult GetByPriority(string teamId, string projectId, string taskFolderId)
        {
            return this.GetBy(taskFolderId
                , this._taskService.GetTasksNotBelongAnyFolder
                , this._taskService.GetTasks
                , this.ParseSortsByPriority
                , this.ParseSortsByPriority);
        }
        //优先级列表模式 不含已经完成
        [HttpPost]
        public ActionResult GetIncompletedByPriority(string teamId, string projectId, string taskFolderId)
        {
            return this.GetBy(taskFolderId
                , this._taskService.GetIncompletedTasksAndNotBelongAnyFolder
                , this._taskService.GetIncompletedTasks
                , this.ParseSortsByPriority
                , this.ParseSortsByPriority);
        }
        //DueTime列表模式
        [HttpPost]
        public ActionResult GetByDueTime(string teamId, string projectId, string taskFolderId)
        {
            return this.GetBy(taskFolderId
                , this._taskService.GetTasksNotBelongAnyFolder
                , this._taskService.GetTasks
                , this.ParseSortsByDueTime
                , this.ParseSortsByDueTime);
        }
        #endregion
    }
}
