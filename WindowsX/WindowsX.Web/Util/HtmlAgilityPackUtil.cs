using System;
using System.Collections.Generic;
using System.Text;

namespace WindowsX.Web.Util
{
    public class HtmlAgilityPackUtil
    {
        public static HtmlAgilityPack.HtmlNodeCollection GetTagList(string html, string tagName)
        {
            HtmlAgilityPack.HtmlDocument doc = new HtmlAgilityPack.HtmlDocument();
            try
            {
                doc.LoadHtml(html);
                return doc.DocumentNode.SelectNodes("//" + tagName);
            }
            catch
            {
                return null;
            }
        }

        public static HtmlAgilityPack.HtmlNodeCollection XPathQuery(string html, string pathExpression)
        {
            HtmlAgilityPack.HtmlDocument doc = new HtmlAgilityPack.HtmlDocument();
            try
            {
                doc.LoadHtml(html);
                return doc.DocumentNode.SelectNodes(pathExpression);
            }
            catch
            {
                return null;
            }
        }

        public static HtmlAgilityPack.HtmlNode XPathQuerySingle(string html, string pathExpression)
        {
            HtmlAgilityPack.HtmlDocument doc = new HtmlAgilityPack.HtmlDocument();
            try
            {
                doc.LoadHtml(html);
                return doc.DocumentNode.SelectSingleNode(pathExpression);
            }
            catch
            {
                return null;
            }
        }
    }
}
