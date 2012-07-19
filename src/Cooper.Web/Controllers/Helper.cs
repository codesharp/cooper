//Copyright (c) CodeSharp.  All rights reserved. - http://www.codesharp.cn/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using CodeSharp.Core;

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
}