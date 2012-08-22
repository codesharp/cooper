﻿//Copyright (c) CodeSharp.  All rights reserved. - http://www.icodesharp.com/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using CodeSharp.Core.DomainBase;
using CodeSharp.Core.Utils;

namespace Cooper.Model.Teams
{
    /// <summary>项目模型
    /// </summary>
    public class Project : EntityBase<int>
    {
        private static readonly Serializer _serializer = new Serializer();
        private string _extensions { get; set; }
        private IDictionary<string, string> _dict;

        /// <summary>获取团队项目名称
        /// </summary>
        public string Name { get; private set; }
        /// <summary>获取团队项目是否公开
        /// <remarks>
        /// 公开的项目可以被非团队成员浏览
        /// </remarks>
        /// </summary>
        public bool IsPublic { get; private set; }
        /// <summary>获取所属团队的标识
        /// </summary>
        public int TeamId { get; private set; }
        /// <summary>创建时间
        /// </summary>
        public DateTime CreateTime { get; private set; }
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

        protected Project() { this.CreateTime = DateTime.Now; }
        internal Project(string name, Team team) : this()
        {
            Assert.IsValid(team);
            this.SetName(name);
            this.SetIsPublic(false); //默认False
            this.TeamId = team.ID;
        }
        /// <summary>获取所有项目设置信息
        /// </summary>
        /// <returns></returns>
        public IDictionary<string, string> GetExtensions()
        {
            if (this._dict == null)
                this._dict = this.Parse();
            return this._dict;
        }
        /// <summary>设置名称
        /// <remarks>
        /// 长度应小于255
        /// </remarks>
        /// </summary>
        public void SetName(string name)
        {
            Assert.IsValidKey(name);

            if (this.Name != name)
            {
                this.Name = name;
            }
        }
        /// <summary>设置是否公开
        /// </summary>
        public void SetIsPublic(bool isPublic)
        {
            this.IsPublic = isPublic;
        }

        private IDictionary<string, string> Parse()
        {
            return string.IsNullOrWhiteSpace(this._extensions)
                ? new Dictionary<string, string>()
                : _serializer.JsonDeserialize<IDictionary<string, string>>(this._extensions);
        }
    }
}
