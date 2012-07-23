//Copyright (c) CodeSharp.  All rights reserved. - http://www.icodesharp.com/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using CodeSharp.Core.DomainBase;

namespace Cooper.Model.Accounts
{
    /// <summary>用户帐号
    /// <remarks>
    /// 由于账号体系支持通过外部账号连接建立
    /// 因而可无需强制账号密码
    /// </remarks>
    /// </summary>
    public class Account : EntityBase<int>, IAggregateRoot
    {
        private string _password { get; set; }
        //由于账号名称可变，临时记录上一个名称
        protected virtual internal string _oldName { get; set; }
        /// <summary>获取账号名称
        /// </summary>
        public virtual string Name { get; private set; }
        /// <summary>获取创建时间
        /// </summary>
        public virtual DateTime CreateTime { get; private set; }
        /// <summary>获取账号设置
        /// 目前不公开使用
        /// </summary>
        private Profile _profile { get; set; }

        protected Account()
        {
            this.CreateTime = DateTime.Now;
        }
        //由于可以创建随机账号，可无需密码
        public Account(string name) : this() { this.SetName(name); }
        public Account(string name, string password) : this(name) { this.SetPassword(password); }

        /// <summary>设置账号名称
        /// </summary>
        /// <param name="name"></param>
        public virtual void SetName(string name)
        {
            Assert.IsNotNullOrWhiteSpace(name);
            Assert.LessOrEqual(name.Length, 255);
            Assert.AreNotEqual(this.Name, name);
            this._oldName = this.Name;
            this.Name = name;
        }
        /// <summary>设置密码
        /// </summary>
        /// <param name="password"></param>
        public virtual void SetPassword(string password)
        {
            Assert.IsNotNullOrWhiteSpace(password);
            Assert.LessOrEqual(password.Length, 255);
            //TODO:密码格式要求
            this._password = password;
        }

        /// <summary>获取指定的个人设置
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        public virtual string GetProfile(string key)
        {
            return this._profile == null ? null : this._profile[key];
        }
        /// <summary>设置个人设置
        /// </summary>
        /// <param name="key"></param>
        /// <param name="value"></param>
        public virtual void SetProfile(string key, string value)
        {
            if (this._profile == null)
                this._profile = new Profile(this);
            this._profile[key] = value;
        }

        /// <summary>检查密码正确性
        /// </summary>
        /// <param name="password"></param>
        /// <returns></returns>
        public virtual bool CheckPassword(string password)
        {
            return string.IsNullOrWhiteSpace(this._password) ? false : this._password == password;
        }
    }
}