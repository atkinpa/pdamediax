using System;
using System.IO;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using TagLib;
using pdaMediaX.Common;

namespace pdaMediaX.Media
{
    public class pdamxAudioProperties
    {
        TagLib.File tlFMp3File;
        FileInfo fiAudioFileInfo;

        public pdamxAudioProperties(String _sAudioFile)
        {
            if (_sAudioFile == null)
                return;

            if (_sAudioFile.Trim().Length == 0)
                return;

            fiAudioFileInfo = new FileInfo(_sAudioFile);
            tlFMp3File = TagLib.File.Create(fiAudioFileInfo.FullName);
        }
        public pdamxAudioProperties(FileInfo _fiAudioFileInfo)
        {
            if (_fiAudioFileInfo == null)
                return;

            fiAudioFileInfo = _fiAudioFileInfo;
            tlFMp3File = TagLib.File.Create(_fiAudioFileInfo.FullName);
        }
        public String AudioBitRate
        {
            get
            {
                return (tlFMp3File.Properties.AudioBitrate.ToString());
            }
        }
        public String AudioChannels
        {
            get
            {
                return (tlFMp3File.Properties.AudioChannels.ToString());
            }
        }
        public String AudioSampleBitRate
        {
            get
            {
                return (tlFMp3File.Properties.AudioSampleRate.ToString());
            }
        }
        public String AlbumTitle
        {
            get
            {
                String sAlbum = null;
                String[] sFileName;

                sAlbum = tlFMp3File.Tag.Album;
                if (sAlbum == null)
                {
                    sFileName = FileName.Split(new String[] { " - " }, StringSplitOptions.None);
                    if (sFileName != null)
                        if (sFileName.Length < 3)
                            return ("");
                    sAlbum = sFileName[1];
                }
                return (sAlbum.Trim());
            }
        }
        public String AlbumYear
        {
            get
            {
                return (tlFMp3File.Tag.Year.ToString());
            }
        }
        public String Artist
        {
            get
            {
                String[] sPerformers = tlFMp3File.Tag.Performers;
                String[] sFileName;
                String sArtist = null;

                if (sPerformers != null)
                {
                    for (int i = 0; i < sPerformers.Length; i++)
                    {
                        if (sPerformers[i].Trim().Length > 0)
                        {
                            sArtist = sPerformers[i].Trim();
                            break;
                        }
                    }
                }
                if (sArtist == null)
                    sArtist = tlFMp3File.Tag.FirstAlbumArtist;

                if (sArtist == null)
                {
                    sFileName = Name.Split(new String[] { " - " }, StringSplitOptions.None);
                    if (sFileName != null)
                        if (sFileName.Length < 2)
                            return ("");
                    sArtist = sFileName[0];
                }
                return (sArtist.Trim());
            }
        }
        private TimeSpan GetDuration()
        {
            TimeSpan tsDuration = tlFMp3File.Properties.Duration;
            return (tsDuration);
        }
        public TimeSpan Duration
        {
            get
            {
                return (GetDuration());
            }
        }
        public long DurationInSeconds
        {
            get
            {
                return (Convert.ToInt64(GetDuration().TotalSeconds));
            }
        }
        public String Description
        {
            get
            {
                String sDescription = "";

                if (tlFMp3File.Properties.Description != null)
                    sDescription = tlFMp3File.Properties.Description;
                return (sDescription);
            }
        }
        public String FileName
        {
            get
            {
                return (fiAudioFileInfo.Name);
            }
        }
        public String FileLocation
        {
            get
            {
                return (fiAudioFileInfo.DirectoryName);
            }
        }
        public String FileType
        {
            get
            {
                return (fiAudioFileInfo.Extension.Replace(".", "").ToUpper());
            }
        }
        public String FullFileName
        {
            get
            {
                return (fiAudioFileInfo.FullName);
            }
        }
        public String Genre
        {
            get
            {
                String sGenre = "";

                if (tlFMp3File.Tag.FirstGenre != null)
                    sGenre = tlFMp3File.Tag.FirstGenre;

                return (sGenre.Trim());
            }
        }
        public DateTime LastWriteTime
        {
            get
            {
                return(fiAudioFileInfo.LastWriteTime);
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
                return (FileType);
            }
        }
        public String MediaType
        {
            get
            {
                MediaTypes mtMediaType = tlFMp3File.Properties.MediaTypes;
                return (mtMediaType.ToString());
            }
        }
        public String Name
        {
            get
            {
                return (fiAudioFileInfo.Name.Replace(fiAudioFileInfo.Extension, ""));
            }
        }
        public String PlayTimeFormatted
        {
            get
            {
                return (pdamxUtility.FormatSeconds(GetDuration().Duration().TotalSeconds.ToString()));
            }
        }
        public String PlayTimeUnformatted
        {
            get
            {
                return (GetDuration().Duration().TotalSeconds.ToString());
            }
        }
        public long FileSize
        {
            get
            {
                return(fiAudioFileInfo.Length);
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
        public String Title
        {
            get
            {
                String sTitle;
                String[] sFileName;

                if (tlFMp3File.Tag.Title != null)
                {
                    sFileName = tlFMp3File.Tag.Title.Split(new String[] { " - " }, StringSplitOptions.None);
                    if (sFileName.Length < 1)
                        sTitle = tlFMp3File.Tag.Title;
                    else
                        sTitle = sFileName[sFileName.Length - 1];
                }
                else
                {
                    sFileName = Name.Split(new String[] { " - " }, StringSplitOptions.None);
                    if (sFileName.Length < 1)
                        sTitle = FileName;
                    else
                        sTitle = sFileName[sFileName.Length - 1];
                }
                return (sTitle.Trim());
            }
        }
        public String Track
        {
            get
            {
                String sTrack;
                String[] sFileName;

                sTrack = tlFMp3File.Tag.Track.ToString();
                if (tlFMp3File.Tag.Track == 0)
                {
                    sTrack = "";
                    sFileName = FileName.Split(new String[] { " " }, StringSplitOptions.None);
                    for (int i = 0; i < sFileName.Length; i++)
                    {
                        if (pdamxUtility.IsNumeric(sFileName[i]))
                        {
                            sTrack = sFileName[i];
                            break;
                        }
                    }
                }
                return (sTrack.Trim());
            }
        }
    }
}
