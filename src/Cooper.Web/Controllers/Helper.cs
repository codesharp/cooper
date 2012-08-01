//Copyright (c) CodeSharp.  All rights reserved. - http://www.icodesharp.com/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using CodeSharp.Core;
using System.Web.Mvc;

namespace Cooper.Web.Controllers
{
    /// <summary>提供基本UI辅助
    /// </summary>
    public static class Helper
    {
        public static bool IsLogin(IContextService context)
        {
            return Helper.IsLogin(context, HttpContext.Current.User, null);
        }
        public static bool IsLogin(IContextService context
            , System.Security.Principal.IPrincipal user
            , ILog log)
        {
            if (context == null || context.Current == null)
            {
                if (log != null && log.IsDebugEnabled)
                    log.DebugFormat("[Deny]IContextService={0}|Current={1}|IsAuthenticated={2}|Name={3}"
                        , context
                        , context != null ? context.Current : null
                        , user.Identity.IsAuthenticated
                        , user.Identity.Name);
                return false;
            }
            return true;
        }
    }
    /// <summary>强制要求https，能够处理ssl直连和反向代理层面的ssl（如stunnel）
    /// </summary>
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, Inherited = true, AllowMultiple = false)]
    public class RequireHttpsAttribute : System.Web.Mvc.RequireHttpsAttribute
    {
        public override void OnAuthorization(AuthorizationContext filterContext)
        {
            if (filterContext.HttpContext.Request.IsSecureConnection)
                return;
            if (string.Equals(filterContext.HttpContext.Request.Headers["X-Forwarded-Proto"]
                , "https"
                , StringComparison.InvariantCultureIgnoreCase))
                return;
            if (filterContext.HttpContext.Request.IsLocal)
                return;

            this.HandleNonHttpsRequest(filterContext);
        }
    }
}