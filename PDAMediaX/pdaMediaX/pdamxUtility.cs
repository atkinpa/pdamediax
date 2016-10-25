using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace pdaMediaX.Common
{
    public class pdamxUtility
    {
        public static String CheckForQuotes(String _sText)
        {
             return (_sText.Replace("'", "''"));
        }
        public static String FilterSpecialChar(String _sText)
        {
            String sFiltedText = _sText;
            String[] sFilter = {"/", "@", "$", "%", "^", "~", "!", "&", "*", "(", ")", "-", "=", "`", "{", "}", "[", "]", ":", "\'", "\"", "?", "<", ">", ",","/", "\\"};
            
            if (_sText == null)
                return(null);

            for (int i = 0; i < sFilter.Length; i++)
                sFiltedText = sFiltedText.Replace(sFilter[i], "");
            return(sFiltedText.Trim( '/', '@', '$', '%', '^', '~', '!', '&', '*', '(', ')', '-', '+', '=', '`', '{', '}', '[', ']', ':', ';', '\'', '\'', '?', '<', '>', ',', '/', '\\'));
        }

        public static String FormatStorageSize(String _sStorageSize)
        {
            //
            // Format disk storage number to display friendly format...
            //
            double dStorageSize;
            double dKB = 0;
            double dMB = 0;
            double dGB = 0;
            double dTB = 0;

            if (_sStorageSize == null)
                return (_sStorageSize);

            if (_sStorageSize.Trim().Length == 0)
                return (_sStorageSize);

            dStorageSize = Convert.ToDouble(_sStorageSize);
            dKB = dStorageSize / 1024;
            if (dKB < 1024)
                return (String.Format("{0:00}", Convert.ToString(Math.Round(dKB, 2))) + " KB");

            dMB = dKB / 1024;
            if (dMB < 1024)
                return (String.Format("{0:00}", Convert.ToString(Math.Round(dMB, 2))) + " MB");

            dGB = dMB / 1024;
            if (dGB < 1024)
                return (String.Format("{0:00}", Convert.ToString(Math.Round(dGB, 2))) + " GB");

            dTB = dGB / 1024;
            if (dTB < 1024)
                return (String.Format("{0:00}", Convert.ToString(Math.Round(dTB, 2))) + " TB");

            return (String.Format("{0:00}", Convert.ToString(Math.Round(dTB, 2))) + " TB+");
        }
        public static String FormatMiliseconds(String _sMiliSecond)
        {
            int hr = 0;
            int mm = 0;
            int ss = 0;

            if (_sMiliSecond == null)
                return (_sMiliSecond);

            if (_sMiliSecond.Trim().Length == 0)
                return (_sMiliSecond);

            ss = (int)(Convert.ToDouble(_sMiliSecond) / 1000);
            if (ss >= 60)
            {
                mm = ss / 60;
                ss = ss - (mm * 60);
                if (mm >= 60)
                {
                    hr = mm / 60;
                    mm = mm - (hr * 60);
                }
            }
            return (String.Format("{0:00}", hr)
                + ":"
                + String.Format("{0:00}", mm)
                + ":"
                + String.Format("{0:00}", ss));
        }
        public static String FormatNanoseconds(String _sNanoSecond)
        {
            int hr = 0;
            int mm = 0;
            int ss = 0;

            if (_sNanoSecond == null)
                return (_sNanoSecond);

            if (_sNanoSecond.Trim().Length == 0)
                return (_sNanoSecond);

            ss = (int)(Convert.ToDouble(_sNanoSecond) / 10000000);
            if (ss >= 60)
            {
                mm = ss / 60;
                ss = ss - (mm * 60);
                if (mm >= 60)
                {
                    hr = mm / 60;
                    mm = mm - (hr * 60);
                }
            }
            return (String.Format("{0:00}", hr)
                + ":"
                + String.Format("{0:00}", mm)
                + ":"
                + String.Format("{0:00}", ss));
        }
        public static String FormatNumber(int nNumber)
        {
            return (String.Format("{0:0,0}", nNumber));
        }
        public static String FormatNumber(long lNumber)
        {
            return (String.Format("{0:0,0}", lNumber));
        }
        public static String FormatSeconds(String _sSecond)
        {
            return(FormatSecondsInternal(_sSecond,false));
        }
        public static String FormatSecondsInText(String _sSecond)
        {
            return (FormatSecondsInternal(_sSecond, true));
        }
        private static String FormatSecondsInternal(String _sSecond, bool bText)
        {
            int hr = 0;
            int mm = 0;
            int ss = 0;
            String sFormattedTime = "";

            if (_sSecond == null)
                return(_sSecond);

            if (_sSecond.Trim().Length == 0)
                return(_sSecond);

            ss = (int)(Convert.ToDouble(_sSecond));
            if (ss >= 60)
            {
                mm = ss / 60;
                ss = ss - (mm * 60);
                if (mm >= 60)
                {
                    hr = mm / 60;
                    mm = mm - (hr * 60);
                }
            }
            if (bText)
            {
                if (hr > 0)
                    sFormattedTime = String.Format(String.Format("{0:00}", hr)) + " hours";
                if (mm > 0)
                    sFormattedTime = sFormattedTime + (hr > 0 ? ", " : "") + String.Format("{0:00}", mm) + " minutes";
                if (ss > 0)
                    sFormattedTime = sFormattedTime + (hr > 0 || mm > 0 ? ", and " : "") + String.Format("{0:00}", ss) + " seconds";
            }
            else
            {
                sFormattedTime = String.Format("{0:00}", hr)
                    + ":"
                    + String.Format("{0:00}", mm)
                    + ":"
                    + String.Format("{0:00}", ss);
            }
            return (sFormattedTime);
        }
        public static bool IsNumeric(String _sNumeric)
        {
            return (Regex.IsMatch(_sNumeric, "^\\d+(\\.\\d+)?$"));
        }
        public static bool IsDecimal(String _sDecimal)
        {
            try
            {
                Convert.ToDouble(_sDecimal);
                return true;
            }
            catch
            {
                return false;
            }
        }
        public static String StripPath(String _sFileWithPath, bool bStripExt)
        {
            String sFile = "";
            int nStartIdx;

            if (_sFileWithPath == null)
                return ("");

            if (_sFileWithPath.Trim().Length == 0)
                return ("");

            nStartIdx = _sFileWithPath.LastIndexOf("\\");
            if (nStartIdx < 0)
                nStartIdx = 0;
            else
                nStartIdx++;
            sFile = _sFileWithPath.Substring(nStartIdx, _sFileWithPath.Length - nStartIdx);
            if (bStripExt)
            {
                nStartIdx = sFile.LastIndexOf(".");
                if (nStartIdx > -1)
                    sFile = sFile.Substring(0, sFile.Length - (sFile.Length - nStartIdx));
            }
            return (sFile);
        }
        public static String TrimLeadingZeros(String _sText)
        {
            int nIdx;
            if (_sText == null)
                return (null);

            if (_sText.Length == 0)
                return (_sText);

            for (nIdx = 0; nIdx < _sText.Length; nIdx++)
                if (!_sText.Substring(nIdx, 1).Equals("0"))
                    break;
            if (_sText.Substring(nIdx,1).Equals(":"))
                nIdx++;
            return (_sText.Substring(nIdx, _sText.Length - nIdx));
        }
        public static String extractHost(String _sUrl)
        {
            int nStartIdx;
            int nEndIdx;

            nStartIdx = _sUrl.IndexOf("//") + 2;
            nEndIdx = _sUrl.Substring(nStartIdx).IndexOf(":");
            if (nEndIdx == -1)
            {
                nEndIdx = _sUrl.Substring(nStartIdx).IndexOf("/");
            }
            if (nEndIdx == -1)
            {
                nEndIdx = _sUrl.Length - 1;
            }
            return (_sUrl.Substring(nStartIdx, (nStartIdx + nEndIdx) - nStartIdx));
        }
        public static bool contains(String _sStringObject, String _sSequence)
        {
            return (_sStringObject.IndexOf(_sSequence) > -1 ? true : false);
        }
        public static String insertString(String _sStringObject, int _nStartIndex, String _sValue)
        {
            return (_sStringObject.Substring(0, _nStartIndex)
                    + _sValue + _sStringObject.Substring(_nStartIndex + 1, _sStringObject.Length - (_nStartIndex + 1)));
        }

        /*
         * Return a hex representation for the byte array.
         */
        public static String getHexValue(byte[] _bValue)
        {
            String sRetValue = "";

            for (int nByteCnt = 0; nByteCnt < _bValue.Length; nByteCnt++)
            {
                sRetValue += getHexValue(_bValue[nByteCnt]);
            }
            return (sRetValue);
        }

        /*
         * Return a hex representation for the byte value.
         */
        public static String getHexValue(byte _bValue)
        {
            String[] sHexTable = { "0", "1", "2", "3", "4", "5", "6", "7", "8", "9", "A", "B", "C", "D", "E", "F" };
            int nHighByte;
            int nLowByte;

            // Convert byte to hex string and return it...
            nHighByte = Math.Abs(Convert.ToInt32(_bValue) / 16);
            nLowByte = Math.Abs((nHighByte * 16) - Convert.ToInt32(_bValue));
            return (sHexTable[nHighByte] + sHexTable[nLowByte]);
        }

        /*
         * Return a hex representation of a alphabetic string value.
         */
       // public static String getHexValue(String _sValue)
        //{
            // Check if _sValue contains a value other than spaces or null...
        //    if (pdamxDataParser.isStringEmpty(_sValue))
        //    {
        //        return (null);
        //    }
        //    return (getHexValue(_sValue.getBytes()));
        //}
    }
}
