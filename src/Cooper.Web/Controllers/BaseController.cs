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
    //用于提供除Home、Account之外的控制器基类
    public class BaseController : Controller
    {
        private ILog _log;
        protected bool IsIE7 { get; private set; }
        /// <summary>获取当前上下文
        /// <remarks>由于通用依赖而简化注入</remarks>
        /// </summary>
        public IContextService Context { get; set; }

        protected override void OnActionExecuting(ActionExecutingContext filterContext)
        {
            var r = filterContext.HttpContext.Request;

            //简易身份验证
            if (!Helper.IsLogin(this.Context, this.User, this._log))
            {
                if (r.IsAjaxRequest())
                    throw new CooperknownException(this.Lang().do_not_login_or_timeout);
                filterContext.Result = RedirectToAction("Login", "Account", new { deny = true });
                return;
            }

            //Safari|Chrome|IE
            if (this._log.IsDebugEnabled)
                this._log.DebugFormat("{0}|{1}", r.Browser.Browser, r.Browser.MajorVersion);
            //浏览器支持判断
            if (r.HttpMethod.Equals("get", StringComparison.InvariantCultureIgnoreCase))
            {
                var ie = r.Browser.Browser.Equals("ie", StringComparison.InvariantCultureIgnoreCase);
                if (r.Browser.MajorVersion == 6 && ie)
                {
                    filterContext.Result = this.NotSupport();
                    return;
                }
                this.IsIE7 = ie && r.Browser.MajorVersion == 7;
                ViewBag.IsIE7 = this.IsIE7;
            }

            base.OnActionExecuting(filterContext);
        }

        public BaseController(ILoggerFactory factory)
        {
            this._log = factory.Create(typeof(BaseController));
        }

        public ActionResult NotSupport()
        {
            return View("NotSupport");
        }
        public ActionResult Error(string error)
        {
            ViewBag.Error = error;
            return View("Error");
        }
    }
}