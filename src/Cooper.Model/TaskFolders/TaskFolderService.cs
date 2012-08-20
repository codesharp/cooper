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
    public interface ITaskFolderService
    {
        /// <summary>创建任务表
        /// </summary>
        /// <param name="folder"></param>
        void Create(TaskFolder folder);
        /// <summary>更新任务表
        /// </summary>
        /// <param name="folder"></param>
        void Update(TaskFolder folder);
        /// <summary>删除任务表
        /// </summary>
        /// <param name="folder"></param>
        void Delete(TaskFolder folder);
        /// <summary>获取任务表
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        TaskFolder GetTaskFolder(int id);
        /// <summary>获取指定账号的拥有的个人任务表
        /// </summary>
        /// <param name="account"></param>
        /// <returns></returns>
        IEnumerable<PersonalTaskFolder> GetTaskFolders(Account account);
    }
    [Transactional]
    public class TaskFolderService : ITaskFolderService
    {
        private static ITaskFolderRepository _repository;
        static TaskFolderService()
        {
            _repository = RepositoryFactory.GetRepository<ITaskFolderRepository, int, TaskFolder>();
        }
        private ILog _log;
        private ITaskService _taskService;
        public TaskFolderService(ILoggerFactory factory, ITaskService taskService)
        {
            this._log = factory.Create(typeof(TaskFolderService));
            this._taskService = taskService;
        }

        #region ITaskFolderService Members
        [Transaction(TransactionMode.Requires)]
        void ITaskFolderService.Create(TaskFolder folder)
        {
            _repository.Add(folder);
            if (this._log.IsInfoEnabled)
                this._log.InfoFormat("新增任务表{0}#{1}|{2}", folder, folder.ID, folder.Name);
        }
        [Transaction(TransactionMode.Requires)]
        void ITaskFolderService.Update(TaskFolder folder)
        {
            _repository.Update(folder);
        }
        [Transaction(TransactionMode.Requires)]
        void ITaskFolderService.Delete(TaskFolder folder)
        {
            _repository.Remove(folder);
            //HACK:目前不会删除任务表下的所有任务
            if (this._log.IsInfoEnabled)
                this._log.InfoFormat("删除任务表#{0}", folder.ID);
        }
        TaskFolder ITaskFolderService.GetTaskFolder(int id)
        {
            return _repository.FindBy(id);
        }
        IEnumerable<PersonalTaskFolder> ITaskFolderService.GetTaskFolders(Account account)
        {
            return _repository.FindBy(account);
        }
        #endregion
    }
}
