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
            var team = CreateSampleTeam();
            var member = AddSampleMemberToTeam(team);
            var project = AddSampleProjectToTeam(team);
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
            var team = CreateSampleTeam();
            var member = AddSampleMemberToTeam(team);
            var project = AddSampleProjectToTeam(team);
            var task = new Task(a, team);
            task.AssignTo(member);
            task.AddToProject(project);
            this._teamTaskService.Create(task);

            this.Evict(task);

            var task2 = this._teamTaskService.GetTask(task.ID);
            var member2 = AddSampleMemberToTeam(team);

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
            var team = CreateSampleTeam();
            var member = AddSampleMemberToTeam(team);
            var project = AddSampleProjectToTeam(team);
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
        public void GetTasksTest()
        {
            var account1 = this.CreateAccount();
            var account2 = this.CreateAccount();
            var team = CreateSampleTeam();
            var member1 = AddSampleMemberToTeam(account1, team);
            var member2 = AddSampleMemberToTeam(account2, team);
            var project1 = AddSampleProjectToTeam(team);
            var project2 = AddSampleProjectToTeam(team);

            var teamTask1 = new Task(account1, team);
            teamTask1.AssignTo(member2);
            teamTask1.MarkAsCompleted();
            teamTask1.AddToProject(project1);

            var teamTask2 = new Task(account2, team);
            teamTask2.AssignTo(member1);
            teamTask2.MarkAsCompleted();
            teamTask2.AddToProject(project2);

            var teamTask3 = new Task(account1, team);
            teamTask3.AssignTo(member1);
            teamTask3.AddToProject(project1);

            this._teamTaskService.Create(teamTask1);
            this._teamTaskService.Create(teamTask2);
            this._teamTaskService.Create(teamTask3);

            var teamTasks = this._teamTaskService.GetTasksByTeam(team);
            Assert.AreEqual(3, teamTasks.Count());
            Assert.IsTrue(teamTasks.Any(x => x.ID == teamTask1.ID));
            Assert.IsTrue(teamTasks.Any(x => x.ID == teamTask2.ID));
            Assert.IsTrue(teamTasks.Any(x => x.ID == teamTask3.ID));

            var member1Tasks = this._teamTaskService.GetTasksByTeamMember(member1);
            Assert.AreEqual(2, member1Tasks.Count());
            Assert.IsTrue(member1Tasks.Any(x => x.ID == teamTask2.ID));
            Assert.IsTrue(member1Tasks.Any(x => x.ID == teamTask3.ID));

            var member2Tasks = this._teamTaskService.GetTasksByTeamMember(member2);
            Assert.AreEqual(1, member2Tasks.Count());
            Assert.IsTrue(member2Tasks.Any(x => x.ID == teamTask1.ID));

            var teamTasks1OfCreatorOrAssignee = this._teamTaskService.GetTasksByTeam(team, account1);
            Assert.AreEqual(3, teamTasks1OfCreatorOrAssignee.Count());
            Assert.IsTrue(teamTasks1OfCreatorOrAssignee.Any(x => x.ID == teamTask1.ID));
            Assert.IsTrue(teamTasks1OfCreatorOrAssignee.Any(x => x.ID == teamTask2.ID));
            Assert.IsTrue(teamTasks1OfCreatorOrAssignee.Any(x => x.ID == teamTask3.ID));

            var teamTasks2OfCreatorOrAssignee = this._teamTaskService.GetTasksByTeam(team, account2);
            Assert.AreEqual(2, teamTasks2OfCreatorOrAssignee.Count());
            Assert.IsTrue(teamTasks2OfCreatorOrAssignee.Any(x => x.ID == teamTask1.ID));
            Assert.IsTrue(teamTasks2OfCreatorOrAssignee.Any(x => x.ID == teamTask2.ID));

            var incompletedTeamTasks1OfCreatorOrAssignee = this._teamTaskService.GetIncompletedTasksByTeam(team, account1);
            Assert.AreEqual(1, incompletedTeamTasks1OfCreatorOrAssignee.Count());
            Assert.IsTrue(incompletedTeamTasks1OfCreatorOrAssignee.Any(x => x.ID == teamTask3.ID));

            var incompletedTeamTasks2OfCreatorOrAssignee = this._teamTaskService.GetIncompletedTasksByTeam(team, account2);
            Assert.AreEqual(0, incompletedTeamTasks2OfCreatorOrAssignee.Count());

            var project1Tasks = this._teamTaskService.GetTasksByProject(project1);
            Assert.AreEqual(2, project1Tasks.Count());
            Assert.IsTrue(project1Tasks.Any(x => x.ID == teamTask1.ID));
            Assert.IsTrue(project1Tasks.Any(x => x.ID == teamTask3.ID));

            var project2Tasks = this._teamTaskService.GetTasksByProject(project2);
            Assert.AreEqual(1, project2Tasks.Count());
            Assert.IsTrue(project2Tasks.Any(x => x.ID == teamTask2.ID));

            var account1Project1Tasks = this._teamTaskService.GetTasksByProject(project1, account1);
            Assert.AreEqual(2, account1Project1Tasks.Count());
            Assert.IsTrue(account1Project1Tasks.Any(x => x.ID == teamTask1.ID));
            Assert.IsTrue(account1Project1Tasks.Any(x => x.ID == teamTask3.ID));

            var account1Project2Tasks = this._teamTaskService.GetTasksByProject(project2, account1);
            Assert.AreEqual(1, account1Project2Tasks.Count());
            Assert.IsTrue(account1Project2Tasks.Any(x => x.ID == teamTask2.ID));

            var account2Project1Tasks = this._teamTaskService.GetTasksByProject(project1, account2);
            Assert.AreEqual(1, account2Project1Tasks.Count());
            Assert.IsTrue(account2Project1Tasks.Any(x => x.ID == teamTask1.ID));

            var account2roject2Tasks = this._teamTaskService.GetTasksByProject(project2, account2);
            Assert.AreEqual(1, account2roject2Tasks.Count());
            Assert.IsTrue(account2roject2Tasks.Any(x => x.ID == teamTask2.ID));

            var account1Project1IncompletedTasks = this._teamTaskService.GetIncompletedTasksByProject(project1, account1);
            Assert.AreEqual(1, account1Project1IncompletedTasks.Count());
            Assert.IsTrue(account1Project1IncompletedTasks.Any(x => x.ID == teamTask3.ID));

            var account1Project2IncompletedTasks = this._teamTaskService.GetIncompletedTasksByProject(project2, account1);
            Assert.AreEqual(0, account1Project2IncompletedTasks.Count());

            var account2Project1IncompletedTasks = this._teamTaskService.GetIncompletedTasksByProject(project1, account2);
            Assert.AreEqual(0, account2Project1IncompletedTasks.Count());

            var account2roject2IncompletedTasks = this._teamTaskService.GetIncompletedTasksByProject(project2, account2);
            Assert.AreEqual(0, account2roject2IncompletedTasks.Count());
        }
    }
}