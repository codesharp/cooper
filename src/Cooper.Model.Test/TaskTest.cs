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
    public class TaskTest : TestBase
    {
        [Test]
        [Microsoft.VisualStudio.TestTools.UnitTesting.TestMethod]
        public void Create()
        {
            var a = this.CreateAccount();
            var task = new Task(a);
            Assert.AreNotEqual(DateTime.MinValue, task.CreateTime);
            Assert.AreNotEqual(DateTime.MinValue, task.LastUpdateTime);

            this._taskService.Create(task);
            Assert.Greater(task.ID, 0);

            this.Evict(task);

            var task2 = this._taskService.GetTask(task.ID);
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
            var task = new Task(a);
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

            var task = new Task(a);
            this._taskService.Create(task);

            task = new Task(a);
            task.SetTaskFolder(folder);
            this._taskService.Create(task);

            task = new Task(a);
            task.SetTaskFolder(folder);
            this._taskService.Create(task);

            Assert.AreEqual(3, this._taskService.GetTasks(a).Count());
            Assert.AreEqual(2, this._taskService.GetTasks(a, folder).Count());
            Assert.AreEqual(1, this._taskService.GetTasksNotBelongAnyFolder(a).Count());
        }
        [Test]
        [Microsoft.VisualStudio.TestTools.UnitTesting.TestMethod]
        public void GetIncompleted()
        {
            var a = this.CreateAccount();
            var folder = this.CreatePersonalTaskFolder(a);

            var task = new Task(a);
            this._taskService.Create(task);

            task = new Task(a);
            task.SetTaskFolder(folder);
            this._taskService.Create(task);

            task = new Task(a);
            task.MarkAsCompleted();
            task.SetTaskFolder(folder);
            this._taskService.Create(task);

            Assert.AreEqual(2, this._taskService.GetIncompletedTasks(a).Count());
            Assert.AreEqual(1, this._taskService.GetIncompletedTasks(a, folder).Count());
            Assert.AreEqual(1, this._taskService.GetIncompletedTasksAndNotBelongAnyFolder(a).Count());
        }

        private void Dump(params Task[] tasks)
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