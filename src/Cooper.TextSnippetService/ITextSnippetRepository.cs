using System;

namespace Cooper.TextSnippetService
{
    /// <summary>
    /// 存储文本缩略信息的仓储
    /// </summary>
    public interface ITextSnippetRepository
    {
        /// <summary>
        /// 根据类型和key返回一个唯一的文本缩略信息
        /// </summary>
        /// <param name="type"></param>
        /// <param name="key"></param>
        /// <returns></returns>
        TextSnippet GetTextSnippet(TextSnippetType type, string key);
        /// <summary>
        /// 新增一个TextSnippet
        /// </summary>
        /// <param name="type"></param>
        /// <param name="key"></param>
        /// <param name="snippet"></param>
        void AddTextSnippet(TextSnippetType type, string key, string snippet);
        /// <summary>
        /// 移除一个指定的文本缩略信息
        /// </summary>
        /// <param name="id"></param>
        void RemoveTextSnippet(long id);
    }
}
