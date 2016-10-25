using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using pdaMediaX.Common;
using pdaMediaX.Net;
using pdaMediaX.IO.Net;
using pdaMediaX.Util;
using pdaMediaX.Util.Xml;

namespace pdaMediaX
{
    public class pdamxBatchJob
    {
        pdamxXMLReader mxXMLReader = null;
        pdamxXMLWriter mxXMLWriter = null;
        pdamxXMLReader mxXMLConfigReader = null;
        pdamxCounters mxCounters = null;
        pdamxCrypter mxCrypter = null;
        pdamxSMTPMailer mxSMTPMailer = null;
        pdamxNetStorage mxNetStorage = null;

        String sConfigFile;
        String sReportTitle = "";
        String sDomain = "PDAMEDIAX.COM";

        DateTimeFormatInfo dtFormat = new CultureInfo("en-US", false).DateTimeFormat;
        DateTime dtStartTime;
        Hashtable hSummaryExtra = null;

        int nSummaryExtraSeqID = 0;
        bool bWriteEndofJobSummaryToFile = false;
        bool bEmailEndofJobSummary = false;

        public pdamxBatchJob()
        {
            dtStartTime = DateTime.Now;
            if (Program.Contains(".exe"))
                ConfigFile = Program.Replace(".exe", "-Confg.xml");
            else
                ConfigFile = Program + "-Confg.xml";

            sReportTitle = Program;
        }
        public bool AddSummaryExtra(String sSummaryExtra)
        {
            if (sSummaryExtra == null)
                return (false);

            if (hSummaryExtra == null)
                hSummaryExtra = new Hashtable();

            hSummaryExtra.Add(Convert.ToString(nSummaryExtraSeqID++), "  " + sSummaryExtra);
            return (true);
        }
        public bool ConnectNetShare(String _sNetworkShare, String _sUser, String _sPassword)
        {
            if (_sNetworkShare == null)
                return (false);

            if (_sNetworkShare.Trim().Length == 0)
                return (false);

            if (_sUser == null)
                return (false);

            if (_sPassword == null)
                return (false);

            if (mxNetStorage == null)
                mxNetStorage = new pdamxNetStorage();

            /*
             * Sampel Code:
             * 
             * Mapping a network connection (Use Connection)
             * netDrive.LocalDrive = null;
             * netDrive.ShareName = "\\ComputerName\Share1"
             * netDrive.MapDrive();
             */
            DisconnectNetkShare(_sNetworkShare);
            mxNetStorage.LocalDrive = null;
            mxNetStorage.ShareName = _sNetworkShare;
            if (_sUser.Trim().Length == 0 && _sPassword.Trim().Length == 0)
                mxNetStorage.MapDrive();
            else
                mxNetStorage.MapDrive(_sUser, _sPassword);
            return (true);
        }
        public bool DisconnectNetkShare(String _sNetworkShare)
        {
            if (_sNetworkShare == null)
                return (false);

            if (_sNetworkShare.Trim().Length == 0)
                return (false);

            if (mxNetStorage == null)
                mxNetStorage = new pdamxNetStorage();

            /*
             * Sample Code:
             * 
             * Unmapping a network connection
             * netDrive.LocalDrive = null;
             * netDrive.ShareName = "\\ComputerName\Share1"
             * netDrive.UnMapDrive();
             */
            try
            {
                mxNetStorage.LocalDrive = null;
                mxNetStorage.ShareName = _sNetworkShare;
                mxNetStorage.UnMapDrive();
            }
            catch (Exception)
            {
                return (false);
            }
            return (true);
        }
        public String GetSettings(String sParam)
        {
            if (sParam == null)
                return (sParam);

            if (sParam.Trim().Length == 0)
                return (sParam);

            return (mxXMLConfigReader.GetNodeValue(sParam));
        }
        public bool MapNetDrive(String _sNetworkShare, String _sDrive, String _sUser, String _sPassword)
        {
            if (_sNetworkShare == null)
                return (false);

            if (_sNetworkShare.Trim().Length == 0)
                return (false);

            if (_sDrive == null)
                return (false);

            if (_sDrive.Trim().Length == 0)
                return (false);

            if (_sUser == null)
                return (false);

            if (_sPassword == null)
                return (false);

            if (mxNetStorage == null)
                mxNetStorage = new pdamxNetStorage();
            /*
             * Sample Code: 
             * 
             * Map drive with current user credentials
             * netDrive.LocalDrive = "m:";
             * netDrive.ShareName = "\\ComputerName\Share1";
             * netDrive.MapDrive();
             * 
             * Map drive with and prompt user for credentials
             * netDrive.LocalDrive = "m:";
             * netDrive.ShareName = "\\ComputerName\Share1";
             * netDrive.MapDrive("Bob_Username","Bob_Password");
             * 
             * Map drive using a persistent connection
             * netDrive.LocalDrive = "m:";
             * netDrive.Persistent = true;
             * netDrive.SaveCredentials = true;
             * netDrive.ShareName = "\\ComputerName\Share1";
             * netDrive.MapDrive("Bob_Username","Bob_Password");
            */
            try
            {
                mxNetStorage.LocalDrive = _sDrive;
                mxNetStorage.ShareName = _sNetworkShare;
                if (_sUser.Trim().Length == 0 && _sPassword.Trim().Length == 0)
                    mxNetStorage.MapDrive();
                else
                    mxNetStorage.MapDrive(_sUser, _sPassword);
            }
            catch (Exception)
            {
                return (false);
            }
            return (true);
        }
        public void PrintEndofJobSummary()
        {
            String sLines = "";
            String sTitle = "*** BATCH CYCLE END OF JOB SUMMARY FOR DOMAIN " + Domain + " ***";

            if (EmailEndofJobSummary)
                SendEndofJobSummaryEmail();

            for (int i = 0; i < sTitle.Length; i++)
                sLines = sLines + "-";

            if (WriteEndofJobSummaryToFile)
            {
                TextWriter twTextWriter = new StreamWriter(Program.Replace(".exe", "") + ".txt");
                twTextWriter.WriteLine(sTitle);
                twTextWriter.WriteLine(sLines);
                twTextWriter.WriteLine();
                twTextWriter.WriteLine("END OF JOB SUMMARY");
                twTextWriter.WriteLine();
                twTextWriter.WriteLine("  Job (Program):  " + Program);
                twTextWriter.WriteLine("  Start Time:     " + StartTime);
                twTextWriter.WriteLine("  End Time:       " + EndTime);
                twTextWriter.WriteLine("  Runtime:        " + ElapseTime);

                if (hSummaryExtra != null)
                {
                    for (int i = 0; i < hSummaryExtra.Count; i++)
                        twTextWriter.WriteLine((String)hSummaryExtra[Convert.ToString(i)]);
                }
                twTextWriter.Close();
            }
            else
            {
                Console.WriteLine(Domain);
                Console.WriteLine(sLines);
                Console.WriteLine();
                Console.WriteLine("END OF JOB SUMMARY");
                Console.WriteLine();
                Console.WriteLine("  Job (Program):  " + Program);
                Console.WriteLine("  Start Time:     " + StartTime);
                Console.WriteLine("  End Time:       " + EndTime);
                Console.WriteLine("  Runtime:        " + ElapseTime);

                if (hSummaryExtra != null)
                {
                    for (int i = 0; i < hSummaryExtra.Count; i++)
                        Console.WriteLine((String)hSummaryExtra[Convert.ToString(i)]);
                }
            }
        }
        public void SendEndofJobSummaryEmail()
        {
        }
        public bool UnMapNetDrive(String _sDrive)
        {
            if (_sDrive == null)
                return (false);

            if (_sDrive.Trim().Length == 0)
                return (false);

            if (mxNetStorage == null)
                mxNetStorage = new pdamxNetStorage();

            /*
             * Sample Code:
             * 
             * Unmap a network connection
             * netDrive.LocalDrive = "m:";
             * netDrive.UnMapDrive();
            */
            try
            {
                mxNetStorage.LocalDrive = _sDrive;
                mxNetStorage.UnMapDrive();
            }
            catch (Exception)
            {
                return (false);
            }
            return (true);
        }
        public pdamxCounters Counters
        {
            get
            {
                if (mxCounters == null)
                    mxCounters = new pdamxCounters();
                return (mxCounters);
            }
        }
        public String ConfigFile
        {
            get
            {
                return (sConfigFile);
            }
            set
            {
                if (value != null)
                {
                    sConfigFile = value;
                    mxXMLConfigReader = new pdamxXMLReader(sConfigFile);
                    if (!mxXMLConfigReader.isOpen())
                        mxXMLConfigReader = null;
                }
            }
        }
        public pdamxCrypter Crypter
        {
            get
            {
                if (mxCrypter == null)
                    mxCrypter = new pdamxCrypter();
                return (mxCrypter);
            }
        }
        public String Domain
        {
            get {
                return (sDomain);
            }
            set
            {
                if (value != null)
                    sDomain = value;
            }
        }
        public bool EmailEndofJobSummary
        {
            get 
            { 
                return (bEmailEndofJobSummary); 
            }
            set 
            { 
                bEmailEndofJobSummary = value; 
            }
        }
        public String EndTime
        {
            get
            {
                return (DateTime.Now.ToString("MM/dd/yyyy [hh:mm:ss tt]", dtFormat));
            }
        }
        public String ElapseTime
        {
            get
            {
                TimeSpan tsTimeSpan = DateTime.Now - dtStartTime;
                return (pdamxUtility.FormatSeconds(Convert.ToString(tsTimeSpan.TotalSeconds)));
            }
        }
        public String Machine
        {
            get
            {
                return (System.Environment.MachineName);
            }
        }
        public String Program
        {
            get
            {
                String sProgram = System.Environment.CommandLine;

                if (sProgram.Contains("\\"))
                    sProgram = sProgram.Substring(sProgram.LastIndexOf("\\") + 1, sProgram.Length - (sProgram.LastIndexOf("\\") + 3));
                return (sProgram);
            }
        }
        public String OS
        {
            get
            {
                return (System.Environment.OSVersion.Platform.ToString());
            }
        }
        public String OSVersion
        {
            get
            {
                return (System.Environment.OSVersion.VersionString);
            }
        }
        public String ReportTitle
        {
            get
            {
                return (sReportTitle);
            }
            set
            {
                if (value != null)
                    sReportTitle = value;
            }
        }
        public pdamxXMLReader SettingsObject
        {
            get 
            { 
                return (mxXMLConfigReader); 
            }
        }
        public pdamxSMTPMailer SMTPMailer
        {
            get
            {
                if (SMTPMailer == null)
                    mxSMTPMailer = new pdamxSMTPMailer();
                return (mxSMTPMailer);
            }
        }
        public String StartTime
        {
            get
            {
                return (dtStartTime.ToString("MM/dd/yyyy [hh:mm:ss tt]", dtFormat));
            }
        }
        public bool WriteEndofJobSummaryToFile
        {
            get 
            { 
                return (bWriteEndofJobSummaryToFile); 
            }
            set 
            { 
                bWriteEndofJobSummaryToFile = value; 
            }
        }
        public pdamxXMLReader XMLReader
        {
            get
            {
                if (mxXMLReader == null)
                    mxXMLReader = new pdamxXMLReader();
                return (mxXMLReader);
            }
        }
        public pdamxXMLWriter XMLWriter
        {
            get
            {
                if (mxXMLWriter == null)
                    mxXMLWriter = new pdamxXMLWriter();
                return(mxXMLWriter);
            }
        }
    }
}
