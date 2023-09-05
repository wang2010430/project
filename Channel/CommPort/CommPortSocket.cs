/***************************************************************************************************
* copyright : 芯迈微半导体（珠海）有限公司
* version   : 1.00
* file      : CommPortSocket.cs
* date      : 2023/03/16
* author    : haozhe.ni
* brief     : CommPortSocket
* section Modification History
* - 1.0 : Initial version - haozhe.ni
***************************************************************************************************/

using Common;
using System;
using System.Net.Sockets;

namespace Channel
{
    public delegate void WhenSocketClosed(CommPortSocket socketPort);

    /// <summary>
    /// CommPortTCPNETServer  网络通讯服务端。
    /// </summary>
    public class CommPortSocket : SocketPortBase
    {
        private bool _closed;                   // 防止重复调用Close函数
        private WhenSocketClosed _beforeSocketClose;

        public WhenSocketClosed BeforeSocketClose
        {
            set
            {
                _beforeSocketClose = value;
            }
        }

        private WhenSocketClosed _afterSocketClose;

        public WhenSocketClosed AfterSocketClose
        {
            set
            {
                _afterSocketClose = value;
            }
        }

        public CommPortSocket(Socket skt)
            : base(skt)
        {
            CallEventConnected();
        }

        void Close(PortClosedReasons portClosedReason)
        {
            base.Close();

            if (EnableDebugMessage)
            {
                UtMessageBase.ShowOneMessage(string.Format("调用 Close({0}) 关闭TCP Socket", portClosedReason), PopupMessageType.Info);
            }

            CloseSocket(PortClosedReasons.LocalClosedNormally);
        }

        override internal void CloseSocket(PortClosedReasons reason)
        {
            try
            {
                if (_closed)
                {
                    return;
                }

                _closed = true;

                if (_beforeSocketClose != null)
                {
                    _beforeSocketClose(this);
                }

                _beforeSocketClose = null;

                if (connectedSocket != null && connectedSocket.Connected)
                {
                    connectedSocket.Shutdown(SocketShutdown.Both);
                    CallEventCommMessageOccured("TCP Socket已经关闭与“" + connectedSocket.RemoteEndPoint + "”的连接。关闭原因：" + reason);
                    connectedSocket.Close();
                }

                #region 以下代码从上面的括号中移出来
                if (_afterSocketClose != null)
                {
                    _afterSocketClose(this);
                }

                _afterSocketClose = null;
                connectedSocket = null;
                #endregion

                isOpened = false;
                CallEventClosed(reason);
            }
            catch (Exception ex)
            {
                CallEventCommMessageOccured("TCP Socket关闭错误：" + ex.Message);
                connectedSocket = null;
                isOpened = false;
            }
        }

        #region ICommPort 成员
        /// <summary>
        /// 返回端口状态（侦听表示打开，建立有效的连接表示连接，还未处于侦听表示关闭状态）。
        /// </summary>
        public override PortStatus PortState
        {
            get
            {
                if (connectedSocket != null && connectedSocket.Connected)
                {
                    return PortStatus.Connected;
                }

                return PortStatus.Closed;
            }
        }

        /// <summary>
        /// 返回是否与远端建立了有效的连接。
        /// </summary>
        public override bool IsConnected
        {
            get
            {
                return connectedSocket != null && connectedSocket.Connected;
            }
        }

        /// <summary>
        /// 进一步判断连接是否中断
        /// 这一判断通常会额外消耗时间
        /// </summary>
        /// <returns></returns>
        public override bool IsConnectionBroken()
        {
            return connectedSocket == null || !TcpSocketKeepAlive.IsSocketConnected(connectedSocket, EnableDebugMessage, PortDescription);
        }

        readonly TcpSocketParam _portParam = new TcpSocketParam();

        public override PortParamBase PortParam
        {
            get
            {
                return _portParam;
            }
        }

        public override bool Open()
        {
            try
            {
                if (connectedSocket != null && connectedSocket.Connected)
                {
                    base.Open();
                    
                    return true;
                }
            }
            catch (SocketException ex)
            {
                CallEventCommMessageOccured("打开TCP Socket端口时异常信息：" + ex.Message);
            }

            return false;
        }

        public override void Close()
        {
            Close(PortClosedReasons.LocalClosedNormally);
        }

        public override bool Send(byte[] data)
        {
            if (data == null || data.Length == 0)
            {
                UtMessageBase.ShowOneMessage(string.Format("{0}发送数据失败，数据为空", PortParam.PortTypeName), PopupMessageType.Info);
                
                return false;
            }

            if (data.Length != 0 && IsConnected)
            {
                return base.Send(data);
            }

            UtMessageBase.ShowOneMessage(string.Format("{0}发送数据失败，端口未打开", PortParam.PortTypeName), PopupMessageType.Info);
           
            return false;
        }

        public override bool Send(byte[] buffer, int offset, int size)
        {
            if (connectedSocket == null || buffer == null || buffer.Length == 0 || buffer.Length < offset + size || !IsConnected)
            {
                return false;
            }

            try
            {
                return base.Send(buffer, offset, size);
            }
            catch (Exception ex)
            {
                CallEventCommMessageOccured("TCP Socket端口发送数据错误：" + ex.Message);
                
                return false;
            }
        }
        #endregion
    }
} 

    
