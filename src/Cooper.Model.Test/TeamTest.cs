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
            var member = this._teamService.AddFullMember(RandomString(), RandomEmailString(), team);

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
            var task = new Task(member, team);
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
            var task = new Task(member, team);
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
            Assert.AreEqual(1, task.ProjectIds.Count());

            this._teamService.RemoveProject(project, team);
            this.Evict(project);
            this.Evict(team);
            this.Evict(task);
            team = this._teamService.GetTeam(team.ID);
            project = team.GetProject(project.ID);
            task = this._teamTaskService.GetTask(task.ID);
            Assert.IsNull(project);
            Assert.IsNotNull(task);
            Assert.AreEqual(0, task.ProjectIds.Count());
        }
        [Test]
        [Microsoft.VisualStudio.TestTools.UnitTesting.TestMethod]
        public void ClearCommentCreatorsAfterTeamMemberRemovedTest()
        {
            var account = CreateAccount();
            var team = CreateSampleTeam();
            var creatorMember = AddSampleMemberToTeam(account, team);
            var member = AddSampleMemberToTeam(team);
            var task = new Task(creatorMember, team);
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

            var member1 = AddSampleMemberToTeam(account, team1);
            var member2 = AddSampleMemberToTeam(team2);
            var member3 = AddSampleMemberToTeam(account, team3);

            var teams = this._teamService.GetTeamsByAccount(account);

            Assert.AreEqual(2, teams.Count());
            Assert.IsTrue(teams.Any(x => x.ID == team1.ID));
            Assert.IsTrue(teams.Any(x => x.ID == team3.ID));
        }
        [Test]
        [Microsoft.VisualStudio.TestTools.UnitTesting.TestMethod]
        public void GetUnassociatedTeamsTest()
        {
            var account = CreateAccount();
            var team1 = CreateSampleTeam();
            var team2 = CreateSampleTeam();
            var team3 = CreateSampleTeam();

            var memberName = RandomString();
            var memberEmail = RandomEmailString();
            var member1 = this._teamService.AddFullMember(memberName, memberEmail, team1, account);
            var member2 = this._teamService.AddFullMember(memberName, memberEmail, team2);
            var member3 = this._teamService.AddFullMember(memberName, memberEmail, team3);

            var teams = _teamService.GetUnassociatedTeams(memberEmail);

            Assert.AreEqual(2, teams.Count());
            Assert.IsTrue(teams.Any(x => x.ID == team2.ID));
            Assert.IsTrue(teams.Any(x => x.ID == team3.ID));
        }

        //TODO,UNDONE
        //发现在测试
        //TeamTest.AddTeamMemberWithDuplicateEmailTest
        //TeamTest.AddTeamMemberWithDuplicateAccountTest
        //TeamTest.AssociateTeamMemberWithDuplicateAccountTest
        //这三个并发测试的时候，如果这三个测试用例中的任何一个和TeamTest中的其他任何一个测试用例搭配，
        //并且将并发测试用例放后面，然后再TeamTest这个类的级别“Run Test”，会出现以下异常：
        //Illegal attempt to associate a collection with two open sessions
        //经过分析，原因是由于TeamService的AddMember方法接收的team参数是在外面的session创建的
        //然后在AddMember时，通过LazyLoad加载team.Members集合时，就会抛出上面的异常；
        //奇怪的是，这种异常只有在通过AssertParallel进行并发测试的时候才会出现，单个顺序的方式调用AddMember方法都不会出现该问题；
        //目前为了解决该问题又不影响功能，采用方法内部再获取属于当前Session的Team对象，然后通过获取到的team对象来AddMember；
        //其实通过测试发现，获取到的对象与外面传入的team对象的引用地址不同，也说明不是从同一个Session拿出来的；
        //最后，对于这个问题，还需要仔细分析，肯定还有更合理的解决方法
        //同坐采用上面的做法后，发现还是会导致其他问题，就是AddMember之后，team对象中的Members集合不会变，因为我们更新的是AddMember
        //内部自己取出来的Team，这个问题可能会导致调用者的逻辑受到影响，所以目前找不到解决方法，故暂时将以下三个测试用例注释掉

        //NOTE：为了确保以下三个用于测试并发的单元测试能够通过，并且因为原因上面也基本分析过了，所以在进行并发测试前先让team对象脱离NHibernate Session，
        //这样可以解决测试用例失败的问题，但理论上还是希望能够不脱离Session，NHibernate也不会抛错。

        [Test]
        [Microsoft.VisualStudio.TestTools.UnitTesting.TestMethod]
        public void AddTeamMemberWithDuplicateEmailTest()
        {
            var team = CreateSampleTeam();
            var name = RandomString();
            var email = RandomEmailString();
            this.Evict(team);
            this.AssertParallel(() => this._teamService.AddFullMember(name, email, team), 4, 1);
            Assert.Catch(typeof(AssertionException), () => this._teamService.AddFullMember(name, email, team));
        }
        [Test]
        [Microsoft.VisualStudio.TestTools.UnitTesting.TestMethod]
        public void AddTeamMemberWithDuplicateAccountTest()
        {
            var account = CreateAccount();
            var team = CreateSampleTeam();
            this.Evict(team);
            this.AssertParallel(() => this._teamService.AddFullMember(Guid.NewGuid().ToString(), RandomEmailString(), team, account), 4, 1);
            Assert.Catch(typeof(AssertionException), () => this._teamService.AddFullMember(Guid.NewGuid().ToString(), RandomEmailString(), team, account));
        }
        [Test]
        [Microsoft.VisualStudio.TestTools.UnitTesting.TestMethod]
        public void AssociateTeamMemberWithDuplicateAccountTest()
        {
            var team = CreateSampleTeam();
            var account = CreateAccount();
            var member = this._teamService.AddFullMember(RandomString(), RandomEmailString(), team);
            this.Evict(team);
            this.AssertParallel(() => this._teamService.AssociateMemberAccount(team, member, account), 4, 1);
            Assert.Catch(typeof(AssertionException), () => this._teamService.AssociateMemberAccount(team, member, account));

            account = CreateAccount();
            var member2 = this._teamService.AddFullMember(RandomString(), RandomEmailString(), team, account);
            this.Evict(team);
            this.AssertParallel(() => this._teamService.AssociateMemberAccount(team, member2, account), 4, 0);
            Assert.Catch(typeof(AssertionException), () => this._teamService.AssociateMemberAccount(team, member2, account));
        }

        [Test]
        [Microsoft.VisualStudio.TestTools.UnitTesting.TestMethod]
        public void MemebrEmailTest()
        {
            var team = CreateSampleTeam();
            var email = RandomString();
            Assert.Catch(typeof(AssertionException), () => this._teamService.AddFullMember(RandomString(), email, team));
            email = RandomString() + "@";
            Assert.Catch(typeof(AssertionException), () => this._teamService.AddFullMember(RandomString(), email, team));
            email = RandomString() + "@1";
            Assert.Catch(typeof(AssertionException), () => this._teamService.AddFullMember(RandomString(), email, team));
            email = RandomString() + "@1.";
            Assert.Catch(typeof(AssertionException), () => this._teamService.AddFullMember(RandomString(), email, team));
            email = "@" + RandomString();
            Assert.Catch(typeof(AssertionException), () => this._teamService.AddFullMember(RandomString(), email, team));
            email = "@" + RandomString() + ".";
            Assert.Catch(typeof(AssertionException), () => this._teamService.AddFullMember(RandomString(), email, team));
            email = "@" + RandomString() + ".1";
            Assert.Catch(typeof(AssertionException), () => this._teamService.AddFullMember(RandomString(), email, team));

            email = "1@" + RandomString() + ".1";
            var member = this._teamService.AddFullMember(RandomString(), email, team);
            Assert.IsTrue(member.ID > 0);
            email = "abc@" + RandomString() + ".123";
            member = this._teamService.AddFullMember(RandomString(), email, team);
            Assert.IsTrue(member.ID > 0);
        }
    }
}