//Copyright (c) CodeSharp.  All rights reserved. - http://www.icodesharp.com/

using System;
using System.Linq;
using Cooper.Model.Teams;
using NUnit.Framework;

namespace Cooper.Model.Test
{
    [TestFixture]
    [Microsoft.VisualStudio.TestTools.UnitTesting.TestClass]
    public class TeamTaskTest : TestBase
    {
        [Test]
        [Microsoft.VisualStudio.TestTools.UnitTesting.TestMethod]
        public void CreateTeamTask()
        {
            var a = this.CreateAccount();
            var team = new Team("Team 1");
            _teamService.Create(team);
            var member = new Member("xuehua", "xuehua@163.com", team);
            _teamService.AddMember(member);
            var project = new Project("Project 1", true, team);
            _teamService.AddProject(project);
            var task = new Task(a, team);
            task.AssignTo(member);
            task.AddToProject(project);

            Assert.AreNotEqual(DateTime.MinValue, task.CreateTime);
            Assert.AreNotEqual(DateTime.MinValue, task.LastUpdateTime);

            Assert.AreEqual(a.ID, task.CreatorAccountId);
            Assert.AreEqual(project.ID, task.Projects.First().ID);
            Assert.AreEqual(team.ID, task.TeamId);
            Assert.AreEqual(1, task.Projects.Count());
            Assert.AreEqual(project.ID, task.Projects.First().ID);

            this._teamTaskService.Create(task);
            Assert.Greater(task.ID, 0);

            this.Evict(task);

            var task2 = this._teamTaskService.GetTask(task.ID);

            Assert.AreEqual(task.Subject, task2.Subject);
            Assert.AreEqual(task.Body, task2.Body);
            Assert.AreEqual(task.Priority, task2.Priority);
            Assert.AreEqual(task.DueTime, task2.DueTime);
            Assert.AreEqual(task.IsCompleted, task2.IsCompleted);
            Assert.AreEqual(task.CreatorAccountId, task2.CreatorAccountId);
            Assert.AreEqual(task.TaskFolderId, task2.TaskFolderId);

            Assert.AreEqual(task.TeamId, task2.TeamId);
            Assert.AreEqual(task.AssigneeId.Value, task2.AssigneeId.Value);
            Assert.AreEqual(task.Projects.Count(), task2.Projects.Count());
            Assert.AreEqual(task.Projects.First().ID, task2.Projects.First().ID);
        }
        [Test]
        [Microsoft.VisualStudio.TestTools.UnitTesting.TestMethod]
        public void UpdateTeamTask()
        {
            var a = this.CreateAccount();
            var team = new Team("Team 1");
            _teamService.Create(team);
            var member = new Member("xuehua", "xuehua@163.com", team);
            _teamService.AddMember(member);
            var project = new Project("Project 1", true, team);
            _teamService.AddProject(project);
            var task = new Task(a, team);
            task.AssignTo(member);
            task.AddToProject(project);
            this._teamTaskService.Create(task);

            this.Evict(task);

            var task2 = this._teamTaskService.GetTask(task.ID);
            var member2 = new Member("ylw", "ylw@163.com", team);
            _teamService.AddMember(member2);

            task2.AssignTo(member2);
            task2.RemoveFromProject(project);

            Assert.AreEqual(member2.ID, task2.AssigneeId.Value);
            Assert.AreEqual(0, task2.Projects.Count());

            this._teamTaskService.Update(task2);
            this.Evict(task2);
            var task3 = this._teamTaskService.GetTask(task2.ID);

            Assert.AreEqual(task2.Subject, task3.Subject);
            Assert.AreEqual(task2.Body, task3.Body);
            Assert.AreEqual(task2.Priority, task3.Priority);
            Assert.AreEqual(task2.DueTime, task3.DueTime);
            Assert.AreEqual(task2.IsCompleted, task3.IsCompleted);
            Assert.AreEqual(task2.CreatorAccountId, task3.CreatorAccountId);
            Assert.AreEqual(task2.TaskFolderId, task3.TaskFolderId);

            Assert.AreEqual(task2.TeamId, task3.TeamId);
            Assert.AreEqual(task2.AssigneeId.Value, task3.AssigneeId.Value);
            Assert.AreEqual(task2.Projects.Count(), task3.Projects.Count());

            this.Evict(task3);

            var task4 = this._teamTaskService.GetTask(task3.ID);

            task4.RemoveAssignee();

            Assert.AreEqual(null, task4.AssigneeId);

            this._teamTaskService.Update(task4);
            this.Evict(task4);
            var task5 = this._teamTaskService.GetTask(task4.ID);

            Assert.AreEqual(task4.AssigneeId, task5.AssigneeId);
        }
        [Test]
        [Microsoft.VisualStudio.TestTools.UnitTesting.TestMethod]
        public void DeleteTeamTask()
        {
            var a = this.CreateAccount();
            var team = new Team("Team 1");
            _teamService.Create(team);
            var member = new Member("xuehua", "xuehua@163.com", team);
            _teamService.AddMember(member);
            var project = new Project("Project 1", true, team);
            _teamService.AddProject(project);
            var task = new Task(a, team);
            task.AssignTo(member);
            task.AddToProject(project);
            this._teamTaskService.Create(task);

            this.Evict(task);

            var task2 = this._teamTaskService.GetTask(task.ID);

            _teamTaskService.Delete(task2);

            this.Evict(task2);

            var task3 = this._teamTaskService.GetTask(task2.ID);

            Assert.IsNull(task3);
        }
        [Test]
        [Microsoft.VisualStudio.TestTools.UnitTesting.TestMethod]
        public void GetTeamTasks()
        {
            var a = this.CreateAccount();
            var team = new Team("Team 1");
            _teamService.Create(team);
            var member = new Member("xuehua", "xuehua@163.com", team);
            _teamService.AddMember(member);
            var project = new Project("Project 1", true, team);
            _teamService.AddProject(project);

            var teamTask1 = new Task(a, team);
            teamTask1.AssignTo(member);
            teamTask1.AddToProject(project);

            var teamTask2 = new Task(a, team);
            teamTask2.AssignTo(member);
            teamTask2.AddToProject(project);

            var teamTask3 = new Task(a, team);
            teamTask3.AssignTo(member);
            teamTask3.AddToProject(project);

            this._teamTaskService.Create(teamTask1);
            this._teamTaskService.Create(teamTask2);
            this._teamTaskService.Create(teamTask3);

            this.Evict(teamTask1);
            this.Evict(teamTask2);
            this.Evict(teamTask3);

            var teamTasks = this._teamTaskService.GetTasksByTeam(team);
            var projectTasks = this._teamTaskService.GetTasksByProject(project);
            var memberTasks = this._teamTaskService.GetTasksByTeamMember(member);

            Assert.AreEqual(3, teamTasks.Count());
            Assert.IsTrue(teamTasks.Any(x => x.ID == teamTask1.ID));
            Assert.IsTrue(teamTasks.Any(x => x.ID == teamTask2.ID));
            Assert.IsTrue(teamTasks.Any(x => x.ID == teamTask3.ID));

            Assert.AreEqual(3, projectTasks.Count());
            Assert.IsTrue(projectTasks.Any(x => x.ID == teamTask1.ID));
            Assert.IsTrue(projectTasks.Any(x => x.ID == teamTask2.ID));
            Assert.IsTrue(projectTasks.Any(x => x.ID == teamTask3.ID));

            Assert.AreEqual(3, memberTasks.Count());
            Assert.IsTrue(memberTasks.Any(x => x.ID == teamTask1.ID));
            Assert.IsTrue(memberTasks.Any(x => x.ID == teamTask2.ID));
            Assert.IsTrue(memberTasks.Any(x => x.ID == teamTask3.ID));
        }
    }
}