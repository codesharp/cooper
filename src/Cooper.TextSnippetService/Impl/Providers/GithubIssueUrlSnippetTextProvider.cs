//Copyright (c) CodeSharp.  All rights reserved. - http://www.icodesharp.com/

using System;
using System.Linq;
using System.Net;
using System.Text;
using Majestic13;

namespace Cooper.TextSnippetService
{
    /// <summary>
    /// 解析github的issue的json中的issue的描述内容
    /// </summary>
    public class GithubIssueUrlSnippetTextProvider : IUrlSnippetTextProvider
    {
        public string GetSnippetText(string url)
        {
            WebClient client = new WebClient();
            client.Encoding = Encoding.UTF8;
            var html = client.DownloadString(url);
            return ParseHtml(html);
        }

        /// <summary>
        /// 分析github的issue，返回issue的title和body
        /// </summary>
        /// <param name="html"></param>
        /// <returns></returns>
        private string ParseHtml(string html)
        {
            var parser = new HtmlParser();
            var node = parser.Parse(html);

            //get issue title
            var visitor = new FindTagsVisitor(x => x.Name == "h2" && x.Attributes.ContainsKey("class") && x.Attributes["class"] == "discussion-topic-title");
            node.AcceptVisitor(visitor);

            var issueTitle = string.Empty;
            if (visitor.Result != null && visitor.Result.Count > 0)
            {
                var textNode = visitor.Result.First().Children.FirstOrDefault() as HtmlNode.Text;
                if (textNode != null)
                {
                    issueTitle = textNode.Value;
                }
            }

            //get issue body
            visitor = new FindTagsVisitor(x => x.Name == "div" && x.Attributes.ContainsKey("class") && x.Attributes["class"] == "js-comment-body comment-body markdown-body markdown-format");
            node.AcceptVisitor(visitor);

            var issueBody = string.Empty;
            if (visitor.Result != null && visitor.Result.Count > 0)
            {
                var childTag = visitor.Result.First().Children.FirstOrDefault(x => x is HtmlNode.Tag && ((HtmlNode.Tag)x).Name == "p") as HtmlNode.Tag;
                if (childTag != null)
                {
                    var textNode = childTag.Children.FirstOrDefault(x => x is HtmlNode.Text) as HtmlNode.Text;
                    if (textNode != null)
                    {
                        issueBody = textNode.Value;
                    }
                }
            }

            return issueTitle + Environment.NewLine + issueBody;
        }
    }
}
