//Copyright (c) CodeSharp.  All rights reserved. - http://www.icodesharp.com/

using System.Collections.Generic;
using Castle.Services.Transaction;
using CodeSharp.Core;
using CodeSharp.Core.RepositoryFramework;
using CodeSharp.Core.Services;
using Cooper.Model.Accounts;

namespace Cooper.Model.Tasks
{
    /// <summary>个人任务服务定义
    /// </summary>
    public interface IPersonalTaskService
    {
        /// <summary>创建任务
        /// </summary>
        /// <param name="task"></param>
        void Create(PersonalTask task);
        /// <summary>更新任务
        /// </summary>
        /// <param name="task"></param>
        void Update(PersonalTask task);
        /// <summary>删除任务
        /// </summary>
        /// <param name="task"></param>
        void Delete(PersonalTask task);
        /// <summary>根据标识获取任务
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        PersonalTask GetTask(long id);
        /// <summary>获取指定账号的所有任务
        /// </summary>
        /// <param name="account"></param>
        /// <returns></returns>
        IEnumerable<PersonalTask> GetTasks(Account account);
        /// <summary>获取指定账号的指定Tag相关的任务
        /// </summary>
        /// <param name="account"></param>
        /// <param name="tag"></param>
        /// <returns></returns>
        IEnumerable<PersonalTask> GetTasks(Account account, string tag);
        /// <summary>获取指定账号的所有不属于任何任务表的任务
        /// </summary>
        /// <param name="account"></param>
        /// <returns></returns>
        IEnumerable<PersonalTask> GetTasksNotBelongAnyFolder(Account account);
        /// <summary>获取指定账号指定任务表的所有任务
        /// </summary>
        /// <param name="account"></param>
        /// <param name="folder"></param>
        /// <returns></returns>
        IEnumerable<PersonalTask> GetTasks(Account account, TaskFolder folder);
        /// <summary>获取指定账号的所有未完成任务
        /// </summary>
        /// <param name="account"></param>
        /// <returns></returns>
        IEnumerable<PersonalTask> GetIncompletedTasks(Account account);
        /// <summary>获取指定账号的所有未完成的指定Tag相关的任务
        /// </summary>
        /// <param name="account"></param>
        /// <param name="tag"></param>
        /// <returns></returns>
        IEnumerable<PersonalTask> GetIncompletedTasks(Account account, string tag);
        /// <summary>获取指定账号的所有未完成且不属于任何任务表的任务
        /// </summary>
        /// <param name="account"></param>
        /// <returns></returns>
        IEnumerable<PersonalTask> GetIncompletedTasksAndNotBelongAnyFolder(Account account);
        /// <summary>获取指定账号指定任务表的所有未完成任务
        /// </summary>
        /// <param name="account"></param>
        /// <param name="folder"></param>
        /// <returns></returns>
        IEnumerable<PersonalTask> GetIncompletedTasks(Account account, TaskFolder folder);
    }
    /// <summary>个人任务服务
    /// </summary>
    [Transactional]
    public class PersonalTaskService : IPersonalTaskService
    {
        private static IPersonalTaskRepository _repository;
        static PersonalTaskService()
        {
            _repository = RepositoryFactory.GetRepository<IPersonalTaskRepository, long, PersonalTask>();
        }
        private ILog _log;
        public PersonalTaskService(ILoggerFactory factory)
        {
            this._log = factory.Create(typeof(PersonalTaskService));
        }

        #region ITaskService Members
        [Transaction(TransactionMode.Requires)]
        void IPersonalTaskService.Create(PersonalTask task)
        {
            _repository.Add(task);
            if (this._log.IsInfoEnabled)
                this._log.InfoFormat("新增任务#{0}|{1}|{2}", task.ID, task.Subject, task.CreatorAccountId);
        }
        [Transaction(TransactionMode.Requires)]
        void IPersonalTaskService.Update(PersonalTask task)
        {
            _repository.Update(task);
        }
        [Transaction(TransactionMode.Requires)]
        void IPersonalTaskService.Delete(PersonalTask task)
        {
            _repository.Remove(task);
            if (this._log.IsInfoEnabled)
                this._log.InfoFormat("删除任务#{0}", task.ID);
        }
        PersonalTask IPersonalTaskService.GetTask(long id)
        {
            return _repository.FindBy(id);
        }
        IEnumerable<PersonalTask> IPersonalTaskService.GetTasks(Account account)
        {
            return _repository.FindBy(account);
        }
        IEnumerable<PersonalTask> IPersonalTaskService.GetTasks(Account account, string tag)
        {
            return _repository.FindByTag(account, tag);
        }
        IEnumerable<PersonalTask> IPersonalTaskService.GetTasksNotBelongAnyFolder(Account account)
        {
            return _repository.FindBy(account, null);
        }
        IEnumerable<PersonalTask> IPersonalTaskService.GetTasks(Account account, TaskFolder folder)
        {
            return _repository.FindBy(account, folder);
        }
        IEnumerable<PersonalTask> IPersonalTaskService.GetIncompletedTasks(Account account)
        {
            return _repository.FindBy(account, false);
        }
        IEnumerable<PersonalTask> IPersonalTaskService.GetIncompletedTasks(Account account, string tag)
        {
            return _repository.FindByTag(account, false, tag);
        }
        IEnumerable<PersonalTask> IPersonalTaskService.GetIncompletedTasksAndNotBelongAnyFolder(Account account)
        {
            return _repository.FindBy(account, false, null);
        }
        IEnumerable<PersonalTask> IPersonalTaskService.GetIncompletedTasks(Account account, TaskFolder folder)
        {
            return _repository.FindBy(account, false, folder);
        }
        #endregion
    }
}
