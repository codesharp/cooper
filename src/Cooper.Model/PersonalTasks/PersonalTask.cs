//Copyright (c) CodeSharp.  All rights reserved. - http://www.icodesharp.com/

using Cooper.Model.Accounts;

namespace Cooper.Model.Tasks
{
    /// <summary>个人任务模型，由账号创建
    /// </summary>
    public class PersonalTask : Task
    {
        /// <summary>获取创建者账号标识
        /// </summary>
        public int CreatorAccountId { get; private set; }
        /// <summary>获取任务所在的任务目录标识
        /// </summary>
        public int? TaskFolderId { get; private set; }

        protected PersonalTask() : base()
        { }
        public PersonalTask(Account creator) : base()
        {
            Assert.IsValid(creator);
            this.CreatorAccountId = creator.ID;
        }

        /// <summary>设置任务所在的任务目录
        /// </summary>
        /// <param name="folder"></param>
        public void SetTaskFolder(TaskFolder folder)
        {
            Assert.IsValid(folder);
            if (this.TaskFolderId == folder.ID)
            {
                return;
            }
            if (folder is PersonalTaskFolder)
            {
                Assert.AreEqual(this.CreatorAccountId, (folder as PersonalTaskFolder).OwnerAccountId);
            }
            this.TaskFolderId = folder.ID;
            this.MakeChange();
        }
    }
}