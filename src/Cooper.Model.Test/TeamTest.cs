//Copyright (c) CodeSharp.  All rights reserved. - http://www.icodesharp.com/

using System;
using System.Linq;
using Cooper.Model.Teams;
using NUnit.Framework;

namespace Cooper.Model.Test
{
    [TestFixture]
    [Microsoft.VisualStudio.TestTools.UnitTesting.TestClass]
    public class TeamTest : TestBase
    {
        [Test]
        [Microsoft.VisualStudio.TestTools.UnitTesting.TestMethod]
        public void CreateTeam()
        {
            var team = new Team("Team 1");

            Assert.AreNotEqual(DateTime.MinValue, team.CreateTime);

            this._teamService.Create(team);
            Assert.Greater(team.ID, 0);

            this.Evict(team);

            var team2 = this._teamService.GetTeam(team.ID);

            Assert.AreEqual(team.Name, team2.Name);
            Assert.AreEqual(FormatTime(team.CreateTime), FormatTime(team2.CreateTime));
        }
        [Test]
        [Microsoft.VisualStudio.TestTools.UnitTesting.TestMethod]
        public void UpdateTeam()
        {
            var team = CreateSampleTeam();

            this.Evict(team);

            var team2 = this._teamService.GetTeam(team.ID);

            Assert.AreEqual(team.Name, team2.Name);
            Assert.AreEqual(FormatTime(team.CreateTime), FormatTime(team2.CreateTime));

            team2.SetName(team.Name + "_updated");

            this._teamService.Update(team2);
            this.Evict(team2);
            var team3 = this._teamService.GetTeam(team2.ID);

            Assert.AreEqual(team2.Name, team3.Name);
        }
        [Test]
        [Microsoft.VisualStudio.TestTools.UnitTesting.TestMethod]
        public void DeleteTeam()
        {
            var team = CreateSampleTeam();

            this.Evict(team);

            var team2 = this._teamService.GetTeam(team.ID);

            this._teamService.Delete(team2);

            this.Evict(team2);

            var team3 = this._teamService.GetTeam(team2.ID);

            Assert.IsNull(team3);
        }

        [Test]
        [Microsoft.VisualStudio.TestTools.UnitTesting.TestMethod]
        public void AddTeamMember()
        {
            var team = CreateSampleTeam();
            var member = this._teamService.AddMember(RandomString(), RandomString(), team);

            Assert.Greater(member.ID, 0);
            Assert.AreEqual(team.ID, member.TeamId);

            this.Evict(member);
            this.Evict(team);

            team = _teamService.GetTeam(team.ID);
            var memberCreated = team.GetMember(member.ID);

            Assert.AreEqual(member.Name, memberCreated.Name);
            Assert.AreEqual(member.Email, memberCreated.Email);
            Assert.AreEqual(member.TeamId, memberCreated.TeamId);
            Assert.AreEqual(FormatTime(member.CreateTime), FormatTime(memberCreated.CreateTime));
        }
        [Test]
        [Microsoft.VisualStudio.TestTools.UnitTesting.TestMethod]
        public void UpdateTeamMember()
        {
            var team = CreateSampleTeam();
            var member = AddSampleMemberToTeam(team);

            this.Evict(member);
            this.Evict(team);

            team = _teamService.GetTeam(team.ID);
            member = team.GetMember(member.ID);

            member.SetName(member.Name + "_updated");

            this._teamService.Update(team);

            this.Evict(member);
            this.Evict(team);

            team = _teamService.GetTeam(team.ID);
            var memberUpdated = team.GetMember(member.ID);

            Assert.AreEqual(member.Name, memberUpdated.Name);
            Assert.AreEqual(member.Email, memberUpdated.Email);
            Assert.AreEqual(member.TeamId, memberUpdated.TeamId);
            Assert.AreEqual(FormatTime(member.CreateTime), FormatTime(memberUpdated.CreateTime));
        }
        [Test]
        [Microsoft.VisualStudio.TestTools.UnitTesting.TestMethod]
        public void RemoveTeamMember()
        {
            var team = CreateSampleTeam();
            var member = AddSampleMemberToTeam(team);

            this.Evict(member);
            this.Evict(team);

            team = _teamService.GetTeam(team.ID);
            member = team.GetMember(member.ID);

            this._teamService.RemoveMember(member, team);

            this.Evict(member);
            this.Evict(team);

            team = _teamService.GetTeam(team.ID);
            member = team.GetMember(member.ID);

            Assert.IsNull(member);
        }
        [Test]
        [Microsoft.VisualStudio.TestTools.UnitTesting.TestMethod]
        public void ClearTaskAssigneeAfterTeamMemberRemovedTest()
        {
            var account = CreateAccount();
            var team = CreateSampleTeam();
            var member = AddSampleMemberToTeam(team);
            var task = new Task(account, team);
            task.AssignTo(member);
            this._teamTaskService.Create(task);
            this.Evict(member);
            this.Evict(team);
            this.Evict(task);
            team = _teamService.GetTeam(team.ID);
            member = team.GetMember(member.ID);
            task = this._teamTaskService.GetTask(task.ID);
            Assert.IsNotNull(task);
            Assert.IsNotNull(task.AssigneeId);
            Assert.AreEqual(member.ID, task.AssigneeId.Value);

            this._teamService.RemoveMember(member, team);
            this.Evict(member);
            this.Evict(team);
            this.Evict(task);
            team = _teamService.GetTeam(team.ID);
            member = team.GetMember(member.ID);
            task = this._teamTaskService.GetTask(task.ID);
            Assert.IsNull(member);
            Assert.IsNotNull(task);
            Assert.IsNull(task.AssigneeId);
        }
        [Test]
        [Microsoft.VisualStudio.TestTools.UnitTesting.TestMethod]
        public void GetTeamMembers()
        {
            var team = CreateSampleTeam();

            var member1 = AddSampleMemberToTeam(team);
            var member2 = AddSampleMemberToTeam(team);
            var member3 = AddSampleMemberToTeam(team);

            var members = _teamService.GetTeam(team.ID).Members;

            Assert.AreEqual(3, members.Count());

            Assert.IsTrue(members.Any(x => x.ID == member1.ID));
            Assert.IsTrue(members.Any(x => x.ID == member2.ID));
            Assert.IsTrue(members.Any(x => x.ID == member3.ID));
        }

        [Test]
        [Microsoft.VisualStudio.TestTools.UnitTesting.TestMethod]
        public void AddProject()
        {
            var team = CreateSampleTeam();
            var projectName = RandomString();
            this._teamService.AddProject(projectName, team);

            this.Evict(team);

            team = _teamService.GetTeam(team.ID);
            Assert.AreEqual(1, team.Projects.Count());
            var project = team.Projects.First();

            Assert.Greater(project.ID, 0);
            Assert.AreEqual(team.ID, project.TeamId);
            Assert.AreEqual(projectName, project.Name);
            Assert.AreEqual(false, project.IsPublic);
        }
        [Test]
        [Microsoft.VisualStudio.TestTools.UnitTesting.TestMethod]
        public void UpdateProject()
        {
            var team = CreateSampleTeam();
            var project = AddSampleProjectToTeam(team);

            this.Evict(project);
            this.Evict(team);

            team = _teamService.GetTeam(team.ID);
            project = team.GetProject(project.ID);

            Assert.IsNotNull(project);

            project.SetName(project.Name + "_updated");
            project.SetIsPublic(!project.IsPublic);

            this._teamService.Update(team);

            this.Evict(project);
            this.Evict(team);

            team = _teamService.GetTeam(team.ID);
            var projectUpdated = team.GetProject(project.ID);

            Assert.AreEqual(project.Name, projectUpdated.Name);
            Assert.AreEqual(project.IsPublic, projectUpdated.IsPublic);
        }
        [Test]
        [Microsoft.VisualStudio.TestTools.UnitTesting.TestMethod]
        public void RemoveProject()
        {
            var team = CreateSampleTeam();
            var project = AddSampleProjectToTeam(team);

            this.Evict(project);
            this.Evict(team);

            team = _teamService.GetTeam(team.ID);
            project = team.GetProject(project.ID);

            Assert.IsNotNull(project);

            this._teamService.RemoveProject(project, team);

            this.Evict(project);
            this.Evict(team);

            team = _teamService.GetTeam(team.ID);
            project = team.GetProject(project.ID);

            Assert.IsNull(project);
        }
        [Test]
        [Microsoft.VisualStudio.TestTools.UnitTesting.TestMethod]
        public void ClearTaskProjectsAfterTeamMemberRemovedTest()
        {
            var account = CreateAccount();
            var team = CreateSampleTeam();
            var member = AddSampleMemberToTeam(team);
            var project = AddSampleProjectToTeam(team);
            var task = new Task(account, team);
            task.AddToProject(project);
            this._teamTaskService.Create(task);
            this.Evict(member);
            this.Evict(team);
            this.Evict(task);
            this.Evict(project);
            team = _teamService.GetTeam(team.ID);
            member = team.GetMember(member.ID);
            task = this._teamTaskService.GetTask(task.ID);
            Assert.IsNotNull(task);
            Assert.AreEqual(1, task.Projects.Count());

            this._teamService.RemoveProject(project, team);
            this.Evict(project);
            this.Evict(team);
            this.Evict(task);
            team = this._teamService.GetTeam(team.ID);
            project = team.GetProject(project.ID);
            task = this._teamTaskService.GetTask(task.ID);
            Assert.IsNull(project);
            Assert.IsNotNull(task);
            Assert.AreEqual(0, task.Projects.Count());
        }
        [Test]
        [Microsoft.VisualStudio.TestTools.UnitTesting.TestMethod]
        public void ClearCommentCreatorsAfterTeamMemberRemovedTest()
        {
            var account = CreateAccount();
            var team = CreateSampleTeam();
            var member = AddSampleMemberToTeam(team);
            var task = new Task(account, team);
            var comment = RandomString();
            task.AddComment(member, comment);
            this._teamTaskService.Create(task);

            this.Evict(member);
            this.Evict(team);
            this.Evict(task);
            this.Evict(comment);

            team = _teamService.GetTeam(team.ID);
            member = team.GetMember(member.ID);
            task = this._teamTaskService.GetTask(task.ID);
            Assert.IsNotNull(task);
            Assert.AreEqual(1, task.Comments.Count());
            Assert.IsTrue(task.Comments.Any(x => x.Body == comment && x.Creator.ID == member.ID));

            this._teamService.RemoveMember(member, team);

            this.Evict(task);
            this.Evict(comment);

            task = this._teamTaskService.GetTask(task.ID);
            Assert.IsNotNull(task);
            Assert.AreEqual(1, task.Comments.Count());
            Assert.IsTrue(task.Comments.Any(x => x.Body == comment && x.Creator == null));
        }
        [Test]
        [Microsoft.VisualStudio.TestTools.UnitTesting.TestMethod]
        public void GetProjects()
        {
            var team = CreateSampleTeam();

            var project1 = AddSampleProjectToTeam(team);
            var project2 = AddSampleProjectToTeam(team);
            var project3 = AddSampleProjectToTeam(team);

            var projects = this._teamService.GetTeam(team.ID).Projects;

            Assert.AreEqual(3, projects.Count());
            Assert.IsTrue(projects.Any(x => x.ID == project1.ID));
            Assert.IsTrue(projects.Any(x => x.ID == project2.ID));
            Assert.IsTrue(projects.Any(x => x.ID == project3.ID));
        }
        [Test]
        [Microsoft.VisualStudio.TestTools.UnitTesting.TestMethod]
        public void ExtensionsTest()
        {
            var team = CreateSampleTeam();

            var project = AddSampleProjectToTeam(team);
            project["key"] = "abc";
            this._teamService.Update(team);

            this.Evict(project);
            this.Evict(team);

            team = _teamService.GetTeam(team.ID);
            project = team.GetProject(project.ID);
            Assert.AreEqual("abc", project["key"]);
        }
        [Test]
        [Microsoft.VisualStudio.TestTools.UnitTesting.TestMethod]
        public void GetTeamsByAccount()
        {
            var account = CreateAccount();

            var team1 = CreateSampleTeam();
            var team2 = CreateSampleTeam();
            var team3 = CreateSampleTeam();

            var member1 = AddSampleMemberToTeam(team1);
            var member2 = AddSampleMemberToTeam(team2);
            var member3 = AddSampleMemberToTeam(team3);

            member1 = team1.GetMember(member1.ID);
            member3 = team3.GetMember(member3.ID);

            this._teamService.AssociateMemberAccount(member1, account);
            this._teamService.AssociateMemberAccount(member3, account);

            this._teamService.Update(team1);
            this._teamService.Update(team3);

            var teams = this._teamService.GetTeamsByAccount(account);

            Assert.AreEqual(2, teams.Count());
            Assert.IsTrue(teams.Any(x => x.ID == team1.ID));
            Assert.IsTrue(teams.Any(x => x.ID == team3.ID));
        }

        [Test]
        [Microsoft.VisualStudio.TestTools.UnitTesting.TestMethod]
        public void AddTeamMemberWithDuplicateEmailTest()
        {
            var team = CreateSampleTeam();
            var name = RandomString();
            var email = RandomString();
            this.AssertParallel(() => this._teamService.AddMember(name, email, team), 100, 1);
            Assert.Catch(typeof(AssertionException), () => this._teamService.AddMember(name, email, team));
        }
        [Test]
        [Microsoft.VisualStudio.TestTools.UnitTesting.TestMethod]
        public void AddTeamMemberWithDuplicateAccountTest()
        {
            var account = CreateAccount();
            var team = CreateSampleTeam();
            this.AssertParallel(() => this._teamService.AddMember(Guid.NewGuid().ToString(), Guid.NewGuid().ToString(), team, account), 100, 1);
            Assert.Catch(typeof(AssertionException), () => this._teamService.AddMember(Guid.NewGuid().ToString(), Guid.NewGuid().ToString(), team, account));
        }
        [Test]
        [Microsoft.VisualStudio.TestTools.UnitTesting.TestMethod]
        public void AssociateTeamMemberWithDuplicateAccountTest()
        {
            var team = CreateSampleTeam();

            var account = CreateAccount();
            var member = this._teamService.AddMember(RandomString(), RandomString(), team);
            this.AssertParallel(() => this._teamService.AssociateMemberAccount(member, account), 100, 1);
            Assert.Catch(typeof(AssertionException), () => this._teamService.AssociateMemberAccount(member, account));

            account = CreateAccount();
            var member2 = this._teamService.AddMember(RandomString(), RandomString(), team, account);
            this.AssertParallel(() => this._teamService.AssociateMemberAccount(member, account), 100, 0);
            Assert.Catch(typeof(AssertionException), () => this._teamService.AssociateMemberAccount(member, account));
        }
    }
}