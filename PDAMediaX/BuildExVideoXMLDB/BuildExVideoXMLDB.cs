using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Globalization;
using System.Xml.XPath;
using pdaMediaX.Common;
using pdaMediaX.Media;
using pdaMediaX.Util.Xml;
using pdaMediaX.Web;

namespace BuildExVideoXMLDB
{
    class BuildExVideoXMLDB : pdaMediaX.pdamxBatchJob
    {
        static void Main(string[] args)
        {
            new BuildExVideoXMLDB();
        }
        public BuildExVideoXMLDB()
        {
            DateTimeFormatInfo dtFormat = new CultureInfo("en-US", false).DateTimeFormat;
            Hashtable hSearchResult = null;
            FileInfo fiFileInfo;
            pdamxXMLReader mxXMLVideoXMLDBReader;
            pdamxXMLReader mxXMLExBackupVideoXMLDBReader = null;
            pdamxXMLWriter mxExVidoeXMLDBWriter;
            pdamxBeyondTV mxBeyondTV;
            XPathNodeIterator xpathINode;
            String sExVideoXMLDBLibraryFile = "";
            String sVideoXMLDBLibraryFile = "";
            String sExVideoXMLDBBackupFile = "";
            String sBTVNetworkLicenseFile = "";
            String sBTVUserAccessFile = "";
            String sBTVNetworkLicense = "";
            String sPrevTitle = "";

            int nNumberOfMoviesRead = 0;
            int nNumberOfMovieEntriesSkipped = 0;
            int nNumberOfMoviesFoundInGuide = 0;
            int nNumberOfMoviesFoundInExArchive = 0;
            int nNumberOfMovieEntryWrittened = 0;
            int nNumberOfMoviesNotFound = 0;
            int nNumberOfArchiveEntriesWrittened = 0;

            bool bExVideoXMLDBBackupFound = false;

            String jobInfoXMLTemplate =
                  "\n   <JobInfo>"
                + "\n      <Generated></Generated>"
                + "\n      <Generator></Generator>"
                + "\n       <Machine></Machine>"
                + "\n      <OS></OS>"
                + "\n      <OSVersion></OSVersion>"
                + "\n   </JobInfo>";

            String movieXMLTemplate =
                      "\n   <Movie>"
                    + "\n     <Title></Title>"
                    + "\n     <Description></Description>"
                    + "\n     <Credits></Credits>"
                    + "\n     <MovieYear></MovieYear>"
                    + "\n     <ParentalRating></ParentalRating>"
                    + "\n   </Movie>";

            String summaryXMLTemplate =
                      "\n   <Summary>"
                    + "\n     <MoviesRead></MoviesRead>"
                    + "\n     <EntriesWrittened></EntriesWrittened>"
                    + "\n     <EntriesFoundInBTVGuide></EntriesFoundInBTVGuide>"
                    + "\n     <EntriesFoundedInArchive></EntriesFoundedInArchive>"
                    + "\n     <EntriesNotFound></EntriesNotFound>"
                    + "\n     <EntriesSkipped></EntriesSkipped>"
                    + "\n     <ArchiveEntriesWrittened></ArchiveEntriesWrittened>"
                    + "\n   </Summary>";

            // Get file names...
            sExVideoXMLDBLibraryFile = GetSettings("/Video/Catalog/ExVideoXMLDBLibraryFile");
            sVideoXMLDBLibraryFile = GetSettings("/Video/Catalog/VideoXMLDBLibraryFile");

            // If file created once today then don't create a second time...
            FileInfo fiVideoInfo = new FileInfo(sExVideoXMLDBLibraryFile);
            if (fiVideoInfo.Exists)
            {
                bool bBypassDateCheck = false;
                if (GetSettings("/Video/Catalog/IgnoreDateCheck") != null)
                    if (GetSettings("/Video/Catalog/IgnoreDateCheck").ToLower().Equals("true"))
                        bBypassDateCheck = true;
                if (!bBypassDateCheck)
                {
                    DateTime dtOfFile = fiVideoInfo.LastWriteTime;
                    if (dtOfFile.ToString("MM/dd/yyyy", dtFormat).Equals(DateTime.Now.ToString("MM/dd/yyyy", dtFormat)))
                        return;
                }
            }
            sExVideoXMLDBBackupFile = GetSettings("/Video/Catalog/BackupFile");
            sBTVNetworkLicenseFile = GetSettings("/Video/BeyondTV/License/NetworkLicense");
            if (sBTVNetworkLicenseFile.ToLower().Contains(".edf"))
                sBTVNetworkLicense = Crypter.DecryptFile(sBTVNetworkLicenseFile);
            else
                sBTVNetworkLicense = sBTVNetworkLicenseFile;

            // Get BeyondTV user account...
            sBTVUserAccessFile = GetSettings("/Video/BeyondTV/License/User");

            // Backup Extended Video Libarary XML Data file...
            bExVideoXMLDBBackupFound = BackupExVXMLDataFile(sExVideoXMLDBLibraryFile, sExVideoXMLDBBackupFile);

            try
            {
                mxXMLVideoXMLDBReader = new pdamxXMLReader();
                mxXMLVideoXMLDBReader.Open(sVideoXMLDBLibraryFile);
                mxXMLVideoXMLDBReader.AddNamespace("vxmldb", "http://www.pdamediax.com/videoxmldb");

                mxBeyondTV = new pdamxBeyondTV(sBTVNetworkLicense, sBTVUserAccessFile);

                if (bExVideoXMLDBBackupFound)
                {
                    mxXMLExBackupVideoXMLDBReader = new pdamxXMLReader();
                    mxXMLExBackupVideoXMLDBReader.Open(sExVideoXMLDBBackupFile);
                    mxXMLExBackupVideoXMLDBReader.AddNamespace("exvxmldb", "http://www.pdamediax.com/exvideoxmldb");
                }
                mxExVidoeXMLDBWriter = new pdamxXMLWriter();
                mxExVidoeXMLDBWriter.LoadXMLTemplate("jobInfoXMLTemplate", jobInfoXMLTemplate);
                mxExVidoeXMLDBWriter.LoadXMLTemplate("movieXMLTemplate", movieXMLTemplate);
                mxExVidoeXMLDBWriter.LoadXMLTemplate("summaryXMLTemplate", summaryXMLTemplate);

                mxExVidoeXMLDBWriter.RootNode = "VideoCatalog";
                mxExVidoeXMLDBWriter.Namespace = "http://www.pdamediax.com/exvideoxmldb";
                mxExVidoeXMLDBWriter.Open("result-" + Program + ".xml");

                // Write XML content to console stream or file...
                mxExVidoeXMLDBWriter.CopyXMLTemplate("jobInfoXMLTemplate");
                mxExVidoeXMLDBWriter.SetXMLTemplateElement("jobInfoXMLTemplate", "Generated", StartTime);
                mxExVidoeXMLDBWriter.SetXMLTemplateElement("jobInfoXMLTemplate", "Generator", Program);
                mxExVidoeXMLDBWriter.SetXMLTemplateElement("jobInfoXMLTemplate", "Machine", Machine);
                mxExVidoeXMLDBWriter.SetXMLTemplateElement("jobInfoXMLTemplate", "OS", OS);
                mxExVidoeXMLDBWriter.SetXMLTemplateElement("jobInfoXMLTemplate", "OSVersion", OSVersion);
                mxExVidoeXMLDBWriter.Write(mxExVidoeXMLDBWriter.GetXMLTemplate("jobInfoXMLTemplate"));

                mxExVidoeXMLDBWriter.Write("\n  <MoviesCatalog>");

                // Write archive entries...
                if (bExVideoXMLDBBackupFound)
                {
                    xpathINode = mxXMLExBackupVideoXMLDBReader.GetNodePath("/exvxmldb:VideoCatalog/exvxmldb:MoviesCatalog/*");
                    xpathINode.MoveNext();
                    xpathINode.Current.MoveToParent();
                    xpathINode.Current.MoveToFirstChild();

                    do
                    {
                        if (xpathINode.Current.Name.Equals("Movie"))
                        {
                            String sTitle = "";
                            String sMovieYear = "";
                            String sDescription = "";
                            String sCredits = "";
                            String sRating = "";

                            xpathINode.Current.MoveToFirstChild();
                            do
                            {
                                if (xpathINode.Current.Name.Equals("Title"))
                                    sTitle = xpathINode.Current.Value;

                                if (xpathINode.Current.Name.Equals("MovieYear"))
                                    sMovieYear = xpathINode.Current.Value;

                                if (xpathINode.Current.Name.Equals("Description"))
                                    sDescription = xpathINode.Current.Value;

                                if (xpathINode.Current.Name.Equals("Credits"))
                                    sCredits = xpathINode.Current.Value;

                                if (xpathINode.Current.Name.Equals("ParentalRating"))
                                    sRating = xpathINode.Current.Value;

                            }
                            while (xpathINode.Current.MoveToNext());
                            mxExVidoeXMLDBWriter.CopyXMLTemplate("movieXMLTemplate");
                            mxExVidoeXMLDBWriter.SetXMLTemplateElement("movieXMLTemplate", "Title", sTitle);
                            mxExVidoeXMLDBWriter.SetXMLTemplateElement("movieXMLTemplate", "Description", sDescription);
                            mxExVidoeXMLDBWriter.SetXMLTemplateElement("movieXMLTemplate", "Credits", sCredits);
                            mxExVidoeXMLDBWriter.SetXMLTemplateElement("movieXMLTemplate", "MovieYear", sMovieYear);
                            mxExVidoeXMLDBWriter.SetXMLTemplateElement("movieXMLTemplate", "ParentalRating", sRating);
                            if (sMovieYear != null && sRating != null)
                            {
                                if (sMovieYear != "" && sRating != "")
                                {
                                    mxExVidoeXMLDBWriter.Write(mxExVidoeXMLDBWriter.GetXMLTemplate("movieXMLTemplate"));
                                    nNumberOfArchiveEntriesWrittened++;
                                    nNumberOfMovieEntryWrittened++;
                                }
                            }
                            xpathINode.Current.MoveToParent();
                        }
                    }
                    while (xpathINode.Current.MoveToNext());
                }
                xpathINode = mxXMLVideoXMLDBReader.GetNodePath("/vxmldb:VideoCatalog/vxmldb:MoviesCatalog/*");
                xpathINode.MoveNext();
                xpathINode.Current.MoveToParent();
                xpathINode.Current.MoveToFirstChild();

                do
                {
                    if (xpathINode.Current.Name.Equals("Movie"))
                    {
                        String sTitle = "";
                        String sMovieYear = "";
                        String sDescription = "";
                        String sCredits = "";

                        xpathINode.Current.MoveToFirstChild();
                        nNumberOfMoviesRead++;
                        do
                        {
                            if (xpathINode.Current.Name.Equals("Title"))
                                sTitle = xpathINode.Current.Value;

                            if (xpathINode.Current.Name.Equals("MovieYear"))
                                sMovieYear = xpathINode.Current.Value;

                            if (xpathINode.Current.Name.Equals("Description"))
                                sDescription = xpathINode.Current.Value;

                            if (xpathINode.Current.Name.Equals("Credits"))
                                sCredits = xpathINode.Current.Value;
                        }
                        while (xpathINode.Current.MoveToNext());

                        if ((sMovieYear.Trim().Length == 0) || (sCredits.Trim().Length == 0)
                            || (sDescription.Trim().Length == 0))
                        {
                            hSearchResult = null;
                            if (!sTitle.Equals(sPrevTitle))
                            {
                                bool bSkipEntry = false;
                                if (bExVideoXMLDBBackupFound)
                                {
                                    if ((hSearchResult = GetArchiveProgramInfo(mxXMLExBackupVideoXMLDBReader, sTitle)) != null)
                                    {
                                        bSkipEntry = true;
                                        nNumberOfMoviesFoundInExArchive++;
                                    }
                                }
                                if (hSearchResult == null)
                                    if ((hSearchResult = GetBTVProgramInfo(mxBeyondTV.SearchGuideAll(sTitle), sTitle)) != null)
                                        nNumberOfMoviesFoundInGuide++;

                                if (!bSkipEntry)
                                {
                                    if (hSearchResult != null)
                                    {
                                        String sRating = (hSearchResult["Rating"] != null ? hSearchResult["Rating"].ToString() : (hSearchResult["ParentalRating"] != null ? hSearchResult["ParentalRating"].ToString() : ""));
                                        sMovieYear = (hSearchResult["MovieYear"] != null ? hSearchResult["MovieYear"].ToString() : "");

                                        mxExVidoeXMLDBWriter.CopyXMLTemplate("movieXMLTemplate");
                                        mxExVidoeXMLDBWriter.SetXMLTemplateElement("movieXMLTemplate", "Title", sTitle);
                                        mxExVidoeXMLDBWriter.SetXMLTemplateElement("movieXMLTemplate", "Description", (hSearchResult["EpisodeDescription"] != null ? hSearchResult["EpisodeDescription"].ToString() : (hSearchResult["Description"] != null ? hSearchResult["Description"].ToString() : "")));
                                        mxExVidoeXMLDBWriter.SetXMLTemplateElement("movieXMLTemplate", "Credits", (hSearchResult["Actors"] != null ? hSearchResult["Actors"].ToString() : (hSearchResult["Credits"] != null ? hSearchResult["Credits"].ToString() : "")));
                                        mxExVidoeXMLDBWriter.SetXMLTemplateElement("movieXMLTemplate", "MovieYear", (hSearchResult["MovieYear"] != null ? hSearchResult["MovieYear"].ToString() : ""));
                                        mxExVidoeXMLDBWriter.SetXMLTemplateElement("movieXMLTemplate", "ParentalRating", (hSearchResult["Rating"] != null ? hSearchResult["Rating"].ToString() : (hSearchResult["ParentalRating"] != null ? hSearchResult["ParentalRating"].ToString() : "")));

                                        if (sMovieYear != "" && sRating != "")
                                        {
                                            mxExVidoeXMLDBWriter.Write(mxExVidoeXMLDBWriter.GetXMLTemplate("movieXMLTemplate"));
                                            nNumberOfMovieEntryWrittened++;
                                        }
                                    }
                                    else
                                    {
                                        nNumberOfMoviesNotFound++;
                                    }
                                }
                                sPrevTitle = sTitle;
                            }
                            else
                            {
                                nNumberOfMovieEntriesSkipped++;
                            }
                        }
                        else 
                        {
                            nNumberOfMovieEntriesSkipped++;
                        }
                        xpathINode.Current.MoveToParent();
                    }
                }
                while (xpathINode.Current.MoveToNext());

                mxExVidoeXMLDBWriter.Write("\n  </MoviesCatalog>");
                mxExVidoeXMLDBWriter.CopyXMLTemplate("summaryXMLTemplate");
                mxExVidoeXMLDBWriter.SetXMLTemplateElement("summaryXMLTemplate", "MoviesRead", Convert.ToString(nNumberOfMoviesRead));
                mxExVidoeXMLDBWriter.SetXMLTemplateElement("summaryXMLTemplate", "EntriesWrittened", Convert.ToString(nNumberOfMovieEntryWrittened));
                mxExVidoeXMLDBWriter.SetXMLTemplateElement("summaryXMLTemplate", "EntriesFoundInBTVGuide", Convert.ToString(nNumberOfMoviesFoundInGuide));
                mxExVidoeXMLDBWriter.SetXMLTemplateElement("summaryXMLTemplate", "EntriesFoundedInArchive", Convert.ToString(nNumberOfMoviesFoundInExArchive));
                mxExVidoeXMLDBWriter.SetXMLTemplateElement("summaryXMLTemplate", "EntriesNotFound", Convert.ToString(nNumberOfMoviesNotFound));
                mxExVidoeXMLDBWriter.SetXMLTemplateElement("summaryXMLTemplate", "EntriesSkipped", Convert.ToString(nNumberOfMovieEntriesSkipped));
                mxExVidoeXMLDBWriter.SetXMLTemplateElement("summaryXMLTemplate", "ArchiveEntriesWrittened", Convert.ToString(nNumberOfArchiveEntriesWrittened));        
                mxExVidoeXMLDBWriter.Write(mxExVidoeXMLDBWriter.GetXMLTemplate("summaryXMLTemplate"));
                mxExVidoeXMLDBWriter.Close();
                File.Copy("result-" + Program + ".xml", sExVideoXMLDBLibraryFile, true);
                fiFileInfo = new FileInfo("result-" + Program + ".xml");
                fiFileInfo.Delete();
                
                // Job Summary...
                WriteEndofJobSummaryToFile = true;
                AddSummaryExtra("");
                AddSummaryExtra("Extended Video Library Processing Summary");
                AddSummaryExtra("");         
                AddSummaryExtra("  Movie Video Entries:                " + pdamxUtility.FormatNumber(nNumberOfMoviesRead));
                AddSummaryExtra("  --- Movie Video Entries Not Found:  " + pdamxUtility.FormatNumber(nNumberOfMoviesNotFound));
                AddSummaryExtra("  --- Movie Video Entries Skipped  :  " + pdamxUtility.FormatNumber(nNumberOfMovieEntriesSkipped));
                AddSummaryExtra("");
                AddSummaryExtra("  Movie Video Entrie's Info Writtened:  " + pdamxUtility.FormatNumber(nNumberOfMovieEntryWrittened));
                AddSummaryExtra("  --- BTV Guide Search Hits:            " + pdamxUtility.FormatNumber(nNumberOfMoviesFoundInGuide));
                AddSummaryExtra("  --- Archive Search Hits:              " + pdamxUtility.FormatNumber(nNumberOfMoviesFoundInExArchive));
                AddSummaryExtra("  --- Archive Entries Writtened:        " + pdamxUtility.FormatNumber(nNumberOfArchiveEntriesWrittened));
                PrintEndofJobSummary();
            }
            catch (Exception e)
            {
                Console.WriteLine("BuildExVideoXMLDB Error: " + e.Message);
                Console.ReadKey();
                return;
            }
        }
        private bool BackupExVXMLDataFile(String _sExVideoXMLDBLibraryFile, String _sExVideoXMLDBBackupFile)
        {
            FileInfo fiSourceFileInfo;
            bool bSuccess = false;

            fiSourceFileInfo = new FileInfo(_sExVideoXMLDBLibraryFile);
            if (!fiSourceFileInfo.Exists)
                return (bSuccess);

            try
            {
                File.Copy(_sExVideoXMLDBLibraryFile, _sExVideoXMLDBBackupFile, true);
                bSuccess = true;
            }
            catch (Exception e)
            {
                Console.WriteLine("Error:" + e.Message);
                Console.ReadKey();
                return (bSuccess);
            }
            return (bSuccess);
        }
        private Hashtable GetBTVProgramInfo(Hashtable _hSearchResult, String _sTitle)
        {
            Hashtable hRecord;

            if (_hSearchResult.Count > 0)
            {
                for (int i = 1; i <= _hSearchResult.Count; i++)
                {
                    hRecord = (Hashtable)_hSearchResult[Convert.ToString(i)];
                    if (_sTitle.Replace(":", "").Trim().Equals(hRecord["DisplayTitle"].ToString().Replace(":", "").Trim()))
                    {
                        if (hRecord["EpisodeDescription"] != null)
                            if (hRecord["EpisodeDescription"].ToString().Trim().Length > 0)
                                return (hRecord);
                        if (hRecord["Actors"] != null)
                            if (hRecord["Actors"].ToString().Trim().Length > 0)
                                return (hRecord);
                        if (hRecord["MovieYear"] != null)
                            if (hRecord["MovieYear"].ToString().Trim().Length > 0)
                                return (hRecord);
                        if (hRecord["Rating"] != null)
                            if (hRecord["Rating"].ToString().Trim().Length > 0)
                                return (hRecord);
                    }
                }
            }
            return (null);
        }
        private Hashtable GetArchiveProgramInfo(pdamxXMLReader _mxXMLExBackupVideoXMLDBReader, String _sTitle)
        {
            XPathNodeIterator xpathINode;
            Hashtable hRecord;
            String sSearchCriteria = "/exvxmldb:VideoCatalog/exvxmldb:MoviesCatalog/exvxmldb:Movie[exvxmldb:Title =\"" + _sTitle.Trim() + "\"]";

            xpathINode = _mxXMLExBackupVideoXMLDBReader.GetNodePath(sSearchCriteria);
            if (!xpathINode.MoveNext())
                return(null);

            xpathINode.Current.MoveToFirstChild();
            hRecord = new Hashtable();
            do
            {
                hRecord.Add(xpathINode.Current.Name, xpathINode.Current.Value);
                if (xpathINode.Current.Name.Equals("ParentalRating"))
                    break;
            }
            while (xpathINode.Current.MoveToNext());

            if ((hRecord["MovieYear"].ToString().Trim().Length == 0) && (hRecord["Credits"].ToString().Trim().Length == 0)
                && (hRecord["Description"].ToString().Trim().Length == 0) && (hRecord["ParentalRating"].ToString().Trim().Length == 0))
                return (null);
            return (hRecord);
        }
    }
}
