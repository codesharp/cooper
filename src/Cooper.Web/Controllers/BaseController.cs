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
        /// <summary>获取当前上下文
        /// <remarks>由于通用依赖而简化注入</remarks>
        /// </summary>
        public IContextService Context { get; set; }

        protected override void OnActionExecuting(ActionExecutingContext filterContext)
        {
            //简易身份验证
            if (!Helper.IsLogin(this.Context, this.User, this._log))
            {
                filterContext.Result = RedirectToAction("Login", "Account", new { deny = true });
                return;
            }

            var r = filterContext.HttpContext.Request;
            //浏览器支持判断
            if (r.HttpMethod.Equals("get", StringComparison.InvariantCultureIgnoreCase))
                if (r.Browser.MajorVersion == 6)
                {
                    filterContext.Result = this.NotSupport();
                    return;
                }

            base.OnActionExecuting(filterContext);
        }

        public BaseController() { }
        public BaseController(ILoggerFactory factory)
        {
            this._log = factory.Create(typeof(BaseController));
        }

        public ActionResult NotSupport()
        {
            return View("NotSupport");
        }
        protected ActionResult Error(string error)
        {
            ViewBag.Error = error;
            return View("Error");
        }
    }
}
