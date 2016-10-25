using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml.XPath;
using pdaMediaX.Common;
using pdaMediaX.Media;
using pdaMediaX.Util;
using pdaMediaX.Util.Xml;
using pdaMediaX.Net.Sql;
using MySql.Data.MySqlClient;
using Oracle.DataAccess.Client;
using pdaMediaX.Web;

// Task to do:
// 2) Get Counters from db table...
// 3) Check db to see if video already there. If there, use ID's stored in db...
// 4) Check JMDB for Movies and Series/Episodes...
// 7) Email stat's on completion of job...

namespace BuildVideoXMLDB
{
    class BuildVideoXMLDB : pdaMediaX.pdamxBatchJob
    {
        DateTimeFormatInfo dtFormat = new CultureInfo("en-US", false).DateTimeFormat;
        pdamxDBConnector mxDBConnector;
        Hashtable hGenreList;
        Hashtable hBackupRpt;
        Hashtable hNetworkShares;

        bool bBackupSuccessful = false;
        //bool bValidBackupAvailable;

        const int BACKUP_SUCCESS = 100;
        const int BACKUP_WARNING = 200;
        const int BACKUP_ERROR = 300;

        int nJMDBMoviesFound = 0;
        int nBackupStatusCode = 0;

        static void Main(string[] args)
        {
            new BuildVideoXMLDB();
        }
        public BuildVideoXMLDB()
        {
            pdamxVideoProperties mxVideoProperties;
            pdamxSearchKeyGen mxSearchKeyGen;
            pdamxXMLReader mxVidoeXMLDBBackupReader = null;
            pdamxXMLReader mxExVidoeXMLDBReader = null;

            FileInfo fiFileSummary;
            DirectoryInfo diDirectoryInfo;
            XPathNodeIterator xpathINode = null;

            String sVideoXMLDBFile = "";
            String sVideoXMLDBSummaryFile = "";
            String sExVideoXMLDBLibraryFile = "";
            String sGenre = "";

            String sPrevSeries = "";
            String sPrevSeason = "";
            String sPrevSpecial = "";

            String sGoogleSearchUrl = "";
            String sMSNBingSearchUrl = "";
            String sMoviesDotComSearchUrl = "";
            String sTVDotComSearchUrl = "";
            String sConnection;
            String[] sAccess;

            Hashtable hMastList;

            int nStartingRange = 1000;
            int nIncrementIDsBy = 1;
            long lMovieID = 1 * nStartingRange;
            long lSeriesID = 1 * lMovieID;
            long lSpecialID = 1 * lMovieID;
            long lSeasonID = 1 * lMovieID;
            long lEpisodeID = 1 * lMovieID;

            long lMovies = 0;
            long lSeries = 0;
            long lSpecials = 0;
            long lEpisodes = 0;
            long lPlayTime = 0;
            long lStorageUsed = 0;

            long lSeriesStorageUsed = 0;
            long lSpecialStorageUsed = 0;
            long lSeasonStorageUsed = 0;

            long lTotMovies = 0;
            long lTotSeries = 0;
            long lTotSpecials = 0;
            long lTotEpisodes = 0;
            long lTotPlayTime = 0;
            long lTotStorageUsage = 0;

            long lGenreWMostMovies = 0;
            long lGenreWMostSeries = 0;
            long lGenreWMostSpecials = 0;
            long lGenreWMostEpisodes = 0;
            long lGenreUMostStorage = 0;
            long lLongestPlayingGenre = 0;
            long lSeriesWMostEpisodes = 0;

            String sGenreWMostMovies = "";
            String sGenreWMostSeries = "";
            String sGenreWMostSpecials = "";
            String sGenreWMostEpisodes = "";
            String sGenreUMostStorage = "";
            String sLongestPlayingGenre = "";
            String sSeriesWMostEpisodes = "";

            int nCnt = 0;
            int nTotalFiles = 0;
            int nTotalFilesRead = 0;
            int nTotalFilesProcessed = 0;
            int nNumberOfMoviesRead = 0;
            int nNumberOfMoviesProcessed = 0;
            int nNumberOfUnsupportedFiles = 0;
            int nNumberOfEpisodesInSeasonRead = 0;
            int nNumberOfEpisodesInSeasonProcessed = 0;
            int nNumberOfSpecialEpisodesRead = 0;
            int nNumberOfSpecialEpisodesProcessed = 0;
            int nNumberOfMoviesFoundInBackupVideoXMLDB = 0;

            int nNumberOfSeasonsInSeries = 0;
            int nNumberOfEpisodesInSeason = 0;
            int nNumberOfEpisodesInSeries = 0;
            int nNumberOfSpecialEpisodes = 0;
            int nNumberOfSeries = 0;
            int nNumberOfSpecials = 0;
            int nNumberOfNetDrivesConnected = 0;
            int nNumberOfMoviesFoundInExArchive = 0;
            int nTotalNetDrives = 0;
            int nTotalNetDrivesAvailable = 0;

            bool bStart = true;

            String[] sSupportedFiles;

            String jobInfoXMLTemplate =
                      "\n   <JobInfo>"
                    + "\n      <Generated></Generated>"
                    + "\n      <Generator></Generator>"
                    + "\n      <Machine></Machine>"
                    + "\n      <OS></OS>"
                    + "\n      <OSVersion></OSVersion>"
                    + "\n   </JobInfo>";

            String statisticsXMLTemplate =
                      "\n   <Statistics>"
                    + "\n     <Movies></Movies>"
                    + "\n     <Series></Series>"
                    + "\n     <Specials></Specials>"
                    + "\n     <Episodes></Episodes>"
                    + "\n     <Genres></Genres>"
                    + "\n     <PlayTime></PlayTime>"
                    + "\n     <StorageUsage></StorageUsage>"
                    + "\n     <UFPlayTime></UFPlayTime>"
                    + "\n     <UFStorageUsage></UFStorageUsage>"
                    + "\n     <GenreWMostMovies Genre=''></GenreWMostMovies>"
                    + "\n     <GenreWMostSeries Genre=''></GenreWMostSeries>"
                    + "\n     <GenreWMostSpecials Genre=''></GenreWMostSpecials>"
                    + "\n     <GenreWMostEpisodes Genre=''></GenreWMostEpisodes>"
                    + "\n     <GenreUsingMostStorage Genre=''></GenreUsingMostStorage>"
                    + "\n     <LongestPlayingGenre Genre=''></LongestPlayingGenre>"
                    + "\n     <SeriesWithMostEpisodes NumberOfEpisodes=''></SeriesWithMostEpisodes>"
                    + "\n     <StatisticsByGenre>"
                    + "&[StatisticsByGenre]&"
                    + "\n     </StatisticsByGenre>"
                    + "\n   </Statistics>";

            String statsByGenreXMLTemplate =
                      "\n       <Category>"
                    + "\n         <Genre></Genre>"
                    + "\n         <Movies></Movies>"
                    + "\n         <Series></Series>"
                    + "\n         <Specials></Specials>"
                    + "\n         <Episodes></Episodes>"
                    + "\n         <PlayTime></PlayTime>"
                    + "\n         <StorageUsage></StorageUsage>"
                    + "\n         <UFPlayTime></UFPlayTime>"
                    + "\n         <UFStorageUsage></UFStorageUsage>"
                    + "\n       </Category>";

            String movieXMLTemplate =
                      "\n   <Movie>"
                    + "\n     <MovieID></MovieID>"
                    + "\n     <JMDBMovieID></JMDBMovieID>"
                    + "\n     <CorrectedVideoSrcID>0</CorrectedVideoSrcID>"
                    + "\n     <Title></Title>"
                    + "\n     <Description></Description>"
                    + "\n     <Credits></Credits>"
                    + "\n     <Genre></Genre>"
                    + "\n     <NetworkAffiliation></NetworkAffiliation>"
                    + "\n     <Channel></Channel>"
                    + "\n     <StationCallSign></StationCallSign>"
                    + "\n     <StationName></StationName>"
                    + "\n     <VideoPlayTime></VideoPlayTime>"
                    + "\n     <ParentalRating></ParentalRating>"
                    + "\n     <ParentalRatingReason></ParentalRatingReason>"
                    + "\n     <ProviderRating></ProviderRating>"
                    + "\n     <MovieYear></MovieYear>"
                    + "\n     <MediaType></MediaType>"
                    + "\n     <MediaFormat></MediaFormat>"
                    + "\n     <VideoCatagory></VideoCatagory>"
                    + "\n     <SettopDVR></SettopDVR>"
                    + "\n     <CanvasImage></CanvasImage>"
                    + "\n     <CorrectedVideoSrc>NONE</CorrectedVideoSrc>"
                    + "\n     <IsHDContent></IsHDContent>"
                    + "\n     <IsDTVContent></IsDTVContent>"
                    + "\n     <HasCorrectedVideoSrc>No</HasCorrectedVideoSrc>"
                    + "\n     <FileName></FileName>"
                    + "\n     <FileType></FileType>"
                    + "\n     <FileLocation></FileLocation>"
                    + "\n     <FileSize></FileSize>"
                    + "\n     <FileSystemGenre></FileSystemGenre>"
                    + "\n     <FileLastModified></FileLastModified>"
                    + "\n     <UFVideoPlayTime></UFVideoPlayTime>"
                    + "\n     <UFFileSize></UFFileSize>"
                    + "\n     <UFFileLastModified></UFFileLastModified>"
                    + "\n     <GoogleSearch></GoogleSearch>"
                    + "\n     <BingSearch></BingSearch>"
                    + "\n     <MoviesDotComSearch></MoviesDotComSearch>"
                    + "\n     <StrongSearchKey></StrongSearchKey>"
                    + "\n     <WeakSearchKey></WeakSearchKey>"
                    + "\n     <CreditsSearchKey></CreditsSearchKey>"
                    + "\n     <NumericSearchKey></NumericSearchKey>"
                    + "\n     <NumericLowRangeSearchKey></NumericLowRangeSearchKey>"
                    + "\n     <NumericHighRangeSearchKey></NumericHighRangeSearchKey>"
                    + "\n   </Movie>";

            String seriesXMLTemplate =
                      "\n   <Series>"
                    + "\n     <SeriesID></SeriesID>"
                    + "\n     <JMDBMovieID></JMDBMovieID>"
                    + "\n     <SeriesName></SeriesName>"
                    + "\n     <Genre></Genre>"
                    + "\n     <SeriesNumberOfSeasons></SeriesNumberOfSeasons>"
                    + "\n     <SeriesNumberOfEpisodes></SeriesNumberOfEpisodes>"
                    + "\n     <SeriesStorageUsed></SeriesStorageUsed>"
                    + "\n     <UFSeriesStorageUsed></UFSeriesStorageUsed>"
                    + "\n     <CanvasImage></CanvasImage>"
                    + "\n     <GoogleSearch></GoogleSearch>"
                    + "\n     <BingSearch></BingSearch>"
                    + "\n     <TVDotComSearch></TVDotComSearch>"
                    + "\n     <StrongSearchKey></StrongSearchKey>"
                    + "\n     <WeakSearchKey></WeakSearchKey>"
                    + "\n     <NumericSearchKey></NumericSearchKey>"
                    + "\n     <NumericLowRangeSearchKey></NumericLowRangeSearchKey>"
                    + "\n     <NumericHighRangeSearchKey></NumericHighRangeSearchKey>"
                    + "&[Seasons]&"
                    + "\n   </Series>";

            String specialXMLTemplate =
                      "\n   <Special>"
                    + "\n     <SpecialID></SpecialID>"
                    + "\n     <SpecialName></SpecialName>"
                    + "\n     <Genre></Genre>"
                    + "\n     <SpecialNumberOfEpisodes></SpecialNumberOfEpisodes>"
                    + "\n     <SpecialStorageUsed></SpecialStorageUsed>"
                    + "\n     <UFSpecialStorageUsed></UFSpecialStorageUsed>"
                    + "\n     <CanvasImage></CanvasImage>"
                    + "\n     <GoogleSearch></GoogleSearch>"
                    + "\n     <BingSearch></BingSearch>"
                    + "\n     <StrongSearchKey></StrongSearchKey>"
                    + "\n     <WeakSearchKey></WeakSearchKey>"
                    + "\n     <NumericSearchKey></NumericSearchKey>"
                    + "\n     <NumericLowRangeSearchKey></NumericLowRangeSearchKey>"
                    + "\n     <NumericHighRangeSearchKey></NumericHighRangeSearchKey>"
                    + "&[Episodes]&"
                    + "\n   </Special>";

            String seasonXMLTemplate =
                      "\n     <Season>"
                    + "\n     <SeasonID></SeasonID>"
                    + "\n     <SeriesID></SeriesID>"
                    + "\n       <SeasonNumber></SeasonNumber>"
                    + "\n       <SeasonNumberOfEpisodes></SeasonNumberOfEpisodes>"
                    + "\n       <SeasonStorageUsed></SeasonStorageUsed>"
                    + "\n       <UFSeasonStorageUsed></UFSeasonStorageUsed>"
                    + "&[Episodes]&"
                    + "\n     </Season>";

            String episodeXMLTemplate =
                      "\n       <Episode>"
                    + "\n         <EpisodeID></EpisodeID>"
                    + "\n         <JMDBMovieID></JMDBMovieID>"
                    + "\n         <ParentID></ParentID>"
                    + "\n         <CorrectedVideoSrcID>0</CorrectedVideoSrcID>"
                    + "\n         <SeasonID></SeasonID>"
                    + "\n         <Title></Title>"
                    + "\n         <ParentTitle></ParentTitle>"
                    + "\n         <Description></Description>"
                    + "\n         <EpisodeNumber></EpisodeNumber>"
                    + "\n         <Credits></Credits>"
                    + "\n         <Genre></Genre>"
                    + "\n         <NetworkAffiliation></NetworkAffiliation>"
                    + "\n         <Channel></Channel>"
                    + "\n         <StationCallSign></StationCallSign>"
                    + "\n         <StationName></StationName>"
                    + "\n         <VideoPlayTime></VideoPlayTime>"
                    + "\n         <ParentalRating></ParentalRating>"
                    + "\n         <ParentalRatingReason></ParentalRatingReason>"
                    + "\n         <ProviderRating></ProviderRating>"
                    + "\n         <MediaType></MediaType>"
                    + "\n         <MediaFormat></MediaFormat>"
                    + "\n         <VideoCatagory></VideoCatagory>"
                    + "\n         <SettopDVR></SettopDVR>"
                    + "\n         <CanvasImage></CanvasImage>"
                    + "\n         <CorrectedVideoSrc>NONE</CorrectedVideoSrc>"
                    + "\n         <IsHDContent></IsHDContent>"
                    + "\n         <IsDTVContent></IsDTVContent>"
                    + "\n         <FileName></FileName>"
                    + "\n         <FileType></FileType>"
                    + "\n         <FileLocation></FileLocation>"
                    + "\n         <FileSize></FileSize>"
                    + "\n         <FileSystemGenre></FileSystemGenre>"
                    + "\n         <FileLastModified></FileLastModified>"
                    + "\n         <UFVideoPlayTime></UFVideoPlayTime>"
                    + "\n         <UFFileSize></UFFileSize>"
                    + "\n         <UFFileLastModified></UFFileLastModified>"
                    + "\n         <StrongSearchKey></StrongSearchKey>"
                    + "\n         <WeakSearchKey></WeakSearchKey>"
                    + "\n         <CreditsSearchKey></CreditsSearchKey>"
                    + "\n         <NumericSearchKey></NumericSearchKey>"
                    + "\n         <NumericLowRangeSearchKey></NumericLowRangeSearchKey>"
                    + "\n         <NumericHighRangeSearchKey></NumericHighRangeSearchKey>"
                    + "\n       </Episode>";

            String archiveInfoXMLTemplate =
                      "\n   <Archive>"
                    + "\n      <BackInfo>"
                    + "\n         <LastRun></LastRun>"
                    + "\n         <BackupFile></BackupFile>"
                    + "\n         <BackupFileSize></BackupFileSize>"
                    + "\n         <BackupFileLastModified></BackupFileLastModified>"
                    + "\n         <UFBackupFileSize></UFBackupFileSize>"
                    + "\n         <UFBackupFileLastModified></UFBackupFileLastModified>"
                    + "\n      </BackInfo>"
                    + "\n      <LibraryFileInfo>"
                    + "\n         <LibraryFileCreated></LibraryFileCreated>"
                    + "\n         <LibraryFile></LibraryFile>"
                    + "\n         <LibraryFileSize></LibraryFileSize>"
                    + "\n         <LibraryFileLastModified></LibraryFileLastModified>"
                    + "\n         <UFLibraryFileSize></UFLibraryFileSize>"
                    + "\n         <UFLibraryFileLastModified></UFLibraryFileLastModified>"
                    + "\n      </LibraryFileInfo>"
                    + "\n   </Archive>";

            // Backup Video Libarary XML Data file...
            bBackupSuccessful = BackupVXMLDataFile();

            mxVidoeXMLDBBackupReader = new pdamxXMLReader();
            if (bBackupSuccessful)
            {
                mxVidoeXMLDBBackupReader.Open(GetSettings("/Video/LibraryBackup/BackupFile"));
                mxVidoeXMLDBBackupReader.AddNamespace("vxmldb", "http://www.pdamediax.com/videoxmldb");
            }
            // Load XML templates into memory...
            XMLWriter.LoadXMLTemplate("jobInfoXMLTemplate", jobInfoXMLTemplate);
            XMLWriter.LoadXMLTemplate("statisticsXMLTemplate", statisticsXMLTemplate);
            XMLWriter.LoadXMLTemplate("statsByGenreXMLTemplate", statsByGenreXMLTemplate);
            XMLWriter.LoadXMLTemplate("movieXMLTemplate", movieXMLTemplate);
            XMLWriter.LoadXMLTemplate("seriesXMLTemplate", seriesXMLTemplate);
            XMLWriter.LoadXMLTemplate("specialXMLTemplate", specialXMLTemplate);
            XMLWriter.LoadXMLTemplate("seasonXMLTemplate", seasonXMLTemplate);
            XMLWriter.LoadXMLTemplate("episodeXMLTemplate", episodeXMLTemplate);
            XMLWriter.LoadXMLTemplate("archiveInfoXMLTemplate", archiveInfoXMLTemplate);

            try
            {
                //Connect to databases
                mxDBConnector = new pdamxDBConnector();
                mxDBConnector.CreateConnection("JMDB");
                mxDBConnector.Server = GetSettings("/Video/JMDBConnectionInfo/Server");
                mxDBConnector.Port = GetSettings("/Video/JMDBConnectionInfo/Port");
                mxDBConnector.Database = GetSettings("/Video/JMDBConnectionInfo/Database");
                mxDBConnector.DataBaseProvider = pdamxDBConnector.DB_MYSQL;
                sConnection = GetSettings("/Video/JMDBConnectionInfo/Connection");
                if (sConnection.ToLower().Contains(".edf"))
                    sAccess = Crypter.DecryptFile(sConnection).Split('/');
                else
                    sAccess = sConnection.Split('/');
                mxDBConnector.User = sAccess[0];
                mxDBConnector.Password = sAccess[1];
                mxDBConnector.OpenConnection();
                if (mxDBConnector.ErrorException != null)
                    throw new Exception(mxDBConnector.ErrorException.Message);
            }
            catch (Exception e)
            {
                Console.WriteLine("JMDB-MySQL Connection Error: " + e.Message);
                Console.ReadKey();
                return;
            }
            try
            {
                mxDBConnector.CreateConnection("PDAMEDIAX");
                mxDBConnector.Server = GetSettings("/Video/PDAMediaxConnectionInfo/Server");
                mxDBConnector.Port = GetSettings("/Video/PDAMediaxConnectionInfo/Port");
                mxDBConnector.Database = GetSettings("/Video/PDAMediaxConnectionInfo/Database");
                mxDBConnector.DataBaseProvider = pdamxDBConnector.DB_MYSQL;
                sConnection = GetSettings("/Video/PDAMediaxConnectionInfo/Connection");
                if (sConnection.ToLower().Contains(".edf"))
                    sAccess = Crypter.DecryptFile(sConnection).Split('/');
                else
                    sAccess = sConnection.Split('/');
                mxDBConnector.User = sAccess[0];
                mxDBConnector.Password = sAccess[1];
                mxDBConnector.OpenConnection();
                if (mxDBConnector.ErrorException != null)
                    throw new Exception(mxDBConnector.ErrorException.Message);
            }
            catch (Exception e)
            {
                Console.WriteLine("PDAMEDIAX-MySQL Connection Error: " + e.Message);
                Console.ReadKey();
                return;
            }
            try
            {
                mxDBConnector.CreateConnection("ORACLE");
                mxDBConnector.Server = GetSettings("/Video/PDAMediaxOracleConnectionInfo/Server");
                mxDBConnector.Port = GetSettings("/Video/PDAMediaxOracleConnectionInfo/Port");
                mxDBConnector.Database = GetSettings("/Video/PDAMediaxOracleConnectionInfo/Database");
                mxDBConnector.DataBaseProvider = pdamxDBConnector.DB_ORACLE;
                sConnection = GetSettings("/Video/PDAMediaxOracleConnectionInfo/Connection");
                if (sConnection.ToLower().Contains(".edf"))
                    sAccess = Crypter.DecryptFile(sConnection).Split('/');
                else
                    sAccess = sConnection.Split('/');
                mxDBConnector.User = sAccess[0];
                mxDBConnector.Password = sAccess[1];
                mxDBConnector.ConnectionString = "Data Source=(DESCRIPTION=(ADDRESS_LIST=(ADDRESS=(PROTOCOL=TCP)(HOST=localhost)(PORT=1521)))(CONNECT_DATA=(SERVER=DEDICATED)(SERVICE_NAME=mediax))); User Id=pdamediaxpgmr;Password=mediaxpgmr;";
                mxDBConnector.OpenConnection();
                if (mxDBConnector.ErrorException != null)
                    throw new Exception(mxDBConnector.ErrorException.Message);
            }
            catch (Exception e)
            {
                Console.WriteLine("PDAMEDIAX-Oracle Connection Error: " + e.Message);
                Console.ReadKey();
                return;
            }
            // help ref: http://bugs.mysql.com/bug.php?id=36549

            // Configuration based on file name + Config.xml automatically loaded...
            // Get Video XML DB file name...
            sVideoXMLDBFile = GetSettings("/Video/Catalog/LibraryFile");

            // Get Video XML summary file name...
            sVideoXMLDBSummaryFile = GetSettings("/Video/Catalog/SummaryFile");

            // Get Video Extend XML DB file name...
            sExVideoXMLDBLibraryFile = GetSettings("/Video/Catalog/ExVideoXMLDBLibraryFile");

            // Get Google search Url...
            sGoogleSearchUrl = GetSettings("/Video/SearchEngines/GoodleUrl");

            // Get MSN Bing search Url...
            sMSNBingSearchUrl = GetSettings("/Video/SearchEngines/BingUrl");

            // Get Movies.com search Url...
            sMoviesDotComSearchUrl = GetSettings("/Video/SearchEngines/MoviesDotComUrl");

            // Get TV.com search Url...
            sTVDotComSearchUrl = GetSettings("/Video/SearchEngines/TVDotComUrl");

            // Get supported files...
            sSupportedFiles = new String[3];
            xpathINode = SettingsObject.GetNodePath("/Video/SupportedMediaFiles/*");
            nCnt = 0;
            while (xpathINode.MoveNext())
            {
                if (xpathINode.Current.Name.Equals("MediaFile"))
                    sSupportedFiles[nCnt++] = xpathINode.Current.Value;
            }

            // Get Genre list...
            hGenreList = new Hashtable();
            xpathINode = SettingsObject.GetNodePath("/Video/GenreList/*");
            nCnt = 0;
            while (xpathINode.MoveNext())
            {
                if (xpathINode.Current.Name.Equals("Genre"))
                {
                    long[] lSubtotals = new long[6];
                    Hashtable hGenreInfo = new Hashtable();
                    hGenreInfo.Add("Genre", xpathINode.Current.Value);
                    hGenreInfo.Add("SubTotals", lSubtotals);
                    hGenreList.Add(Convert.ToString(nCnt++), hGenreInfo);
                }
            }

            // Get Network shares and connect to them...
            hNetworkShares = new Hashtable();
            xpathINode = SettingsObject.GetNodePath("/Video/NetworkShares/*");
            nCnt = 0;
            Hashtable hShareInfo = null;
            XPathNodeIterator xpathIChildNode;
            while (xpathINode.MoveNext())
            {
                if (xpathINode.Current.Name.Equals("Share"))
                {
                    String sNetShare;
                    hShareInfo = new Hashtable();

                    nTotalNetDrives++;
                    xpathIChildNode = xpathINode.Current.SelectChildren("ShareName", "");
                    xpathIChildNode.MoveNext();
                    sNetShare = xpathIChildNode.Current.Value;

                    xpathIChildNode = xpathINode.Current.SelectChildren("Connection", "");
                    xpathIChildNode.MoveNext();
                    sConnection = xpathIChildNode.Current.Value;

                    xpathIChildNode = xpathINode.Current.SelectChildren("Map", "");
                    xpathIChildNode.MoveNext();

                    if (xpathIChildNode.Current.Value.ToLower().Equals("yes"))
                    {
                        nTotalNetDrivesAvailable++;
                        if (sConnection.ToLower().Contains(".edf"))
                            sAccess = Crypter.DecryptFile(sConnection).Split('/');
                        else
                            sAccess = sConnection.Split('/');
                        try
                        {
                            DisconnectNetkShare(@sNetShare);
                            if (sAccess.Length > 1)
                                ConnectNetShare(@sNetShare, sAccess[0].Trim(), sAccess[1].Trim());
                            else
                                ConnectNetShare(@sNetShare, "", "");
                            hNetworkShares.Add(sNetShare, "Connected");
                            nNumberOfNetDrivesConnected++;
                        }
                        catch (Exception e)
                        {
                            if (e.Message.ToLower().Contains("multiple connections"))
                            {
                                hNetworkShares.Add(sNetShare, "Connected");
                                nNumberOfNetDrivesConnected++;
                            }
                            else
                                hNetworkShares.Add(sNetShare, "");
                        }
                    }
                    else
                    {
                        hNetworkShares.Add(sNetShare, "");
                    }
                }
            }
            // Open Extended XML DB file, if exist;
            mxExVidoeXMLDBReader = new pdamxXMLReader();
            mxExVidoeXMLDBReader.Open(sExVideoXMLDBLibraryFile);
            mxExVidoeXMLDBReader.AddNamespace("exvxmldb", "http://www.pdamediax.com/exvideoxmldb");

            // Create XML DB file...
            XMLWriter.Open(Program + "-temp.xml");
            XMLWriter.RootNode = "VideoCatalog";
            XMLWriter.DTD = "DTD/" + pdamxUtility.StripPath(sVideoXMLDBFile, true);
            XMLWriter.Namespace = "http://www.pdamediax.com/videoxmldb";

            //Write XML content to console stream or file...
            XMLWriter.CopyXMLTemplate("jobInfoXMLTemplate");
            XMLWriter.SetXMLTemplateElement("jobInfoXMLTemplate", "Generated", StartTime);
            XMLWriter.SetXMLTemplateElement("jobInfoXMLTemplate", "Generator", Program);
            XMLWriter.SetXMLTemplateElement("jobInfoXMLTemplate", "Machine", Machine);
            XMLWriter.SetXMLTemplateElement("jobInfoXMLTemplate", "OS", OS);
            XMLWriter.SetXMLTemplateElement("jobInfoXMLTemplate", "OSVersion", OSVersion);
            XMLWriter.Write(XMLWriter.GetXMLTemplate("jobInfoXMLTemplate"));

            //Process movie directories....
            XMLWriter.Write("\n  <MoviesCatalog>");
            xpathINode = SettingsObject.GetNodePath("/Video/Movies/*"); //Positon to first Catagory element of 'Movie' element...
            mxSearchKeyGen = new pdamxSearchKeyGen();
            while (xpathINode.MoveNext())
            {
                sGenre = xpathINode.Current.GetAttribute("Genre", "");
                XPathNodeIterator xpathICatagoryNode = xpathINode.Current.SelectChildren("ScanFolder", "");
                FileInfo[][] fileInfo = new FileInfo[xpathICatagoryNode.Count][];
                nCnt = 0;
                nTotalFiles = 0;

                while (xpathICatagoryNode.MoveNext())
                {
                    if (!IsNetShareConnected(xpathICatagoryNode.Current.Value)) // Network share not connected...
                        continue;
                    if (Directory.Exists(xpathICatagoryNode.Current.Value))
                    {
                        diDirectoryInfo = new DirectoryInfo(xpathICatagoryNode.Current.Value);
                        fileInfo[nCnt] = diDirectoryInfo.GetFiles("*.*");
                        nTotalFiles = nTotalFiles + fileInfo[nCnt].Length;
                        nCnt++;
                    }
                }
                if (nCnt > 0)
                {
                    FileInfo[] fiMastList = new FileInfo[nTotalFiles];
                    int nMasterCnt = 0;
                    for (int i = 0; i < nCnt; i++)
                    {
                        foreach (FileInfo file in fileInfo[i])
                        {
                            fiMastList[nMasterCnt++] = file;
                        }
                    }
                    Array.Sort<FileInfo>(fiMastList, delegate(FileInfo a, FileInfo b) { return a.Name.CompareTo(b.Name); });
                    foreach (FileInfo file in fiMastList)
                    {
                        nTotalFilesRead++;
                        if (!SupportedFile(sSupportedFiles, file.Extension))
                        {
                            nNumberOfUnsupportedFiles++;
                            continue;
                        }
                        nNumberOfMoviesRead++;
                        XMLWriter.CopyXMLTemplate("movieXMLTemplate");
                        mxVideoProperties = new pdamxVideoProperties(file.FullName);
                        XMLWriter.SetXMLTemplateElement("movieXMLTemplate", "MovieID", "M" + Convert.ToString(lMovieID));
                        XMLWriter.SetXMLTemplateElement("movieXMLTemplate", "JMDBMovieID", GetJMDBMovieID(mxVideoProperties.Title, mxVideoProperties.MovieYear));
                        mxSearchKeyGen.GenerateKey(mxVideoProperties.Title);
                        XMLWriter.SetXMLTemplateElement("movieXMLTemplate", "StrongSearchKey", mxSearchKeyGen.StrongKey);
                        XMLWriter.SetXMLTemplateElement("movieXMLTemplate", "WeakSearchKey", mxSearchKeyGen.WeakKey);
                        XMLWriter.SetXMLTemplateElement("movieXMLTemplate", "NumericSearchKey", mxSearchKeyGen.NumericKey);
                        XMLWriter.SetXMLTemplateElement("movieXMLTemplate", "NumericLowRangeSearchKey", mxSearchKeyGen.NumericLowRangeKey);
                        XMLWriter.SetXMLTemplateElement("movieXMLTemplate", "NumericHighRangeSearchKey", mxSearchKeyGen.NumericHighRangeKey);
                        XMLWriter.SetXMLTemplateElement("movieXMLTemplate", "GoogleSearch", sGoogleSearchUrl + mxVideoProperties.Title);
                        XMLWriter.SetXMLTemplateElement("movieXMLTemplate", "BingSearch", sMSNBingSearchUrl + mxVideoProperties.Title);
                        XMLWriter.SetXMLTemplateElement("movieXMLTemplate", "MoviesDotComSearch", sMoviesDotComSearchUrl + mxVideoProperties.Title);

                        Hashtable hSearchResult = null;
                        if ((mxVideoProperties.MovieYear.Trim().Length == 0) || (mxVideoProperties.Credits.Trim().Length == 0)
                            || (mxVideoProperties.Description.Trim().Length == 0))
                        {
                            if (mxExVidoeXMLDBReader.isOpen())
                            {
                                if ((hSearchResult = GetArchiveProgramInfo(mxExVidoeXMLDBReader, mxVideoProperties.Title)) != null)
                                    nNumberOfMoviesFoundInExArchive++;
                            }
                            if (hSearchResult == null)
                                if (mxVidoeXMLDBBackupReader.isOpen())
                                    if ((hSearchResult = GetProgramInfoFromBackupVideoXMLDB(mxVidoeXMLDBBackupReader, mxVideoProperties.Title)) != null)
                                        nNumberOfMoviesFoundInBackupVideoXMLDB++;
                        }
                        if (hSearchResult == null)
                        {
                            XMLWriter.SetXMLTemplateElement("movieXMLTemplate", "Description", mxVideoProperties.Description);
                            XMLWriter.SetXMLTemplateElement("movieXMLTemplate", "Credits", mxVideoProperties.Credits);
                            XMLWriter.SetXMLTemplateElement("movieXMLTemplate", "MovieYear", mxVideoProperties.MovieYear);
                            XMLWriter.SetXMLTemplateElement("movieXMLTemplate", "ProviderRating", mxVideoProperties.ProviderRating);
                            XMLWriter.SetXMLTemplateElement("movieXMLTemplate", "CreditsSearchKey", GenerateCreditsSearchKey(mxVideoProperties.Credits));
                        }
                        else
                        {
                            XMLWriter.SetXMLTemplateElement("movieXMLTemplate", "Description", (hSearchResult["EpisodeDescription"] != null ? hSearchResult["EpisodeDescription"].ToString() : (hSearchResult["Description"] != null ? hSearchResult["Description"].ToString() : mxVideoProperties.Description)));
                            XMLWriter.SetXMLTemplateElement("movieXMLTemplate", "Credits", mxVideoProperties.CleanupCredits((hSearchResult["Actors"] != null ? hSearchResult["Actors"].ToString() : (hSearchResult["Credits"] != null ? hSearchResult["Credits"].ToString() : mxVideoProperties.Credits))));
                            XMLWriter.SetXMLTemplateElement("movieXMLTemplate", "MovieYear", (hSearchResult["MovieYear"] != null ? hSearchResult["MovieYear"].ToString() : mxVideoProperties.MovieYear));
                            XMLWriter.SetXMLTemplateElement("movieXMLTemplate", "ParentalRating", mxVideoProperties.CleanupRatings((hSearchResult["Rating"] != null ? hSearchResult["Rating"].ToString() : (hSearchResult["ParentalRating"] != null ? hSearchResult["ParentalRating"].ToString() : mxVideoProperties.ProviderRating))));
                            XMLWriter.SetXMLTemplateElement("movieXMLTemplate", "CreditsSearchKey", (hSearchResult["Actors"] != null ? GenerateCreditsSearchKey(hSearchResult["Actors"].ToString()) : (hSearchResult["Credits"] != null ? GenerateCreditsSearchKey(hSearchResult["Credits"].ToString()) : GenerateCreditsSearchKey(mxVideoProperties.Credits))));

                            if (hSearchResult["ParentalRatingReason"] != null)
                                XMLWriter.SetXMLTemplateElement("movieXMLTemplate", "ParentalRatingReason", hSearchResult["ParentalRatingReason"].ToString());
                            if (hSearchResult["ProviderRating"] != null)
                                XMLWriter.SetXMLTemplateElement("movieXMLTemplate", "ProviderRating", mxVideoProperties.CleanupProviderRatings(hSearchResult["ProviderRating"].ToString()));
                            if (hSearchResult["Genre"] != null)
                                XMLWriter.SetXMLTemplateElement("movieXMLTemplate", "Genre", hSearchResult["Genre"].ToString());
                        }
                        XMLWriter.SetXMLTemplateElement("movieXMLTemplate", "Title", mxVideoProperties.Title);
                        XMLWriter.SetXMLTemplateElement("movieXMLTemplate", "Genre", mxVideoProperties.Genre);
                        XMLWriter.SetXMLTemplateElement("movieXMLTemplate", "NetworkAffiliation", mxVideoProperties.NetworkAffiliation);
                        XMLWriter.SetXMLTemplateElement("movieXMLTemplate", "Channel", mxVideoProperties.Channel);
                        XMLWriter.SetXMLTemplateElement("movieXMLTemplate", "StationCallSign", mxVideoProperties.StationCallSign);
                        XMLWriter.SetXMLTemplateElement("movieXMLTemplate", "StationName", mxVideoProperties.StationName);
                        XMLWriter.SetXMLTemplateElement("movieXMLTemplate", "ParentalRating", mxVideoProperties.ParentalRating);
                        XMLWriter.SetXMLTemplateElement("movieXMLTemplate", "ParentalRatingReason", mxVideoProperties.ParentalRatingReason);
                        XMLWriter.SetXMLTemplateElement("movieXMLTemplate", "VideoPlayTime", mxVideoProperties.PlayTimeFormatted);
                        XMLWriter.SetXMLTemplateElement("movieXMLTemplate", "UFVideoPlayTime", mxVideoProperties.PlayTimeUnFormatted);
                        XMLWriter.SetXMLTemplateElement("movieXMLTemplate", "FileName", mxVideoProperties.FileName);
                        XMLWriter.SetXMLTemplateElement("movieXMLTemplate", "FileType", mxVideoProperties.FileType);
                        XMLWriter.SetXMLTemplateElement("movieXMLTemplate", "FileLocation", mxVideoProperties.FileLocation);
                        XMLWriter.SetXMLTemplateElement("movieXMLTemplate", "FileSize", mxVideoProperties.FileSizeFormatted);
                        XMLWriter.SetXMLTemplateElement("movieXMLTemplate", "FileLastModified", mxVideoProperties.LastWriteTimeFormatted);
                        XMLWriter.SetXMLTemplateElement("movieXMLTemplate", "UFFileSize", mxVideoProperties.FileSizeUnformatted);
                        XMLWriter.SetXMLTemplateElement("movieXMLTemplate", "UFFileLastModified", mxVideoProperties.LastWriteTimeUnformatted);
                        XMLWriter.SetXMLTemplateElement("movieXMLTemplate", "VideoCatagory", mxVideoProperties.VideoCatagory);
                        XMLWriter.SetXMLTemplateElement("movieXMLTemplate", "MediaType", mxVideoProperties.MediaType);
                        XMLWriter.SetXMLTemplateElement("movieXMLTemplate", "MediaFormat", mxVideoProperties.MediaFormat);
                        XMLWriter.SetXMLTemplateElement("movieXMLTemplate", "SettopDVR", mxVideoProperties.SettopDVR);
                        XMLWriter.SetXMLTemplateElement("movieXMLTemplate", "CanvasImage", mxSearchKeyGen.StrongKey + ".jpg");
                        XMLWriter.SetXMLTemplateElement("movieXMLTemplate", "FileSystemGenre", mxVideoProperties.FileGenre);
                        XMLWriter.SetXMLTemplateElement("movieXMLTemplate", "IsDTVContent", mxVideoProperties.IsDTVContent);
                        XMLWriter.SetXMLTemplateElement("movieXMLTemplate", "IsHDContent", mxVideoProperties.IsHDContent);
                        XMLWriter.Write(XMLWriter.GetXMLTemplate("movieXMLTemplate"));
                        nNumberOfMoviesProcessed++;
                        nTotalFilesProcessed++;
                        lMovieID = lMovieID + nIncrementIDsBy;
                        lMovies++;
                        lPlayTime = lPlayTime + + mxVideoProperties.DurationInSeconds;
                        lStorageUsed = lStorageUsed + mxVideoProperties.FileSize;
                    }
                    // Accumulate Sub-totals....
                    long[] lSubtotals = GetGenreSubTotals(sGenre);
                    lSubtotals[0] = lSubtotals[0] + lMovies;
                    lSubtotals[4] = lSubtotals[4] + lPlayTime;
                    lSubtotals[5] = lSubtotals[5] + lStorageUsed;
                    UpdateGenreSubTotals(sGenre, lSubtotals);

                    // Reset count for next genre...
                    lMovies = 0;
                    lPlayTime = 0;
                    lStorageUsed = 0;
                }
            }
            XMLWriter.Write("\n  </MoviesCatalog>");

            //Process series directories....
            XMLWriter.Write("\n  <SeriesCatalog>");
            xpathINode = SettingsObject.GetNodePath("/Video/Series/*"); //Position to first Category element of 'Series' element...
            XMLWriter.CopyXMLTemplate("seriesXMLTemplate");
            XMLWriter.CopyXMLTemplate("seasonXMLTemplate");
            lPlayTime = 0;
            lStorageUsed = 0;
            while (xpathINode.MoveNext())
            {
                sGenre = xpathINode.Current.GetAttribute("Genre", "");
                XPathNodeIterator xpathICatagoryNode = xpathINode.Current.SelectChildren("ScanFolder", "");
                FileInfo[] fileInfo;

                while (xpathICatagoryNode.MoveNext())
                {
                    hMastList = new Hashtable();
                    nTotalFiles = 0;
                    nCnt = 0;

                    if (!IsNetShareConnected(xpathICatagoryNode.Current.Value)) // Network share not connected...
                        continue;
                    if (Directory.Exists(xpathICatagoryNode.Current.Value))
                    {
                        diDirectoryInfo = new DirectoryInfo(xpathICatagoryNode.Current.Value);
                        fileInfo = diDirectoryInfo.GetFiles("*.*");
                        if (fileInfo != null)
                        {
                            hMastList.Add(Convert.ToString(nCnt++), fileInfo);
                            nTotalFiles = nTotalFiles + fileInfo.Length;
                        }
                        DirectoryInfo[] diSeriesDirectoryInfo = diDirectoryInfo.GetDirectories();
                        foreach (DirectoryInfo diSeriesDir in diSeriesDirectoryInfo)
                        {
                            fileInfo = diSeriesDir.GetFiles("*.*");
                            if (fileInfo != null)
                            {
                                hMastList.Add(Convert.ToString(nCnt++), fileInfo);
                                nTotalFiles = nTotalFiles + fileInfo.Length;
                            }
                            DirectoryInfo[] diSeasonDirectoryInfo = diSeriesDir.GetDirectories();
                            if (diSeasonDirectoryInfo != null)
                            {
                                foreach (DirectoryInfo diSeasonDir in diSeasonDirectoryInfo)
                                {
                                    fileInfo = diSeasonDir.GetFiles("*.*");
                                    if (fileInfo != null)
                                    {
                                        hMastList.Add(Convert.ToString(nCnt++), fileInfo);
                                        nTotalFiles = nTotalFiles + fileInfo.Length;
                                    }
                                }
                            }
                        }
                        FileInfo[] fiMastList = new FileInfo[nTotalFiles];
                        int nMasterCnt = 0;
                        for (int i = 0; i < nCnt; i++)
                        {
                            fileInfo = (FileInfo[])hMastList[Convert.ToString(i)];
                            if (fileInfo != null)
                            {
                                foreach (FileInfo file in fileInfo)
                                {
                                    fiMastList[nMasterCnt++] = file;
                                }
                            }
                        }
                        hMastList = null;
                        Array.Sort<FileInfo>(fiMastList, delegate(FileInfo a, FileInfo b) { return a.FullName.CompareTo(b.FullName); });
                        foreach (FileInfo file in fiMastList)
                        {
                            nTotalFilesRead++;
                            if (!SupportedFile(sSupportedFiles, file.Extension))
                            {
                                nNumberOfUnsupportedFiles++;
                                continue;
                            }
                            mxVideoProperties = new pdamxVideoProperties(file.FullName);
                            if (bStart)
                            {
                                sPrevSeries = mxVideoProperties.Series;
                                sPrevSeason = mxVideoProperties.Season;
                                bStart = false;
                            }
                            if (sPrevSeries != mxVideoProperties.Series)
                            {
                                if (nNumberOfEpisodesInSeason > 0)
                                {
                                    XMLWriter.SetXMLTemplateElement("seasonXMLTemplate", "SeasonID", "SS" + Convert.ToString(lSeasonID));
                                    XMLWriter.SetXMLTemplateElement("seasonXMLTemplate", "SeasonNumber", sPrevSeason);
                                    XMLWriter.SetXMLTemplateElement("seasonXMLTemplate", "SeasonNumberOfEpisodes", Convert.ToString(nNumberOfEpisodesInSeason));
                                    XMLWriter.SetXMLTemplateElement("seasonXMLTemplate", "SeasonStorageUsed", pdamxUtility.FormatStorageSize(Convert.ToString(lSeasonStorageUsed)));
                                    XMLWriter.SetXMLTemplateElement("seasonXMLTemplate", "UFSeasonStorageUsed", Convert.ToString(lSeasonStorageUsed));
                                    XMLWriter.InsertXMLAtTemplateElementMarker("seriesXMLTemplate", "Seasons", XMLWriter.GetXMLTemplate("seasonXMLTemplate"));
                                    XMLWriter.CopyXMLTemplate("seasonXMLTemplate");
                                    sPrevSeason = mxVideoProperties.Season;
                                    nNumberOfSeasonsInSeries++;
                                    lSeasonID = lSeasonID + nIncrementIDsBy;
                                }
                                mxSearchKeyGen.GenerateKey(sPrevSeries);
                                XMLWriter.SetXMLTemplateElement("seriesXMLTemplate", "StrongSearchKey", mxSearchKeyGen.StrongKey);
                                XMLWriter.SetXMLTemplateElement("seriesXMLTemplate", "WeakSearchKey", mxSearchKeyGen.WeakKey);
                                XMLWriter.SetXMLTemplateElement("seriesXMLTemplate", "NumericSearchKey", mxSearchKeyGen.NumericKey);
                                XMLWriter.SetXMLTemplateElement("seriesXMLTemplate", "NumericLowRangeSearchKey", mxSearchKeyGen.NumericLowRangeKey);
                                XMLWriter.SetXMLTemplateElement("seriesXMLTemplate", "NumericHighRangeSearchKey", mxSearchKeyGen.NumericHighRangeKey);
                                XMLWriter.SetXMLTemplateElement("seriesXMLTemplate", "GoogleSearch", sGoogleSearchUrl + sPrevSeries);
                                XMLWriter.SetXMLTemplateElement("seriesXMLTemplate", "BingSearch", sMSNBingSearchUrl + sPrevSeries);
                                XMLWriter.SetXMLTemplateElement("seriesXMLTemplate", "TVDotComSearch", sTVDotComSearchUrl.Replace("{SearchKey}", sPrevSeries));
                                XMLWriter.SetXMLTemplateElement("seriesXMLTemplate", "SeriesID", "S" + Convert.ToString(lSeriesID));
                                XMLWriter.SetXMLTemplateElement("seriesXMLTemplate", "JMDBMovieID", GetJMDBMovieID(sPrevSeries, ""));
                                XMLWriter.SetXMLTemplateElement("seriesXMLTemplate", "SeriesName", sPrevSeries);
                                XMLWriter.SetXMLTemplateElement("seriesXMLTemplate", "CanvasImage", mxSearchKeyGen.StrongKey + ".jpg");
                                XMLWriter.SetXMLTemplateElement("seriesXMLTemplate", "SeriesNumberOfEpisodes", Convert.ToString(nNumberOfEpisodesInSeries));
                                XMLWriter.SetXMLTemplateElement("seriesXMLTemplate", "SeriesStorageUsed", pdamxUtility.FormatStorageSize(Convert.ToString(lSeriesStorageUsed)));
                                XMLWriter.SetXMLTemplateElement("seriesXMLTemplate", "UFSeriesStorageUsed", Convert.ToString(lSeriesStorageUsed));

                                //if (nNumberOfSeasonsInSeries == 0 || mxVideoProperties.Season.Equals("N/A"))
                                if (nNumberOfSeasonsInSeries == 0)
                                    XMLWriter.SetXMLTemplateElement("seriesXMLTemplate", "SeriesNumberOfSeasons", "N/A");
                                else
                                    XMLWriter.SetXMLTemplateElement("seriesXMLTemplate", "SeriesNumberOfSeasons", Convert.ToString(nNumberOfSeasonsInSeries));
                                XMLWriter.Write(XMLWriter.GetXMLTemplate("seriesXMLTemplate"));

                                if (nNumberOfEpisodesInSeries > lSeriesWMostEpisodes)
                                {
                                    lSeriesWMostEpisodes = nNumberOfEpisodesInSeries;
                                    sSeriesWMostEpisodes = sPrevSeries;
                                }
                                XMLWriter.CopyXMLTemplate("seriesXMLTemplate");
                                sPrevSeries = mxVideoProperties.Series;
                                nNumberOfSeries++;
                                nNumberOfEpisodesInSeason = 0;
                                nNumberOfSeasonsInSeries = 0;
                                nNumberOfEpisodesInSeries = 0;
                                lSeasonStorageUsed = 0;
                                lSeriesStorageUsed = 0;
                                lSeriesID = lSeriesID + nIncrementIDsBy;
                                lSeries++;
                            }
                            if (sPrevSeason != mxVideoProperties.Season)
                            {
                                if (nNumberOfEpisodesInSeason > 0)
                                {
                                    XMLWriter.SetXMLTemplateElement("seasonXMLTemplate", "SeasonID", "SS" + Convert.ToString(lSeasonID));
                                    XMLWriter.SetXMLTemplateElement("seasonXMLTemplate", "SeasonNumber", sPrevSeason);
                                    XMLWriter.SetXMLTemplateElement("seasonXMLTemplate", "SeasonNumberOfEpisodes", Convert.ToString(nNumberOfEpisodesInSeason));
                                    XMLWriter.SetXMLTemplateElement("seasonXMLTemplate", "SeasonStorageUsed", pdamxUtility.FormatStorageSize(Convert.ToString(lSeasonStorageUsed)));
                                    XMLWriter.SetXMLTemplateElement("seasonXMLTemplate", "UFSeasonStorageUsed", Convert.ToString(lSeasonStorageUsed));
                                    XMLWriter.InsertXMLAtTemplateElementMarker("seriesXMLTemplate", "Seasons", XMLWriter.GetXMLTemplate("seasonXMLTemplate"));
                                    nNumberOfSeasonsInSeries++;
                                    lSeasonID = lSeasonID + nIncrementIDsBy;
                                }
                                XMLWriter.CopyXMLTemplate("seasonXMLTemplate");
                                sPrevSeason = mxVideoProperties.Season;
                                nNumberOfEpisodesInSeason = 0;
                                lSeasonStorageUsed = 0;
                            }
                            nNumberOfEpisodesInSeasonRead++;
                            XMLWriter.CopyXMLTemplate("episodeXMLTemplate");
                            XMLWriter.SetXMLTemplateElement("episodeXMLTemplate", "EpisodeID", "E" + Convert.ToString(lEpisodeID));
                            XMLWriter.SetXMLTemplateElement("episodeXMLTemplate", "ParentID", "S" + Convert.ToString(lSeriesID));
                            XMLWriter.SetXMLTemplateElement("episodeXMLTemplate", "SeasonID", "SS" +Convert.ToString(lSeasonID));
                            XMLWriter.SetXMLTemplateElement("episodeXMLTemplate", "ParentTitle", sPrevSeries);
                            mxSearchKeyGen.GenerateKey(mxVideoProperties.Title);
                            XMLWriter.SetXMLTemplateElement("episodeXMLTemplate", "StrongSearchKey", mxSearchKeyGen.StrongKey);
                            XMLWriter.SetXMLTemplateElement("episodeXMLTemplate", "WeakSearchKey", mxSearchKeyGen.WeakKey);
                            XMLWriter.SetXMLTemplateElement("episodeXMLTemplate", "NumericSearchKey", mxSearchKeyGen.NumericKey);
                            XMLWriter.SetXMLTemplateElement("episodeXMLTemplate", "NumericLowRangeSearchKey", mxSearchKeyGen.NumericLowRangeKey);
                            XMLWriter.SetXMLTemplateElement("episodeXMLTemplate", "NumericHighRangeSearchKey", mxSearchKeyGen.NumericHighRangeKey);
                            XMLWriter.SetXMLTemplateElement("episodeXMLTemplate", "Title", mxVideoProperties.Title);
                            XMLWriter.SetXMLTemplateElement("episodeXMLTemplate", "ParentTitle", mxVideoProperties.Series);
                            XMLWriter.SetXMLTemplateElement("episodeXMLTemplate", "Description", mxVideoProperties.Description);
                            XMLWriter.SetXMLTemplateElement("episodeXMLTemplate", "EpisodeNumber", mxVideoProperties.Episode);
                            XMLWriter.SetXMLTemplateElement("episodeXMLTemplate", "Genre", mxVideoProperties.Genre);
                            XMLWriter.SetXMLTemplateElement("episodeXMLTemplate", "NetworkAffiliation", mxVideoProperties.NetworkAffiliation);
                            XMLWriter.SetXMLTemplateElement("episodeXMLTemplate", "Channel", mxVideoProperties.Channel);
                            XMLWriter.SetXMLTemplateElement("episodeXMLTemplate", "StationCallSign", mxVideoProperties.StationCallSign);
                            XMLWriter.SetXMLTemplateElement("episodeXMLTemplate", "StationName", mxVideoProperties.StationName);
                            XMLWriter.SetXMLTemplateElement("episodeXMLTemplate", "Credits", mxVideoProperties.Credits);
                            XMLWriter.SetXMLTemplateElement("episodeXMLTemplate", "ParentalRating", mxVideoProperties.ParentalRating);
                            XMLWriter.SetXMLTemplateElement("episodeXMLTemplate", "ParentalRatingReason", mxVideoProperties.ParentalRatingReason);
                            XMLWriter.SetXMLTemplateElement("episodeXMLTemplate", "ProviderRating", mxVideoProperties.ProviderRating);
                            XMLWriter.SetXMLTemplateElement("episodeXMLTemplate", "MovieYear", mxVideoProperties.MovieYear);
                            XMLWriter.SetXMLTemplateElement("episodeXMLTemplate", "VideoPlayTime", mxVideoProperties.PlayTimeFormatted);
                            XMLWriter.SetXMLTemplateElement("episodeXMLTemplate", "UFVideoPlayTime", mxVideoProperties.PlayTimeUnFormatted);
                            XMLWriter.SetXMLTemplateElement("episodeXMLTemplate", "FileName", mxVideoProperties.FileName);
                            XMLWriter.SetXMLTemplateElement("episodeXMLTemplate", "FileType", mxVideoProperties.FileType);
                            XMLWriter.SetXMLTemplateElement("episodeXMLTemplate", "FileLocation", mxVideoProperties.FileLocation);
                            XMLWriter.SetXMLTemplateElement("episodeXMLTemplate", "FileSize", mxVideoProperties.FileSizeFormatted);
                            XMLWriter.SetXMLTemplateElement("episodeXMLTemplate", "FileLastModified", mxVideoProperties.LastWriteTimeFormatted);
                            XMLWriter.SetXMLTemplateElement("episodeXMLTemplate", "UFFileSize", mxVideoProperties.FileSizeUnformatted);
                            XMLWriter.SetXMLTemplateElement("episodeXMLTemplate", "UFFileLastModified", mxVideoProperties.LastWriteTimeUnformatted);
                            XMLWriter.SetXMLTemplateElement("episodeXMLTemplate", "VideoCatagory", mxVideoProperties.VideoCatagory);
                            XMLWriter.SetXMLTemplateElement("episodeXMLTemplate", "MediaType", mxVideoProperties.MediaType);
                            XMLWriter.SetXMLTemplateElement("episodeXMLTemplate", "MediaFormat", mxVideoProperties.MediaFormat);
                            XMLWriter.SetXMLTemplateElement("episodeXMLTemplate", "SettopDVR", mxVideoProperties.SettopDVR);
                            XMLWriter.SetXMLTemplateElement("episodeXMLTemplate", "CanvasImage", mxSearchKeyGen.StrongKey + ".jpg");
                            XMLWriter.SetXMLTemplateElement("episodeXMLTemplate", "FileSystemGenre", mxVideoProperties.FileGenre);
                            XMLWriter.SetXMLTemplateElement("episodeXMLTemplate", "IsDTVContent", mxVideoProperties.IsDTVContent);
                            XMLWriter.SetXMLTemplateElement("episodeXMLTemplate", "IsHDContent", mxVideoProperties.IsHDContent);
                            XMLWriter.SetXMLTemplateElement("episodeXMLTemplate", "CreditsSearchKey", GenerateCreditsSearchKey(mxVideoProperties.Credits));

                            XMLWriter.InsertXMLAtTemplateElementMarker("seasonXMLTemplate", "Episodes", XMLWriter.GetXMLTemplate("episodeXMLTemplate"));
                            XMLWriter.SetXMLTemplateElement("seriesXMLTemplate", "Genre", mxVideoProperties.Genre);

                            nNumberOfEpisodesInSeasonProcessed++;
                            nNumberOfEpisodesInSeason++;
                            nNumberOfEpisodesInSeries++;
                            nTotalFilesProcessed++;
                            lEpisodeID = lEpisodeID + nIncrementIDsBy;
                            lEpisodes++;
                            lPlayTime = lPlayTime + mxVideoProperties.DurationInSeconds;
                            lStorageUsed = lStorageUsed + mxVideoProperties.FileSize;
                            lSeasonStorageUsed = lSeasonStorageUsed + mxVideoProperties.FileSize;
                            lSeriesStorageUsed = lSeriesStorageUsed + mxVideoProperties.FileSize;
                        }
                        if (nNumberOfEpisodesInSeason > 0)
                        {
                            XMLWriter.SetXMLTemplateElement("seasonXMLTemplate", "SeasonID", "SS" + Convert.ToString(lSeasonID));
                            XMLWriter.SetXMLTemplateElement("seasonXMLTemplate", "SeasonNumber", sPrevSeason);
                            XMLWriter.SetXMLTemplateElement("seasonXMLTemplate", "SeasonNumberOfEpisodes", Convert.ToString(nNumberOfEpisodesInSeason));
                            XMLWriter.SetXMLTemplateElement("seasonXMLTemplate", "SeasonStorageUsed", pdamxUtility.FormatStorageSize(Convert.ToString(lSeasonStorageUsed)));
                            XMLWriter.SetXMLTemplateElement("seasonXMLTemplate", "UFSeasonStorageUsed", Convert.ToString(lSeasonStorageUsed));
                            XMLWriter.InsertXMLAtTemplateElementMarker("seriesXMLTemplate", "Seasons", XMLWriter.GetXMLTemplate("seasonXMLTemplate"));
                            nNumberOfSeasonsInSeries++;
                            mxSearchKeyGen.GenerateKey(sPrevSeries);
                            XMLWriter.SetXMLTemplateElement("seriesXMLTemplate", "StrongSearchKey", mxSearchKeyGen.StrongKey);
                            XMLWriter.SetXMLTemplateElement("seriesXMLTemplate", "WeakSearchKey", mxSearchKeyGen.WeakKey);
                            XMLWriter.SetXMLTemplateElement("seriesXMLTemplate", "NumericSearchKey", mxSearchKeyGen.NumericKey);
                            XMLWriter.SetXMLTemplateElement("seriesXMLTemplate", "NumericLowRangeSearchKey", mxSearchKeyGen.NumericLowRangeKey);
                            XMLWriter.SetXMLTemplateElement("seriesXMLTemplate", "NumericHighRangeSearchKey", mxSearchKeyGen.NumericHighRangeKey);
                            XMLWriter.SetXMLTemplateElement("seriesXMLTemplate", "GoogleSearch", sGoogleSearchUrl + sPrevSeries);
                            XMLWriter.SetXMLTemplateElement("seriesXMLTemplate", "BingSearch", sMSNBingSearchUrl + sPrevSeries);
                            XMLWriter.SetXMLTemplateElement("seriesXMLTemplate", "TVDotComSearch", sTVDotComSearchUrl.Replace("{SearchKey}", sPrevSeries));
                            XMLWriter.SetXMLTemplateElement("seriesXMLTemplate", "SeriesID", "S" + Convert.ToString(lSeriesID));
                            XMLWriter.SetXMLTemplateElement("seriesXMLTemplate", "JMDBMovieID", GetJMDBMovieID(sPrevSeries, ""));
                            XMLWriter.SetXMLTemplateElement("seriesXMLTemplate", "SeriesName", sPrevSeries);
                            XMLWriter.SetXMLTemplateElement("seriesXMLTemplate", "Genre", sGenre);
                            XMLWriter.SetXMLTemplateElement("seriesXMLTemplate", "CanvasImage", mxSearchKeyGen.StrongKey + ".jpg");
                            XMLWriter.SetXMLTemplateElement("seriesXMLTemplate", "SeriesNumberOfEpisodes", Convert.ToString(nNumberOfEpisodesInSeries));
                            XMLWriter.SetXMLTemplateElement("seriesXMLTemplate", "SeriesStorageUsed", pdamxUtility.FormatStorageSize(Convert.ToString(lSeriesStorageUsed)));
                            XMLWriter.SetXMLTemplateElement("seriesXMLTemplate", "UFSeriesStorageUsed", Convert.ToString(lSeriesStorageUsed));
                            if (nNumberOfSeasonsInSeries == 0)
                                XMLWriter.SetXMLTemplateElement("seriesXMLTemplate", "SeriesNumberOfSeasons", "N/A");
                            else
                                XMLWriter.SetXMLTemplateElement("seriesXMLTemplate", "SeriesNumberOfSeasons", Convert.ToString(nNumberOfSeasonsInSeries));
                            XMLWriter.Write(XMLWriter.GetXMLTemplate("seriesXMLTemplate"));
                            if (nNumberOfEpisodesInSeries > lSeriesWMostEpisodes)
                            {
                                lSeriesWMostEpisodes = nNumberOfEpisodesInSeries;
                                sSeriesWMostEpisodes = sPrevSeries;
                            }
                            XMLWriter.CopyXMLTemplate("seriesXMLTemplate");
                            XMLWriter.CopyXMLTemplate("seasonXMLTemplate");
                            nNumberOfSeasonsInSeries = 0;
                            nNumberOfEpisodesInSeason = 0;
                            nNumberOfEpisodesInSeries = 0;
                            lSeasonStorageUsed = 0;
                            lSeriesStorageUsed = 0;
                            nNumberOfSeries++;
                            lSeries++;
                            lSeriesID = lSeriesID + nIncrementIDsBy;
                        }
                    }
                    // Accumulate Sub-totals....
                    long[] lSubtotals = GetGenreSubTotals(sGenre);
                    lSubtotals[1] = lSubtotals[1] + lSeries;
                    lSubtotals[3] = lSubtotals[3] + lEpisodes;
                    lSubtotals[4] = lSubtotals[4] + lPlayTime;
                    lSubtotals[5] = lSubtotals[5] + lStorageUsed;
                    UpdateGenreSubTotals(sGenre, lSubtotals);

                    // Reset count for next genre...
                    lSeries = 0;
                    lEpisodes = 0;
                    lPlayTime = 0;
                    lStorageUsed = 0;
                    lSeasonStorageUsed = 0;
                    lSeriesStorageUsed = 0;
                }
            }
            XMLWriter.Write("\n  </SeriesCatalog>");
            //Process Specials directories....
            bStart = true;
            XMLWriter.Write("\n  <SpecialsCatalog>");
            xpathINode = SettingsObject.GetNodePath("/Video/Specials/*"); //Position to first Category element of 'Series' element...
            XMLWriter.CopyXMLTemplate("specialXMLTemplate");
            lEpisodes = 0;
            lPlayTime = 0;
            lStorageUsed = 0;
            while (xpathINode.MoveNext())
            {
                sGenre = xpathINode.Current.GetAttribute("Genre", "");
                XPathNodeIterator xpathICatagoryNode = xpathINode.Current.SelectChildren("ScanFolder", "");
                FileInfo[] fileInfo;

                while (xpathICatagoryNode.MoveNext())
                {
                    hMastList = new Hashtable();
                    nTotalFiles = 0;
                    nCnt = 0;

                    if (!IsNetShareConnected(xpathICatagoryNode.Current.Value)) // Network share not connected...
                        continue;
                    if (Directory.Exists(xpathICatagoryNode.Current.Value))
                    {
                        diDirectoryInfo = new DirectoryInfo(xpathICatagoryNode.Current.Value);

                        fileInfo = diDirectoryInfo.GetFiles("*.*");
                        if (fileInfo != null)
                        {
                            hMastList.Add(Convert.ToString(nCnt++), fileInfo);
                            nTotalFiles = nTotalFiles + fileInfo.Length;
                        }
                        DirectoryInfo[] diSeriesDirectoryInfo = diDirectoryInfo.GetDirectories();
                        foreach (DirectoryInfo diSeriesDir in diSeriesDirectoryInfo)
                        {
                            fileInfo = diSeriesDir.GetFiles("*.*");
                            if (fileInfo != null)
                            {
                                hMastList.Add(Convert.ToString(nCnt++), fileInfo);
                                nTotalFiles = nTotalFiles + fileInfo.Length;
                            }
                            DirectoryInfo[] diSeasonDirectoryInfo = diSeriesDir.GetDirectories();
                            if (diSeasonDirectoryInfo != null)
                            {
                                foreach (DirectoryInfo diSeasonDir in diSeasonDirectoryInfo)
                                {
                                    fileInfo = diSeasonDir.GetFiles("*.*");
                                    if (fileInfo != null)
                                    {
                                        hMastList.Add(Convert.ToString(nCnt++), fileInfo);
                                        nTotalFiles = nTotalFiles + fileInfo.Length;
                                    }
                                }
                            }
                        }
                        FileInfo[] fiMastList = new FileInfo[nTotalFiles];
                        int nMasterCnt = 0;
                        for (int i = 0; i < nCnt; i++)
                        {
                            fileInfo = (FileInfo[])hMastList[Convert.ToString(i)];
                            if (fileInfo != null)
                            {
                                foreach (FileInfo file in fileInfo)
                                {
                                    fiMastList[nMasterCnt++] = file;
                                }
                            }
                        }
                        hMastList = null;
                        Array.Sort<FileInfo>(fiMastList, delegate(FileInfo a, FileInfo b) { return a.FullName.CompareTo(b.FullName); });
                        foreach (FileInfo file in fiMastList)
                        {
                            nTotalFilesRead++;
                            if (!SupportedFile(sSupportedFiles, file.Extension))
                            {
                                nNumberOfUnsupportedFiles++;
                                continue;
                            }
                            mxVideoProperties = new pdamxVideoProperties(file.FullName);
                            if (bStart)
                            {
                                sPrevSpecial = mxVideoProperties.Series;
                                bStart = false;
                            }
                            if (sPrevSpecial != mxVideoProperties.Series)
                            {
                                mxSearchKeyGen.GenerateKey(sPrevSpecial);
                                XMLWriter.SetXMLTemplateElement("specialXMLTemplate", "StrongSearchKey", mxSearchKeyGen.StrongKey);
                                XMLWriter.SetXMLTemplateElement("specialXMLTemplate", "WeakSearchKey", mxSearchKeyGen.WeakKey);
                                XMLWriter.SetXMLTemplateElement("specialXMLTemplate", "NumericSearchKey", mxSearchKeyGen.NumericKey);
                                XMLWriter.SetXMLTemplateElement("specialXMLTemplate", "NumericLowRangeSearchKey", mxSearchKeyGen.NumericLowRangeKey);
                                XMLWriter.SetXMLTemplateElement("specialXMLTemplate", "NumericHighRangeSearchKey", mxSearchKeyGen.NumericHighRangeKey);
                                XMLWriter.SetXMLTemplateElement("specialXMLTemplate", "GoogleSearch", sGoogleSearchUrl + sPrevSpecial);
                                XMLWriter.SetXMLTemplateElement("specialXMLTemplate", "BingSearch", sMSNBingSearchUrl + sPrevSpecial);
                                XMLWriter.SetXMLTemplateElement("specialXMLTemplate", "SpecialID", "SP" + Convert.ToString(lSpecialID));
                                XMLWriter.SetXMLTemplateElement("specialXMLTemplate", "SpecialName", sPrevSpecial);
                                XMLWriter.SetXMLTemplateElement("specialXMLTemplate", "SpecialNumberOfEpisodes", Convert.ToString(nNumberOfSpecialEpisodes));
                                XMLWriter.SetXMLTemplateElement("specialXMLTemplate", "SpecialStorageUsed", pdamxUtility.FormatStorageSize(Convert.ToString(lSpecialStorageUsed)));
                                XMLWriter.SetXMLTemplateElement("specialXMLTemplate", "UFSpecialStorageUsed", Convert.ToString(lSpecialStorageUsed));

                                XMLWriter.Write(XMLWriter.GetXMLTemplate("specialXMLTemplate"));
                                XMLWriter.CopyXMLTemplate("specialXMLTemplate");
                                sPrevSpecial = mxVideoProperties.Series;
                                nNumberOfSpecials++;
                                nNumberOfSpecialEpisodes = 0;
                                lSpecialStorageUsed = 0;
                                lSpecialID = lSpecialID + nIncrementIDsBy;
                                lSpecials++;
                            }
                            nNumberOfSpecialEpisodesRead++;
                            XMLWriter.CopyXMLTemplate("episodeXMLTemplate");
                            XMLWriter.SetXMLTemplateElement("episodeXMLTemplate", "EpisodeID", "E" + Convert.ToString(lEpisodeID));
                            XMLWriter.SetXMLTemplateElement("episodeXMLTemplate", "ParentID", "SP"+ Convert.ToString(lSpecialID));
                            XMLWriter.SetXMLTemplateElement("episodeXMLTemplate", "ParentTitle", sPrevSpecial);
                            XMLWriter.SetXMLTemplateElement("episodeXMLTemplate", "JMDBMovieID", GetJMDBMovieID(mxVideoProperties.Title, mxVideoProperties.MovieYear));
                            mxSearchKeyGen.GenerateKey(mxVideoProperties.Title);
                            XMLWriter.SetXMLTemplateElement("episodeXMLTemplate", "StrongSearchKey", mxSearchKeyGen.StrongKey);
                            XMLWriter.SetXMLTemplateElement("episodeXMLTemplate", "WeakSearchKey", mxSearchKeyGen.WeakKey);
                            XMLWriter.SetXMLTemplateElement("episodeXMLTemplate", "NumericSearchKey", mxSearchKeyGen.NumericKey);
                            XMLWriter.SetXMLTemplateElement("episodeXMLTemplate", "NumericLowRangeSearchKey", mxSearchKeyGen.NumericLowRangeKey);
                            XMLWriter.SetXMLTemplateElement("episodeXMLTemplate", "NumericHighRangeSearchKey", mxSearchKeyGen.NumericHighRangeKey);
                            XMLWriter.SetXMLTemplateElement("episodeXMLTemplate", "Title", mxVideoProperties.Title);
                            XMLWriter.SetXMLTemplateElement("episodeXMLTemplate", "Description", mxVideoProperties.Description);
                            XMLWriter.SetXMLTemplateElement("episodeXMLTemplate", "EpisodeNumber", mxVideoProperties.Episode);
                            XMLWriter.SetXMLTemplateElement("episodeXMLTemplate", "Genre", mxVideoProperties.Genre);
                            XMLWriter.SetXMLTemplateElement("episodeXMLTemplate", "NetworkAffiliation", mxVideoProperties.NetworkAffiliation);
                            XMLWriter.SetXMLTemplateElement("episodeXMLTemplate", "Channel", mxVideoProperties.Channel);
                            XMLWriter.SetXMLTemplateElement("episodeXMLTemplate", "StationCallSign", mxVideoProperties.StationCallSign);
                            XMLWriter.SetXMLTemplateElement("episodeXMLTemplate", "StationName", mxVideoProperties.StationName);
                            XMLWriter.SetXMLTemplateElement("episodeXMLTemplate", "Credits", mxVideoProperties.Credits);
                            XMLWriter.SetXMLTemplateElement("episodeXMLTemplate", "ParentalRating", mxVideoProperties.ParentalRating);
                            XMLWriter.SetXMLTemplateElement("episodeXMLTemplate", "ParentalRatingReason", mxVideoProperties.ParentalRatingReason);
                            XMLWriter.SetXMLTemplateElement("episodeXMLTemplate", "ProviderRating", mxVideoProperties.ProviderRating);
                            XMLWriter.SetXMLTemplateElement("episodeXMLTemplate", "MovieYear", mxVideoProperties.MovieYear);
                            XMLWriter.SetXMLTemplateElement("episodeXMLTemplate", "VideoPlayTime", mxVideoProperties.PlayTimeFormatted);
                            XMLWriter.SetXMLTemplateElement("episodeXMLTemplate", "UFVideoPlayTime", mxVideoProperties.PlayTimeUnFormatted);
                            XMLWriter.SetXMLTemplateElement("episodeXMLTemplate", "FileName", mxVideoProperties.FileName);
                            XMLWriter.SetXMLTemplateElement("episodeXMLTemplate", "FileType", mxVideoProperties.FileType);
                            XMLWriter.SetXMLTemplateElement("episodeXMLTemplate", "FileLocation", mxVideoProperties.FileLocation);
                            XMLWriter.SetXMLTemplateElement("episodeXMLTemplate", "FileSize", mxVideoProperties.FileSizeFormatted);
                            XMLWriter.SetXMLTemplateElement("episodeXMLTemplate", "FileLastModified", mxVideoProperties.LastWriteTimeFormatted);
                            XMLWriter.SetXMLTemplateElement("episodeXMLTemplate", "UFFileSize", mxVideoProperties.FileSizeUnformatted);
                            XMLWriter.SetXMLTemplateElement("episodeXMLTemplate", "UFFileLastModified", mxVideoProperties.LastWriteTimeUnformatted);
                            XMLWriter.SetXMLTemplateElement("episodeXMLTemplate", "VideoCatagory", mxVideoProperties.VideoCatagory);
                            XMLWriter.SetXMLTemplateElement("episodeXMLTemplate", "MediaType", mxVideoProperties.MediaType);
                            XMLWriter.SetXMLTemplateElement("episodeXMLTemplate", "MediaFormat", mxVideoProperties.MediaFormat);
                            XMLWriter.SetXMLTemplateElement("episodeXMLTemplate", "SettopDVR", mxVideoProperties.SettopDVR);
                            XMLWriter.SetXMLTemplateElement("episodeXMLTemplate", "CanvasImage", mxSearchKeyGen.StrongKey + ".jpg");
                            XMLWriter.SetXMLTemplateElement("episodeXMLTemplate", "FileSystemGenre", mxVideoProperties.FileGenre);
                            XMLWriter.SetXMLTemplateElement("episodeXMLTemplate", "IsDTVContent", mxVideoProperties.IsDTVContent);
                            XMLWriter.SetXMLTemplateElement("episodeXMLTemplate", "IsHDContent", mxVideoProperties.IsHDContent);
                            XMLWriter.SetXMLTemplateElement("episodeXMLTemplate", "CreditsSearchKey", GenerateCreditsSearchKey(mxVideoProperties.Credits));
                            XMLWriter.InsertXMLAtTemplateElementMarker("specialXMLTemplate", "Episodes", XMLWriter.GetXMLTemplate("episodeXMLTemplate"));
                            XMLWriter.SetXMLTemplateElement("specialXMLTemplate", "Genre", mxVideoProperties.Genre);
                            nNumberOfSpecialEpisodesProcessed++;
                            nNumberOfSpecialEpisodes++;
                            nTotalFilesProcessed++;
                            lEpisodeID = lEpisodeID + nIncrementIDsBy;
                            lEpisodes++;
                            lPlayTime = lPlayTime + mxVideoProperties.DurationInSeconds;
                            lStorageUsed = lStorageUsed + mxVideoProperties.FileSize;
                            lSpecialStorageUsed = lSpecialStorageUsed + mxVideoProperties.FileSize;
                        }
                        if (nNumberOfEpisodesInSeason > 0)
                        {
                            mxSearchKeyGen.GenerateKey(sPrevSpecial);
                            XMLWriter.SetXMLTemplateElement("specialXMLTemplate", "StrongSearchKey", mxSearchKeyGen.StrongKey);
                            XMLWriter.SetXMLTemplateElement("specialXMLTemplate", "WeakSearchKey", mxSearchKeyGen.WeakKey);
                            XMLWriter.SetXMLTemplateElement("specialXMLTemplate", "NumericSearchKey", mxSearchKeyGen.NumericKey);
                            XMLWriter.SetXMLTemplateElement("specialXMLTemplate", "NumericLowRangeSearchKey", mxSearchKeyGen.NumericLowRangeKey);
                            XMLWriter.SetXMLTemplateElement("specialXMLTemplate", "NumericHighRangeSearchKey", mxSearchKeyGen.NumericHighRangeKey);
                            XMLWriter.SetXMLTemplateElement("specialXMLTemplate", "GoogleSearch", sGoogleSearchUrl + sPrevSpecial);
                            XMLWriter.SetXMLTemplateElement("specialXMLTemplate", "BingSearch", sMSNBingSearchUrl + sPrevSpecial);
                            XMLWriter.SetXMLTemplateElement("specialXMLTemplate", "SpecdialID", "SP" + Convert.ToString(lSpecialID));
                            XMLWriter.SetXMLTemplateElement("specialXMLTemplate", "SpecialName", sPrevSpecial);
                            XMLWriter.SetXMLTemplateElement("specialXMLTemplate", "Genre", sGenre);
                            XMLWriter.SetXMLTemplateElement("specialXMLTemplate", "CanvasImage", mxSearchKeyGen.StrongKey + ".jpg");
                            XMLWriter.SetXMLTemplateElement("specialXMLTemplate", "SpecialNumberOfEpisodes", Convert.ToString(nNumberOfSpecialEpisodes));
                            XMLWriter.SetXMLTemplateElement("specialXMLTemplate", "SpecialStorageUsed", pdamxUtility.FormatStorageSize(Convert.ToString(lSpecialStorageUsed)));
                            XMLWriter.SetXMLTemplateElement("specialXMLTemplate", "UFSpecialStorageUsed", Convert.ToString(lSpecialStorageUsed));
                            XMLWriter.Write(XMLWriter.GetXMLTemplate("specialXMLTemplate"));
                            XMLWriter.CopyXMLTemplate("specialXMLTemplate");
                            nNumberOfSpecials++;
                            lSpecials++;
                            nNumberOfEpisodesInSeason = 0;
                            lSpecialStorageUsed = 0;
                            lSpecialID = lSpecialID + nIncrementIDsBy;
                        }
                    }
                    // Accumulate Sub-totals....
                    long[] lSubtotals = GetGenreSubTotals(sGenre);
                    lSubtotals[2] = lSubtotals[2] + lSpecials;
                    lSubtotals[3] = lSubtotals[3] + lEpisodes;
                    lSubtotals[4] = lSubtotals[4] + lPlayTime;
                    lSubtotals[5] = lSubtotals[5] + lStorageUsed;
                    UpdateGenreSubTotals(sGenre, lSubtotals);

                    // Reset count for next genre...
                    lSpecials = 0;
                    lEpisodes = 0;
                    lPlayTime = 0;
                    lStorageUsed = 0;
                }
            }
            XMLWriter.Write("\n  </SpecialsCatalog>");
            XMLWriter.Close();

            XMLWriter.Open(sVideoXMLDBSummaryFile);
            XMLWriter.RootNode = "VideoCatalogSummary";
            XMLWriter.DTD = "DTD/" + pdamxUtility.StripPath(sVideoXMLDBSummaryFile, true);
            XMLWriter.Namespace = "http://www.pdamediax.com/videoxmldbsummary";
            XMLWriter.Write(XMLWriter.GetXMLTemplate("jobInfoXMLTemplate"));
            XMLWriter.CopyXMLTemplate("statisticsXMLTemplate");

            // Build by genre xml subtotal section...
            for (int i = 0; i < hGenreList.Count; i++)
            {
                XMLWriter.CopyXMLTemplate("statsByGenreXMLTemplate");
                Hashtable hGenreInfo = GetGenreInfo(i);
                sGenre = (String)hGenreInfo["Genre"];
                long[] lSubtotals = GetGenreSubTotals(sGenre);
                XMLWriter.SetXMLTemplateElement("statsByGenreXMLTemplate", "Genre", sGenre);
                XMLWriter.SetXMLTemplateElement("statsByGenreXMLTemplate", "Movies", Convert.ToString(lSubtotals[0]));
                XMLWriter.SetXMLTemplateElement("statsByGenreXMLTemplate", "Series", Convert.ToString(lSubtotals[1]));
                XMLWriter.SetXMLTemplateElement("statsByGenreXMLTemplate", "Specials", Convert.ToString(lSubtotals[2]));
                XMLWriter.SetXMLTemplateElement("statsByGenreXMLTemplate", "Episodes", Convert.ToString(lSubtotals[3]));
                XMLWriter.SetXMLTemplateElement("statsByGenreXMLTemplate", "PlayTime", pdamxUtility.FormatSeconds(Convert.ToString(lSubtotals[4])));
                XMLWriter.SetXMLTemplateElement("statsByGenreXMLTemplate", "StorageUsage", pdamxUtility.FormatStorageSize(Convert.ToString(lSubtotals[5])));
                XMLWriter.SetXMLTemplateElement("statsByGenreXMLTemplate", "UFPlayTime", Convert.ToString(lSubtotals[4]));
                XMLWriter.SetXMLTemplateElement("statsByGenreXMLTemplate", "UFStorageUsage", Convert.ToString(lSubtotals[5]));
                XMLWriter.InsertXMLAtTemplateElementMarker("statisticsXMLTemplate", "StatisticsByGenre", "statsByGenreXMLTemplate");

                lTotMovies = lTotMovies + lSubtotals[0];
                lTotSeries = lTotSeries + lSubtotals[1];
                lTotSpecials = lTotSpecials + lSubtotals[2];
                lTotEpisodes = lTotEpisodes + lSubtotals[3];
                lTotPlayTime = lTotPlayTime + lSubtotals[4];
                lTotStorageUsage = lTotStorageUsage + lSubtotals[5];

                if (lSubtotals[0] > lGenreWMostMovies)
                {
                    lGenreWMostMovies = lSubtotals[0];
                    sGenreWMostMovies = sGenre;
                }
                if (lSubtotals[1] > lGenreWMostSeries)
                {
                    lGenreWMostSeries = lSubtotals[1];
                    sGenreWMostSeries = sGenre;
                }
                if (lSubtotals[2] > lGenreWMostSpecials)
                {
                    lGenreWMostSpecials = lSubtotals[2];
                    sGenreWMostSpecials = sGenre;
                }
                if (lSubtotals[3] > lGenreWMostEpisodes)
                {
                    lGenreWMostEpisodes = lSubtotals[3];
                    sGenreWMostEpisodes = sGenre;
                }
                if (lSubtotals[5] > lGenreUMostStorage)
                {
                    lGenreUMostStorage = lSubtotals[5];
                    sGenreUMostStorage = sGenre;
                }
                if (lSubtotals[4] > lLongestPlayingGenre)
                {
                    lLongestPlayingGenre = lSubtotals[4];
                    sLongestPlayingGenre = sGenre;
                }
            }
            XMLWriter.SetXMLTemplateElement("statisticsXMLTemplate", "Movies", Convert.ToString(lTotMovies));
            XMLWriter.SetXMLTemplateElement("statisticsXMLTemplate", "Series", Convert.ToString(lTotSeries));
            XMLWriter.SetXMLTemplateElement("statisticsXMLTemplate", "Specials", Convert.ToString(lTotSpecials));
            XMLWriter.SetXMLTemplateElement("statisticsXMLTemplate", "Episodes", Convert.ToString(lTotEpisodes));
            XMLWriter.SetXMLTemplateElement("statisticsXMLTemplate", "Genres", Convert.ToString(hGenreList.Count));
            XMLWriter.SetXMLTemplateElement("statisticsXMLTemplate", "PlayTime", pdamxUtility.FormatSeconds(Convert.ToString(lTotPlayTime)));
            XMLWriter.SetXMLTemplateElement("statisticsXMLTemplate", "StorageUsage", pdamxUtility.FormatStorageSize(Convert.ToString(lTotStorageUsage)));
            XMLWriter.SetXMLTemplateElement("statisticsXMLTemplate", "UFPlayTime", Convert.ToString(lTotPlayTime));
            XMLWriter.SetXMLTemplateElement("statisticsXMLTemplate", "UFStorageUsage", Convert.ToString(lTotStorageUsage));
            XMLWriter.SetXMLTemplateElement("statisticsXMLTemplate", "GenreWMostMovies", Convert.ToString(lGenreWMostMovies));
            XMLWriter.SetXMLTemplateElement("statisticsXMLTemplate", "GenreWMostSeries", Convert.ToString(lGenreWMostSeries));
            XMLWriter.SetXMLTemplateElement("statisticsXMLTemplate", "GenreWMostSpecials", Convert.ToString(lGenreWMostSpecials));
            XMLWriter.SetXMLTemplateElement("statisticsXMLTemplate", "GenreWMostEpisodes", Convert.ToString(lGenreWMostEpisodes));
            XMLWriter.SetXMLTemplateElement("statisticsXMLTemplate", "GenreUsingMostStorage", pdamxUtility.FormatStorageSize(Convert.ToString(lGenreUMostStorage)));
            XMLWriter.SetXMLTemplateElement("statisticsXMLTemplate", "LongestPlayingGenre", pdamxUtility.FormatSeconds(Convert.ToString(lLongestPlayingGenre)));
            XMLWriter.SetXMLTemplateElement("statisticsXMLTemplate", "SeriesWithMostEpisodes", sSeriesWMostEpisodes);
            XMLWriter.SetXMLTemplateElementAttribute("statisticsXMLTemplate", "GenreWMostMovies", "Genre", sGenreWMostMovies);
            XMLWriter.SetXMLTemplateElementAttribute("statisticsXMLTemplate", "GenreWMostSeries", "Genre", sGenreWMostSeries);
            XMLWriter.SetXMLTemplateElementAttribute("statisticsXMLTemplate", "GenreWMostSpecials", "Genre", sGenreWMostSpecials);
            XMLWriter.SetXMLTemplateElementAttribute("statisticsXMLTemplate", "GenreWMostEpisodes", "Genre", sGenreWMostEpisodes);
            XMLWriter.SetXMLTemplateElementAttribute("statisticsXMLTemplate", "GenreUsingMostStorage", "Genre", sGenreUMostStorage);
            XMLWriter.SetXMLTemplateElementAttribute("statisticsXMLTemplate", "LongestPlayingGenre", "Genre", sLongestPlayingGenre);
            XMLWriter.SetXMLTemplateElementAttribute("statisticsXMLTemplate", "SeriesWithMostEpisodes", "NumberOfEpisodes", Convert.ToString(lSeriesWMostEpisodes));
            XMLWriter.Write(XMLWriter.GetXMLTemplate("statisticsXMLTemplate"));
            XMLWriter.Close();
            mxDBConnector.CloseAllConnections();
            File.Copy(Program + "-temp.xml", sVideoXMLDBFile, true);
            FileInfo fiFileInfo = new FileInfo(Program + "-temp.xml");
            fiFileInfo.Delete();
            // Disconnect network shares...
            IDictionaryEnumerator idNetShares = hNetworkShares.GetEnumerator();
            while (idNetShares.MoveNext())
            {
                String sNetShare = (String)idNetShares.Key;
                String sConnected = (String)hNetworkShares[sNetShare];
                if (sConnected.ToLower().Equals("yes"))
                    DisconnectNetkShare(@sNetShare);
            }
            // Job Summary...
            WriteEndofJobSummaryToFile = true;
            AddSummaryExtra("");
            AddSummaryExtra("Video Library Processing Summary");
            AddSummaryExtra("");
            AddSummaryExtra("  Video files Read:                         " + pdamxUtility.FormatNumber(nTotalFilesRead));
            AddSummaryExtra("  Video files Processed:                    " + pdamxUtility.FormatNumber(nTotalFilesProcessed));
            AddSummaryExtra("  Video files Unsupported:                  " + pdamxUtility.FormatNumber(nNumberOfUnsupportedFiles));

            AddSummaryExtra("  Storage Used:                             " + pdamxUtility.FormatStorageSize(Convert.ToString(lTotStorageUsage)));
            AddSummaryExtra("  Total Playtime:                           " + pdamxUtility.FormatSecondsInText(Convert.ToString(lTotPlayTime)));

            AddSummaryExtra("");
            AddSummaryExtra("  Number of Movies Read:                    " + pdamxUtility.FormatNumber(nNumberOfMoviesRead));
            AddSummaryExtra("  Number of Movies Processed:               " + pdamxUtility.FormatNumber(nNumberOfMoviesProcessed));
            AddSummaryExtra("  Number of JMDB Movies Matched:            " + pdamxUtility.FormatNumber(nJMDBMoviesFound));
            AddSummaryExtra("  Number of Extended Archive Search Hits:   " + pdamxUtility.FormatNumber(nNumberOfMoviesFoundInExArchive));
            AddSummaryExtra("  Number of Video Backup XMLDB Search Hits: " + pdamxUtility.FormatNumber(nNumberOfMoviesFoundInBackupVideoXMLDB));

            AddSummaryExtra("");
            AddSummaryExtra("  Number of Series:                         " + pdamxUtility.FormatNumber(nNumberOfSeries));
            AddSummaryExtra("  Number of Episodes Read:                  " + pdamxUtility.FormatNumber(nNumberOfEpisodesInSeasonRead));
            AddSummaryExtra("  Number of Episodes Processed:             " + pdamxUtility.FormatNumber(nNumberOfEpisodesInSeasonProcessed));

            AddSummaryExtra("");
            AddSummaryExtra("  Number of Specials (Movies/Mini-Series):  " + pdamxUtility.FormatNumber(nNumberOfSpecials));
            AddSummaryExtra("  Number of Special Episodes Read:          " + pdamxUtility.FormatNumber(nNumberOfSpecialEpisodesRead));
            AddSummaryExtra("  Number of Special Episodes Processed:     " + pdamxUtility.FormatNumber(nNumberOfSpecialEpisodesProcessed));

            fiFileSummary = new FileInfo(sVideoXMLDBFile);
            AddSummaryExtra("");
            AddSummaryExtra("Video Library XML Data File Information");
            AddSummaryExtra("");
            AddSummaryExtra("  Name:      " + fiFileSummary.Name);
            AddSummaryExtra("  Location:  " + fiFileSummary.Directory);
            AddSummaryExtra("  Size:      " + pdamxUtility.FormatStorageSize(Convert.ToString(fiFileSummary.Length)));
            AddSummaryExtra("  Created:   " + fiFileSummary.LastWriteTime.ToString("MM/dd/yyyy (hh:mm:ss tt)", dtFormat));

            fiFileSummary = new FileInfo(sVideoXMLDBSummaryFile);
            AddSummaryExtra("");
            AddSummaryExtra("Video Library XML Data File Summary File Information");
            AddSummaryExtra("");
            AddSummaryExtra("  Name:      " + fiFileSummary.Name);
            AddSummaryExtra("  Location:  " + fiFileSummary.Directory);
            AddSummaryExtra("  Size:      " + pdamxUtility.FormatStorageSize(Convert.ToString(fiFileSummary.Length)));
            AddSummaryExtra("  Created:   " + fiFileSummary.LastWriteTime.ToString("MM/dd/yyyy (hh:mm:ss tt)", dtFormat));

            AddSummaryExtra("");
            AddSummaryExtra("Network Drives Mapping Information");
            AddSummaryExtra("");
            AddSummaryExtra("  Total Number of Drives:                 " + pdamxUtility.FormatNumber(nTotalNetDrives));
            AddSummaryExtra("  Number of Drives Available for Mapping: " + pdamxUtility.FormatNumber(nTotalNetDrivesAvailable));
            AddSummaryExtra("  Number of Drives Mapped:                " + pdamxUtility.FormatNumber(nNumberOfNetDrivesConnected));


            AddSummaryExtra("");
            AddSummaryExtra("Video Library XML Data File Backup Information");
            AddSummaryExtra("");
            AddSummaryExtra("  Backup Status:  " + GetBackupStatus(nBackupStatusCode) + " (" + nBackupStatusCode + ")");

            AddSummaryExtra("");
            for (int i = 0; i < hBackupRpt.Count; i++)
                AddSummaryExtra((String)hBackupRpt[Convert.ToString(i)]);

            if (nBackupStatusCode != BACKUP_ERROR)
            {
                fiFileSummary = new FileInfo(GetSettings("/Video/LibraryBackup/BackupFile"));
                AddSummaryExtra("");
                AddSummaryExtra("Video Library Backup XML Data File Information");
                AddSummaryExtra("");
                AddSummaryExtra("  Name:      " + fiFileSummary.Name);
                AddSummaryExtra("  Location:  " + fiFileSummary.Directory);
                AddSummaryExtra("  Size:      " + pdamxUtility.FormatStorageSize(Convert.ToString(fiFileSummary.Length)));
                AddSummaryExtra("  Created:   " + fiFileSummary.LastWriteTime.ToString("MM/dd/yyyy (hh:mm:ss tt)", dtFormat));

                XMLWriter.CopyXMLTemplate("archiveInfoXMLTemplate");
                XMLWriter.SetXMLTemplateElement("archiveInfoXMLTemplate", "LastRun", StartTime);
                XMLWriter.SetXMLTemplateElement("archiveInfoXMLTemplate", "BackupFile", fiFileSummary.FullName);
                XMLWriter.SetXMLTemplateElement("archiveInfoXMLTemplate", "BackupFileSize", pdamxUtility.FormatStorageSize(Convert.ToString(fiFileSummary.Length)));
                XMLWriter.SetXMLTemplateElement("archiveInfoXMLTemplate", "BackupFileLastModified", fiFileSummary.LastWriteTime.ToString("MM/dd/yyyy (hh:mm:ss tt)", dtFormat));
                XMLWriter.SetXMLTemplateElement("archiveInfoXMLTemplate", "UFBackupFileSize", Convert.ToString(fiFileSummary.Length));
                XMLWriter.SetXMLTemplateElement("archiveInfoXMLTemplate", "UFBackupFileLastModified", Convert.ToString(fiFileSummary.LastWriteTime.ToFileTime()));

                fiFileSummary = new FileInfo(sVideoXMLDBFile);
                XMLWriter.SetXMLTemplateElement("archiveInfoXMLTemplate", "LibraryFileCreated", StartTime);
                XMLWriter.SetXMLTemplateElement("archiveInfoXMLTemplate", "LibraryFile", fiFileSummary.FullName);
                XMLWriter.SetXMLTemplateElement("archiveInfoXMLTemplate", "LibraryFileSize", pdamxUtility.FormatStorageSize(Convert.ToString(fiFileSummary.Length)));
                XMLWriter.SetXMLTemplateElement("archiveInfoXMLTemplate", "LibraryFileLastModified", fiFileSummary.LastWriteTime.ToString("MM/dd/yyyy (hh:mm:ss tt)", dtFormat));
                XMLWriter.SetXMLTemplateElement("archiveInfoXMLTemplate", "UFLibraryFileSize", Convert.ToString(fiFileSummary.Length));
                XMLWriter.SetXMLTemplateElement("archiveInfoXMLTemplate", "UFLibraryFileLastModified", Convert.ToString(fiFileSummary.LastWriteTime.ToFileTime()));

                XMLWriter.Open(GetSettings("/Video/LibraryBackup/ArchiveInfoFile"));
                XMLWriter.RootNode = "VideoCatalogArchive";
                XMLWriter.Namespace = "";
                XMLWriter.DTD = "";
                XMLWriter.Write(XMLWriter.GetXMLTemplate("jobInfoXMLTemplate"));
                XMLWriter.Write(XMLWriter.GetXMLTemplate("archiveInfoXMLTemplate"));
                XMLWriter.Close();
            }
            PrintEndofJobSummary();
        }
        private bool BackupVXMLDataFile()
        {
            FileInfo fiFileInfo;

            String sSourceVFile = GetSettings("/Video/Catalog/LibraryFile");
            String sBackupVFile = GetSettings("/Video/LibraryBackup/BackupFile");
            String sArchiveInfoFile = GetSettings("/Video/LibraryBackup/ArchiveInfoFile");
            String sVLibrarySize = "";
            String sVLibraryLastModifiedTS = "";

            int nLineCnt = 0;
            hBackupRpt = new Hashtable();
            //bValidBackupAvailable = true;
            if (sArchiveInfoFile.Trim().Length == 0)
            {
                hBackupRpt.Add(Convert.ToString(nLineCnt++), "  Backup didn't run due to the following error:");
                hBackupRpt.Add(Convert.ToString(nLineCnt++), "");
                hBackupRpt.Add(Convert.ToString(nLineCnt++), "     --- No backup info (archive) file name provided!");
               // bValidBackupAvailable = false;
                nBackupStatusCode = BACKUP_ERROR;
                return (false);
            }
            if (sBackupVFile.Trim().Length == 0)
            {
                hBackupRpt.Add(Convert.ToString(nLineCnt++), "  Backup didn't run due to the following error:");
                hBackupRpt.Add(Convert.ToString(nLineCnt++), "");
                hBackupRpt.Add(Convert.ToString(nLineCnt++), "     --- No backup file name provided!");
               // bValidBackupAvailable = false;
                nBackupStatusCode = BACKUP_ERROR;
                return (false);
            }
            try
            {
                fiFileInfo = new FileInfo(sArchiveInfoFile);
                if (fiFileInfo.Exists)
                {
                    pdaMediaX.Util.Xml.pdamxXMLReader xmlReader = new pdaMediaX.Util.Xml.pdamxXMLReader(sArchiveInfoFile);
                    sVLibrarySize = xmlReader.GetNodeValue("/VideoCatalogArchive/Archive/LibraryFileInfo/UFLibraryFileSize");
                    sVLibraryLastModifiedTS = xmlReader.GetNodeValue("/VideoCatalogArchive/Archive/LibraryFileInfo/UFLibraryFileLastModified");

                    fiFileInfo = new FileInfo(sSourceVFile);
                    if (!sVLibrarySize.Equals(Convert.ToString(fiFileInfo.Length)))
                    {
                        hBackupRpt.Add(Convert.ToString(nLineCnt++), "  Backup didn't run due to the following error:");
                        hBackupRpt.Add(Convert.ToString(nLineCnt++), "");
                        hBackupRpt.Add(Convert.ToString(nLineCnt++), "     --- Source file to backup ");
                        hBackupRpt.Add(Convert.ToString(nLineCnt++), "           " + sSourceVFile);
                        hBackupRpt.Add(Convert.ToString(nLineCnt++), "");
                        hBackupRpt.Add(Convert.ToString(nLineCnt++), "     --- Size [" + fiFileInfo.Length + "] didn't match archive's record of it [" + sVLibrarySize + "].");
                        nBackupStatusCode = BACKUP_WARNING;
                        return (false);
                    }
                    if (!sVLibraryLastModifiedTS.Equals(Convert.ToString(fiFileInfo.LastWriteTime.ToFileTime())))
                    {
                        hBackupRpt.Add(Convert.ToString(nLineCnt++), "  Backup didn't run due to the following error:");
                        hBackupRpt.Add(Convert.ToString(nLineCnt++), "");
                        hBackupRpt.Add(Convert.ToString(nLineCnt++), "     --- Source file to backup");
                        hBackupRpt.Add(Convert.ToString(nLineCnt++), "           " + sSourceVFile);
                        hBackupRpt.Add(Convert.ToString(nLineCnt++), "");
                        hBackupRpt.Add(Convert.ToString(nLineCnt++), "     --- Last modified timestamp [" + Convert.ToString(fiFileInfo.LastWriteTime.ToFileTime()) + "] didn't match archive's record of it [" + sVLibraryLastModifiedTS + "].");
                        nBackupStatusCode = BACKUP_WARNING;
                        return (false);
                    }
                }
            }
            catch (Exception e)
            {
                hBackupRpt.Add(Convert.ToString(nLineCnt++), "  Backup didn't run due to the following error:");
                hBackupRpt.Add(Convert.ToString(nLineCnt++), "");
                hBackupRpt.Add(Convert.ToString(nLineCnt++), "     --- Exception [" + e.GetBaseException()  + "]");
                hBackupRpt.Add(Convert.ToString(nLineCnt++), "");
                hBackupRpt.Add(Convert.ToString(nLineCnt++), "     --- Error Message [" + e.Message + "]");
                hBackupRpt.Add(Convert.ToString(nLineCnt++), "");
                hBackupRpt.Add(Convert.ToString(nLineCnt++), "     --- Stack Trace [" + e.StackTrace + "]");
                nBackupStatusCode = BACKUP_ERROR;
                return (false);
            }
            try
            {
                File.Copy(sSourceVFile, sBackupVFile,true);
                hBackupRpt.Add(Convert.ToString(nLineCnt++), "Backup successful");
                hBackupRpt.Add(Convert.ToString(nLineCnt++), "");
                hBackupRpt.Add(Convert.ToString(nLineCnt++), "   --- Video Library XML Data File");
                hBackupRpt.Add(Convert.ToString(nLineCnt++), "           " + sSourceVFile + "");
                hBackupRpt.Add(Convert.ToString(nLineCnt++), "");
                hBackupRpt.Add(Convert.ToString(nLineCnt++), "   --- Backed up to Archive");
                hBackupRpt.Add(Convert.ToString(nLineCnt++), "           " + sBackupVFile + "");

                fiFileInfo = new FileInfo(sBackupVFile);
                if (fiFileInfo.Exists)
                {
                   // if (!sVLibraryLastModifiedTS.Equals(Convert.ToString(fiFileInfo.LastWriteTime.ToFileTime())))
                   //     bValidBackupAvailable = false;
                   // if (!sVLibrarySize.Equals(Convert.ToString(fiFileInfo.Length)))
                   //     bValidBackupAvailable = false;
                }
            }
            catch (Exception e)
            {
                hBackupRpt.Add(Convert.ToString(nLineCnt++), "Backup failed for the following error:");
                hBackupRpt.Add(Convert.ToString(nLineCnt++), "");
                hBackupRpt.Add(Convert.ToString(nLineCnt++), "   --- Exception [" + e.GetBaseException() + "]");
                hBackupRpt.Add(Convert.ToString(nLineCnt++), "");
                hBackupRpt.Add(Convert.ToString(nLineCnt++), "   --- Error Message [" + e.Message + "]");
                hBackupRpt.Add(Convert.ToString(nLineCnt++), "");
                hBackupRpt.Add(Convert.ToString(nLineCnt++), "   --- Stack Trace [" + e.StackTrace + "]");
                nBackupStatusCode = BACKUP_ERROR;
                return (false);
            }
            nBackupStatusCode = BACKUP_SUCCESS;
            return (true);
        }
        private String GetBackupStatus(int nStatusCode)
        {
            if (nStatusCode == BACKUP_SUCCESS)
                return ("Success");

            if (nStatusCode == BACKUP_WARNING)
                return ("Warning");

            return ("Error");
        }
        private bool IsNetShareConnected(String _sPath)
        {
            String sNetShare;
            String sConnected;
            IDictionaryEnumerator idNetShares = hNetworkShares.GetEnumerator();

            while (idNetShares.MoveNext())
            {
                sNetShare = (String)idNetShares.Key;
                sConnected = (String)hNetworkShares[sNetShare];
                if (_sPath.ToLower().Contains(sNetShare.ToLower()))
                    if (sConnected.ToLower().Equals("connected"))
                        return (true);
            }
            return (false);
        }
        private bool SupportedFile(String[] _sSupportedFiles, String _sFileExtension)
        {
            for (int i = 0; i < _sSupportedFiles.Length; i++)
            {
                if (_sSupportedFiles[i].Trim().ToLower().Equals(_sFileExtension.Replace(".", "").ToLower()))
                    return (true);
            }
            return (false);
        }
        private Hashtable GetGenreInfo(int _nIndex)
        {
            return ((Hashtable)hGenreList[Convert.ToString(_nIndex)]);
        }
        private long [] GetGenreSubTotals(String _sGenre)
        {
            for (int i = 0; i < hGenreList.Count; i++)
            {
                Hashtable hGenreInfo = (Hashtable)hGenreList[Convert.ToString(i)];
                String sGenre = (String)hGenreInfo["Genre"];
                if (!sGenre.Equals(_sGenre))
                    continue;
                return ((long []) hGenreInfo["SubTotals"]);
            }
            return (null);
        }
        private bool UpdateGenreSubTotals(String _sGenre, long[] _lSubTotals)
        {
            for (int i = 0; i < hGenreList.Count; i++)
            {
                Hashtable hGenreInfo = (Hashtable)hGenreList[Convert.ToString(i)];
                String sGenre = (String)hGenreInfo["Genre"];
                if (!sGenre.Equals(_sGenre))
                    continue;
                hGenreInfo.Remove("SubTotals");
                hGenreInfo.Add("SubTotals", _lSubTotals);
                hGenreList.Remove(Convert.ToString(i));
                hGenreList.Add(Convert.ToString(i), hGenreInfo);
                return (true);
            }
            return (false);
        }
        private String GetJMDBMovieID(String _sEpisodeName, String sYear)
        {
            MySqlDataReader mySqlDataReader;
            String sMovieID = "0";
            String sAltMovieID = "0";
            String sSearchCheck1 = _sEpisodeName;
            String sFilter;
            int nCnt = 0;
            //Ref: http://www.keithjbrown.co.uk/vworks/mysql/mysql_p9.php

            if (sYear != null)
                if (sYear.Trim().Length > 0)
                    sSearchCheck1 = sSearchCheck1 + " (" + sYear + ")";

            sFilter = _sEpisodeName.Replace("(HD)","");
            sFilter = sFilter.Replace("((HD-TP)","").Trim();
            sFilter = sFilter.Replace(" - ", ": ").Trim().ToLower();

            mxDBConnector.SqlCommand = "select movieid, LOWER(title) from jmdb.movies"
               + " where LOWER(title) like '" + pdamxUtility.CheckForQuotes(sFilter) + "%'";
            mySqlDataReader = (MySqlDataReader)mxDBConnector.ExecuteQuery();
            if (mySqlDataReader != null)
            {
                if (!mySqlDataReader.HasRows)
                {
                    mySqlDataReader.Close();
                    mxDBConnector.SqlCommand = "select movieid, LOWER(title) from jmdb.movies"
                        + " where LOWER(title) like '" + pdamxUtility.CheckForQuotes(sFilter.Replace("the ", "")) + "%'";
                    mySqlDataReader = (MySqlDataReader)mxDBConnector.ExecuteQuery();
                }
                if (!mySqlDataReader.HasRows)
                {
                    mySqlDataReader.Close();
                    mxDBConnector.SqlCommand = "select movieid, LOWER(title) from jmdb.movies"
                        + " where LOWER(title) like '" + pdamxUtility.CheckForQuotes("the " + sFilter) + "%'";
                    mySqlDataReader = (MySqlDataReader)mxDBConnector.ExecuteQuery();
                }
                while (mySqlDataReader.Read())
                {
                    if (mySqlDataReader.GetValue(1).Equals(sSearchCheck1.ToLower()))
                    {
                        sMovieID = mySqlDataReader.GetValue(0).ToString();
                        break;
                    }
                    if (mySqlDataReader.GetValue(1).ToString().Contains(sFilter))
                    {
                        sMovieID = mySqlDataReader.GetValue(0).ToString();
                        break;
                    }
                    nCnt++;
                    sAltMovieID = mySqlDataReader.GetValue(0).ToString();
                }
                mySqlDataReader.Close();
            }
            else
            {
                if (mxDBConnector.ErrorException.InnerException != null)
                    Console.WriteLine("Error:" + mxDBConnector.ErrorException.InnerException.Message);
                else if (mxDBConnector.ErrorException != null)
                    Console.WriteLine("Error:" + mxDBConnector.ErrorException.Message);
                else
                    Console.WriteLine("An unknown error has occurred in method: GetJMDBMovieID()");
            }
            if ((sMovieID.Equals("0")) && (nCnt > 0))
                sMovieID = sAltMovieID;
            if (!sMovieID.Equals("0"))
                nJMDBMoviesFound++;
            return (sMovieID);
        }
        private Hashtable GetArchiveProgramInfo(pdamxXMLReader _mxXMLExBackupVideoXMLDBReader, String _sTitle)
        {
            XPathNodeIterator xpathINode;
            Hashtable hRecord;
            String sSearchCriteria = "/exvxmldb:VideoCatalog/exvxmldb:MoviesCatalog/exvxmldb:Movie[exvxmldb:Title =\"" + _sTitle + "\"]";

            xpathINode = _mxXMLExBackupVideoXMLDBReader.GetNodePath(sSearchCriteria);
            if (!xpathINode.MoveNext())
                return (null);

            xpathINode.Current.MoveToFirstChild();
            hRecord = new Hashtable();
            do
            {
                hRecord.Add(xpathINode.Current.Name, xpathINode.Current.Value);
                if (xpathINode.Current.Name.Equals("ParentalRating"))
                    break;
            }
            while (xpathINode.Current.MoveToNext());
            return (hRecord);
        }
        private Hashtable GetProgramInfoFromBackupVideoXMLDB(pdamxXMLReader _mxXMLExBackupVideoXMLDBReader, String _sTitle)
        {
            XPathNodeIterator xpathINode;
            Hashtable hRecord;
            String sSearchCriteria;
            String sTitle = _sTitle;

            if (_sTitle.Contains("("))
                sTitle = _sTitle.Substring(0, _sTitle.IndexOf('(') - 1).Trim();

            sSearchCriteria = "/vxmldb:VideoCatalog/vxmldb:MoviesCatalog/vxmldb:Movie[vxmldb:Title =\"" + sTitle + "\"]";
            xpathINode = _mxXMLExBackupVideoXMLDBReader.GetNodePath(sSearchCriteria);
            if (!xpathINode.MoveNext())
                return (null);

            xpathINode.Current.MoveToFirstChild();
            hRecord = new Hashtable();
            String sMediaFormat = "";
            do
            {
                hRecord.Add(xpathINode.Current.Name, xpathINode.Current.Value);
                if (xpathINode.Current.Name.Equals("MediaFormat"))
                    sMediaFormat = xpathINode.Current.Value;

                if (xpathINode.Current.Name.Equals("NumericHighRangeSearchKey"))
                    if (sMediaFormat.Trim().ToLower().Equals("dvr-ms"))
                        break;
            }
            while (xpathINode.Current.MoveToNext());
            return (hRecord);
        }
        private String GenerateCreditsSearchKey(String _sCredits)
        {
            pdamxSearchKeyGen mxSearchKeyGen = new pdamxSearchKeyGen();
            String sCreditsSearchKey = "";
            String sCredits;
            String [] sActors;

            if (_sCredits == null)
                return ("");

            if (_sCredits.Trim().Length == 0)
                return ("");

            sCredits = _sCredits.Trim();
            while (sCredits.Substring(sCredits.Length - 1, 1).Equals(";"))
            {
                sCredits = sCredits.Substring(0, sCredits.Length - 1);
                if (sCredits.Length - 1 < 0)
                    break;
            }
            if (sCredits.Trim().Length == 0)
                return ("");
            sActors = sCredits.ToString().Replace(";", "/").Replace(",", "/").Split('/');
            for (int i = 0; i < sActors.Length; i++)
            {
                if (!sActors[i].Trim().Equals(""))
                {
                    mxSearchKeyGen.GenerateKey(sActors[i].Trim());
                    sCreditsSearchKey = sCreditsSearchKey + (sCreditsSearchKey.Length > 0 ? ";" : "") + mxSearchKeyGen.StrongKey;
                }
            }
            return (sCreditsSearchKey);
        }
    }
}
