//Copyright (c) CodeSharp.  All rights reserved. - http://www.icodesharp.com/

using System;

namespace Cooper.TextSnippetService
{
    /// <summary>
    /// 存储文本缩略信息的仓储
    /// </summary>
    public interface ISnippetTextRepository
    {
        /// <summary>
        /// 根据类型和key返回一个唯一的SnippetText
        /// </summary>
        /// <param name="type"></param>
        /// <param name="key"></param>
        /// <returns></returns>
        SnippetText GetSnippetText(SnippetTextType type, string key);
        /// <summary>
        /// 新增一个SnippetText
        /// </summary>
        /// <param name="type"></param>
        /// <param name="key"></param>
        /// <param name="snippetText"></param>
        void AddSnippetText(SnippetTextType type, string key, string snippetText);
        /// <summary>
        /// 移除一个指定的SnippetText
        /// </summary>
        /// <param name="id"></param>
        void RemoveSnippetText(long id);
    }

    /// <summary>
    /// 表示一条文本缩略信息
    /// </summary>
    public class SnippetText
    {
        /// <summary>
        /// 唯一标识，自增的long型
        /// </summary>
        public long Id { get; set; }
        /// <summary>
        /// 一个Key，可以为一个URL，或者一个evernote笔记的ID，等等
        /// </summary>
        public string Key { get; set; }
        /// <summary>
        /// 文本缩略信息的类型，比如html, evernote
        /// </summary>
        public SnippetTextType Type { get; set; }
        /// <summary>
        /// 文本缩略信息
        /// </summary>
        public string Text { get; set; }
    }
    public enum SnippetTextType
    {
        /// <summary>
        /// Html页面的纯文本缩略信息
        /// </summary>
        Html,
        /// <summary>
        /// EverNote笔记的纯文本缩略信息
        /// </summary>
        EverNote
    }
}
