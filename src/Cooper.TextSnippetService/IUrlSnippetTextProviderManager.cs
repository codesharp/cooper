//Copyright (c) CodeSharp.  All rights reserved. - http://www.icodesharp.com/

using System;
using System.Text.RegularExpressions;

namespace Cooper.TextSnippetService
{
    public interface IUrlSnippetTextProviderManager
    {
        /// <summary>
        /// 注册URL类型及其对应的URL纯文本解析Provider
        /// </summary>
        /// <param name="providerKey"></param>
        /// <param name="provider"></param>
        void RegisterProvider(UrlSnippetTextProviderKey providerKey, IUrlSnippetTextProvider provider);
        /// <summary>
        /// 根据指定的url返回一个适当的IUrlSnippetTextProvider
        /// </summary>
        /// <param name="url"></param>
        /// <returns></returns>
        IUrlSnippetTextProvider GetProvider(string url);
    }

    public class UrlSnippetTextProviderKey
    {
        public string UrlRegexPattern { get; set; }

        public bool IsUrlMatch(string url)
        {
            if (!string.IsNullOrEmpty(UrlRegexPattern))
            {
                return new Regex(UrlRegexPattern).IsMatch(url);
            }
            return false;
        }
    }
}
