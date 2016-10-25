using System;
using System.Collections.Generic;
using System.Collections;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml.XPath;
using pdaMediaX.Common;
using pdaMediaX.Util;
using pdaMediaX.Web;
using pdaMediaX.Util.Xml;

namespace TivoNowPlaying
{
    class TivoNowPlaying : pdaMediaX.pdamxBatchJob
    {
        static void Main(string[] args)
        {
            new TivoNowPlaying();
        }
        public TivoNowPlaying()
        {
            pdamxTivo mxTivo;
            Hashtable hResultSet;
            FileInfo fiFileInfo;
            String sNowPlayingFile;
            String sSettopName = "";

            String jobInfoXMLTemplate =
                      "\n   <JobInfo>"
                    + "\n      <Generated></Generated>"
                    + "\n      <Generator></Generator>"
                    + "\n      <Machine></Machine>"
                    + "\n      <OS></OS>"
                    + "\n      <OSVersion></OSVersion>"
                    + "\n   </JobInfo>";

            String summaryXMLTemplate =
                      "\n    <Summary>"
                    + "\n      <SettopName></SettopName>"
                    + "\n      <SettopIP></SettopIP>"
                    + "\n      <SettopUrl></SettopUrl>"
                    + "\n      <MediaKey></MediaKey>"
                    + "\n      <ScheduledRecordings></ScheduledRecordings>"
                    + "\n      <TotalItems></TotalItems>"
                    + "\n      <GlobalSort></GlobalSort>"
                    + "\n      <SortOrder></SortOrder>"
                    + "\n      <TotalStorageUsed></TotalStorageUsed>"
                    + "\n      <TotalStoragedUsedByTivoSuggestions></TotalStoragedUsedByTivoSuggestions>"
                    + "\n      <TotalSStorageUsedByScheduledRecordings></TotalSStorageUsedByScheduledRecordings>"
                    + "\n      <TotalRecordingTime></TotalRecordingTime>"
                    + "\n      <TotalRecordingTimeOfTivoSuggestions></TotalRecordingTimeOfTivoSuggestions>"
                    + "\n      <TotalRecordingTimeOfScheduledRecordings></TotalRecordingTimeOfScheduledRecordings>"
                    + "\n      <UFTotalStorageUsed></UFTotalStorageUsed>"
                    + "\n      <UFTotalStoragedUsedByTivoSuggestions></UFTotalStoragedUsedByTivoSuggestions>"
                    + "\n      <UFTotalSStorageUsedByScheduledRecordings></UFTotalSStorageUsedByScheduledRecordings>"
                    + "\n      <UFTotalRecordingTime></UFTotalRecordingTime>"
                    + "\n      <UFTotalRecordingTimeOfTivoSuggestions></UFTotalRecordingTimeOfTivoSuggestions>"
                    + "\n      <UFTotalRecordingTimeOfScheduledRecordings></UFTotalRecordingTimeOfScheduledRecordings>"
                    + "\n    </Summary>";

            String showXMLTemplate =
                      "\n    <Show>"
                    + "\n      <Title></Title>"
                    + "\n      <ProgramId></ProgramId>"
                    + "\n      <EpisodeTitle></EpisodeTitle>"
                    + "\n      <EpisodeNumber></EpisodeNumber>"
                    + "\n      <Description></Description>"
                    + "\n      <Credits></Credits>"
                    + "\n      <Genre></Genre>"
                    + "\n      <MovieYear></MovieYear>"
                    + "\n      <Channel></Channel>"
                    + "\n      <StationName></StationName>"
                    + "\n      <NetworkAffiliate></NetworkAffiliate>"
                    + "\n      <PlayTime></PlayTime>"
                    + "\n      <UFPlayTime></UFPlayTime>"
                    + "\n      <ParentalRating></ParentalRating>"
                    + "\n      <Advisory></Advisory>"
                    + "\n      <StarRating></StarRating>"
                    + "\n      <IsHDContent></IsHDContent>"
                    + "\n      <IsRecording></IsRecording>"
                    + "\n      <Recorded></Recorded>"
                    + "\n      <StartTime></StartTime>"
                    + "\n      <StopTime></StopTime>"
                    + "\n      <TivoSuggestion></TivoSuggestion>"
                    + "\n      <VidoeSize></VidoeSize>"
                    + "\n      <UFVidoeSize></UFVidoeSize>"
                    + "\n      <DownloadUrl></DownloadUrl>"
                    + "\n      <SearchAll>All</SearchAll>"
                    + "\n      <TitleStrongSearchKey></TitleStrongSearchKey>"
                    + "\n      <EpisodeStrongSearchKey></EpisodeStrongSearchKey>"
                    + "\n    </Show>";

            // Load XML templates into memory...
            XMLWriter.LoadXMLTemplate("jobInfoXMLTemplate", jobInfoXMLTemplate);
            XMLWriter.LoadXMLTemplate("summaryXMLTemplate", summaryXMLTemplate);
            XMLWriter.LoadXMLTemplate("showXMLTemplate", showXMLTemplate);

            sNowPlayingFile = GetSettings("/Tivo/NowPlayingFile");
            mxTivo = new pdamxTivo();

            hResultSet = mxTivo.GetTivoNowPlayingList(GetSettings("/Tivo/TivoBoxUrl").Replace("Container=", "Container1="),
                GetSettings("/Tivo/Credentals"),
                GetSettings("/Tivo/TempDataFile"));

            if (hResultSet != null)
            {
                /*
                if (hResultSet.Count > 0)
                {
                    Hashtable hRecord = (Hashtable)hResultSet["1"];
                    if (hRecord != null)
                    {
                        if (hRecord["Title"] != null)
                            sSettopName = hRecord["Title"].ToString();
                    }
                }
                 * */
                if (hResultSet["Title"] != null)
                    sSettopName = hResultSet["Title"].ToString();
            }
            hResultSet = mxTivo.GetTivoNowPlayingList(GetSettings("/Tivo/TivoBoxUrl"),
                GetSettings("/Tivo/Credentals"),
                GetSettings("/Tivo/TempDataFile"));

            if (hResultSet != null)
            {
                if (hResultSet.Count > 0)
                {
                    XMLWriter.Open("result-" + GetSettings("/Tivo/TempDataFile"));
                    XMLWriter.RootNode = "TivoNowPlaying";
                    XMLWriter.Namespace = "http://www.pdamediax.com/tivonowplaying";

                    XMLWriter.CopyXMLTemplate("jobInfoXMLTemplate");
                    XMLWriter.SetXMLTemplateElement("jobInfoXMLTemplate", "Generated", StartTime);
                    XMLWriter.SetXMLTemplateElement("jobInfoXMLTemplate", "Generator", Program);
                    XMLWriter.SetXMLTemplateElement("jobInfoXMLTemplate", "Machine", Machine);
                    XMLWriter.SetXMLTemplateElement("jobInfoXMLTemplate", "OS", OS);
                    XMLWriter.SetXMLTemplateElement("jobInfoXMLTemplate", "OSVersion", OSVersion);
                    XMLWriter.Write(XMLWriter.GetXMLTemplate("jobInfoXMLTemplate"));

                    XMLWriter.CopyXMLTemplate("summaryXMLTemplate");
                    XMLWriter.SetXMLTemplateElement("summaryXMLTemplate", "SettopName", sSettopName);
                    XMLWriter.SetXMLTemplateElement("summaryXMLTemplate", "SettopIP", hResultSet["SettopIP"].ToString());
                    XMLWriter.SetXMLTemplateElement("summaryXMLTemplate", "SettopUrl", hResultSet["SettopUrl"].ToString());
                    XMLWriter.SetXMLTemplateElement("summaryXMLTemplate", "MediaKey", hResultSet["MediaKey"].ToString());
                    XMLWriter.SetXMLTemplateElement("summaryXMLTemplate", "ScheduledRecordings", hResultSet["ScheduledRecordings"].ToString());
                    XMLWriter.SetXMLTemplateElement("summaryXMLTemplate", "TotalItems", hResultSet["ProcessCount"].ToString());
                    XMLWriter.SetXMLTemplateElement("summaryXMLTemplate", "GlobalSort", hResultSet["GlobalSort"].ToString());
                    XMLWriter.SetXMLTemplateElement("summaryXMLTemplate", "SortOrder", hResultSet["SortOrder"].ToString());
                    XMLWriter.SetXMLTemplateElement("summaryXMLTemplate", "TotalStorageUsed", hResultSet["TotalStorageUsed"].ToString());
                    XMLWriter.SetXMLTemplateElement("summaryXMLTemplate", "TotalStoragedUsedByTivoSuggestions", hResultSet["TotalStoragedUsedByTivoSuggestions"].ToString());
                    XMLWriter.SetXMLTemplateElement("summaryXMLTemplate", "TotalSStorageUsedByScheduledRecordings", hResultSet["TotalSStorageUsedByScheduledRecordings"].ToString());
                    XMLWriter.SetXMLTemplateElement("summaryXMLTemplate", "UFTotalStorageUsed", hResultSet["UFTotalStorageUsed"].ToString());
                    XMLWriter.SetXMLTemplateElement("summaryXMLTemplate", "UFTotalStoragedUsedByTivoSuggestions", hResultSet["UFTotalStoragedUsedByTivoSuggestions"].ToString());
                    XMLWriter.SetXMLTemplateElement("summaryXMLTemplate", "UFTotalSStorageUsedByScheduledRecordings", hResultSet["UFTotalSStorageUsedByScheduledRecordings"].ToString());
                    XMLWriter.SetXMLTemplateElement("summaryXMLTemplate", "TotalRecordingTime", hResultSet["TotalRecordingTime"].ToString());
                    XMLWriter.SetXMLTemplateElement("summaryXMLTemplate", "TotalRecordingTimeOfTivoSuggestions", hResultSet["TotalRecordingTimeOfTivoSuggestions"].ToString());
                    XMLWriter.SetXMLTemplateElement("summaryXMLTemplate", "TotalRecordingTimeOfScheduledRecordings", hResultSet["TotalRecordingTimeOfScheduledRecordings"].ToString());
                    XMLWriter.SetXMLTemplateElement("summaryXMLTemplate", "UFTotalRecordingTime", hResultSet["UFTotalRecordingTime"].ToString());
                    XMLWriter.SetXMLTemplateElement("summaryXMLTemplate", "UFTotalRecordingTimeOfTivoSuggestions", hResultSet["UFTotalRecordingTimeOfTivoSuggestions"].ToString());
                    XMLWriter.SetXMLTemplateElement("summaryXMLTemplate", "UFTotalRecordingTimeOfScheduledRecordings", hResultSet["UFTotalRecordingTimeOfScheduledRecordings"].ToString());
                    XMLWriter.Write(XMLWriter.GetXMLTemplate("summaryXMLTemplate"));

                    int nMaxRows = Convert.ToInt32(hResultSet["ProcessCount"].ToString());
                    XMLWriter.Write("\n  <ShowList>");
                    for (int nRowCnt = 0; nRowCnt < nMaxRows;)
                    {
                        XMLWriter.CopyXMLTemplate("showXMLTemplate");
                        Hashtable hRecord = (Hashtable)hResultSet[Convert.ToString(++nRowCnt)];
                        XMLWriter.SetXMLTemplateElement("showXMLTemplate", "Title", hRecord["Title"].ToString());
                        XMLWriter.SetXMLTemplateElement("showXMLTemplate", "ProgramId", hRecord["ProgramId"].ToString());
                        XMLWriter.SetXMLTemplateElement("showXMLTemplate", "EpisodeTitle", hRecord["EpisodeTitle"].ToString());
                        XMLWriter.SetXMLTemplateElement("showXMLTemplate", "EpisodeNumber", hRecord["EpisodeNumber"].ToString());
                        XMLWriter.SetXMLTemplateElement("showXMLTemplate", "Description", hRecord["Description"].ToString());
                        XMLWriter.SetXMLTemplateElement("showXMLTemplate", "Credits", hRecord["Credits"].ToString());
                        XMLWriter.SetXMLTemplateElement("showXMLTemplate", "Genre", hRecord["Genre"].ToString());
                        XMLWriter.SetXMLTemplateElement("showXMLTemplate", "MovieYear", hRecord["MovieYear"].ToString());
                        XMLWriter.SetXMLTemplateElement("showXMLTemplate", "Channel", hRecord["Channel"].ToString());
                        XMLWriter.SetXMLTemplateElement("showXMLTemplate", "StationName", hRecord["StationName"].ToString());
                        XMLWriter.SetXMLTemplateElement("showXMLTemplate", "NetworkAffiliate", hRecord["NetworkAffiliate"].ToString());
                        XMLWriter.SetXMLTemplateElement("showXMLTemplate", "PlayTime", hRecord["Duration"].ToString());
                        XMLWriter.SetXMLTemplateElement("showXMLTemplate", "UFPlayTime", hRecord["UFDuration"].ToString());
                        XMLWriter.SetXMLTemplateElement("showXMLTemplate", "ParentalRating", hRecord["ParentalRating"].ToString());
                        XMLWriter.SetXMLTemplateElement("showXMLTemplate", "Advisory", hRecord["Advisory"].ToString());
                        XMLWriter.SetXMLTemplateElement("showXMLTemplate", "StarRating", hRecord["StarRating"].ToString());
                        XMLWriter.SetXMLTemplateElement("showXMLTemplate", "IsHDContent", hRecord["IsHDContent"].ToString());
                        XMLWriter.SetXMLTemplateElement("showXMLTemplate", "IsRecording", hRecord["IsRecording"].ToString());
                        XMLWriter.SetXMLTemplateElement("showXMLTemplate", "Recorded", hRecord["Recorded"].ToString());
                        XMLWriter.SetXMLTemplateElement("showXMLTemplate", "StartTime", hRecord["StartTime"].ToString());
                        XMLWriter.SetXMLTemplateElement("showXMLTemplate", "StopTime", hRecord["StopTime"].ToString());
                        XMLWriter.SetXMLTemplateElement("showXMLTemplate", "TivoSuggestion", hRecord["TivoSuggestion"].ToString());
                        XMLWriter.SetXMLTemplateElement("showXMLTemplate", "VidoeSize", hRecord["VidoeSize"].ToString());
                        XMLWriter.SetXMLTemplateElement("showXMLTemplate", "UFVidoeSize", hRecord["UFVidoeSize"].ToString());
                        
                        
                        XMLWriter.SetXMLTemplateElement("showXMLTemplate", "DownloadUrl", hRecord["DownloadUrl"].ToString());
                        XMLWriter.SetXMLTemplateElement("showXMLTemplate", "TitleStrongSearchKey", hRecord["TitleStrongSearchKey"].ToString());
                        XMLWriter.SetXMLTemplateElement("showXMLTemplate", "EpisodeStrongSearchKey", hRecord["EpisodeStrongSearchKey"].ToString());
                        XMLWriter.Write(XMLWriter.GetXMLTemplate("showXMLTemplate"));
                    }
                    XMLWriter.Write("\n  </ShowList>");
                    XMLWriter.Close();
                    File.Copy("result-" + GetSettings("/Tivo/TempDataFile"), sNowPlayingFile, true);
                    fiFileInfo = new FileInfo("result-" + GetSettings("/Tivo/TempDataFile"));
                    fiFileInfo.Delete();
                }
            }
        }
    }
}