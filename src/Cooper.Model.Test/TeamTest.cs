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
            var team = new Team("Team 1");

            Assert.AreNotEqual(DateTime.MinValue, team.CreateTime);

            this._teamService.Create(team);
            Assert.Greater(team.ID, 0);

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
            var team = new Team("Team 1");

            Assert.AreNotEqual(DateTime.MinValue, team.CreateTime);

            this._teamService.Create(team);
            Assert.Greater(team.ID, 0);

            this.Evict(team);

            var team2 = this._teamService.GetTeam(team.ID);

            this._teamService.Delete(team2);

            this.Evict(team2);

            var team3 = this._teamService.GetTeam(team2.ID);

            Assert.IsNull(team3);
        }

        [Test]
        [Microsoft.VisualStudio.TestTools.UnitTesting.TestMethod]
        public void CreateTeamMember()
        {
            var team = new Team("Test Team");
            this._teamService.Create(team);

            var member = new Member("TeamMember 1", "xuehua@163.com", team);

            Assert.AreNotEqual(DateTime.MinValue, team.CreateTime);
            Assert.AreEqual(team.ID, member.TeamId);

            this._teamService.AddMember(member);
            Assert.Greater(member.ID, 0);

            this.Evict(member);

            var member2 = this._teamService.GetMember(member.ID);

            Assert.AreEqual(member.Name, member2.Name);
            Assert.AreEqual(member.Email, member2.Email);
            Assert.AreEqual(member.TeamId, member2.TeamId);
            Assert.AreEqual(FormatTime(member.CreateTime), FormatTime(member2.CreateTime));
        }
        [Test]
        [Microsoft.VisualStudio.TestTools.UnitTesting.TestMethod]
        public void UpdateTeamMember()
        {
            var team = new Team("Test Team");
            this._teamService.Create(team);

            var member = new Member("TeamMember 1", "xuehua@163.com", team);
            this._teamService.AddMember(member);

            this.Evict(member);

            var member2 = this._teamService.GetMember(member.ID);

            member2.SetName(member.Name + "_updated");
            member2.SetEmail(member.Email + "_updated");

            this._teamService.UpdateMember(member2);
            this.Evict(member2);
            var member3 = this._teamService.GetMember(member2.ID);

            Assert.AreEqual(member2.Name, member3.Name);
            Assert.AreEqual(member2.Email, member3.Email);
            Assert.AreEqual(member2.TeamId, member3.TeamId);
            Assert.AreEqual(FormatTime(member2.CreateTime), FormatTime(member3.CreateTime));
        }
        [Test]
        [Microsoft.VisualStudio.TestTools.UnitTesting.TestMethod]
        public void DeleteTeamMember()
        {
            var team = new Team("Test Team");
            this._teamService.Create(team);

            var member = new Member("TeamMember 1", "xuehua@163.com", team);
            this._teamService.AddMember(member);

            this.Evict(member);

            var member2 = this._teamService.GetMember(member.ID);

            this._teamService.RemoveMember(member2);

            this.Evict(member2);

            var member3 = this._teamService.GetMember(member2.ID);

            Assert.IsNull(member3);
        }
        [Test]
        [Microsoft.VisualStudio.TestTools.UnitTesting.TestMethod]
        public void GetTeamMembers()
        {
            var team = new Team("Test Team");
            this._teamService.Create(team);

            var member1 = new Member("TeamMember 1", "xuehua1@163.com", team);
            var member2 = new Member("TeamMember 2", "xuehua2@163.com", team);
            var member3 = new Member("TeamMember 3", "xuehua3@163.com", team);

            this._teamService.AddMember(member1);
            this._teamService.AddMember(member2);
            this._teamService.AddMember(member3);

            this.Evict(team);

            team = _teamService.GetTeam(team.ID);

            var members = team.Members;

            Assert.AreEqual(3, members.Count());
            Assert.IsTrue(members.Any(x => x.ID == member1.ID));
            Assert.IsTrue(members.Any(x => x.ID == member2.ID));
            Assert.IsTrue(members.Any(x => x.ID == member3.ID));
        }

        [Test]
        [Microsoft.VisualStudio.TestTools.UnitTesting.TestMethod]
        public void CreateProject()
        {
            var team = new Team("Test Team");
            this._teamService.Create(team);

            var project = new Project("Project 1", true, team);

            Assert.AreNotEqual(DateTime.MinValue, team.CreateTime);
            Assert.AreEqual(team.ID, project.TeamId);

            this._teamService.AddProject(project);
            Assert.Greater(project.ID, 0);

            this.Evict(project);

            var project2 = this._teamService.GetProject(project.ID);

            Assert.AreEqual(project.Name, project2.Name);
            Assert.AreEqual(project.IsPublic, project2.IsPublic);
            Assert.AreEqual(project.TeamId, project2.TeamId);
            Assert.AreEqual(FormatTime(project.CreateTime), FormatTime(project.CreateTime));
        }
        [Test]
        [Microsoft.VisualStudio.TestTools.UnitTesting.TestMethod]
        public void UpdateProject()
        {
            var team = new Team("Test Team");
            this._teamService.Create(team);

            var project = new Project("Project 1", true, team);
            this._teamService.AddProject(project);

            this.Evict(project);

            var project2 = this._teamService.GetProject(project.ID);

            project2.SetName(project.Name + "_updated");
            project2.SetIsPublic(!project.IsPublic);

            this._teamService.UpdateProject(project2);
            this.Evict(project2);
            var project3 = this._teamService.GetProject(project2.ID);

            Assert.AreEqual(project2.Name, project3.Name);
            Assert.AreEqual(project2.IsPublic, project3.IsPublic);
        }
        [Test]
        [Microsoft.VisualStudio.TestTools.UnitTesting.TestMethod]
        public void DeleteProject()
        {
            var team = new Team("Test Team");
            this._teamService.Create(team);

            var project = new Project("Project 1", true, team);
            this._teamService.AddProject(project);

            this.Evict(project);

            var project2 = this._teamService.GetProject(project.ID);

            this._teamService.RemoveProject(project2);

            this.Evict(project2);

            var project3 = this._teamService.GetProject(project2.ID);

            Assert.IsNull(project3);
        }
        [Test]
        [Microsoft.VisualStudio.TestTools.UnitTesting.TestMethod]
        public void GetProjects()
        {
            var team = new Team("Test Team");
            this._teamService.Create(team);

            var project1 = new Project("Project 1", true, team);
            var project2 = new Project("Project 2", true, team);
            var project3 = new Project("Project 3", true, team);

            this._teamService.AddProject(project1);
            this._teamService.AddProject(project2);
            this._teamService.AddProject(project3);

            this.Evict(team);

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
            var team = new Team("Test Team");
            this._teamService.Create(team);

            var project = new Project("Project 1", true, team);
            project["key"] = "abc";
            this._teamService.AddProject(project);

            this.Evict(project);

            project = this._teamService.GetProject(project.ID);
            Assert.AreEqual("abc", project["key"]);
        }
    }
}