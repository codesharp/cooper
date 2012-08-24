//Copyright (c) CodeSharp.  All rights reserved. - http://www.icodesharp.com/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Cooper.Model.Accounts;
using CodeSharp.Core.DomainBase;

namespace Cooper.Model.Teams
{
    /// <summary>团队任务评论模型
    /// <remarks>
    /// 评论只能新增和删除，不能更改
    /// </remarks>
    /// </summary>
    public class Comment : EntityBase<long>
    {
        /// <summary>获取评论作者的账号
        /// </summary>
        public Account Creator { get; private set; }
        /// <summary>获取评论内容
        /// </summary>
        public string Body { get; private set; }
        /// <summary>获取创建时间
        /// </summary>
        public DateTime CreateTime { get; private set; }

        protected Comment() { this.CreateTime = DateTime.Now; }
        public Comment(Account creator, string body) : this()
        {
            Assert.IsValid(creator);
            this.Creator = creator;
            this.Body = body;
        }
    }
}
