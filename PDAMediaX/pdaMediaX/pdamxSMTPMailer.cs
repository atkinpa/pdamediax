using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using System.Net.Mail;
using System.Net.Mime;
using pdaMediaX.Util;
using pdaMediaX.Util.Xml;

// Example send email code : http://www.c-sharpcorner.com/UploadFile/scottlysle/EmailAttachmentsCS08052008234321PM/EmailAttachmentsCS.aspx

namespace pdaMediaX.Net
{
    public class pdamxSMTPMailer
    {
        pdamxXMLReader mxXMLConfigReader = null;

        MailMessage mmMessage = null;
        SmtpClient scSmtpClient = null;

        String sMailProfileFileName = "SMTPSettings-Config.xml";
        String sMailHost = "pdamediax.com";
        String sPickupDirectoryLocation = "";
        String sSMTPLogFile = "SMTPSettings-Log.txt";

        int nMailHostPort = 25;

        ArrayList alTo;
        ArrayList alCC;
        ArrayList alBcc;

        //String sCredentialsFile;
        String sFrom;
        String sReplyTo;
        String sSubject;
        String sBodyText;
        String sBodyHtml;
        String sCredentials;

        bool bSettingsProfileLoaded = false;
        bool bUseMailQue = false;
        bool bUseCredentials = false;

        // Add some email templates to use...

        public pdamxSMTPMailer()
        {
            mxXMLConfigReader = new pdamxXMLReader();

            if (isSettingsLoaded()) // Look for file in current directory...
                LoadMailSettings(".");

            if (isSettingsLoaded()) // Look for file in parent directory...
                LoadMailSettings("../");

            if (isSettingsLoaded()) // Look for file in parent's, parent directory...
                LoadMailSettings("../../");

            if (isSettingsLoaded()) // Look for file in path of executable...
                LoadMailSettings("../../");

            scSmtpClient = new SmtpClient(); 
            scSmtpClient.Host = Host;
            scSmtpClient.Port = Port;
            scSmtpClient.PickupDirectoryLocation = MailQueDirectory;
            if (UsebUseCredentials)
            {
                String [] sAccessInfo;
                pdamxCrypter mxCrypter = new pdamxCrypter();
                //String sCredentials = mxCrypter.DecryptFile(sCredentialsFile);
                sAccessInfo = sCredentials.Split('/');
                //scSmtpClient.Credentials
            }
        }
        public bool isSettingsLoaded()
        {
            return (bSettingsProfileLoaded);
        }
        public bool FlushMailQue()
        {
            return(true);
        }
        private void LoadMailSettings(String sDirectory)
        {
            try
            {
                mxXMLConfigReader.Open(sDirectory + sMailProfileFileName);

                if (mxXMLConfigReader.isOpen())
                {
                    Host = mxXMLConfigReader.GetNodeValue("/SMTP/MailServer/Host");
                    Port = Convert.ToInt32(mxXMLConfigReader.GetNodeValue("/SMTP/MailServer/Port"));
                    MailQueDirectory = mxXMLConfigReader.GetNodeValue("/SMTP/MailServer/PickupDirectoryLocation");
                    SMTPLog = mxXMLConfigReader.GetNodeValue("/SMTP/MailServer/SMTPLog");
                    //SMTPLog = mxXMLConfigReader.GetNodeValue("/SMTP/MailServer/Crendentails"); //edf file.
                    bSettingsProfileLoaded = true;
                }
            }
            catch (Exception) {}
        }
        public void ClearMailMessage()
        {
        }
        public void NewMailMessage()
        {
            mmMessage = new MailMessage();
            alTo = new ArrayList();
            alCC = new ArrayList();
            alBcc = new ArrayList();
            sBodyHtml = "";
            sBodyText = "";
            sSubject = "";
        }
        public void NewMailMessage(bool bFlushQue)
        {
            if (bFlushQue)
                FlushMailQue();
            NewMailMessage();
        }
        public void ResetMailMessage()
        {
        }
        public bool SendMail()
        {
            return (true);
        }
        public bool SendMailXMLMessageProfileFile(String sXMLMessageProfileFile)
        {
            return (true);
        }
        public String Attachment
        {
            set
            {
                if (value != null)
                    if (value.Trim().Length > 0)
                    {
                        Attachment aAttachment = new Attachment(value, MediaTypeNames.Application.Octet);
                        ContentDisposition disposition = aAttachment.ContentDisposition;
                        disposition.CreationDate = System.IO.File.GetCreationTime(value);
                        disposition.ModificationDate = System.IO.File.GetLastWriteTime(value);
                        disposition.ReadDate = System.IO.File.GetLastAccessTime(value);
                        if (mmMessage == null)
                            NewMailMessage();
                        mmMessage.Attachments.Add(aAttachment);
                    }
            }
        }
        public String Bcc
        {
            get
            {
                String sBccList = "";
                foreach (String sBcc in alBcc)
                {
                    if (sBccList.Trim().Length > 0)
                        sBccList = sBccList + ";";
                    sBccList = sBccList + sBcc;
                }
                return (sBccList);
            }
            set
            {
                if (value != null)
                    if (value.Length > 0)
                        alBcc.Add(value);
            }
        }
        public String BodyHtml
        {
            get
            {
                return (sBodyHtml);
            }
            set
            {
                if (value != null)
                    if (value.Trim().Length > 0)
                        sBodyHtml = value;
            }
        }
        public String BodyText
        {
            get
            {
                return (sBodyText);
            }
            set
            {
                if (value != null)
                    if (value.Trim().Length > 0)
                        sBodyText = value;
            }
        }
        public String CC
        {
            get
            {
                String sCCList = "";
                foreach (String sCC in alCC)
                {
                    if (sCCList.Length > 0)
                        sCCList = sCCList + ";";
                    sCCList = sCCList + sCC;
                }
                return (sCCList);
            }
            set
            {
                if (value != null)
                    if (value.Trim().Length > 0)
                        alCC.Add(value);
            }
        }
        public String Credentials
        {
            get
            {
                return (sCredentials);
            }
            set
            {
                if (value != null)
                    if (value.Trim().Length > 0)
                        sCredentials = value;
            }
        }
        public String From
        {
            get
            {
                return (sFrom);
            }
            set
            {
                if (value != null)
                    if (value.Trim().Length > 0)
                        sFrom = value;
            }
        }
        public String Host
        {
            get
            {
                return (sMailHost);
            }
            set
            {
                if (value != null)
                    if (value.Trim().Length != 0)
                        sMailHost = value;
            }
        }
        public int Port
        {
            get
            {
                return (nMailHostPort);
            }
            set
            {
                nMailHostPort = value;
            }
        }
        public String MailQueDirectory
        {
            get
            {
                return (sPickupDirectoryLocation);
            }
            set
            {
                if (value != null)
                    if (value.Trim().Length != 0)
                        sPickupDirectoryLocation = value;
            }
        }
        public String ReplyTo
        {
            get
            {
                return (sReplyTo);
            }
            set
            {
                if (value != null)
                    if (value.Trim().Length > 0)
                        sReplyTo = value;

            }
        }
        public String SMTPLog
        {
            get
            {
                return (sSMTPLogFile);
            }
            set
            {
                if (value != null)
                    if (value.Trim().Length != 0)
                        sSMTPLogFile = value;
            }
        }
        public String Subject
        {
            get
            {
                return (sSubject);
            }
            set
            {
                if (value != null)
                    sSubject = value;
            }
        }
        public String To
        {
            get
            {
                String sToList = "";
                foreach (String sTo in alTo)
                {
                    if (sToList.Length > 0)
                        sToList = sToList + ";";
                    sToList = sToList + sTo;
                }
                return (sToList);
            }
            set
            {
                if (value != null)
                    if (value.Length > 0)
                        alTo.Add(value);
            }
        }
        public bool UsebUseCredentials
        {
            get
            {
                return (bUseCredentials);
            }
            set
            {
                bUseCredentials = value;
            }
        }
        public bool UseMailQue
        {
            get
            {
                return (bUseMailQue);
            }
            set
            {
                bUseMailQue = true;
            }
        }
    }
}
