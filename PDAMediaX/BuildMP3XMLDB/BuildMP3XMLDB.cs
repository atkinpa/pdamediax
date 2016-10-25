using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using pdaMediaX.Common;
using pdaMediaX.Media;
using pdaMediaX.Util;

// Task to do:
// 2) Check db to see if music already there. If there, use ID's stored in db...
// 3) Get Counters from db table...
// 4) Email stat's on completion of job...

namespace BuildMP3XMLDB
{
    class BuildMP3XMLDB : pdaMediaX.pdamxBatchJob
    {
        static void Main(string[] args)
        {
            new BuildMP3XMLDB();
        }
        public BuildMP3XMLDB()
        {
            pdamxAudioProperties mxAudioProperties;
            pdamxSearchKeyGen mxSearchKeyGen;

            FileInfo fiFileSummary;
            DirectoryInfo diDirectoryInfo;
            DateTimeFormatInfo dtFormat = new CultureInfo("en-US", false).DateTimeFormat;
            TimeSpan tspAlbumPlayTime = new TimeSpan();

            bool bStart = true;

            int nGenreCnt = 0;
            int nSongCnt = 0;
            int nAlbumCnt = 0;
            int nArtistWithMostTitles = 0;
            int nArtistTitleCnt = 0;
            int nFilesProcessed = 0;
            int nFilesRead = 0;
            int nStartingRange = 1000;
            int nIncrementIDsBy = 1;
            
            long[,] lSubtotals;
            long lTitles = 0;
            long lArtist = 0;
            long lAlbums = 0;
            long lPlayTime = 0;
            long lArtistPlayTime = 0;
            long lStorageUsage = 0;
            long lAlbumStorageUsage = 0;
            long lArtistStorageUsage = 0;

            long lArtistID = 1 * nStartingRange;
            long lAlbumID = 3 * lArtistID;
            long lSongID = 2 * lAlbumID;

            long lTotTitles = 0;
            long lTotArtist = 0;
            long lTotAlbums = 0;
            long lTotPlayTime = 0;
            long lTotStorageUsage = 0;
            long lGenreWithMostTitles = 0;
            long lGenreUsingMostStorage = 0;
            long lLongestPlayingGenre = 0;
            long lLongestPlaySongTitleArtistID = 0;
            long lLongestPlaySongTitleAlbumID = 0;
            long lShorestPlaySongTitleArtistID = 0;
            long lShorestPlaySongTitleAlbumID = 0;
            
            double dLongestPlayingSongTitle = 0;
            double dShorestPlayingSongTitle = 9999999999;

            String[] sGenre;
            String sLongestPlaySongTitle = "";
            String sLongestPlaySongTitleArtist = "";
            String sLongestPlaySongTitleArtistAlbum = "";
            String sShorestPlaySongTitle = "";
            String sShorestPlaySongTitleArtist = "";
            String sShorestPlaySongTitleArtistAlbum = "";
            String sGenreWithMostTitles = "";
            String sGenreUsingMostStorage = "";
            String sLongestPlayingGenre = "";
            String sArtistWithMostTitles = "";
            String sPrevAlbum = "";
            String sPreviousArtist = "";
            String sSubTotalList = "";
            String sCatalogDirectory = "";
            String sMP3XMLDBFile = "";
            String sMP3XMLDBSummaryFile = "";
            String sRealPlayerServerUrl = "";
            String sMediaPlayerServerUrl = "";
            String sGoogleSearchUrl = "";
            String sMSNBingSearchUrl = "";

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
                    + "\n     <Artist></Artist>"
                    + "\n     <Albums></Albums>"
                    + "\n     <SongTitles></SongTitles>"
                    + "\n     <Genres></Genres>"
                    + "\n     <PlayTime></PlayTime>"
                    + "\n     <StorageUsage></StorageUsage>"
                    + "\n     <UFPlayTime></UFPlayTime>"
                    + "\n     <UFStorageUsage></UFStorageUsage>"
                    + "\n     <GenreWMostTitles Genre=''></GenreWMostTitles>"
                    + "\n     <GenreUsingMostStorage Genre=''></GenreUsingMostStorage>"
                    + "\n     <LongestPlayingGenre Genre=''></LongestPlayingGenre>"
                    + "\n     <ArtistWithMostTitles NumberOfSongs=''></ArtistWithMostTitles>"
                    + "&[LongestPlaySongTitle]&"
                    + "&[ShorestPlaySongTitle]&"
                    + "\n     <StatisticsByGenre>"
                    + "&[StatisticsByGenre]&"
                    + "\n     </StatisticsByGenre>"
                    + "\n   </Statistics>";

            String longestPlayingSongTitleXMLTemplate =
                      "\n     <LongestPlayingSongTitle>"
                    + "\n       <ArtistID></ArtistID>"
                    + "\n       <AlbumID></AlbumID>"
                    + "\n       <ArtistName></ArtistName>"
                    + "\n       <AlbumTitle></AlbumTitle>"
                    + "&[Song]&"
                    + "\n     </LongestPlayingSongTitle>";

            String shorestPlayingSongTitleXMLTemplate =
                      "\n     <ShorestPlayingSongTitle>"
                    + "\n       <ArtistID></ArtistID>"
                    + "\n       <AlbumID></AlbumID>"
                    + "\n       <ArtistName></ArtistName>"
                    + "\n       <AlbumTitle></AlbumTitle>"
                    + "&[Song]&"
                    + "\n     </ShorestPlayingSongTitle>";

            String statsByGenreXMLTemplate =
                      "\n       <Category>"
                    + "\n         <Genre></Genre>"
                    + "\n         <Artist></Artist>"
                    + "\n         <Albums></Albums>"
                    + "\n         <SongTitles></SongTitles>"
                    + "\n         <PlayTime></PlayTime>"
                    + "\n         <StorageUsage></StorageUsage>"
                    + "\n         <UFPlayTime></UFPlayTime>"
                    + "\n         <UFStorageUsage></UFStorageUsage>"
                    + "\n       </Category>";

            String artistXMLTemplate =
                      "\n   <Artist>"
                    + "\n     <ArtistID></ArtistID>"
                    + "\n     <ArtistName></ArtistName>"
                    + "\n     <NumberOfAlbums></NumberOfAlbums>"
                    + "\n     <ArtistPlayTime></ArtistPlayTime>"
                    + "\n     <ArtistStorageUsage></ArtistStorageUsage>"
                    + "\n     <GoogleSearch></GoogleSearch>"
                    + "\n     <BingSearch></BingSearch>"
                    + "\n     <StrongSearchKey></StrongSearchKey>"
                    + "\n     <WeakSearchKey></WeakSearchKey>"
                    + "\n     <NumericSearchKey></NumericSearchKey>"
                    + "\n     <NumericLowRangeSearchKey></NumericLowRangeSearchKey>"
                    + "\n     <NumericHighRangeSearchKey></NumericHighRangeSearchKey>"
                    + "&[Albums]&"
                    + "\n   </Artist>";

            String albumXMLTemplate =
                      "\n    <Album>"
                    + "\n      <AlbumID></AlbumID>"
                    + "\n      <AlbumTitle></AlbumTitle>"
                    + "\n      <AlbumGenre></AlbumGenre>"
                    + "\n      <NumberOfSongs></NumberOfSongs>"
                    + "\n      <AlbumPlayTime></AlbumPlayTime>"
                    + "\n      <AlbumStorageUsage></AlbumStorageUsage>"
                    + "\n      <UFAlbumPlayTime></UFAlbumPlayTime>"
                    + "\n      <UFAlbumStorageUsage></UFAlbumStorageUsage>"
                    + "\n      <StrongSearchKey></StrongSearchKey>"
                    + "\n      <WeakSearchKey></WeakSearchKey>"
                    + "\n      <NumericSearchKey></NumericSearchKey>"
                    + "\n      <NumericLowRangeSearchKey></NumericLowRangeSearchKey>"
                    + "\n      <NumericHighRangeSearchKey></NumericHighRangeSearchKey>"
                    + "&[Songs]&"
                    + "\n    </Album>";

            String songXMLTemplate =
                      "\n      <Song>"
                    + "\n        <SongID></SongID>"
                    + "\n        <SongTitle></SongTitle>"
                    + "\n        <Genre></Genre>"
                    + "\n        <Track></Track>"
                    + "\n        <SongPlayTime></SongPlayTime>"
                    + "\n        <Year></Year>"
                    + "\n        <MediaType></MediaType>"
                    + "\n        <MediaFormat></MediaFormat>"
                    + "\n        <SampleBitRate></SampleBitRate>"
                    + "\n        <BitRate></BitRate>"
                    + "\n        <AudioChannels></AudioChannels>"
                    + "\n        <FileName></FileName>"
                    + "\n        <FileType></FileType>"
                    + "\n        <FileLocation></FileLocation>"
                    + "\n        <FileSize></FileSize>"
                    + "\n        <FileLastModified></FileLastModified>"
                    + "\n        <UFSongPlayTime></UFSongPlayTime>"
                    + "\n        <UFFileSize></UFFileSize>"
                    + "\n        <UFFileLastModified></UFFileLastModified>"
                    + "\n        <RealHelexServerUrl></RealHelexServerUrl>"
                    + "\n        <WindowsMediaServerUrl></WindowsMediaServerUrl>"
                    + "\n        <StrongSearchKey></StrongSearchKey>"
                    + "\n        <WeakSearchKey></WeakSearchKey>"
                    + "\n        <NumericSearchKey></NumericSearchKey>"
                    + "\n        <NumericLowRangeSearchKey></NumericLowRangeSearchKey>"
                    + "\n        <NumericHighRangeSearchKey></NumericHighRangeSearchKey>"
                    + "\n      </Song>";

            // Load XML templates into memory...
            XMLWriter.LoadXMLTemplate("jobInfoXMLTemplate", jobInfoXMLTemplate);
            XMLWriter.LoadXMLTemplate("statisticsXMLTemplate", statisticsXMLTemplate);
            XMLWriter.LoadXMLTemplate("longestPlayingSongTitleXMLTemplate", longestPlayingSongTitleXMLTemplate);
            XMLWriter.LoadXMLTemplate("shorestPlayingSongTitleXMLTemplate", shorestPlayingSongTitleXMLTemplate);
            XMLWriter.LoadXMLTemplate("statsByGenreXMLTemplate", statsByGenreXMLTemplate);
            XMLWriter.LoadXMLTemplate("artistXMLTemplate", artistXMLTemplate);
            XMLWriter.LoadXMLTemplate("albumXMLTemplate", albumXMLTemplate);
            XMLWriter.LoadXMLTemplate("songXMLTemplate", songXMLTemplate);

            // Configuration based on file name + Config.xml automatically loaded...
            // Get catalog directory...
            sCatalogDirectory = GetSettings("/Music/Catalog/ScanFolder");

            // Get MP3 XML DB file name...
            sMP3XMLDBFile = GetSettings("/Music/Catalog/LibraryFile");

            // Get MP3 XML summary file name...
            sMP3XMLDBSummaryFile = GetSettings("/Music/Catalog/SummaryFile");

            // Get HelexServer Url...
            sRealPlayerServerUrl = GetSettings("/Music/PlaybackServers/RealHelexServerUrl");

            // Get MediaServer Url...
            sMediaPlayerServerUrl = GetSettings("/Music/PlaybackServers/WindowsMediaServerUrl");

            // Get Google search Url...
            sGoogleSearchUrl = GetSettings("/Music/SearchEngines/GoodleUrl");

            // Get MSN Bing search Url...
            sMSNBingSearchUrl = GetSettings("/Music/SearchEngines/BingUrl");

            // Create XML DB file...
            XMLWriter.Open(sMP3XMLDBFile);
            XMLWriter.RootNode = "MusicCatalog";
            XMLWriter.DTD = "DTD/" + pdamxUtility.StripPath(sMP3XMLDBFile, true);
            XMLWriter.Namespace = "http://www.pdamediax.com/mp3xmldb";

            //Write XML content to console stream or file...
            XMLWriter.CopyXMLTemplate("jobInfoXMLTemplate");
            XMLWriter.SetXMLTemplateElement("jobInfoXMLTemplate", "Generated", StartTime);
            XMLWriter.SetXMLTemplateElement("jobInfoXMLTemplate", "Generator", Program);
            XMLWriter.SetXMLTemplateElement("jobInfoXMLTemplate", "Machine", Machine);
            XMLWriter.SetXMLTemplateElement("jobInfoXMLTemplate", "OS", OS);
            XMLWriter.SetXMLTemplateElement("jobInfoXMLTemplate", "OSVersion", OSVersion);
            XMLWriter.Write(XMLWriter.GetXMLTemplate("jobInfoXMLTemplate"));

            //Get List of MP3'S directories (by genre)..
            diDirectoryInfo = new DirectoryInfo(sCatalogDirectory);
            DirectoryInfo[] dirInfo = diDirectoryInfo.GetDirectories();
            lSubtotals = new long[dirInfo.Count(), 5];
            sGenre = new String[dirInfo.Count()];
            XMLWriter.CopyXMLTemplate("artistXMLTemplate");
            XMLWriter.CopyXMLTemplate("albumXMLTemplate");
            mxSearchKeyGen = new pdamxSearchKeyGen();
            foreach (DirectoryInfo dir in dirInfo)
            {
                sGenre[nGenreCnt] = dir.Name;
                if (sGenre[nGenreCnt].ToLower().Equals("randb"))
                    sGenre[nGenreCnt] = "R&amp;B";
                FileInfo[] fileInfo = dir.GetFiles("*.mp3");
                Array.Sort<FileInfo>(fileInfo, delegate(FileInfo a, FileInfo b) { return getSortName(a).CompareTo(getSortName(b)); });
                foreach (FileInfo file in fileInfo)
                {
                    nFilesRead++;
                    XMLWriter.CopyXMLTemplate("songXMLTemplate");

                    mxAudioProperties = new pdamxAudioProperties(file);

                    if (bStart)
                    {
                        sPrevAlbum = stripSpeciallChars(mxAudioProperties.AlbumTitle);
                        sPreviousArtist = stripSpeciallChars(mxAudioProperties.Artist);
                        bStart = false;
                    }
                    if (sPrevAlbum.ToLower() != stripSpeciallChars(mxAudioProperties.AlbumTitle.ToLower()))
                    {
                        XMLWriter.SetXMLTemplateElement("albumXMLTemplate", "AlbumID", Convert.ToString(lAlbumID));
                        mxSearchKeyGen.GenerateKey(sPrevAlbum);
                        XMLWriter.SetXMLTemplateElement("albumXMLTemplate", "StrongSearchKey", mxSearchKeyGen.StrongKey);
                        XMLWriter.SetXMLTemplateElement("albumXMLTemplate", "WeakSearchKey", mxSearchKeyGen.WeakKey);
                        XMLWriter.SetXMLTemplateElement("albumXMLTemplate", "NumericSearchKey", mxSearchKeyGen.NumericKey);
                        XMLWriter.SetXMLTemplateElement("albumXMLTemplate", "NumericLowRangeSearchKey", mxSearchKeyGen.NumericLowRangeKey);
                        XMLWriter.SetXMLTemplateElement("albumXMLTemplate", "NumericHighRangeSearchKey", mxSearchKeyGen.NumericHighRangeKey);
                        XMLWriter.SetXMLTemplateElement("albumXMLTemplate", "AlbumGenre", sGenre[nGenreCnt]);
                        XMLWriter.SetXMLTemplateElement("albumXMLTemplate", "NumberOfSongs", Convert.ToString(nSongCnt));
                        XMLWriter.SetXMLTemplateElement("albumXMLTemplate", "AlbumPlayTime", pdamxUtility.FormatSeconds(tspAlbumPlayTime.Duration().TotalSeconds.ToString()));
                        XMLWriter.SetXMLTemplateElement("albumXMLTemplate", "AlbumStorageUsage", pdamxUtility.FormatStorageSize(Convert.ToString(lAlbumStorageUsage)));
                        XMLWriter.SetXMLTemplateElement("albumXMLTemplate", "UFAlbumStorageUsage", Convert.ToString(lAlbumStorageUsage));
                        XMLWriter.SetXMLTemplateElement("albumXMLTemplate", "UFAlbumPlayTime", tspAlbumPlayTime.Duration().TotalSeconds.ToString());
                        XMLWriter.InsertXMLAtTemplateElementMarker("artistXMLTemplate", "Albums", "albumXMLTemplate");
                        XMLWriter.CopyXMLTemplate("albumXMLTemplate");
                        sPrevAlbum = stripSpeciallChars(mxAudioProperties.AlbumTitle);
                        lAlbums++;
                        nAlbumCnt++;
                        lAlbumID = lAlbumID + nIncrementIDsBy;
                        nSongCnt = 0;
                        lAlbumStorageUsage = 0;
                        tspAlbumPlayTime = new TimeSpan();
                    }
                    if ((sPreviousArtist.ToLower() != stripSpeciallChars(mxAudioProperties.Artist.ToLower())))
                    {
                        XMLWriter.SetXMLTemplateElement("artistXMLTemplate", "ArtistID", Convert.ToString(lArtistID));
                        mxSearchKeyGen.GenerateKey(sPreviousArtist);
                        XMLWriter.SetXMLTemplateElement("artistXMLTemplate", "StrongSearchKey", mxSearchKeyGen.StrongKey);
                        XMLWriter.SetXMLTemplateElement("artistXMLTemplate", "WeakSearchKey", mxSearchKeyGen.WeakKey);
                        XMLWriter.SetXMLTemplateElement("artistXMLTemplate", "NumericSearchKey", mxSearchKeyGen.NumericKey);
                        XMLWriter.SetXMLTemplateElement("artistXMLTemplate", "NumericLowRangeSearchKey", mxSearchKeyGen.NumericLowRangeKey);
                        XMLWriter.SetXMLTemplateElement("artistXMLTemplate", "NumericHighRangeSearchKey", mxSearchKeyGen.NumericHighRangeKey);
                        XMLWriter.SetXMLTemplateElement("artistXMLTemplate", "NumberOfAlbums", Convert.ToString(nAlbumCnt));
                        XMLWriter.SetXMLTemplateElement("artistXMLTemplate", "ArtistPlayTime", pdamxUtility.FormatSeconds(Convert.ToString(lArtistPlayTime)));
                        XMLWriter.SetXMLTemplateElement("artistXMLTemplate", "ArtistStorageUsage", pdamxUtility.FormatStorageSize(Convert.ToString(lArtistStorageUsage)));
                        XMLWriter.Write(XMLWriter.GetXMLTemplate("artistXMLTemplate"));
                        XMLWriter.CopyXMLTemplate("artistXMLTemplate");
                    
                        // Artist with most song titles...
                        if (nArtistTitleCnt > nArtistWithMostTitles)
                        {
                            nArtistWithMostTitles = nArtistTitleCnt;
                            sArtistWithMostTitles = sPreviousArtist;
                        }
                        sPreviousArtist = stripSpeciallChars(mxAudioProperties.Artist);
                        lArtist++;
                        lArtistID = lArtistID + nIncrementIDsBy;
                        nAlbumCnt = 0;
                        nArtistTitleCnt = 0;
                        lArtistPlayTime = 0;
                        lArtistStorageUsage = 0;
                    }
                    XMLWriter.SetXMLTemplateElement("songXMLTemplate", "SongID", Convert.ToString(lSongID));
                    XMLWriter.SetXMLTemplateElement("songXMLTemplate", "RealHelexServerUrl", sRealPlayerServerUrl + "/" + sGenre[nGenreCnt] + "/" + mxAudioProperties.FileName);
                    XMLWriter.SetXMLTemplateElement("songXMLTemplate", "WindowsMediaServerUrl", sMediaPlayerServerUrl + "/" + sGenre[nGenreCnt] + "/" + mxAudioProperties.FileName);
                    XMLWriter.SetXMLTemplateElement("songXMLTemplate", "SongTitle", mxAudioProperties.Title);
                    XMLWriter.SetXMLTemplateElement("artistXMLTemplate", "ArtistName", mxAudioProperties.Artist);
                    if (mxAudioProperties.Artist.Length == 0)
                    {
                        XMLWriter.SetXMLTemplateElement("artistXMLTemplate", "GoogleSearch", mxAudioProperties.Artist);
                        XMLWriter.SetXMLTemplateElement("artistXMLTemplate", "BingSearch", mxAudioProperties.Artist);
                    }
                    else
                    {
                        XMLWriter.SetXMLTemplateElement("artistXMLTemplate", "GoogleSearch", sGoogleSearchUrl + mxAudioProperties.Artist);
                        XMLWriter.SetXMLTemplateElement("artistXMLTemplate", "BingSearch", sMSNBingSearchUrl + mxAudioProperties.Artist);
                    }
                    mxSearchKeyGen.GenerateKey(mxAudioProperties.Title);
                    XMLWriter.SetXMLTemplateElement("songXMLTemplate", "StrongSearchKey", mxSearchKeyGen.StrongKey);
                    XMLWriter.SetXMLTemplateElement("songXMLTemplate", "WeakSearchKey", mxSearchKeyGen.WeakKey);
                    XMLWriter.SetXMLTemplateElement("songXMLTemplate", "NumericSearchKey", mxSearchKeyGen.NumericKey);
                    XMLWriter.SetXMLTemplateElement("songXMLTemplate", "NumericLowRangeSearchKey", mxSearchKeyGen.NumericLowRangeKey);
                    XMLWriter.SetXMLTemplateElement("songXMLTemplate", "NumericHighRangeSearchKey", mxSearchKeyGen.NumericHighRangeKey);
                    XMLWriter.SetXMLTemplateElement("albumXMLTemplate", "AlbumTitle", mxAudioProperties.AlbumTitle);
                    XMLWriter.SetXMLTemplateElement("songXMLTemplate", "Track", mxAudioProperties.Track);
                    tspAlbumPlayTime = tspAlbumPlayTime.Add(mxAudioProperties.Duration);
                    lPlayTime = lPlayTime + Convert.ToInt64(mxAudioProperties.DurationInSeconds);
                    lArtistPlayTime = lArtistPlayTime + Convert.ToInt64(mxAudioProperties.DurationInSeconds);
                    XMLWriter.SetXMLTemplateElement("songXMLTemplate", "SongPlayTime", mxAudioProperties.PlayTimeFormatted);
                    XMLWriter.SetXMLTemplateElement("songXMLTemplate", "UFSongPlayTime", mxAudioProperties.PlayTimeUnformatted);
                    XMLWriter.SetXMLTemplateElement("songXMLTemplate", "Genre", mxAudioProperties.Genre);
                    XMLWriter.SetXMLTemplateElement("songXMLTemplate", "Year", mxAudioProperties.AlbumYear);
                    XMLWriter.SetXMLTemplateElement("songXMLTemplate", "SampleBitRate", mxAudioProperties.AudioSampleBitRate);
                    XMLWriter.SetXMLTemplateElement("songXMLTemplate", "BitRate", mxAudioProperties.AudioBitRate);
                    XMLWriter.SetXMLTemplateElement("songXMLTemplate", "AudioChannels", mxAudioProperties.AudioChannels);
                    XMLWriter.SetXMLTemplateElement("songXMLTemplate", "MediaType", mxAudioProperties.MediaType);
                    XMLWriter.SetXMLTemplateElement("songXMLTemplate", "MediaFormat", mxAudioProperties.MediaFormat);
                    XMLWriter.SetXMLTemplateElement("songXMLTemplate", "FileName", mxAudioProperties.FileName);
                    XMLWriter.SetXMLTemplateElement("songXMLTemplate", "FileType", mxAudioProperties.FileType);
                    XMLWriter.SetXMLTemplateElement("songXMLTemplate", "FileLocation", mxAudioProperties.FileLocation);
                    XMLWriter.SetXMLTemplateElement("songXMLTemplate", "FileSize", mxAudioProperties.FileSizeFormatted);
                    XMLWriter.SetXMLTemplateElement("songXMLTemplate", "UFFileSize", mxAudioProperties.FileSizeUnformatted);
                    lAlbumStorageUsage = lAlbumStorageUsage + mxAudioProperties.FileSize;
                    lArtistStorageUsage = lArtistStorageUsage + mxAudioProperties.FileSize;
                    lStorageUsage = lStorageUsage + mxAudioProperties.FileSize;
                    XMLWriter.SetXMLTemplateElement("songXMLTemplate", "FileLastModified", mxAudioProperties.LastWriteTimeFormatted);
                    XMLWriter.SetXMLTemplateElement("songXMLTemplate", "UFFileLastModified", mxAudioProperties.LastWriteTimeUnformatted);
                    lTitles++;
                    lSongID = lSongID + nIncrementIDsBy;
                    nArtistTitleCnt++;
                    nSongCnt++;
                    XMLWriter.InsertXMLAtTemplateElementMarker("albumXMLTemplate", "Songs", "songXMLTemplate");

                    // Longest playing song title...
                    if (mxAudioProperties.DurationInSeconds > dLongestPlayingSongTitle)
                    {
                        dLongestPlayingSongTitle = mxAudioProperties.DurationInSeconds;
                        sLongestPlaySongTitle = XMLWriter.GetXMLTemplate("songXMLTemplate");
                        sLongestPlaySongTitleArtist = mxAudioProperties.Artist;
                        sLongestPlaySongTitleArtistAlbum = mxAudioProperties.AlbumTitle;
                        lLongestPlaySongTitleArtistID = lArtistID;
                        lLongestPlaySongTitleAlbumID = lAlbumID;
                    }
                    // Shortest playing song title...
                    if (mxAudioProperties.DurationInSeconds < dShorestPlayingSongTitle)
                    {
                        dShorestPlayingSongTitle = mxAudioProperties.DurationInSeconds;
                        sShorestPlaySongTitle = XMLWriter.GetXMLTemplate("songXMLTemplate");
                        sShorestPlaySongTitleArtist = mxAudioProperties.Artist;
                        sShorestPlaySongTitleArtistAlbum = mxAudioProperties.AlbumTitle;
                        lShorestPlaySongTitleArtistID = lArtistID;
                        lShorestPlaySongTitleAlbumID = lAlbumID;
                    }
                    nFilesProcessed++;
                }
                // Accumlate Sub-totals....
                lSubtotals[nGenreCnt, 0] = lArtist;
                lSubtotals[nGenreCnt, 1] = lAlbums;
                lSubtotals[nGenreCnt, 2] = lTitles;
                lSubtotals[nGenreCnt, 3] = lPlayTime;
                lSubtotals[nGenreCnt, 4] = lStorageUsage;

                // Reset count for next genre...
                lArtist = 0;
                lAlbums = 0;
                lTitles = 0;
                lPlayTime = 0;
                lStorageUsage = 0;
                nGenreCnt++;
            }
            mxSearchKeyGen.GenerateKey(sPrevAlbum);
            XMLWriter.SetXMLTemplateElement("albumXMLTemplate", "StrongSearchKey", mxSearchKeyGen.StrongKey);
            XMLWriter.SetXMLTemplateElement("albumXMLTemplate", "WeakSearchKey", mxSearchKeyGen.WeakKey);
            XMLWriter.SetXMLTemplateElement("albumXMLTemplate", "NumericSearchKey", mxSearchKeyGen.NumericKey);
            XMLWriter.SetXMLTemplateElement("albumXMLTemplate", "NumericLowRangeSearchKey", mxSearchKeyGen.NumericLowRangeKey);
            XMLWriter.SetXMLTemplateElement("albumXMLTemplate", "NumericHighRangeSearchKey", mxSearchKeyGen.NumericHighRangeKey);
            XMLWriter.SetXMLTemplateElement("albumXMLTemplate", "AlbumID", Convert.ToString(lAlbumID));
            XMLWriter.SetXMLTemplateElement("albumXMLTemplate", "AlbumGenre", sGenre[nGenreCnt - 1]);
            XMLWriter.SetXMLTemplateElement("albumXMLTemplate", "NumberOfSongs", Convert.ToString(nSongCnt));
            XMLWriter.SetXMLTemplateElement("albumXMLTemplate", "AlbumPlayTime", pdamxUtility.FormatSeconds(tspAlbumPlayTime.Duration().TotalSeconds.ToString()));
            XMLWriter.SetXMLTemplateElement("albumXMLTemplate", "AlbumStorageUsage", pdamxUtility.FormatStorageSize(Convert.ToString(lAlbumStorageUsage)));
            XMLWriter.SetXMLTemplateElement("albumXMLTemplate", "UFAlbumStorageUsage", Convert.ToString(lAlbumStorageUsage));
            XMLWriter.SetXMLTemplateElement("albumXMLTemplate", "UFAlbumPlayTime", tspAlbumPlayTime.Duration().TotalSeconds.ToString());
            XMLWriter.InsertXMLAtTemplateElementMarker("artistXMLTemplate", "Albums", "albumXMLTemplate");
            XMLWriter.SetXMLTemplateElement("artistXMLTemplate", "NumberOfAlbums", Convert.ToString(nAlbumCnt));
            XMLWriter.Write(XMLWriter.GetXMLTemplate("artistXMLTemplate"));
            XMLWriter.Close();

            XMLWriter.Open(sMP3XMLDBSummaryFile);
            XMLWriter.RootNode = "MusicCatalogSummary";
            XMLWriter.DTD = "DTD/" + pdamxUtility.StripPath(sMP3XMLDBSummaryFile, true);
            XMLWriter.Namespace = "http://www.pdamediax.com/mp3xmldbsummary";
            XMLWriter.Write(XMLWriter.GetXMLTemplate("jobInfoXMLTemplate"));
            XMLWriter.CopyXMLTemplate("statisticsXMLTemplate");

            // Build by genre xml subtotal section...
            for (int i = 0; i < nGenreCnt; i++)
            {
                XMLWriter.CopyXMLTemplate("statsByGenreXMLTemplate");
                XMLWriter.SetXMLTemplateElement("statsByGenreXMLTemplate", "Genre", sGenre[i]);
                XMLWriter.SetXMLTemplateElement("statsByGenreXMLTemplate", "Artist", Convert.ToString(lSubtotals[i, 0]));
                XMLWriter.SetXMLTemplateElement("statsByGenreXMLTemplate", "Albums", Convert.ToString(lSubtotals[i, 1]));
                XMLWriter.SetXMLTemplateElement("statsByGenreXMLTemplate", "SongTitles", Convert.ToString(lSubtotals[i, 2]));
                XMLWriter.SetXMLTemplateElement("statsByGenreXMLTemplate", "PlayTime", pdamxUtility.FormatSeconds(Convert.ToString(lSubtotals[i, 3])));
                XMLWriter.SetXMLTemplateElement("statsByGenreXMLTemplate", "StorageUsage", pdamxUtility.FormatStorageSize(Convert.ToString(lSubtotals[i, 4])));
                XMLWriter.SetXMLTemplateElement("statsByGenreXMLTemplate", "UFPlayTime", Convert.ToString(lSubtotals[i, 3]));
                XMLWriter.SetXMLTemplateElement("statsByGenreXMLTemplate", "UFStorageUsage", Convert.ToString(lSubtotals[i, 4]));
                XMLWriter.InsertXMLAtTemplateElementMarker("statisticsXMLTemplate", "StatisticsByGenre", "statsByGenreXMLTemplate");

                lTotArtist = lTotArtist + lSubtotals[i, 0];
                lTotAlbums = lTotAlbums + lSubtotals[i, 1];
                lTotTitles = lTotTitles + lSubtotals[i, 2];
                lTotPlayTime = lTotPlayTime + lSubtotals[i, 3];
                lTotStorageUsage = lTotStorageUsage + lSubtotals[i, 4];

                if (lSubtotals[i, 2] > lGenreWithMostTitles)
                {
                    lGenreWithMostTitles = lSubtotals[i, 2];
                    sGenreWithMostTitles = sGenre[i];
                }
                if (lSubtotals[i, 4] > lGenreUsingMostStorage)
                {
                    lGenreUsingMostStorage = lSubtotals[i, 4];
                    sGenreUsingMostStorage = sGenre[i];
                }
                if (lSubtotals[i, 3] > lLongestPlayingGenre)
                {
                    lLongestPlayingGenre = lSubtotals[i, 3];
                    sLongestPlayingGenre = sGenre[i];
                }
            }
            XMLWriter.SetXMLTemplateElement("statisticsXMLTemplate", "Artist", Convert.ToString(lTotArtist));
            XMLWriter.SetXMLTemplateElement("statisticsXMLTemplate", "Albums", Convert.ToString(lTotAlbums));
            XMLWriter.SetXMLTemplateElement("statisticsXMLTemplate", "SongTitles", Convert.ToString(lTotTitles));
            XMLWriter.SetXMLTemplateElement("statisticsXMLTemplate", "Genres", Convert.ToString(nGenreCnt));
            XMLWriter.SetXMLTemplateElement("statisticsXMLTemplate", "PlayTime", pdamxUtility.FormatSeconds(Convert.ToString(lTotPlayTime)));
            XMLWriter.SetXMLTemplateElement("statisticsXMLTemplate", "StorageUsage", pdamxUtility.FormatStorageSize(Convert.ToString(lTotStorageUsage)));
            XMLWriter.SetXMLTemplateElement("statisticsXMLTemplate", "UFPlayTime", Convert.ToString(lTotPlayTime));
            XMLWriter.SetXMLTemplateElement("statisticsXMLTemplate", "UFStorageUsage", Convert.ToString(lTotStorageUsage));
            XMLWriter.SetXMLTemplateElement("statisticsXMLTemplate", "StatisticsByGenre", sSubTotalList);
            XMLWriter.SetXMLTemplateElement("statisticsXMLTemplate", "ArtistWithMostTitles", sArtistWithMostTitles);
            XMLWriter.SetXMLTemplateElementAttribute("statisticsXMLTemplate", "ArtistWithMostTitles", "NumberOfSongs", Convert.ToString(nArtistWithMostTitles));
            XMLWriter.SetXMLTemplateElement("statisticsXMLTemplate", "GenreWMostTitles", Convert.ToString(lGenreWithMostTitles));
            XMLWriter.SetXMLTemplateElementAttribute("statisticsXMLTemplate", "GenreWMostTitles", "Genre", sGenreWithMostTitles);
            XMLWriter.SetXMLTemplateElement("statisticsXMLTemplate", "GenreUsingMostStorage", pdamxUtility.FormatStorageSize(Convert.ToString(lGenreUsingMostStorage)));
            XMLWriter.SetXMLTemplateElementAttribute("statisticsXMLTemplate", "GenreUsingMostStorage", "Genre", sGenreUsingMostStorage);
            XMLWriter.SetXMLTemplateElement("statisticsXMLTemplate", "LongestPlayingGenre", pdamxUtility.FormatSeconds(Convert.ToString(lLongestPlayingGenre)));
            XMLWriter.SetXMLTemplateElementAttribute("statisticsXMLTemplate", "LongestPlayingGenre", "Genre", sLongestPlayingGenre);
            XMLWriter.CopyXMLTemplate("longestPlayingSongTitleXMLTemplate");

            XMLWriter.SetXMLTemplateElement("longestPlayingSongTitleXMLTemplate", "ArtistID", Convert.ToString(lLongestPlaySongTitleArtistID));
            XMLWriter.SetXMLTemplateElement("longestPlayingSongTitleXMLTemplate", "AlbumID", Convert.ToString(lLongestPlaySongTitleAlbumID));
            XMLWriter.SetXMLTemplateElement("longestPlayingSongTitleXMLTemplate", "ArtistName", sLongestPlaySongTitleArtist);
            XMLWriter.SetXMLTemplateElement("longestPlayingSongTitleXMLTemplate", "AlbumTitle", sLongestPlaySongTitleArtistAlbum);
            XMLWriter.ReplactXMPTemplateElementMarker("longestPlayingSongTitleXMLTemplate","Song", sLongestPlaySongTitle);
            XMLWriter.CopyXMLTemplate("shorestPlayingSongTitleXMLTemplate");
            
            XMLWriter.SetXMLTemplateElement("shorestPlayingSongTitleXMLTemplate", "ArtistID", Convert.ToString(lShorestPlaySongTitleArtistID));
            XMLWriter.SetXMLTemplateElement("shorestPlayingSongTitleXMLTemplate", "AlbumID", Convert.ToString(lShorestPlaySongTitleAlbumID));
            XMLWriter.SetXMLTemplateElement("shorestPlayingSongTitleXMLTemplate", "ArtistName", sShorestPlaySongTitleArtist);
            XMLWriter.SetXMLTemplateElement("shorestPlayingSongTitleXMLTemplate", "AlbumTitle", sShorestPlaySongTitleArtistAlbum);
            XMLWriter.ReplactXMPTemplateElementMarker("shorestPlayingSongTitleXMLTemplate", "Song", sShorestPlaySongTitle);
            XMLWriter.ReplactXMPTemplateElementMarker("statisticsXMLTemplate","LongestPlaySongTitle", "longestPlayingSongTitleXMLTemplate");
            XMLWriter.ReplactXMPTemplateElementMarker("statisticsXMLTemplate","ShorestPlaySongTitle", "shorestPlayingSongTitleXMLTemplate");
            XMLWriter.Write(XMLWriter.GetXMLTemplate("statisticsXMLTemplate"));
            XMLWriter.Close();


            // Job Summary...
            WriteEndofJobSummaryToFile = true;
            AddSummaryExtra("");
            AddSummaryExtra("Music Library (MP3's) Processing Summary");
            AddSummaryExtra("");
            AddSummaryExtra("  Music Directory:        " + diDirectoryInfo.FullName);
            AddSummaryExtra("");
            AddSummaryExtra("  Music files Read:       " + pdamxUtility.FormatNumber(nFilesRead));
            AddSummaryExtra("  Music files Processed:  " + pdamxUtility.FormatNumber(nFilesProcessed));
            AddSummaryExtra("  Storage Used:           " + pdamxUtility.FormatStorageSize(Convert.ToString(lTotStorageUsage)));
            AddSummaryExtra("  Total Playtime:         " + pdamxUtility.FormatSecondsInText(Convert.ToString(lTotPlayTime)));

            AddSummaryExtra("");
            AddSummaryExtra("  Number of Artist:       " + pdamxUtility.FormatNumber(Convert.ToInt32(lTotArtist)));
            AddSummaryExtra("  Number of Albums:       " + pdamxUtility.FormatNumber(Convert.ToInt32(lTotAlbums)));
            AddSummaryExtra("  Number of Titles:       " + pdamxUtility.FormatNumber(Convert.ToInt32(lTotTitles)));

            fiFileSummary = new FileInfo(sMP3XMLDBFile);
            AddSummaryExtra("");
            AddSummaryExtra("Music Library (MP3's) XML Data File Information");
            AddSummaryExtra("");
            AddSummaryExtra("  Name:      " + fiFileSummary.Name);
            AddSummaryExtra("  Location:  " + fiFileSummary.Directory);
            AddSummaryExtra("  Size:      " + pdamxUtility.FormatStorageSize(Convert.ToString(fiFileSummary.Length)));
            AddSummaryExtra("  Created:   " + fiFileSummary.LastWriteTime.ToString("MM/dd/yyyy (hh:mm:ss tt)", dtFormat));

            fiFileSummary = new FileInfo(sMP3XMLDBSummaryFile);
            AddSummaryExtra("");
            AddSummaryExtra("Music Library (MP3's) XML Data File Summary File Information");
            AddSummaryExtra("");
            AddSummaryExtra("  Name:      " + fiFileSummary.Name);
            AddSummaryExtra("  Location:  " + fiFileSummary.Directory);
            AddSummaryExtra("  Size:      " + pdamxUtility.FormatStorageSize(Convert.ToString(fiFileSummary.Length)));
            AddSummaryExtra("  Created:   " + fiFileSummary.LastWriteTime.ToString("MM/dd/yyyy (hh:mm:ss tt)", dtFormat));
            PrintEndofJobSummary();
        }
        private String getSortName(FileInfo fiFile)
        {
            String sArtist = "";
            String sSongTitle = "";
            String sAlbumTitle = "";

            pdamxAudioProperties mxAudioProperties = new pdamxAudioProperties(fiFile);
            sArtist = mxAudioProperties.Artist;
            sSongTitle = mxAudioProperties.Title;
            sAlbumTitle = mxAudioProperties.AlbumTitle;
            return (stripSpeciallChars(sArtist) + ":" + stripSpeciallChars(sAlbumTitle) + ":" + String.Format("{0:00}", mxAudioProperties.Track) + ":" + stripSpeciallChars(sSongTitle));
        }
        private String stripSpeciallChars(String sField)
        {
            String sReturn = sField;

            if (sField == null)
                return (null);
            sReturn = sReturn.Replace("'", "");
            sReturn = sReturn.Replace("=", "");
            sReturn = sReturn.Replace("-", "");
            sReturn = sReturn.Replace("(", "");
            sReturn = sReturn.Replace(")", "");
            sReturn = sReturn.Trim();
            return (sReturn);
        }
    }
}
