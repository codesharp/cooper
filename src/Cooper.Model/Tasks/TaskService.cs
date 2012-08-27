//Copyright (c) CodeSharp.  All rights reserved. - http://www.icodesharp.com/

using Castle.Services.Transaction;
using CodeSharp.Core;
using CodeSharp.Core.RepositoryFramework;
using CodeSharp.Core.Services;

namespace Cooper.Model.Tasks
{
    /// <summary>任务核心模型领域服务定义
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
    }
    /// <summary>任务核心模型领域服务
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
            {
                this._log.InfoFormat("新增任务#{0}|{1}", task.ID, task.Subject);
            }
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
            {
                this._log.InfoFormat("删除任务#{0}", task.ID);
            }
        }
        Task ITaskService.GetTask(long id)
        {
            return _repository.FindBy(id);
        }
        #endregion
    }
}
