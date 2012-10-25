//Copyright (c) CodeSharp.  All rights reserved. - http://www.icodesharp.com/

using System;

namespace Cooper.TextSnippetService
{
    /// <summary>
    /// 提供提取文本缩略信息，以及缓存文本缩略信息的服务接口
    /// </summary>
    public interface ISnippetTextService
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
        string GetHtmlSnippetText(string url);
        /// <summary>
        /// 获取EverNote笔记的文本缩略信息
        /// <remarks>
        /// 首先从本地缓存中查找是否有缓存，如果有则直接返回；
        /// 如果没有，则调用EventNote API获取文本缩略信息，获取过来后自动缓存在数据库；
        /// 最后返回文本缩略信息；
        /// </remarks>
        /// </summary>
        /// <param name="authToken"></param>
        /// <param name="noteId"></param>
        /// <returns></returns>
        string GetEverNoteSnippetText(string authToken, string noteId);
        /// <summary>
        /// 清除指定id的文本缩略信息数据库缓存
        /// </summary>
        /// <param name="id"></param>
        void ClearSnippetText(long id);
    }
}
