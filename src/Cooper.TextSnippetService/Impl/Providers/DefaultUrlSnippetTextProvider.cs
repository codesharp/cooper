//Copyright (c) CodeSharp.  All rights reserved. - http://www.icodesharp.com/

using System.Net;
using System.Text;
using CodeSharp.Core;
using Majestic13;

namespace Cooper.TextSnippetService
{
    /// <summary>
    /// 利用Majestic13组件实现对Html的默认纯文本解析
    /// </summary>
    public class DefaultUrlSnippetTextProvider : IUrlSnippetTextProvider
    {
        public string GetSnippetText(string url)
        {
            WebClient client = new WebClient();
            client.Encoding = Encoding.UTF8;
            var html = client.DownloadString(url);
            return ParseHtml(html);
        }

        private string ParseHtml(string html)
        {
            var parser = new HtmlParser();
            var node = parser.Parse(html);
            var stringBuilder = new StringBuilder();
            VisitHtmlNode(node, stringBuilder);
            return stringBuilder.ToString();
        }
        private void VisitHtmlNode(HtmlNode htmlNode, StringBuilder stringBuilder)
        {
            if (htmlNode is HtmlNode.Text)
            {
                var text = (htmlNode as HtmlNode.Text).Value;
                if (string.IsNullOrWhiteSpace(text))
                {
                    return;
                }
                stringBuilder.Append(text.Trim());
                stringBuilder.Append(" ");
            }
            else if (htmlNode is HtmlNode.Tag)
            {
                foreach (var child in (htmlNode as HtmlNode.Tag).Children)
                {
                    VisitHtmlNode(child, stringBuilder);
                }
            }
        }
    }
}
