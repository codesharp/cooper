using System.Collections.Generic;
using CooperDemo.Infrastructure;

namespace CooperDemo.Model
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
        /// <summary>获取所有任务表
        /// </summary>
        /// <returns></returns>
        IEnumerable<TaskFolder> GetAllTaskFolders();
    }
    public class TaskFolderService : ITaskFolderService
    {
        private ITaskFolderRepository _repository;
        private ILogger _logger;

        public TaskFolderService(ITaskFolderRepository repository, ILoggerFactory factory)
        {
            this._repository = repository;
            this._logger = factory.Create(GetType());
        }

        #region ITaskFolderService Members

        void ITaskFolderService.Create(TaskFolder folder)
        {
            _repository.Add(folder);
            _logger.InfoFormat("新增任务表{0}#{1}|{2}", folder, folder.ID, folder.Name);
        }
        void ITaskFolderService.Update(TaskFolder folder)
        {
            _repository.Update(folder);
        }
        void ITaskFolderService.Delete(TaskFolder folder)
        {
            _repository.Remove(folder);
            _logger.InfoFormat("删除任务表#{0}", folder.ID);
        }
        TaskFolder ITaskFolderService.GetTaskFolder(int id)
        {
            return _repository.FindBy(id);
        }
        IEnumerable<TaskFolder> ITaskFolderService.GetAllTaskFolders()
        {
            return _repository.FindAll();
        }

        #endregion
    }
}

