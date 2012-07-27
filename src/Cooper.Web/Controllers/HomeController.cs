//Copyright (c) CodeSharp.  All rights reserved. - http://www.icodesharp.com/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using CodeSharp.Core.Services;
using CodeSharp.Core;

namespace Cooper.Web.Controllers
{
    public class HomeController : Controller
    {
        private ILog _log;
        private IContextService _context;

        public HomeController(ILoggerFactory factory, IContextService contextService)
        {
            this._log = factory.Create(typeof(HomeController));
            this._context = contextService;
        }

        public ActionResult Index()
        {
            if (this._context.Current == null)
                return RedirectToAction("Login", "Account");
            //已登录账号跳转至账号对应的Cooper版本
            else
                return RedirectToAction("Index", "Per");
        }

        public ActionResult Issues()
        {
            using (var wc = new System.Net.WebClient() { Encoding = System.Text.Encoding.UTF8 })
                ViewBag.Open = wc.DownloadString("https://api.github.com/repos/codesharp/cooper/issues");
            using (var wc = new System.Net.WebClient() { Encoding = System.Text.Encoding.UTF8 })
                ViewBag.Close = wc.DownloadString("https://api.github.com/repos/codesharp/cooper/issues?state=close");
            using (var wc = new System.Net.WebClient() { Encoding = System.Text.Encoding.UTF8 })
                ViewBag.Events = wc.DownloadString("https://api.github.com/repos/codesharp/cooper/issues/events");
            return View();
        }
        //模拟iframe内使用
        public ActionResult IFrame()
        {
            return View();
        }
    }
}
