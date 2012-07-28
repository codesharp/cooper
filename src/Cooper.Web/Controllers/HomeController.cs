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
                ViewBag.web_open = wc.DownloadString("https://api.github.com/repos/codesharp/cooper/issues");
            using (var wc = new System.Net.WebClient() { Encoding = System.Text.Encoding.UTF8 })
                ViewBag.web_closed = wc.DownloadString("https://api.github.com/repos/codesharp/cooper/issues?state=close");
            using (var wc = new System.Net.WebClient() { Encoding = System.Text.Encoding.UTF8 })
                ViewBag.mobi_open = wc.DownloadString("https://api.github.com/repos/codesharp/cooper-mobi/issues");
            using (var wc = new System.Net.WebClient() { Encoding = System.Text.Encoding.UTF8 })
                ViewBag.mobi_closed = wc.DownloadString("https://api.github.com/repos/codesharp/cooper-mobi/issues?state=close");
            //using (var wc = new System.Net.WebClient() { Encoding = System.Text.Encoding.UTF8 })
            //    ViewBag.Events = wc.DownloadString("https://api.github.com/repos/codesharp/cooper/issues/events");
            return View();
        }
        //模拟iframe内使用
        public ActionResult IFrame()
        {
            return View();
        }

        public ActionResult Feedback(string level, string content)
        {
            this._log.InfoFormat("来自用户{0}的反馈：{1} | {2}", User.Identity.Name, level, content);
            return Content(string.Empty);
        }
    }
}
