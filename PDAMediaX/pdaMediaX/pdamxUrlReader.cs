using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;

namespace pdaMediaX.Web
{
	public class pdamxUrlReader
	{
        String[] sUserCredentals = null;
        String sUrl = "";
        String sWriteToFile = "";
        bool bXMLFiltering = false;
        bool bAcceptInvalidSSLCertificate = false;
        bool bUseCredentials = false;

        public pdamxUrlReader()
        {
        }
        public pdamxUrlReader(String _sUrl)
        {
            if (_sUrl == null)
                return;

            if (_sUrl.Trim().Length == 0)
                return;

            Url = _sUrl;
        }
        public String OpenUrl()
        {
            TextWriter tw = null;
            WebClient wbClient;
            Stream stData;
            StreamReader srReader;
            String sUrlData = "";

            if (Url.Length == 0)
                return (null);

            try
            {
                wbClient = new WebClient();
                wbClient.Headers.Add("user-agent", "Mozilla/4.0 (compatible; MSIE 6.0; Windows NT 5.2; .NET CLR 1.0.3705;)");
               
                // Igonore uncertified SSL certific warnings..
                if (AcceptInvalidSSLCertificate)
                    ServicePointManager.ServerCertificateValidationCallback = delegate { return true; };

                // Use user credentials...
                if (UseCredentials)
                {
                    ICredentials credentials = new NetworkCredential(sUserCredentals[0], sUserCredentals[1]);
                    wbClient.Credentials = credentials;
                }
                stData = wbClient.OpenRead(Url.Replace("&amp;", "&"));
                srReader = new StreamReader(stData);

                // Output content returned from url to file. Otherwise, return in a string...
                if (WriteToFile.Length > 0)
                {
                    tw = new StreamWriter(WriteToFile);
                    if (XMLFiltering)
                        tw.Write(srReader.ReadToEnd().Replace("&", "&amp;"));
                    else
                        tw.Write(srReader.ReadToEnd());
                    tw.Close();
                }
                else
                {
                    if (XMLFiltering)
                        sUrlData = srReader.ReadToEnd().Replace("&", "&amp;");
                    else
                        sUrlData = srReader.ReadToEnd();
                }
                srReader.Close();
            }
            catch (Exception e)
            {
                Console.WriteLine("pdamxUrlReader:OpenUrl() - " + e.Message);
                sUrlData = null;
                try
                {
                    if (tw != null)
                    {
                        tw.Close();
                    }
                }
                catch (Exception e1)
                {
                    Console.WriteLine("pdamxUrlReader:OpenUrl() - " + e1.Message);
                }
            }
            return (sUrlData);
        }
        public String OpenUrl(String _sUrl, String _sWriteToFile)
        {
            if (_sUrl == null)
                return (null);

            if (_sUrl.Trim().Length == 0)
                return (null);

            if (_sWriteToFile == null)
                return (null);

            if (_sWriteToFile.Trim().Length == 0)
                return (null);

            Url = _sUrl;
            WriteToFile = _sWriteToFile;

            return (OpenUrl());
        }
        public bool AcceptInvalidSSLCertificate
        {
            get
            {
                return (bAcceptInvalidSSLCertificate);
            }
            set
            {
                bAcceptInvalidSSLCertificate = value;
            }
        }
        public String Url
        {
            get
            {
                return (sUrl);
            }
            set
            {
                if (value != null)
                    if (value.Trim().Length > 0)
                        sUrl = value;
            }
        }
        public bool UseCredentials
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
        public String[] UserCredentals
        {
            set
            {
                sUserCredentals = value;
            }
        }
        public String WriteToFile
        {
            get
            {
                return (sWriteToFile);
            }
            set
            {
                if (value != null)
                    if (value.Trim().Length > 0)
                        sWriteToFile = value;
            }
        }
        public bool XMLFiltering
        {
            get
            {
                return (bXMLFiltering);
            }
            set
            {
                bXMLFiltering = value;
            }
        }
	}
}
