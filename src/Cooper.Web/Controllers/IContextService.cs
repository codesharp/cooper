//Copyright (c) CodeSharp.  All rights reserved. - http://www.codesharp.cn/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Cooper.Model.Accounts;
using System.Web;

namespace Cooper.Web.Controllers
{
    //UNDONE:后续将移植为支持服务节点

    /// <summary>上下文服务
    /// </summary>
    public interface IContextService
    {
        /// <summary>获取当前账号
        /// </summary>
        Account Current { get; }
        /// <summary>从当前上下文获取指定对象
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        T Get<T>();
        /// <summary>从当前上下文获取指定键的值
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        object Get(string key);
    }

    //基于Form验证和Http实现上下文
    [CodeSharp.Core.Component]
    public class WebContextService : IContextService
    {
        private IAccountService _accountService;
        public WebContextService(IAccountService accountService)
        {
            this._accountService = accountService;
        }

        #region IContextService Members

        public Account Current
        {
            get
            {
                return HttpContext.Current.User.Identity.IsAuthenticated
                    ? this._accountService.GetAccount(Convert.ToInt32(HttpContext.Current.User.Identity.Name))
                    : null;
            }
        }

        public T Get<T>()
        {
            throw new NotImplementedException();
        }

        public object Get(string key)
        {
            throw new NotImplementedException();
        }

        #endregion
    }
}