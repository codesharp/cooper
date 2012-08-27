//Copyright (c) CodeSharp.  All rights reserved. - http://www.icodesharp.com/

using System;
using System.Linq;
using Cooper.Model.Tasks;
using NUnit.Framework;
using TeamTask = Cooper.Model.Teams.Task;

namespace Cooper.Model.Test
{
    [TestFixture]
    [Microsoft.VisualStudio.TestTools.UnitTesting.TestClass]
    public class TaskTest : TestBase
    {
        [Test]
        [Microsoft.VisualStudio.TestTools.UnitTesting.TestMethod]
        public void CreatePersonalTask()
        {
            var account = this.CreateAccount();
            var task = new PersonalTask(account);
            Assert.AreNotEqual(DateTime.MinValue, task.CreateTime);
            Assert.AreNotEqual(DateTime.MinValue, task.LastUpdateTime);

            this._taskService.Create(task);
            Assert.Greater(task.ID, 0);

            this.Evict(task);

            var task2 = this._taskService.GetTask(task.ID) as PersonalTask;
            Assert.AreEqual(task.Subject, task2.Subject);
            Assert.AreEqual(task.Body, task2.Body);
            Assert.AreEqual(task.Priority, task2.Priority);
            Assert.AreEqual(task.DueTime, task2.DueTime);
            Assert.AreEqual(task.IsCompleted, task2.IsCompleted);
            Assert.AreEqual(FormatTime(task.CreateTime), FormatTime(task2.CreateTime));
            Assert.AreEqual(FormatTime(task.LastUpdateTime), FormatTime(task2.LastUpdateTime));
            Assert.AreEqual(task.CreatorAccountId, task2.CreatorAccountId);
        }
        [Test]
        [Microsoft.VisualStudio.TestTools.UnitTesting.TestMethod]
        public void CreateTeamTask()
        {
            var account = this.CreateAccount();
            var team = CreateSampleTeam();
            var member = AddSampleMemberToTeam(account, team);
            var task = new TeamTask(member, team);

            Assert.AreEqual(member.ID, task.CreatorMemberId);
            Assert.AreEqual(team.ID, task.TeamId);

            this._taskService.Create(task);
            Assert.Greater(task.ID, 0);

            this.Evict(task);

            var task2 = this._taskService.GetTask(task.ID) as TeamTask;

            Assert.AreEqual(task.CreatorMemberId, task2.CreatorMemberId);
            Assert.AreEqual(task.TeamId, task2.TeamId);
        }
        [Test]
        [Microsoft.VisualStudio.TestTools.UnitTesting.TestMethod]
        public void UpdatePersonalTask()
        {
            var account = this.CreateAccount();
            var task = new PersonalTask(account);
            task.SetSubject(RandomString());
            task.SetBody(RandomString());
            task.SetPriority(Priority.Later);
            task.MarkAsCompleted();
            task.SetDueTime(DateTime.Now.AddDays(3));

            this._taskService.Create(task);

            this.Evict(task);

            var task2 = this._taskService.GetTask(task.ID) as PersonalTask;
            task2.SetSubject(task2.Subject + "updated");
            task2.SetBody(task2.Body + "updated");
            task2.SetPriority(Priority.Upcoming);
            task2.MarkAsInCompleted();
            task2.SetDueTime(DateTime.Now.AddDays(4));

            this._taskService.Update(task2);
            this.Evict(task2);
            var task3 = this._taskService.GetTask(task2.ID) as PersonalTask;

            Assert.AreEqual(task2.Subject, task3.Subject);
            Assert.AreEqual(task2.Body, task3.Body);
            Assert.AreEqual(task2.Priority, task3.Priority);
            Assert.AreEqual(FormatTime(task2.DueTime.Value), FormatTime(task3.DueTime.Value));
            Assert.AreEqual(task2.IsCompleted, task3.IsCompleted);
            Assert.AreEqual(task2.CreatorAccountId, task3.CreatorAccountId);
        }
        [Test]
        [Microsoft.VisualStudio.TestTools.UnitTesting.TestMethod]
        public void UpdateTeamTask()
        {
            var a = this.CreateAccount();
            var team = CreateSampleTeam();
            var creatorMember = AddSampleMemberToTeam(a, team);
            var member = AddSampleMemberToTeam(team);
            var project = AddSampleProjectToTeam(team);
            var task = new TeamTask(creatorMember, team);
            task.AssignTo(member);
            task.AddToProject(project);
            this._taskService.Create(task);

            this.Evict(task);

            var task2 = this._taskService.GetTask(task.ID) as TeamTask;
            var member2 = AddSampleMemberToTeam(team);

            task2.AssignTo(member2);
            task2.RemoveFromProject(project);

            Assert.AreEqual(member2.ID, task2.AssigneeId.Value);
            Assert.AreEqual(0, task2.Projects.Count());

            this._taskService.Update(task2);
            this.Evict(task2);
            var task3 = this._taskService.GetTask(task2.ID) as TeamTask;

            Assert.AreEqual(task2.Subject, task3.Subject);
            Assert.AreEqual(task2.Body, task3.Body);
            Assert.AreEqual(task2.Priority, task3.Priority);
            Assert.AreEqual(task2.DueTime, task3.DueTime);
            Assert.AreEqual(task2.IsCompleted, task3.IsCompleted);
            Assert.AreEqual(task2.CreatorMemberId, task3.CreatorMemberId);

            Assert.AreEqual(task2.TeamId, task3.TeamId);
            Assert.AreEqual(task2.AssigneeId.Value, task3.AssigneeId.Value);
            Assert.AreEqual(task2.Projects.Count(), task3.Projects.Count());

            this.Evict(task3);

            var task4 = this._taskService.GetTask(task3.ID) as TeamTask;

            task4.RemoveAssignee();

            Assert.AreEqual(null, task4.AssigneeId);

            this._taskService.Update(task4);
            this.Evict(task4);
            var task5 = this._taskService.GetTask(task4.ID) as TeamTask;

            Assert.AreEqual(task4.AssigneeId, task5.AssigneeId);
        }
        [Test]
        [Microsoft.VisualStudio.TestTools.UnitTesting.TestMethod]
        public void DeletePersonalTask()
        {
            var account = this.CreateAccount();
            var task = new PersonalTask(account);
            this._taskService.Create(task);

            this.Evict(task);

            var task2 = this._taskService.GetTask(task.ID) as PersonalTask;

            _taskService.Delete(task2);

            this.Evict(task2);

            var task3 = this._taskService.GetTask(task2.ID) as PersonalTask;

            Assert.IsNull(task3);
        }
        [Test]
        [Microsoft.VisualStudio.TestTools.UnitTesting.TestMethod]
        public void DeleteTeamTask()
        {
            var a = this.CreateAccount();
            var team = CreateSampleTeam();
            var member = AddSampleMemberToTeam(team);
            var project = AddSampleProjectToTeam(team);
            var task = new TeamTask(member, team);
            task.AssignTo(member);
            task.AddToProject(project);
            this._taskService.Create(task);

            this.Evict(task);

            var task2 = this._taskService.GetTask(task.ID) as TeamTask;

            _taskService.Delete(task2);

            this.Evict(task2);

            var task3 = this._taskService.GetTask(task2.ID) as TeamTask;

            Assert.IsNull(task3);
        }
    }
}