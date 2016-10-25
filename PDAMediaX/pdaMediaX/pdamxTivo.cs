using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml.XPath;
using Oracle.DataAccess.Client;
using pdaMediaX.Common;
using pdaMediaX.Util;
using pdaMediaX.Web;
using pdaMediaX.Util.Xml;

namespace pdaMediaX.Web
{
	public class pdamxTivo
	{
        String sTivoXMLDB;
        OracleConnection odrDBConn;
        int nTivoSeries = 0;
        bool bDBSearch;

        public pdamxTivo()
        {
        }
        private String GetDate(String _sUnformattedDate)
        {
            DateTimeFormatInfo dtFormat = new CultureInfo("en-US", false).DateTimeFormat;
            DateTime dtDateTime;
            String[] sTime;
            String[] sDate;

            if (_sUnformattedDate == null)
                return (null);

            if (_sUnformattedDate.Length == 0)
                return (null);

            sDate = _sUnformattedDate.Split('T')[0].Split('-');
            sTime = _sUnformattedDate.Replace("Z", "").Split('T')[1].Split(':');

            dtDateTime = Convert.ToDateTime(sDate[1] + "/" + sDate[2] + "/" + sDate[0] + " " + sTime[0] + ":" + sTime[1] + ":" + sTime[2]);
            dtDateTime = dtDateTime.ToLocalTime();
            return (dtDateTime.ToString("MM/dd/yyyy", dtFormat));
        }
        public Hashtable GetListOfPrograms()
        {
            Hashtable hResultSet;

            if (TivoXMLDB == null)
                return (null);

            hResultSet = new Hashtable();
            return (hResultSet);
        }
        public Hashtable GetProgramInfo(String _sProgramID)
        {
            pdamxXMLReader mxXMLReader = null;
            XPathNodeIterator xpathINode;
            Hashtable hResultSet;
            String sSearchCriteria = "";

            if (TivoXMLDB == null)
                return (null);

            hResultSet = new Hashtable();            
            mxXMLReader = new pdamxXMLReader();
            mxXMLReader.Open(TivoXMLDB);
            mxXMLReader.AddNamespace("tivo", "http://www.pdamediax.com/tivonowplaying");

            sSearchCriteria = "/tivo:TivoNowPlaying/tivo:ShowList/tivo:Show[tivo:ProgramID =\"" + _sProgramID + "\"]";
            xpathINode = mxXMLReader.GetNodePath(sSearchCriteria);
            if (!xpathINode.MoveNext())
                return (hResultSet);

            xpathINode.Current.MoveToFirstChild();
            do
            {
                hResultSet.Add(xpathINode.Current.Name, xpathINode.Current.Value);
            }
            while (xpathINode.Current.MoveToNext());

            return (hResultSet);
        }
        private String GetTime(String _sUnformattedTime)
        {
            DateTimeFormatInfo dtFormat = new CultureInfo("en-US", false).DateTimeFormat;
            DateTime dtTime;
            String[] sTime;

            if (_sUnformattedTime == null)
                return (null);

            if (_sUnformattedTime.Length == 0)
                return (null);

            sTime = _sUnformattedTime.Replace("Z", "").Split('T')[1].Split(':');

            dtTime = Convert.ToDateTime("01/01/1900 " + sTime[0] + ":" + sTime[1] + ":" + sTime[2]);
            dtTime = dtTime.ToLocalTime();
            return (dtTime.ToString("hh:mm tt", dtFormat));

        }
        public Hashtable GetTivoNowPlayingList(String _sSettopUrl, String _sCredentials, String _sTempDataFile)
        {
            String[] sAdvisoryList = { "AL", "Language", "MV", "Mild Violence", "V", "Violence", "BN", "Brief Nudity", "N", "Nudity", "SC", "Strong Sexual Content", "GV", "Graphic Violence", "AC", "Adult Situations" };
            String[] sStarRatingList = { "ONE", "*", "ONE POINT FIVE", "*½", "TWO", "**", "TWO POINT FIVE", "**½", "THREE", "***", "THREE POINT FIVE", "***½", "FOUR", "****", "FOUR POINT FIVE", "****½", "FIVE", "*****" };
            String[] sNetworAffiliateList = { "2", "CBS Affiliate", "4", "NBC Affiliate", "5", "Fox Affiliate", "7", "ABC Affiliate", "9", "MyNetworkTV Affiliate", "11", "CW Affiliate" };
            String sNetworkAffiliate = "";
            bool bSeries5 = true;

            DateTimeFormatInfo timeFormat = new CultureInfo("en-US", false).DateTimeFormat;
            XPathNodeIterator xpathINode;
            XPathNodeIterator xpathINodeDetails;
            pdamxUrlReader mxUrlReader;
            pdamxCrypter mxCrypter;
            pdamxXMLReader mxXMLReader;
            pdamxSearchKeyGen mxSearchKeyGen;
            FileInfo fiTempFileInfo;
            Hashtable hResultSet = null;
            Hashtable hRecord = new Hashtable();
            String sTempDataFile;
            String[] sCredentials;
            int nRowCnt = 0;
            int nTotalChildren = 0;
            int nTivoItemCount = 0;
            int nEntriesRead = 0;
            int nSeries5StartPos = 0;
            int nSeries5NewStartPos = 0;
            int nScheduledRecordings = 0;
            long lTotalRecordingTimeOfScheduledRecordings = 0;
            long lTotalRecordingTimeOfTivoSuggestions = 0;
            long lTotalSStorageUsedByScheduledRecordings = 0;
            long lTotalStoragedUsedByTivoSuggestions = 0;
            bool bFirstPast = true;

            if (_sSettopUrl == null)
                return (null);

            if (_sCredentials == null)
                return (null);

            if (_sTempDataFile == null)
                return (null);

            if (_sSettopUrl.Length == 0)
                return (null);

            if (_sCredentials.Length == 0)
                return (null);

            if (_sTempDataFile.Length == 0)
                return (null);

            mxCrypter = new pdamxCrypter();
            if (_sCredentials.Contains(".edf"))
                sCredentials = mxCrypter.DecryptFile(_sCredentials).Split('/');
            else
                sCredentials = _sCredentials.Split('/');

            hResultSet = new Hashtable();
            sTempDataFile = DateTime.Now.Millisecond + "-" + _sTempDataFile;
            while (bSeries5)
            {
                if (!_sSettopUrl.Contains("AnchorOffset") || _sSettopUrl.Contains("Container1"))
                {
                    bSeries5 = false;
                }
                if (bSeries5)
                {
                    _sSettopUrl = _sSettopUrl.Replace("AnchorOffset=" + Convert.ToString(nSeries5StartPos), "AnchorOffset=" + Convert.ToString(nSeries5NewStartPos));
                    nSeries5NewStartPos = nSeries5NewStartPos + 50;
                }
                mxUrlReader = new pdamxUrlReader();
                mxUrlReader.AcceptInvalidSSLCertificate = true;
                mxUrlReader.UseCredentials = true;
                mxUrlReader.XMLFiltering = true;
                mxUrlReader.UserCredentals = sCredentials;
                mxUrlReader.Url = _sSettopUrl;
                mxUrlReader.WriteToFile = sTempDataFile;

                if (mxUrlReader.OpenUrl() != null) // Temp file created successfully...
                {
                    mxXMLReader = new pdamxXMLReader();
                    mxSearchKeyGen = new pdamxSearchKeyGen();
                    mxXMLReader.Open(sTempDataFile);
                    mxXMLReader.AddNamespace("tivo", "http://www.tivo.com/developer/calypso-protocol-1.6/");
                    if (mxXMLReader.GetNodeValue("/tivo:TiVoContainer/tivo:ItemCount") != null)
                        nTivoItemCount = Convert.ToInt32(mxXMLReader.GetNodeValue("/tivo:TiVoContainer/tivo:ItemCount"));

                    if (bSeries5)
                    {
                        nSeries5StartPos = Convert.ToInt32(mxXMLReader.GetNodeValue("/tivo:TiVoContainer/tivo:ItemStart"));
                    }
                    if (nTivoItemCount == 0)
                    {
                        break;
                    }
                    if (bFirstPast)
                    {
                        hResultSet.Add("Title", mxXMLReader.GetNodeValue("/tivo:TiVoContainer/tivo:Details/tivo:Title"));
                        if (Convert.ToInt32(mxXMLReader.GetNodeValue("/tivo:TiVoContainer/tivo:Details/tivo:TotalItems")) == 1) {
                            fiTempFileInfo = new FileInfo(sTempDataFile);
                            fiTempFileInfo.Delete();
                            return(hResultSet);
                        }
                        hResultSet.Add("SortOrder", mxXMLReader.GetNodeValue("/tivo:TiVoContainer/tivo:SortOrder"));
                        hResultSet.Add("GlobalSort", mxXMLReader.GetNodeValue("/tivo:TiVoContainer/tivo:GlobalSort"));
                        hResultSet.Add("MediaKey", sCredentials[1]);
                        int nStartIdx = _sSettopUrl.IndexOf("//") + 2;
                        hResultSet.Add("SettopIP", _sSettopUrl.Substring(nStartIdx, _sSettopUrl.IndexOf(":", nStartIdx) - nStartIdx));
                        hResultSet.Add("SettopUrl", _sSettopUrl.Replace("&", "&amp;"));
                        bFirstPast = false;
                    }
                    xpathINode = mxXMLReader.GetNodePath("/tivo:TiVoContainer/tivo:Item/*");
                    while (xpathINode.MoveNext())
                    {
                        if (xpathINode.Current.Name.Equals("Details"))
                        {
                            hRecord = new Hashtable();
                            nTotalChildren = 0;
                            sNetworkAffiliate = "";
                            xpathINode.Current.MoveToFirstChild();
                            nEntriesRead++;

                            hRecord.Add("TivoSuggestion", "No");
                            do
                            {
                                if (xpathINode.Current.Name.Equals("Title"))
                                {
                                    mxSearchKeyGen.GenerateKey(xpathINode.Current.Value);
                                    hRecord.Add("Title", xpathINode.Current.Value);
                                    hRecord.Add("TitleStrongSearchKey", mxSearchKeyGen.StrongKey);
                                }
                                if (xpathINode.Current.Name.Equals("ProgramId"))
                                    hRecord.Add("ProgramId", xpathINode.Current.Value);

                                if (xpathINode.Current.Name.Equals("EpisodeTitle"))
                                {
                                    mxSearchKeyGen.GenerateKey(xpathINode.Current.Value);
                                    hRecord.Add("EpisodeTitle", xpathINode.Current.Value);
                                    hRecord.Add("EpisodeStrongSearchKey", mxSearchKeyGen.StrongKey);
                                }
                                if (xpathINode.Current.Name.Equals("EpisodeNumber"))
                                    hRecord.Add("EpisodeNumber", xpathINode.Current.Value);

                                if (xpathINode.Current.Name.Equals("Duration"))
                                {
                                    hRecord.Add("Duration", pdamxUtility.FormatMiliseconds(xpathINode.Current.Value));
                                    hRecord.Add("UFDuration", GetTimeInSeconds(pdamxUtility.FormatMiliseconds(xpathINode.Current.Value)));
                                }
                                if (xpathINode.Current.Name.Equals("CaptureDate"))
                                    hRecord.Add("RecordDate", xpathINode.Current.Value);

                                if (xpathINode.Current.Name.Equals("Description"))
                                    hRecord.Add("Description", xpathINode.Current.Value.Replace(" Copyright Tribune Media Services, Inc.", ""));

                                if (xpathINode.Current.Name.Equals("SourceChannel"))
                                {
                                    hRecord.Add("Channel", xpathINode.Current.Value);
                                    for (int i = 0; i < sNetworAffiliateList.Length; i = i + 2)
                                    {
                                        if ((hRecord["Channel"].ToString().Equals(sNetworAffiliateList[i]))
                                            || (hRecord["Channel"].ToString().Equals("70" + sNetworAffiliateList[i]))
                                            || (hRecord["Channel"].ToString().Equals("7" + sNetworAffiliateList[i])))
                                        {
                                            sNetworkAffiliate = sNetworAffiliateList[i + 1];
                                            break;
                                        }
                                    }
                                    hRecord.Add("NetworkAffiliate", (sNetworkAffiliate.Length > 0 ? sNetworkAffiliate : "Satellite"));
                                }
                                if (xpathINode.Current.Name.Equals("SourceStation"))
                                    hRecord.Add("StationName", xpathINode.Current.Value);

                                if (xpathINode.Current.Name.Equals("HighDefinition"))
                                    hRecord.Add("IsHDContent", xpathINode.Current.Value);

                                if (xpathINode.Current.Name.Equals("InProgress"))
                                    hRecord.Add("IsRecording", xpathINode.Current.Value);

                                if (xpathINode.Current.Name.Equals("SourceSize"))
                                {
                                    hRecord.Add("VidoeSize", pdamxUtility.FormatStorageSize(xpathINode.Current.Value));
                                    hRecord.Add("UFVidoeSize", xpathINode.Current.Value);
                                }

                                if (xpathINode.Current.Name.Equals("TotalItems"))
                                    nTotalChildren = Convert.ToInt32(xpathINode.Current.Value);
                            }
                            while (xpathINode.Current.MoveToNext());
                            if (hRecord["IsRecording"] == null)
                                hRecord.Add("IsRecording", "No"); // Default if not assigned...
                            xpathINode.Current.MoveToParent();
                        }
                        if (xpathINode.Current.Name.Equals("Links"))
                        {
                            xpathINode.Current.MoveToFirstChild();
                            do
                            {
                                if (xpathINode.Current.Name.Equals("Content"))
                                {
                                    xpathINode.Current.MoveToFirstChild();
                                    do
                                    {
                                        if (xpathINode.Current.Name.Equals("Url"))
                                        {
                                            // Get entries in group...
                                            if (nTotalChildren > 0)
                                            {
                                                pdamxTivo mxTivoChildEntries = new pdamxTivo();
                                                Hashtable hTivoChildrenRecord = mxTivoChildEntries.GetTivoNowPlayingList(xpathINode.Current.Value, _sCredentials, nTotalChildren + "-" + _sTempDataFile);
                                                int nMaxChildrenRows = Convert.ToInt32(hTivoChildrenRecord["ProcessCount"].ToString());
                                                for (int i = 0; i < nMaxChildrenRows; )
                                                {
                                                    if (nEntriesRead == nTivoItemCount)
                                                    {
                                                        Hashtable hChildRecord = (Hashtable)hTivoChildrenRecord[Convert.ToString(++i)];
                                                        hChildRecord.Remove("TivoSuggestion");
                                                        hChildRecord.Add("TivoSuggestion", "Yes");
                                                        lTotalStoragedUsedByTivoSuggestions = lTotalStoragedUsedByTivoSuggestions + Convert.ToInt64(hChildRecord["UFVidoeSize"]);
                                                        lTotalRecordingTimeOfTivoSuggestions = lTotalRecordingTimeOfTivoSuggestions + Convert.ToInt64(GetTimeInSeconds(hChildRecord["Duration"].ToString()));
                                                        hResultSet.Add(Convert.ToString(++nRowCnt), hChildRecord);
                                                        nScheduledRecordings = nRowCnt - nTotalChildren;
                                                    }
                                                    else
                                                    {
                                                        Hashtable hChildRecord = (Hashtable)hTivoChildrenRecord[Convert.ToString(++i)];
                                                        lTotalSStorageUsedByScheduledRecordings = lTotalSStorageUsedByScheduledRecordings + Convert.ToInt64(hChildRecord["UFVidoeSize"]);
                                                        lTotalRecordingTimeOfScheduledRecordings = lTotalRecordingTimeOfScheduledRecordings + Convert.ToInt64(GetTimeInSeconds(hChildRecord["Duration"].ToString()));
                                                        hResultSet.Add(Convert.ToString(++nRowCnt), hChildRecord);
                                                    }
                                                }
                                            }
                                            else
                                            {
                                                hRecord.Add("DownloadUrl", xpathINode.Current.Value);
                                            }
                                        }
                                    }
                                    while (xpathINode.Current.MoveToNext());
                                    xpathINode.Current.MoveToParent();
                                }
                                if (xpathINode.Current.Name.Equals("TiVoVideoDetails"))
                                {
                                    xpathINode.Current.MoveToFirstChild();
                                    do
                                    {
                                        if (xpathINode.Current.Name.Equals("Url"))
                                        {
                                            mxUrlReader.Url = xpathINode.Current.Value;
                                            mxUrlReader.WriteToFile = "temp-" + sTempDataFile;

                                            if (mxUrlReader.OpenUrl() != null)
                                            {
                                                mxXMLReader = new pdamxXMLReader();
                                                mxXMLReader.Open("temp-" + sTempDataFile);
                                                mxXMLReader.AddNamespace("TvBusMarshalledStruct", "http://tivo.com/developer/xml/idl/TvBusMarshalledStruct");

                                                // Get credits...
                                                String sActorList = "";
                                                xpathINodeDetails = mxXMLReader.GetNodePath("/TvBusMarshalledStruct:TvBusEnvelope/showing/program/vActor/*");
                                                while (xpathINodeDetails.MoveNext())
                                                {
                                                    String[] sActors = xpathINodeDetails.Current.Value.Split('|');
                                                    sActorList = sActorList + (sActorList.Length > 0 ? ";" : "") + sActors[1] + " " + sActors[0];
                                                }
                                                hRecord.Add("Credits", sActorList);

                                                // Get genres...
                                                String sGenreList = "";
                                                xpathINodeDetails = mxXMLReader.GetNodePath("/TvBusMarshalledStruct:TvBusEnvelope/showing/program/vProgramGenre/*");
                                                while (xpathINodeDetails.MoveNext())
                                                {
                                                    sGenreList = sGenreList + (sGenreList.Length > 0 ? "," : "") + xpathINodeDetails.Current.Value.Replace("_", "");
                                                }
                                                hRecord.Add("Genre", sGenreList);

                                                // Get advisory...
                                                String sAdvisory = "";
                                                xpathINodeDetails = mxXMLReader.GetNodePath("/TvBusMarshalledStruct:TvBusEnvelope/showing/program/vAdvisory/*");
                                                while (xpathINodeDetails.MoveNext())
                                                {
                                                    sAdvisory = sAdvisory + (sAdvisory.Length > 0 ? "," : "") + xpathINodeDetails.Current.Value.Replace("_", " ");
                                                }
                                                String[] sParentalRatingReason = sAdvisory.Split(',');
                                                String sRatingAdvisoryAddon = "";
                                                for (int j = 0; j < sParentalRatingReason.Length; j++)
                                                {
                                                    for (int i = 0; i < sAdvisoryList.Length; i = i + 2)
                                                    {
                                                        if (sParentalRatingReason[j].ToLower().Trim().Equals(sAdvisoryList[i + 1].ToLower()))
                                                        {
                                                            sRatingAdvisoryAddon = sRatingAdvisoryAddon + (sRatingAdvisoryAddon.Length > 0 ? ";" : "") + sAdvisoryList[i];
                                                        }
                                                    }
                                                }
                                                hRecord.Add("Advisory", sRatingAdvisoryAddon);

                                                // Get movie year...
                                                xpathINodeDetails = mxXMLReader.GetNodePath("/TvBusMarshalledStruct:TvBusEnvelope/showing/program/movieYear");
                                                xpathINodeDetails.MoveNext();
                                                hRecord.Add("MovieYear", (xpathINodeDetails.Current.Name.Equals("movieYear") ? xpathINodeDetails.Current.Value : ""));

                                                // Get movie rating...
                                                xpathINodeDetails = mxXMLReader.GetNodePath("/TvBusMarshalledStruct:TvBusEnvelope/showing/program/mpaaRating");
                                                xpathINodeDetails.MoveNext();
                                                if (xpathINodeDetails.Current.Name.Equals("mpaaRating"))
                                                    hRecord.Add("ParentalRating", xpathINodeDetails.Current.Value.Replace("_", ""));

                                                // Get tv rating...
                                                xpathINodeDetails = mxXMLReader.GetNodePath("/TvBusMarshalledStruct:TvBusEnvelope/showing/tvRating");
                                                xpathINodeDetails.MoveNext();
                                                if (xpathINodeDetails.Current.Name.Equals("tvRating"))
                                                {
                                                    if (hRecord["ParentalRating"] == null)
                                                        hRecord.Add("ParentalRating", "TV-" + xpathINodeDetails.Current.Value.Replace("_", ""));
                                                    //                                                else
                                                    //                                                {
                                                    //                                                    String sParentalRating = hRecord["ParentalRating"].ToString() + ";" + "TV-" + xpathINodeDetails.Current.Value.Replace("_", "");
                                                    //                                                    hRecord.Remove("ParentalRating");
                                                    //                                                   hRecord.Add("ParentalRating", sParentalRating);
                                                    //                                              }
                                                }

                                                // Get star rating...
                                                xpathINodeDetails = mxXMLReader.GetNodePath("/TvBusMarshalledStruct:TvBusEnvelope/showing/program/starRating");
                                                xpathINodeDetails.MoveNext();
                                                String sStarRating = (xpathINodeDetails.Current.Name.Equals("starRating") ? xpathINodeDetails.Current.Value.Replace("_", " ") : "");
                                                for (int i = 0; i < sStarRatingList.Length; i = i + 2)
                                                {
                                                    if (sStarRatingList[i].Equals(sStarRating))
                                                    {
                                                        hRecord.Add("StarRating", sStarRatingList[i + 1]);
                                                        break;
                                                    }
                                                }
                                                // Get start time...
                                                xpathINodeDetails = mxXMLReader.GetNodePath("/TvBusMarshalledStruct:TvBusEnvelope/startTime");
                                                xpathINodeDetails.MoveNext();
                                                if (xpathINodeDetails.Current.Name.Equals("startTime"))
                                                {
                                                    hRecord.Add("Recorded", GetDate(xpathINodeDetails.Current.Value));
                                                    hRecord.Add("StartTime", GetDate(xpathINodeDetails.Current.Value) + " (" + GetTime(xpathINodeDetails.Current.Value) + ")");
                                                    //hRecord.Add("StartTime", xpathINodeDetails.Current.Value.Replace("T", " (").Replace("Z", ")").Replace("-", "/"));
                                                }

                                                // Get start time...
                                                xpathINodeDetails = mxXMLReader.GetNodePath("/TvBusMarshalledStruct:TvBusEnvelope/stopTime");
                                                xpathINodeDetails.MoveNext();
                                                if (xpathINodeDetails.Current.Name.Equals("stopTime"))
                                                    hRecord.Add("StopTime", GetDate(xpathINodeDetails.Current.Value) + " (" + GetTime(xpathINodeDetails.Current.Value) + ")");

                                                //hRecord.Add("StopTime", xpathINodeDetails.Current.Value.Replace("T", " (").Replace("Z", ")").Replace("-","/"));
                                            }
                                            fiTempFileInfo = new FileInfo("temp-" + sTempDataFile);
                                            fiTempFileInfo.Delete();
                                        }
                                    }
                                    while (xpathINode.Current.MoveToNext());
                                    xpathINode.Current.MoveToParent();
                                }
                            }
                            while (xpathINode.Current.MoveToNext());
                            xpathINode.Current.MoveToParent();

                            if (hRecord["Title"] == null)
                                hRecord.Add("Title", "");
                            if (hRecord["ProgramId"] == null)
                                hRecord.Add("ProgramId", "");
                            if (hRecord["EpisodeTitle"] == null)
                                hRecord.Add("EpisodeTitle", "");
                            if (hRecord["EpisodeNumber"] == null)
                                hRecord.Add("EpisodeNumber", "");
                            if (hRecord["Duration"] == null)
                                hRecord.Add("Duration", "");
                            if (hRecord["Description"] == null)
                                hRecord.Add("Description", "");
                            if (hRecord["Channel"] == null)
                                hRecord.Add("Channel", "");
                            if (hRecord["StationName"] == null)
                                hRecord.Add("StationName", "");
                            if (hRecord["NetworkAffiliate"] == null)
                                hRecord.Add("NetworkAffiliate", "");
                            if (hRecord["IsHDContent"] == null)
                                hRecord.Add("IsHDContent", "");
                            if (hRecord["IsRecording"] == null)
                                hRecord.Add("IsRecording", "");
                            if (hRecord["VidoeSize"] == null)
                                hRecord.Add("VidoeSize", "");
                            if (hRecord["DownloadUrl"] == null)
                                hRecord.Add("DownloadUrl", "");
                            if (hRecord["Credits"] == null)
                                hRecord.Add("Credits", "");
                            if (hRecord["Genre"] == null)
                                hRecord.Add("Genre", "");
                            if (hRecord["Advisory"] == null)
                                hRecord.Add("Advisory", "");
                            if (hRecord["MovieYear"] == null)
                                hRecord.Add("MovieYear", "");
                            if (hRecord["ParentalRating"] == null)
                                hRecord.Add("ParentalRating", "");
                            if (hRecord["StarRating"] == null)
                                hRecord.Add("StarRating", "");
                            if (hRecord["StartTime"] == null)
                                hRecord.Add("StartTime", "");
                            if (hRecord["StopTime"] == null)
                                hRecord.Add("StopTime", "");
                            if (hRecord["TivoSuggestion"] == null)
                                hRecord.Add("TivoSuggestion", "");
                            if (hRecord["TitleStrongSearchKey"] == null)
                                hRecord.Add("TitleStrongSearchKey", "");
                            if (hRecord["EpisodeStrongSearchKey"] == null)
                                hRecord.Add("EpisodeStrongSearchKey", "");

                            if (nTotalChildren == 0)
                            {
                                lTotalSStorageUsedByScheduledRecordings = lTotalSStorageUsedByScheduledRecordings + Convert.ToInt64(hRecord["UFVidoeSize"]);
                                lTotalRecordingTimeOfScheduledRecordings = lTotalRecordingTimeOfScheduledRecordings + Convert.ToInt64(GetTimeInSeconds(hRecord["Duration"].ToString()));
                                hResultSet.Add(Convert.ToString(++nRowCnt), hRecord);
                            }
                        }
                    }
                }
                else
                {
                    break;
                }
                fiTempFileInfo = new FileInfo(sTempDataFile);
                fiTempFileInfo.Delete();
            }
            fiTempFileInfo = new FileInfo(sTempDataFile);
            fiTempFileInfo.Delete();
            if (nScheduledRecordings == 0) // Not set...
                nScheduledRecordings = nRowCnt;
            hResultSet.Add("ProcessCount", Convert.ToString(nRowCnt));
            hResultSet.Add("ScheduledRecordings", Convert.ToString(nScheduledRecordings));
            hResultSet.Add("TotalStoragedUsedByTivoSuggestions", pdamxUtility.FormatStorageSize(Convert.ToString(lTotalStoragedUsedByTivoSuggestions)));
            hResultSet.Add("TotalSStorageUsedByScheduledRecordings", pdamxUtility.FormatStorageSize(Convert.ToString(lTotalSStorageUsedByScheduledRecordings)));
            hResultSet.Add("TotalStorageUsed", pdamxUtility.FormatStorageSize(Convert.ToString(lTotalSStorageUsedByScheduledRecordings + lTotalStoragedUsedByTivoSuggestions)));
            hResultSet.Add("UFTotalStoragedUsedByTivoSuggestions", Convert.ToString(lTotalStoragedUsedByTivoSuggestions));
            hResultSet.Add("UFTotalSStorageUsedByScheduledRecordings", Convert.ToString(lTotalSStorageUsedByScheduledRecordings));
            hResultSet.Add("UFTotalStorageUsed", Convert.ToString(lTotalSStorageUsedByScheduledRecordings + lTotalStoragedUsedByTivoSuggestions));
            hResultSet.Add("TotalRecordingTimeOfTivoSuggestions", pdamxUtility.FormatSeconds(Convert.ToString(lTotalRecordingTimeOfTivoSuggestions)));
            hResultSet.Add("TotalRecordingTimeOfScheduledRecordings", pdamxUtility.FormatSeconds(Convert.ToString(lTotalRecordingTimeOfScheduledRecordings)));
            hResultSet.Add("TotalRecordingTime", pdamxUtility.FormatSeconds(Convert.ToString(lTotalRecordingTimeOfScheduledRecordings + lTotalRecordingTimeOfTivoSuggestions)));
            hResultSet.Add("UFTotalRecordingTimeOfTivoSuggestions", Convert.ToString(lTotalRecordingTimeOfTivoSuggestions));
            hResultSet.Add("UFTotalRecordingTimeOfScheduledRecordings", Convert.ToString(lTotalRecordingTimeOfScheduledRecordings));
            hResultSet.Add("UFTotalRecordingTime", Convert.ToString(lTotalRecordingTimeOfScheduledRecordings + lTotalRecordingTimeOfTivoSuggestions));
            return (hResultSet);
        }
        private long GetTimeInSeconds(String _sTime)
        {
            long lTime = 0;
            String[] sTime;

            if (_sTime == null)
                return (lTime);

            if (_sTime.Length == 0)
                return (lTime);

            sTime = _sTime.Split(':');

            lTime = ((Convert.ToInt64(sTime[0]) * 60) * 60)
                + (Convert.ToInt64(sTime[1]) * 60)
                + (Convert.ToInt64(sTime[2]));
            return (lTime);
        }
        public Hashtable SearchTivo(String _sSearchValue)
        {
            pdamxSearchKeyGen mxSearchKeyGen;
            String[] sMultiSearch;
            String[] sSearchValues;

            if (_sSearchValue == null)
                return (null);

            if (_sSearchValue.Trim().Length == 0)
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
                mxSearchKeyGen.GenerateKey(sMultiSearch[i].Replace("\"", "").Replace(":", " - "));
                sSearchValues[i] = mxSearchKeyGen.StrongKey;
            }
            if (TivoRDMSSearch) 
                return(SearchTivoRDMSDB(sSearchValues));
            else
                return (SearchTivoXMLDB(sSearchValues));
        }
        private Hashtable SearchTivoXMLDB(String [] _sSearchValues)
        {
            pdamxXMLReader mxXMLReader = null;
            XPathNodeIterator xpathINode;
            Hashtable hResultSet = new Hashtable();
            Hashtable hRecord;
            String sSearchCriteria = "";
            String sSearchValue;
            int nRecordCnt = 0;

            mxXMLReader = new pdamxXMLReader();
            mxXMLReader.Open(TivoXMLDB);
            mxXMLReader.AddNamespace("tivo", "http://www.pdamediax.com/tivonowplaying");

            for (int nSearches = 0; nSearches < _sSearchValues.Length; nSearches++)
            {
                sSearchValue = pdamxUtility.FilterSpecialChar(_sSearchValues[nSearches]).ToLower();
                if (sSearchValue.Equals("all"))
                    sSearchCriteria = "/tivo:TivoNowPlaying/tivo:ShowList/tivo:Show[starts-with(tivo:SearchAll,\"All\")]";
                else
                    sSearchCriteria = "/tivo:TivoNowPlaying/tivo:ShowList/tivo:Show[starts-with(tivo:TitleStrongSearchKey,\"" + sSearchValue + "\")]"
                        + " | /tivo:TivoNowPlaying/tivo:ShowList/tivo:Show[starts-with(tivo:EpisodeStrongSearchKey,\"" + sSearchValue + "\")]";

                xpathINode = mxXMLReader.GetNodePath(sSearchCriteria);
                while (xpathINode.MoveNext())
                {
                    hRecord = new Hashtable();
                    xpathINode.Current.MoveToFirstChild();
                    do
                    {
                        hRecord.Add(xpathINode.Current.Name, xpathINode.Current.Value);
                    }
                    while (xpathINode.Current.MoveToNext());
                    hResultSet.Add(Convert.ToString(++nRecordCnt), hRecord);
                }
            }
            return (hResultSet);
        }
        private Hashtable SearchTivoRDMSDB(String[] _sSearchValues)
        {
            OracleCommand oCmd;
            OracleDataReader odrReader;
            Hashtable hResultSet = new Hashtable();
            Hashtable hRecord;
            String sSearchValue;
            String sFilter;
            int nRecordCnt = 0;

            TivoDBConn.Open();
            oCmd = new OracleCommand();
            oCmd.Connection = TivoDBConn;

            for (int nSearches = 0; nSearches < _sSearchValues.Length; nSearches++)
            {
                sSearchValue = _sSearchValues[nSearches];
                if (sSearchValue.Equals("all"))
                {
                    sFilter = "where tnp_search_all = 'All'";
                }
                else
                {
                    sFilter = "where tnp_title_strong_key like '" + sSearchValue + "%'";
                }
                if (TivoSeries != 0)
                    sFilter = sFilter + " and tnp_series_model = " + Convert.ToString(TivoSeries);

                if (!sSearchValue.Equals("all")) { 
                    sFilter = sFilter + " union all"
                        + " select * from v1mx_tivo_now_playing"
                        + " where tnp_episode_title_strong_key like '" + sSearchValue + "%'";               
                    if (TivoSeries != 0)
                        sFilter = sFilter + " and tnp_series_model = " + Convert.ToString(TivoSeries);
                }
                sFilter = sFilter + " order by 3 asc, 22 desc";
                oCmd.CommandText = "select * from v1mx_tivo_now_playing " + sFilter;
                oCmd.CommandType = CommandType.Text;
                odrReader = oCmd.ExecuteReader();
                while (odrReader.Read())
                {
                    hRecord = new Hashtable();
                    hRecord.Add("SeriesType", odrReader.GetValue(1));
                    hRecord.Add("Title", odrReader.GetValue(2));
                    hRecord.Add("EpisodeTitle", odrReader.GetValue(4).ToString().Trim());
                    hRecord.Add("EpisodeNumber", odrReader.GetValue(5).ToString().Trim());
                    hRecord.Add("ProgramId", odrReader.GetValue(6).ToString().Trim());
                    hRecord.Add("Description", odrReader.GetValue(7).ToString().Trim());
                    hRecord.Add("Credits", odrReader.GetValue(8).ToString().Trim());
                    hRecord.Add("Genre", odrReader.GetValue(9).ToString().Trim());
                    hRecord.Add("MovieYear", odrReader.GetValue(10).ToString().Trim());
                    hRecord.Add("Channel", odrReader.GetValue(11).ToString().Trim());
                    hRecord.Add("StationName", odrReader.GetValue(12).ToString().Trim());
                    hRecord.Add("NetworkAffiliate", odrReader.GetValue(13).ToString().Trim());
                    hRecord.Add("PlayTime", odrReader.GetValue(14).ToString().Trim());
                    hRecord.Add("UFPlayTime", odrReader.GetValue(15).ToString().Trim());
                    hRecord.Add("ParentalRating", odrReader.GetValue(16).ToString().Trim());
                    hRecord.Add("Advisory", odrReader.GetValue(17).ToString().Trim());
                    hRecord.Add("StarRating", odrReader.GetValue(18).ToString().Trim());
                    hRecord.Add("IsHDContent", (odrReader.GetValue(19).ToString().Trim().Equals("Y") ? "Yes" : "No"));
                    hRecord.Add("IsRecording", (odrReader.GetValue(20).ToString().Trim().Equals("Y") ? "Yes" : "No"));
                    hRecord.Add("Recorded", odrReader.GetValue(21).ToString().Trim());
                    hRecord.Add("StartTime", odrReader.GetValue(22).ToString().Trim());
                    hRecord.Add("StopTime", odrReader.GetValue(23).ToString().Trim());
                    hRecord.Add("TivoSuggestion", odrReader.GetValue(24).ToString().Trim());
                    hRecord.Add("VideoSize", odrReader.GetValue(25).ToString().Trim());
                    hRecord.Add("UFVideoSize", odrReader.GetValue(26).ToString().Trim());
                    hRecord.Add("DownloadUrl", odrReader.GetValue(27).ToString().Trim());
                    hRecord.Add("SearchAll", odrReader.GetValue(28).ToString().Trim());
                    hRecord.Add("TitleStrongSearchKey", odrReader.GetValue(29).ToString().Trim());
                    hRecord.Add("EpisodeStrongSearchKey", odrReader.GetValue(30).ToString().Trim());
                    hResultSet.Add(Convert.ToString(++nRecordCnt), hRecord);
                }
            }
            TivoDBConn.Close();
            return (hResultSet);
        }
        public String TivoXMLDB
        {
            get
            {
                return (sTivoXMLDB);
            }
            set
            {
                if (value != null)
                    if (value.Length > 0)
                        sTivoXMLDB = value;
            }
        }
        public OracleConnection TivoDBConn
        {
            get
            {
                return (odrDBConn);
            }
            set
            {
                if (value != null)
                    odrDBConn = value;
            }
        }
        public int TivoSeries
        {
            get
            {
                return (nTivoSeries);
            }
            set
            {  
                nTivoSeries = value;
            }
        }
        public bool TivoRDMSSearch
        {
            get
            {
                return (bDBSearch);
            }
            set
            {
                bDBSearch = value;
            }
        }       
	}
}
