//Copyright (c) CodeSharp.  All rights reserved. - http://www.icodesharp.com/

using System;
using CodeSharp.Core.DomainBase;

namespace Cooper.Model.Tasks
{
    /// <summary>任务核心模型，仅描述任务自身基本属性和行为
    /// </summary>
    public abstract class Task : EntityBase<long>, IAggregateRoot
    {
        /// <summary>获取标题/主题
        /// </summary>
        public virtual string Subject { get; private set; }
        /// <summary>获取内容/描述
        /// </summary>
        public virtual string Body { get; private set; }
        /// <summary>获取基本优先级
        /// </summary>
        public virtual Priority Priority { get; private set; }
        /// <summary>获取截止时间
        /// </summary>
        public virtual DateTime? DueTime { get; private set; }
        /// <summary>获取是否完成
        /// </summary>
        public virtual bool IsCompleted { get; private set; }
        /// <summary>获取创建时间
        /// </summary>
        public virtual DateTime CreateTime { get; private set; }
        /// <summary>获取最后更新时间
        /// </summary>
        public virtual DateTime LastUpdateTime { get; private set; }

        protected Task() { this.CreateTime = this.LastUpdateTime = DateTime.Now; }

        //HACK:核心模型严格约束，避免未知重构问题

        /// <summary>标记为完成
        /// </summary>
        public virtual void MarkAsCompleted()
        {
            if (this.IsCompleted) return;
            this.IsCompleted = true;
            this.MakeChange();
        }
        /// <summary>标记为未完成
        /// </summary>
        public virtual void MarkAsInCompleted()
        {
            if (!this.IsCompleted) return;
            this.IsCompleted = false;
            this.MakeChange();
        }
        /// <summary>设置标题
        /// <remarks>长度应小于500</remarks>
        /// </summary>
        /// <param name="subject"></param>
        public virtual void SetSubject(string subject)
        {
            if (subject != null)
                Assert.LessOrEqual(subject.Length, 500);
            if (this.Subject == subject) return;
            this.Subject = subject;
            this.MakeChange();
        }
        /// <summary>设置内容
        /// <remarks>长度应小于5000</remarks>
        /// </summary>
        /// <param name="body"></param>
        public virtual void SetBody(string body)
        {
            if (body != null)
                Assert.LessOrEqual(body.Length, 5000);
            if (this.Body == body) return;
            this.Body = body;
            this.MakeChange();
        }
        /// <summary>设置优先级
        /// </summary>
        /// <param name="priority"></param>
        public virtual void SetPriority(Priority priority)
        {
            if (this.Priority == priority) return;
            this.Priority = priority;
            this.MakeChange();
        }
        /// <summary>设置截止日期
        /// </summary>
        /// <param name="dueTime"></param>
        public virtual void SetDueTime(DateTime? dueTime)
        {
            this.DueTime = dueTime;
            this.MakeChange();
        }
        //UNDONE:允许外部直接更改lastUpdateTime可能对后期业务设计带来隐患
        /// <summary>设置最后更新时间
        /// </summary>
        public virtual void SetLastUpdateTime(DateTime lastUpdateTime)
        {
            this.LastUpdateTime = lastUpdateTime;
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