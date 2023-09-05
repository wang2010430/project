/***************************************************************************************************
* copyright : 芯迈微半导体（珠海）有限公司
* version   : 1.00
* file      : CommPortTcpClient.cs
* date      : 2023/03/16
* author    : haozhe.ni
* brief     : CommPortTcpClient
* section Modification History
* - 1.0 : Initial version - haozhe.ni
***************************************************************************************************/

using Common;
using System;
using System.Net;
using System.Net.Sockets;
using System.Threading;

namespace Channel
{
    /// <summary>
    /// CommPortTCPNETClient  网络通讯客户端
    /// </summary>
    public class CommPortTcpClient : SocketPortBase
    {
        private bool _closeCalled;
        private AutoResetEvent waitConnectedEvent;
        private readonly TcpClientParam portParam;

        public bool AutoConnect { get; set; }

        public CommPortTcpClient(TcpClientParam portParam)
            : base(null)
        {
            this.portParam = portParam;
        }

        #region ICommPort 成员
        override public string PortDescription
        {
            get
            {
                if (connectedSocket != null && connectedSocket.Connected)
                {
                    return string.Format("{0}:{1}", portParam.PortTypeName, connectedSocket.RemoteEndPoint);
                }

                return base.PortDescription;
            }
        }

        public override PortStatus PortState
        {
            get
            {
                return IsConnected ? PortStatus.Connected : PortStatus.Closed;
            }
        }

        public override bool IsConnected
        {
            get
            {
                return connectedSocket != null && connectedSocket.Connected;
            }
        }

        public override bool CanReopen
        {
            get
            {
                return true;
            }
        }

        public override PortParamBase PortParam
        {
            get
            {
                return portParam;
            }
        }

        static readonly object Lockopen = new object();

        public override bool Open()
        {
            _closeCalled = false;

            if (IsConnected)
            {
                return true;
            }

            if (portParam == null)
            {
                throw new ApplicationException("端口参数没有设置，或参数解析失败");
            }

            try
            {
                lock (Lockopen)
                {
                    if (IsConnected)
                    {
                        return true;
                    }

                    if (connectedSocket != null)
                    {
                        // 不知道要不要
                        CloseSocket(PortClosedReasons.PortIsPhysicallyAbnormal, false);
                    }

                    portParam.ResolveIP();
                    connectedSocket = new Socket(portParam.RemoteIPPort.Address.AddressFamily, SocketType.Stream, ProtocolType.Tcp);

                    if (portParam.LocalIPPort != null && !portParam.LocalIPPort.Address.Equals(PortParamFormatService.IP127001))
                    {
                        IPEndPoint b = new IPEndPoint(portParam.LocalIPPort.Address, portParam.LocalPort);
                        connectedSocket.Bind(b);
                    }

                    waitConnectedEvent = new AutoResetEvent(false);
                    connectedSocket.BeginConnect(portParam.RemoteIPPort, ConnectCallback, connectedSocket);
                    waitConnectedEvent.WaitOne(4000, false);

                    if (connectedSocket.Connected)
                    {
                        base.Open();
                        CallEventConnected();
                        CallEventCommMessageOccured(string.Format("{0}已经连接到：{1}", portParam.PortTypeName, portParam.RemoteIPPort));

                        return IsConnected;
                    }
                    else
                    {
                        CallEventCommMessageOccured(string.Format("{0}尝试连接到：{1}失败！", portParam.PortTypeName, portParam.RemoteIPPort));
                    }

                    connectedSocket.Close();
                    connectedSocket = null;
                }
            }
            catch (Exception ex)
            {
                CallEventCommMessageOccured(string.Format("{0}建立连接时异常：{1}", PortParam.PortTypeName, ex.Message));
                Close();
            }

            return false;
        }

        public override void Close()
        {
            _closeCalled = true;

            if (waitConnectedEvent != null)
            {
                waitConnectedEvent.Close();
                waitConnectedEvent = null;
            }

            CallEventCommMessageOccured("TCPClient已经关闭。");

            CloseSocket(PortClosedReasons.LocalClosedNormally);
        }

        public override bool Send(byte[] buffer, int offset, int size)
        {
            if (IsConnected || _closeCalled || !AutoConnect)
            {
                return base.Send(buffer, offset, size);
            }

            UtMessageBase.ShowOneMessage(string.Format("发送数据前重新连接到：{0}", portParam.RemoteIPPort), PopupMessageType.Info);

            return !Open() && base.Send(buffer, offset, size);
        }
        #endregion

        /// <summary>
        /// 连接服务回调函数
        /// </summary>
        /// <param name="ar"></param>
        public void ConnectCallback(IAsyncResult ar)
        {
            Socket sock = (Socket)ar.AsyncState;    // 本地作为客户端的Socket

            try
            {
                sock.EndConnect(ar);
            }
            catch
            {
                CallEventCommMessageOccured(string.Format("{0}连接到服务端失败", PortDescription));
            }

            if (waitConnectedEvent != null)
            {
                waitConnectedEvent.Set();
            }
        }

        override internal void CloseSocket(PortClosedReasons portClosedReason)
        {
            CloseSocket(portClosedReason, true);
        }

        private void CloseSocket(PortClosedReasons portClosedReason, bool logEvent)
        {
            try
            {
                if (connectedSocket == null)
                {
                    return;
                }

                if (connectedSocket.Connected)
                {
                    connectedSocket.Shutdown(SocketShutdown.Both);
                }

                connectedSocket.Close();
                CallChannelStateChangeEvent(false);

                if (logEvent)
                {
                    CallEventClosed(portClosedReason);
                }

                connectedSocket = null;
                isOpened = false;
            }
            catch (Exception ex)
            {
                if (connectedSocket != null)
                {
                    connectedSocket.Close();
                }

                connectedSocket = null;

                isOpened = false;
               
                if (logEvent)
                {
                    CallEventCommMessageOccured("TCPClient关闭异常：" + ex.Message);
                }
            }
        }

        private ChannelStateChanged onChannelStateChanged;

        /// <summary>
        /// 状态变化
        /// </summary>
        public ChannelStateChanged OnChannelStateChanged
        {
            get
            {
                return onChannelStateChanged;
            }

            set
            {
                onChannelStateChanged = value;
            }
        }

        /// <summary>
        ///调用通道状态改变事件
        /// </summary>
        private void CallChannelStateChangeEvent(bool isConnect)
        {
            if(OnChannelStateChanged!=null)
            {
                OnChannelStateChanged.Invoke(this, ChannelGuid, isConnect);
            }           
        }
    }
}