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
        /// <summary>获取创建该评论的团队成员
        /// </summary>
        public Member Creator { get; private set; }
        /// <summary>获取评论内容
        /// </summary>
        public string Body { get; private set; }
        /// <summary>获取创建时间
        /// </summary>
        public DateTime CreateTime { get; private set; }

        protected Comment() { this.CreateTime = DateTime.Now; }
        public Comment(Member creator, string body) : this()
        {
            Assert.IsNotNullOrWhiteSpace(body);
            Assert.IsValid(creator);
            this.Body = body;
            this.Creator = creator;
        }

        /// <summary>
        /// 将Comment的创建者清空
        /// <remarks>
        /// 当团队成员被从团队中删除时，需要将该团队成员的Comments的Creator信息清空，表示该Comment的作者已经不存在
        /// </remarks>
        /// </summary>
        internal void SetCreatorAsNull()
        {
            this.Creator = null;
        }
    }
}
