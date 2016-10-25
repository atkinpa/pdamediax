using System;
using System.Diagnostics;
using System.Linq;
using System.Text;

namespace pdaMediaX.Util
{
    public class pdamxEventLogger
    {
        public const int InformationMsgType = 0;
        public const int ErrorMsgType = -1;
        public const int WarningMsgType = 1;

        private static String __sLoggerName = "pdaMediaxLog";
        private static String __sLoggerSource = "pdaMediaxApps";

        public static void AddEvent(Exception _eException)
        {
            AddEvent(null, __sLoggerName, __sLoggerSource, _eException);
        }
        public static void AddEvent(String _sApplication, Exception _eException)
        {
            AddEvent(_sApplication, __sLoggerName, __sLoggerSource, _eException);
        }
        public static void AddEvent(String sApplication, String _sLoggerName, String _sLoggerSource, Exception _eException)
        {
            String sLoggerName = __sLoggerName;
            String sLoggerSource = __sLoggerSource;

            if (_sLoggerName != null && sLoggerName != "")
            {
                sLoggerName = _sLoggerName;
            }
            if (_sLoggerSource != null & _sLoggerSource != "")
            {
                sLoggerSource = _sLoggerSource;
            }
            StringBuilder sbExceptionMessage = new StringBuilder();
            if (sApplication != null && sApplication != "")
            {
                sbExceptionMessage.Append("Application " + sApplication + Environment.NewLine);
            }
            sbExceptionMessage.Append("Exception Type" + Environment.NewLine);
            sbExceptionMessage.Append(_eException.GetType().Name);
            sbExceptionMessage.Append(Environment.NewLine + Environment.NewLine);
            sbExceptionMessage.Append("Message" + Environment.NewLine);
            sbExceptionMessage.Append(_eException.Message + Environment.NewLine + Environment.NewLine);
            sbExceptionMessage.Append("Stack Trace" + Environment.NewLine);
            sbExceptionMessage.Append(_eException.StackTrace + Environment.NewLine + Environment.NewLine);

            Exception eInnerException = _eException.InnerException;

            while (eInnerException != null)
            {
                sbExceptionMessage.Append("Exception Type" + Environment.NewLine);
                sbExceptionMessage.Append(eInnerException.GetType().Name);
                sbExceptionMessage.Append(Environment.NewLine + Environment.NewLine);
                sbExceptionMessage.Append("Message" + Environment.NewLine);
                sbExceptionMessage.Append(eInnerException.Message + Environment.NewLine + Environment.NewLine);
                sbExceptionMessage.Append("Stack Trace" + Environment.NewLine);
                sbExceptionMessage.Append(eInnerException.StackTrace + Environment.NewLine + Environment.NewLine);
                eInnerException = eInnerException.InnerException;
            }
            if (EventLog.SourceExists(sLoggerSource))
            {
                EventLog eLog = new EventLog(sLoggerName);
                eLog.Source = sLoggerSource;
                eLog.WriteEntry(sbExceptionMessage.ToString(), EventLogEntryType.Error);
            }
        }
        public static void AddEventMessage(String _sMessage)
        {
            AddEventMessage(__sLoggerName, __sLoggerSource, _sMessage, ErrorMsgType);
        }
        public static void AddEventMessage(String _sMessage, int _nMsgType)
        {
            AddEventMessage(__sLoggerName, __sLoggerSource, _sMessage, _nMsgType);
        }
        public static void AddEventMessage(String _sLoggerName, String _sLoggerSource, String _sMessage, int _nMsgType)
        {
            String sLoggerName = __sLoggerName;
            String sLoggerSource = __sLoggerSource;
            EventLogEntryType eltyMsgType = EventLogEntryType.Error;

            if (_sMessage == null || _sMessage == "")
            {
                return;
            }
            if (_sLoggerName != null && sLoggerName != "")
            {
                sLoggerName = _sLoggerName;
            }
            if (_sLoggerSource != null & _sLoggerSource != "")
            {
                sLoggerSource = _sLoggerSource;
            }
            switch (_nMsgType)
            {
                case InformationMsgType:
                    eltyMsgType = EventLogEntryType.Information;
                    break;
                case WarningMsgType:
                    eltyMsgType = EventLogEntryType.Warning;
                    break;
            }
            
            if (EventLog. SourceExists(sLoggerSource))
            {
                EventLog eLog = new EventLog(sLoggerName);
                eLog.Source = sLoggerSource;
                eLog.WriteEntry(_sMessage, eltyMsgType);
            }
            
        }
    }
}
