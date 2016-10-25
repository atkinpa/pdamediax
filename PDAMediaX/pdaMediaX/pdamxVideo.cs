using System;
using System.Collections.Generic;
using System.Collections;
using System.Linq;
using System.Text;
using System.Xml.XPath;
using pdaMediaX.Common;
using pdaMediaX.Util;
using pdaMediaX.Util.Xml;
using pdaMediaX.Web;

namespace pdaMediaX.Media
{
    public class pdamxVideo
    {
        String sVideoXMLDB = null;
        String sVideoExXMLDB = null;
        String sSearchType = "xmlr";
        String sSiteSearchUrl = "";
        String sDataSource = "";
        String sSiteSearchAccess = "";

        public pdamxVideo()
        {
        }
        private String CapalizeText(String _sText)
        {
            String sReturnText = "";

            if (_sText == null)
                return (null);

            for (int i = 0; i < _sText.Length; i++)
            {
                if (i == 0)
                    sReturnText = _sText.Substring(i, 1).ToUpper();
                else if (_sText.Substring(i - 1, 1).Equals(" "))
                    sReturnText = sReturnText + _sText.Substring(i, 1).ToUpper();
                else
                    sReturnText = sReturnText + _sText.Substring(i, 1);

            }
            return (sReturnText);
        }
        public Hashtable GetVideoInfo(String _SVideoID, String _sIDType)
        {
            if (_SVideoID == null)
                return (null);

            if (_SVideoID.Trim().Length == 0)
                return (null);

            if (_sIDType == null)
                return (null);

            if (_sIDType.Trim().Length == 0)
                return (null);

            if ((!_sIDType.Trim().ToLower().Equals("movie"))
                && (!_sIDType.Trim().ToLower().Equals("episode"))
                && (!_sIDType.Trim().ToLower().Equals("special")))
                return (null);

            if (SearchType.Equals("xmlr"))
                return (GetVideoInfoXMLDB(_SVideoID, _sIDType));

            if (SearchType.Equals("rdms"))
                return (GetVideoInfoRDMS(_SVideoID, _sIDType));

            return (null);
        }
        private Hashtable GetVideoInfoHashTag (String _SVideoID, String _sIDType)
        {
            pdamxXMLReader mxXMLReader = null;
            pdamxXMLReader mxXMLReaderEx = null;
            Hashtable hRecord = new Hashtable();
            XPathNodeIterator xpathINode;
            XPathNodeIterator xpathINodeEx;
            String sSearchCriteria = "";
            String sDescription = "";
            String sCredits = "";
            String sMovieYear = "";
            String sParentalRating = "";

            mxXMLReader = new pdamxXMLReader();
            mxXMLReader.Open(VideoXMLDB);
            mxXMLReader.AddNamespace("vxmldb", "http://www.pdamediax.com/videoxmldb");

            if (VideoExXMLDB != null)
            {
                mxXMLReaderEx = new pdamxXMLReader();
                mxXMLReaderEx.Open(VideoExXMLDB);
                mxXMLReaderEx.AddNamespace("exvxmldb", "http://www.pdamediax.com/exvideoxmldb");
            }

            if (_sIDType.Trim().ToLower().Equals("movie"))
                sSearchCriteria = "/vxmldb:VideoCatalog/vxmldb:MoviesCatalog/vxmldb:Movie[vxmldb:MovieID =\"" + _SVideoID + "\"]";

            if (_sIDType.Trim().ToLower().Equals("episode"))
                sSearchCriteria = "/vxmldb:VideoCatalog/vxmldb:SeriesCatalog/vxmldb:Series/vxmldb:Season/vxmldb:Episode[vxmldb:EpisodeID =\"" + _SVideoID + "\"]";

            //           if (_sIDType.Trim().ToLower().Equals("special"))
            //               sSearchCriteria = "/vxmldb:VideoCatalog/vxmldb:SpecialsCatalog/vxmldb:Special/vxmldb:Episode/vxmldb[EpisodeID =\"" + _SVideoID + "\"]";

            xpathINode = mxXMLReader.GetNodePath(sSearchCriteria);
            if (!xpathINode.MoveNext())
            {
                if (_sIDType.Trim().ToLower().Equals("episode"))
                {
                    sSearchCriteria = "/vxmldb:VideoCatalog/vxmldb:SpecialsCatalog/vxmldb:Special/vxmldb:Episode[vxmldb:EpisodeID =\"" + _SVideoID + "\"]";
                    xpathINode = mxXMLReader.GetNodePath(sSearchCriteria);
                }
                else
                    return (hRecord);

                if (!xpathINode.MoveNext())
                    return (hRecord);
            }
            xpathINode.Current.MoveToFirstChild();
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

            if (_sIDType.Trim().ToLower().Equals("episode"))
            {
                xpathINode.Current.MoveToParent();
                xpathINode.Current.MoveToParent();
                xpathINode.Current.MoveToFirstChild();

                bool bSession = false;
                do
                {
                    if (xpathINode.Current.Name.Equals("SeriesNumberOfSeasons"))
                        hRecord.Add(xpathINode.Current.Name, xpathINode.Current.Value);
                    if (xpathINode.Current.Name.Equals("SeriesNumberOfEpisodes"))
                        hRecord.Add(xpathINode.Current.Name, xpathINode.Current.Value);
                    if (xpathINode.Current.Name.Equals("SeriesStorageUsed"))
                        hRecord.Add(xpathINode.Current.Name, xpathINode.Current.Value);
                    if (xpathINode.Current.Name.Equals("UFSeriesStorageUsed"))
                        hRecord.Add(xpathINode.Current.Name, xpathINode.Current.Value);

                    if (xpathINode.Current.Name.Equals("SpecialNumberOfEpisodes"))
                        hRecord.Add(xpathINode.Current.Name, xpathINode.Current.Value);
                    if (xpathINode.Current.Name.Equals("SpecialStorageUsed"))
                        hRecord.Add(xpathINode.Current.Name, xpathINode.Current.Value);
                    if (xpathINode.Current.Name.Equals("UFSpecialStorageUsed"))
                        hRecord.Add(xpathINode.Current.Name, xpathINode.Current.Value);

                    if (xpathINode.Current.Name.Equals("SeasonNumber"))
                        hRecord.Add(xpathINode.Current.Name, xpathINode.Current.Value);
                    if (xpathINode.Current.Name.Equals("SeasonNumberOfEpisodes"))
                        hRecord.Add(xpathINode.Current.Name, xpathINode.Current.Value);
                    if (xpathINode.Current.Name.Equals("SeasonStorageUsed"))
                        hRecord.Add(xpathINode.Current.Name, xpathINode.Current.Value);
                    if (xpathINode.Current.Name.Equals("UFSeasonStorageUsed"))
                        hRecord.Add(xpathINode.Current.Name, xpathINode.Current.Value);

                    if (xpathINode.Current.Name.Equals("SeasonID"))
                        bSession = true;
                }
                while (xpathINode.Current.MoveToNext());
                if (bSession)
                {
                    xpathINode.Current.MoveToParent();
                    xpathINode.Current.MoveToParent();
                    xpathINode.Current.MoveToFirstChild();
                    do
                    {
                        if (xpathINode.Current.Name.Equals("SeriesNumberOfSeasons"))
                            hRecord.Add(xpathINode.Current.Name, xpathINode.Current.Value);
                        if (xpathINode.Current.Name.Equals("SeriesNumberOfEpisodes"))
                            hRecord.Add(xpathINode.Current.Name, xpathINode.Current.Value);
                        if (xpathINode.Current.Name.Equals("SeriesStorageUsed"))
                            hRecord.Add(xpathINode.Current.Name, xpathINode.Current.Value);
                        if (xpathINode.Current.Name.Equals("UFSeriesStorageUsed"))
                            hRecord.Add(xpathINode.Current.Name, xpathINode.Current.Value);
                    }
                    while (xpathINode.Current.MoveToNext());
                }
                /*
                String sSeasonNumber = "";
                String sGenre = hRecord["FileSystemGenre"].ToString();
                String sFileLocation = hRecord["FileLocation"].ToString();

                int nStartIdx = sFileLocation.ToLower().IndexOf(sGenre.ToLower() + "\\") + sGenre.Length + 1;
                int nEndIdx = (nStartIdx > 0 ? sFileLocation.IndexOf("\\", nStartIdx) : 0);
                int nLen = 0;
                if (nEndIdx < 1)
                    nEndIdx = sFileLocation.Length;

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
                 */
            }
            else
            {
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

                            hRecord.Add("ExtraVideoInfo", "true");
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
            }
            return (hRecord);
        }

        private Hashtable GetVideoInfoXMLDB(String _SVideoID, String _sIDType)
        {
            pdamxXMLReader mxXMLReader = null;
            pdamxXMLReader mxXMLReaderEx = null;
            Hashtable hRecord = new Hashtable();
            XPathNodeIterator xpathINode;
            XPathNodeIterator xpathINodeEx;
            String sSearchCriteria = "";
            String sDescription = "";
            String sCredits = "";
            String sMovieYear = "";
            String sParentalRating = "";

            mxXMLReader = new pdamxXMLReader();
            mxXMLReader.Open(VideoXMLDB);
            mxXMLReader.AddNamespace("vxmldb", "http://www.pdamediax.com/videoxmldb");

            if (VideoExXMLDB != null)
            {
                mxXMLReaderEx = new pdamxXMLReader();
                mxXMLReaderEx.Open(VideoExXMLDB);
                mxXMLReaderEx.AddNamespace("exvxmldb", "http://www.pdamediax.com/exvideoxmldb");
            }

            if (_sIDType.Trim().ToLower().Equals("movie"))
                sSearchCriteria = "/vxmldb:VideoCatalog/vxmldb:MoviesCatalog/vxmldb:Movie[vxmldb:MovieID =\"" + _SVideoID + "\"]";

            if (_sIDType.Trim().ToLower().Equals("episode"))
                sSearchCriteria = "/vxmldb:VideoCatalog/vxmldb:SeriesCatalog/vxmldb:Series/vxmldb:Season/vxmldb:Episode[vxmldb:EpisodeID =\"" + _SVideoID + "\"]";

            //           if (_sIDType.Trim().ToLower().Equals("special"))
            //               sSearchCriteria = "/vxmldb:VideoCatalog/vxmldb:SpecialsCatalog/vxmldb:Special/vxmldb:Episode/vxmldb[EpisodeID =\"" + _SVideoID + "\"]";

            xpathINode = mxXMLReader.GetNodePath(sSearchCriteria);
            if (!xpathINode.MoveNext())
            {
                if (_sIDType.Trim().ToLower().Equals("episode"))
                {
                    sSearchCriteria = "/vxmldb:VideoCatalog/vxmldb:SpecialsCatalog/vxmldb:Special/vxmldb:Episode[vxmldb:EpisodeID =\"" + _SVideoID + "\"]";
                    xpathINode = mxXMLReader.GetNodePath(sSearchCriteria);
                }
                else
                    return (hRecord);

                if (!xpathINode.MoveNext())
                    return (hRecord);
            }
            xpathINode.Current.MoveToFirstChild();
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

            if (_sIDType.Trim().ToLower().Equals("episode"))
            {
                xpathINode.Current.MoveToParent();
                xpathINode.Current.MoveToParent();
                xpathINode.Current.MoveToFirstChild();

                bool bSession = false;
                do
                {
                    if (xpathINode.Current.Name.Equals("SeriesNumberOfSeasons"))
                        hRecord.Add(xpathINode.Current.Name, xpathINode.Current.Value);
                    if (xpathINode.Current.Name.Equals("SeriesNumberOfEpisodes"))
                        hRecord.Add(xpathINode.Current.Name, xpathINode.Current.Value);
                    if (xpathINode.Current.Name.Equals("SeriesStorageUsed"))
                        hRecord.Add(xpathINode.Current.Name, xpathINode.Current.Value);
                    if (xpathINode.Current.Name.Equals("UFSeriesStorageUsed"))
                        hRecord.Add(xpathINode.Current.Name, xpathINode.Current.Value);

                    if (xpathINode.Current.Name.Equals("SpecialNumberOfEpisodes"))
                        hRecord.Add(xpathINode.Current.Name, xpathINode.Current.Value);
                    if (xpathINode.Current.Name.Equals("SpecialStorageUsed"))
                        hRecord.Add(xpathINode.Current.Name, xpathINode.Current.Value);
                    if (xpathINode.Current.Name.Equals("UFSpecialStorageUsed"))
                        hRecord.Add(xpathINode.Current.Name, xpathINode.Current.Value);

                    if (xpathINode.Current.Name.Equals("SeasonNumber"))
                        hRecord.Add(xpathINode.Current.Name, xpathINode.Current.Value);
                    if (xpathINode.Current.Name.Equals("SeasonNumberOfEpisodes"))
                        hRecord.Add(xpathINode.Current.Name, xpathINode.Current.Value);
                    if (xpathINode.Current.Name.Equals("SeasonStorageUsed"))
                        hRecord.Add(xpathINode.Current.Name, xpathINode.Current.Value);
                    if (xpathINode.Current.Name.Equals("UFSeasonStorageUsed"))
                        hRecord.Add(xpathINode.Current.Name, xpathINode.Current.Value);

                    if (xpathINode.Current.Name.Equals("SeasonID"))
                        bSession = true;
                }
                while (xpathINode.Current.MoveToNext());
                if (bSession)
                {
                    xpathINode.Current.MoveToParent();
                    xpathINode.Current.MoveToParent();
                    xpathINode.Current.MoveToFirstChild();
                    do
                    {
                        if (xpathINode.Current.Name.Equals("SeriesNumberOfSeasons"))
                            hRecord.Add(xpathINode.Current.Name, xpathINode.Current.Value);
                        if (xpathINode.Current.Name.Equals("SeriesNumberOfEpisodes"))
                            hRecord.Add(xpathINode.Current.Name, xpathINode.Current.Value);
                        if (xpathINode.Current.Name.Equals("SeriesStorageUsed"))
                            hRecord.Add(xpathINode.Current.Name, xpathINode.Current.Value);
                        if (xpathINode.Current.Name.Equals("UFSeriesStorageUsed"))
                            hRecord.Add(xpathINode.Current.Name, xpathINode.Current.Value);
                    }
                    while (xpathINode.Current.MoveToNext());
                }
                /*
                String sSeasonNumber = "";
                String sGenre = hRecord["FileSystemGenre"].ToString();
                String sFileLocation = hRecord["FileLocation"].ToString();

                int nStartIdx = sFileLocation.ToLower().IndexOf(sGenre.ToLower() + "\\") + sGenre.Length + 1;
                int nEndIdx = (nStartIdx > 0 ? sFileLocation.IndexOf("\\", nStartIdx) : 0);
                int nLen = 0;
                if (nEndIdx < 1)
                    nEndIdx = sFileLocation.Length;

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
                 */
            }
            else
            {
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

                            hRecord.Add("ExtraVideoInfo", "true");
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
            }
            return (hRecord);
        }

        private Hashtable GetVideoInfoRDMS(String _SVideoID, String _sIDType)
        {
            Hashtable hRecord = new Hashtable();

            return (hRecord);
        }
        public Hashtable SearchVideoAll(String _sSearchValue, String _sSearchCatagory)
        {
            pdamxSearchKeyGen mxSearchKeyGen;
            String[] sMultiSearch;
            String[] sSearchValues;

            if (_sSearchValue == null)
                return (null);

            if (_sSearchValue.Trim().Length == 0)
                return (null);

            if (_sSearchCatagory == null)
                return (null);

            if (_sSearchCatagory.Trim().Length == 0)
                return (null);

            sMultiSearch = _sSearchValue.Split(';');
            if (sMultiSearch.Length == 0)
            {
                sMultiSearch = new String[1];
                sMultiSearch[0] = _sSearchValue;
            }
            sSearchValues = new String[sMultiSearch.Length];

            mxSearchKeyGen = new pdamxSearchKeyGen();
            for (int i = 0; i < sMultiSearch.Length; i++)
            {
                if (_sSearchCatagory == "genre")
                    sSearchValues[i] = CapalizeText(sMultiSearch[i]);
                else
                {
                    mxSearchKeyGen.GenerateKey(sMultiSearch[i].Replace("\"", "").Replace(":", " - "));
                    sSearchValues[i] = mxSearchKeyGen.StrongKey;
                }
            }

            if (SearchType.Equals("xmlr"))
                return (SearchAllXMLDB(sSearchValues, _sSearchCatagory));

            //if (_sSearchCatagory == "actor" || _sSearchCatagory == "genre")
            //    return (SearchAllXMLDB(CapalizeText(_sSearchValue), _sSearchCatagory));
            //else
            //    return (SearchAllXMLDB(mxSearchKeyGen.StrongKey, _sSearchCatagory));

            if (SearchType.Equals("rdms"))
                return (SearchAllRDMS(sSearchValues, _sSearchCatagory));

            //if (_sSearchCatagory == "actor" || _sSearchCatagory == "genre")
            //    return (SearchAllRDMS(CapalizeText(_sSearchValue), _sSearchCatagory));
            //else
            //    return (SearchAllRDMS(mxSearchKeyGen.StrongKey, _sSearchCatagory));
            if (SearchType.Equals("msss"))
                return (SearchAllMSSiteSearch(sSearchValues, _sSearchCatagory));
            //return (SearchAllMSSiteSearch(mxSearchKeyGen.StrongKey, _sSearchCatagory));

            return (null);
        }

        private Hashtable SearchAllMSSiteSearch(String[] _sSearchValue, String _sSearchCatagory)
        {
            Hashtable hSearchResult = new Hashtable();
            Hashtable hRecord = new Hashtable();

            return (hSearchResult);
        }
        private Hashtable SearchAllRDMS(String[] _sSearchValue, String _sSearchCatagory)
        {
            Hashtable hSearchResult = new Hashtable();
            Hashtable hRecord = new Hashtable();

            return (hSearchResult);
        }
        public Hashtable SearchAllHashTag(String _sSearchValue, String _sSearchCatagory, String _sUrl)
        {
            pdamxXMLReader mxXMLReader = null;
            pdamxXMLReader mxXMLReaderEx = null;
            Hashtable hSearchResult = new Hashtable();
            Hashtable hRecord; 
            XPathNodeIterator xpathINode;
            XPathNodeIterator xpathINodeEx;

            int nRecordCnt = 0;

            mxXMLReader = new pdamxXMLReader();
            mxXMLReader.OpenUrl(_sUrl);
            mxXMLReader.AddNamespace("vxmldb", "http://www.pdamediax.com/videoxmldb");

            if (VideoExXMLDB != null)
            {
                mxXMLReaderEx = new pdamxXMLReader();
                mxXMLReaderEx.Open(VideoExXMLDB);
                mxXMLReaderEx.AddNamespace("exvxmldb", "http://www.pdamediax.com/exvideoxmldb");
            }
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
        private Hashtable SearchAllXMLDB(String[] _sSearchValue, String _sSearchCatagory)
        {
            pdamxXMLReader mxXMLReader = null;
            pdamxXMLReader mxXMLReaderEx = null;
            Hashtable hSearchResult = new Hashtable();
            Hashtable hRecord;
            XPathNodeIterator xpathINode;
            XPathNodeIterator xpathINodeEx;
            String sSearchCriteria = "";
            String sSearchValue;

            int nRecordCnt = 0;

            if (VideoXMLDB == null)
                return (null);

            mxXMLReader = new pdamxXMLReader();
            mxXMLReader.Open(VideoXMLDB);
            mxXMLReader.AddNamespace("vxmldb", "http://www.pdamediax.com/videoxmldb");

            if (VideoExXMLDB != null)
            {
                mxXMLReaderEx = new pdamxXMLReader();
                mxXMLReaderEx.Open(VideoExXMLDB);
                mxXMLReaderEx.AddNamespace("exvxmldb", "http://www.pdamediax.com/exvideoxmldb");
            }
            for (int nSearches = 0; nSearches < _sSearchValue.Length; nSearches++)
            {
                if (_sSearchCatagory == "actor" || _sSearchCatagory == "genre")
                    sSearchValue = pdamxUtility.FilterSpecialChar(_sSearchValue[nSearches]);
                else
                    sSearchValue = pdamxUtility.FilterSpecialChar(_sSearchValue[nSearches]).ToLower();

                if (_sSearchCatagory == "actor")
                {
                    sSearchCriteria = "/vxmldb:VideoCatalog/vxmldb:MoviesCatalog/vxmldb:Movie/vxmldb:CreditsSearchKey[contains(.,\"" + sSearchValue + "\")]"
                      + " | /vxmldb:VideoCatalog/vxmldb:SeriesCatalog/vxmldb:Series/vxmldb:Season/vxmldb:Episode/vxmldb:CreditsSearchKey[contains(.,\"" + sSearchValue + "\")]"
                      + " | /vxmldb:VideoCatalog/vxmldb:SpecialsCatalog/vxmldb:Special/vxmldb:Episode/vxmldb:CreditsSearchKey[contains(.,\"" + sSearchValue + "\")]";
                }
                if (_sSearchCatagory == "all")
                {
                    sSearchCriteria = "/vxmldb:VideoCatalog/vxmldb:MoviesCatalog/vxmldb:Movie[starts-with(vxmldb:StrongSearchKey,\"" + sSearchValue + "\")]"
                      + " | /vxmldb:VideoCatalog/vxmldb:SeriesCatalog/vxmldb:Series[starts-with(vxmldb:StrongSearchKey,\"" + sSearchValue + "\")]"
                      + " | /vxmldb:VideoCatalog/vxmldb:SpecialsCatalog/vxmldb:Special[starts-with(vxmldb:StrongSearchKey,\"" + sSearchValue + "\")]"
                      + " | /vxmldb:VideoCatalog/vxmldb:SeriesCatalog/vxmldb:Series/vxmldb:Season/vxmldb:Episode[starts-with(vxmldb:StrongSearchKey,\"" + sSearchValue + "\")]"
                      + " | /vxmldb:VideoCatalog/vxmldb:SpecialsCatalog/vxmldb:Special/vxmldb:Episode[starts-with(vxmldb:StrongSearchKey,\"" + sSearchValue + "\")]";
                }
                if (_sSearchCatagory == "genre")
                {
                    sSearchCriteria = "/vxmldb:VideoCatalog/vxmldb:MoviesCatalog/vxmldb:Movie/vxmldb:Genre[contains(.,\"" + sSearchValue + "\")]"
                      + " | /vxmldb:VideoCatalog/vxmldb:SeriesCatalog/vxmldb:Series/vxmldb:Season/vxmldb:Episode/vxmldb:Genre[contains(.,\"" + sSearchValue + "\")]"
                      + " | /vxmldb:VideoCatalog/vxmldb:SpecialsCatalog/vxmldb:Special/vxmldb:Episode/vxmldb:Genre[contains(.,\"" + sSearchValue + "\")]";
                }
                if (_sSearchCatagory == "movie")
                {
                    sSearchCriteria = "/vxmldb:VideoCatalog/vxmldb:MoviesCatalog/vxmldb:Movie[starts-with(vxmldb:StrongSearchKey,\"" + sSearchValue + "\")]";
                }
                if (_sSearchCatagory == "series")
                {
                    sSearchCriteria = "/vxmldb:VideoCatalog/vxmldb:SeriesCatalog/vxmldb:Series[starts-with(vxmldb:StrongSearchKey,\"" + sSearchValue + "\")]";
                }
                if (_sSearchCatagory == "special")
                {
                    sSearchCriteria = "/vxmldb:VideoCatalog/vxmldb:SpecialsCatalog/vxmldb:Special[starts-with(vxmldb:StrongSearchKey,\"" + sSearchValue + "\")]";
                }
                if (_sSearchCatagory == "episode")
                {
                    sSearchCriteria = "/vxmldb:VideoCatalog/vxmldb:SeriesCatalog/vxmldb:Series/vxmldb:Season/vxmldb:Episode[starts-with(vxmldb:StrongSearchKey,\"" + sSearchValue + "\")]";
                }
                if (_sSearchCatagory == "keyword")
                {
                    sSearchCriteria = "/vxmldb:VideoCatalog/vxmldb:MoviesCatalog/vxmldb:Movie/vxmldb:StrongSearchKey[contains(.,\"" + sSearchValue + "\")]"
                        + " | /vxmldb:VideoCatalog/vxmldb:SeriesCatalog/vxmldb:Series/vxmldb:StrongSearchKey[contains(.,\"" + sSearchValue + "\")]"
                        + " | /vxmldb:VideoCatalog/vxmldb:SpecialsCatalog/vxmldb:Special/vxmldb:StrongSearchKey[contains(.,\"" + sSearchValue + "\")]"
                        + " | /vxmldb:VideoCatalog/vxmldb:SeriesCatalog/vxmldb:Series/vxmldb:Season/vxmldb:Episode/vxmldb:StrongSearchKey[contains(.,\"" + sSearchValue + "\")]"
                        + " | /vxmldb:VideoCatalog/vxmldb:SpecialsCatalog/vxmldb:Special/vxmldb:Episode/vxmldb:StrongSearchKey[contains(.,\"" + sSearchValue + "\")]";
                }
                xpathINode = mxXMLReader.GetNodePath(sSearchCriteria);


                while (xpathINode.MoveNext())
                {
                    hRecord = new Hashtable();
                    if (_sSearchCatagory == "keyword" || _sSearchCatagory == "actor" || _sSearchCatagory == "genre")
                        xpathINode.Current.MoveToParent();

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
                }
            }
            return (hSearchResult);
        }
        public String DataSource
        {
            get
            {
                return (sDataSource);
            }
            set
            {
                if (value != null)
                    if (value.Trim().Length > 0)
                        sDataSource = value;
            }
        }
        public String SearchType
        {
            get
            {
                return (sSearchType);
            }
            set
            {
                if (value != null)
                    if (value.ToLower().Trim().Equals("xmlr")
                        || value.ToLower().Trim().Equals("rdms")
                        || value.ToLower().Trim().Equals("msss"))
                        sSearchType = value;
            }
        }
        public String SiteSearchAccess
        {
            set
            {
                if (value != null)
                    if (value.Trim().Length > 0)
                        sSiteSearchAccess = value;
            }
        }
        public String SiteSearchUrl
        {
            get
            {
                return (sSiteSearchUrl);
            }
            set
            {
                if (value != null)
                    if (value.Trim().Length > 0)
                        sSiteSearchUrl = value;
            }
        }
        public String VideoXMLDB
        {
            get
            {
                return (sVideoXMLDB);
            }
            set
            {
                sVideoXMLDB = value;
            }
        }
        public String VideoExXMLDB
        {
            get
            {
                return (sVideoExXMLDB);
            }
            set
            {
                sVideoExXMLDB = value;
            }
        }
    }
}
