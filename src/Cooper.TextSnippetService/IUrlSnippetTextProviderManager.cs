//Copyright (c) CodeSharp.  All rights reserved. - http://www.icodesharp.com/

using System;

namespace Cooper.TextSnippetService
{
    public interface IUrlSnippetTextProviderManager
    {
        /// <summary>
        /// 注册URL类型及其对应的URL纯文本解析Provider
        /// </summary>
        /// <param name="urlType"></param>
        /// <param name="provider"></param>
        void RegisterProvider(UrlType urlType, IUrlSnippetTextProvider provider);
        /// <summary>
        /// 根据指定的URL类型返回一个适当的IUrlSnippetTextProvider
        /// </summary>
        /// <param name="urlType">URL类型</param>
        /// <returns></returns>
        IUrlSnippetTextProvider GetProvider(UrlType urlType);
    }
}
