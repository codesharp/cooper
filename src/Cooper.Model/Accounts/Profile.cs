//Copyright (c) CodeSharp.  All rights reserved. - http://www.codesharp.cn/

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
        private static readonly Serializer _serializer = new Serializer();
        private Account _account { get; set; }
        //为私人设置提供的通用k/v格式，若有需要结构化的设置可按需通过新增类属性完成
        private string _profile { get; set; }
        private IDictionary<string, string> _dict;

        /// <summary>根据键获取对应设置
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        public virtual string this[string key]
        {
            get
            {
                Assert.IsNotNullOrWhiteSpace(key);
                if (this._dict == null)
                    this._dict = this.Parse();
                return this._dict.ContainsKey(key) ? this._dict[key] : null;
            }
            set
            {
                Assert.IsNotNullOrWhiteSpace(key);
                if (this._dict == null)
                    this._dict = this.Parse();
                this._dict[key] = value;
                this._profile = _serializer.JsonSerialize(this._dict);
            }
        }

        protected Profile() { }
        internal Profile(Account account)
            : this()
        {
            this._account = account;
        }

        private IDictionary<string, string> Parse()
        {
            return string.IsNullOrWhiteSpace(this._profile)
                ? new Dictionary<string, string>()
                : _serializer.JsonDeserialize<IDictionary<string, string>>(this._profile);
            //TODO:需要考虑反序列化意外出错情况
        }
    }
}