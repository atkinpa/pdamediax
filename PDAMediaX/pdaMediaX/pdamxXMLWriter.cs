using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.IO;
using pdaMediaX.Common;

namespace pdaMediaX.Util.Xml
{
    public class pdamxXMLWriter
    {
        pdamxXMLTextTemplate mxXMLTextTemplate;

        TextWriter twTextWriter = null;
        bool bFirstWrite = true;
        bool bFileOpen = false;

        String sXMLVersion = "'1.0'";
        String sXMLEncoding = "'UTF-8'";
        String sRootNode = "";
        String sNamespace = "";
        String sDTD = "";

        public pdamxXMLWriter()
        {
            mxXMLTextTemplate = new pdamxXMLTextTemplate();
        }
        public pdamxXMLWriter (String _sFileName)
        {
            mxXMLTextTemplate = new pdamxXMLTextTemplate();
            Open(_sFileName);
        }
        public bool AddNSPrefix(String _sNSPrefix, String _sNSValue)
        {
            if (_sNSPrefix == null)
                return(false);

            if (_sNSPrefix.Trim().Length == 0)
                return (false);

            if (_sNSValue == null)
                return (false);

            if (_sNSValue.Trim().Length == 0)
                return (false);

            sNamespace = sNamespace + " " + "xmlns:" + _sNSPrefix + "='" + _sNSValue + "'";
            return (true);
        }
        public bool Close()
        {
            if (twTextWriter == null)
                return (false);

            if (sRootNode.Trim().Length != 0)
                twTextWriter.Write("\n</" + sRootNode + ">");

            twTextWriter.Close();
            twTextWriter = null;
            bFileOpen = false;
            bFirstWrite = true;
            return (true);
        }
        public static String ConvertUnprintableymbols(String _sTextData)
        {
            //
            // Convert unprintable chars above the value 127 to their number value...
            //
            StringBuilder sbResult;

            if (_sTextData == null)
                return (_sTextData);

            if (_sTextData.Trim().Length == 0)
                return (_sTextData);

            sbResult = new StringBuilder(_sTextData.Length + (int)(_sTextData.Length * 0.1));
            char[] chars = _sTextData.ToCharArray();
            foreach (char c in chars)
            {
                int value = Convert.ToInt32(c);
                if (value > 127)
                    sbResult.AppendFormat("&#{0};", value);
                else
                    sbResult.Append(c);
            }
            return (sbResult.ToString());
        }
        public bool CopyXMLTemplate(String _sTemplateName)
        {
            return(mxXMLTextTemplate.AddTemple(_sTemplateName));
        }
        public String GetXMLTemplate(String _sTemplateName)
        {
            return (RemoveXMLElementPlaceHolders(RemoveXMLElementMarkers(mxXMLTextTemplate.GetTemplate(_sTemplateName))));
        }
        public String GetNSPrefix(String _sNSPrefix)
        {
            String sNSPrefix = "xmlns:" + _sNSPrefix + "='";
            int nStartIdx;
            int nLen;

            if (_sNSPrefix == null)
                return (null);

            if (_sNSPrefix.Trim().Length == 0)
                return (null);

            if (!sNamespace.Contains(sNSPrefix))
                return (null); ;

            nStartIdx = sNamespace.IndexOf(sNSPrefix);
            nLen = sNamespace.IndexOf("'", (nStartIdx + sNSPrefix.Length + 2)) - nStartIdx;
            return (sNamespace.Substring(nStartIdx, nLen + 1));
        }
        public bool InsertXMLAtTemplateElementMarker(String _sTemplateName, String _sElementMarker, String _sInsertTemplateName)
        {
            return(mxXMLTextTemplate.InsertAtTemplateElementMarker(_sTemplateName, _sElementMarker, _sInsertTemplateName));
        }
        public bool isFileOpen()
        {
            return (bFileOpen);
        }
        public bool LoadXMLTemplate(String _sTemplateName, String _sTemplate)
        {
            return(mxXMLTextTemplate.LoadTemple(_sTemplateName, _sTemplate));
        }
        public bool Open(String _sFileName)
        {
            if (_sFileName == null)
                return (false);

            if (_sFileName.Trim().Length == 0)
                return (false);

            twTextWriter = new StreamWriter(_sFileName);
            bFileOpen = true;
            return (true);
        }
        public bool RemoveNSPrefix(String _sNSPrefix)
        {
            String sNSPrefix = "xmlns:" + _sNSPrefix + "='";
            int nStartIdx;
            int nLen;

            if (_sNSPrefix == null)
                return (false);

            if (_sNSPrefix.Trim().Length == 0)
                return (false);

            if (!sNamespace.Contains(sNSPrefix))
                return (false);

            nStartIdx = sNamespace.IndexOf(sNSPrefix);
            nLen = sNamespace.IndexOf("'", (nStartIdx + sNSPrefix.Length + 2)) - nStartIdx;
            sNamespace = sNamespace.Replace(sNamespace.Substring(nStartIdx, nLen + 1), "");
            return (true);
        }
        public bool ReplactXMPTemplateElementMarker(String _sTemplateName, String _sElementMarker, String _sElementMarkerTemplateName)
        {
            return(mxXMLTextTemplate.ReplactTemplateElementMarker(_sTemplateName, _sElementMarker, _sElementMarkerTemplateName));
        }
        public static String RemoveXMLElementMarkers(String _sXMLText)
        {
            //
            // Remove all '&[field]&' place holder from text data...
            //
            String sReturnData;

            if (_sXMLText == null)
                return (_sXMLText);

            if (_sXMLText.Trim().Length == 0)
                return (_sXMLText);

            sReturnData = _sXMLText;
            while (sReturnData.IndexOf("&[") > 0)
            {
                int nStartIdx = sReturnData.IndexOf("&[");
                int nLen = (sReturnData.IndexOf("]&") + 2) - nStartIdx;
                sReturnData = sReturnData.Replace(sReturnData.Substring(nStartIdx, nLen), "");
            }
            return (sReturnData);
        }
        public static String RemoveXMLElementPlaceHolders(String _sXMLText)
        {
            //
            // Remove all '&field&' place holder from text data...
            //
            String sReturnData;

            if (_sXMLText == null)
                return (_sXMLText);

            if (_sXMLText.Trim().Length == 0)
                return (_sXMLText);

            sReturnData = _sXMLText;
            while (sReturnData.IndexOf(">~") > 0)
            {
                int nStartIdx = sReturnData.IndexOf(">~");
                int nLen = (sReturnData.IndexOf("~<") + 1) - nStartIdx;
                sReturnData = sReturnData.Replace(sReturnData.Substring(nStartIdx + 1, nLen - 1), "");
            }
            return (sReturnData);
        }
        public String DTD
        {
            get
            {
                return (sDTD);
            }
            set
            {
                if (value != null)
                {
                    if (value.Trim().Length > 0)
                        sDTD = value + (value.ToLower().Contains(".dtd") ? "" : ".dtd");
                    else
                        sDTD = value;
                }
            }
        }
        public String Namespace
        {
            get
            {
                return (sNamespace);
            }
            set
            {
                if (value != null)
                {
                    if (value.Trim().Length > 0)
                        sNamespace = "xmlns='" + value + "'";
                    else
                        sNamespace = value;
                }
            }
        }
        public bool SetXMLTemplateElement(String _sTemplateName, String _sElementName, String _sElementValue)
        {
            return(mxXMLTextTemplate.SetTemplateElement(_sTemplateName, _sElementName, _sElementValue));
        }
        public bool SetXMLTemplateElementAttribute(String _sTemplateName, String _sElementName, String _sAttributeName, String _sAttributeValue)
        {
            return(mxXMLTextTemplate.SetTemplateElementAttribute(_sTemplateName, _sElementName, _sAttributeName, _sAttributeValue));
        }
        public bool Write(String _sXmlText)
        {
            String sXMLHeader;
            String sRootNodeHeader;

            if (twTextWriter == null)
                return (false);

            if (sRootNode.Trim().Length == 0)
                return (false);

            if (bFirstWrite)
            {
                sXMLHeader = "<?xml version=" + sXMLVersion + " encoding=" + sXMLEncoding + "?>";
                twTextWriter.Write(sXMLHeader);
                if (sDTD != null)
                    if (sDTD.Trim().Length > 0)
                        twTextWriter.Write("\n<!DOCTYPE " + sRootNode + " SYSTEM '" + sDTD + "'>");
                sRootNodeHeader = "\n<" + sRootNode;
                if (sNamespace != null)
                    if (sNamespace.Trim().Length > 0)
                        sRootNodeHeader = sRootNodeHeader + " " + sNamespace;
                sRootNodeHeader = sRootNodeHeader + ">";
                twTextWriter.Write(sRootNodeHeader);
                bFirstWrite = false;
            }
            twTextWriter.Write(ConvertUnprintableymbols(_sXmlText));
            twTextWriter.Flush();
            return (true);
        }
        public String RootNode
        {
            get
            {
                return (sRootNode);
            }
            set
            {
                if (value != null)
                    sRootNode = value;
            }
        }
    }
    class pdamxXMLTextTemplate
    {
        Hashtable hXMLTemplate;
        Hashtable hXMLTemplateWorkingArea;

        public pdamxXMLTextTemplate()
        {
            hXMLTemplate = new Hashtable();
            hXMLTemplateWorkingArea = new Hashtable();

        }
        public bool AddTemple(String _sTemplateName)
        {
            return(AddTemple(_sTemplateName, null));
        }
        public bool AddTemple(String _sTemplateName, String _sTemplateValue)
        {
            if (_sTemplateName == null)
                return (false);

            if (_sTemplateName.Trim().Length == 0)
                return (false);

            if (!hXMLTemplate.Contains(_sTemplateName))
                return (false);

            RemoveTemplate(_sTemplateName);
            if (_sTemplateValue == null)
                hXMLTemplateWorkingArea.Add(_sTemplateName, hXMLTemplate[_sTemplateName]);
            else
                hXMLTemplateWorkingArea.Add(_sTemplateName, _sTemplateValue);
            return (true);
        }

        public String GetTemplate(String _sTemplateName)
        {
            if (_sTemplateName == null)
                return(null);

            if (_sTemplateName.Trim().Length == 0)
                return (null);

            if (!hXMLTemplateWorkingArea.Contains(_sTemplateName))
                return(null);

            return ((String)hXMLTemplateWorkingArea[_sTemplateName].ToString());
        }
        public bool InsertAtTemplateElementMarker(String _sTemplateName, String _sElementMarker, String _sInsertTemplateName)
        {
            String sSearchElement;
            String sTemplate;
            String _sInsertTemplateValue;

            if (_sTemplateName == null)
                return (false);

            if (_sTemplateName.Trim().Length == 0)
                return (false);

            if (_sElementMarker == null)
                return (false);

            if (_sElementMarker.Trim().Length == 0)
                return (false);

            if (_sInsertTemplateName == null)
                return (false);

            if (_sInsertTemplateName.Trim().Length == 0)
                return (false);

            if ((sTemplate = GetTemplate(_sTemplateName)) == null)
                return (false);

            if ((_sInsertTemplateValue = GetTemplate(_sInsertTemplateName)) == null)
                _sInsertTemplateValue = _sInsertTemplateName;

            sSearchElement = "&[" + _sElementMarker + "]&";
            if (!sTemplate.Contains(sSearchElement))
                return (false);

            sTemplate = sTemplate.Replace(sSearchElement, _sInsertTemplateValue + sSearchElement);
            RemoveTemplate(_sTemplateName);
            AddTemple(_sTemplateName, sTemplate);
            return (true);
        }
        public bool LoadTemple(String _sTemplateName, String _sTemplate)
        {
            if (_sTemplateName == null)
                return (false);

            if (_sTemplateName.Trim().Length == 0)
                return (false);

            if (_sTemplate == null)
                return (false);

            if (_sTemplate.Trim().Length == 0)
                return (false);

            if (hXMLTemplate.Contains(_sTemplateName))
                return (false);

            hXMLTemplate.Add(_sTemplateName, _sTemplate);
            return (true);
        }
        private bool RemoveTemplate(String _sTemplateName)
        {
            if (_sTemplateName == null)
                return (false);

            if (_sTemplateName.Trim().Length == 0)
                return (false);

            hXMLTemplateWorkingArea.Remove(_sTemplateName);
            return (true);
        }
        public bool ReplactTemplateElementMarker(String _sTemplateName, String _sElementMarker, String _sElementMarkerTemplateName)
        {
            String sTemplate;
            String sSearchElement;
            String sReplacementValue;

            if (_sTemplateName == null)
                return (false);

            if (_sTemplateName.Trim().Length == 0)
                return (false);

            if (_sElementMarker == null)
                return (false);

            if (_sElementMarker.Trim().Length == 0)
                return (false);

            if (_sElementMarkerTemplateName == null)
                return (false);

            if (_sElementMarkerTemplateName.Trim().Length == 0)
                return (false);

            if ((sTemplate = GetTemplate(_sTemplateName)) == null)
                return (false);

            if ((sReplacementValue = GetTemplate(_sElementMarkerTemplateName)) == null)
                sReplacementValue = _sElementMarkerTemplateName;

            sSearchElement = "&[" + _sElementMarker + "]&";
            if (!sTemplate.Contains(sSearchElement))
                return (false);

            sTemplate = sTemplate.Replace(sSearchElement, sReplacementValue);
            RemoveTemplate(_sTemplateName);
            AddTemple(_sTemplateName, sTemplate);
            return (true);
        }
        public bool SetTemplateElement(String _sTemplateName, String _sElementName, String _sElementValue)
        {
            String sTemplate;
            String sSearchElement;
            String sReplacementValue;
            
            if (_sTemplateName == null)
                return (false);

            if (_sTemplateName.Trim().Length == 0 )
                return (false);

            if (_sElementName == null)
                return (false);

            if (_sElementName.Trim().Length == 0)
                return (false);

            if (_sElementValue == null)
                _sElementValue = "";

            if ((sTemplate = GetTemplate(_sTemplateName)) == null)
                return (false);
            
            sSearchElement =  "></" + _sElementName + ">";
            sReplacementValue = ">" + EscapeText(_sElementValue) + "</" + _sElementName + ">";

            int nStartIdx = sTemplate.IndexOf(sSearchElement);
            String sToReplace = sTemplate.Substring(0, nStartIdx + sSearchElement.Length);

            sToReplace = sToReplace.Replace(sSearchElement, sReplacementValue);
            sTemplate = sToReplace + sTemplate.Substring(nStartIdx + sSearchElement.Length,
                sTemplate.Length - (nStartIdx + sSearchElement.Length));

            RemoveTemplate(_sTemplateName);
            AddTemple(_sTemplateName, sTemplate);
            return (true);
        }
        public bool SetTemplateElementAttribute(String _sTemplateName, String _sElementName, String _sAttributeName, String _sAttributeValue)
        {
            String sTemplate;
            String sSearchElement;
            String sReplacementValue;

            if (_sTemplateName == null)
                return (false);

            if (_sTemplateName.Trim().Length == 0 )
                return (false);

            if (_sElementName == null)
                return (false);

            if (_sElementName.Trim().Length == 0)
                return (false);

            if (_sAttributeName == null)
                return (false);

            if (_sAttributeName.Trim().Length == 0)
                return (false);

            if (_sAttributeValue == null)
                _sAttributeValue = "";

            if ((sTemplate = GetTemplate(_sTemplateName)) == null)
                return (false);

            int nStartIdx = sTemplate.IndexOf("<" + _sElementName + " ");
            int nEndIdx = sTemplate.IndexOf(">", nStartIdx);
            sSearchElement = sTemplate.Substring(nStartIdx, nEndIdx - nStartIdx);
            sReplacementValue = sSearchElement.Replace(_sAttributeName + "=''", _sAttributeName + "='" + EscapeText(_sAttributeValue) + "'");
            sTemplate = sTemplate.Replace(sSearchElement, sReplacementValue);
            RemoveTemplate(_sTemplateName);
            AddTemple(_sTemplateName, sTemplate);
            return (true);
        }
        private String EscapeText(String sText)
        {
            String sRetunValue = sText;

            sRetunValue = sRetunValue.Replace("&", "&amp;");
            sRetunValue = sRetunValue.Replace("<", "&lt;");
            sRetunValue = sRetunValue.Replace(">", "&gt;");
            sRetunValue = sRetunValue.Replace("\"", "&quot;");
            sRetunValue = sRetunValue.Replace("'", "&apos;");
            return (sRetunValue);
        }
    }
}
