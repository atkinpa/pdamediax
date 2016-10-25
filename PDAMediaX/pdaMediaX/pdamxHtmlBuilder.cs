using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml;
using System.Xml.XPath;
using System.Xml.Xsl;

namespace pdaMediaX.Web
{
    class pdamxHtmlBuilder
    {
        public pdamxHtmlBuilder()
        {
        }
        public static String Drawblueline()
        {
            return ("<div class='row><div class='col-sm-12' class='titlebguline'></div></div>");
        }
        public static String MoreLink(String _sLink)
        {
            return ("<a class='btn btn-link' style='padding:2px' href='" + _sLink + "'>more...</a>");
        }
        public static String RSSXSLTranslator(String _sRSSFile, String _sXSLTFile)
        {
            StringWriter swStringWriter;

            try
            {
                XPathDocument myXPathDoc = new XPathDocument(_sRSSFile);
                XslCompiledTransform myXslTrans = new XslCompiledTransform();
                myXslTrans.Load(_sXSLTFile);
                swStringWriter = new StringWriter();
                XmlTextWriter myWriter = new XmlTextWriter(swStringWriter);
                myXslTrans.Transform(myXPathDoc, null, myWriter);
                myWriter.Close();
                return (swStringWriter.ToString());
            }
            catch (Exception e)
            {
                return ("No data available, Error: " + e.Message);
            }
        }
        public static String TrimString(String _sString, int _nLength)
        {
            String sTrimInd = "...";
            int nLength = _nLength;

            if (_sString.Length < _nLength)
            {
                nLength = _sString.Length;
                sTrimInd = "";
            }
            return (_sString.Substring(0, nLength) + sTrimInd);
        }
    }
}
