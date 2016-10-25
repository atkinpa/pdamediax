using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.XPath;
using pdaMediaX.Common;
using pdaMediaX.Util;
using pdaMediaX.Util.Xml;
using pdaMediaX.Web;

namespace pdaMediaX.util
{

    class pdamxHashTag
    {
        pdamxXMLReader mxXMLReader = null;
        Hashtable hSearchResult = null;
        String sUrl = null;

        public pdamxHashTag()
        {
        }
        public Hashtable FetchMusicSearchResult()
        {
            hSearchResult = new Hashtable();
            Hashtable hRecord;

            return (hSearchResult);
        }
        public Hashtable FetchMusicInfo()
        {
            hSearchResult = new Hashtable();

            
            return (hSearchResult);
        }
        public Hashtable FetchTivoSearchResult()
        {
            hSearchResult = new Hashtable();
            Hashtable hRecord;

            return (hSearchResult);
        }
        public Hashtable FetchVideoSearchResult()
        {
            Hashtable hRecord;
            //pdamxXMLReader mxXMLReader = null;
            //pdamxXMLReader mxXMLReaderEx = null;
            Hashtable hRecord;
            XPathNodeIterator xpathINode;
            XPathNodeIterator xpathINodeEx;
            int nRecordCnt = 0;
            hSearchResult = new Hashtable();
            if (!Open())
            {
                return (hSearhResult);
            }
            mxXMLReader.AddNamespace("vxmldb", "http://www.pdamediax.com/videoxmldb");

            ////if (VideoExXMLDB != null)
            //{
            //    mxXMLReaderEx = new pdamxXMLReader();
            //    mxXMLReaderEx.Open(VideoExXMLDB);
            //    mxXMLReaderEx.AddNamespace("exvxmldb", "http://www.pdamediax.com/exvideoxmldb");
           // }
            mxXMLReader.GetXPathNavigator().MoveToRoot();
            xpathINode = mxXMLReader.GetXPathNavigator().SelectChildren(XPathNodeType.Element);
            xpathINode.Current.MoveToFirstChild();
            while (xpathINode.MoveNext())
            {
                hRecord = new Hashtable();
                xpathINode.Current.MoveToFirstChild();
                if (xpathINode.Current.Name.Equals("SeriesID"))
                {
                    String sSeriesName = "";
                    do
                    {
                        if (xpathINode.Current.Name.Equals("SeriesName"))
                            sSeriesName = xpathINode.Current.Value;
                        if (xpathINode.Current.Name.Equals("Season"))
                        {
                            String sSeasonNumber = "";
                            xpathINode.Current.MoveToFirstChild();
                            do
                            {
                                if (xpathINode.Current.Name.Equals("SeasonNumber") && xpathINode.Current.Value != "N/vxmldb:A")
                                    sSeasonNumber = xpathINode.Current.Value;
                                if (xpathINode.Current.Name.Equals("Episode"))
                                {
                                    xpathINode.Current.MoveToFirstChild();
                                    hRecord = new Hashtable();
                                    hRecord.Add("SeasonNumber", sSeasonNumber);
                                    hRecord.Add("SeriesName", sSeriesName);
                                    do
                                    {
                                        if (xpathINode.Current.Name.Equals("Title"))
                                            hRecord.Add(xpathINode.Current.Name, sSeriesName + " - " + xpathINode.Current.Value);
                                        else
                                            if (xpathINode.Current.Name.Equals("EpisodeNumber") && xpathINode.Current.Value != "N/vxmldb:A")
                                            hRecord.Add(xpathINode.Current.Name, xpathINode.Current.Value);
                                        else
                                                if (!xpathINode.Current.Name.Equals("EpisodeNumber"))
                                            hRecord.Add(xpathINode.Current.Name, xpathINode.Current.Value);
                                    }
                                    while (xpathINode.Current.MoveToNext());
                                    hSearchResult.Add(Convert.ToString(++nRecordCnt), hRecord);
                                    xpathINode.Current.MoveToParent();
                                }
                            }
                            while (xpathINode.Current.MoveToNext());
                            xpathINode.Current.MoveToParent();
                        }
                    }
                    while (xpathINode.Current.MoveToNext());
                }
                if (xpathINode.Current.Name.Equals("SpecialID"))
                {
                    String sSpecialName = "";
                    do
                    {
                        if (xpathINode.Current.Name.Equals("SpecialName"))
                            sSpecialName = xpathINode.Current.Value;
                        if (xpathINode.Current.Name.Equals("Episode"))
                        {
                            xpathINode.Current.MoveToFirstChild();
                            hRecord = new Hashtable();
                            do
                            {
                                if (xpathINode.Current.Name.Equals("Title"))
                                    hRecord.Add(xpathINode.Current.Name, sSpecialName + " - " + xpathINode.Current.Value);
                                else
                                    hRecord.Add(xpathINode.Current.Name, xpathINode.Current.Value);
                            }
                            while (xpathINode.Current.MoveToNext());
                            hSearchResult.Add(Convert.ToString(++nRecordCnt), hRecord);
                            xpathINode.Current.MoveToParent();
                        }
                    }
                    while (xpathINode.Current.MoveToNext());
                    xpathINode.Current.MoveToParent();
                }
                if (xpathINode.Current.Name.Equals("MovieID"))
                {
                    String sDescription = "";
                    String sCredits = "";
                    String sMovieYear = "";
                    String sParentalRating = "";

                    do
                    {
                        if (xpathINode.Current.Name.Equals("Description"))
                            sDescription = xpathINode.Current.Value;
                        if (xpathINode.Current.Name.Equals("Credits"))
                            sCredits = xpathINode.Current.Value;
                        if (xpathINode.Current.Name.Equals("MovieYear"))
                            sMovieYear = xpathINode.Current.Value;
                        if (xpathINode.Current.Name.Equals("ParentalRating"))
                            sParentalRating = xpathINode.Current.Value;

                        hRecord.Add(xpathINode.Current.Name, xpathINode.Current.Value);
                    }
                    while (xpathINode.Current.MoveToNext());
                    if (VideoExXMLDB != null)
                    {
                        if ((sMovieYear.Trim().Length == 0) || (sCredits.Trim().Length == 0)
                            || (sDescription.Trim().Length == 0) || (sParentalRating.Trim().Length == 0))
                        {
                            xpathINodeEx = mxXMLReaderEx.GetNodePath("/exvxmldb:VideoCatalog/exvxmldb:MoviesCatalog/exvxmldb:Movie[exvxmldb:Title =\"" + hRecord["Title"].ToString() + "\"]");
                            if (xpathINodeEx.MoveNext())
                            {
                                xpathINodeEx.Current.MoveToFirstChild();
                                Hashtable hExRecord = new Hashtable();
                                do
                                {
                                    hExRecord.Add(xpathINodeEx.Current.Name, xpathINodeEx.Current.Value);
                                    if (xpathINodeEx.Current.Name.Equals("ParentalRating"))
                                        break;
                                }
                                while (xpathINodeEx.Current.MoveToNext());

                                if ((hExRecord["Description"].ToString().Trim().Length > 0))
                                {
                                    hRecord.Remove("Description");
                                    hRecord.Add("Description", hExRecord["Description"].ToString().Trim());
                                }
                                if ((hExRecord["Credits"].ToString().Trim().Length > 0))
                                {
                                    hRecord.Remove("Credits");
                                    hRecord.Add("Credits", hExRecord["Credits"].ToString().Trim());
                                }
                                if ((hExRecord["MovieYear"].ToString().Trim().Length > 0))
                                {
                                    hRecord.Remove("MovieYear");
                                    hRecord.Add("MovieYear", hExRecord["MovieYear"].ToString().Trim());
                                }
                                if ((hExRecord["ParentalRating"].ToString().Trim().Length > 0))
                                {
                                    hRecord.Remove("ParentalRating");
                                    hRecord.Add("ParentalRating", hExRecord["ParentalRating"].ToString().Trim());
                                }
                            }
                        }
                    }
                    hSearchResult.Add(Convert.ToString(++nRecordCnt), hRecord);
                }
                if (xpathINode.Current.Name.Equals("EpisodeID"))
                {
                    do
                    {
                        hRecord.Add(xpathINode.Current.Name, xpathINode.Current.Value);
                    }
                    while (xpathINode.Current.MoveToNext());

                    String sSeriesName;
                    String sSeasonNumber = "";
                    String sTitle;
                    String sGenre = hRecord["FileSystemGenre"].ToString();
                    String sFileLocation = hRecord["FileLocation"].ToString();

                    int nStartIdx = sFileLocation.ToLower().IndexOf(sGenre.ToLower() + "\\") + sGenre.Length + 1;
                    int nEndIdx = (nStartIdx > 0 ? sFileLocation.IndexOf("\\", nStartIdx) : 0);
                    int nLen = 0;
                    if (nEndIdx < 1)
                        nEndIdx = sFileLocation.Length;

                    sSeriesName = sFileLocation.Substring(nStartIdx, nEndIdx - nStartIdx);

                    if (sFileLocation.ToUpper().Contains("\\SEASON "))
                    {
                        nStartIdx = sFileLocation.ToUpper().IndexOf("\\SEASON ");
                        nLen = sFileLocation.Length - nStartIdx;
                        sSeasonNumber = sFileLocation.Substring(nStartIdx + 1, nLen - 1).Trim();
                    }
                    if (sFileLocation.ToUpper().Contains("\\BOOK ONE "))
                    {
                        nStartIdx = sFileLocation.ToUpper().IndexOf("\\BOOK ONE ");
                        nLen = sFileLocation.Length - nStartIdx;
                        sSeasonNumber = sFileLocation.Substring(nStartIdx + 1, nLen - 1).Trim();
                    }
                    if (sFileLocation.ToUpper().Contains("\\BOOK TWO "))
                    {
                        nStartIdx = sFileLocation.ToUpper().IndexOf("\\BOOK TWO ");
                        nLen = sFileLocation.Length - nStartIdx;
                        sSeasonNumber = sFileLocation.Substring(nStartIdx + 1, nLen - 1).Trim();
                    }
                    if (sFileLocation.ToUpper().Contains("\\BOOK THREE "))
                    {
                        nStartIdx = sFileLocation.ToUpper().IndexOf("\\BOOK THREE ");
                        nLen = sFileLocation.Length - nStartIdx;
                        sSeasonNumber = sFileLocation.Substring(nStartIdx + 1, nLen - 1).Trim();
                    }
                    hRecord.Add("SeasonNumber", sSeasonNumber);
                    sTitle = sSeriesName + " - " + hRecord["Title"];
                    hRecord.Remove("Title");
                    hRecord.Add("Title", sTitle);
                    hRecord.Add("SeriesName", sSeriesName);
                    hSearchResult.Add(Convert.ToString(++nRecordCnt), hRecord);
                }
                xpathINode.Current.MoveToPrevious();
                xpathINode.Current.MoveToParent();
            }
            return (hSearchResult);
        }
        public Hashtable FetchVideoInfo()
        {
            hSearchResult = new Hashtable();

            return(hSearchResult);
        }
        private pdamxXMLReader getXMLReader()
        {
            return (mxXMLReader);
        }
        public bool Open()
        {
            if (Url == null)
            {
                return (false);
            }
            mxXMLReader = new pdamxXMLReader();
            mxXMLReader.OpenUrl(Url);
            return (true);
        }
        public String XMLStringResult
        {
            get
            {
                return ((mxXMLReader != null ? mxXMLReader.URLXMLString : ""));
            }
        }
        public Hashtable XMLHashResult
        {
            get { return (hSearchResult); }
        }
        public String Url
        {
            get
            {
                return (sUrl);
            }
            set
            {
                if (value != null)
                    if (value.Trim().Length > 0)
                        sUrl = value;
            }
        }
    }
}
