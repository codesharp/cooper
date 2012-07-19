//Copyright (c) CodeSharp.  All rights reserved. - http://www.codesharp.cn/

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
        private IAccountService _accountService;
        private IAccountConnectionService _accountConnectionService;
        public PersonalController(ILoggerFactory factory
            , ITaskService taskService
            , IAccountService accountService
            , IAccountConnectionService accountConnectionService)
            : base(factory)
        {
            this._log = factory.Create(typeof(PersonalController));
            this._taskService = taskService;
            this._accountService = accountService;
            this._accountConnectionService = accountConnectionService;
        }

        public ActionResult Index(string desk)
        {
            //判断是否移动
            if (string.IsNullOrWhiteSpace(desk) 
                && Request.Browser.IsMobileDevice)
                return RedirectToAction("Mobile");
            ViewBag.Connections = this._accountConnectionService.GetConnections(this.Context.Current);
            return View();
        }
        public ActionResult Full()
        {
            ViewBag.Connections = this._accountConnectionService.GetConnections(this.Context.Current);
            return View();
        }
        public ActionResult Mini()
        {
            ViewBag.Connections = this._accountConnectionService.GetConnections(this.Context.Current);
            return View();
        }
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
            , string duetime
            , string priority
            , string isCompleted)
        {
            long i;
            long.TryParse(id, out i);
            var task = this._taskService.GetTask(i) ?? new Task(this.Context.Current);

            if (task.CreatorAccountId != this.Context.Current.ID)
                throw new CooperknownException("抱歉，不能编辑其他人的任务");

            task.SetSubject(subject);
            task.SetBody(body);
            DateTime time;
            task.SetDueTime(DateTime.TryParse(duetime, out time) ? time : new DateTime?());
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

        //优先级列表模式 个人任务的完整模式
        [HttpPost]
        public ActionResult GetByPriority()
        {
            var account = this.Context.Current;
            var tasks = this._taskService.GetTasks(account).ToArray();
            return Json(new { List = this.ParseTasks(tasks), Sorts = this.ParseSortsByPriority(account, tasks) });
        }
        //优先级列表模式 不含已经完成
        [HttpPost]
        public ActionResult GetIncompletedByPriority()
        {
            var account = this.Context.Current;
            var tasks = this._taskService.GetIncompletedTasks(account).ToArray();
            return Json(new { List = this.ParseTasks(tasks), Sorts = this.ParseSortsByPriority(account, tasks) });
        }
        //DueTime列表模式
        [HttpPost]
        public ActionResult GetByDueTime()
        {
            var account = this.Context.Current;
            var tasks = this._taskService.GetTasks(account).ToArray();
            return Json(new { List = this.ParseTasks(tasks), Sorts = this.ParseSortsByDueTime(account, tasks) });
        }
        /// <summary>用于接收终端的变更同步数据
        /// </summary>
        /// <param name="changes">变更数据 changelog[]</param>
        /// <param name="by">排序依据标识，参考静态变量PROFILE_SORT_PRIORITY、PROFILE_SORT_DUETIME等的值</param>
        /// <param name="sorts">排序数据 sort[]</param>
        /// <returns></returns>
        [HttpPost]
        [ValidateInput(false)]
        public ActionResult Sync(string changes, string by, string sorts)
        {
            //模拟同步间隙
            //System.Threading.Thread.Sleep(2000);
            //模拟连接失败
            //this.TryFail();

            var account = this.Context.Current;
            var list = _serializer.JsonDeserialize<ChangeLog[]>(changes);
            var idChanges = new Dictionary<string, string>();//old,new

            //UNDONE:详细考虑潜在异常以及批量事务的意外问题是否会造成丢失变更
            #region 执行对应变更
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
                        this._taskService.Create(t = new Task(account));
                        idChanges.Add(c.ID, t.ID.ToString());//添加到id变更

                        if (this._log.IsDebugEnabled)
                            this._log.DebugFormat("从临时标识#{0}新建任务#{1}", c.ID, t.ID);
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
                //更新排序信息至用户设置
                var d = _serializer.JsonSerialize(temp);
                account.SetProfile(by, d);
                this._accountService.Update(account);

                if (this._log.IsDebugEnabled)
                    this._log.DebugFormat("修正后的排序数据为：{0}", d);
            }
            catch (Exception e)
            {
                this._log.Error(string.Format("保存排序信息至用户设置时异常"), e);
            }
            #endregion

            //返回修正列表
            return Json(idChanges.Select(o => new Correction() { OldId = o.Key, NewId = o.Value }));
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
        private Sort[] ParseSortsByPriority(Account account, params Task[] tasks)
        {
            Sort today, upcoming, later;
            var arr = this.GetSorts(account, PROFILE_SORT_PRIORITY);
            today = arr.FirstOrDefault(o => o.Key == "0") ?? new Sort() { By = "priority", Key = "0" };
            today.Name = "今天";
            upcoming = arr.FirstOrDefault(o => o.Key == "1") ?? new Sort() { By = "priority", Key = "1" };
            upcoming.Name = "稍后完成";
            later = arr.FirstOrDefault(o => o.Key == "2") ?? new Sort() { By = "priority", Key = "2" };
            later.Name = "迟些再说";
            //修正索引
            this.RepairIndexs(today, this.Parse(tasks, o => o.Priority == Priority.Today));
            this.RepairIndexs(upcoming, this.Parse(tasks, o => o.Priority == Priority.Upcoming));
            this.RepairIndexs(later, this.Parse(tasks, o => o.Priority == Priority.Later));

            return new Sort[] { today, upcoming, later };
        }
        private Sort[] ParseSortsByDueTime(Account account, params Task[] tasks)
        {
            Sort due, today, upcoming, later;
            var arr = this.GetSorts(account, PROFILE_SORT_DUETIME);
            due = arr.FirstOrDefault(o => o.Key == "dueTime") ?? new Sort() { By = "", Key = "dueTime" };
            due.Name = "按截止日期排序";
            today = arr.FirstOrDefault(o => o.Key == "0") ?? new Sort() { By = "priority", Key = "0" };
            today.Name = "今天";
            upcoming = arr.FirstOrDefault(o => o.Key == "1") ?? new Sort() { By = "priority", Key = "1" };
            upcoming.Name = "稍后完成";
            later = arr.FirstOrDefault(o => o.Key == "2") ?? new Sort() { By = "priority", Key = "2" };
            later.Name = "迟些再说";
            //修正索引
            this.RepairIndexs(due, this.Parse(tasks, o => o.DueTime.HasValue));
            this.RepairIndexs(today, this.Parse(tasks, o => !o.DueTime.HasValue && o.Priority == Priority.Today));
            this.RepairIndexs(upcoming, this.Parse(tasks, o => !o.DueTime.HasValue && o.Priority == Priority.Upcoming));
            this.RepairIndexs(later, this.Parse(tasks, o => !o.DueTime.HasValue && o.Priority == Priority.Later));

            return new Sort[] { due, today, upcoming, later };
        }
        private void RepairIndexs(Sort sort, IDictionary<string, Task> tasks)
        {
            var temp = (sort.Indexs ?? new string[0]).ToList();
            if (this._log.IsDebugEnabled)
                this._log.DebugFormat("原始排序为{0}|{1}|{2}|{3}", sort.By, sort.Key, sort.Name, string.Join(",", temp));
            //移除索引中不存在的项
            temp.RemoveAll(o => !tasks.ContainsKey(o));
            if (this._log.IsDebugEnabled)
                this._log.DebugFormat("过滤后排序为{0}", string.Join(",", temp));
            //合并未在索引中出现的项
            sort.Indexs = temp.Union(tasks.Select(o => o.Value.ID.ToString())).ToArray();
            if (this._log.IsDebugEnabled)
                this._log.DebugFormat("合并后排序为{0}", string.Join(",", sort.Indexs));
        }
        private IDictionary<string, Task> Parse(Task[] tasks, Func<Task, bool> filter)
        {
            return tasks.Where(filter)
                .Select(o => new KeyValuePair<string, Task>(o.ID.ToString(), o))
                .ToDictionary(o => o.Key, o => o.Value);
        }
        private Sort[] GetSorts(Account a, string key)
        {
            return !string.IsNullOrWhiteSpace(a.GetProfile(key))
                ? _serializer.JsonDeserialize<Sort[]>(a.GetProfile(key))
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