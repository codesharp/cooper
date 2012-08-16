//Copyright (c) CodeSharp.  All rights reserved. - http://www.icodesharp.com/

using System;
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
        public void TotalTests()
        {
            CreateTeam();
            UpdateTeam();
            DeleteTeam();
        }

        [Test]
        [Microsoft.VisualStudio.TestTools.UnitTesting.TestMethod]
        public void CreateTeam()
        {
            var team = new Team("Team 1");

            Assert.AreNotEqual(DateTime.MinValue, team.CreateTime);

            this._teamService.Create(team);
            Assert.Greater(team.ID, 0);

            this.Evict(team);

            var team2 = this._teamService.GetTeam(team.ID) as Team;

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

            var team2 = this._teamService.GetTeam(team.ID) as Team;

            Assert.AreEqual(team.Name, team2.Name);
            Assert.AreEqual(FormatTime(team.CreateTime), FormatTime(team2.CreateTime));

            team2.SetName(team.Name + "_updated");

            this._teamService.Update(team2);
            this.Evict(team2);
            var team3 = this._teamService.GetTeam(team2.ID) as Team;

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

            var team2 = this._teamService.GetTeam(team.ID) as Team;

            this._teamService.Delete(team2);

            this.Evict(team2);

            var team3 = this._teamService.GetTeam(team2.ID) as Team;

            Assert.IsNull(team3);
        }
    }
}