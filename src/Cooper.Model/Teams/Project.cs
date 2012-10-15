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
    /// </summary>
    public class Project : EntityBase<int>
    {
        private ExtensionDictionary _settings;

        /// <summary>获取团队项目名称
        /// </summary>
        public string Name { get; private set; }
        /// <summary>获取团队项目描述
        /// </summary>
        public string Description { get; private set; }
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

        protected Project() { this.CreateTime = DateTime.Now; }
        internal Project(string name, Team team) : this()
        {
            Assert.IsValid(team);
            this.SetName(name);
            this.SetIsPublic(false); //默认False
            this.TeamId = team.ID;
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
        /// <summary>设置描述
        /// <remarks>
        /// 长度应小于2000
        /// </remarks>
        /// </summary>
        public void SetDescription(string description)
        {
            if (!string.IsNullOrEmpty(description))
            {
                Assert.LessOrEqual(description.Length, 2000);
            }

            if (this.Description != description)
            {
                this.Description = description;
            }
        }
        /// <summary>设置是否公开
        /// </summary>
        public void SetIsPublic(bool isPublic)
        {
            this.IsPublic = isPublic;
        }
    }
    /// <summary>值对象，显示定义Project实体的唯一标识
    /// <remarks>
    /// Team聚合根之外的其他聚合根如果要关联Project，则可以关联此值对象；
    /// 如果直接关联Project实体，会导致Project实体被其他外部聚合根持有引用，
    /// 这可能会导致Project在Team聚合之外被意外修改。因此，如果外部聚合根只关联Project的ID，
    /// 就不会有这个问题，也就没有违反DDD的原则了。
    /// </remarks>
    /// </summary>
    public class ProjectId : EntityBase<int>
    {
        protected ProjectId() { }
        public ProjectId(int id) { this.ID = id; }
    }
}
