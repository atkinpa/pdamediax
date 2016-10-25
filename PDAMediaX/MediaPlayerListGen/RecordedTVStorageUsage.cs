using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using pdaMediaX.Common;
using pdaMediaX.Util.Xml;

namespace RecordedTVStorageUsage
{
    class RecordedTVStorageUsage : pdaMediaX.pdamxBatchJob
    {
        //static void Main(string[] args)
       // {
       //     new RecordedTVStorageUsage();
       // }
        public RecordedTVStorageUsage()
        {
            TextWriter twWriter;
            String sRecordedTVDrive;
            String sRecordedTVStatFile;
            String sRecordedTVFolder;
            String sRecordedTVFolderList;
            String[] sRecordedTVFileFilter;
            List<String> lFiles;
            Boolean bLimitStatsToRecordedTVDrive = true;
            int nTotalRecording = 0;
            long lTotalRecordedTVSize = 0;

            String jobInfoXMLTemplate =
                  "\n   <JobInfo>"
                + "\n      <Generated></Generated>"
                + "\n      <Generator></Generator>"
                + "\n       <Machine></Machine>"
                + "\n      <OS></OS>"
                + "\n      <OSVersion></OSVersion>"
                + "\n   </JobInfo>";

            String machineInfoXMLTemplate =
                      "\n   <MachineInfo>"
                    + "\n     <Machine></Machine>"
                    + "\n     <Platform></Platform>"
                    + "\n     <Processors></Processors>"
                    + "\n     <OSVersion></OSVersion>"
                    + "\n     <IPAddress></IPAddress>"
                    + "\n   </MachineInfo>";

            String driveXMLTemplate =
                      "\n   <Drive>"
                    + "\n     <DriveLetter></DriveLetter>"
                    + "\n     <VolumeLabel></VolumeLabel>"
                    + "\n     <DriveFormat></DriveFormat>"
                    + "\n     <TotalStorage></TotalStorage>"
                    + "\n     <TotalStorageFree></TotalStorageFree>"
                    + "\n     <TotalStorageUsed></TotalStorageUsed>"
                    + "\n     <UFTotalStorage></UFTotalStorage>"
                    + "\n     <UFTotalStorageFree></UFTotalStorageFree>"
                    + "\n     <UFTotalStorageUsed></UFTotalStorageUsed>"
                    + "\n   </Drive>";

            String recordedDirectoryXMLTemplate =
                      "\n   <RecordingDirectory>"
                    + "\n     <Directory></Directory>"
                    + "\n     <Files></Files>"
                    + "\n     <TotalStorageUsed></TotalStorageUsed>"
                    + "\n     <UFTotalStorageUsed></UFTotalStorageUsed>"
                    + "\n   </RecordingDirectory>";

            // Load XML templates into memory...
            XMLWriter.LoadXMLTemplate("jobInfoXMLTemplate", jobInfoXMLTemplate);
            XMLWriter.LoadXMLTemplate("driveXMLTemplate", driveXMLTemplate);
            XMLWriter.LoadXMLTemplate("recordedDirectoryXMLTemplate", recordedDirectoryXMLTemplate);
            XMLWriter.LoadXMLTemplate("machineInfoXMLTemplate", machineInfoXMLTemplate);

            sRecordedTVDrive = GetSettings("/RecordedTV/RecordedTVDrive");
            sRecordedTVStatFile = GetSettings("/RecordedTV/RecordedTVDriveStatFile");

            if (GetSettings("/RecordedTV/LimitStatsToRecordedTVDrive").ToLower() == "yes")
            {
                bLimitStatsToRecordedTVDrive = true;
            }

            // Create XML DB file...
            XMLWriter.Open(sRecordedTVStatFile);
            XMLWriter.RootNode = "RecordedTV";
            XMLWriter.Namespace = "http://www.pdamediax.com/recordedtv";

           // System.Environment.Version;
            //System.Net.Dns.GetHostAddresses().GetValue

            //Write XML content to console stream or file...
            XMLWriter.CopyXMLTemplate("jobInfoXMLTemplate");
            XMLWriter.SetXMLTemplateElement("jobInfoXMLTemplate", "Generated", StartTime);
            XMLWriter.SetXMLTemplateElement("jobInfoXMLTemplate", "Generator", Program);
            XMLWriter.SetXMLTemplateElement("jobInfoXMLTemplate", "Machine", Machine);
            XMLWriter.SetXMLTemplateElement("jobInfoXMLTemplate", "OS", OS);
            XMLWriter.SetXMLTemplateElement("jobInfoXMLTemplate", "OSVersion", OSVersion);
            XMLWriter.Write(XMLWriter.GetXMLTemplate("jobInfoXMLTemplate"));

            XMLWriter.CopyXMLTemplate("machineInfoXMLTemplate");
            XMLWriter.SetXMLTemplateElement("machineInfoXMLTemplate", "Machine", System.Environment.MachineName);
            XMLWriter.SetXMLTemplateElement("machineInfoXMLTemplate", "OSVersion", System.Environment.OSVersion.ToString());
            XMLWriter.SetXMLTemplateElement("machineInfoXMLTemplate", "Platform", System.Environment.GetEnvironmentVariable("PROCESSOR_ARCHITECTURE"));
            XMLWriter.SetXMLTemplateElement("machineInfoXMLTemplate", "Processors", System.Environment.GetEnvironmentVariable("NUMBER_OF_PROCESSORS"));

            IPAddress[] ipAddress = System.Net.Dns.GetHostAddresses(System.Net.Dns.GetHostName());
            String sIpAddress = "";
            foreach (IPAddress ip in ipAddress)
            {
                if (ip.IsIPv6LinkLocal || ip.IsIPv6Multicast || ip.IsIPv6SiteLocal)
                    continue;
                if (ip.ToString().StartsWith("192."))
                    continue;
                sIpAddress = ip.ToString();
                break;
            }
            XMLWriter.SetXMLTemplateElement("machineInfoXMLTemplate", "IPAddress", sIpAddress);
            XMLWriter.Write(XMLWriter.GetXMLTemplate("machineInfoXMLTemplate"));

            foreach (DriveInfo drive in DriveInfo.GetDrives())
            {
                if (drive.IsReady)
                {
                    if (bLimitStatsToRecordedTVDrive && drive.Name.ToLower() != sRecordedTVDrive.ToLower())
                    {
                        continue;
                    }
                    XMLWriter.CopyXMLTemplate("driveXMLTemplate");
                    XMLWriter.SetXMLTemplateElement("driveXMLTemplate", "DriveLetter", drive.Name);
                    XMLWriter.SetXMLTemplateElement("driveXMLTemplate", "VolumeLabel", drive.VolumeLabel);
                    XMLWriter.SetXMLTemplateElement("driveXMLTemplate", "DriveFormat", drive.DriveFormat);
                    XMLWriter.SetXMLTemplateElement("driveXMLTemplate", "TotalStorage", pdamxUtility.FormatStorageSize(Convert.ToString(drive.TotalSize)));
                    XMLWriter.SetXMLTemplateElement("driveXMLTemplate", "TotalStorageFree", pdamxUtility.FormatStorageSize(Convert.ToString(drive.TotalFreeSpace)));
                    XMLWriter.SetXMLTemplateElement("driveXMLTemplate", "TotalStorageUsed", pdamxUtility.FormatStorageSize(Convert.ToString(drive.TotalSize - drive.TotalFreeSpace)));
                    XMLWriter.SetXMLTemplateElement("driveXMLTemplate", "UFTotalStorage", Convert.ToString(drive.TotalSize));
                    XMLWriter.SetXMLTemplateElement("driveXMLTemplate", "UFTotalStorageFree", Convert.ToString(drive.TotalFreeSpace));
                    XMLWriter.SetXMLTemplateElement("driveXMLTemplate", "UFTotalStorageUsed", Convert.ToString(drive.TotalSize - drive.TotalFreeSpace));
                    XMLWriter.Write(XMLWriter.GetXMLTemplate("driveXMLTemplate"));
                }
            }
            sRecordedTVFolder = GetSettings("/RecordedTV/RecordedTVFolder");
            sRecordedTVFolderList = GetSettings("/RecordedTV/RecordedTVFolderList");
            sRecordedTVFileFilter = GetSettings("/RecordedTV/RecordedTVFileFilter").Split(';');
            lFiles = new List<String>();
            foreach (String sFilter in sRecordedTVFileFilter)
            {
                String[] sFiles = Directory.GetFiles(sRecordedTVFolder, "*." + sFilter);
                foreach (String sFile in sFiles)
                {
                    lFiles.Add(sFile);
                }

            }
            twWriter = new StreamWriter(sRecordedTVFolderList);
            twWriter.WriteLine("{\"RecordedFiles\":[");
                String sbuffer = "";

                foreach (String sFile in lFiles)
                {
                    String sIsHD = "No";
                    FileInfo fiInfo = new FileInfo(sFile);
                    if (sbuffer != "") {
                        twWriter.WriteLine(sbuffer + ",");
                    }
                    if (fiInfo.Name.ToUpper().Contains("HD_") || fiInfo.Name.ToUpper().Contains("DT_")
                        || fiInfo.Name.ToUpper().Contains("H_") || fiInfo.Name.ToUpper().Contains("HDP_"))
                    {
                        sIsHD = "Yes";
                    }
                    if (fiInfo.Extension.ToUpper().Contains("TP"))
                    {
                        sIsHD = "Yes";
                    }
                    sbuffer = "{\"FileName\":\"" + fiInfo.Name + "\","
                       + "\"Directory\":\"" + fiInfo.Directory.ToString().Replace("\\", "\\\\") + "\","
                       + "\"FileExt\":\"" + fiInfo.Extension + "\","
                       + "\"HD\":\"" + sIsHD + "\","
                       + "\"FileSize\":\"" + pdamxUtility.FormatStorageSize(Convert.ToString(fiInfo.Length)) + "\","
                       + "\"UnformattedFileSize\":\"" + Convert.ToString(fiInfo.Length) + "\","
                       + "\"CreateTm\":\"" + fiInfo.CreationTime + "\","
                       + "\"LastModifiedTm\":\"" + fiInfo.LastWriteTime + "\","
                       + "\"LastAccessTm\":\"" + fiInfo.LastAccessTime + "\"}";
                    lTotalRecordedTVSize = lTotalRecordedTVSize + fiInfo.Length;
                    nTotalRecording++;
                }
                if (sbuffer != "")
                {
                    twWriter.WriteLine(sbuffer);
                }
            twWriter.WriteLine("]}");
            twWriter.Close();
            DirectoryInfo diDirectory = new DirectoryInfo(sRecordedTVFolder);

            XMLWriter.CopyXMLTemplate("recordedDirectoryXMLTemplate");
            XMLWriter.SetXMLTemplateElement("recordedDirectoryXMLTemplate", "Directory", diDirectory.FullName);
            XMLWriter.SetXMLTemplateElement("recordedDirectoryXMLTemplate", "Files", Convert.ToString(nTotalRecording));
            XMLWriter.SetXMLTemplateElement("recordedDirectoryXMLTemplate", "TotalStorageUsed", pdamxUtility.FormatStorageSize(Convert.ToString(lTotalRecordedTVSize)));
            XMLWriter.SetXMLTemplateElement("recordedDirectoryXMLTemplate", "UFTotalStorageUsed", Convert.ToString(lTotalRecordedTVSize));
            XMLWriter.Write(XMLWriter.GetXMLTemplate("recordedDirectoryXMLTemplate"));
            XMLWriter.Close();

            WriteEndofJobSummaryToFile = true;
            AddSummaryExtra("");
            AddSummaryExtra("Recorded TV Storage Usage Processing Summary");
            AddSummaryExtra("");
            AddSummaryExtra("  Number of Recordings:                " + nTotalRecording);
            AddSummaryExtra("  Total Storage Used       :           " + pdamxUtility.FormatStorageSize(Convert.ToString(lTotalRecordedTVSize)));
            PrintEndofJobSummary();
        }
    }
}
