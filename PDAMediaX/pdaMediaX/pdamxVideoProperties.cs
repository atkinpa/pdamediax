using System;
using System.IO;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using Toub.MediaCenter.Dvrms.Metadata;
using pdaMediaX.Common;

namespace pdaMediaX.Media
{
    public class pdamxVideoProperties
    {
        DvrmsMetadataEditor dmeFMp3File;
        FileInfo fiVideoFileInfo;

        String sTitle;
        String sSeries;
        String sSeason;
        String sEpisode;
        String sEpisodeTitle;
        String sDescription;
        String sGenre;
        String sNetworkAffiliation;
        String sChannel;
        String sStationCallSign;
        String sStationName;
        String sCredits;
        String sParentalRating;
        String sParentalRatingReason;
        String sProviderRating;
        String sMovieYear;
        String sPlayTime;
        String sUFPlayTime;
        String sIsHDContent;
        String sIsDTVContent;
        String sClosedCaptioningPresent;
        int nDurationInSeconds;

        public pdamxVideoProperties(String _sVideoFile)
        {
            if (_sVideoFile == null)
                return;

            if (_sVideoFile.Trim().Length == 0)
                return;

            fiVideoFileInfo = new FileInfo(_sVideoFile);
            if (fiVideoFileInfo.Extension.Trim().ToUpper().Replace(".", "").Equals("DVR-MS"))
                SetDVRMSProperties();
            else
                SetNonDVRMSProperties();
        }
        public pdamxVideoProperties(FileInfo _fiVideoFileInfo)
        {
            if (_fiVideoFileInfo == null)
                return;

            fiVideoFileInfo = _fiVideoFileInfo;
            if (fiVideoFileInfo.Extension.Trim().ToUpper().Replace(".", "").Equals("DVR-MS"))
                SetDVRMSProperties();
            else
                SetNonDVRMSProperties();
        }
        public String CleanupCredits(String sCredits)
        {
            String sCreditFilter = sCredits.Replace("/", ";");

            sCreditFilter = sCreditFilter.Replace(",", ";");

            return (sCreditFilter);
        }
        public String CleanupRatings(String sRatings)
        {
            String sFilteredRatings;

            sFilteredRatings = sRatings.Replace("PG-", "PG");
            sFilteredRatings = sFilteredRatings.Replace("*;", "");
            sFilteredRatings = sFilteredRatings.Replace("*", "");
            sFilteredRatings = sFilteredRatings.Replace("½", "");
            if (sFilteredRatings.Length > 0)
            {
                if (sFilteredRatings.Substring(0, 1).Equals(";")
                    || sFilteredRatings.Substring(sFilteredRatings.Length-1, 1).Equals(";"))
                {
                    int nStartIdx = sFilteredRatings.Substring(0, 1).Equals(";") ? 1 : 0;
                    int nEndChar = sFilteredRatings.Substring(sFilteredRatings.Length-1, 1).Equals(";") ? 1:0;
                    sFilteredRatings = sFilteredRatings.Substring(nStartIdx, (sFilteredRatings.Length - nStartIdx) - nEndChar);
                }
            }
            return (sFilteredRatings);
        }
        public String CleanupProviderRatings(String sRatings)
        {
            String sFilteredRatings;

            sFilteredRatings = sRatings.Replace("PG-", "");
            sFilteredRatings = sFilteredRatings.Replace("*;", "*");
            if (sFilteredRatings.Length > 0)
            {
                if (sFilteredRatings.Substring(0, 1).Equals(";")
                    || sFilteredRatings.Substring(sFilteredRatings.Length - 1, 1).Equals(";"))
                {
                    int nStartIdx = sFilteredRatings.Substring(0, 1).Equals(";") ? 1 : 0;
                    int nEndChar = sFilteredRatings.Substring(sFilteredRatings.Length - 1, 1).Equals(";") ? 1 : 0;
                    sFilteredRatings = sFilteredRatings.Substring(nStartIdx, (sFilteredRatings.Length - nStartIdx) - nEndChar);
                }
            }
            return (sFilteredRatings);
        }
        private String GetSeries(String sFileName, String sGenre)
        {
            String sSeries = null;
            int nStartIdx;
            int nLen;

            if (sFileName.ToUpper().Contains("\\SERIES\\" + sGenre.ToUpper() + "\\"))
            {
                nStartIdx = sFileName.ToUpper().IndexOf("\\SERIES\\" + sGenre.ToUpper() + "\\") + (9 + sGenre.Length);
                nLen = sFileName.IndexOf("\\", nStartIdx) - nStartIdx;
                sSeries = sFileName.Substring(nStartIdx, nLen).Trim();
            }
            if (sFileName.ToUpper().Contains("\\SPECIALS\\" + sGenre.ToUpper() + "\\"))
            {
                nStartIdx = sFileName.ToUpper().IndexOf("\\SPECIALS\\" + sGenre.ToUpper() + "\\") + (11 + sGenre.Length);
                nLen = sFileName.IndexOf("\\", nStartIdx) - nStartIdx;
                if (nLen > -1)
                    sSeries = sFileName.Substring(nStartIdx, nLen).Trim();
                else
                    sSeries = Name;

            }
            return (sSeries);
        }
        private String GetSeason(String sFileName)
        {
            String sSeason = null;
            int nStartIdx;
            int nLen;

            if (sFileName.ToUpper().Contains("\\SEASON "))
            {
                nStartIdx = sFileName.ToUpper().IndexOf("\\SEASON ");
                nLen = sFileName.IndexOf("\\", nStartIdx + 2) - nStartIdx;
                sSeason = sFileName.Substring(nStartIdx + 1, nLen - 1).Trim();
            }
            if (sFileName.ToUpper().Contains("\\BOOK ONE "))
            {
                nStartIdx = sFileName.ToUpper().IndexOf("\\BOOK ONE ");
                nLen = sFileName.IndexOf("\\", nStartIdx + 2) - nStartIdx;
                sSeason = sFileName.Substring(nStartIdx + 1, nLen - 1).Trim();
            }
            if (sFileName.ToUpper().Contains("\\BOOK TWO "))
            {
                nStartIdx = sFileName.ToUpper().IndexOf("\\BOOK TWO ");
                nLen = sFileName.IndexOf("\\", nStartIdx + 2) - nStartIdx;
                sSeason = sFileName.Substring(nStartIdx + 1, nLen - 1).Trim();
            }
            if (sFileName.ToUpper().Contains("\\BOOK THREE "))
            {
                nStartIdx = sFileName.ToUpper().IndexOf("\\BOOK THREE ");
                nLen = sFileName.IndexOf("\\", nStartIdx + 2) - nStartIdx;
                sSeason = sFileName.Substring(nStartIdx + 1, nLen - 1).Trim();
            }
            return (sSeason);
        }
        private String GetEpisode(String sFileName)
        {
            String sEpisode = null;
            int nStartIdx;
            int nLen;

            if (sFileName.ToUpper().Contains("''EPISODE "))
            {
                nStartIdx = sFileName.ToUpper().IndexOf("''EPISODE ");
                nLen = sFileName.ToUpper().IndexOf(", ", nStartIdx + 2) - nStartIdx;
                if (nLen < 0)
                    nLen = sFileName.ToUpper().IndexOf("- ", nStartIdx + 2) - nStartIdx;
                if (nLen < 0)
                    nLen = sFileName.IndexOf("''", nStartIdx + 2) - nStartIdx;
                sEpisode = sFileName.Substring(nStartIdx + 2, nLen - 2).Trim();
            }
            if (sFileName.ToUpper().Contains("''CH."))
            {
                nStartIdx = sFileName.ToUpper().IndexOf("''CH.");
                nLen = sFileName.IndexOf("- ", nStartIdx + 2) - nStartIdx;
                if (nLen < 0)
                    nLen = sFileName.IndexOf("''", nStartIdx + 2) - nStartIdx;
                sEpisode = sFileName.Substring(nStartIdx + 2, nLen - 2).Trim();
            }
            if (sFileName.ToUpper().Contains("''STAGE "))
            {
                nStartIdx = sFileName.ToUpper().IndexOf("''STAGE ");
                nLen = sFileName.IndexOf(", ", nStartIdx + 2) - nStartIdx;
                if (nLen < 0)
                    nLen = sFileName.IndexOf("''", nStartIdx + 2) - nStartIdx;
                sEpisode = sFileName.Substring(nStartIdx + 2, nLen - 2).Trim();
            }
            if (sFileName.ToUpper().Contains("''SESSION "))
            {
                nStartIdx = sFileName.ToUpper().IndexOf("''SESSION ");
                nLen = sFileName.IndexOf(", ", nStartIdx + 2) - nStartIdx;
                if (nLen < 0)
                    nLen = sFileName.IndexOf("''", nStartIdx + 2) - nStartIdx;
                sEpisode = sFileName.Substring(nStartIdx + 2, nLen - 2).Trim();
            }
            return (sEpisode);
        }
        private String GetEpisodeTitle(String sFileName)
        {
            String sEpisodeTitle = null;
            int nStartIdx;
            int nLen;

            try
            {
                if (sFileName.ToUpper().Contains("''EPISODE ")
                    || sFileName.ToUpper().Contains("''SESSION ")
                    || sFileName.ToUpper().Contains("''STAGE "))
                {
                    nStartIdx = sFileName.IndexOf(", ");
                    if (nStartIdx < 0)
                        nStartIdx = sFileName.IndexOf("- ");
                    if (nStartIdx < 0)
                        nStartIdx = sFileName.IndexOf(" (");
                    if (nStartIdx < 0)
                        nStartIdx = sFileName.IndexOf("''");
                    nLen = sFileName.IndexOf("''", nStartIdx + 1) - nStartIdx;
                    if (nLen == 1)
                        nLen = sFileName.IndexOf("''", nStartIdx + 2) - nStartIdx;
                    sEpisodeTitle = sFileName.Substring(nStartIdx + 2, nLen - 2).Trim();
                    if (sEpisodeTitle.Trim().Length == 0)
                    {
                        nStartIdx = sFileName.IndexOf("''");
                        nLen = sFileName.IndexOf("''", nStartIdx + 1) - nStartIdx;
                        if (nLen == 1)
                            nLen = sFileName.IndexOf("''", nStartIdx + 2) - nStartIdx;
                        sEpisodeTitle = sFileName.Substring(nStartIdx + 2, nLen - 2).Trim();
                    }
                }
                else if (sFileName.ToUpper().Contains("''CH. "))
                {
                    nStartIdx = sFileName.IndexOf("- ");
                    if (nStartIdx < 0)
                        nStartIdx = sFileName.IndexOf("''");
                    nLen = sFileName.IndexOf("''", nStartIdx + 1) - nStartIdx;
                    sEpisodeTitle = sFileName.Substring(nStartIdx + 2, nLen - 2).Trim();
                }
                else
                {
                    nStartIdx = sFileName.IndexOf("''");
                    nLen = sFileName.IndexOf("''", nStartIdx + 1) - nStartIdx;
                    if (nLen == 1)
                        nLen = sFileName.IndexOf("''", nStartIdx + 2) - nStartIdx;
                    if (nLen == 0)
                    {
                        nStartIdx = sFileName.IndexOf(" - ");
                        nLen = sFileName.IndexOf(".", nStartIdx + 2) - nStartIdx;
                    }
                    sEpisodeTitle = sFileName.Substring(nStartIdx + 2, nLen - 2).Trim();
                    if (sEpisodeTitle.Length > Name.Length)
                        sEpisodeTitle = Name;
                }
            }
            catch (Exception)
            {
                sEpisodeTitle = null;
            }
            return (sEpisodeTitle);
        }
        private void SetDVRMSProperties()
        {
            IDictionary idMetaData;
            IDictionaryEnumerator ideMetaData;
            sClosedCaptioningPresent = "No";
            sIsHDContent = "No";
            sIsDTVContent = "No";
            if (VideoCatagory.ToUpper().Equals("SERIES")
                || VideoCatagory.ToUpper().Equals("SPECIALS"))
            {
                sSeries = GetSeries(FullFileName, FileGenre);
                sSeason = GetSeason(FullFileName);
                sEpisode = GetEpisode(FullFileName);
                sEpisodeTitle = GetEpisodeTitle(FullFileName);
                if (sSeason == null)
                    sSeason = "N/A";

                if (sEpisodeTitle == null)
                    sEpisodeTitle = Name;

                if (sEpisode == null)
                    sEpisode = "N/A";
            }
            try
            {
                dmeFMp3File = new DvrmsMetadataEditor(FullFileName);
                idMetaData = dmeFMp3File.GetAttributes();
                ideMetaData = idMetaData.GetEnumerator();

                while (ideMetaData.MoveNext())
                {
                    MetadataItem miData = (MetadataItem)ideMetaData.Value;
                    if (VideoCatagory.ToUpper().Equals("MOVIES"))
                    {
                        if (miData.Name.Equals("Title"))
                            sTitle = miData.Value.ToString();
                    }
                    if (VideoCatagory.ToUpper().Equals("SERIES")
                        || VideoCatagory.ToUpper().Equals("SPECIALS"))
                    {
                        if (miData.Name.Equals("WM/SubTitle"))
                        {
                            if (miData.Value.ToString().Trim() == "")
                            {
                                if (sEpisode != "N/A")
                                    sTitle = sEpisodeTitle;
                            }
                            else
                                sTitle = miData.Value.ToString();
                        }
                    }
                    if (miData.Name.Equals("WM/SubTitleDescription"))
                        sDescription = miData.Value.ToString();
                    if (miData.Name.Equals("WM/Genre"))
                    {
                        String [] sGenreExclude = {"/Movies", "/Movies.", "Movies.", "Movies", "/Series", "/Series.", "Series.", "Series"};
                        sGenre = miData.Value.ToString();
                        for (int i = 0; i < sGenreExclude.Length; i++)
                            sGenre = sGenre.Replace(sGenreExclude[i], "");
                    }
                    if (miData.Name.Equals("WM/MediaNetworkAffiliation"))
                        sNetworkAffiliation = miData.Value.ToString();
                    if (miData.Name.Equals("WM/MediaOriginalChannel"))
                        sChannel = miData.Value.ToString();
                    if (miData.Name.Equals("WM/MediaStationCallSign"))
                    {
                        if (miData.Value.ToString().Equals("Edited with VideoReDo Plus"))
                        {
                            sStationCallSign = "TIVO";
                            sStationName = "Tivo-Extract";
                        }
                        else
                            sStationCallSign = miData.Value.ToString();
                    }
                    if (miData.Name.Equals("WM/MediaStationName"))
                        sStationName = miData.Value.ToString();
                    if (miData.Name.Equals("WM/MediaCredits"))
                        sCredits = CleanupCredits(miData.Value.ToString());
                    if (miData.Name.Equals("WM/ParentalRating"))
                        sParentalRating = CleanupRatings(miData.Value.ToString());
                    if (miData.Name.Equals("WM/ParentalRatingReason"))
                        sParentalRatingReason = CleanupRatings(miData.Value.ToString());
                    if (miData.Name.Equals("WM/ProviderRating"))
                        sProviderRating = CleanupProviderRatings(miData.Value.ToString());
                    if (miData.Name.Equals("WM/Year"))
                        sMovieYear = miData.Value.ToString();
                    if (miData.Name.Equals("Duration"))
                    {
                        sPlayTime = pdamxUtility.FormatNanoseconds(miData.Value.ToString());
                        sUFPlayTime = miData.Value.ToString();
                        nDurationInSeconds = (int)(Convert.ToDouble(miData.Value.ToString()) / 10000000);
                    }
                    if (miData.Name.Equals("WM/WMRVHDContent"))
                    {
                        if (miData.Value.ToString().ToUpper().Trim().Equals("TRUE"))
                            sIsHDContent = "Yes";
                        else
                            sIsHDContent = "No";
                    }
                    if (FullFileName.ToUpper().Trim().Contains("(HD-TP)"))
                        sIsHDContent = "Yes";
                    if (miData.Name.Equals("WM/WMRVDTVContent"))
                    {
                        if (miData.Value.ToString().ToUpper().Trim().Equals("TRUE"))
                            sIsDTVContent = "Yes";
                        else
                            sIsDTVContent = "No";
                    }
                    if (FullFileName.ToUpper().Trim().Contains("(HD)"))
                        sIsDTVContent = "Yes";
                    if (sStationName.ToUpper().Contains(" HD") || sStationName.ToUpper().Contains("-DT"))
                        sIsDTVContent = "Yes";
                    if (miData.Name.Equals("WM/VideoClosedCaptioning"))
                        sClosedCaptioningPresent = "Yes";
                }
                if (VideoCatagory.ToUpper().Equals("SPECIALS"))
                    sGenre = FileGenre;
                if (sGenre.Trim().Length == 0)
                    sGenre = FileGenre;
            }
            catch (Exception)
            {
                sGenre = FileGenre;
                if (VideoCatagory.ToUpper().Equals("MOVIES"))
                {
                    sTitle = Name;
                }
                if (VideoCatagory.ToUpper().Equals("SERIES")
                    || VideoCatagory.ToUpper().Equals("SPECIALS"))
                {
                    if (sEpisodeTitle.Trim().Length > 0)
                        sTitle = sEpisodeTitle;
                    else
                        sEpisodeTitle = Name;
                }
            }
            if (VideoCatagory.ToUpper().Equals("SERIES")
                || VideoCatagory.ToUpper().Equals("SPECIALS"))
            {
                if (sTitle == null)
                    sTitle = sEpisodeTitle;
                if (sTitle.Trim().Length == 0)
                    sTitle = sEpisodeTitle;
                if (sTitle.Trim().Length == 0)
                    sTitle = Name;
            }
        }
        private void SetNonDVRMSProperties()
        {
            pdamxAudioProperties mxAudioProperties;

            sClosedCaptioningPresent = "No";
            if (VideoCatagory.ToUpper().Equals("SERIES")
                || VideoCatagory.ToUpper().Equals("SPECIALS"))
            {
                sSeries = GetSeries(FullFileName, FileGenre);
                sSeason = GetSeason(FullFileName);
                sEpisode = GetEpisode(FullFileName);
                sEpisodeTitle = GetEpisodeTitle(FullFileName);
                if (sSeason == null)
                    sSeason = "N/A";

                if (sEpisodeTitle == null)
                    sEpisodeTitle = Name;
                else if (sEpisodeTitle.Trim().Length == 0)
                    sEpisodeTitle = Name;

                if (sEpisode == null)
                    sEpisode = "N/A";
            }
            try
            {
                mxAudioProperties = new pdamxAudioProperties(FullFileName);

                sPlayTime = mxAudioProperties.PlayTimeFormatted;
                sUFPlayTime = mxAudioProperties.PlayTimeUnformatted;
                nDurationInSeconds = (int)Convert.ToDouble(mxAudioProperties.DurationInSeconds);

                if (VideoCatagory.ToUpper().Equals("MOVIES"))
                {
                    sTitle = Name;
                }
                if (VideoCatagory.ToUpper().Equals("SERIES")
                    || VideoCatagory.ToUpper().Equals("SPECIALS"))
                {

                    if (sEpisode != "N/A")
                        sTitle = sEpisodeTitle;
                    else
                        sTitle = Name;
               }
            }
            catch (Exception)
            {
                sPlayTime = "??:??:??";
                sUFPlayTime = "0";
                nDurationInSeconds = 0;

                if (VideoCatagory.ToUpper().Equals("MOVIES"))
                {
                    sTitle = Name;
                }
                if (VideoCatagory.ToUpper().Equals("SERIES")
                    || VideoCatagory.ToUpper().Equals("SPECIALS"))
                {
                    sEpisodeTitle = Name;
                    sTitle = sEpisodeTitle;
                }
            }
            sGenre = FileGenre;
            sIsHDContent = "No";
            sIsDTVContent = "No";
            if (VideoCatagory.ToUpper().Equals("SERIES")
                || VideoCatagory.ToUpper().Equals("SPECIALS"))
            {
                if (sTitle == null)
                    sTitle = sEpisodeTitle;
                if (sTitle.Trim().Length == 0)
                    sTitle = sEpisodeTitle;
                if (sTitle.Trim().Length == 0)
                    sTitle = Name;
            }
        }
        public String Channel
        {
            get
            {
                return ((sChannel != null ? sChannel : ""));
            }
        }
        public String Credits
        {
            get
            {
                return ((sCredits != null ? sCredits : ""));
            }
        }
        public String Description
        {
            get
            {
                return ((sDescription != null ? sDescription : ""));
            }
        }
        public int DurationInSeconds
        {
            get
            {
                return (nDurationInSeconds);
            }
        }
        public String Episode
        {
            get
            {
                return ((sEpisode != null ? sEpisode : ""));
            }
        }
        public String EpisodeTitle
        {
            get
            {
                return ((sEpisodeTitle != null ? sEpisodeTitle : ""));
            }
        }
        public String FileGenre
        {
            get
            {
                int nStartIdx = FullFileName.ToUpper().IndexOf("\\" + VideoCatagory.ToUpper()) + VideoCatagory.Length + 2;
                int nLen = FullFileName.IndexOf("\\", nStartIdx) - nStartIdx;
                return (FullFileName.Substring(nStartIdx, nLen));
            }
        }
        public String FileLocation
        {
            get
            {
                return (fiVideoFileInfo.DirectoryName);
            }
        }
        public String FileName
        {
            get
            {
                return (fiVideoFileInfo.Name);
            }
        }
        public long FileSize
        {
            get
            {
                return (fiVideoFileInfo.Length);
            }
        }
        public String FileSizeFormatted
        {
            get
            {
                return (pdamxUtility.FormatStorageSize(Convert.ToString(FileSize)));
            }
        }
        public String FileSizeUnformatted
        {
            get
            {
                return (Convert.ToString(FileSize));
            }
        }
        public String FileType
        {
            get
            {
                return (fiVideoFileInfo.Extension.Replace(".", "").ToUpper());
            }
        }
        public String FullFileName
        {
            get
            {
                return (fiVideoFileInfo.FullName);
            }
        }
        public String Genre
        {
            get
            {
                return ((sGenre.Trim().Length > 0 ? sGenre : FileGenre));
            }
        }
        public String IsDTVContent
        {
            get
            {
                return (sIsDTVContent);
            }
        }
        public String IsHDContent
        {
            get
            {
                return (sIsHDContent);
            }
        }
        public DateTime LastWriteTime
        {
            get
            {
                return (fiVideoFileInfo.LastWriteTime);
            }
        }
        public String LastWriteTimeFormatted
        {
            get
            {
                DateTimeFormatInfo dtFormat = new CultureInfo("en-US", false).DateTimeFormat;
                return (LastWriteTime.ToString("MM/dd/yyyy (hh:mm:ss tt)", dtFormat));
            }
        }
        public String LastWriteTimeUnformatted
        {
            get
            {
                return (Convert.ToString(LastWriteTime.ToFileTime()));
            }
        }
        public String MediaFormat
        {
            get
            {
                if (FullFileName.ToUpper().Contains("DIVX") && FileType.Equals("AVI"))
                    return("DIVX");
                else
                    return(FileType);
            }
        }
        public String MediaType
        {
            get
            {
                return ("Video");
            }
        }
        public String MovieYear
        {
            get
            {
                return ((sMovieYear != null ? sMovieYear : ""));
            }
        }
        public String Name
        {
            get
            {
                return (fiVideoFileInfo.Name.Replace(fiVideoFileInfo.Extension, ""));
            }
        }
        public String NetworkAffiliation
        {
            get
            {
                return ((sNetworkAffiliation != null ? sNetworkAffiliation : ""));
            }
        }
        public String ParentalRating
        {
            get
            {
                return ((sParentalRating != null ? sParentalRating : ""));
            }
        }
        public String ParentalRatingReason
        {
            get
            {
                return ((sParentalRatingReason != null ? sParentalRatingReason : ""));
            }
        }
        public String PlayTimeFormatted
        {
            get
            {
                return ((sPlayTime != null ? sPlayTime : ""));
            }
        }
        public String PlayTimeUnFormatted
        {
            get
            {
                return (sUFPlayTime);
            }
        }
        public String ProviderRating
        {
            get
            {
                return ((sProviderRating != null ? sProviderRating : ""));
            }
        }
        public String Season
        {
            get
            {
                return ((sSeason != null ? sSeason : ""));
            }
        }
        public String Series
        {
            get
            {
                return ((sSeries != null ? sSeries : ""));
            }
        }
        public String SettopDVR
        {
            get
            {
                String sSettopDVR = "";

                if (MediaFormat.Equals("DVR-MS"))
                {
                    if (StationCallSign == null)
                        sSettopDVR = "TIVO";
                    else
                        if (StationCallSign.Equals("TIVO"))
                            sSettopDVR = "TIVO";
                        else
                            if (sClosedCaptioningPresent.ToLower().Equals("no"))
                                sSettopDVR = "BTV";
                            else
                                sSettopDVR = "MCE";
                }
                else
                    sSettopDVR = MediaFormat;

                return (sSettopDVR);
            }
        }
        public String StationCallSign
        {
            get
            {
                return ((sStationCallSign != null ? sStationCallSign : ""));
            }
        }
        public String StationName
        {
            get
            {
                return ((sStationName != null ? sStationName : ""));
            }
        }
        public String Title
        {
            get
            {
                if (VideoCatagory.ToUpper().Equals("MOVIES"))
                    return (sTitle.Replace("(HD)", "").Replace("(HD-TP)", "").Replace(" - ", ": "));
                else
                    return (sTitle.Replace("(HD)", "").Replace("(HD-TP)", ""));
            }
        }
        public String VideoCatagory
        {
            get
            {
                String sVideoCatagory = "Series";
                if (fiVideoFileInfo.FullName.ToUpper().Contains("MOVIES"))
                    sVideoCatagory = "Movies";
                if (fiVideoFileInfo.FullName.ToUpper().Contains("SERIES"))
                    sVideoCatagory = "Series";
                if (fiVideoFileInfo.FullName.ToUpper().Contains("SPECIALS"))
                    sVideoCatagory = "Specials";
                if (fiVideoFileInfo.FullName.ToUpper().Contains("VHS"))
                {
                    if (fiVideoFileInfo.FullName.ToUpper().Contains("ANIMATION")
                        || fiVideoFileInfo.FullName.ToUpper().Contains("MARTIAL ARTS"))
                        sVideoCatagory = "Movies";
                    else
                        sVideoCatagory = "Specials";
                }
                return (sVideoCatagory);
            }
        }
    }
}
