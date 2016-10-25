using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.XPath;
using pdaMediaX.Util;
using pdaMediaX.Util.Xml;
namespace pdaMediaX.Web
{
	public class pdamxSchedule
	{
        public static int CATAGORY_ALL = 100;
        public static int CATAGORY_BEYONDTV = 200;
        public static int CATAGORY_MCE = 300;

        String sMCESchedule = "";
        String sBeyondTVchedule = "";

        public pdamxSchedule()
        {
        }
        public Hashtable GetActiveRecordings(int _nCatagory)
        {
            pdamxXMLReader mxXMLReader = null;
            Hashtable hResultSet;
            Hashtable hRecord;
            XPathNodeIterator xpathINode = null;
            int nCnt = 0;

            if (_nCatagory != CATAGORY_BEYONDTV && _nCatagory != CATAGORY_MCE && _nCatagory != CATAGORY_ALL)
                return (null);

            if (_nCatagory == CATAGORY_BEYONDTV && sBeyondTVchedule == "")
                return (null);

            if (_nCatagory == CATAGORY_MCE && sMCESchedule == "")
                return (null);

            if (_nCatagory == CATAGORY_ALL && (_nCatagory == CATAGORY_MCE && sMCESchedule == ""))
                return (null);

            mxXMLReader = new pdamxXMLReader();
            hResultSet = new Hashtable();
            if (_nCatagory == CATAGORY_BEYONDTV || _nCatagory == CATAGORY_ALL)
            {
                mxXMLReader.Open(sBeyondTVchedule);
                mxXMLReader.AddNamespace("sch", "http://www.pdamediax.com/btv");
                xpathINode = mxXMLReader.GetNodePath("/sch:BeyondTV/sch:ActiveRecordings/*");

                while (xpathINode.MoveNext())
                {
                    if (xpathINode.Current.Name.Equals("Show"))
                    {
                        hRecord = new Hashtable();
                        xpathINode.Current.MoveToFirstChild();
                        do
                        {
                            hRecord.Add(xpathINode.Current.Name, xpathINode.Current.Value);
                        }
                        while (xpathINode.Current.MoveToNext());
                        hRecord.Add("Catagory", "BeyondTV");
                        hResultSet.Add(Convert.ToString(++nCnt), hRecord);
                        xpathINode.Current.MoveToParent();
                    }
                }
            }
            if (_nCatagory == CATAGORY_MCE || _nCatagory == CATAGORY_ALL)
            {
                mxXMLReader.Open(sMCESchedule);
                mxXMLReader.AddNamespace("sch", "http://www.pdamediax.com/mce");
                xpathINode = mxXMLReader.GetNodePath("/sch:MCETV/sch:ActiveRecordings/*");

                while (xpathINode.MoveNext())
                {
                    if (xpathINode.Current.Name.Equals("Show"))
                    {
                        hRecord = new Hashtable();
                        xpathINode.Current.MoveToFirstChild();
                        do
                        {
                            hRecord.Add(xpathINode.Current.Name, xpathINode.Current.Value);
                        }
                        while (xpathINode.Current.MoveToNext());
                        hRecord.Add("Catagory", "MCE");
                        hResultSet.Add(Convert.ToString(++nCnt), hRecord);
                        xpathINode.Current.MoveToParent();
                    }
                }
            }
            return (hResultSet);
        }
        public Hashtable GetLastScheduledRecording(int _nCatagory)
        {
            pdamxXMLReader mxXMLReader = null;
            Hashtable hResultSet;
            Hashtable hRecord;
            XPathNodeIterator xpathINode = null;
            int nCnt = 0;

            if (_nCatagory != CATAGORY_BEYONDTV && _nCatagory != CATAGORY_MCE && _nCatagory != CATAGORY_ALL)
                return (null);

            if (_nCatagory == CATAGORY_BEYONDTV && sBeyondTVchedule == "")
                return (null);

            if (_nCatagory == CATAGORY_MCE && sMCESchedule == "")
                return (null);

            if (_nCatagory == CATAGORY_ALL && (_nCatagory == CATAGORY_MCE && sMCESchedule == ""))
                return (null);

            mxXMLReader = new pdamxXMLReader();
            hResultSet = new Hashtable();
            if (_nCatagory == CATAGORY_BEYONDTV || _nCatagory == CATAGORY_ALL)
            {
                mxXMLReader.Open(sBeyondTVchedule);
                mxXMLReader.AddNamespace("sch", "http://www.pdamediax.com/btv");
                xpathINode = mxXMLReader.GetNodePath("/sch:BeyondTV/sch:LastRecording/*");


                while (xpathINode.MoveNext())
                {
                    if (xpathINode.Current.Name.Equals("Show"))
                    {
                        hRecord = new Hashtable();
                        xpathINode.Current.MoveToFirstChild();
                        do
                        {
                            hRecord.Add(xpathINode.Current.Name, xpathINode.Current.Value);
                        }
                        while (xpathINode.Current.MoveToNext());
                        hRecord.Add("Catagory", "BeyondTV");
                        hResultSet.Add(Convert.ToString(++nCnt), hRecord);
                        xpathINode.Current.MoveToParent();
                    }
                }
            }
            if (_nCatagory == CATAGORY_MCE || _nCatagory == CATAGORY_ALL)
            {
                mxXMLReader.Open(sMCESchedule);
                mxXMLReader.AddNamespace("sch", "http://www.pdamediax.com/mce");
                xpathINode = mxXMLReader.GetNodePath("/sch:MCETV/sch:LastRecording/*");

                while (xpathINode.MoveNext())
                {
                    if (xpathINode.Current.Name.Equals("Show"))
                    {
                        hRecord = new Hashtable();
                        xpathINode.Current.MoveToFirstChild();
                        do
                        {
                            hRecord.Add(xpathINode.Current.Name, xpathINode.Current.Value);
                        }
                        while (xpathINode.Current.MoveToNext());
                        hRecord.Add("Catagory", "MCE");
                        hResultSet.Add(Convert.ToString(++nCnt), hRecord);
                        xpathINode.Current.MoveToParent();
                    }
                }
            }
            return (hResultSet);
        }
        public Hashtable GetNextScheduledRecording(int _nCatagory)
        {
            pdamxXMLReader mxXMLReader = null;
            Hashtable hResultSet;
            Hashtable hRecord;
            XPathNodeIterator xpathINode = null;
            int nCnt = 0;

            if (_nCatagory != CATAGORY_BEYONDTV && _nCatagory != CATAGORY_MCE && _nCatagory != CATAGORY_ALL)
                return (null);

            if (_nCatagory == CATAGORY_BEYONDTV && sBeyondTVchedule == "")
                return (null);

            if (_nCatagory == CATAGORY_MCE && sMCESchedule == "")
                return (null);

            if (_nCatagory == CATAGORY_ALL && (_nCatagory == CATAGORY_MCE && sMCESchedule == ""))
                return (null);

            mxXMLReader = new pdamxXMLReader();
            hResultSet = new Hashtable();
            if (_nCatagory == CATAGORY_BEYONDTV || _nCatagory == CATAGORY_ALL)
            {
                mxXMLReader.Open(sBeyondTVchedule);
                mxXMLReader.AddNamespace("sch", "http://www.pdamediax.com/btv");
                xpathINode = mxXMLReader.GetNodePath("/sch:BeyondTV/sch:NextRecording/*");


                while (xpathINode.MoveNext())
                {
                    if (xpathINode.Current.Name.Equals("Show"))
                    {
                        hRecord = new Hashtable();
                        xpathINode.Current.MoveToFirstChild();
                        do
                        {
                            hRecord.Add(xpathINode.Current.Name, xpathINode.Current.Value);
                        }
                        while (xpathINode.Current.MoveToNext());
                        hRecord.Add("Catagory", "BeyondTV");
                        hResultSet.Add(Convert.ToString(++nCnt), hRecord);
                        xpathINode.Current.MoveToParent();
                    }
                }
            }
            if (_nCatagory == CATAGORY_MCE || _nCatagory == CATAGORY_ALL)
            {
                mxXMLReader.Open(sMCESchedule);
                mxXMLReader.AddNamespace("sch", "http://www.pdamediax.com/mce");
                xpathINode = mxXMLReader.GetNodePath("/sch:MCETV/sch:NextRecording/*");

                while (xpathINode.MoveNext())
                {
                    if (xpathINode.Current.Name.Equals("Show"))
                    {
                        hRecord = new Hashtable();
                        xpathINode.Current.MoveToFirstChild();
                        do
                        {
                            hRecord.Add(xpathINode.Current.Name, xpathINode.Current.Value);
                        }
                        while (xpathINode.Current.MoveToNext());
                        hRecord.Add("Catagory", "MCE");
                        hResultSet.Add(Convert.ToString(++nCnt), hRecord);
                        xpathINode.Current.MoveToParent();
                        break; // Only read first one.
                    }
                }
            }
            return (hResultSet);
        }
        public Hashtable GetUpcomingRecordings(int _nCatagory)
        {
            pdamxXMLReader mxXMLReader = null;
            Hashtable hResultSet;
            Hashtable hRecord;
            XPathNodeIterator xpathINode = null;
            int nCnt = 0;

            if (_nCatagory != CATAGORY_BEYONDTV && _nCatagory != CATAGORY_MCE && _nCatagory != CATAGORY_ALL)
                return (null);

            if (_nCatagory == CATAGORY_BEYONDTV && sBeyondTVchedule == "")
                return (null);

            if (_nCatagory == CATAGORY_MCE && sMCESchedule == "")
                return (null);

            if (_nCatagory == CATAGORY_ALL && (_nCatagory == CATAGORY_MCE && sMCESchedule == ""))
                return (null);

            mxXMLReader = new pdamxXMLReader();
            hResultSet = new Hashtable();
            if (_nCatagory == CATAGORY_BEYONDTV || _nCatagory == CATAGORY_ALL)
            {
                mxXMLReader.Open(sBeyondTVchedule);
                mxXMLReader.AddNamespace("sch", "http://www.pdamediax.com/btv");
                xpathINode = mxXMLReader.GetNodePath("/sch:BeyondTV/sch:UpcomingRecordings/*");

                while (xpathINode.MoveNext())
                {
                    if (xpathINode.Current.Name.Equals("Show"))
                    {
                        hRecord = new Hashtable();
                        xpathINode.Current.MoveToFirstChild();
                        do
                        {
                            hRecord.Add(xpathINode.Current.Name, xpathINode.Current.Value);
                        }
                        while (xpathINode.Current.MoveToNext());
                        hRecord.Add("Catagory", "BeyondTV");
                        hResultSet.Add(Convert.ToString(++nCnt), hRecord);
                        xpathINode.Current.MoveToParent();
                    }
                }
            }
            if (_nCatagory == CATAGORY_MCE || _nCatagory == CATAGORY_ALL)
            {
                mxXMLReader.Open(sMCESchedule);
                mxXMLReader.AddNamespace("sch", "http://www.pdamediax.com/mce");
                xpathINode = mxXMLReader.GetNodePath("/sch:MCETV/sch:UpcomingRecordings/*");

                while (xpathINode.MoveNext())
                {
                    if (xpathINode.Current.Name.Equals("Show"))
                    {
                        hRecord = new Hashtable();
                        xpathINode.Current.MoveToFirstChild();
                        do
                        {
                            hRecord.Add(xpathINode.Current.Name, xpathINode.Current.Value);
                        }
                        while (xpathINode.Current.MoveToNext());
                        hRecord.Add("Catagory", "MCE");
                        hResultSet.Add(Convert.ToString(++nCnt), hRecord);
                        xpathINode.Current.MoveToParent();
                    }
                }
            }
            return (hResultSet);
        }
        public Hashtable SearchSchedule(String _sSearchValue, int _nSearchCatagory)
        {
            pdamxXMLReader mxXMLReader = null;
            pdamxSearchKeyGen mxSearchKeyGen; ;
            Hashtable hResultSet;
            Hashtable hRecord;
            XPathNodeIterator xpathINode = null;
            String sSearchCriteria;
            String[] sMultiSearch;
            int nCnt = 0;

            if (_nSearchCatagory != CATAGORY_BEYONDTV && _nSearchCatagory != CATAGORY_MCE && _nSearchCatagory != CATAGORY_ALL)
                return (null);

            if (_nSearchCatagory == CATAGORY_BEYONDTV && sBeyondTVchedule == "")
                return (null);

            if (_nSearchCatagory == CATAGORY_MCE && sMCESchedule == "")
                return (null);

            if (_nSearchCatagory == CATAGORY_BEYONDTV && sBeyondTVchedule == "")
                return (null);

            if (_nSearchCatagory == CATAGORY_ALL && (sMCESchedule == "" && sBeyondTVchedule == ""))
                return (null);

            sMultiSearch = _sSearchValue.Split(';');
            if (sMultiSearch.Length == 0)
            {
                sMultiSearch = new String[1];
                sMultiSearch[0] = _sSearchValue;
            }
            mxXMLReader = new pdamxXMLReader();
            hResultSet = new Hashtable();
            if (_nSearchCatagory == CATAGORY_BEYONDTV || _nSearchCatagory == CATAGORY_ALL)
            {
                mxSearchKeyGen = new pdamxSearchKeyGen();
                mxXMLReader.Open(sBeyondTVchedule);
                mxXMLReader.AddNamespace("sch", "http://www.pdamediax.com/btv");

                for (int i = 0; i < sMultiSearch.Length; i++)
                {
                    mxSearchKeyGen.GenerateKey(sMultiSearch[i].Replace("\"", "").Replace(":", " - "));
                    sSearchCriteria = "/sch:BeyondTV/sch:UpcomingRecordings/sch:Show[starts-with(sch:TitleStrongSearchKey,\"" + mxSearchKeyGen.StrongKey + "\")]"
                        + " | /sch:BeyondTV/sch:UpcomingRecordings/sch:Show[starts-with(sch:EpisodeStrongSearchKey,\"" + mxSearchKeyGen.StrongKey + "\")]";
                    
                    xpathINode = mxXMLReader.GetNodePath(sSearchCriteria);
                    while (xpathINode.MoveNext())
                    {
                        hRecord = new Hashtable();
                        xpathINode.Current.MoveToFirstChild();
                        if (xpathINode.Current.Name.Equals("Title"))
                        {
                            do
                            {
                                hRecord.Add(xpathINode.Current.Name, xpathINode.Current.Value);
                            }
                            while (xpathINode.Current.MoveToNext());
                        }
                        hRecord.Add("Catagory", "BeyondTV");
                        hResultSet.Add(Convert.ToString(++nCnt), hRecord);
                    }
                }
            }
            if (_nSearchCatagory == CATAGORY_MCE || _nSearchCatagory == CATAGORY_ALL)
            {
                mxSearchKeyGen = new pdamxSearchKeyGen();
                mxXMLReader.Open(sMCESchedule);
                mxXMLReader.AddNamespace("sch", "http://www.pdamediax.com/mce");

                for (int i = 0; i < sMultiSearch.Length; i++)
                {
                    mxSearchKeyGen.GenerateKey(sMultiSearch[i].Replace("\"", "").Replace(":", " - "));
                    sSearchCriteria = "/sch:MCETV/sch:UpcomingRecordings/sch:Show[starts-with(sch:TitleStrongSearchKey,\"" + mxSearchKeyGen.StrongKey + "\")]"
                        + " | /sch:MCETV/sch:UpcomingRecordings/sch:Show[starts-with(sch:EpisodeStrongSearchKey,\"" + mxSearchKeyGen.StrongKey + "\")]";

                    xpathINode = mxXMLReader.GetNodePath(sSearchCriteria);
                    while (xpathINode.MoveNext())
                    {
                        hRecord = new Hashtable();
                        xpathINode.Current.MoveToFirstChild();
                        if (xpathINode.Current.Name.Equals("Title"))
                        {
                            do
                            {
                                hRecord.Add(xpathINode.Current.Name, xpathINode.Current.Value);
                            }
                            while (xpathINode.Current.MoveToNext());
                        }
                        hRecord.Add("Catagory", "MCE");
                        hResultSet.Add(Convert.ToString(++nCnt), hRecord);
                    }
                }
            }
            return (hResultSet);
        }
        public String BeyondTVSchedule
        {
            get
            {
                return (sBeyondTVchedule);
            }
            set
            {
                if (value != null)
                    if (value.Length > 0)
                        sBeyondTVchedule = value;
            }
        }
        public String MCESchedule
        {
            get
            {
                return (sMCESchedule);
            }
            set
            {
                if (value != null)
                    if (value.Length > 0)
                        sMCESchedule = value;
            }
        }
	}
}
