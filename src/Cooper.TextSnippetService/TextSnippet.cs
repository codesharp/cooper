using System;

namespace Cooper.TextSnippetService
{
    /// <summary>
    /// 表示一条文本缩略信息
    /// </summary>
    public class TextSnippet
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
        public TextSnippetType Type { get; set; }
        /// <summary>
        /// 文本缩略信息
        /// </summary>
        public string SnippetText { get; set; }
    }
    public enum TextSnippetType
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
