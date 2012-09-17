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
            task.AddTag("程序设计");
            task.AddTag(".NET");
            task.AddTag(".net");
            task.AddTag("∮   ∮；∮;");
            task.AddTag("∮bb∮");
            task.AddTag("∮∮ ∮b∮a∮");
            task.AddTag("A ");
            task.AddTag("∮∮ ∮)∮）∮）");
            task.AddTag("∮∮ ∮)∮）∮)");
            task.AddTag("001_Tag_001");
            this._personalTaskService.Create(task);

            this.Evict(task);

            task = this._personalTaskService.GetTask(task.ID);
            Assert.AreEqual(8, task.Tags.Count());

            Assert.IsFalse(task.Tags.Any(x => x == ".NET"));
            Assert.IsFalse(task.Tags.Any(x => x == "a"));
            Assert.IsFalse(task.Tags.Any(x => x == "A "));
            Assert.IsFalse(task.Tags.Any(x => x == "；"));
            Assert.IsFalse(task.Tags.Any(x => x == "）"));

            Assert.IsTrue(task.Tags.Any(x => x == "程序设计"));
            Assert.IsTrue(task.Tags.Any(x => x == ".net"));
            Assert.IsTrue(task.Tags.Any(x => x == ";"));
            Assert.IsTrue(task.Tags.Any(x => x == "bb"));
            Assert.IsTrue(task.Tags.Any(x => x == "A"));
            Assert.IsTrue(task.Tags.Any(x => x == "b"));
            Assert.IsTrue(task.Tags.Any(x => x == ")"));
            Assert.IsTrue(task.Tags.Any(x => x == "001_Tag_001"));
        }
        [Test]
        [Microsoft.VisualStudio.TestTools.UnitTesting.TestMethod]
        public void RemoveTag()
        {
            var account = this.CreateAccount();
            var task = new PersonalTask(account);
            task.AddTag("程序设计");
            task.AddTag(".NET");
            task.AddTag(".net");
            task.AddTag("∮   ∮；∮;");
            task.AddTag("∮bb∮");
            task.AddTag("∮∮ ∮b∮a∮");
            task.AddTag("A");
            task.AddTag("∮∮ ∮)∮）∮）");
            task.AddTag("∮∮ ∮)∮）∮)");
            task.AddTag("001_Tag_001");
            this._personalTaskService.Create(task);

            this.Evict(task);

            task = this._personalTaskService.GetTask(task.ID);
            Assert.AreEqual(8, task.Tags.Count());
            Assert.IsFalse(task.Tags.Any(x => x == ".NET"));
            Assert.IsFalse(task.Tags.Any(x => x == "；"));
            Assert.IsFalse(task.Tags.Any(x => x == "a"));
            Assert.IsFalse(task.Tags.Any(x => x == "）"));

            Assert.IsTrue(task.Tags.Any(x => x == "程序设计"));
            Assert.IsTrue(task.Tags.Any(x => x == ".net"));
            Assert.IsTrue(task.Tags.Any(x => x == ";"));
            Assert.IsTrue(task.Tags.Any(x => x == "bb"));
            Assert.IsTrue(task.Tags.Any(x => x == "A"));
            Assert.IsTrue(task.Tags.Any(x => x == "b"));
            Assert.IsTrue(task.Tags.Any(x => x == ")"));
            Assert.IsTrue(task.Tags.Any(x => x == "001_Tag_001"));

            task.RemoveTag("）");
            task.RemoveTag(".Net");
            task.RemoveTag("∮a∮bba");
            task.RemoveTag("∮；∮  b");
            task.RemoveTag(" A ");

            this._personalTaskService.Update(task);

            this.Evict(task);

            task = this._personalTaskService.GetTask(task.ID);
            Assert.AreEqual(3, task.Tags.Count());
            Assert.IsFalse(task.Tags.Any(x => x == ".net"));
            Assert.IsFalse(task.Tags.Any(x => x == "A"));
            Assert.IsFalse(task.Tags.Any(x => x == "b"));
            Assert.IsFalse(task.Tags.Any(x => x == ";"));
            Assert.IsFalse(task.Tags.Any(x => x == ")"));

            Assert.IsTrue(task.Tags.Any(x => x == "程序设计"));
            Assert.IsTrue(task.Tags.Any(x => x == "bb"));
            Assert.IsTrue(task.Tags.Any(x => x == "001_Tag_001"));
        }
        [Test]
        [Microsoft.VisualStudio.TestTools.UnitTesting.TestMethod]
        public void GetTags()
        {
            var account = this.CreateAccount();
            var task1 = new PersonalTask(account);
            task1.AddTag("程序设计");
            task1.AddTag(".NET");
            task1.AddTag("ASP.NET");
            task1.AddTag("001_Tag_001");
            task1.MarkAsCompleted();
            this._personalTaskService.Create(task1);
            var task2 = new PersonalTask(account);
            task2.AddTag("Mono");
            task2.AddTag(".net");
            task2.AddTag("JAVA");
            task1.AddTag("JAVA.NET");
            task2.AddTag("001_tag_001");
            this._personalTaskService.Create(task2);

            this.Evict(task1);
            this.Evict(task2);

            var tasks = this._personalTaskService.GetTasks(account, ".net");
            Assert.AreEqual(2, tasks.Count());
            Assert.IsFalse(!tasks.All(x => x.Tags.Contains(".net", StringComparer.OrdinalIgnoreCase)));
            tasks = this._personalTaskService.GetTasks(account, "java");
            Assert.AreEqual(1, tasks.Count());
            Assert.IsFalse(!tasks.All(x => x.Tags.Contains("java", StringComparer.OrdinalIgnoreCase)));
            tasks = this._personalTaskService.GetTasks(account, "程序设计");
            Assert.AreEqual(1, tasks.Count());
            Assert.IsFalse(!tasks.All(x => x.Tags.Contains("程序设计", StringComparer.OrdinalIgnoreCase)));
            tasks = this._personalTaskService.GetTasks(account, "Mono");
            Assert.AreEqual(1, tasks.Count());
            Assert.IsFalse(!tasks.All(x => x.Tags.Contains("Mono", StringComparer.OrdinalIgnoreCase)));
            tasks = this._personalTaskService.GetTasks(account, "001_tag_001");
            Assert.AreEqual(2, tasks.Count());
            Assert.IsFalse(!tasks.All(x => x.Tags.Contains("001_tag_001", StringComparer.OrdinalIgnoreCase)));

            tasks = this._personalTaskService.GetIncompletedTasks(account, ".net");
            Assert.AreEqual(1, tasks.Count());
            Assert.IsFalse(!tasks.All(x => x.Tags.Contains(".net", StringComparer.OrdinalIgnoreCase)));
            tasks = this._personalTaskService.GetIncompletedTasks(account, "java");
            Assert.AreEqual(1, tasks.Count());
            Assert.IsFalse(!tasks.All(x => x.Tags.Contains("java", StringComparer.OrdinalIgnoreCase)));
            tasks = this._personalTaskService.GetIncompletedTasks(account, "程序设计");
            Assert.AreEqual(0, tasks.Count());
            Assert.IsFalse(!tasks.All(x => x.Tags.Contains("程序设计", StringComparer.OrdinalIgnoreCase)));
            tasks = this._personalTaskService.GetIncompletedTasks(account, "Mono");
            Assert.AreEqual(1, tasks.Count());
            Assert.IsFalse(!tasks.All(x => x.Tags.Contains("Mono", StringComparer.OrdinalIgnoreCase)));
            tasks = this._personalTaskService.GetIncompletedTasks(account, "001_tag_001");
            Assert.AreEqual(1, tasks.Count());
            Assert.IsFalse(!tasks.All(x => x.Tags.Contains("001_tag_001", StringComparer.OrdinalIgnoreCase)));

            task1 = this._personalTaskService.GetTask(task1.ID);
            task2 = this._personalTaskService.GetTask(task2.ID);

            task1.RemoveTag(".net");
            task2.RemoveTag("001_tag_001");
            task2.RemoveTag("JAVA");

            this._personalTaskService.Update(task1);
            this._personalTaskService.Update(task2);

            this.Evict(task1);
            this.Evict(task2);

            tasks = this._personalTaskService.GetTasks(account, ".net");
            Assert.AreEqual(1, tasks.Count());
            Assert.IsFalse(!tasks.All(x => x.Tags.Contains(".net", StringComparer.OrdinalIgnoreCase)));
            tasks = this._personalTaskService.GetTasks(account, "java");
            Assert.AreEqual(0, tasks.Count());
            Assert.IsFalse(!tasks.All(x => x.Tags.Contains("java", StringComparer.OrdinalIgnoreCase)));
            tasks = this._personalTaskService.GetTasks(account, "程序设计");
            Assert.AreEqual(1, tasks.Count());
            Assert.IsFalse(!tasks.All(x => x.Tags.Contains("程序设计", StringComparer.OrdinalIgnoreCase)));
            tasks = this._personalTaskService.GetTasks(account, "Mono");
            Assert.AreEqual(1, tasks.Count());
            Assert.IsFalse(!tasks.All(x => x.Tags.Contains("Mono", StringComparer.OrdinalIgnoreCase)));
            tasks = this._personalTaskService.GetTasks(account, "001_tag_001");
            Assert.AreEqual(1, tasks.Count());
            Assert.IsFalse(!tasks.All(x => x.Tags.Contains("001_tag_001", StringComparer.OrdinalIgnoreCase)));

            tasks = this._personalTaskService.GetIncompletedTasks(account, ".net");
            Assert.AreEqual(1, tasks.Count());
            Assert.IsFalse(!tasks.All(x => x.Tags.Contains(".net", StringComparer.OrdinalIgnoreCase)));
            tasks = this._personalTaskService.GetIncompletedTasks(account, "java");
            Assert.AreEqual(0, tasks.Count());
            Assert.IsFalse(!tasks.All(x => x.Tags.Contains("java", StringComparer.OrdinalIgnoreCase)));
            tasks = this._personalTaskService.GetIncompletedTasks(account, "程序设计");
            Assert.AreEqual(0, tasks.Count());
            Assert.IsFalse(!tasks.All(x => x.Tags.Contains("程序设计", StringComparer.OrdinalIgnoreCase)));
            tasks = this._personalTaskService.GetIncompletedTasks(account, "Mono");
            Assert.AreEqual(1, tasks.Count());
            Assert.IsFalse(!tasks.All(x => x.Tags.Contains("Mono", StringComparer.OrdinalIgnoreCase)));
            tasks = this._personalTaskService.GetIncompletedTasks(account, "001_tag_001");
            Assert.AreEqual(0, tasks.Count());
            Assert.IsFalse(!tasks.All(x => x.Tags.Contains("001_tag_001", StringComparer.OrdinalIgnoreCase)));
        }

        [Test]
        [Microsoft.VisualStudio.TestTools.UnitTesting.TestMethod]
        public void TrashTaskTest()
        {
            var account = this.CreateAccount();
            var task = new PersonalTask(account);
            this._personalTaskService.Create(task);

            var taskId = task.ID;

            this.Evict(task);

            task = this._personalTaskService.GetTask(task.ID);
            task.MarkAsTrashed();
            this._personalTaskService.Update(task);

            this.Evict(task);

            task = this._personalTaskService.GetTask(task.ID);
            Assert.IsNull(task);

            var tasks = this._personalTaskService.GetTasks(account);
            Assert.AreEqual(0, tasks.Count());

            tasks = this._personalTaskService.GetTrashedTasks(account);
            Assert.AreEqual(1, tasks.Count());
            Assert.AreEqual(taskId, tasks.First().ID);

            task = tasks.First();
            task.MarkAsUnTrashed();
            this._personalTaskService.Update(task);

            this.Evict(task);

            task = this._personalTaskService.GetTask(task.ID);
            Assert.IsNotNull(task);

            tasks = this._personalTaskService.GetTrashedTasks(account);
            Assert.AreEqual(0, tasks.Count());
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