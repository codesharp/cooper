using System;

namespace Cooper.TextSnippetService
{
    /// <summary>
    /// 提供提取文本缩略信息，以及缓存文本缩略信息的服务接口
    /// </summary>
    public interface ITextSnippetService
    {
        /// <summary>
        /// 根据url返回该页面的文本缩略信息
        /// <remarks>
        /// 首先从本地缓存中查找是否有缓存，如果有则直接返回；
        /// 如果没有，则抓取页面信息生成文本缩略信息，然后自动缓存在数据库；
        /// 最后返回文本缩略信息；
        /// </remarks>
        /// </summary>
        /// <param name="url"></param>
        /// <returns></returns>
        string GetSnippetText(string url);
        /// <summary>
        /// 根据类型和key返回对应的文本缩略信息
        /// <remarks>
        /// 首先从本地缓存中查找是否有缓存，如果有则直接返回；
        /// 如果没有，则从外部抓取文本缩略信息，抓取过来后自动缓存在数据库；
        /// 最后返回文本缩略信息；
        /// </remarks>
        /// </summary>
        /// <param name="type"></param>
        /// <param name="key"></param>
        /// <returns></returns>
        string GetSnippetText(TextSnippetType type, string key);
        /// <summary>
        /// 清除指定id的文本缩略信息数据库缓存
        /// </summary>
        /// <param name="id"></param>
        void ClearSnippetText(long id);
    }
    /// <summary>
    /// 提供提取文本缩略信息，以及缓存文本缩略信息的服务默认实现类
    /// </summary>
    public class TextSnippetService : ITextSnippetService
    {
        private IHtmlParseService _htmlParseService;
        private IEverNoteService _everNoteService;
        private ITextSnippetRepository _textSnippetRepository;

        public TextSnippetService(IHtmlParseService htmlParseService, IEverNoteService everNoteService, ITextSnippetRepository textSnippetRepository)
        {
            _htmlParseService = htmlParseService;
            _everNoteService = everNoteService;
            _textSnippetRepository = textSnippetRepository;
        }

        public string GetSnippetText(string url)
        {
            return GetSnippetText(TextSnippetType.Html, url);
        }
        public string GetSnippetText(TextSnippetType type, string key)
        {
            var textSnippet = _textSnippetRepository.GetTextSnippet(type, key);
            if (textSnippet != null)
            {
                return textSnippet.SnippetText;
            }

            var snippetText = _htmlParseService.ParseUrl(key);
            _textSnippetRepository.AddTextSnippet(type, key, snippetText);
            return snippetText;
        }
        public void ClearSnippetText(long id)
        {
            _textSnippetRepository.RemoveTextSnippet(id);
        }
    }
}
