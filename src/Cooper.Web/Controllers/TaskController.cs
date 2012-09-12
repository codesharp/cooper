//Copyright (c) CodeSharp.  All rights reserved. - http://www.icodesharp.com/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using CodeSharp.Core;
using CodeSharp.Core.Services;
using Cooper.Model.Tasks;
using CodeSharp.Core.Utils;
using Cooper.Model.Accounts;

namespace Cooper.Web.Controllers
{
    public abstract class TaskController : BaseController
    {
        protected static readonly Sort[] _emptySorts = new Sort[0];
        protected static readonly Serializer _serializer = new Serializer();
        protected static readonly string TEMP = "temp_";
        protected static readonly string SORT_PRIORITY = "ByPriority";
        protected static readonly string SORT_DUETIME = "ByDueTime";

        protected ILog _log;
        protected IAccountService _accountService;
        protected ITaskService _taskService;
        protected ITaskFolderService _taskFolderService;
        protected IFetchTaskHelper _fetchTaskHelper;

        public TaskController(ILoggerFactory factory
            , IAccountService accountService
            , ITaskService taskService
            , ITaskFolderService taskFolderService
            , IFetchTaskHelper fetchTaskHelper)
            : base(factory)
        {
            this._log = factory.Create(this.GetType());
            this._accountService = accountService;
            this._taskService = taskService;
            this._taskFolderService = taskFolderService;
            this._fetchTaskHelper = fetchTaskHelper;
        }

        protected override void OnActionExecuting(ActionExecutingContext filterContext)
        {
            var r = filterContext.RequestContext.HttpContext.Request;
            if (r.HttpMethod.Equals("post", StringComparison.InvariantCultureIgnoreCase)
                && Convert.ToBoolean(r["tryfail"]))
                this.TryFail();
            base.OnActionExecuting(filterContext);
        }

        public ActionResult Mobile()
        {
            return Redirect("~/hybrid/index.htm#taskListPage");
        }

        protected TaskInfo[] ParseTasks(params Task[] tasks)
        {
            return this.ParseTasks(() => new TaskInfo(), (t, tInfo) => { }, tasks);
        }
        protected TaskInfo[] ParseTasks(Func<TaskInfo> create, Action<Task, TaskInfo> filter, params Task[] tasks)
        {
            return tasks.Select(o =>
            {
                var t = create();
                t.ID = o.ID.ToString();
                t.Subject = o.Subject;
                t.Body = o.Body;
                //UNDONE:UTC时间按当前时区格式化
                t.CreateTime = o.CreateTime.ToString("yyyy-MM-dd HH:mm:ss");
                //HACK:DueTime格式化按Date.ToString("yyyy-MM-dd")精度到天
                t.DueTime = o.DueTime.HasValue ? o.DueTime.Value.Date.ToString("yyyy-MM-dd") : null;
                t.Priority = (int)o.Priority;
                t.IsCompleted = o.IsCompleted;
                t.Tags = o.Tags;
                filter(o, t);
                return t;
            }).ToArray();
        }
        protected Sort[] ParseSortsByPriority(Sort[] sorts, params TaskInfo[] tasks)
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
        protected Sort[] ParseSortsByDueTime(Sort[] sorts, params TaskInfo[] tasks)
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
        protected Sort[] GetSorts(Account a, string key)
        {
            return !string.IsNullOrWhiteSpace(a.GetProfile(key))
                ? _serializer.JsonDeserialize<Sort[]>(a.GetProfile(key))
                : _emptySorts;
        }
        /// <summary>用于接收终端的变更同步数据
        /// </summary>
        /// <param name="changes">变更数据 changelog[]</param>
        /// <param name="by">排序依据标识，参考静态变量PROFILE_SORT_PRIORITY、PROFILE_SORT_DUETIME等的值</param>
        /// <param name="sorts">排序数据 sort[]</param>
        /// <param name="ifNew">为兼容原personalcontroller的taskfolder同步行为而设计</param>
        /// <param name="isPersonalSorts">判断是否是个人排序数据</param>
        /// <param name="saveSorts">保存非个人排序数据</param>
        /// <returns></returns>
        protected IEnumerable<Correction> Sync(string changes
            , string by
            , string sorts
            , Func<Task> ifNew
            , Action<Task> verify
            , Func<bool> isPersonalSorts
            , Func<string, string> getSortKey
            , Action<string> saveSorts)
        {
            //模拟同步间隙
            //System.Threading.Thread.Sleep(2000);

            var account = this.Context.Current;
            var list = _serializer.JsonDeserialize<ChangeLog[]>(changes);
            var idChanges = new Dictionary<string, string>();//old,new
            //同步变更
            this.ApplyChanges(account, list, idChanges, ifNew, verify);
            //同步排序数据
            this.UpdateSorts(account, by, sorts, idChanges, isPersonalSorts, getSortKey, saveSorts);
            //返回修正记录
            return idChanges.Select(o => new Correction() { OldId = o.Key, NewId = o.Value });
        }
        protected virtual void ApplyUpdate(Task t, ChangeLog c)
        {
            var n = c.Name.ToLower();

            if (n.Equals("subject"))
                t.SetSubject(c.Value);
            else if (n.Equals("body"))
                t.SetBody(c.Value);
            else if (n.Equals("priority"))
                t.SetPriority((Priority)Convert.ToInt32(c.Value));
            else if (n.Equals("duetime"))
                t.SetDueTime(string.IsNullOrWhiteSpace(c.Value) ? new DateTime?() : Convert.ToDateTime(c.Value));
            else if (n.Equals("iscompleted"))
                if (Convert.ToBoolean(c.Value))
                    t.MarkAsCompleted();
                else
                    t.MarkAsInCompleted();
            else if (n.Equals("tags"))
                if (c.Type == ChangeType.Insert)
                    t.AddTag(c.Value);
                else if (c.Type == ChangeType.Delete)
                    t.RemoveTag(c.Value);
        }

        private void ApplyChanges(Account account
            , ChangeLog[] list
            , IDictionary<string, string> idChanges
            , Func<Task> ifNew
            , Action<Task> verify)
        {
            foreach (var c in list)
            {
                try
                {
                    //临时记录无需删除
                    if (this.IsTaskDelete(c) && c.ID.StartsWith(TEMP))
                        continue;

                    Task t;
                    if (idChanges.ContainsKey(c.ID))
                        t = this._taskService.GetTask(long.Parse(idChanges[c.ID]));
                    else if (c.ID.StartsWith(TEMP))
                    {
                        t = ifNew();
                        this._taskService.Create(t);
                        idChanges.Add(c.ID, t.ID.ToString());//添加到id变更
                    }
                    else
                        t = this._taskService.GetTask(long.Parse(c.ID));

                    if (t == null)
                    {
                        this._log.WarnFormat("执行变更时出现不存在的任务#{0}", c.ID);
                        continue;
                    }

                    //执行变更权限验证
                    verify(t);

                    //UNDONE:根据最后更新时间判断变更记录有效性，时间间隔过长的变更会被丢弃?
                    //由于LastUpdateTime粒度过大，不适合这种细粒度变更比对，需要引入Merge机制来处理文本更新合并问题
                    DateTime createTime;
                    if (DateTime.TryParse(c.CreateTime, out createTime))
                        if (t.LastUpdateTime - createTime > TimeSpan.FromMinutes(5))
                            if (this._log.IsInfoEnabled)
                                this._log.InfoFormat("变更创建时间{0}与任务最后更新时间{1}的隔间过长", c.CreateTime, t.LastUpdateTime);
                    
                    if (this.IsTaskDelete(c))
                        this._taskService.Delete(t);
                    else
                    {
                        this.ApplyUpdate(t, c);
                        this._taskService.Update(t);
                    }
                    if (this._log.IsInfoEnabled)
                        this._log.InfoFormat("为任务#{0}执行变更{1}|{2}|{3}|{4}|{5}", t.ID, c.Type, c.ID, c.Name, c.Value, c.CreateTime);
                }
                catch (Exception e)
                {
                    this._log.Error(string.Format("执行变更时异常：{0}|{1}|{2}", c.ID, c.Name, c.Value), e);
                }
            }
        }
        private bool IsTaskDelete(ChangeLog c)
        {
            return c.Type == ChangeType.Delete && string.IsNullOrWhiteSpace(c.Name);
        }
        private void UpdateSorts(Account account
            , string by
            , string sorts
            , IDictionary<string, string> idChanges
            , Func<bool> isPersonalSorts
            , Func<string, string> getPersonalSortKey
            , Action<string> saveSorts)
        {
            //没有变更则无需提交排序数据
            if (string.IsNullOrWhiteSpace(sorts) || sorts == "null") return;

            var temp = _serializer.JsonDeserialize<Sort[]>(sorts);
            foreach (var s in temp)
            {
                s.Indexs = s.Indexs.Where(o =>
                    !string.IsNullOrWhiteSpace(o)).Distinct().ToArray();
                for (var i = 0; i < s.Indexs.Length; i++)
                    //修正索引中含有的临时标识
                    if (s.Indexs[i].StartsWith(TEMP) && idChanges.ContainsKey(s.Indexs[i]))
                        s.Indexs[i] = idChanges[s.Indexs[i]];
            }
            try
            {
                var d = _serializer.JsonSerialize(temp);

                if (isPersonalSorts())
                {
                    //更新排序信息至用户设置
                    account.SetProfile(getPersonalSortKey(by), d);
                    this._accountService.Update(account);
                }
                else
                    //更新排序信息到非个人存储区域
                    saveSorts(d);

                if (this._log.IsDebugEnabled)
                    this._log.DebugFormat("修正后的排序数据为：{0}|{1}", by, d);
            }
            catch (Exception e)
            {
                this._log.Error(string.Format("保存排序信息至用户设置时异常"), e);
            }
        }

        private void RepairIndexs(Sort sort, IDictionary<string, TaskInfo> tasks)
        {
            var temp = (sort.Indexs ?? new string[0]).ToList();
            if (this._log.IsDebugEnabled)
                this._log.DebugFormat("原始排序为{0}|{1}|{2}|{3}", sort.By, sort.Key, sort.Name, string.Join(",", temp));
            //移除索引中不存在的项
            temp.RemoveAll(o => string.IsNullOrWhiteSpace(o) || !tasks.ContainsKey(o));
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
        /// <summary>创建时间
        /// </summary>
        public string CreateTime { get; set; }
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
        /// <summary>标签列表
        /// </summary>
        public string[] Tags { get; set; }

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
        /// <summary>变更记录 UTC字符串，如 Tue, 28 Aug 2012 07:17:42 GMT
        /// </summary>
        public string CreateTime { get; set; }
    }
    /// <summary>描述数据变更类型
    /// </summary>
    public enum ChangeType
    {
        /// <summary>更新
        /// </summary>
        Update = 0,
        /// <summary>删除
        /// <remarks>可用于描述数据实体删除以及对实体中集合属性的元素的删除</remarks>
        /// </summary>
        Delete = 1,
        /// <summary>插入
        /// <remarks>可用于描述对集合属性的元素插入</remarks>
        /// </summary>
        Insert = 2
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
    /// <summary>描述在客户端中使用的任务夹
    /// </summary>
    public class TaskFolderInfo
    {
        public string ID { get; set; }
        public string Name { get; set; }
        public string Type { get; set; }
    }
    #endregion
}