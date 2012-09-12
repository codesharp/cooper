//Copyright (c) CodeSharp.  All rights reserved. - http://www.icodesharp.com/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using CodeSharp.Core.DomainBase;
using CodeSharp.Core.Utils;

namespace Cooper.Model.Accounts
{
    /// <summary>个人账号设置
    /// </summary>
    public class Profile : EntityBase<int>
    {
        private Account _account { get; set; }
        private ExtensionDictionary _extensionDictionary = new ExtensionDictionary();

        /// <summary>根据键获取对应设置
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        public string this[string key]
        {
            get { return _extensionDictionary[key]; }
            set { _extensionDictionary[key] = value; }
        }
        /// <summary>获取所有扩展信息字典
        /// </summary>
        /// <returns></returns>
        public IDictionary<string, string> GetExtensions()
        {
            return _extensionDictionary.GetExtensions();
        }

        protected Profile() { }
        internal Profile(Account account)
            : this()
        {
            this._account = account;
        }
    }
}