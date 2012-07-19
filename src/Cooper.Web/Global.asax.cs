//Copyright (c) CodeSharp.  All rights reserved. - http://www.codesharp.cn/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Mvc.Html;
using System.Web.Routing;
using CodeSharp.Framework.Castles;
using CodeSharp.Core.Castles;
using CodeSharp.Core.Web;
using System.Reflection;

namespace Cooper.Web
{
    public class MvcApplication : CodeSharp.Framework.Castles.Web.WebApplication
    {
        public static void RegisterGlobalFilters(GlobalFilterCollection filters)
        {
            filters.Add(new HandleErrorAttribute());
        }
        public static void RegisterRoutes(RouteCollection routes)
        {
            routes.IgnoreRoute("{resource}.axd/{*pathInfo}");
            routes.IgnoreRoute("{*favicon}", new { favicon = @"(.*/)?favicon.ico(/.*)?" });

            routes.MapRoute("Personal", "per/{action}/{id}", new { controller = "Personal", action = "Index", id = UrlParameter.Optional });
            routes.MapRoute("Professional", "pro/{action}/{id}", new { controller = "Professional", action = "Index", id = UrlParameter.Optional });
            routes.MapRoute("Enterprise", "ent/{action}/{id}", new { controller = "Enterprise", action = "Index", id = UrlParameter.Optional });

            routes.MapRoute("Account", "Account/{action}/{id}", new { controller = "Account", action = "Profile", id = UrlParameter.Optional });
            routes.MapRoute("Default", "{controller}/{action}/{id}", new { controller = "Home", action = "Index", id = UrlParameter.Optional });
        }
        protected void Application_Start()
        {
            AreaRegistration.RegisterAllAreas();
            RegisterGlobalFilters(GlobalFilters.Filters);
            RegisterRoutes(RouteTable.Routes);

#if DEBUG
            //CodeSharp.Framework.SystemConfig.CompileSymbol = "debug";
#else
            //CodeSharp.Framework.SystemConfig.CompileSymbol = "release";
#endif

            CodeSharp.Framework.SystemConfig
                .Configure("CooperWeb")
                .Castle()
                //.ReadProperties("CooperConfig")
                .Resolve(this.Prepare);
        }
        private void Prepare(WindsorResolver r)
        {
            var windsor = r.Container;
            //Cooper模型相关
            windsor.RegisterRepositories(Assembly.Load("Cooper.Repositories"));
            windsor.RegisterServices(Assembly.Load("Cooper.Model"));
            windsor.RegisterComponent(Assembly.Load("Cooper.Model"));
            //Controller注入
            windsor.ControllerFactory();
            windsor.RegisterControllers(Assembly.GetExecutingAssembly());
            //注册web上下文
            windsor.RegisterComponent(typeof(Cooper.Web.Controllers.WebContextService));
        }

        protected override bool IsKnownException(Exception e)
        {
            return base.IsKnownException(e) || e is CooperknownException;
        }
        protected override void OnError(Exception e)
        {
            if (!(e is CooperknownException))
                base.OnError(e);
           
            Server.ClearError();
            //TODO:切换为使用razor engine
            Response.Write((e as CooperknownException).Message);
            Response.Flush();

        }
    }

    /// <summary>描述系统内已经异常
    /// </summary>
    public class CooperknownException : Exception
    {
        public CooperknownException(string m) : base(m) { }
        public CooperknownException(string m, Exception inner) : base(m, inner) { }
    }
}

/// <summary>提供WebUI扩展，如文案、链接等
/// </summary>
public static class WebExtensions
{
    /// <summary>Cooper站点统一后缀
    /// <remarks>如： - Cooper</remarks>
    /// </summary>
    /// <param name="obj"></param>
    /// <returns></returns>
    public static string Suffix(this object obj)
    {
        return CodeSharp.Framework.SystemConfig.Settings["webSiteSuffix"];
    }
    /// <summary>输出全局设置脚本，主要用于解决url，服务器设置在js中共享等问题
    /// </summary>
    /// <param name="html"></param>
    public static void RenderSettings(this HtmlHelper html)
    {
        html.RenderPartial("Settings");
    }
    /// <summary>输出左侧导航栏
    /// </summary>
    /// <param name="html"></param>
    public static void RenderLeftBar(this HtmlHelper html)
    {
        html.RenderPartial("LeftBar");
    }
    /// <summary>获取连接类型的文字描述
    /// </summary>
    /// <param name="connection"></param>
    /// <returns></returns>
    public static string ConnectionName(this Cooper.Model.Accounts.AccountConnection connection)
    {
        return connection.GetType().Name.ConnectionName();
    }
    /// <summary>获取连接类型的文字描述
    /// </summary>
    /// <param name="type"></param>
    /// <returns></returns>
    public static string ConnectionName(this string type)
    {
        if (type.StartsWith("Google"))
            return "Google";
        else if (type.StartsWith("Git"))
            return "GitHub";
        else
            return type;
    }
    /// <summary>获取当前的环境版本，小写
    /// </summary>
    /// <returns></returns>
    public static string VersionFlag(this object obj)
    {
        return CodeSharp.Framework.SystemConfig.Settings.VersionFlag.ToLower();
    }
}