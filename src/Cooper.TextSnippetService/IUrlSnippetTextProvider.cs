//Copyright (c) CodeSharp.  All rights reserved. - http://www.icodesharp.com/

using System;

namespace Cooper.TextSnippetService
{
    public interface IUrlSnippetTextProvider
    {
        /// <summary>
        /// 返回指定URL对应页面的文本缩略信息
        /// </summary>
        /// <param name="url"></param>
        /// <returns></returns>
        string GetSnippetText(string url);
    }
}
