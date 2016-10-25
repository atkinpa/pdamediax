using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace pdaMediaX.Web
{
	class pdamxRSS
	{
        String sXML_XSLTStyleSheetFile;

        public pdamxRSS()
        {
            sXML_XSLTStyleSheetFile = null;
        }
        public bool DownloadRSS(String _sUrl, String _sFileName)
        {
            if (_sUrl == null)
                return (false);

            if (_sUrl.Trim().Length == 0)
                return (false);

            if (_sFileName == null)
                return (false);

            if (_sFileName.Trim().Length == 0)
                return (false);

            return(true);
        }
        public String LoadRSS(String _sFileName)
        {
            String sRss = null;

            if (_sFileName == null)
                return (null);

            if (_sFileName.Trim().Length == 0)
                return (null);

            return (sRss);
        }
        public String LoadRSSUrl(String _sUrl)
        {
            String sRss = null;

            if (_sUrl == null)
                return (null);

            if (_sUrl.Trim().Length == 0)
                return (null);

            return (sRss);
        }
        public String XMLStyleSheet
        {
            get
            {
                return (sXML_XSLTStyleSheetFile);
            }
            set
            {
                if (value != null)
                    if (value.Trim().Length > 0)
                        sXML_XSLTStyleSheetFile = value;
            }
        }
        public String RenderHTML
        {
            get
            {
                return ("Output goes here");
            }
        }
	}
}
