using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml.XPath;

namespace MediaPlayerListGen
{
    class MediaPlayerListGen : pdaMediaX.pdamxBatchJob
    {
        int nRecordsRead = 0;
        int nRecordsProcessed = 0;

        static void Main(string[] args)
        {
            new MediaPlayerListGen();
        }
        public MediaPlayerListGen()
        {
            XPathNodeIterator xpathINode = null;

            String sServerUrl = "";
            String sPlayListFileXT = "";
            String sPlayListFile = "";
            String sPlayListDirectory = "";
            String sGenre = "";

            int nPlayListRead = 0;
            int nPlayListRecordsProcessed = 0;

            TextWriter tfwOutFile = null;
            // Configuration based on file name + Config.xml automatically loaded...
 
            // Get playlist file extension...
            //sPlayListFileXT = GetSettings("/WMPlayer/IISServer/PlayListFileExtension");

            // Get playlist directory...
            sPlayListDirectory = GetSettings("/WMPlayer/IISServer/PlayListFolder");

            // Get Windows Stream Media...
            sServerUrl = "mms://" + GetSettings("/WMPlayer/IISServer/PlayerUrl");
            sPlayListFileXT = "asx";
            // Get directories to scan for MP3's...
            xpathINode = SettingsObject.GetNodePath("/WMPlayer/Catalog/ScanFolder/*");
            while (xpathINode.MoveNext())
            {
                if (xpathINode.Current.Name.Equals("Genre"))
                {
                    sPlayListFile = xpathINode.Current.Value + "." + sPlayListFileXT;
                    sGenre = xpathINode.Current.Value;
                }
                if (xpathINode.Current.Name.Equals("Location"))
                {
                    writePlayEntries(null, xpathINode.Current.Value, sPlayListDirectory, sServerUrl, sPlayListFile, sGenre, null);
                }
            }
            // Get favorites playlist....
            xpathINode = SettingsObject.GetNodePath("/WMPlayer/Playlist/Favorite/*");
            while (xpathINode.MoveNext())
            {
                if (xpathINode.Current.Name.Equals("ListName"))
                {
                    if (tfwOutFile != null)
                        tfwOutFile.Close();
                    tfwOutFile = new StreamWriter(sPlayListDirectory + "\\" + xpathINode.Current.Value + "." + sPlayListFileXT);
                    nPlayListRead++;
                }
                if (xpathINode.Current.Name.Equals("MusicFile"))
                {
                    sPlayListFile = xpathINode.Current.Value + ".mp3";
                    tfwOutFile.WriteLine(sServerUrl + "/OnDemand" + sPlayListFile);
                    nPlayListRecordsProcessed++;
                }
            }
            if (tfwOutFile != null)
                tfwOutFile.Close();

            // Get Windows Stream Media...
            sServerUrl = "rtsp://" + GetSettings("/WMPlayer/IISServer/PlayerUrl");
            sPlayListFileXT = "ram";
            // Get directories to scan for MP3's...
            xpathINode = SettingsObject.GetNodePath("/WMPlayer/Catalog/ScanFolder/*");
            while (xpathINode.MoveNext())
            {
                if (xpathINode.Current.Name.Equals("Genre"))
                {
                    sPlayListFile = xpathINode.Current.Value + "." + sPlayListFileXT;
                    sGenre = xpathINode.Current.Value;
                }
                if (xpathINode.Current.Name.Equals("Location"))
                {
                    writePlayEntries(null, xpathINode.Current.Value, sPlayListDirectory, sServerUrl, sPlayListFile, sGenre, null);
                }
            }
            // Get favorites playlist....
            xpathINode = SettingsObject.GetNodePath("/WMPlayer/Playlist/Favorite/*");
            while (xpathINode.MoveNext())
            {
                if (xpathINode.Current.Name.Equals("ListName"))
                {
                    if (tfwOutFile != null)
                        tfwOutFile.Close();
                    tfwOutFile = new StreamWriter(sPlayListDirectory + "\\" + xpathINode.Current.Value + "." + sPlayListFileXT);
                    nPlayListRead++;
                }
                if (xpathINode.Current.Name.Equals("MusicFile"))
                {
                    sPlayListFile = xpathINode.Current.Value + ".mp3";
                    tfwOutFile.WriteLine(sServerUrl + "/OnDemand" + sPlayListFile);
                    nPlayListRecordsProcessed++;
                }
            }
            if (tfwOutFile != null)
                tfwOutFile.Close();

            WriteEndofJobSummaryToFile = true;
            AddSummaryExtra("");
            AddSummaryExtra("Windows Streaming Media Playlist Generator Processing Summary");
            AddSummaryExtra("");
            AddSummaryExtra("  Number of Music Files Read:                " + nRecordsRead);
            AddSummaryExtra("  Number of Music Files Processed:           " + nRecordsProcessed);
            AddSummaryExtra("  Number of Playlist Read:                   " + nPlayListRead);
            AddSummaryExtra("  Number of Playlist Music Files Processed:  " + nPlayListRecordsProcessed);
            PrintEndofJobSummary();
        }
        public void writePlayEntries(TextWriter _tfwOutFile, String _ScanDirectory, String _sPlayListDirectory, String _sServerUrl, String _sPlayListFile, String _sGenre, String _sSubDir)
        {
            DirectoryInfo diDirectoryInfo;
            TextWriter tfwOutFile;
            String sSubDir = "";

            if (_sSubDir != null)
                sSubDir = "/" + _sSubDir;
            //Get List of MP3'S in directory..
            diDirectoryInfo = new DirectoryInfo(_ScanDirectory);
            if (_tfwOutFile == null)
            {
                tfwOutFile = new StreamWriter(_sPlayListDirectory + "\\" + _sPlayListFile);
                //tfwOutFile.WriteLine("<ASX VERSION = \"3.0\"><TITLE>pdaMEDIAX.com - Catalog selection " + _sGenre + "</TITLE>");
            }
            else
                tfwOutFile = _tfwOutFile;

            DirectoryInfo[] rgDirs = diDirectoryInfo.GetDirectories();
            foreach (DirectoryInfo fileInfo in rgDirs)
            {
                nRecordsRead++;
                writePlayEntries(tfwOutFile, fileInfo.FullName, _sPlayListDirectory, _sServerUrl, _sPlayListFile, _sGenre, fileInfo.Name);
            }
            FileInfo[] rgFiles = diDirectoryInfo.GetFiles("*.mp3");
            foreach (FileInfo fileInfo in rgFiles)
            {
                nRecordsRead++;
//                tfwOutFile.WriteLine("<ENTRY><REF HREF = \"" + _sServerUrl + "/" + _sGenre + sSubDir + "/" + fileInfo.Name + "\"/></ENTRY>");
                tfwOutFile.WriteLine(_sServerUrl + "/OnDemand/"+ (_sGenre == "All" ? "" : _sGenre) + sSubDir + "/" + fileInfo.Name);      
                nRecordsProcessed++;
            }
            if (_tfwOutFile == null) // Close file if calling routine didn't create it...
            {
//                tfwOutFile.WriteLine("</ASX>");
                tfwOutFile.Close();
            }
        }
    }
}
