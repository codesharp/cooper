//Copyright (c) CodeSharp.  All rights reserved. - http://www.icodesharp.com/

using System;
using System.Collections.Generic;
using System.Linq;
using CodeSharp.Core.DomainBase;

namespace Cooper.Model.Tasks
{
    /// <summary>任务核心模型，仅描述任务自身基本属性和行为
    /// </summary>
    public abstract class Task : EntityBase<long>, IAggregateRoot
    {
        private StringList _tagList { get; set; }

        /// <summary>获取标题/主题
        /// </summary>
        public string Subject { get; private set; }
        /// <summary>获取内容/描述
        /// </summary>
        public string Body { get; private set; }
        /// <summary>获取基本优先级
        /// </summary>
        public Priority Priority { get; private set; }
        /// <summary>获取截止时间
        /// </summary>
        public DateTime? DueTime { get; private set; }
        /// <summary>获取是否完成
        /// </summary>
        public bool IsCompleted { get; private set; }
        /// <summary>Tags
        /// </summary>
        public IEnumerable<string> Tags
        {
            get
            {
                if (this._tagList == null)
                    this._tagList = new StringList();
                return this._tagList.GetAllItems();
            }
        }
        /// <summary>获取创建时间
        /// </summary>
        public DateTime CreateTime { get; private set; }
        /// <summary>获取最后更新时间
        /// </summary>
        public DateTime LastUpdateTime { get; private set; }
        /// <summary>是否已废弃
        /// <remarks>
        /// 设计该属性的意图是为了在用户删除一个task时，不做直接物理删除，而是放入废纸篓；
        /// 这样以便于用户后面想恢复该task时可以恢复；
        /// 该字段就是用来标记当前任务是否已标记为逻辑删除，即是否已放入废纸篓；
        /// 目前只有任务支持废纸篓，将来会实现团队成员也支持废纸篓功能；
        /// 相关问题链接：
        /// https://github.com/codesharp/cooper/issues/88
        /// https://github.com/codesharp/cooper/issues/79
        /// </remarks>
        /// </summary>
        public bool IsTrashed { get; private set; }

        protected Task() { this.CreateTime = this.LastUpdateTime = DateTime.Now; }

        //HACK:核心模型严格约束，避免未知重构问题

        /// <summary>标记为完成
        /// </summary>
        public void MarkAsCompleted()
        {
            if (this.IsCompleted) return;
            this.IsCompleted = true;
            this.MakeChange();
        }
        /// <summary>标记为未完成
        /// </summary>
        public void MarkAsInCompleted()
        {
            if (!this.IsCompleted) return;
            this.IsCompleted = false;
            this.MakeChange();
        }
        /// <summary>设置标题
        /// <remarks>长度应小于500</remarks>
        /// </summary>
        /// <param name="subject"></param>
        public void SetSubject(string subject)
        {
            if (subject != null)
                Assert.LessOrEqual(subject.Length, 255);
            if (this.Subject == subject) return;
            this.Subject = subject;
            this.MakeChange();
        }
        /// <summary>设置内容
        /// <remarks>长度应小于5000</remarks>
        /// </summary>
        /// <param name="body"></param>
        public void SetBody(string body)
        {
            if (body != null)
                Assert.LessOrEqual(body.Length, 1000);
            if (this.Body == body) return;
            this.Body = body;
            this.MakeChange();
        }
        /// <summary>设置优先级
        /// </summary>
        /// <param name="priority"></param>
        public void SetPriority(Priority priority)
        {
            if (this.Priority == priority) return;
            this.Priority = priority;
            this.MakeChange();
        }
        /// <summary>设置截止日期
        /// </summary>
        /// <param name="dueTime"></param>
        public void SetDueTime(DateTime? dueTime)
        {
            this.DueTime = dueTime;
            this.MakeChange();
        }
        /// <summary>新增一个Tag
        /// </summary>
        /// <param name="tag"></param>
        public void AddTag(string tag)
        {
            Assert.IsNotNullOrWhiteSpace(tag);
            Assert.LessOrEqual(tag.Length, 50);
            if (this._tagList == null)
                this._tagList = new StringList();
            this._tagList.Add(tag);
            this.MakeChange();
        }
        /// <summary>移除一个Tag
        /// </summary>
        /// <param name="tag"></param>
        public void RemoveTag(string tag)
        {
            Assert.IsNotNullOrWhiteSpace(tag);
            Assert.LessOrEqual(tag.Length, 50);
            if (this._tagList == null)
                this._tagList = new StringList();
            this._tagList.Remove(tag);
            this.MakeChange();
        }
        /// <summary>标记为已废弃
        /// </summary>
        public void MarkAsTrashed()
        {
            if (this.IsTrashed) return;
            this.IsTrashed = true;
            this.MakeChange();
        }
        /// <summary>标记为未废弃
        /// </summary>
        public void MarkAsUnTrashed()
        {
            if (!this.IsTrashed) return;
            this.IsTrashed = false;
            this.MakeChange();
        }

        protected void MakeChange()
        {
            this.LastUpdateTime = DateTime.Now;
        }
    }
    /// <summary>任务基本优先级，描述大概过多久时间开始执行任务
    /// <remarks>与DueTime没有直接联系</remarks>
    /// </summary>
    public enum Priority
    {
        /// <summary>0 今天，马上
        /// </summary>
        Today = 0,
        /// <summary>1 尽快
        /// </summary>
        Upcoming = 1,
        /// <summary>2 迟些再说
        /// </summary>
        Later = 2
    }
}