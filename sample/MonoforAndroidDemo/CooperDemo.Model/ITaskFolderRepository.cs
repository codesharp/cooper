using System.Collections.Generic;

namespace CooperDemo.Model
{
    public interface ITaskFolderRepository
    {
        /// <summary>创建任务表
        /// </summary>
        /// <param name="folder"></param>
        void Add(TaskFolder folder);
        /// <summary>更新任务表
        /// </summary>
        /// <param name="folder"></param>
        void Update(TaskFolder folder);
        /// <summary>删除任务表
        /// </summary>
        /// <param name="folder"></param>
        void Remove(TaskFolder folder);
        /// <summary>获取任务表
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        TaskFolder FindBy(int id);
        /// <summary>获取所有任务表
        /// </summary>
        /// <returns></returns>
        IEnumerable<TaskFolder> FindAll();
    }
}

