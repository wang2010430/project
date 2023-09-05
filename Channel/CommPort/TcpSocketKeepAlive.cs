/***************************************************************************************************
* copyright : 芯迈微半导体（珠海）有限公司
* version   : 1.00
* file      : SocketPortBase.cs
* date      : 2023/03/16
* author    : haozhe.ni
* brief     : SocketPortBase
* section Modification History
* - 1.0 : Initial version - haozhe.ni
***************************************************************************************************/

using Common;
using System;
using System.Net.Sockets;
using System.Runtime.InteropServices;

namespace Channel
{
    internal class TcpSocketKeepAlive
    {
        static private byte[] KeepAlive(uint onOff, uint keepAliveTime, uint keepAliveInterval)
        {
            byte[] buffer = new byte[Marshal.SizeOf(onOff) * 3];
            
            BitConverter.GetBytes(onOff).CopyTo(buffer, 0);
            BitConverter.GetBytes(keepAliveTime).CopyTo(buffer, Marshal.SizeOf(onOff));
            BitConverter.GetBytes(keepAliveInterval).CopyTo(buffer, Marshal.SizeOf(onOff) * 2);
           
            return buffer;
        }

        static internal void KeepAlive(Socket socket, uint onOff, uint keepAliveTime, uint keepAliveInterval)
        {
            if (socket != null && socket.Connected)
            {
                socket.IOControl(IOControlCode.KeepAliveValues, KeepAlive(onOff, keepAliveTime, keepAliveInterval), null);
            }
        }

        /// <summary>
        /// 另一种判断connected的方法，但未检测对端网线断开或ungraceful的情况
        /// </summary>
        /// <param name="theSocket"></param>
        /// <param name="debugMode"></param>
        /// <param name="portDesc"></param>
        /// <returns></returns>
        internal static bool IsSocketConnected(Socket theSocket, bool debugMode, string portDesc)
        {
            #region remarks
            /* As zendar wrote, it is nice to use the Socket.Poll and Socket.Available, but you need to take into consideration
             * that the socket might not have been initialized in the first place.
             * This is the last (I believe) piece of information and it is supplied by the Socket.Connected property.
             * The revised version of the method would looks something like this:
             * from：http://stackoverflow.com/questions/2661764/how-to-check-if-a-socket-is-connected-disconnected-in-c */
            #endregion

            #region 过程

            if (theSocket == null)
            {
                return false;
            }

            bool part1 = theSocket.Poll(1000, SelectMode.SelectRead);
            bool part2 = (theSocket.Available == 0);
           
            if ((part1 && part2) || !theSocket.Connected)
            {
                if(debugMode)
                {
                    UtMessageBase.ShowOneMessage(string.Format("socket连接状态判断结果:{0}已经断开", portDesc), PopupMessageType.Info);
                }

                return false;
            }
            
            return true;
            #endregion
        }
    }
}
