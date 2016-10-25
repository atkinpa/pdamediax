using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using pdaMediaX.Util;

namespace pdaMediaX.Web
{
	public class pdamxBeyondTV
	{
        BTVLicenseManager.BTVLicenseManager btvLicenseManager;
        String sAuthTicket = null;
        String sNetworkLicense = "";
        String sUser = "";
        String sPassword = "";
        bool bLoggedOn = false;

        public pdamxBeyondTV(String _sLicenseFile, String _sAccessFile)
        {
            pdamxCrypter mxCyrpter;
            String[] sAccess;

            if (_sLicenseFile == null)
                return;

            if (_sLicenseFile.Trim().Length == 0)
                return;

            if (_sAccessFile == null)
                return;

            if (_sAccessFile.Trim().Length == 0)
                return;

            btvLicenseManager = new BTVLicenseManager.BTVLicenseManager();
            mxCyrpter = new pdamxCrypter();
            if (_sLicenseFile.ToLower().Contains(".edf"))
                sNetworkLicense = mxCyrpter.DecryptFile(_sLicenseFile);
            else
                sNetworkLicense = _sLicenseFile;

            if (_sAccessFile.ToLower().Contains(".edf"))
                sAccess = mxCyrpter.DecryptFile(_sAccessFile).Split('/');
            else
                sAccess = _sAccessFile.Split('/');
            sUser = sAccess[0];
            sPassword = sAccess[1];
        }
        public Hashtable GetProgramByEPGID(String _sEPGID)
        {
            BTVGuideData.BTVGuideData btvGuideData;
            Hashtable hSearchResult = new Hashtable();

            if (_sEPGID == null)
                return (null);

            if (_sEPGID.Trim().Length == 0)
                return (null);

            if (LogonBTVServer())
            {
                btvGuideData = new BTVGuideData.BTVGuideData();
                BTVGuideData.PVSPropertyBag pvsSearchResult = btvGuideData.GetFirstEpisodeBySeriesID(sAuthTicket, _sEPGID);

                foreach (BTVGuideData.PVSProperty pvspProp in pvsSearchResult.Properties)
                {
                    hSearchResult.Add(pvspProp.Name, pvspProp.Value);
                }
                LogoffBTVServer();
            }
            return (hSearchResult);
        }
        public Boolean ScheduleRecording()
        {
            BTVGuideData.PVSPropertyBag pvspbScheduleInfo = new BTVGuideData.PVSPropertyBag();
            bool bScheduled = false;

            return (bScheduled);
        }
        public Hashtable SearchGuide(String _sSearchValue)
        {
           BTVGuideData. BTVGuideData btvGuideData;
            Hashtable hSearchResult = new Hashtable();

            if (_sSearchValue == null)
                return (null);

            if (_sSearchValue.Trim().Length == 0)
                return (null);

            if (LogonBTVServer())
            {
                btvGuideData = new BTVGuideData.BTVGuideData();
                BTVGuideData.PVSPropertyBag[] pvsSearchResult = btvGuideData.GetEpisodesByKeywordWithLimit(sAuthTicket, _sSearchValue, 1);
                foreach (BTVGuideData.PVSPropertyBag pvspbPropBag in pvsSearchResult)
                {
                    foreach (BTVGuideData.PVSProperty pvspProp in pvspbPropBag.Properties)
                    {
                        hSearchResult.Add(pvspProp.Name, pvspProp.Value);
                    }
                }
                LogoffBTVServer();
            }
            return (hSearchResult);
        }
        public Hashtable SearchGuideAll(String _sSearchValue)
        {
            BTVGuideData.BTVGuideData btvGuideData;

            Hashtable hSearchResult = new Hashtable();
            Hashtable hWildCards = new Hashtable();
            Hashtable hSearchMatch;
            int nCnt = 0;
            int nWildCardCnt = 0;
            String sSearchKey = "";
            String sEpisode = "";

            if (_sSearchValue == null)
                return (null);

            if (_sSearchValue.Trim().Length == 0)
                return (null);

            if (LogonBTVServer())
            {
                btvGuideData = new BTVGuideData.BTVGuideData();
                sSearchKey = _sSearchValue;
                if (_sSearchValue.Contains("::"))
                {
                    int nStartIdx = _sSearchValue.IndexOf("::");
                    sSearchKey = _sSearchValue.Substring(0, nStartIdx);
                    sEpisode = _sSearchValue.Substring(nStartIdx + 2, _sSearchValue.Length - (nStartIdx + 2));
                }
                BTVGuideData.PVSPropertyBag[] pvsSearchResult = btvGuideData.GetEpisodesByKeyword(sAuthTicket, sSearchKey.Replace(":", " "));
                foreach (BTVGuideData.PVSPropertyBag pvspbPropBag in pvsSearchResult)
                {
                    hSearchMatch = new Hashtable();
                    foreach (BTVGuideData.PVSProperty pvspProp in pvspbPropBag.Properties)
                    {
                        hSearchMatch.Add(pvspProp.Name, pvspProp.Value);
                    }
                    if (hSearchMatch["SeriesTitle"].ToString().Trim().ToLower().Equals(_sSearchValue.Trim().ToLower())
                        || hSearchMatch["DisplayTitle"].ToString().Trim().ToLower().Equals(_sSearchValue.Trim().ToLower()))
                    {
                        if (sEpisode != "")
                        {
                            if (hSearchMatch["EpisodeTitle"].ToString().Contains(sEpisode))
                                hSearchResult.Add(Convert.ToString(++nCnt), hSearchMatch);
                        }
                        else
                            hSearchResult.Add(Convert.ToString(++nCnt), hSearchMatch);
                    }
                    else
                    {
                        hWildCards.Add(Convert.ToString(++nWildCardCnt), hSearchMatch);
                    }
                }
                for (int i = 1; i <= nWildCardCnt; i++)
                    hSearchResult.Add(Convert.ToString(++nCnt), hWildCards[Convert.ToString(i)]);
                LogoffBTVServer();
            }
            return (hSearchResult);
        }
        private bool LogoffBTVServer()
        {
            if (sAuthTicket == null)
                return (false);

            if (!bLoggedOn)
                return(bLoggedOn);

            try
            {
                btvLicenseManager.Logoff(sAuthTicket);;
            }
            catch (Exception) 
            {
                return (false);
            }
            sAuthTicket = null;
            bLoggedOn = false;
            return (true);
        }
        private bool LogonBTVServer()
        {
            if (bLoggedOn)
                return(bLoggedOn);

            sAuthTicket = null;
            try
            {
                BTVLicenseManager.PVSPropertyBag licenseInfo = btvLicenseManager.LogonRemote(sNetworkLicense, sUser, sPassword);
                foreach (BTVLicenseManager.PVSProperty prop in licenseInfo.Properties)
                {
                    if (prop.Name == "AuthTicket")
                    {
                        sAuthTicket = prop.Value;
                        bLoggedOn = true;
                    }
                }
            }
            catch (Exception)
            {
                LogoffBTVServer();
                return(false);
            }
            return (true);
        }
	}
}
