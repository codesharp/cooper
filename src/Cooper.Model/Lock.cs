//Copyright (c) CodeSharp.  All rights reserved. - http://www.codesharp.cn/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using CodeSharp.Core.DomainBase;

namespace Cooper.Model
{
    /// <summary>用于提供同步锁
    /// </summary>
    public class Lock : EntityBase<string>, IAggregateRoot
    {
        protected Lock() { }
        public Lock(string id) { this.ID = id; }
    }
    /// <summary>用于提供同步锁辅助
    /// <remarks>由于表锁和范围锁都有局限性，因此抽离出此设计</remarks>
    /// </summary>
    public interface ILockHelper
    {
        /// <summary>初始化指定类型的全局锁
        /// </summary>
        /// <typeparam name="T"></typeparam>
        void Init<T>();
        /// <summary>获得全局锁
        /// </summary>
        /// <typeparam name="T">指定类型</typeparam>
        /// <returns></returns>
        Lock Require<T>();
        /// <summary>获得全局锁
        /// </summary>
        /// <param name="type">指定类型</param>
        /// <returns></returns>
        Lock Require(Type type);
    }
}
