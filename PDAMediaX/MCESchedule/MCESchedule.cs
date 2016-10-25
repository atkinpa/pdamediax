using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Net;
using System.Text;
using System.IO;
using pdaMediaX.Common;
using pdaMediaX.Util;
using pdaMediaX.Web;

namespace MCESchedule {
    using BTVServer;

    class MCESchedule : pdaMediaX.pdamxBatchJob
    {
        static void Main(string[] args)
        {
            new MCESchedule(args);
        }
        public MCESchedule(string[] args)
        {
            pdamxBeyondTV mxBeyondTV;
            DateTimeFormatInfo dtFormat = new CultureInfo("en-US", false).DateTimeFormat;
            pdamxSearchKeyGen mxSearchKeyGen;
            FileInfo fiFileSummary;
            Hashtable hProgramInfo;

            int iNumberOfsActiveRecordings = 0;
            int iNumberOfUpCommingShows = 0;

            String sBTVNetworkLicense = "";
            String sBTVNetworkLicenseFile = "";
            String sBTVUserAccessFile = "";
            String sWebGuideURL = "";
            String sWebGuideFile = "";
            String sXMLOutFile = "";
            String sTVGuideSearchUrl = "";
            String sTVComSearchUrl = "";
            String spubDate = "";

            String rootXMLNodeTemplate = 
                      "&[JobInfo]&"
                    + "\n   <ActiveRecordings>&[ActiveRecordings]&\n   </ActiveRecordings>"
                    + "\n   <NextRecording>&[NextRecording]&\n   </NextRecording>"
                    + "\n   <LastRecording>&[LastRecording]&\n   </LastRecording>"
                    + "\n   <UpcomingRecordings NumberOfShows=''>&[UpcommingRecordings]&\n   </UpcomingRecordings>";

            String jobInfoXMLTemplate =
                      "\n   <JobInfo>"
                    + "\n      <Generated></Generated>"
                    + "\n      <Generator></Generator>"
                    + "\n      <Machine></Machine>"
                    + "\n      <OS></OS>"
                    + "\n      <OSVersion></OSVersion>"
                    + "\n      <WebGuideServer>"
                    + "\n         <WGHost></WGHost>"
                    + "\n         <WGPort></WGPort>"
                    + "\n         <WGVersion></WGVersion>"
                    + "\n      </WebGuideServer>"
                    + "\n   </JobInfo>";

            String recordingsXMLTemplate =
                      "\n      <Show>"
                    + "\n         <Title></Title>"
                    + "\n         <EpisodeTitle></EpisodeTitle>"
                    + "\n         <Description></Description>"
                    + "\n         <Actors></Actors>"
                    + "\n         <Credits></Credits>"
                    + "\n         <Genre></Genre>"
                    + "\n         <Channel></Channel>"
                    + "\n         <StationCallSign></StationCallSign>"
                    + "\n         <StationName></StationName>"
                    + "\n         <TargetStartTime></TargetStartTime>"
                    + "\n         <TargetDuration></TargetDuration>"
                    + "\n         <ActualStartTime></ActualStartTime>"
                    + "\n         <ActualDuration></ActualDuration>"
                    + "\n         <OriginalAirDate></OriginalAirDate>"
                    + "\n         <Rating></Rating>"
                    + "\n         <MovieYear></MovieYear>"
                    + "\n         <IsHDBroadcast></IsHDBroadcast>"
                    + "\n         <IsHDRecording></IsHDRecording>"
                    + "\n         <IsEpisode></IsEpisode>"
                    + "\n         <TVGuideSearch></TVGuideSearch>"
                    + "\n         <TVDotComSearch></TVDotComSearch>"
                    + "\n         <TitleStrongSearchKey></TitleStrongSearchKey>"
                    + "\n         <EpisodeStrongSearchKey></EpisodeStrongSearchKey>"
                    + "\n      </Show>";

            // Load XML templates into memory...
            XMLWriter.LoadXMLTemplate("rootXMLNodeTemplate", rootXMLNodeTemplate);
            XMLWriter.LoadXMLTemplate("jobInfoXMLTemplate", jobInfoXMLTemplate);
            XMLWriter.LoadXMLTemplate("recordingsXMLTemplate", recordingsXMLTemplate);
            XMLWriter.CopyXMLTemplate("rootXMLNodeTemplate");
            XMLWriter.CopyXMLTemplate("jobInfoXMLTemplate");

            // Check if content should be written to a file...
            if (args.Length > 0)
                sXMLOutFile = args[0];

            // Configuration based on file name + Config.xml automatically loaded...
            // Get BeyondTV network license...
            sBTVNetworkLicenseFile = GetSettings("/MCETV/BeyondTV/License/NetworkLicense");

            if (sBTVNetworkLicenseFile.ToLower().Contains(".edf"))
                sBTVNetworkLicense = Crypter.DecryptFile(sBTVNetworkLicenseFile);
            else
                sBTVNetworkLicense = sBTVNetworkLicenseFile;

            // Get BeyondTV user account...
            sBTVUserAccessFile = GetSettings("/MCETV/BeyondTV/License/User");

            // Get WebGuide url...
            sWebGuideURL = GetSettings("/MCETV/WebGuide/Url");

            // Get WebGuide data file name...
            sWebGuideFile = GetSettings("/MCETV/WebGuide/TempDataFile");

            // Get sechdule output file name...
            if (sXMLOutFile.Length == 0) //Not overriden by file name passed to program...
            {
                if (GetSettings("/MCETV/Schedule/FileWriteEnabled").ToUpper().Equals("YES"))
                {
                    sXMLOutFile = GetSettings("/MCETV/Schedule/ScheduleFile");
                }
            }
            // Get Google search Url...
            sTVGuideSearchUrl = GetSettings("/MCETV/SearchEngines/TVGuideUrl");

            // Get MSN Bing search Url...
            sTVComSearchUrl = GetSettings("/MCETV/SearchEngines/TVCOMUrl");

            // Get TV schedule for MCE from WebGuide services...
            mxBeyondTV = new pdamxBeyondTV(sBTVNetworkLicense, sBTVUserAccessFile);
            getScheduleFromWebGuide(sWebGuideURL, sWebGuideFile);
            XMLReader.Open(sWebGuideFile);
            XMLWriter.SetXMLTemplateElement("jobInfoXMLTemplate", "WGVersion", XMLReader.GetNodeValue("/rss/channel/generator"));
            XMLWriter.SetXMLTemplateElement("jobInfoXMLTemplate", "WGHost", sWebGuideURL.Substring(sWebGuideURL.LastIndexOf("//") + 2, sWebGuideURL.LastIndexOf(":") - (sWebGuideURL.LastIndexOf("//") + 2)));
            XMLWriter.SetXMLTemplateElement("jobInfoXMLTemplate", "WGPort", sWebGuideURL.Substring(sWebGuideURL.LastIndexOf(":") + 1, sWebGuideURL.LastIndexOf(":", (sWebGuideURL.LastIndexOf("/") + 1) - (sWebGuideURL.LastIndexOf(":") + 1))));

            // Get Schedule Information...
            XMLReader.GetNodePath("/rss/channel/item/*");
            String sGenre = "";
            String sTitle = "";
            String sEpisodeTitle = "";
            bool bActiveRecording = false;
            XMLWriter.CopyXMLTemplate("recordingsXMLTemplate");
            while (XMLReader.MoveNext())
            {
                if (XMLReader.GetXPathNodeIterator().Current.Name.Equals("title"))
                {
                    sEpisodeTitle = "";
                    sTitle = XMLReader.GetXPathNodeIterator().Current.Value;
                    if (sTitle.Contains(": \""))
                    {
                        int nStartIdx = sTitle.IndexOf(": \"") + 1;
                        sEpisodeTitle = sTitle.Substring(nStartIdx + 2, sTitle.Length - (nStartIdx + 3));
                        sTitle = sTitle.Substring(0, sTitle.IndexOf(": \""));
                    }
                    if (iNumberOfUpCommingShows > 0)
                    {
                        XMLWriter.SetXMLTemplateElement("recordingsXMLTemplate", "Genre", sGenre);
                        XMLWriter.InsertXMLAtTemplateElementMarker("rootXMLNodeTemplate", "UpcommingRecordings", "recordingsXMLTemplate");
                        if (bActiveRecording)
                        {
                            bActiveRecording = false;
                            XMLWriter.InsertXMLAtTemplateElementMarker("rootXMLNodeTemplate", "ActiveRecordings", "recordingsXMLTemplate");
                            iNumberOfsActiveRecordings++;
                        }
                        if ((iNumberOfUpCommingShows == 1) && (iNumberOfsActiveRecordings == 0))
                            XMLWriter.InsertXMLAtTemplateElementMarker("rootXMLNodeTemplate", "NextRecording", "recordingsXMLTemplate");
                        else if ((iNumberOfUpCommingShows == 2) && (XMLWriter.GetXMLTemplate("NextRecording") == null))
                            XMLWriter.InsertXMLAtTemplateElementMarker("rootXMLNodeTemplate", "NextRecording", "recordingsXMLTemplate");
                    }
                    sGenre = "";
                    XMLWriter.CopyXMLTemplate("recordingsXMLTemplate");
                    mxSearchKeyGen = new pdamxSearchKeyGen();
                    mxSearchKeyGen.GenerateKey(sTitle);
                    XMLWriter.SetXMLTemplateElement("recordingsXMLTemplate", "Title", sTitle);
                    XMLWriter.SetXMLTemplateElement("recordingsXMLTemplate", "TitleStrongSearchKey", mxSearchKeyGen.StrongKey);
                    XMLWriter.SetXMLTemplateElement("recordingsXMLTemplate", "TVGuideSearch", sTVGuideSearchUrl + XMLReader.GetXPathNodeIterator().Current.Value);
                    XMLWriter.SetXMLTemplateElement("recordingsXMLTemplate", "TVDotComSearch", sTVComSearchUrl + XMLReader.GetXPathNodeIterator().Current.Value);

                    if (sEpisodeTitle.Length > 0)
                    {
                        mxSearchKeyGen.GenerateKey(sEpisodeTitle);
                        XMLWriter.SetXMLTemplateElement("recordingsXMLTemplate", "EpisodeTitle", sEpisodeTitle);
                        XMLWriter.SetXMLTemplateElement("recordingsXMLTemplate", "EpisodeStrongSearchKey", mxSearchKeyGen.StrongKey);
                    }
                    iNumberOfUpCommingShows++;
                }
                if (XMLReader.GetXPathNodeIterator().Current.Name.Equals("description"))
                    XMLWriter.SetXMLTemplateElement("recordingsXMLTemplate", "Description", XMLReader.GetXPathNodeIterator().Current.Value);
                if (XMLReader.GetXPathNodeIterator().Current.Name.Equals("pubDate"))
                {
                    int result = DateTime.Compare(Convert.ToDateTime(XMLReader.GetXPathNodeIterator().Current.Value), System.DateTime.Now);
                    if (result < 0)
                        bActiveRecording = true;
                    spubDate = XMLReader.GetXPathNodeIterator().Current.Value;
                }
                if (XMLReader.GetXPathNodeIterator().Current.Name.Equals("dc:creator"))
                {
                    String sChannel = XMLReader.GetXPathNodeIterator().Current.Value.Substring(0, XMLReader.GetXPathNodeIterator().Current.Value.IndexOf(" "));
                    String sStationCallSign = XMLReader.GetXPathNodeIterator().Current.Value.Substring(XMLReader.GetXPathNodeIterator().Current.Value.IndexOf(" ") + 1, (XMLReader.GetXPathNodeIterator().Current.Value.IndexOf("-") - 1) - XMLReader.GetXPathNodeIterator().Current.Value.IndexOf(" "));
                    String sStationName = XMLReader.GetXPathNodeIterator().Current.Value.Substring(XMLReader.GetXPathNodeIterator().Current.Value.IndexOf("-") + 2, (XMLReader.GetXPathNodeIterator().Current.Value.Length - XMLReader.GetXPathNodeIterator().Current.Value.IndexOf("-")) - 2);

                    hProgramInfo = GetBTVProgramInfo(mxBeyondTV.SearchGuideAll(sTitle.Trim()), spubDate, sChannel);
                    if (hProgramInfo != null)
                    {
                        XMLWriter.SetXMLTemplateElement("recordingsXMLTemplate", "Actors", (hProgramInfo["Actors"] != null ? hProgramInfo["Actors"].ToString() : ""));
                        XMLWriter.SetXMLTemplateElement("recordingsXMLTemplate", "Credits", (hProgramInfo["Credits"] != null ? hProgramInfo["Credits"].ToString() : ""));
                        XMLWriter.SetXMLTemplateElement("recordingsXMLTemplate", "Genre", (hProgramInfo["Genre"] != null ? hProgramInfo["Genre"].ToString() : ""));
                        XMLWriter.SetXMLTemplateElement("recordingsXMLTemplate", "TargetStartTime", DateTime.FromFileTime(Convert.ToInt64(hProgramInfo["Start"].ToString())).ToString("MM/dd/yyyy (hh:mm:ss tt)", dtFormat));
                        XMLWriter.SetXMLTemplateElement("recordingsXMLTemplate", "TargetDuration", pdamxUtility.FormatNanoseconds(hProgramInfo["Duration"].ToString()));
                        XMLWriter.SetXMLTemplateElement("recordingsXMLTemplate", "ActualStartTime", DateTime.FromFileTime(Convert.ToInt64(hProgramInfo["Start"].ToString())).ToString("MM/dd/yyyy (hh:mm:ss tt)", dtFormat));
                        XMLWriter.SetXMLTemplateElement("recordingsXMLTemplate", "ActualDuration", pdamxUtility.FormatNanoseconds(hProgramInfo["Duration"].ToString()));

                        if (hProgramInfo["OriginalAirDate"] == null)
                            XMLWriter.SetXMLTemplateElement("recordingsXMLTemplate", "OriginalAirDate", "");
                        else
                        {
                            if (hProgramInfo["OriginalAirDate"].ToString() == "")
                            {
                                XMLWriter.SetXMLTemplateElement("recordingsXMLTemplate", "OriginalAirDate", "");
                            }
                            else
                            {
                                DateTime startTime = new DateTime(Convert.ToInt32(hProgramInfo["OriginalAirDate"].ToString().Substring(0, 4)),
                                Convert.ToInt32(hProgramInfo["OriginalAirDate"].ToString().Substring(4, 2)),
                                Convert.ToInt32(hProgramInfo["OriginalAirDate"].ToString().Substring(6, 2)));
                                XMLWriter.SetXMLTemplateElement("recordingsXMLTemplate", "OriginalAirDate", startTime.ToString("d", dtFormat));
                            }
                        }
                        XMLWriter.SetXMLTemplateElement("recordingsXMLTemplate", "OriginalAirDate", (hProgramInfo["OriginalAirDate"] != null ? hProgramInfo["OriginalAirDate"].ToString() : ""));
                        XMLWriter.SetXMLTemplateElement("recordingsXMLTemplate", "Rating", (hProgramInfo["Rating"] != null ? hProgramInfo["Rating"].ToString() : ""));
                        XMLWriter.SetXMLTemplateElement("recordingsXMLTemplate", "MovieYear", (hProgramInfo["MovieYear"] != null ? hProgramInfo["MovieYear"].ToString() : ""));
                        XMLWriter.SetXMLTemplateElement("recordingsXMLTemplate", "IsHDBroadcast", (hProgramInfo["IsHD"] != null ? (hProgramInfo["IsHD"].ToString() == "Y" ? "Yes" : "No") : "No"));
                        XMLWriter.SetXMLTemplateElement("recordingsXMLTemplate", "IsHDRecording", "No");
                        XMLWriter.SetXMLTemplateElement("recordingsXMLTemplate", "IsEpisode", (hProgramInfo["EpisodeDescription"] != null ? "Yes" : "No"));
                    }
                    else
                    {
                        DateTime startTime = Convert.ToDateTime(spubDate);
                        XMLWriter.SetXMLTemplateElement("recordingsXMLTemplate", "Actors", "");
                        XMLWriter.SetXMLTemplateElement("recordingsXMLTemplate", "Credits", "");
                        XMLWriter.SetXMLTemplateElement("recordingsXMLTemplate", "Genre", sGenre);
                        XMLWriter.SetXMLTemplateElement("recordingsXMLTemplate", "TargetStartTime", startTime.ToString("MM/dd/yyyy (hh:mm:ss tt)", dtFormat));
                        XMLWriter.SetXMLTemplateElement("recordingsXMLTemplate", "TargetDuration", "");
                        XMLWriter.SetXMLTemplateElement("recordingsXMLTemplate", "ActualStartTime", startTime.ToString("MM/dd/yyyy (hh:mm:ss tt)", dtFormat));
                        XMLWriter.SetXMLTemplateElement("recordingsXMLTemplate", "ActualDuration", "");
                        XMLWriter.SetXMLTemplateElement("recordingsXMLTemplate", "OriginalAirDate", "");
                        XMLWriter.SetXMLTemplateElement("recordingsXMLTemplate", "Rating", "");
                        XMLWriter.SetXMLTemplateElement("recordingsXMLTemplate", "MovieYear", "");
                        XMLWriter.SetXMLTemplateElement("recordingsXMLTemplate", "IsHDBroadcast", "No");
                        XMLWriter.SetXMLTemplateElement("recordingsXMLTemplate", "IsHDRecording", "No");
                        XMLWriter.SetXMLTemplateElement("recordingsXMLTemplate", "IsEpisode", "No");
                    }
                    XMLWriter.SetXMLTemplateElement("recordingsXMLTemplate", "Channel", sChannel.Trim());
                    XMLWriter.SetXMLTemplateElement("recordingsXMLTemplate", "StationCallSign", sStationCallSign.Trim());
                    XMLWriter.SetXMLTemplateElement("recordingsXMLTemplate", "StationName", sStationName.Trim());
                }
                if (XMLReader.GetXPathNodeIterator().Current.Name.Equals("category"))
                {
                    if (sGenre != "")
                        sGenre = sGenre + "/";
                    sGenre = sGenre + XMLReader.GetXPathNodeIterator().Current.Value.Trim();
                }
            }
            // Delete WebGuide data file...
            FileInfo fiTempFileInfo = new FileInfo(sWebGuideFile);
            fiTempFileInfo.Delete();

            // Get last recording...
            XMLWriter.SetXMLTemplateElement("recordingsXMLTemplate", "Genre", sGenre);
            XMLWriter.InsertXMLAtTemplateElementMarker("rootXMLNodeTemplate", "LastRecording", "recordingsXMLTemplate");

            //Write XML content to console stream or file...
            XMLWriter.SetXMLTemplateElement("jobInfoXMLTemplate", "Generated", StartTime);
            XMLWriter.SetXMLTemplateElement("jobInfoXMLTemplate", "Generator", Program);
            XMLWriter.SetXMLTemplateElement("jobInfoXMLTemplate", "Machine", Machine);
            XMLWriter.SetXMLTemplateElement("jobInfoXMLTemplate", "OS", OS);
            XMLWriter.SetXMLTemplateElement("jobInfoXMLTemplate", "OSVersion", OSVersion);

            XMLWriter.SetXMLTemplateElementAttribute("rootXMLNodeTemplate", "UpcomingRecordings", "NumberOfShows", Convert.ToString(iNumberOfUpCommingShows));
            XMLWriter.ReplactXMPTemplateElementMarker("rootXMLNodeTemplate", "JobInfo", "jobInfoXMLTemplate");

            if (sXMLOutFile != "") //Write to file...
            {
                XMLWriter.Open(sXMLOutFile);
                XMLWriter.RootNode = "MCETV";
                XMLWriter.DTD = "DTD/" + pdamxUtility.StripPath(sXMLOutFile, true);
                XMLWriter.Namespace = "http://www.pdamediax.com/mce";
                XMLWriter.Write(XMLWriter.GetXMLTemplate("rootXMLNodeTemplate"));
                XMLWriter.Close();
            }
            else
            {
                Console.WriteLine(XMLWriter.GetXMLTemplate("rootXMLNodeTemplate"));
            }
            WriteEndofJobSummaryToFile = true;
            AddSummaryExtra("");
            AddSummaryExtra("MCE Scheduling Summary");
            AddSummaryExtra("");
            AddSummaryExtra("  Number of Up Comming Shows:  " + pdamxUtility.FormatNumber(iNumberOfUpCommingShows));
            AddSummaryExtra("  Number of Active Shows:      " + pdamxUtility.FormatNumber(iNumberOfsActiveRecordings));

            if (sXMLOutFile != "") //Write to file...
            {
                fiFileSummary = new FileInfo(sXMLOutFile);
                AddSummaryExtra("");
                AddSummaryExtra("MCE XML Schedule Data File Information");
                AddSummaryExtra("");
                AddSummaryExtra("  Name:      " + fiFileSummary.Name);
                AddSummaryExtra("  Location:  " + fiFileSummary.Directory);
                AddSummaryExtra("  Size:      " + pdamxUtility.FormatStorageSize(Convert.ToString(fiFileSummary.Length)));
                AddSummaryExtra("  Created:   " + fiFileSummary.LastWriteTime.ToString("MM/dd/yyyy (hh:mm:ss tt)", dtFormat));
            }
            PrintEndofJobSummary();
            return;
        }
        private void getScheduleFromWebGuide(String sUrl, String sWebGuideFile)
        {
            WebClient wbClient;
            Stream stData;
            StreamReader srReader;

            try
            {
                wbClient = new WebClient();
                wbClient.Headers.Add("user-agent", "Mozilla/4.0 (compatible; MSIE 6.0; Windows NT 5.2; .NET CLR 1.0.3705;)");

                stData = wbClient.OpenRead(sUrl);
                srReader = new StreamReader(stData);
                TextWriter tw = new StreamWriter(sWebGuideFile);
                tw.Write(srReader.ReadToEnd().Replace("&", "&amp;"));
                tw.Close();
                srReader.Close();
            }
            catch (Exception)
            { }
        }
        private Hashtable GetBTVProgramInfo(Hashtable _hSearchResult, String _sProgramDate, String _sChannel)
        {
            Hashtable hRecord;
            DateTimeFormatInfo dtfiTimeFormat = new CultureInfo("en-US", false).DateTimeFormat;

            if (_hSearchResult.Count > 0)
            {
                for (int i = 1; i <= _hSearchResult.Count; i++)
                {
                    hRecord = (Hashtable)_hSearchResult[Convert.ToString(i)];
                    DateTime dtBTVStartTime = DateTime.FromFileTime(Convert.ToInt64(hRecord["Start"]));
                    DateTime dtMCEStartTime = Convert.ToDateTime(_sProgramDate);
                    String sChannel = (hRecord["TMSChannel"] != null ? hRecord["TMSChannel"].ToString() : "");

                    if ((dtBTVStartTime.ToString("MM/dd/yyyy, hh", dtfiTimeFormat).Equals(dtMCEStartTime.ToString("MM/dd/yyyy, hh", dtfiTimeFormat))))
                    {
                        int nBTVMinute = dtBTVStartTime.Minute;
                        int nMCEMinute = dtMCEStartTime.Minute;

                        if ((nMCEMinute >= nBTVMinute) && (nMCEMinute <= (nBTVMinute + 5)))
                            if (pdamxUtility.TrimLeadingZeros(sChannel).Equals(pdamxUtility.TrimLeadingZeros(_sChannel)))
                                return (hRecord);
                    }
                }
            }
            return (null);
        }
    }
}
