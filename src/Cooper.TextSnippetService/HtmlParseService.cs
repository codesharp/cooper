using System.Net;
using System.Text;
using Majestic13;

namespace Cooper.TextSnippetService
{
    /// <summary>
    /// 提供提取Url页面或给定html中的文本信息的服务
    /// </summary>
    public interface IHtmlParseService
    {
        /// <summary>
        /// 提取指定html中的文本信息
        /// </summary>
        /// <param name="html"></param>
        /// <returns></returns>
        string ParseHtml(string html);
        /// <summary>
        /// 提取指定url连接对应页面中的文本信息
        /// </summary>
        /// <param name="url"></param>
        /// <returns></returns>
        string ParseUrl(string url);
    }
    /// <summary>
    /// 利用Majestic13组件实现Html的解析
    /// </summary>
    public class HtmlParseService : IHtmlParseService
    {
        public string ParseHtml(string html)
        {
            var parser = new HtmlParser();
            var node = parser.Parse(html);
            var stringBuilder = new StringBuilder();
            VisitHtmlNode(node, stringBuilder);
            return stringBuilder.ToString();
        }
        public string ParseUrl(string url)
        {
            WebClient client = new WebClient();
            client.Encoding = Encoding.UTF8;
            var html = client.DownloadString(url);
            return ParseHtml(html);
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
