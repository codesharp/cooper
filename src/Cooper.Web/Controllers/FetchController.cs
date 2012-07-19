//Copyright (c) CodeSharp.  All rights reserved. - http://www.codesharp.cn/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Cooper.Model.Accounts;
using System.Net;
using CodeSharp.Core.Utils;
using System.Text;

namespace Cooper.Web.Controllers
{
    //临时提供同步集成外部连接任务数据
    public class FetchController : BaseController
    {
        private Serializer _serializer = new Serializer();
        private IAccountConnectionService _connectionService;

        private Castle.Facilities.NHibernateIntegration.ISessionManager _sessionManager;
        private string _git_api_issues;
        public FetchController(IAccountConnectionService connectionService
            , string git_api_issues)
        {
            this._connectionService = connectionService;
            this._git_api_issues = git_api_issues;
        }

        protected override void OnActionExecuting(ActionExecutingContext filterContext)
        {
            base.OnActionExecuting(filterContext);
            ViewBag.Connections = this._connectionService.GetConnections(this.Context.Current);
        }

        public ActionResult Index()
        {
            return RedirectToAction("GitHub");
        }

        //集成github issues
        public ActionResult GitHub()
        {
            var git = this._connectionService.GetConnections(this.Context.Current).FirstOrDefault(o => o is GitHubConnection);

            if (git == null)
                return View();

            var access_token = _serializer.JsonDeserialize<IDictionary<string, string>>(git.Token)["access_token"];
            using (var wc = new WebClient() { Encoding = Encoding.UTF8 })
                ViewBag.Issues = wc.DownloadString(string.Format("{0}?access_token={1}"
                    , this._git_api_issues
                    , access_token));
            return View();
        }
    }
}
