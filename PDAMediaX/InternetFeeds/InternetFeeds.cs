using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.XPath;
using pdaMediaX.Web;

namespace InternetFeeds
{
    class InternetFeeds : pdaMediaX.pdamxBatchJob
    {
        static void Main(string[] args)
        {
            new InternetFeeds();
        }
        public InternetFeeds()
        {
            XPathNodeIterator xpathINode;
            pdamxUrlReader mxUrlReader = new pdamxUrlReader();

            String sRSSExt;
            String sFeedDirectory;

            sRSSExt = GetSettings("/Feeds/RSS/RSSExtension");
            sFeedDirectory = GetSettings("/Feeds/RSS/RSSFeedDirectory");

            xpathINode = SettingsObject.GetNodePath("/Feeds/RSS/*");
            while (xpathINode.MoveNext())
            {
                if (xpathINode.Current.Name.Equals("Feed"))
                {
                    String sFileName = "";
                    String sUrl = "";
                    xpathINode.Current.MoveToFirstChild();

                    do
                    {
                        if (xpathINode.Current.Name.Equals("Name"))
                            sFileName = sFeedDirectory + xpathINode.Current.Value + "." + sRSSExt;
                        if (xpathINode.Current.Name.Equals("Url"))
                            sUrl = xpathINode.Current.Value;
                    }
                    while (xpathINode.Current.MoveToNext());
                    xpathINode.Current.MoveToParent();
                    mxUrlReader.Url = sUrl;
                    mxUrlReader.WriteToFile = sFileName;
                    mxUrlReader.OpenUrl();
                }
            }
        }
    }
}
