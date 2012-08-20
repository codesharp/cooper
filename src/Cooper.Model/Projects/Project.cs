//Copyright (c) CodeSharp.  All rights reserved. - http://www.icodesharp.com/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using CodeSharp.Core.DomainBase;
using CodeSharp.Core.Utils;

namespace Cooper.Model.Teams
{
    /// <summary>项目模型
    /// <remarks>
    /// 项目在目前的Cooper模型中，完全由团队模型管理，项目的概念只存在于团队上下文。
    /// 而目前仍然将项目设计为独立聚合根，是因为项目会直接和任务关联；
    /// 考虑到聚合根不能引用其他聚合根内的实体，所以将项目设计为聚合根；
    /// </remarks>
    /// </summary>
    public class Project : EntityBase<int>, IAggregateRoot
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
        public Project(string name, bool isPublic, Team team) : this()
        {
            Assert.IsValid(team);
            this.SetName(name);
            this.SetIsPublic(isPublic);
            this.TeamId = team.ID;
        }

        /// <summary>设置名称
        /// <remarks>
        /// 长度应小于100
        /// </remarks>
        /// </summary>
        public void SetName(string name)
        {
            Assert.IsNotNullOrWhiteSpace(name);
            Assert.LessOrEqual(name.Length, 100);

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
