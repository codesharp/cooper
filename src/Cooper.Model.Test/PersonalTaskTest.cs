//Copyright (c) CodeSharp.  All rights reserved. - http://www.icodesharp.com/

using System;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using Cooper.Model.Tasks;
using Cooper.Model.Accounts;

namespace Cooper.Model.Test
{
    [TestFixture]
    [Microsoft.VisualStudio.TestTools.UnitTesting.TestClass]
    public class PersonalTaskTest : TestBase
    {
        [Test]
        [Microsoft.VisualStudio.TestTools.UnitTesting.TestMethod]
        public void Create()
        {
            var a = this.CreateAccount();
            var task = new PersonalTask(a);
            Assert.AreNotEqual(DateTime.MinValue, task.CreateTime);
            Assert.AreNotEqual(DateTime.MinValue, task.LastUpdateTime);

            this._personalTaskService.Create(task);
            Assert.Greater(task.ID, 0);

            this.Evict(task);

            var task2 = this._personalTaskService.GetTask(task.ID);
            this.Dump(task2);
            Assert.AreEqual(task.Subject, task2.Subject);
            Assert.AreEqual(task.Body, task2.Body);
            Assert.AreEqual(task.Priority, task2.Priority);
            Assert.AreEqual(task.DueTime, task2.DueTime);
            Assert.AreEqual(task.IsCompleted, task2.IsCompleted);
            //精度有限
            //Assert.AreEqual(task.CreateTime, task2.CreateTime);
            //Assert.AreEqual(task.LastUpdateTime, task2.LastUpdateTime);
            Assert.AreEqual(task.CreatorAccountId, task2.CreatorAccountId);
            Assert.AreEqual(task.TaskFolderId, task2.TaskFolderId);
        }
        [Test]
        [Microsoft.VisualStudio.TestTools.UnitTesting.TestMethod]
        public void LastUpdateTime()
        {
            var a = this.CreateAccount();
            var task = new PersonalTask(a);
            //subjet
            var old = task.LastUpdateTime;
            this.Idle();
            task.SetSubject(task.Subject);
            Assert.AreEqual(task.LastUpdateTime, old);
            task.SetSubject(string.Empty);
            Assert.Greater(task.LastUpdateTime, old);
            //body
            old = task.LastUpdateTime;
            this.Idle();
            task.SetBody(task.Body);
            Assert.AreEqual(task.LastUpdateTime, old);
            task.SetBody(string.Empty);
            Assert.Greater(task.LastUpdateTime, old);
            //dueTime
            old = task.LastUpdateTime;
            this.Idle();
            task.SetDueTime(null);
            Assert.Greater(task.LastUpdateTime, old);
            //priority
            old = task.LastUpdateTime;
            this.Idle();
            task.SetPriority(task.Priority);
            Assert.AreEqual(task.LastUpdateTime, old);
            task.SetPriority(Priority.Upcoming);
            Assert.Greater(task.LastUpdateTime, old);
            //taskfolder
            old = task.LastUpdateTime;
            this.Idle();
            var folder = this.CreatePersonalTaskFolder(a);
            task.SetTaskFolder(folder);
            Assert.Greater(task.LastUpdateTime, old);
            old = task.LastUpdateTime;
            task.SetTaskFolder(folder);
            Assert.AreEqual(task.LastUpdateTime, old);
        }
        [Test]
        [Microsoft.VisualStudio.TestTools.UnitTesting.TestMethod]
        public void GetAll()
        {
            var a = this.CreateAccount();
            var folder = this.CreatePersonalTaskFolder(a);

            var task = new PersonalTask(a);
            this._personalTaskService.Create(task);

            task = new PersonalTask(a);
            task.SetTaskFolder(folder);
            this._personalTaskService.Create(task);

            task = new PersonalTask(a);
            task.SetTaskFolder(folder);
            this._personalTaskService.Create(task);

            Assert.AreEqual(3, this._personalTaskService.GetTasks(a).Count());
            Assert.AreEqual(2, this._personalTaskService.GetTasks(a, folder).Count());
            Assert.AreEqual(1, this._personalTaskService.GetTasksNotBelongAnyFolder(a).Count());
        }
        [Test]
        [Microsoft.VisualStudio.TestTools.UnitTesting.TestMethod]
        public void GetIncompleted()
        {
            var a = this.CreateAccount();
            var folder = this.CreatePersonalTaskFolder(a);

            var task = new PersonalTask(a);
            this._personalTaskService.Create(task);

            task = new PersonalTask(a);
            task.SetTaskFolder(folder);
            this._personalTaskService.Create(task);

            task = new PersonalTask(a);
            task.MarkAsCompleted();
            task.SetTaskFolder(folder);
            this._personalTaskService.Create(task);

            Assert.AreEqual(2, this._personalTaskService.GetIncompletedTasks(a).Count());
            Assert.AreEqual(1, this._personalTaskService.GetIncompletedTasks(a, folder).Count());
            Assert.AreEqual(1, this._personalTaskService.GetIncompletedTasksAndNotBelongAnyFolder(a).Count());
        }
        [Test]
        [Microsoft.VisualStudio.TestTools.UnitTesting.TestMethod]
        public void AddTag()
        {
            var account = this.CreateAccount();
            var task = new PersonalTask(account);
            this._personalTaskService.Create(task);
            var tag = this._personalTaskService.AddTaskTag(task, RandomString());

            this.Evict(task);

            task = this._personalTaskService.GetTask(task.ID);
            Assert.AreEqual(1, task.Tags.Count());
            Assert.IsTrue(task.Tags.Any(x => x.ReferenceEntityId == task.ID && x.ID == tag.ID));
        }
        [Test]
        [Microsoft.VisualStudio.TestTools.UnitTesting.TestMethod]
        public void RemoveTag()
        {
            var account = this.CreateAccount();
            var task = new PersonalTask(account);
            this._personalTaskService.Create(task);
            var tag1 = this._personalTaskService.AddTaskTag(task, RandomString());
            var tag2 = this._personalTaskService.AddTaskTag(task, RandomString());
            this._personalTaskService.RemoveTaskTag(task, tag1);

            this.Evict(task);

            task = this._personalTaskService.GetTask(task.ID);
            Assert.AreEqual(1, task.Tags.Count());
            Assert.IsTrue(task.Tags.Any(x => x.ReferenceEntityId == task.ID && x.ID == tag2.ID));
        }

        private void Dump(params PersonalTask[] tasks)
        {
            if (tasks == null) return;
            tasks.ToList().ForEach(o => this._log.InfoFormat(
                "{0}|{1}|{2}|{3}|{4}|{5}|{6}|{7}"
                , o.Subject
                , o.Body
                , o.Priority
                , o.DueTime
                , o.IsCompleted
                , o.CreateTime
                , o.LastUpdateTime
                , o.CreatorAccountId));
        }
    }
}