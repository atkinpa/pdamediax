using System;
using System.Globalization;
using System.IO;
using System.Text;
using pdaMediaX.Common;
using pdaMediaX.Util;
using pdaMediaX.Web;
using pdaMediaX.Media;

namespace BTVSchedule
{

    class BTVSchedule : pdaMediaX.pdamxBatchJob
    {
        String sTVGuideSearchUrl = "";
        String sTVComSearchUrl = "";

        [STAThread]
        static void Main(string[] args)
        {
            new BTVSchedule(args);
        }
        public BTVSchedule(string [] args)
        {
            BTVLicenseManager.BTVLicenseManager btvLicenseManager;
            BTVGuideUpdater.BTVGuideUpdater btvGuideUpdater;
            BTVDispatcher.BTVDispatcher btvDispatcher;
            BTVScheduler.BTVScheduler btvScheduler;

            DateTime dtDateTime;
            DateTimeFormatInfo dtFormat = new CultureInfo("en-US", false).DateTimeFormat;
            FileInfo fiFileSummary;

            int iNumberOfUpCommingShows = 0;
            int iNumberOfsActiveRecordings = 0;
            int iNumberOfNextShows = 0;
            bool bAllNextShowsFound = false;

            String sNetworkLicense = "";
            String sNetworkLicenseFile = "";
            String sUserAccessFile = "";
            String sUser = "";
            String sPassword = "";

            String sAuthTicket = "";
            String sUpcomingRecsTotalDuration = "";
            String sXMLOutFile = "";
            String sPreviousStartTime = "";

            String rootXMLNodeTemplate =
                      "&[JobInfo]&"
                    + "\n   <ActiveRecordings NumberOfShows=''>&[ActiveRecordings]&\n   </ActiveRecordings>"
                    + "\n   <NextRecording NumberOfShows=''>&[NextRecording]&\n   </NextRecording>"
                    + "\n   <LastRecording>&[LastRecording]&\n   </LastRecording>"
                    + "\n   <UpcomingRecordings NumberOfShows='' Duration=''>&[UpcommingRecordings]&\n   </UpcomingRecordings>";


            String jobInfoXMLTemplate =
                      "\n   <JobInfo>"
                    + "\n      <Generated></Generated>"
                    + "\n      <Generator></Generator>"
                    + "\n      <Machine></Machine>"
                    + "\n      <OS></OS>"
                    + "\n      <OSVersion></OSVersion>"
                    + "\n      <BTVServer>"
                    + "\n         <Host></Host>"
                    + "\n         <Port></Port>"
                    + "\n         <Version></Version>"
                    + "\n         <GuideUpdate>"
                    + "\n            <GuideLastUpdateAttempt></GuideLastUpdateAttempt>"
                    + "\n            <GuideNextUpdateAttempt></GuideNextUpdateAttempt>"
                    + "\n            <GuideLastSuccessfulUpdate></GuideLastSuccessfulUpdate>"
                    + "\n         </GuideUpdate>"
                    + "\n      </BTVServer>"
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

            // Test section...

           // pdamxBeyondTV mxBeyondTV = new pdamxBeyondTV(GetSettings("/BeyondTV/License/NetworkLicense"),
           //     GetSettings("/BeyondTV/License/User"));

            //mxBeyondTV.SearchGuide("Batman Beyond");
            //Console.ReadKey();
            //mxBeyondTV.GetProgramByEPGID("MV0068340000");

            // Check if content should be written to a file...
            if (args.Length > 0)
                sXMLOutFile = args[0];

            // Configuration based on file name + Config.xml automatically loaded...
            // Get BeyondTV network license...
            sNetworkLicenseFile = GetSettings("/BeyondTV/License/NetworkLicense");

            if (sNetworkLicenseFile.ToLower().Contains(".edf"))
                sNetworkLicense = Crypter.DecryptFile(sNetworkLicenseFile);
            else
                sNetworkLicense = sNetworkLicenseFile;

            // Get BeyondTV user account...
            sUserAccessFile = GetSettings("/BeyondTV/License/User");
            String[] sAccess;
            if (sUserAccessFile.ToLower().Contains(".edf"))
                sAccess = Crypter.DecryptFile(sUserAccessFile).Split('/');
            else
                sAccess = sUserAccessFile.Split('/');
            sUser = sAccess[0];
            sPassword = sAccess[1];

            // Get sechdule output file name...
            if (sXMLOutFile.Length == 0) //Not overriden by file name passed to program...
            {
                if (GetSettings("/BeyondTV/Schedule/FileWriteEnabled").ToUpper().Equals("YES"))
                {
                    sXMLOutFile = GetSettings("/BeyondTV/Schedule/ScheduleFile");
                }
            }
            // Get Google search Url...
            sTVGuideSearchUrl = GetSettings("/BeyondTV/SearchEngines/TVGuideUrl");

            // Get MSN Bing search Url...
            sTVComSearchUrl = GetSettings("/BeyondTV/SearchEngines/TVCOMUrl");

            // Log into BeyondTV Server ...
            try
            {
                btvLicenseManager = new BTVLicenseManager.BTVLicenseManager();
                BTVLicenseManager.PVSPropertyBag licenseInfo = btvLicenseManager.LogonRemote(sNetworkLicense, sUser, sPassword);
                foreach (BTVLicenseManager.PVSProperty prop in licenseInfo.Properties)
                {
                    if (prop.Name == "AuthTicket")
                        sAuthTicket = prop.Value;
                }
            }
            catch (Exception)
            {
                return;
            }
            XMLWriter.SetXMLTemplateElement("jobInfoXMLTemplate", "Version", btvLicenseManager.GetVersionNumber());
            string url = btvLicenseManager.Url;
            XMLWriter.SetXMLTemplateElement("jobInfoXMLTemplate", "Host", url.Substring(url.LastIndexOf("//") + 2, url.LastIndexOf(":") - (url.LastIndexOf("//") + 2)));
            XMLWriter.SetXMLTemplateElement("jobInfoXMLTemplate", "Port", url.Substring(url.LastIndexOf(":") + 1, url.LastIndexOf(":", (url.LastIndexOf("/") + 1) - (url.LastIndexOf(":") + 1))));

            // Get guide information...
            btvGuideUpdater = new BTVGuideUpdater.BTVGuideUpdater();
            dtDateTime = DateTime.FromFileTime(Convert.ToInt64(btvGuideUpdater.GetLastSuccessfulUpdate(sAuthTicket)));
            XMLWriter.SetXMLTemplateElement("jobInfoXMLTemplate", "GuideLastSuccessfulUpdate", dtDateTime.ToString("MM/dd/yyyy (hh:mm:ss tt)", dtFormat));
            dtDateTime = DateTime.FromFileTime(Convert.ToInt64(btvGuideUpdater.GetLastAttemptedUpdate(sAuthTicket)));
            XMLWriter.SetXMLTemplateElement("jobInfoXMLTemplate", "GuideLastUpdateAttempt", dtDateTime.ToString("MM/dd/yyyy (hh:mm:ss tt)", dtFormat));
            dtDateTime = DateTime.FromFileTime(Convert.ToInt64(btvGuideUpdater.GetNextAttemptedUpdate(sAuthTicket)));
            XMLWriter.SetXMLTemplateElement("jobInfoXMLTemplate", "GuideNextUpdateAttempt", dtDateTime.ToString("MM/dd/yyyy (hh:mm:ss tt)", dtFormat));

            //Get active recordings...
            btvDispatcher = new BTVDispatcher.BTVDispatcher();
            BTVDispatcher.PVSPropertyBag[] activeRecs = btvDispatcher.GetActiveRecordings(sAuthTicket);
            foreach (BTVDispatcher.PVSPropertyBag rec in activeRecs)
            {
                XMLWriter.CopyXMLTemplate("recordingsXMLTemplate");
                getRecordingInfo(rec, "recordingsXMLTemplate");
                XMLWriter.InsertXMLAtTemplateElementMarker("rootXMLNodeTemplate", "ActiveRecordings", "recordingsXMLTemplate");
                iNumberOfsActiveRecordings++;
            }

            // Get upcoming recordings...
            btvScheduler = new BTVScheduler.BTVScheduler();
            BTVScheduler.PVSPropertyBag[] upcomingRecs = btvScheduler.GetUpcomingRecordings(sAuthTicket);
            foreach (BTVScheduler.PVSPropertyBag rec in upcomingRecs)
            {
                XMLWriter.CopyXMLTemplate("recordingsXMLTemplate");
                getRecordingInfo(rec, "recordingsXMLTemplate");
                XMLWriter.InsertXMLAtTemplateElementMarker("rootXMLNodeTemplate", "UpcommingRecordings", "recordingsXMLTemplate");

                if (bAllNextShowsFound) // Only contine to code below if all next shows haven't been processed...
                {
                    iNumberOfUpCommingShows++;
                    continue;
                }
                if (iNumberOfUpCommingShows >= iNumberOfsActiveRecordings) // Set next recordings (skip active recordings)...
                {
                    string startTime = "";
                    foreach (BTVScheduler.PVSProperty prop in rec.Properties)
                    {
                        if (prop.Name == "TargetStart")
                        {
                            DateTime dtStartTime = DateTime.FromFileTime(Convert.ToInt64(prop.Value.Trim()));
                            startTime = dtStartTime.ToString("MM/dd/yyyy (hh:mm:ss tt)", dtFormat);
                            break;
                        }
                    }
                    if ((startTime == sPreviousStartTime)
                        || (iNumberOfUpCommingShows == iNumberOfsActiveRecordings)) // More then one show schedule at the same time...
                    {
                        XMLWriter.CopyXMLTemplate("recordingsXMLTemplate");
                        getRecordingInfo(rec, "recordingsXMLTemplate");
                        XMLWriter.InsertXMLAtTemplateElementMarker("rootXMLNodeTemplate", "NextRecording", "recordingsXMLTemplate");
                        sPreviousStartTime = startTime;
                        iNumberOfNextShows++;
                    }
                    else
//                    {
//                        if (iNumberOfUpCommingShows == iNumberOfsActiveRecordings)
//                        {
//                            XMLWriter.CopyXMLTemplate("recordingsXMLTemplate");
//                            getRecordingInfo(rec, XMLWriter, "recordingsXMLTemplate");
//                            XMLWriter.InsertXMLAtTemplateElementMarker("rootXMLNodeTemplate", "NextRecording", "recordingsXMLTemplate");
//                            sPreviousStartTime = startTime;
//                            iNumberOfNextShows++;
//                        }
//                        else
                        {
                            bAllNextShowsFound = true; // All next shows found...
                        }
//                    }
                }
                iNumberOfUpCommingShows++;
            }

            // Get upcoming recordings duration...
            sUpcomingRecsTotalDuration = pdamxUtility.FormatNanoseconds(Convert.ToString(btvScheduler.GetUpcomingRecordingDuration(sAuthTicket)));

            // Get last recording...
            XMLWriter.CopyXMLTemplate("recordingsXMLTemplate");
            getRecordingInfo(btvScheduler.GetLastRecording(sAuthTicket), "recordingsXMLTemplate");
            XMLWriter.InsertXMLAtTemplateElementMarker("rootXMLNodeTemplate", "LastRecording", "recordingsXMLTemplate");


            //Write XML content to console stream or file...
            XMLWriter.SetXMLTemplateElement("jobInfoXMLTemplate", "Generated", StartTime);
            XMLWriter.SetXMLTemplateElement("jobInfoXMLTemplate", "Generator", Program);
            XMLWriter.SetXMLTemplateElement("jobInfoXMLTemplate", "Machine", Machine);
            XMLWriter.SetXMLTemplateElement("jobInfoXMLTemplate", "OS", OS);
            XMLWriter.SetXMLTemplateElement("jobInfoXMLTemplate", "OSVersion", OSVersion);

            XMLWriter.SetXMLTemplateElementAttribute("rootXMLNodeTemplate", "ActiveRecordings", "NumberOfShows", Convert.ToString(iNumberOfsActiveRecordings));
            XMLWriter.SetXMLTemplateElementAttribute("rootXMLNodeTemplate", "UpcomingRecordings", "NumberOfShows", Convert.ToString(iNumberOfUpCommingShows));
            XMLWriter.SetXMLTemplateElementAttribute("rootXMLNodeTemplate", "NextRecording", "NumberOfShows", Convert.ToString(iNumberOfNextShows));
            XMLWriter.SetXMLTemplateElementAttribute("rootXMLNodeTemplate", "UpcomingRecordings", "Duration", sUpcomingRecsTotalDuration);
            XMLWriter.ReplactXMPTemplateElementMarker("rootXMLNodeTemplate", "JobInfo", "jobInfoXMLTemplate");
            btvLicenseManager.Logoff(sAuthTicket);

            if (sXMLOutFile != "") //Write to file...
            {
                XMLWriter.Open(sXMLOutFile);
                XMLWriter.RootNode = "BeyondTV";
                XMLWriter.DTD = "DTD/" + pdamxUtility.StripPath(sXMLOutFile, true);
                XMLWriter.Namespace = "http://www.pdamediax.com/btv";
                XMLWriter.Write(XMLWriter.GetXMLTemplate("rootXMLNodeTemplate"));
                XMLWriter.Close();
            }
            else
            {
                Console.WriteLine(XMLWriter.GetXMLTemplate("rootXMLNodeTemplate"));
            }
            WriteEndofJobSummaryToFile = true;
            AddSummaryExtra("");
            AddSummaryExtra("Beyond TV Scheduling Summary");
            AddSummaryExtra("");
            AddSummaryExtra("  Number of Up Comming Shows:  " + pdamxUtility.FormatNumber(iNumberOfUpCommingShows));
            AddSummaryExtra("  Number of Active Shows:      " + pdamxUtility.FormatNumber(iNumberOfsActiveRecordings));
            AddSummaryExtra("  Number of Next Shows:        " + pdamxUtility.FormatNumber(iNumberOfNextShows));

            if (sXMLOutFile != "") //Write to file...
            {
                fiFileSummary = new FileInfo(sXMLOutFile);
                AddSummaryExtra("");
                AddSummaryExtra("BeyondTV XML Schedule Data File Information");
                AddSummaryExtra("");
                AddSummaryExtra("  Name:      " + fiFileSummary.Name);
                AddSummaryExtra("  Location:  " + fiFileSummary.Directory);
                AddSummaryExtra("  Size:      " + pdamxUtility.FormatStorageSize(Convert.ToString(fiFileSummary.Length)));
                AddSummaryExtra("  Created:   " + fiFileSummary.LastWriteTime.ToString("MM/dd/yyyy (hh:mm:ss tt)", dtFormat));
            }
            PrintEndofJobSummary();
            return;
        }
        virtual public void getRecordingInfo(BTVScheduler.PVSPropertyBag rec, string xmlTemplate)
        {
            DateTimeFormatInfo timeFormat = new CultureInfo("en-US", false).DateTimeFormat;
            pdamxSearchKeyGen mxSearchKeyGen = new pdamxSearchKeyGen();
            string recordingInfo = xmlTemplate;
            string description = "";
            string episodeDescription = "";
            string stationCallSign = "";
            string isHDBroadcast = "";
            string isEpisode = "No";

            XMLWriter.CopyXMLTemplate(xmlTemplate);
            foreach (BTVScheduler.PVSProperty prop in rec.Properties)
            {
                if (prop.Name == "DisplayTitle")
                {
                    mxSearchKeyGen.GenerateKey(prop.Value.Trim());
                    XMLWriter.SetXMLTemplateElement("recordingsXMLTemplate", "Title", prop.Value.Trim());
                    XMLWriter.SetXMLTemplateElement("recordingsXMLTemplate", "TitleStrongSearchKey", mxSearchKeyGen.StrongKey);
                    XMLWriter.SetXMLTemplateElement("recordingsXMLTemplate", "TVGuideSearch", sTVGuideSearchUrl + prop.Value.Trim());
                    XMLWriter.SetXMLTemplateElement("recordingsXMLTemplate", "TVDotComSearch", sTVComSearchUrl + prop.Value.Trim());
                }
                if (prop.Name == "Channel")
                    XMLWriter.SetXMLTemplateElement("recordingsXMLTemplate", "Channel", prop.Value.Trim());
                if (prop.Name == "StationName")
                    XMLWriter.SetXMLTemplateElement("recordingsXMLTemplate", "StationName", prop.Value.Trim());
                if (prop.Name == "StationCallsign")
                {
                    XMLWriter.SetXMLTemplateElement("recordingsXMLTemplate", "StationCallSign", prop.Value.Trim());
                    stationCallSign = prop.Value.Trim().ToUpper();
                }
                if (prop.Name == "Actors")
                    XMLWriter.SetXMLTemplateElement("recordingsXMLTemplate", "Actors", prop.Value.Trim());
                if (prop.Name == "Credits")
                    XMLWriter.SetXMLTemplateElement("recordingsXMLTemplate", "Credits", prop.Value.Trim());
                if (prop.Name == "Genre")
                    XMLWriter.SetXMLTemplateElement("recordingsXMLTemplate", "Genre", prop.Value.Trim());
                if (prop.Name == "TargetStart")
                {
                    DateTime startTime = DateTime.FromFileTime(Convert.ToInt64(prop.Value.Trim()));
                    XMLWriter.SetXMLTemplateElement("recordingsXMLTemplate", "TargetStartTime", startTime.ToString("MM/dd/yyyy (hh:mm:ss tt)", timeFormat));
                }
                if (prop.Name == "ActualStart")
                {
                    DateTime startTime = DateTime.FromFileTime(Convert.ToInt64(prop.Value.Trim()));
                    XMLWriter.SetXMLTemplateElement("recordingsXMLTemplate", "ActualStartTime", startTime.ToString("MM/dd/yyyy (hh:mm:ss tt)", timeFormat));
                }
                if (prop.Name == "ActualDuration")
                    XMLWriter.SetXMLTemplateElement("recordingsXMLTemplate", "ActualDuration", pdamxUtility.FormatNanoseconds(prop.Value));
                if (prop.Name == "TargetDuration")
                    XMLWriter.SetXMLTemplateElement("recordingsXMLTemplate", "TargetDuration", pdamxUtility.FormatNanoseconds(prop.Value));
                if (prop.Name == "Description")
                    description = prop.Value.Trim();
                if (prop.Name == "EpisodeDescription")
                    episodeDescription = prop.Value.Trim();

                if (prop.Name == "EpisodeTitle")
                {
                    mxSearchKeyGen.GenerateKey(prop.Value.Trim());
                    XMLWriter.SetXMLTemplateElement("recordingsXMLTemplate", "EpisodeTitle", prop.Value.Trim());
                    XMLWriter.SetXMLTemplateElement("recordingsXMLTemplate", "EpisodeStrongSearchKey", mxSearchKeyGen.StrongKey);
                    //if (prop.Value.Trim() != "")
                    isEpisode = "Yes";
                }

                if (prop.Name == "OriginalAirDate")
                {
                    if (prop.Value.Trim() == "")
                    {
                        XMLWriter.SetXMLTemplateElement("recordingsXMLTemplate", "OriginalAirDate", "");
                    }
                    else
                    {
                        DateTime startTime = new DateTime(Convert.ToInt32(prop.Value.Substring(0, 4)),
                        Convert.ToInt32(prop.Value.Substring(4, 2)),
                        Convert.ToInt32(prop.Value.Substring(6, 2)));
                        XMLWriter.SetXMLTemplateElement("recordingsXMLTemplate", "OriginalAirDate", startTime.ToString("d", timeFormat));
                    }
                }
                if (prop.Name == "Rating")
                    XMLWriter.SetXMLTemplateElement("recordingsXMLTemplate", "Rating", prop.Value.Trim());
                if (prop.Name == "MovieYear")
                    XMLWriter.SetXMLTemplateElement("recordingsXMLTemplate", "MovieYear", prop.Value.Trim());
                if (prop.Name == "IsHD")
                    isHDBroadcast = prop.Value;
            }
            XMLWriter.SetXMLTemplateElement("recordingsXMLTemplate", "IsEpisode", isEpisode);
            if (episodeDescription.Equals(""))
                XMLWriter.SetXMLTemplateElement("recordingsXMLTemplate", "Description", description.ToLower());
            else
                XMLWriter.SetXMLTemplateElement("recordingsXMLTemplate", "Description", episodeDescription);
            if (stationCallSign.Equals("WCBS") | stationCallSign.Equals("WNBC")
                | stationCallSign.Equals("WNYW") | stationCallSign.Equals("WABC")
                | stationCallSign.Equals("WWOR") | stationCallSign.Equals("WPIX")
                | stationCallSign.Equals("WNET") | stationCallSign.Equals("WPXN"))
            {
                XMLWriter.SetXMLTemplateElement("recordingsXMLTemplate", "IsHDBroadcast", "Yes");
                XMLWriter.SetXMLTemplateElement("recordingsXMLTemplate", "IsHDRecording", "Yes");
            }
            else
            {
                XMLWriter.SetXMLTemplateElement("recordingsXMLTemplate", "IsHDBroadcast", (isHDBroadcast == "Y") ? "Yes" : "No");
                XMLWriter.SetXMLTemplateElement("recordingsXMLTemplate", "IsHDRecording", "No");
            }

        }
        virtual public void getRecordingInfo(BTVDispatcher.PVSPropertyBag rec, string xmlTemplate)
        {
            DateTimeFormatInfo timeFormat = new CultureInfo("en-US", false).DateTimeFormat;
            pdamxSearchKeyGen mxSearchKeyGen = new pdamxSearchKeyGen();
            string recordingInfo = xmlTemplate;
            string description = "";
            string episodeDescription = "";
            string stationCallSign = "";
            string isHDBroadcast = "";
            string isEpisode = "No";

            XMLWriter.CopyXMLTemplate(xmlTemplate);
            foreach (BTVDispatcher.PVSProperty prop in rec.Properties)
            {
                if (prop.Name == "DisplayTitle")
                {
                    mxSearchKeyGen.GenerateKey(prop.Value.Trim());
                    XMLWriter.SetXMLTemplateElement("recordingsXMLTemplate", "Title", prop.Value.Trim());
                    XMLWriter.SetXMLTemplateElement("recordingsXMLTemplate", "TitleStrongSearchKey", mxSearchKeyGen.StrongKey);
                    XMLWriter.SetXMLTemplateElement("recordingsXMLTemplate", "TVGuideSearch", sTVGuideSearchUrl + prop.Value.Trim());
                    XMLWriter.SetXMLTemplateElement("recordingsXMLTemplate", "TVDotComSearch", sTVComSearchUrl + prop.Value.Trim());
                }
                if (prop.Name == "Channel")
                    XMLWriter.SetXMLTemplateElement("recordingsXMLTemplate", "Channel", prop.Value.Trim());
                if (prop.Name == "StationName")
                    XMLWriter.SetXMLTemplateElement("recordingsXMLTemplate", "StationName", prop.Value.Trim());
                if (prop.Name == "StationCallsign")
                {
                    XMLWriter.SetXMLTemplateElement("recordingsXMLTemplate", "StationCallSign", prop.Value.Trim());
                    stationCallSign = prop.Value.Trim().ToUpper();
                }
                if (prop.Name == "Actors")
                    XMLWriter.SetXMLTemplateElement("recordingsXMLTemplate", "Actors", prop.Value.Trim());
                if (prop.Name == "Credits")
                    XMLWriter.SetXMLTemplateElement("recordingsXMLTemplate", "Credits", prop.Value.Trim());
                if (prop.Name == "Genre")
                    XMLWriter.SetXMLTemplateElement("recordingsXMLTemplate", "Genre", prop.Value.Trim());
                if (prop.Name == "TargetStart")
                {
                    DateTime startTime = DateTime.FromFileTime(Convert.ToInt64(prop.Value.Trim()));
                    XMLWriter.SetXMLTemplateElement("recordingsXMLTemplate", "TargetStartTime", startTime.ToString("MM/dd/yyyy (hh:mm:ss tt)", timeFormat));
                }
                if (prop.Name == "ActualStart")
                {
                    DateTime startTime = DateTime.FromFileTime(Convert.ToInt64(prop.Value.Trim()));
                    XMLWriter.SetXMLTemplateElement("recordingsXMLTemplate", "ActualStartTime", startTime.ToString("MM/dd/yyyy (hh:mm:ss tt)", timeFormat));
                }
                if (prop.Name == "ActualDuration")
                    XMLWriter.SetXMLTemplateElement("recordingsXMLTemplate", "ActualDuration", pdamxUtility.FormatNanoseconds(prop.Value));
                if (prop.Name == "TargetDuration")
                    XMLWriter.SetXMLTemplateElement("recordingsXMLTemplate", "TargetDuration", pdamxUtility.FormatNanoseconds(prop.Value));
                if (prop.Name == "Description")
                    description = prop.Value.Trim();
                if (prop.Name == "EpisodeDescription")
                    episodeDescription = prop.Value.Trim();

                if (prop.Name == "EpisodeTitle")
                {
                    mxSearchKeyGen.GenerateKey(prop.Value.Trim());
                    XMLWriter.SetXMLTemplateElement("recordingsXMLTemplate", "EpisodeTitle", prop.Value.Trim());
                    XMLWriter.SetXMLTemplateElement("recordingsXMLTemplate", "EpisodeStrongSearchKey", mxSearchKeyGen.StrongKey);
                    //if (prop.Value.Trim() != "")
                    isEpisode = "Yes";
                }

                if (prop.Name == "OriginalAirDate")
                {
                    if (prop.Value.Trim() == "")
                    {
                        XMLWriter.SetXMLTemplateElement("recordingsXMLTemplate", "OriginalAirDate", "");
                    }
                    else
                    {
                        DateTime startTime = new DateTime(Convert.ToInt32(prop.Value.Substring(0, 4)),
                        Convert.ToInt32(prop.Value.Substring(4, 2)),
                        Convert.ToInt32(prop.Value.Substring(6, 2)));
                        XMLWriter.SetXMLTemplateElement("recordingsXMLTemplate", "OriginalAirDate", startTime.ToString("d", timeFormat));
                    }
                }
                if (prop.Name == "Rating")
                    XMLWriter.SetXMLTemplateElement("recordingsXMLTemplate", "Rating", prop.Value.Trim());
                if (prop.Name == "MovieYear")
                    XMLWriter.SetXMLTemplateElement("recordingsXMLTemplate", "MovieYear", prop.Value.Trim());
                if (prop.Name == "IsHD")
                    isHDBroadcast = prop.Value;
            }
            XMLWriter.SetXMLTemplateElement("recordingsXMLTemplate", "IsEpisode", isEpisode);
            if (episodeDescription.Equals(""))
                XMLWriter.SetXMLTemplateElement("recordingsXMLTemplate", "Description", description.ToLower());
            else
                XMLWriter.SetXMLTemplateElement("recordingsXMLTemplate", "Description", episodeDescription);
            if (stationCallSign.Equals("WCBS") | stationCallSign.Equals("WNBC")
                | stationCallSign.Equals("WNYW") | stationCallSign.Equals("WABC")
                | stationCallSign.Equals("WWOR") | stationCallSign.Equals("WPIX")
                | stationCallSign.Equals("WNET") | stationCallSign.Equals("WPXN"))
            {
                XMLWriter.SetXMLTemplateElement("recordingsXMLTemplate", "IsHDBroadcast", "Yes");
                XMLWriter.SetXMLTemplateElement("recordingsXMLTemplate", "IsHDRecording", "Yes");
            }
            else
            {
                XMLWriter.SetXMLTemplateElement("recordingsXMLTemplate", "IsHDBroadcast", (isHDBroadcast == "Y") ? "Yes" : "No");
                XMLWriter.SetXMLTemplateElement("recordingsXMLTemplate", "IsHDRecording", "No");
            }

        }
    }
}
