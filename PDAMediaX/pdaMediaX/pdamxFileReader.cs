using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace pdaMediaX.IO
{
	class pdamxFileReader
	{
        FileInfo fiFileInfo;
        FileStream fsFileReader;
        StreamReader srTextFileReader;

        String sFileName;
        bool bFileOpen = false;

        public pdamxFileReader()
        {
        }
        public pdamxFileReader(String _sFileName)
        {
            Open(_sFileName, false);
        }
        public bool Close()
        {
            if (fsFileReader != null)
            {
                fsFileReader.Close();
                fsFileReader = null;
                bFileOpen = false;
            }
            if (srTextFileReader != null)
            {
                srTextFileReader.Close();
                srTextFileReader = null;
                bFileOpen = false;
            }
            return(bFileOpen);
        }
        public static String LoadTextFile(String _sFileName)
        {
            FileInfo fiFileInfo;
            StreamReader srTextFileReader;

            if (_sFileName == null)
                return (null);

            if (_sFileName.Trim().Length == 0)
                return (null);

            try{
                fiFileInfo = new FileInfo(_sFileName);
                srTextFileReader = fiFileInfo.OpenText();
                return(srTextFileReader.ReadToEnd());
            }
            catch (FileNotFoundException) 
            {
                return (null);
            }
        }
        public bool Open (String _sFileName)
        {
            return (Open(_sFileName,false));
        }
        public bool Open(String _sFileName, bool bBinary)
        {
            if (_sFileName == null)
                return (false);

            if (_sFileName.Trim().Length == 0)
                return (false);

            try
            {
                Close();
                fiFileInfo = new FileInfo(_sFileName);
                if (bBinary)
                    fsFileReader = fiFileInfo.OpenRead();
                else
                    srTextFileReader = fiFileInfo.OpenText();
                sFileName = _sFileName;
                bFileOpen = true;
                return (true);
            }
            catch (FileNotFoundException) 
            {
                return (false);
            }
        }
        public String ReadLine()
        {
            if (srTextFileReader == null)
                return (null);

            if (srTextFileReader.EndOfStream)
                return (null);

            return (srTextFileReader.ReadLine());
        }
        public String ReadToEnd()
        {
            if (srTextFileReader == null)
                return (null);

            if (srTextFileReader.EndOfStream)
                return (null);

            return (srTextFileReader.ReadLine());
        }
        public int ReadByte()
        {
            if (fsFileReader == null)
                return (-1);

            return(fsFileReader.ReadByte());
        }
        public FileInfo FileInfo
        {
            get
            {
                return (fiFileInfo);
            }
        }
        public String FileName
        {
            get
            {
                return (sFileName);
            }
        }
        public bool isFileOpen
        {
            get
            {
                return (bFileOpen);
            }
        }
	}
}
