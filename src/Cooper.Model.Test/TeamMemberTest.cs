//Copyright (c) CodeSharp.  All rights reserved. - http://www.icodesharp.com/

using System;
using System.Linq;
using Cooper.Model.Teams;
using NUnit.Framework;

namespace Cooper.Model.Test
{
    [TestFixture]
    [Microsoft.VisualStudio.TestTools.UnitTesting.TestClass]
    public class TeamMemberMemberTest : TestBase
    {
        [Test]
        [Microsoft.VisualStudio.TestTools.UnitTesting.TestMethod]
        public void TotalTests()
        {
            CreateTeamMember();
            UpdateTeamMember();
            DeleteTeamMember();
            GetTeamMembers();
        }

        [Test]
        [Microsoft.VisualStudio.TestTools.UnitTesting.TestMethod]
        public void CreateTeamMember()
        {
            var team = new Team("Test Team");
            this._teamService.Create(team);

            var teamMember = new TeamMember("TeamMember 1", "xuehua@163.com", team);

            Assert.AreNotEqual(DateTime.MinValue, team.CreateTime);
            Assert.AreEqual(team.ID, teamMember.TeamId);

            this._teamMemberService.Create(teamMember);
            Assert.Greater(teamMember.ID, 0);

            this.Evict(teamMember);

            var teamMember2 = this._teamMemberService.GetTeamMember(teamMember.ID) as TeamMember;

            Assert.AreEqual(teamMember.Name, teamMember2.Name);
            Assert.AreEqual(teamMember.Email, teamMember2.Email);
            Assert.AreEqual(teamMember.TeamId, teamMember2.TeamId);
            Assert.AreEqual(FormatTime(teamMember.CreateTime), FormatTime(teamMember2.CreateTime));
        }
        [Test]
        [Microsoft.VisualStudio.TestTools.UnitTesting.TestMethod]
        public void UpdateTeamMember()
        {
            var team = new Team("Test Team");
            this._teamService.Create(team);

            var teamMember = new TeamMember("TeamMember 1", "xuehua@163.com", team);
            this._teamMemberService.Create(teamMember);

            this.Evict(teamMember);

            var teamMember2 = this._teamMemberService.GetTeamMember(teamMember.ID) as TeamMember;

            teamMember2.SetName(teamMember.Name + "_updated");
            teamMember2.SetEmail(teamMember.Email + "_updated");

            this._teamMemberService.Update(teamMember2);
            this.Evict(teamMember2);
            var teamMember3 = this._teamMemberService.GetTeamMember(teamMember2.ID) as TeamMember;

            Assert.AreEqual(teamMember2.Name, teamMember3.Name);
            Assert.AreEqual(teamMember2.Email, teamMember3.Email);
            Assert.AreEqual(teamMember2.TeamId, teamMember3.TeamId);
            Assert.AreEqual(FormatTime(teamMember2.CreateTime), FormatTime(teamMember3.CreateTime));
        }
        [Test]
        [Microsoft.VisualStudio.TestTools.UnitTesting.TestMethod]
        public void DeleteTeamMember()
        {
            var team = new Team("Test Team");
            this._teamService.Create(team);

            var teamMember = new TeamMember("TeamMember 1", "xuehua@163.com", team);
            this._teamMemberService.Create(teamMember);

            this.Evict(teamMember);

            var teamMember2 = this._teamMemberService.GetTeamMember(teamMember.ID) as TeamMember;

            this._teamMemberService.Delete(teamMember2);

            this.Evict(teamMember2);

            var teamMember3 = this._teamMemberService.GetTeamMember(teamMember2.ID) as TeamMember;

            Assert.IsNull(teamMember3);
        }
        [Test]
        [Microsoft.VisualStudio.TestTools.UnitTesting.TestMethod]
        public void GetTeamMembers()
        {
            var team = new Team("Test Team");
            this._teamService.Create(team);

            var teamMember1 = new TeamMember("TeamMember 1", "xuehua1@163.com", team);
            var teamMember2 = new TeamMember("TeamMember 2", "xuehua2@163.com", team);
            var teamMember3 = new TeamMember("TeamMember 3", "xuehua3@163.com", team);

            this._teamMemberService.Create(teamMember1);
            this._teamMemberService.Create(teamMember2);
            this._teamMemberService.Create(teamMember3);

            this.Evict(teamMember1);
            this.Evict(teamMember2);
            this.Evict(teamMember3);

            var teamMembers = this._teamMemberService.GetTeamMembersByTeam(team);

            Assert.AreEqual(3, teamMembers.Count());
            Assert.IsTrue(teamMembers.Any(x => x.ID == teamMember1.ID));
            Assert.IsTrue(teamMembers.Any(x => x.ID == teamMember2.ID));
            Assert.IsTrue(teamMembers.Any(x => x.ID == teamMember3.ID));
        }
    }
}