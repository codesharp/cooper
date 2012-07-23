//Copyright (c) CodeSharp.  All rights reserved. - http://www.icodesharp.com/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using CodeSharp.Core.RepositoryFramework;
using Castle.Services.Transaction;
using Cooper.Model.Accounts;
using CodeSharp.Core.Services;
using CodeSharp.Core;

namespace Cooper.Model.Tasks
{
    /// <summary>任务服务定义
    /// </summary>
    public interface ITaskService
    {
        /// <summary>创建任务
        /// </summary>
        /// <param name="task"></param>
        void Create(Task task);
        /// <summary>更新任务
        /// </summary>
        /// <param name="task"></param>
        void Update(Task task);
        /// <summary>删除任务
        /// </summary>
        /// <param name="task"></param>
        void Delete(Task task);
        /// <summary>根据标识获取任务
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        Task GetTask(long id);
        /// <summary>获取指定账号的所有任务
        /// </summary>
        /// <param name="account"></param>
        /// <returns></returns>
        IEnumerable<Task> GetTasks(Account account);
        /// <summary>获取指定账号指定任务表的所有任务
        /// </summary>
        /// <param name="account"></param>
        /// <param name="tasklist"></param>
        /// <returns></returns>
        IEnumerable<Task> GetTasks(Account account,Tasklist tasklist);
        /// <summary>获取指定账号的所有未完成任务
        /// </summary>
        /// <param name="account"></param>
        /// <returns></returns>
        IEnumerable<Task> GetIncompletedTasks(Account account);
        /// <summary>获取指定账号指定任务表的所有未完成任务
        /// </summary>
        /// <param name="account"></param>
        /// <param name="tasklist"></param>
        /// <returns></returns>
        IEnumerable<Task> GetIncompletedTasks(Account account, Tasklist tasklist);

    }
    /// <summary>任务DomainService
    /// </summary>
    [Transactional]
    public class TaskService : ITaskService
    {
        private static ITaskRepository _repository;
        static TaskService()
        {
            _repository = RepositoryFactory.GetRepository<ITaskRepository, long, Task>();
        }
        private ILog _log;
        public TaskService(ILoggerFactory factory)
        {
            this._log = factory.Create(typeof(TaskService));
        }

        #region ITaskService Members
        [Transaction(TransactionMode.Requires)]
        void ITaskService.Create(Task task)
        {
            _repository.Add(task);
            if (this._log.IsInfoEnabled)
                this._log.InfoFormat("新增任务#{0}|{1}|{2}", task.ID, task.Subject, task.CreatorAccountId);
        }
        [Transaction(TransactionMode.Requires)]
        void ITaskService.Update(Task task)
        {
            _repository.Update(task);
        }
        [Transaction(TransactionMode.Requires)]
        void ITaskService.Delete(Task task)
        {
            _repository.Remove(task);
            if (this._log.IsInfoEnabled)
                this._log.InfoFormat("删除任务#{0}", task.ID);
        }
        Task ITaskService.GetTask(long id)
        {
            return _repository.FindBy(id);
        }
        IEnumerable<Task> ITaskService.GetTasks(Account account)
        {
            return _repository.FindBy(account);
        }
        IEnumerable<Task> ITaskService.GetTasks(Account account, Tasklist tasklist)
        {
            return _repository.FindBy(account, tasklist);
        }
        IEnumerable<Task> ITaskService.GetIncompletedTasks(Account account)
        {
            return _repository.FindBy(account, false);
        }
        IEnumerable<Task> ITaskService.GetIncompletedTasks(Account account, Tasklist tasklist)
        {
            return _repository.FindBy(account, false, tasklist);
        }
        #endregion
    }
}
