using System.Reflection;
using System.Threading;
using Castle.Facilities.NHibernateIntegration;
using Castle.Windsor;
using CodeSharp.Core;
using CodeSharp.Core.Castles;
using CodeSharp.Core.Services;
using Cooper.Model;
using Cooper.Model.Accounts;
using Cooper.Model.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using NUnit.Framework;

namespace Cooper.Sync.Test
{
    [TestFixture]
    [TestClass]
    public class TestBase
    {
        protected ILog _logger;
        protected ISessionManager _sessionManager;
        protected Account _account;
        protected GoogleConnection _googleConnection;
        protected ITaskService _taskService;
        protected IAccountService _accountService;
        protected IAccountConnectionService _accountConnectionService;
        protected IAccountHelper _accountHelper;
        protected IExternalServiceProvider _externalServiceProvider;
        protected IGoogleTokenService _googleTokenService;
        protected ISyncProcesser _syncProcessor;

        [TestFixtureSetUp]
        [TestInitialize]
        public void TestFixtureSetUp()
        {
            Configuration.ConfigWithEmbeddedXml(null, "application_config", Assembly.GetExecutingAssembly(), "Cooper.Sync.Test.ConfigFiles")
                .RenderProperties()
                .Castle(resolver => Resolve(resolver.Container));

            //初始化同步锁
            DependencyResolver.Resolve<ILockHelper>().Init<Account>();
            DependencyResolver.Resolve<ILockHelper>().Init<GoogleConnection>();

            _logger = DependencyResolver.Resolve<ILoggerFactory>().Create(GetType());
            _sessionManager = DependencyResolver.Resolve<ISessionManager>();

            _accountHelper = DependencyResolver.Resolve<IAccountHelper>();
            _accountService = DependencyResolver.Resolve<IAccountService>();
            _accountConnectionService = DependencyResolver.Resolve<IAccountConnectionService>();
            _taskService = DependencyResolver.Resolve<ITaskService>();
            _externalServiceProvider = DependencyResolver.Resolve<IExternalServiceProvider>();
            _googleTokenService = DependencyResolver.Resolve<IGoogleTokenService>();

            _syncProcessor = DependencyResolver.Resolve<ISyncProcesser>();

            GoogleSyncSettings.ClientIdentifier = "234919028272-gsmmng06nheoih4ajp60oq8s33at1os0.apps.googleusercontent.com";
            GoogleSyncSettings.ClientSecret = "jXcjxFzCQGferMI37I0GSc05";
        }

        protected virtual void Resolve(IWindsorContainer windsor)
        {
            windsor.RegisterRepositories(Assembly.Load("Cooper.Repositories"));
            windsor.RegisterServices(Assembly.Load("Cooper.Model"));
            windsor.RegisterComponent(Assembly.Load("Cooper.Model"));
            windsor.RegisterServices(Assembly.Load("Cooper.Sync"));
            windsor.RegisterComponent(Assembly.Load("Cooper.Sync"));
            windsor.RegisterComponent(Assembly.Load("Cooper.Sync.Test"));
        }

        protected void Idle()
        {
            Thread.Sleep(100);
        }
        protected void Idle(int second)
        {
            Thread.Sleep(second * 1000);
        }
    }
}
