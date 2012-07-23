//Copyright (c) CodeSharp.  All rights reserved. - http://www.icodesharp.com/

using System;
using CodeSharp.Core.DomainBase;

namespace Cooper.Model.Accounts
{
    /// <summary>描述外部帐号连接
    /// </summary>
    public abstract class AccountConnection : EntityBase<int>, IAggregateRoot
    {
        /// <summary>获取账号名
        /// </summary>
        public virtual string Name { get; protected set; }
        /// <summary>获取关联的系统内账号标识
        /// </summary>
        public virtual int AccountId { get; private set; }
        /// <summary>获取创建时间
        /// </summary>
        public virtual DateTime CreateTime { get; private set; }
        /// <summary>获取外部账号令牌信息
        /// </summary>
        public virtual string Token { get; private set; }

        protected AccountConnection() { this.CreateTime = DateTime.Now; }
        protected AccountConnection(string name, string token, Account account)
            : this()
        {
            Assert.IsNotNullOrWhiteSpace(name);
            Assert.LessOrEqual(name.Length, 255);
            this.Name = name;

            this.SetToken(token);

            Assert.IsValid(account);
            this.AccountId = account.ID;
        }

        /// <summary>设置令牌信息
        /// </summary>
        /// <param name="token"></param>
        public virtual void SetToken(string token)
        {
            Assert.IsNotNullOrWhiteSpace(token);
            Assert.LessOrEqual(token.Length, 1000);
            this.Token = token;
        }
    }
}