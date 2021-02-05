using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Winista.Text.HtmlParser;
using Winista.Text.HtmlParser.Lex;
using Winista.Text.HtmlParser.Util;
using Winista.Text.HtmlParser.Tags;
using Winista.Text.HtmlParser.Filters;
using System.Text.RegularExpressions;

namespace TSE_Core
{
    public static class htmlParser
    {
        public static List<string> getUrlsFromContent(string content) {
            int x = content.IndexOf("</HEAD>");
            if (x == -1) x = content.IndexOf("</head>");

            if (x != -1) content = content.Substring(x);

            string hrefRegex = "href=[\\\"\\\'](http:\\/\\/|\\.\\/|\\/)?\\w+(\\.\\w+)*(\\/\\w+(\\.\\w+)?)*(\\/|\\?\\w*=\\w*(&\\w*=\\w*)*)?[\\\"\\\']";
            Regex re = new Regex(hrefRegex);
            MatchCollection matches = re.Matches(content);

            List<string> matchStrings = new List<string>();

            var urlRegex = "^((https|http|ftp|rtsp|mms)?://)" +
               "?(([0-9a-z_!~*'().&=+$%-]+: )?[0-9a-z_!~*'().&=+$%-]+@)?" + //ftp的user@ +
               "(([0-9]{1,3}\\.){3}[0-9]{1,3}" + // IP形式的URL- 199.194.52.184 +
               "|" + // 允许IP和DOMAIN（域名） +
               "([0-9a-z_!~*'()-]+\\.)*" + // 域名- www. +
               "([0-9a-z][0-9a-z-]{0,61})?[0-9a-z]\\." + // 二级域名 +
               "[a-z]{2,6})" + // first level domain- .com or .museum +
               "(:[0-9]{1,4})?" + // 端口- :80 +
               "((/?)|" + // a slash isn't required if there is no file name +
               "(/[0-9a-z_!~*'().;?:@&=+$,%#-]+)+/?)$";
            Regex re2 = new Regex(urlRegex);
            foreach (Match match in matches)
            {
                string link = match.Value.Substring(5).Trim('\"');
                if(re2.IsMatch(link) && link.IndexOf('/') != -1)
                    matchStrings.Add(link);
            }

            return matchStrings;
        }

        public static string GetTitleFromContent(string content)
        {
            content = DropComment(content);

            Lexer lexer = new Lexer(content);
            Parser parser = new Parser(lexer);
            NodeFilter filter = new TagNameFilter("TITLE");
            NodeList list = parser.ExtractAllNodesThatMatch(filter);

            return list.Count == 0 ? "" : list[0].ToPlainTextString();
        }

        public static string DropComment(string content) {
            return Regex.Replace(content, @"(?s)<!--.*?-->", "");
        }
        public static string Html2PlainText(string content) {

            content = DropComment(content);

            Lexer lexer = new Lexer(content);
            Parser parser = new Parser(lexer);
            NodeList htmlNodes = parser.Parse(null);
            String plainText = "";
            
            for (int i = 0; i < htmlNodes.Count; i++)
            {
                plainText += HtmlNode2PlainText(htmlNodes[i]);
            }
            return plainText;
        }

        private static string HtmlNode2PlainText(INode htmlNode)
        {
            if (htmlNode == null) return "";

            string text = "";

            if(htmlNode.Children == null || htmlNode.Children.Count == 0)
            {
                text = htmlNode.ToPlainTextString();
            }
            else
            {
                INode child = htmlNode.FirstChild;
                while (child != null)
                {
                    if (child is ITag)
                    {
                        ITag tag = child as ITag;
                        if (tag.TagName == "SCRIPT")
                        {
                            if(tag.GetEndTag() != null)
                                child = tag.GetEndTag().NextSibling;
                            else
                                child = child.NextSibling;
                            continue;
                        }
                    }

                    text += HtmlNode2PlainText(child);

                    child = child.NextSibling;
                }
            }

            return text;
        }
    }
}
