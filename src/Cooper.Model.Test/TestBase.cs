//Copyright (c) CodeSharp.  All rights reserved. - http://www.icodesharp.com/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using CodeSharp.Core;
using CodeSharp.Core.Castles;
using CodeSharp.Core.Services;
using Cooper.Model.Accounts;
using Cooper.Model.Contacts;
using Cooper.Model.Tasks;
using Cooper.Model.Teams;
using NUnit.Framework;

namespace Cooper.Model.Test
{
    [TestFixture]
    [Microsoft.VisualStudio.TestTools.UnitTesting.TestClass]
    public class TestBase
    {
        protected static Random _rd = new Random();
        protected ILog _log;
        protected Castle.Facilities.NHibernateIntegration.ISessionManager _sessionManager;
        protected Cooper.Model.Tasks.ITaskService _taskService;
        protected IAccountService _accountService;
        protected IAccountConnectionService _accountConnectionService;
        protected IAccountHelper _accountHelper;
        protected ITaskFolderService _taskFolderService;
        protected IAddressBookService _addressBookService;
        protected IContactGroupService _contactGroupService;
        protected IContactService _contactService;
        protected ITeamService _teamService;
        protected ITeamMemberService _teamMemberService;
        protected IProjectService _projectService;
        protected Cooper.Model.Teams.ITaskService _teamTaskService;

        [TestFixtureSetUp]
        [Microsoft.VisualStudio.TestTools.UnitTesting.TestInitialize]
        public void TestFixtureSetUp()
        {
            try
            {
                CodeSharp.Core.Configuration.ConfigWithEmbeddedXml(null
                    , "application_config"
                    , Assembly.GetExecutingAssembly()
                    , "Cooper.Model.Test.ConfigFiles")
                    .RenderProperties()
                    .Castle(o => this.Resolve(o.Container));

                //初始化同步锁
                DependencyResolver.Resolve<ILockHelper>().Init<Account>();
                DependencyResolver.Resolve<ILockHelper>().Init<GoogleConnection>();
                DependencyResolver.Resolve<ILockHelper>().Init<GitHubConnection>();
            }
            catch (Exception e)
            {
                if (!e.Message.Contains("不可重复初始化配置"))
                    Console.WriteLine(e.Message);
            }

            this._log = DependencyResolver.Resolve<ILoggerFactory>().Create(this.GetType());
            this._sessionManager = DependencyResolver.Resolve<Castle.Facilities.NHibernateIntegration.ISessionManager>();

            this._accountHelper = DependencyResolver.Resolve<IAccountHelper>();
            this._accountService = DependencyResolver.Resolve<IAccountService>();
            this._accountConnectionService = DependencyResolver.Resolve<IAccountConnectionService>();
            this._taskService = DependencyResolver.Resolve<Cooper.Model.Tasks.ITaskService>();
            this._taskFolderService = DependencyResolver.Resolve<ITaskFolderService>();
            this._addressBookService = DependencyResolver.Resolve<IAddressBookService>();
            this._contactGroupService = DependencyResolver.Resolve<IContactGroupService>();
            this._contactService = DependencyResolver.Resolve<IContactService>();
            this._teamService = DependencyResolver.Resolve<ITeamService>();
            this._teamMemberService = DependencyResolver.Resolve<ITeamMemberService>();
            this._projectService = DependencyResolver.Resolve<IProjectService>();
            this._teamTaskService = DependencyResolver.Resolve<Cooper.Model.Teams.ITaskService>();
        }

        protected virtual void Resolve(Castle.Windsor.IWindsorContainer windsor)
        {
            //常规注册
            windsor.RegisterRepositories(Assembly.Load("Cooper.Repositories"));
            windsor.RegisterServices(Assembly.Load("Cooper.Model"));
            windsor.RegisterComponent(Assembly.Load("Cooper.Model"));
        }
        protected void Evict(object entity)
        {
            this._sessionManager.OpenSession().Evict(entity);
        }
        protected void Idle()
        {
            System.Threading.Thread.Sleep(100);
        }
        protected void Idle(int second)
        {
            System.Threading.Thread.Sleep(second * 1000);
        }
        protected string RandomString()
        {
            return "Cooper_" + DateTime.Now.ToString("yyyyMMddHHmmss") + DateTime.Now.Ticks + _rd.Next(100);
        }
        protected void AssertParallel(Action func, int total, int expected)
        {
            var flag = 0;
            new int[total].AsParallel().ForAll(i =>
            {
                try
                {
                    func();
                    System.Threading.Interlocked.Increment(ref flag);
                }
                catch (Exception e) { this._log.Info(e.Message); }
            });
            Assert.AreEqual(expected, flag);
        }
        protected Account CreateAccount()
        {
            var a = new Account(this.RandomString());
            this._accountService.Create(a);
            return a;
        }
        protected PersonalTaskFolder CreatePersonalTaskFolder(Account a)
        {
            var folder = new PersonalTaskFolder(this.RandomString(), a);
            this._taskFolderService.Create(folder);
            return folder;
        }
        protected DateTime FormatTime(DateTime time)
        {
            return new DateTime(time.Year, time.Month, time.Day, time.Hour, time.Minute, time.Second);
        }
    }
}
