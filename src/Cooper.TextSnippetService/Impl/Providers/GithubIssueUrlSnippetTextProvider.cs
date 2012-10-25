//Copyright (c) CodeSharp.  All rights reserved. - http://www.icodesharp.com/

using System.Net;
using System.Text;
using CodeSharp.Core;

namespace Cooper.TextSnippetService
{
    /// <summary>
    /// 解析github的issue的json中的issue的描述内容
    /// </summary>
    [Component]
    public class GithubIssueUrlSnippetTextProvider : IUrlSnippetTextProvider
    {
        public string GetSnippetText(string url)
        {
            WebClient client = new WebClient();
            client.Encoding = Encoding.UTF8;
            var json = client.DownloadString(url);
            return ParseJson(json);
        }

        private string ParseJson(string json)
        {
            //TODO
            return null;
        }
    }
}
