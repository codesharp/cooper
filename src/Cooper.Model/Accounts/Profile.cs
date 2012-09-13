//Copyright (c) CodeSharp.  All rights reserved. - http://www.icodesharp.com/

using CodeSharp.Core.DomainBase;

namespace Cooper.Model.Accounts
{
    /// <summary>个人账号设置
    /// </summary>
    public class Profile : EntityBase<int>
    {
        private Account _account { get; set; }
        private ExtensionDictionary _settings = new ExtensionDictionary();

        /// <summary>自定义扩展信息字典
        /// </summary>
        public ExtensionDictionary Settings
        {
            get
            {
                if (this._settings == null)
                    this._settings = new ExtensionDictionary();
                return _settings;
            }
        }

        protected Profile() { }
        internal Profile(Account account)
            : this()
        {
            this._account = account;
        }
    }
}