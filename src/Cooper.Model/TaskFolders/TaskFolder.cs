//Copyright (c) CodeSharp.  All rights reserved. - http://www.icodesharp.com/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using CodeSharp.Core.DomainBase;
using CodeSharp.Core.Utils;

namespace Cooper.Model.Tasks
{
    /// <summary>任务表
    /// <remarks>
    /// 任务的最基本组织单位，但不是所有任务都必须对应一个任务表
    /// </remarks>
    /// </summary>
    public abstract class TaskFolder : EntityBase<int>, IAggregateRoot
    {
        private ExtensionDictionary _extensionDictionary = new ExtensionDictionary();

        /// <summary>获取任务表名称
        /// </summary>
        public string Name { get; private set; }
        /// <summary>获取创建时间
        /// </summary>
        public DateTime CreateTime { get; private set; }

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

        protected TaskFolder() { this.CreateTime = DateTime.Now; }
        protected TaskFolder(string name)
            : this()
        {
            this.SetName(name);
        }

        /// <summary>设置任务表名称
        /// </summary>
        /// <param name="name"></param>
        public void SetName(string name)
        {
            Assert.IsNotNull(name);
            Assert.LessOrEqual(name.Length, 50);
            this.Name = name;
        }
    }
}