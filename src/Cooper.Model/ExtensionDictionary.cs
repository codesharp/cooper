//Copyright (c) CodeSharp.  All rights reserved. - http://www.icodesharp.com/

using System.Collections.Generic;
using CodeSharp.Core.DomainBase;
using CodeSharp.Core.Utils;

namespace Cooper.Model
{
    /// <summary>提供支持基于字典的扩展功能的Component
    /// </summary>
    public class ExtensionDictionary
    {
        private static readonly Serializer _serializer = new Serializer();
        private string _extensions { get; set; }
        private IDictionary<string, string> _dict;

        /// <summary>根据键获取对应设置
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        public string this[string key]
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
                this._extensions = _serializer.JsonSerialize(this._dict);
            }
        }

        /// <summary>获取所有扩展信息字典
        /// </summary>
        /// <returns></returns>
        public IDictionary<string, string> GetExtensions()
        {
            if (this._dict == null)
                this._dict = this.Parse();
            return this._dict;
        }

        private IDictionary<string, string> Parse()
        {
            return (string.IsNullOrWhiteSpace(this._extensions)
                    ? new Dictionary<string, string>() : _serializer.JsonDeserialize<IDictionary<string, string>>(this._extensions)
                   ) ?? new Dictionary<string, string>();
        }
    }
}
