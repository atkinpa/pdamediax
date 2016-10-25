using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using pdaMediaX.Common;

namespace pdaMediaX.Util
{
    public class pdamxSearchKeyGen
	{
        String sStrongKey;
        String sWeakKey;
        String sNumericKey;
        String sNumericLowRangeKey;
        String sNumericHighRangeKey;
        String[] sVowelFilter = { "A", "E", "I", "O", "U","Y"};

        public pdamxSearchKeyGen()
        {
        }
        private String FilterVowels(String _sText)
        {
            String sReturnText = _sText;

            if (_sText == null)
                return (null);

            if (_sText.Trim().Length == 0)
                return (_sText);

            for (int i = 0; i < sVowelFilter.Length; i++)
                sReturnText = sReturnText.Replace(sVowelFilter[i], "").Replace(sVowelFilter[i].ToLower(), "");
            return (sReturnText);
        }
        private String FilterStartWordFilter(String _sText)
        {
            String[] sStartWordFilter = { "A ", "An ", "And ", "I ", "The "};
            String sReturnText = _sText;

            if (_sText == null)
                return (null);

            if (_sText.Trim().Length == 0)
                return (_sText);

            for (int i = 0; i < sStartWordFilter.Length; i++)
            {
                if (_sText.Length <= sStartWordFilter[i].Length)
                    break;

                if (_sText.Substring(0, sStartWordFilter[i].Length).ToLower().Equals(sStartWordFilter[i].ToLower()))
                {
                    sReturnText = (_sText.Substring(sStartWordFilter[i].Length, _sText.Length - (sStartWordFilter[i].Length)));
                    break;
                }
            }
            return (sReturnText.Replace("(hd)", "").Replace("(hd-tp)", "").Replace(" - ", ": "));
        }
        public String GenerateKey(String _sText)
        {
            String sKey;

            sWeakKey = "";
            sStrongKey = "";
            sNumericKey = "";
            sNumericLowRangeKey = "";
            sNumericHighRangeKey = "";

            if (_sText == null)
                _sText = "NO DATA";

            if (_sText.Trim().Length == 0)
                _sText = "NO DATA";

            sKey = FilterStartWordFilter(pdamxUtility.FilterSpecialChar(_sText).ToLower());
            sKey = Regex.Replace(sKey, @"[^a-z0-9,.]", "", RegexOptions.IgnoreCase);
            if (sKey.Trim().Length == 0)
            {
                sKey = Regex.Replace(sKey, @"[^0-9,.]", "", RegexOptions.IgnoreCase);
                if (sKey.Trim().Length == 0)
                {
                    if (pdamxUtility.IsNumeric(pdamxUtility.FilterSpecialChar(_sText).ToLower()))
                        sKey = pdamxUtility.FilterSpecialChar(_sText).ToLower();
                    else
                        sKey = "NO KEY";
                }
            }
            sStrongKey = sKey;
            sKey = Regex.Replace(sKey, @"[^a-z,.]", "", RegexOptions.IgnoreCase);
            sKey = FilterVowels(sKey);
            if (sKey.Trim().Length == 0)
                sKey = sStrongKey;
            sWeakKey = sKey;
            sNumericKey = GetNumerickKey(sStrongKey);
            sNumericLowRangeKey = GetNumerickKey(sWeakKey);
            sNumericHighRangeKey = sNumericKey;
            return(sStrongKey);
        }
        public String NumericLowRangeKey
        {
            get
            {
                return (sNumericLowRangeKey);
            }
        }
        public String NumericHighRangeKey
        {
            get
            {
                return (sNumericHighRangeKey);
            }
        }
        public String NumericKey
        {
            get
            {
                return (sNumericKey);
            }
        }
        public String StrongKey
        {
            get
            {
                return(sStrongKey);
            }
        }
        public String WeakKey
        {
            get
            {
                return(sWeakKey);
            }
        }
        private String GetNumerickKey(String _sText)
        {
            String sMergedKey = "";
            String[] sAscII = { "a", "b", "c", "d", "e", "f", "g", "h", "i", "j", "k", "l", "m", "n", "o", "p", "q", "r", "s", "t", "u", "v", "w", "x", "y", "z", "0", "1", "2", "3", "4", "5", "6", "7", "8", "9" };

            int nDefaultKeyWeight = 8;
            int nKeyWeightforVowels = 15;
            int nKeyWeight = 0;

            int[] nStarterKey = new int[6];
            int[] nMiddleKey = new int[5];
            int[] nEndKey = new int[4];

            for (int i = 0; i < nStarterKey.Length; i++)
            {
                nStarterKey[i] = 255;
            }
            for (int i = 0; i < nMiddleKey.Length; i++)
            {
                nMiddleKey[i] = 0;
            }
            for (int i = 0; i < nEndKey.Length; i++)
            {
                nEndKey[i] = 0;
            }
            for (int i = 0; i < _sText.Length; i++)
            {
                bool bVowelFound = false;
                for (int j = 0; j < sVowelFilter.Length; j++)
                {
                    if (_sText.Substring(i, 1).ToLower().Equals(sVowelFilter[j].ToLower()))
                    {
                        nKeyWeight = nKeyWeight + nKeyWeightforVowels;
                        bVowelFound = true;
                        break;
                    }
                }
                if (!bVowelFound)
                    nKeyWeight = nKeyWeight + nDefaultKeyWeight;
                for (int j = 0, k = 0; j < sAscII.Length; j++, k++)
                {
                    if (_sText.Substring(i, 1).ToLower().Equals(sAscII[j]))
                        nMiddleKey[k] = nMiddleKey[k] + nDefaultKeyWeight;
                    if (k == (nMiddleKey.Length - 1))
                        k = 0;
                }
                for (int j = 0; j < sVowelFilter.Length; j++)
                {
                    if (_sText.Substring(i, 1).ToLower().Equals(sVowelFilter[j]))
                        nStarterKey[j] = nStarterKey[j] - nKeyWeightforVowels;
                }
                for (int j = 0, k = 0; j < sAscII.Length; j++, k++)
                {
                    if (_sText.Substring(i, 1).ToLower().Equals(sAscII[j]))
                        nEndKey[k] = nEndKey[k] + (nKeyWeight * j);
                    if (k == (nEndKey.Length-1))
                        k = 0;
                }
            }
            for (int i = 0; i < nEndKey.Length; i++)
                nStarterKey[i] = nStarterKey[i] + nEndKey[i];

            for (int i = 0; i < nMiddleKey.Length; i++)
                nStarterKey[i] = nStarterKey[i] + nMiddleKey[i];

            for (int i = 0; i < nStarterKey.Length; i++)
                sMergedKey = sMergedKey + Convert.ToString(nStarterKey[i]);

            if (sMergedKey.Length > 20)
                sMergedKey = sMergedKey.Substring(0, 19);

            return (sMergedKey);
        }
	}
}
