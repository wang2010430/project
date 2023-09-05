/***************************************************************************************************
* copyright : CMIND-SEMI
* version   : 1.00
* file      : GlobalEventHandler.cs
* date      : 2023/7/12 15:10:53
* author    : jinlong.wang
* brief     : 
* section Modification History
* - 1.0 : Initial version - jinlong.wang
***************************************************************************************************/

using Common;
using System;

namespace NVTool.BLL
{
    class GlobalEventHandler
    {
        #region runMessage
        // 定义静态的 delegate 类型
        public delegate void RumMessageDelegate(string message);

        // 定义静态的 event
        public static event RumMessageDelegate runMessageEvent;

        // 触发全局事件的方法
        public static void TriggerrunMsgEvent(string message)
        {
            // 如果有注册的事件处理程序，则触发事件
            runMessageEvent?.Invoke(message);
        }
        #endregion

        #region CommMessage
        public delegate void CommMessageDelegate(object sender, DateTime time, BytesType type, byte[] bytes, string message);
        public static event CommMessageDelegate commEvent;
        public static void TriggerCommMsgEvent(object sender, DateTime time, BytesType type, byte[] bytes, string message)
        {
            commEvent?.Invoke(sender, time, type, bytes, message);
        }
        #endregion

    }
}
