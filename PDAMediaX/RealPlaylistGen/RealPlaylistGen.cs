using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml.XPath;

namespace RealPlaylistGen 
{
    class RealPlaylistGen : pdaMediaX.pdamxBatchJob
    {
        int nRecordsRead = 0;
        int nRecordsProcessed = 0;

        static void Main(string[] args)
        {
            new RealPlaylistGen();
        }
        public RealPlaylistGen()
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
            // Get BeyondTV network license...
            sServerUrl = GetSettings("/RealOne/HelexServer/Url");

            // Get Real Server playlist file extension...
            sPlayListFileXT = GetSettings("/RealOne/HelexServer/PlayListFileExtension");

            // Get playlist directory...
            sPlayListDirectory = GetSettings("/RealOne/HelexServer/PlayListFolder");

            // Get directories to scan for MP3's...
            xpathINode = SettingsObject.GetNodePath("/RealOne/Catalog/ScanFolder/*");
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
            xpathINode = SettingsObject.GetNodePath("/RealOne/Playlist/Favorite/*");
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
            AddSummaryExtra("Realplayer Playlist Generator Processing Summary");
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
                tfwOutFile = new StreamWriter(_sPlayListDirectory + "\\" + _sPlayListFile);
            else
                tfwOutFile = _tfwOutFile;

            DirectoryInfo [] rgDirs = diDirectoryInfo.GetDirectories();
            foreach (DirectoryInfo fileInfo in rgDirs)
            {
                nRecordsRead++;
                writePlayEntries(tfwOutFile, fileInfo.FullName, _sPlayListDirectory, _sServerUrl, _sPlayListFile, _sGenre, fileInfo.Name);
            }
            FileInfo[] rgFiles = diDirectoryInfo.GetFiles("*.mp3");
            foreach (FileInfo fileInfo in rgFiles)
            {
                nRecordsRead++;
                tfwOutFile.WriteLine(_sServerUrl + "/" + _sGenre + sSubDir + "/" + fileInfo.Name);
                nRecordsProcessed++;
            }
            if (_tfwOutFile == null) // Close file if calling routine didn't create it...
                tfwOutFile.Close();
        }
    }
}
