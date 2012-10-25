//Copyright (c) CodeSharp.  All rights reserved. - http://www.icodesharp.com/

using System;

namespace Cooper.TextSnippetService
{
    public interface IUrlTypeParseService
    {
        /// <summary>
        /// 分析给定的URL，返回一个类型，不同的URL类型会有不同的解析方式；
        /// 可以随时扩展新的URL类型以及解析方式；
        /// </summary>
        /// <param name="url"></param>
        /// <returns></returns>
        UrlType ParseUrlType(string url);
    }

    /// <summary>
    /// URL类型枚举
    /// </summary>
    public enum UrlType
    {
        /// <summary>
        /// 未知的URL，采用通用的URL内容抓取的方式来提取文本缩略信息
        /// </summary>
        UnKnown,
        /// <summary>
        /// URL指向一个Github的Issue
        /// </summary>
        GithubIssue
    }
}
