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
    //个人任务 目前只编写该模式
    public class PersonalController : BaseController
    {
        private static readonly Sort[] _empty = new Sort[0];
        private static readonly Serializer _serializer = new Serializer();
        private static readonly string TEMP = "temp_";
        private static readonly string PROFILE_SORT_PRIORITY = "ByPriority";
        private static readonly string PROFILE_SORT_DUETIME = "ByDueTime";

        private ILog _log;
        private ITaskService _taskService;
        private ITasklistService _tasklistService;
        private IAccountService _accountService;
        private IAccountConnectionService _accountConnectionService;
        private IFetchTasklistHelper _fetchTasklistHelper;
        public PersonalController(ILoggerFactory factory
            , ITaskService taskService
            , ITasklistService tasklistService
            , IAccountService accountService
            , IAccountConnectionService accountConnectionService
            , IFetchTasklistHelper fetchTasklistHelper)
            : base(factory)
        {
            this._log = factory.Create(typeof(PersonalController));
            this._taskService = taskService;
            this._tasklistService = tasklistService;
            this._accountService = accountService;
            this._accountConnectionService = accountConnectionService;
            this._fetchTasklistHelper = fetchTasklistHelper;
        }

        public ActionResult Index(string desk)
        {
            //判断是否移动
            if (string.IsNullOrWhiteSpace(desk)
                && Request.Browser.IsMobileDevice)
                return RedirectToAction("Mobile");

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

        #region Mobile处理
        public ActionResult Mobile()
        {
            ViewBag.Tasks = this._taskService.GetTasks(this.Context.Current);
            ViewBag.Groups = this._taskService.GetTasks(this.Context.Current).GroupBy(o => o.Priority).OrderBy(o => o.Key);
            return View();
        }
        public ActionResult MobileDetail(string id)
        {
            long i;
            long.TryParse(id, out i);
            var t = this._taskService.GetTask(i);
            if (t != null
                && t.CreatorAccountId != this.Context.Current.ID)
                throw new CooperknownException("抱歉，不能编辑其他人的任务");

            ViewBag.Task = t;
            return View();
        }
        [HttpPost]
        [ValidateInput(false)]
        public ActionResult MobileDetail(string id
            , string subject
            , string body
            , string dueTime 
            , string priority
            , string isCompleted)
        {
            long i;
            long.TryParse(id, out i);
            var task = this._taskService.GetTask(i) ?? new Task(this.Context.Current);

            if (task.CreatorAccountId != this.Context.Current.ID)
                throw new CooperknownException(this.Lang().can_not_edit_others_task);

            task.SetSubject(subject);
            task.SetBody(body);
            DateTime time;
            task.SetDueTime(DateTime.TryParse(dueTime, out time) ? time : new DateTime?());
            int p;
            task.SetPriority((Priority)(int.TryParse(priority, out p) ? p : 0));
            bool c;
            if (bool.TryParse(isCompleted, out c) && c)
                task.MarkAsCompleted();
            else
                task.MarkAsInCompleted();

            if (task.ID <= 0)
                this._taskService.Create(task);
            else
                this._taskService.Update(task);
            return Json(true);
        }
        #endregion

        #region 各类显示模式数据获取
        //优先级列表模式 个人任务的完整模式
        [HttpPost]
        public ActionResult GetByPriority(string tasklistId)
        {
            return this.GetBy(tasklistId
                , this._taskService.GetTasks
                , this._taskService.GetTasks
                , this.ParseSortsByPriority
                , this.ParseSortsByPriority);
        }
        //优先级列表模式 不含已经完成
        [HttpPost]
        public ActionResult GetIncompletedByPriority(string tasklistId)
        {
            return this.GetBy(tasklistId
                , this._taskService.GetIncompletedTasks
                , this._taskService.GetIncompletedTasks
                , this.ParseSortsByPriority
                , this.ParseSortsByPriority);
        }
        //DueTime列表模式
        [HttpPost]
        public ActionResult GetByDueTime(string tasklistId)
        {
            return this.GetBy(tasklistId
                , this._taskService.GetTasks
                , this._taskService.GetTasks
                , this.ParseSortsByDueTime
                , this.ParseSortsByDueTime);
        }
        #endregion

        #region Tasklist处理
        //返回所有可用的tasklists id|name
        public ActionResult GetTasklists()
        {
            var dict = this._fetchTasklistHelper.GetFetchTasklists(this.Context.Current);
            this._tasklistService.GetTasklists(this.Context.Current).ToList()
                .ForEach(o => dict.Add(o.ID.ToString(), o.Name));
            return Json(dict);
        }
        /// <summary>创建任务表
        /// </summary>
        /// <param name="name"></param>
        /// <param name="type">任务表类型，如：personal，team，project...</param>
        /// <returns></returns>
        [HttpPost]
        [ValidateInput(false)]
        public ActionResult CreateTasklist(string name, string type)
        {
            //目前只提供个人任务表
            var list = new PersonalTasklist(name, this.Context.Current);
            this._tasklistService.Create(list);
            return Json(list.ID);
        }
        /// <summary>删除任务表
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [HttpPost]
        public ActionResult DeleteTasklist(string id)
        {
            var list = this.GetTasklist(id);
            if (list != null)
                this._tasklistService.Delete(list);
            return Json(true);
        }
        #endregion

        /// <summary>用于接收终端的变更同步数据，按tasklist为同步
        /// </summary>
        /// <param name="tasklistId">任务表标识，允许为空，客户端在同步数据前应先确保tasklist已经创建，通过CreateTasklist</param>
        /// <param name="tasklistChanges">任务表变更数据</param>
        /// <param name="changes">变更数据 changelog[]</param>
        /// <param name="by">排序依据标识，参考静态变量PROFILE_SORT_PRIORITY、PROFILE_SORT_DUETIME等的值</param>
        /// <param name="sorts">排序数据 sort[]</param>
        /// <returns></returns>
        [HttpPost]
        [ValidateInput(false)]
        public ActionResult Sync(string tasklistId, string tasklistChanges, string changes, string by, string sorts)
        {
            //模拟同步间隙
            //System.Threading.Thread.Sleep(2000);
            //模拟连接失败
            //this.TryFail();

            //HACK:Fetch模式不支持同步变更
            Assert.IsFalse(this._fetchTasklistHelper.IsFetchTasklist(tasklistId));

            var account = this.Context.Current;
            var list = _serializer.JsonDeserialize<ChangeLog[]>(changes);
            var idChanges = new Dictionary<string, string>();//old,new
            var tasklist = this.GetTasklist(tasklistId);

            //UNDONE:详细考虑潜在异常以及批量事务的意外问题是否会造成丢失变更
            #region 对task执行对应变更
            foreach (var c in list)
            {
                try
                {
                    //临时记录无需删除
                    if (c.Type == ChangeType.Delete && c.ID.StartsWith(TEMP))
                        continue;

                    Task t;
                    if (idChanges.ContainsKey(c.ID))
                        t = this._taskService.GetTask(long.Parse(idChanges[c.ID]));
                    else if (c.ID.StartsWith(TEMP))
                    {
                        t = new Task(account);
                        if (tasklist != null) t.SetTasklist(tasklist);
                        this._taskService.Create(t);
                        idChanges.Add(c.ID, t.ID.ToString());//添加到id变更

                        if (this._log.IsDebugEnabled)
                            this._log.DebugFormat("从临时标识#{0}新建任务#{1}{2}"
                                , c.ID, t.ID, tasklist != null ? "并加入任务表#" + tasklist.ID : string.Empty);
                    }
                    else
                        t = this._taskService.GetTask(long.Parse(c.ID));

                    if (t == null)
                    {
                        this._log.WarnFormat("执行变更时出现不存在的任务#{0}", c.ID);
                        continue;
                    }

                    if (c.Type == ChangeType.Update)
                        this.ApplyUpdate(t, c);
                    else if (c.Type == ChangeType.Delete)
                        this._taskService.Delete(t);
                }
                catch (Exception e)
                {
                    this._log.Error(string.Format("执行变更时异常：{0}|{1}|{2}", c.ID, c.Name, c.Value), e);
                }
            }
            #endregion

            #region 保存排序信息
            var temp = _serializer.JsonDeserialize<Sort[]>(sorts);
            foreach (var s in temp)
                for (var i = 0; i < s.Indexs.Length; i++)
                    //修正索引中含有的临时标识
                    if (s.Indexs[i].StartsWith(TEMP) && idChanges.ContainsKey(s.Indexs[i]))
                        s.Indexs[i] = idChanges[s.Indexs[i]];
            try
            {
                
                var d = _serializer.JsonSerialize(temp);

                if (tasklist == null)
                {
                    //更新排序信息至用户设置
                    account.SetProfile(by, d);
                    this._accountService.Update(account);
                }
                else
                {
                    //更新排序信息至对应的任务表
                    tasklist[by] = d;
                    this._tasklistService.Update(tasklist);
                }


                if (this._log.IsDebugEnabled)
                    this._log.DebugFormat("修正后的排序数据为：{0}|{1}", by, d);
            }
            catch (Exception e)
            {
                this._log.Error(string.Format("保存排序信息至用户设置时异常"), e);
            }
            #endregion

            //返回修正列表
            return Json(idChanges.Select(o => new Correction() { OldId = o.Key, NewId = o.Value }));
        }

        private ActionResult GetBy(string tasklistId
            , Func<Account, IEnumerable<Task>> func1
            , Func<Account, Tasklist, IEnumerable<Task>> func2
            , Func<Account, TaskInfo[], Sort[]> func3
            , Func<Account, Tasklist, TaskInfo[], Sort[]> func4)
        {
            var list = this.GetTasklist(tasklistId);
            var account = this.Context.Current;

            TaskInfo[] tasks = null;

            var editable = list != null
                || (tasks = this._fetchTasklistHelper.FetchTasks(account, tasklistId)) == null;

            tasks = list == null
                ? (tasks ?? this.ParseTasks(func1(account).ToArray()))
                : this.ParseTasks(func2(account, list).ToArray());

            return Json(new
            {
                Editable = editable,
                List = tasks,
                Sorts = list == null
                    ? func3(account, tasks)
                    : func4(account, list, tasks),
            });
        }
        private Tasklist GetTasklist(string tasklistId)
        {
            int listId;
            var list = int.TryParse(tasklistId, out listId) ? this._tasklistService.GetTasklist(listId) : null;
            //HACK:个人任务列表只有拥有者能查看
            if (list != null && list is PersonalTasklist)
                Assert.AreEqual(this.Context.Current.ID, (list as PersonalTasklist).OwnerAccountId);
            //UNDONE:根据不同类型的任务表验证权限
            return list;
        }
        private void Prepare()
        {
            var a = this.Context.Current;
            ViewBag.Connections = this._accountConnectionService.GetConnections(a);
            ViewBag.Tasklists = this._tasklistService.GetTasklists(a);
            ViewBag.FetchTasklists = this._fetchTasklistHelper.GetFetchTasklists(a);
        }

        private TaskInfo[] ParseTasks(params Task[] tasks)
        {
            return tasks.Select(o => new TaskInfo()
            {
                ID = o.ID.ToString(),
                Subject = o.Subject,
                Body = o.Body,
                DueTime = o.DueTime.HasValue ? o.DueTime.Value.Date.ToString("yyyy-MM-dd") : null,
                Priority = (int)o.Priority,
                IsCompleted = o.IsCompleted
            }).ToArray();
        }
        private Sort[] ParseSortsByPriority(Account account, params TaskInfo[] tasks)
        {
            return this.ParseSortsByPriority(account
                , this.GetSorts(account, PROFILE_SORT_PRIORITY)
                , tasks);
        }
        private Sort[] ParseSortsByPriority(Account account, Tasklist tasklist, params TaskInfo[] tasks)
        {
            return this.ParseSortsByPriority(account
                , this.GetSorts(tasklist, PROFILE_SORT_PRIORITY)
                , tasks);
        }
        private Sort[] ParseSortsByPriority(Account account, Sort[] sorts, params TaskInfo[] tasks)
        {
            Sort today, upcoming, later;
            today = sorts.FirstOrDefault(o => o.Key == "0") ?? new Sort() { By = "priority", Key = "0" };
            today.Name = this.Lang().priority_today;
            upcoming = sorts.FirstOrDefault(o => o.Key == "1") ?? new Sort() { By = "priority", Key = "1" };
            upcoming.Name = this.Lang().priority_upcoming;
            later = sorts.FirstOrDefault(o => o.Key == "2") ?? new Sort() { By = "priority", Key = "2" };
            later.Name = this.Lang().priority_later;
            //修正索引
            this.RepairIndexs(today, this.Parse(tasks, o => o.Priority == (int)Priority.Today));
            this.RepairIndexs(upcoming, this.Parse(tasks, o => o.Priority == (int)Priority.Upcoming));
            this.RepairIndexs(later, this.Parse(tasks, o => o.Priority == (int)Priority.Later));

            return new Sort[] { today, upcoming, later };
        }
        private Sort[] ParseSortsByDueTime(Account account, params TaskInfo[] tasks)
        {
            return this.ParseSortsByDueTime(account, this.GetSorts(account, PROFILE_SORT_DUETIME), tasks);
        }
        private Sort[] ParseSortsByDueTime(Account account, Tasklist tasklist, params TaskInfo[] tasks)
        {
            return this.ParseSortsByDueTime(account, this.GetSorts(tasklist, PROFILE_SORT_DUETIME), tasks);
        }
        private Sort[] ParseSortsByDueTime(Account account, Sort[] sorts, params TaskInfo[] tasks)
        {
            Sort due, today, upcoming, later;
            due = sorts.FirstOrDefault(o => o.Key == "dueTime") ?? new Sort() { By = "", Key = "dueTime" };
            due.Name = this.Lang().sort_by_dueTime;
            today = sorts.FirstOrDefault(o => o.Key == "0") ?? new Sort() { By = "priority", Key = "0" };
            today.Name = this.Lang().priority_today;
            upcoming = sorts.FirstOrDefault(o => o.Key == "1") ?? new Sort() { By = "priority", Key = "1" };
            upcoming.Name = this.Lang().priority_upcoming;
            later = sorts.FirstOrDefault(o => o.Key == "2") ?? new Sort() { By = "priority", Key = "2" };
            later.Name = this.Lang().priority_later;
            //修正索引
            this.RepairIndexs(due, this.Parse(tasks, o => !string.IsNullOrEmpty(o.DueTime)));
            this.RepairIndexs(today, this.Parse(tasks, o => string.IsNullOrEmpty(o.DueTime) && o.Priority == (int)Priority.Today));
            this.RepairIndexs(upcoming, this.Parse(tasks, o => string.IsNullOrEmpty(o.DueTime) && o.Priority == (int)Priority.Upcoming));
            this.RepairIndexs(later, this.Parse(tasks, o => string.IsNullOrEmpty(o.DueTime) && o.Priority == (int)Priority.Later));

            return new Sort[] { due, today, upcoming, later };
        }
        private void RepairIndexs(Sort sort, IDictionary<string, TaskInfo> tasks)
        {
            var temp = (sort.Indexs ?? new string[0]).ToList();
            if (this._log.IsDebugEnabled)
                this._log.DebugFormat("原始排序为{0}|{1}|{2}|{3}", sort.By, sort.Key, sort.Name, string.Join(",", temp));
            //移除索引中不存在的项
            temp.RemoveAll(o => !tasks.ContainsKey(o));
            if (this._log.IsDebugEnabled)
                this._log.DebugFormat("过滤后排序为{0}", string.Join(",", temp));
            //合并未在索引中出现的项
            sort.Indexs = temp.Union(tasks.Select(o => o.Value.ID)).ToArray();
            if (this._log.IsDebugEnabled)
                this._log.DebugFormat("合并后排序为{0}", string.Join(",", sort.Indexs));
        }
        private IDictionary<string, TaskInfo> Parse(TaskInfo[] tasks, Func<TaskInfo, bool> filter)
        {
            return tasks.Where(filter)
                .Select(o => new KeyValuePair<string, TaskInfo>(o.ID, o))
                .ToDictionary(o => o.Key, o => o.Value);
        }
        private Sort[] GetSorts(Account a, string key)
        {
            return !string.IsNullOrWhiteSpace(a.GetProfile(key))
                ? _serializer.JsonDeserialize<Sort[]>(a.GetProfile(key))
                : _empty;
        }
        private Sort[] GetSorts(Tasklist a, string key)
        {
            return !string.IsNullOrWhiteSpace(a[key])
                ? _serializer.JsonDeserialize<Sort[]>(a[key])
                : _empty;
        }
        private void ApplyUpdate(Task t, ChangeLog c)
        {
            if (c.Name.Equals("subject", StringComparison.InvariantCultureIgnoreCase))
                t.SetSubject(c.Value);
            if (c.Name.Equals("body", StringComparison.InvariantCultureIgnoreCase))
                t.SetBody(c.Value);
            if (c.Name.Equals("priority", StringComparison.InvariantCultureIgnoreCase))
                t.SetPriority((Priority)Convert.ToInt32(c.Value));
            if (c.Name.Equals("duetime", StringComparison.InvariantCultureIgnoreCase))
                t.SetDueTime(string.IsNullOrWhiteSpace(c.Value) ? new DateTime?() : Convert.ToDateTime(c.Value));
            if (c.Name.Equals("iscompleted", StringComparison.InvariantCultureIgnoreCase))
                if (Convert.ToBoolean(c.Value))
                    t.MarkAsCompleted();
                else
                    t.MarkAsInCompleted();
            this._taskService.Update(t);

            if (this._log.IsDebugEnabled)
                this._log.DebugFormat("为任务#{0}执行变更{1}|{2}|{3}", t.ID, c.ID, c.Name, c.Value);
        }

        private static int _flag = 0;
        private void TryFail()
        {
            if (_flag++ % 2 == 0)
                throw new Exception("模拟服务器连接异常");
        }
    }

    #region 以下是面向终端的数据模型设计 目前由UI驱动设计
    /// <summary>描述在客户端中使用的任务
    /// </summary>
    public class TaskInfo
    {
        /// <summary>标识，临时标识用temp_前缀
        /// </summary>
        public string ID { get; set; }
        /// <summary>标题 小等于500字符
        /// </summary>
        public string Subject { get; set; }
        /// <summary>内容 小等于5000字符
        /// </summary>
        public string Body { get; set; }
        /// <summary>截止日期 格式yyyy-MM-dd，精度到天
        /// </summary>
        public string DueTime { get; set; }
        /// <summary>优先级 0、1、2
        /// </summary>
        public int Priority { get; set; }
        /// <summary>是否完成
        /// </summary>
        public bool IsCompleted { get; set; }

        //额外属性

        /// <summary>是否可编辑
        /// </summary>
        public bool Editable { get; set; }

        public TaskInfo() { this.Editable = true; }
    }
    /// <summary>描述数据变更记录
    /// </summary>
    public class ChangeLog
    {
        /// <summary>变更类型
        /// </summary>
        public ChangeType Type { get; set; }
        /// <summary>数据标识
        /// </summary>
        public string ID { get; set; }
        /// <summary>变更对应的属性名称
        /// </summary>
        public string Name { get; set; }
        /// <summary>变更后属性的值
        /// </summary>
        public string Value { get; set; }
    }
    /// <summary>描述数据变更类型
    /// </summary>
    public enum ChangeType
    {
        /// <summary>更新
        /// </summary>
        Update = 0,
        /// <summary>删除
        /// </summary>
        Delete = 1
    }
    /// <summary>描述对于客户端数据的修正
    /// </summary>
    public class Correction
    {
        public string OldId { get; set; }
        public string NewId { get; set; }
    }
    /// <summary>描述任务的排序记录
    /// </summary>
    public class Sort
    {
        /// <summary>排序依据
        /// </summary>
        public string By { get; set; }
        /// <summary>排序依据的值
        /// <remarks></remarks>
        /// </summary>
        public string Key { get; set; }
        /// <summary>排序依据显示名称
        /// </summary>
        public string Name { get; set; }
        /// <summary>排序数据
        /// </summary>
        public string[] Indexs { get; set; }
    }
    #endregion
}