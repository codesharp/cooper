//Copyright (c) CodeSharp.  All rights reserved. - http://www.icodesharp.com/

using System;
using CodeSharp.Core.DomainBase;

namespace Cooper.Model.Contacts
{
    /// <summary>通讯簿模型
    /// <remarks>
    /// 表示一个通用的通讯簿，支持父子通讯簿关系
    /// </remarks>
    /// </summary>
    public abstract class AddressBook : EntityBase<int>, IAggregateRoot
    {
        protected AddressBook() { this.CreateTime = DateTime.Now; }
        protected AddressBook(string name) : this()
        {
            this.SetName(name);
        }

        /// <summary>通讯簿名称
        /// <remarks>
        /// 所有通讯簿名称不能重名
        /// </remarks>
        /// </summary>
        public string Name { get; private set; }
        /// <summary>父通讯簿
        /// </summary>
        public AddressBook Parent { get; private set; }
        /// <summary>创建时间
        /// </summary>
        public DateTime CreateTime { get; private set; }

        /// <summary>设置通讯簿名称
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
        /// <summary>设置父通讯簿
        /// <remarks>
        /// 父通讯簿不能为自己，也不能将其某个子的通讯簿设置为其父通讯簿
        /// </remarks>
        /// </summary>
        public void SetParent(AddressBook parent)
        {
            Assert.IsNotNull(parent);
            Assert.AreNotEqual(this.ID, parent.ID);

            //判断不能将其某个子的通讯簿设置为其父通讯簿
            var parentParent = parent.Parent;
            while (parentParent != null)
            {
                Assert.AreNotEqual(this.ID, parentParent.ID);
                parentParent = parentParent.Parent;
            }

            this.Parent = parent;
        }
    }
}