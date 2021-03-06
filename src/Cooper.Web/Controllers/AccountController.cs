﻿//Copyright (c) CodeSharp.  All rights reserved. - http://www.icodesharp.com/

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Web;
using System.Web.Mvc;
using System.Web.Security;
using Cooper.Model.Accounts;
using CodeSharp.Core;
using CodeSharp.Core.Services;
using CodeSharp.Core.Utils;
using System.Text.RegularExpressions;
using Cooper.Model.Teams;

namespace Cooper.Web.Controllers
{
    /// <summary>处理账号相关
    /// </summary>
    public class AccountController : Controller
    {
        protected Serializer _serializer = new Serializer();
        protected ILog _log;
        protected IContextService _context;
        protected IAccountHelper _accountHelper;
        protected IAccountService _accountService;
        protected IAccountConnectionService _accountConnectionService;
        protected ITeamService _teamService;
        protected string _sysConfig_versionFlag;
     
        private string _googleOAuth2Url;
        private string _googleOAuth2TokenUrl;
        private string _googleOAuth2UserUrl;
        private string _googleScope;
        private string _googleClientId;
        private string _googleClientSecret;
        private string _googleClientId_mobi;
        private string _googleClientSecret_mobi;

        private string _gitOAuthUrl;
        private string _gitOAuthTokenUrl;
        private string _gitOAuthUserUrl;
        private string _gitClientId;
        private string _gitClientSecret;
        private string _gitScope;

        public AccountController(ILoggerFactory factory
            , IContextService context
            , IAccountHelper accountHelper
            , IAccountService accountService
            , IAccountConnectionService accountConnectionService
            
            , ITeamService teamService

            , string sysConfig_versionFlag

            , string googleOAuth2Url
            , string googleOAuth2TokenUrl
            , string googleOAuth2UserUrl
            , string googleScope
            , string googleClientId
            , string googleClientSecret
            , string googleClientId_mobi
            , string googleClientSecret_mobi

            , string gitOAuthUrl
            , string gitOAuthTokenUrl
            , string gitOAuthUserUrl
            , string gitClientId
            , string gitClientSecret
            , string gitScope)
        {
            this._log = factory.Create(typeof(AccountController));
            this._context = context;
            this._accountHelper = accountHelper;
            this._accountService = accountService;
            this._accountConnectionService = accountConnectionService;

            this._teamService = teamService;

            this._sysConfig_versionFlag = sysConfig_versionFlag;

            this._googleOAuth2Url = googleOAuth2Url;
            this._googleOAuth2TokenUrl = googleOAuth2TokenUrl;
            this._googleOAuth2UserUrl = googleOAuth2UserUrl;
            this._googleScope = googleScope;
            this._googleClientId = googleClientId;
            this._googleClientSecret = googleClientSecret;
            this._googleClientId_mobi = googleClientId_mobi;
            this._googleClientSecret_mobi = googleClientSecret_mobi;


            this._gitOAuthUrl = gitOAuthUrl;
            this._gitOAuthTokenUrl = gitOAuthTokenUrl;
            this._gitOAuthUserUrl = gitOAuthUserUrl;
            this._gitClientId = gitClientId;
            this._gitClientSecret = gitClientSecret;
            this._gitScope = gitScope;
        }

        //处理个别需要登录的Action
        protected override void OnActionExecuting(ActionExecutingContext filterContext)
        {
            var a = filterContext.ActionDescriptor.ActionName;
            if ((a.Equals("Profile")
                || a.Equals("Connection")
                || a.Equals("StartSync")
                || a.Equals("QuerySync"))
                && !Helper.IsLogin(this._context, this.User, this._log))
            {
                filterContext.Result = RedirectToAction("Login", new { deny = true });
                return;
            }
            base.OnActionExecuting(filterContext);
        }
        //个人设置
        public ActionResult Profile()
        {
            ViewBag.Account = this._context.Current;
            return View();
        }
        //外部账号连接
        public ActionResult Connection()
        {
            ViewBag.Connections = this._accountConnectionService.GetConnections(this._context.Current);
            this.SetConnectionUrls("connect");
            return View();
        }
        //删除连接
        [HttpPost]
        public ActionResult Connection(string connectionId)
        {
            var c = this._accountConnectionService.GetConnection(int.Parse(connectionId));
            if (c != null && c.AccountId == this._context.Current.ID)
                this._accountConnectionService.Delete(c);
            return Connection();
        }
        //启动同步
        public ActionResult StartSync(string connectionId)
        {
            //if (this.SyncService == null)
            //    this._log.WarnFormat("{0}服务依赖为满足", typeof(Cooper.Api.ISyncJobService));
            //if (this.SyncService != null)
            //    this.SyncService.StartSyncJob(int.Parse(connectionId));
            return Json(connectionId, JsonRequestBehavior.AllowGet);
        }
        //查询同步进度
        public ActionResult QuerySync(string connectionId)
        {
            return Json(true, JsonRequestBehavior.AllowGet);
            //return Json(this.SyncService != null
            //    ? !this.SyncService.IsSyncJobRunning(int.Parse(connectionId))
            //    : true
            //    , JsonRequestBehavior.AllowGet);
        }

        #region Login
        public ActionResult Login()
        {
            this.SetConnectionUrls("login");
            return View();
        }
        [HttpPost]
        public ActionResult Login(string userName, string password)
        {
            if (string.IsNullOrWhiteSpace(userName)
                || string.IsNullOrWhiteSpace(password))
                throw new CooperknownException(this.Lang().username_or_password_cannot_empty);

            var a = this._accountService.GetAccount(userName);

            var release = this._sysConfig_versionFlag.Equals("Release");
            if (!release)
                //非正式环境时自动创建账号便于测试
                if (a == null)
                    this._accountService.Create(a = new Account(userName));

            //TODO:目前正式环境不提供注册，之后将改为MD5
            if ((a != null && a.CheckPassword(password)) || !release)
                this.SetLogin(a.ID);
            else
                throw new CooperknownException(this.Lang().username_or_password_was_wrong);

            return Request.IsAjaxRequest() ? Json(a.Name) : this.Home();
        }
        public ActionResult Logout()
        {
            FormsAuthentication.SignOut();
            if (Request.IsAjaxRequest())
                return Json(true);
            return this.RedirectToAction("Login");
        }
        #endregion

        //TODO:重构以下各类型连接登录和连接
        /// <summary>处理Google登录回调
        /// </summary>
        /// <param name="error">异常信息</param>
        /// <param name="code"></param>
        /// <param name="state">
        /// 指定回调动作，login、connect、joke，
        /// 使用joke会忽略请求
        /// </param>
        /// <param name="mobi">是否是来自mobi的回调请求</param>
        /// <returns></returns>
        public ActionResult GoogleLogin(string error, string code, string state, string mobi)
        {
            //it's a joke?!
            if (state == "joke")
                return Json(false, JsonRequestBehavior.AllowGet);

            if (!string.IsNullOrWhiteSpace(error))
                throw new CooperknownException(error);

            //根据google回调获取对应google账号
            var grant = this.GetGoogleGrantByCode(code);
            if (this._log.IsDebugEnabled) this._log.Debug(grant);
            var dict = _serializer.JsonDeserialize<IDictionary<string, string>>(grant);
            var email = this.GetGoogleAccount(dict["access_token"]);

            if (state == "login")
                this.SetLogin<GoogleConnection>(email, grant);
            else if (state == "connect")
                this.Connect<GoogleConnection>(email, grant);

            return Convert.ToBoolean(mobi)
                ? Json(email, JsonRequestBehavior.AllowGet)
                : this.StateResult(state);
        }
        //Git
        public ActionResult GitLogin(string error, string code, string state)
        {
            if (!string.IsNullOrWhiteSpace(error))
                throw new CooperknownException(error);
            //根据Git回调获取对应Git账号
            var grant = this.GetGitGrantByCode(code, state);
            this._log.Debug(grant);
            var dict = _serializer.JsonDeserialize<IDictionary<string, string>>(grant);
            if (dict.ContainsKey("error"))
                throw new CooperknownException(dict["error"]);
            var account = this.GetGitAccount(dict["access_token"]);

            if (state == "login")
                this.SetLogin<GitHubConnection>(account, grant);
            else if (state == "connect")
                this.Connect<GitHubConnection>(account, grant);

            return this.StateResult(state);
        }

        protected ActionResult Home()
        {
            return RedirectToAction("Index", "Home");
        }
        protected void SetLogin(int accountId)
        {
            FormsAuthentication.SetAuthCookie(accountId.ToString(), true);
        }
        protected void SetLogin<T>(string name, string token) where T : AccountConnection
        {
            var c = this._accountConnectionService.GetConnection<T>(name);
            var flag = c == null;
            //设置登录
            this.SetLogin(flag
                ? this._accountHelper.CreateBy<T>(name, token).ID//根据外部连接账号创建系统内账号
                : c.AccountId);

            //if (!flag) return;
            c = this._accountConnectionService.GetConnection<T>(name);
            c.SetToken(token);
            this._accountConnectionService.Update(c);
            //用于指示UI启动同步
            ViewBag.ConnectionId = c.ID;
            //HACK:账号创建时自动关联一切可以关联的信息
            //if (flag)
            //UNDONE:由于导入程序未自动创建关联，目前在登录时总是创建关联
            this.AssociateEverything(c);
        }
        protected void Connect<T>(string name, string token) where T : AccountConnection
        {
            var a = this._context.Current;
            var c = this._accountConnectionService.GetConnection<T>(name);

            if (this._context.Current == null
                || (c != null && c.AccountId == a.ID)) return;
            if (c != null && c.AccountId != a.ID)
                throw new CooperknownException(string.Format(this.Lang().sorry_already_connect_another, name));

            if (typeof(T) == typeof(GoogleConnection))
                this._accountConnectionService.Create(c = new GoogleConnection(name, token, a));
            else if (typeof(T) == typeof(GitHubConnection))
                this._accountConnectionService.Create(c = new GitHubConnection(name, token, a));
            //HACK:连接账号时自动关联一切可以关联的信息
            this.AssociateEverything(c);
        }
        protected void SetConnectionUrls(string state)
        {
            ViewBag.GoogleUrl = this.GetGoogleUrl(state);
            ViewBag.GitUrl = this.GetGitUrl(state);
        }
        protected ActionResult StateResult(string state)
        {
            return View((string.IsNullOrWhiteSpace(state) ? "Login" : state) + "Success");
        }
        /// <summary>关联一切可以关联的信息，子类可在合适的时机调用此方法，目前包括TeamMember
        /// </summary>
        /// <param name="c"></param>
        protected void AssociateEverything(AccountConnection c)
        {
            if (c == null) return;
            var a = this._accountService.GetAccount(c.AccountId);
            var email = this.ParseEmail(c);

            //HACK:根据email建立关联TeamMember和Account关联
            if (!string.IsNullOrWhiteSpace(email))
                this._teamService.GetUnassociatedTeams(email).ToList().ForEach(o =>
                {
                    try { this._teamService.AssociateMemberAccount(o, o.GetMember(email), a); }
                    catch (Exception e)
                    {
                        this._log.Error(string.Format("为账号#{0}邮箱{1}与团队#{2}|{3}创建关联时异常"
                            , c.AccountId
                            , email
                            , o.ID
                            , o.Name)
                            , e);
                    }
                });
            //其他...
        }
        /// <summary>根据账号连接获取Email信息，默认处理Google账号
        /// </summary>
        /// <param name="c"></param>
        /// <returns></returns>
        protected virtual string ParseEmail(AccountConnection c)
        {
            return c is GoogleConnection ? c.Name : null;
        }

        //问题参考：https://github.com/netshare/Cooper/issues/4
        private string GetGoogleUrl(string state)
        {
            return string.Format(
                //access_type=offline申请离线使用授权
                "{0}?response_type=code&approval_prompt=force&access_type=offline&scope={1}&redirect_uri={2}&client_id={3}&state={4}"
                , this._googleOAuth2Url
                , HttpUtility.UrlEncode(this._googleScope)
                , HttpUtility.UrlEncode(this.GetGoogleRedirectUrl())
                , this._googleClientId
                , state);
        }
        private string GetGoogleRedirectUrl()
        {
            return Url.ToPublicUrl(Url.Action("GoogleLogin"));
        }
        //格式如{ "access_token" : "", "token_type" : "Bearer", "expires_in" : 3600, "id_token" : "", "refresh_token" : "" }
        private string GetGoogleGrantByCode(string code)
        {
            //解决部分服务器证书问题
            ServicePointManager.ServerCertificateValidationCallback = new System.Net.Security.RemoteCertificateValidationCallback(delegate { return true; });
            ServicePointManager.SecurityProtocol = SecurityProtocolType.Ssl3;
            ServicePointManager.Expect100Continue = true;

            using (var wc = new WebClient())
            {
                wc.Headers.Add("Content-Type", "application/x-www-form-urlencoded");
                //登录后根据code获取token
                var data = string.Format("client_id={0}&client_secret={1}&redirect_uri={2}&code={3}&grant_type=authorization_code"
                    , this._googleClientId
                    , this._googleClientSecret
                    , HttpUtility.UrlEncode(this.GetGoogleRedirectUrl())
                    , code);
                if (this._log.IsDebugEnabled)
                    this._log.Debug(data);
                try
                {
                    return Encoding.UTF8.GetString(wc.UploadData(this._googleOAuth2TokenUrl
                        , "POST"
                        , Encoding.UTF8.GetBytes(data)));
                }
                catch (WebException e)
                {
                    if (e.Response == null)
                        throw e;
                    using (var reader = new StreamReader((e as WebException).Response.GetResponseStream()))
                        throw new Exception(reader.ReadToEnd());
                }
            }
        }
        private string GetGoogleAccount(string access_token)
        {
            ServicePointManager.ServerCertificateValidationCallback = new System.Net.Security.RemoteCertificateValidationCallback(delegate { return true; });
            using (var wc = new WebClient() { Encoding = Encoding.UTF8 })
                return _serializer.JsonDeserialize<IDictionary<string, string>>(
                    wc.DownloadString(this._googleOAuth2UserUrl + "?access_token=" + access_token))["email"];
        }

        private string GetGitUrl(string state)
        {
            return string.Format(
                "{0}?scope={1}&redirect_uri={2}&client_id={3}&state={4}"
                , this._gitOAuthUrl
                , HttpUtility.UrlEncode(this._gitScope)
                , HttpUtility.UrlEncode(this.GetGitRedirectUrl())
                , this._gitClientId
                , state);
        }
        private string GetGitRedirectUrl()
        {
            return Url.ToPublicUrl(Url.Action("GitLogin"));
        }
        private string GetGitGrantByCode(string code, string state)
        {
            using (var wc = new WebClient())
            {
                wc.Headers.Add("Content-Type", "application/x-www-form-urlencoded");
                wc.Headers.Add("Accept", "application/json");
                //登录后根据code获取token
                var data = string.Format("client_id={0}&client_secret={1}&redirect_uri={2}&code={3}&state={4}"
                    , this._gitClientId
                    , this._gitClientSecret
                    , HttpUtility.UrlEncode(this.GetGitRedirectUrl())
                    , code
                    , state);
                if (this._log.IsDebugEnabled)
                    this._log.Debug(data);
                try
                {
                    return Encoding.UTF8.GetString(wc.UploadData(this._gitOAuthTokenUrl
                        , "POST"
                        , Encoding.UTF8.GetBytes(data)));
                }
                catch (WebException e)
                {
                    if (e.Response == null)
                        throw e;
                    using (var reader = new StreamReader((e as WebException).Response.GetResponseStream()))
                        throw new Exception(reader.ReadToEnd());
                }
            }
        }
        private string GetGitAccount(string access_token)
        {
            string result;
            using (var wc = new WebClient() { Encoding = Encoding.UTF8 })
                result = wc.DownloadString(this._gitOAuthUserUrl + "?access_token=" + access_token);
            this._log.Debug(result);
            return _serializer.JsonDeserialize<IDictionary<string, object>>(result)["login"].ToString();//TODO:外部连接若用户名可变更，需要同时记录id，如Git
        }
    }
}