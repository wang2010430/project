/***************************************************************************************************
* copyright : CMIND-SEMI
* version   : 1.00
* file      : LogNetHelper.cs
* date      : 2023/7/20 15:16:36
* author    : jinlong.wang
* brief     : Log Helper
* section Modification History
* - 1.0 : Initial version - jinlong.wang
***************************************************************************************************/

using log4net;
using System;
using System.Diagnostics;
using System.IO;

namespace log4net
{
    public class LogNetHelper
    {
        private static readonly ILog _log = LogManager.GetLogger(typeof(LogNetHelper));
        public static void Error(string msg) => _log.Error(AppendClassLine(msg));
        public static void Error(Exception ex, string msg = null) => _log.Error(msg, ex);

        public static void Info(string msg) => _log.Info(AppendClassLine(msg));
        public static void Info(Exception ex, string msg = null) => _log.Info(msg, ex);

        public static void Debug(string msg) => _log.Debug(AppendClassLine(msg));
        public static void Debug(Exception ex, string msg = null) => _log.Debug(msg, ex);

        public static void Warn(string msg) => _log.Warn(AppendClassLine(msg));
        public static void Warn(Exception ex, string msg = null) => _log.Warn(msg, ex);

        public static void Fatal(string msg) => _log.Warn(AppendClassLine(msg));
        public static void Fatal(Exception ex, string msg = null) => _log.Fatal(msg, ex);

        static string AppendClassLine(string msg)
        {
            string logStr = msg;
            try
            {
                StackTrace st = new StackTrace(true);
                StackFrame sf = st.GetFrame(2);
                logStr = $"[{Path.GetFileName(sf.GetFileName())}:{sf.GetFileLineNumber().ToString()}] {msg} ";
            }
            catch { }
            return logStr;
        }
    }
}
