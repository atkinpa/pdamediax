using System;
using System.IO;
using System.Text;
using System.Xml;
using System.Xml.XPath;
using pdaMediaX.Web;

namespace pdaMediaX.Util.Xml
{
    public class pdamxXMLReader
    {
        XPathDocument xpathDocument;
        XPathNodeIterator xpathINode = null;
        XPathNavigator xpathNavigator;
        XmlNamespaceManager xmlnsManager;
        String sXMLString = "";

        bool bOpenFlag = false;

        public pdamxXMLReader()
        {
        }
        public pdamxXMLReader(String _sFileName)
        {
            if (_sFileName == null)
                return;

            Open(_sFileName);
        }
        public bool AddNamespace(String _sPrefix, String _sUri)
        {
            if (_sPrefix == null)
                return (false);

            if (_sPrefix.Trim().Length == 0)
                return (false);

            if (_sUri == null)
                return (false);

            if (_sUri.Trim().Length == 0)
                return (false);

            if (GetXPathNavigator() == null)
                return (false);

            xmlnsManager = new System.Xml.XmlNamespaceManager(GetXPathNavigator().NameTable);
            xmlnsManager.AddNamespace(_sPrefix, _sUri);
            return (true);
        }
        public String DefaultNamespace()
        {
            if (xmlnsManager == null)
                return (null);

            return (xmlnsManager.DefaultNamespace);
        }
        public String GetNamespace(String _sNSPrefix)
        {
            if (_sNSPrefix == null)
                return (_sNSPrefix);

            if (_sNSPrefix.Trim().Length == 0)
                return (_sNSPrefix);

            return (GetXPathNavigator().GetNamespace(_sNSPrefix));
        }
        public XmlNamespaceManager GetNamespaceManager()
        {
            return (xmlnsManager);
        }
        public String GetNodeAttribute(String _sXPath, String _sAttributeName, String _sNameSpace)
        {
            XPathNodeIterator xpathINode;

            if (_sXPath == null)
                return (null);

            if (_sXPath.Trim().Length == 0)
                return (null);

            if (_sAttributeName == null)
                return (null);

            if (_sAttributeName.Trim().Length == 0)
                return (null);

            if (_sNameSpace == null)
                return (null);

            xpathINode = GetNodePath(_sXPath);

            if (xpathINode == null)
                return (null);

            xpathINode.MoveNext();
            return (xpathINode.Current.GetAttribute(_sAttributeName, _sNameSpace));
        }

        public XPathNodeIterator GetNodePath(String _sXPath)
        {
            if (_sXPath == null)
                return (null);

            if (_sXPath.Trim().Length == 0)
                return (null);

            if (xmlnsManager != null)
                xpathINode = xpathNavigator.Select(_sXPath, xmlnsManager);
            else
                xpathINode = xpathNavigator.Select(_sXPath);
            return (xpathINode);
        }
        public String GetNodeValue(String _sXPath)
        {  
            if (_sXPath == null)
                return(_sXPath);

            if (_sXPath.Trim().Length == 0)
                return (_sXPath);

            GetNodePath(_sXPath);

            if (GetXPathNodeIterator() == null)
                return (null);

            MoveNext();
            return (GetXPathNodeIterator().Current.Value);
        }
        public XPathNavigator GetXPathNavigator()
        {
            return (xpathNavigator);
        }
        public XPathNodeIterator GetXPathNodeIterator()
        {
            return (xpathINode);
        }
        public bool isOpen()
        {
            return (bOpenFlag);
        }
        public String LookupNamespace(String _sNSPrefix)
        {
            if (_sNSPrefix == null)
                return (_sNSPrefix);

            if (_sNSPrefix.Trim().Length == 0)
                return (_sNSPrefix);

            return (GetXPathNavigator().LookupNamespace(_sNSPrefix));
        }
        public bool MoveNext()
        {
            if (GetXPathNodeIterator() == null)
                return (false);

            return(GetXPathNodeIterator().MoveNext());
        }
        public bool Open(String _sFileName)
        {
            if (_sFileName == null)
                return (false);

            if (_sFileName.Trim().Length == 0)
                return (false);

            try
            {
                bOpenFlag = false;
                xpathDocument = new XPathDocument(_sFileName.Trim());
                xpathNavigator = xpathDocument.CreateNavigator();
                xpathNavigator.MoveToRoot();
                bOpenFlag = true;
                return (true);
            }
            catch (Exception e)
            {
                xpathDocument = null;
                Console.WriteLine("pdamxXMLReader:Open() - " + e.Message);
                return (false);
            }
        }
        public bool OpenRSS(String _sFileName)
        {
            if (_sFileName == null)
                return (false);

            if (_sFileName.Trim().Length == 0)
                return (false);

            try
            {
                bOpenFlag = false;
                XmlTextReader xmlTextReader = new XmlTextReader(_sFileName);
                xmlTextReader.MoveToContent();
                xmlTextReader.Read();

                xpathDocument = new XPathDocument(xmlTextReader);
                xpathNavigator = xpathDocument.CreateNavigator();
                xpathNavigator.MoveToRoot();
                bOpenFlag = true;
                return (true);
            }
            catch (Exception e)
            {
                xpathDocument = null;
                Console.WriteLine("pdamxXMLReader:OpenRSS() - " + e.Message);
                return (false);
            }
        }
        public bool OpenString(String _sXMLString)
        {
           
            if (_sXMLString == null)
                return (false);

            if (_sXMLString.Trim().Length == 0)
                return (false);

            try
            {
                //byte[] byteArray = Encoding.ASCII.GetBytes(_sXMLString);
               // MemoryStream stream = new MemoryStream(byteArray);

                //XmlTextReader xmlTextReader = new XmlTextReader(stream);
                //xmlTextReader.MoveToContent();
                //xmlTextReader.Read();

                xpathDocument = new XPathDocument(new StringReader(_sXMLString));
                //xpathDocument = new XPathDocument(xmlTextReader);
                xpathNavigator = xpathDocument.CreateNavigator();
                xpathNavigator.MoveToRoot();
                //bOpenFlag = true;
                return (true);
            }
            catch (Exception e)
            {
                xpathDocument = null;
                Console.WriteLine("pdamxXMLReader:OpenString() - " + e.Message);
                return (false);
            }
        }
        public bool OpenUrl(String _sUrl)
        {
            pdamxUrlReader mxUrlReader;

            if (_sUrl == null)
                return (false);

            if (_sUrl.Trim().Length == 0)
                return (false);

            try
            {
                mxUrlReader = new pdamxUrlReader(_sUrl);
                sXMLString = mxUrlReader.OpenUrl();
                OpenString(sXMLString);
            }
            catch (Exception e)
            {
                Console.WriteLine("pdamxXMLReader:OpenUrl() - " + e.Message);
                return (false);
            }
            return (true);
        }
        public bool RemoveNamespace(String _sPrefix, String _sUri)
        {
            if (_sPrefix == null)
                return (false);

            if (_sPrefix.Trim().Length == 0)
                return (false);

            if (_sUri == null)
                return (false);

            if (_sUri.Trim().Length == 0)
                return (false);

            if (GetNamespaceManager() == null)
                return (false);

            GetNamespaceManager().RemoveNamespace(_sPrefix, _sUri);
            return (true);
        }
        public String RootNodeLocalName
        {
            get
            {
                XPathNavigator xpathNavigator = GetXPathNavigator();

                xpathNavigator.MoveToRoot();
                return (xpathNavigator.LocalName);
            }
        }
        public String URLXMLString
        {
            get { return (sXMLString);  }
        }
        public XPathNavigator XPathNavigator
        {
            get { return (XPathNavigator); }
        }
        public XPathNodeIterator XPathNodeIterator
        {
            get
            {
                if (xpathINode == null)
                    xpathINode = GetXPathNavigator().Select("/");
                return (xpathINode);
            }
        }
    }
}
