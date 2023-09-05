/***************************************************************************************************
* copyright : 芯迈微半导体（珠海）有限公司
* version   : 1.00
* file      : LogHelper.cs
* date      : 2023/03/16
* author    : haozhe.ni
* brief     : 日志帮助类
* section Modification History
* - 1.0 : Initial version - haozhe.ni
***************************************************************************************************/

using System;

namespace Common
{
    public static class LogHelper
    {
        /// <summary>
        /// 系统日志
        /// </summary>
        public static LogWriteHelper SystemLog = new LogWriteHelper(string.Format("SystemLog_{0}_", DateTime.Now.ToString("yyyyMMdd")));

        /// <summary>
        /// 系统日志记录
        /// </summary>
        /// <param name="msg"></param>
        /// <param name="type"></param>
        public static void Log(string msg, LogMsgType type = LogMsgType.Info)
        {
            SystemLog.Log(msg, type);
        }
    }
}
