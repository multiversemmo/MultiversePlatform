/********************************************************************

The Multiverse Platform is made available under the MIT License.

Copyright (c) 2012 The Multiverse Foundation

Permission is hereby granted, free of charge, to any person 
obtaining a copy of this software and associated documentation 
files (the "Software"), to deal in the Software without restriction, 
including without limitation the rights to use, copy, modify, 
merge, publish, distribute, sublicense, and/or sell copies 
of the Software, and to permit persons to whom the Software 
is furnished to do so, subject to the following conditions:

The above copyright notice and this permission notice shall be 
included in all copies or substantial portions of the Software.

THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, 
EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES 
OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND 
NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT 
HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, 
WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING 
FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE 
OR OTHER DEALINGS IN THE SOFTWARE.

*********************************************************************/

using HtmlAgilityPack;
using System;
using System.IO;
using System.Xml;
using System.Text.RegularExpressions;
namespace WidgetApiDownloader
{
    class Program
    {
        static string currentTable = String.Empty;

        static void Main(string[] args)
        {
            HtmlWeb web = new HtmlWeb();
            HtmlDocument doc = web.Load("http://www.wowwiki.com/Widget_API");

            HtmlNodeCollection nc = doc.DocumentNode.SelectNodes("//div[@id='bodyContent']//dl/dd/a");
            if (nc == null)
                return;

            XmlWriterSettings settings = new XmlWriterSettings();
            settings.Indent = true;

            using (StreamWriter sw = new StreamWriter("output.xml"))
            using (XmlWriter xw = XmlWriter.Create(sw, settings))
            {
                xw.WriteStartDocument();
                xw.WriteStartElement("doc");
                xw.WriteStartElement("tables");

                foreach (HtmlNode node in nc)
                {

                    ProcessLink(node, xw);
                    Console.WriteLine(node.InnerHtml);
                }

                xw.WriteEndElement();
                xw.WriteEndElement();
                xw.WriteEndDocument();
            }
        }

        private static void ProcessLink(HtmlNode node, XmlWriter xw)
        {
            string title = node.GetAttributeValue("title", null);
            if (String.IsNullOrEmpty(title))
                return;

            HtmlAttribute attr = node.Attributes["title"];

            if (!title.StartsWith("API ", StringComparison.Ordinal))
                return;

            int colonIndex = node.InnerHtml.IndexOf(':');

            if (colonIndex == -1)
                return;

            string tableName = node.InnerHtml.Substring(0, colonIndex);

            if (tableName != currentTable)
            {
                if (!String.IsNullOrEmpty(currentTable))
                    xw.WriteEndElement();

                xw.WriteStartElement("table");
                xw.WriteAttributeString("name", tableName);

                currentTable = tableName;
            }

            string functionName = node.InnerHtml.Substring(colonIndex + 1);

            xw.WriteStartElement("function");
            xw.WriteAttributeString("name", functionName);

            WriteParamsAndSummary(node, xw);

            xw.WriteEndElement();
        }

        static Regex cleanerRegex = new Regex(@"(^\s+)|(\s+$)|([\(|\)""\[\],])+", RegexOptions.Compiled | RegexOptions.CultureInvariant);

        private static void WriteParamsAndSummary(HtmlNode node, XmlWriter xw)
        {
            string paramsText = node.NextSibling.InnerText;

            // usual format is <a ..>METHOD_NAME</a>(0..N PARAMS) - DESCRIPTION
            int start = paramsText.IndexOf('(');
            int end = paramsText.IndexOf(')');
            int dash = paramsText.IndexOf('-');

            if ((dash > -1 && dash < start)
                || start < 0
                || end < 0
                || start + 1 == end)
                return;

            string tmp = paramsText.Substring(start + 1, end - start - 1);
            string[] list = tmp.Split(',');
            string desc = paramsText.Remove(0, dash + 1).Trim();

            xw.WriteStartElement("summary");
            xw.WriteString(desc);
            xw.WriteEndElement();


            for (int i = 0; i < list.Length; i++)
            {
                string p = cleanerRegex.Replace(list[i], String.Empty);
                if (p.Length == 0)
                    continue;

                xw.WriteStartElement("param");
                xw.WriteAttributeString("name", p);
                xw.WriteEndElement();
            }
        }

    }
}
