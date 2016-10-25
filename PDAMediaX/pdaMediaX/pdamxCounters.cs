using System;
using System.Globalization;
using System.Collections;
using System.Collections.Generic;
using pdaMediaX.Common;

// Learning c# by example, http://www.fincher.org/tips/Languages/csharp.shtml

namespace pdaMediaX.Util
{
    public class pdamxCounters
    {
        Hashtable hCounerTable;

        public pdamxCounters()
        {
            hCounerTable = new Hashtable();
        }
        private void AddCounter(String _sCounterName, int nCount)
        {
            GetCounterTable().Add(_sCounterName, Convert.ToString(nCount));
        }
        public bool CreatCounter(String _sCounterName)
        {
            if (isExist(_sCounterName))
                return (false);

            AddCounter(_sCounterName, 0);
            return (true);
        }
        public int GetCounter(String _sCounterName)
        {
            if (!isExist(_sCounterName))
                return (-1);

            return (Int32.Parse((String)GetCounterTable()[_sCounterName]));
        }
        public IDictionary<String, int> GetCounter1(String _sCounterWildCardName, bool bSort)
        {
            IDictionaryEnumerator ideKeys = GetCounterTable().GetEnumerator();
            Dictionary<String, int> dList = new Dictionary<String, int>();
            int nWildCardLen = 0;
            int nSearchLen = 0;

            if (_sCounterWildCardName == null)
                return (null);

            if (_sCounterWildCardName.Trim().Length == 0)
                return (null);

            nWildCardLen = _sCounterWildCardName.Length;
            if (_sCounterWildCardName.Contains("*"))
                nWildCardLen = _sCounterWildCardName.IndexOf("*");

            if (bSort)
            {
                ArrayList alKeys = new ArrayList(GetCounterTable().Keys);
                alKeys.Sort();

                foreach (String sKey in alKeys)
                {
                    nSearchLen = nWildCardLen;
                    if (sKey.ToString().Length < nWildCardLen)
                        nSearchLen = sKey.ToString().Length;

                    if (sKey.ToString().Substring(0, nSearchLen).Equals(_sCounterWildCardName.Substring(0, nWildCardLen)))
                        dList.Add(sKey.ToString(), GetCounter(sKey.ToString()));
                }
            } 
            else
            {
                while (ideKeys.MoveNext())
                {
                    nSearchLen = nWildCardLen;
                    if (ideKeys.Key.ToString().Length < nWildCardLen)
                        nSearchLen = ideKeys.Key.ToString().Length;

                    if (ideKeys.Key.ToString().Substring(0, nSearchLen).Equals(_sCounterWildCardName.Substring(0, nWildCardLen)))
                        dList.Add(ideKeys.Key.ToString(), Convert.ToInt32(ideKeys.Entry.Value.ToString()));
                }   
            }
            return (dList);
        }
        public IDictionary<String, int> GetCounters(bool bSort)
        {
            IDictionaryEnumerator ideKeys = GetCounterTable().GetEnumerator();
            Dictionary<String, int> dList = new Dictionary<String, int>();
            while (ideKeys.MoveNext())
            {
                dList.Add(ideKeys.Key.ToString(), Convert.ToInt32(ideKeys.Entry.Value.ToString()));
            }
            return (dList);
        }
        public IEnumerable<String>GetCounterNames()
        {
            IDictionaryEnumerator ideKeys = GetCounterTable().GetEnumerator();
            while(ideKeys.MoveNext())
            {
                yield return ideKeys.Key.ToString();
            }
        }
        public IEnumerable<int> GetCounterValues()
        {
            IDictionaryEnumerator ideKeys = GetCounterTable().GetEnumerator();
            while (ideKeys.MoveNext())
            {
                yield return Convert.ToInt32(ideKeys.Entry.Value.ToString()); ;
            }
        }
        private Hashtable GetCounterTable()
        {
            return (hCounerTable);
        }
        private bool isExist(String _sCountName)
        {
            if (_sCountName == null)
                return (false);

            if (_sCountName.Length == 0)
                return (false);

            return (GetCounterTable().ContainsKey(_sCountName));
        }
        public void RemoveAllCounter()
        {
            foreach (String sKey in GetCounterNames())
                GetCounterTable().Remove(sKey);
        }
        public bool RomoveCounter(String _sCountName)
        {
            if (!isExist(_sCountName))
                return (false);

            GetCounterTable().Remove(_sCountName);
            return (true);
        }
        public bool RomoveCounter1(String _sCounterWildCardName)
        {
            if (_sCounterWildCardName == null)
                return (false);

            if (_sCounterWildCardName.Trim().Length == 0)
                return (false); ;

            foreach (KeyValuePair<String, int> kvpList in GetCounter1(_sCounterWildCardName, false))
            {
                RomoveCounter(kvpList.Key);
            }
            return (true);
        }
        public bool SetCounter(String _sCountName, int nValue)
        {
            int nCnt = GetCounter(_sCountName);

            if (nCnt == -1) // Not found...
                return (false);

            nCnt = nValue;
            RomoveCounter(_sCountName);
            AddCounter(_sCountName, nCnt);
            return (true);
        }
        public bool SetCounter1(String _sCounterWildCardName, int nValue)
        {
            if (_sCounterWildCardName == null)
                return (false);

            if (_sCounterWildCardName.Trim().Length == 0)
                return (false); ;

            foreach (KeyValuePair<String, int> kvpList in GetCounter1(_sCounterWildCardName, false))
            {
                SetCounter(kvpList.Key, nValue);
            }
            return (true);
        }
        public int SumCounters(String _sCounterWildCardName)
        {
            int nTotal = 0;

            if (_sCounterWildCardName == null)
                return (-1);

            if (_sCounterWildCardName.Trim().Length == 0)
                return (-1);

            foreach (KeyValuePair<String, int> kvpList in GetCounter1(_sCounterWildCardName, false))
            {
                nTotal = nTotal + Convert.ToInt32(kvpList.Value);
            }
            return (nTotal);
        }
    }
}
