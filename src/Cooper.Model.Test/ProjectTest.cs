//Copyright (c) CodeSharp.  All rights reserved. - http://www.icodesharp.com/

using System;
using System.Linq;
using Cooper.Model.Teams;
using NUnit.Framework;

namespace Cooper.Model.Test
{
    [TestFixture]
    [Microsoft.VisualStudio.TestTools.UnitTesting.TestClass]
    public class ProjectTest : TestBase
    {
        [Test]
        [Microsoft.VisualStudio.TestTools.UnitTesting.TestMethod]
        public void TotalTests()
        {
            CreateProject();
            UpdateProject();
            DeleteProject();
            GetProjects();
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

            this._projectService.Create(project);
            Assert.Greater(project.ID, 0);

            this.Evict(project);

            var project2 = this._projectService.GetProject(project.ID) as Project;

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
            this._projectService.Create(project);

            this.Evict(project);

            var project2 = this._projectService.GetProject(project.ID) as Project;

            project2.SetName(project.Name + "_updated");
            project2.SetIsPublic(!project.IsPublic);

            this._projectService.Update(project2);
            this.Evict(project2);
            var project3 = this._projectService.GetProject(project2.ID) as Project;

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
            this._projectService.Create(project);

            this.Evict(project);

            var project2 = this._projectService.GetProject(project.ID) as Project;

            this._projectService.Delete(project2);

            this.Evict(project2);

            var project3 = this._projectService.GetProject(project2.ID) as Project;

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

            this._projectService.Create(project1);
            this._projectService.Create(project2);
            this._projectService.Create(project3);

            this.Evict(project1);
            this.Evict(project2);
            this.Evict(project3);

            var projects = this._projectService.GetProjectsByTeam(team);

            Assert.AreEqual(3, projects.Count());
            Assert.IsTrue(projects.Any(x => x.ID == project1.ID));
            Assert.IsTrue(projects.Any(x => x.ID == project2.ID));
            Assert.IsTrue(projects.Any(x => x.ID == project3.ID));
        }

        [Test]
        [Microsoft.VisualStudio.TestTools.UnitTesting.TestMethod]
        public void Extension()
        {
            var team = new Team("Test Team");
            this._teamService.Create(team);

            var project = new Project("Project 1", true, team);
            project["key"] = "abc";
            this._projectService.Create(project);

            this.Evict(project);

            project = this._projectService.GetProject(project.ID);
            Assert.AreEqual("abc", project["key"]);
        }
    }
}