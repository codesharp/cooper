//Copyright (c) CodeSharp.  All rights reserved. - http://www.icodesharp.com/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using CodeSharp.Core.RepositoryFramework;
using Castle.Services.Transaction;
using CodeSharp.Core;
using CodeSharp.Core.Services;
using Cooper.Model.Accounts;

namespace Cooper.Model.Tasks
{
    /// <summary>任务表服务定义
    /// </summary>
    public interface ITasklistService
    {
        /// <summary>创建任务表
        /// </summary>
        /// <param name="list"></param>
        void Create(Tasklist list);
        /// <summary>更新任务表
        /// </summary>
        /// <param name="list"></param>
        void Update(Tasklist list);
        /// <summary>删除任务表
        /// </summary>
        /// <param name="list"></param>
        void Delete(Tasklist list);
        /// <summary>获取任务表
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        Tasklist GetTasklist(int id);
        /// <summary>获取指定账号的拥有的个人任务表
        /// </summary>
        /// <param name="account"></param>
        /// <returns></returns>
        IEnumerable<PersonalTasklist> GetTasklists(Account account);
    }
    /// <summary>任务表DomainService
    /// </summary>
    [Transactional]
    public class TasklistService : ITasklistService
    {
        private static ITasklistRepository _repository;
        static TasklistService()
        {
            _repository = RepositoryFactory.GetRepository<ITasklistRepository, int, Tasklist>();
        }
        private ILog _log;
        private ITaskService _taskService;
        public TasklistService(ILoggerFactory factory, ITaskService taskService)
        {
            this._log = factory.Create(typeof(TasklistService));
            this._taskService = taskService;
        }

        #region ITasklistService Members
        [Transaction(TransactionMode.Requires)]
        void ITasklistService.Create(Tasklist list)
        {
            _repository.Add(list);
            if (this._log.IsInfoEnabled)
                this._log.InfoFormat("新增任务表{0}#{1}|{2}", list, list.ID, list.Name);
        }
        [Transaction(TransactionMode.Requires)]
        void ITasklistService.Update(Tasklist list)
        {
            _repository.Update(list);
        }
        [Transaction(TransactionMode.Requires)]
        void ITasklistService.Delete(Tasklist list)
        {
            _repository.Remove(list);
            //UNDONE:是否删除任务表下的所有任务？
            if (this._log.IsInfoEnabled)
                this._log.InfoFormat("删除任务表#{0}", list.ID);
        }
        Tasklist ITasklistService.GetTasklist(int id)
        {
            return _repository.FindBy(id);
        }
        IEnumerable<PersonalTasklist> ITasklistService.GetTasklists(Account account)
        {
            var all = _repository.FindBy(account);

            if (all.Count() == 0)
            {
                this.RepairTasklist(account);
                all = _repository.FindBy(account);
            }
            return all;
        }
        #endregion

        //为防止意外数据而做的修复操作，由于没有列表的任务在常规业务下不会被显示
        //[Transaction(TransactionMode.RequiresNew)]
        protected virtual void RepairTasklist(Account account)
        {
            //创建默认任务表
            //var l = new PersonalTasklist("DefaultTasklist", account);
            //(this as ITasklistService).Create(l);
            //HACK:批量修正任务所在列表
            //this._taskService.GetTasks(account)
            //    .Where(o => o.TasklistId <= 0)
            //    .ToList()
            //    .ForEach(o =>
            //    {
            //        o.SetTasklist(l);
            //        this._taskService.Update(o);
            //    });
        }
    }
}
