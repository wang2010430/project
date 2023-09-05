/***************************************************************************************************
* copyright : 芯迈微半导体（珠海）有限公司
* version   : 1.00
* file      : CommPortTcpServerV3.cs
* date      : 2023/03/16
* author    : haozhe.ni
* brief     : CommPortTcpServerV3
* section Modification History
* - 1.0 : Initial version - haozhe.ni
***************************************************************************************************/

using System;
using System.Net;
using System.Xml;
using Common;
using System.Threading;
using System.Net.Sockets;
using System.Collections.Generic;
using System.Linq;

namespace Channel
{
    public delegate void WhenSocketConnected(string channelGuid, ICommPort loadedPort, PortParamBase portParam);

    public delegate void ChannelStateChanged(object sender, string channelGuid, bool isConnect);

    /// <summary>
    /// CommPortTCPNETServer  网络通讯服务端。
    /// </summary>
    public class CommPortTcpServer : CommPortBase
    {
        private int connectionCount;
        private Socket listeningSocket;	                 // 客户端侦听套接字
        private readonly TcpServerParam portParam;
        private readonly List<CommPortSocket> connectedSockets = new List<CommPortSocket>();

        public Socket RemoteSocket { get; set; }

        override public string PortDescription
        {
            get
            {
                if (listeningSocket != null && listeningSocket.Connected)
                {
                    return string.Format("TcpServer:{0}", listeningSocket.LocalEndPoint);
                }

                return base.PortDescription;
            }
        }

        public CommPortTcpServer(TcpServerParam portParam)
        {
            RemoteSocket = null;
            needProtocol = false;
            this.portParam = portParam;
        }

        private WhenSocketConnected onSocketConnected;

        /// <summary>
        /// Socket接入
        /// </summary>
        public WhenSocketConnected OnSocketConnected
        {
            set
            {
                onSocketConnected = value;
            }
        }

        private WhenSocketClosed onSocketClosed;

        public WhenSocketClosed OnSocketClosed
        {
            set
            {
                onSocketClosed = value;
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

        public AddressFamily ListeningAddressFamily
        {
            get
            {
                return listeningSocket != null ? listeningSocket.AddressFamily : AddressFamily.InterNetwork;
            }
        }

        /// <summary>
        /// 已经连接的TCP Client 数量 2020.08.26
        /// </summary>
        public int ConnectionCount
        {
            get
            {
                return connectedSockets != null ? connectedSockets.Count : 0;
            }
        }

        private object commonData;

        /// <summary>
        /// 协议公共数据
        /// </summary>
        public object CommonData
        {
            get
            {
                return commonData;
            }

            set
            {
                commonData = value;
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
                return IsConnected ? PortStatus.Connected : PortStatus.Closed;
            }
        }

        /// <summary>
        /// 返回是否与远端建立了有效的连接。
        /// </summary>
        public override bool IsConnected
        {
            get
            {
                return listeningSocket != null && listeningSocket.IsBound;
            }
        }

        public override PortParamBase PortParam
        {
            get
            { 
                return portParam; 
            }
        }

        protected IPEndPoint PortParam_RemoteIPPort
        {
            get
            { 
                return portParam.RemoteIPPort;
            }
        }

        public int ListingPort
        {
            get
            {
                return portParam.LocalIPPort.Port;
            }
        }

        public override bool Open()
        {
            try
            {
                if (!IsConnected)
                {
                    Unloading = false;  // Remoting通道重新加载时会对此值赋值
                    portParam.ResolveIP();
                    
                    if (portParam.LocalIPPort == null || portParam.LocalIPPort.Address.Equals(PortParamFormatService.IP127001) || portParam.LocalIPPort.Address.ToString().Equals("0.0.0.0"))
                    {
                        // 空白、127.0.0.1、0.0.0,0时绑定任意IP
                        AddressFamily f = (portParam.RemoteIPPort == null) ? AddressFamily.InterNetwork : portParam.RemoteIPPort.AddressFamily;
                        listeningSocket = new Socket(f, SocketType.Stream, ProtocolType.Tcp);

                        if (portParam.LocalIPPort != null)
                        {
                            listeningSocket.Bind(new IPEndPoint((f == AddressFamily.InterNetworkV6) ? IPAddress.IPv6Any : IPAddress.Any, portParam.LocalIPPort.Port));
                        }
                    }
                    else
                    {
                        listeningSocket = new Socket(portParam.LocalIPPort.AddressFamily, SocketType.Stream, ProtocolType.Tcp);
                        listeningSocket.Bind(portParam.LocalIPPort);
                    }

                    if (portParam.LocalIPPort != null)
                    {
                        ListeningTCPServerPort.AddToListeningTcps(portParam.LocalIPPort.Port, listeningSocket.AddressFamily);
                    }

                    listeningSocket.Listen(10);
                    listeningSocket.BeginAccept(AcceptCallback, listeningSocket);
                    CallEventCommMessageOccured(string.Format("Tcp Server {0} 已经处于侦听状态。", PortDescription));
                    PortClosed = false;
                    //UtMessageBase.ShowOneMessage("通讯测试", string.Format("监听端口:{0} 对应的规约数量为：{1}", ListingPort, ListeningTCPServerPort.ConfiguredProtocolCount(ListingPort)), PopupMessageType.Info, 0);
                    
                    return true;
                }
            }
            catch (SocketException ex)
            {
                CallEventCommMessageOccured("打开TCPServer端口时异常信息：" + ex.Message);
            }

            return false;
        }

        public override void Close()
        {
            Close(PortClosedReasons.LocalClosedNormally);
        }

        public override bool Send(byte[] data)
        {
            if (data == null || data.Length == 0 || !IsConnected)
            {
                return false;
            }

            return base.Send(data);
        }

        public override bool Send(byte[] buffer, int offset, int size)
        {
            return Send(buffer.Skip(offset).Take(size).ToArray());
        }

        #endregion

        protected void Close(PortClosedReasons portClosedReason)
        {
            if (!PortClosed)
            {
                try
                {
                    PortClosed = true;

                    if (listeningSocket != null)
                    {
                        // 防止在关闭过程中有连接再进入
                        listeningSocket.Close();
                        listeningSocket.Dispose();
                        listeningSocket = null;
                    }

                    CloseClientSocket();
                    CallEventClosed(portClosedReason);
                    CallEventCommMessageOccured("TCPServer已经关闭侦听。");
                }
                catch (Exception ex)    // socket had closed
                {
                    CallEventCommMessageOccured("关闭TCPServer端口处：" + ex.Message);
                }
            }
        }

        protected virtual void RegisterReceiveEvent(Socket remoteClientSocket)
        {
        }

        protected virtual void SetRemoteSocket(Socket newRemoteSocket)
        {
        }

        /// <summary>
        /// 接受连接的回调函数
        /// </summary>
        void AcceptCallback(IAsyncResult ar)
        {
            if (Unloading || PortClosed || listeningSocket == null)
            {
                return;
            }

            try
            {
                Socket newRemoteSocket = null;

                try
                {
                    newRemoteSocket = listeningSocket.EndAccept(ar);
                    SetRemoteSocket(newRemoteSocket);
                    CheckSockes();
                    RegisterReceiveEvent(newRemoteSocket);
                }
                catch
                {//关闭监听时收到的回调
                }

                if (!Unloading && newRemoteSocket != null && newRemoteSocket.Connected)
                {
                    IPEndPoint remoteEndPoint = (IPEndPoint)newRemoteSocket.RemoteEndPoint;

                    bool exactMatch;

                    TcpClientsProtocolInfo endPointProtocol = ListeningTCPServerPort.FindRelatedIPProtocol(ListingPort, remoteEndPoint, out exactMatch);

                    if (PortClosed || PortState == PortStatus.Closed || (endPointProtocol == null && ListeningTCPServerPort.TotalProtocols() != 0) || onSocketConnected == null ||
                        ExcessConnectionLimit(remoteEndPoint))
                    {
                        // 非工作状态或连接超出规定数量并且超出了该监听端口配置的协议数量
                        if (endPointProtocol == null && ListeningTCPServerPort.TotalProtocols() != 0)
                        {
                            CallEventCommMessageOccured(string.Format("没有定义地址{0}的相应协议", remoteEndPoint.Address));
                        }

                        CloseRemoteSocket(newRemoteSocket);
                    }
                    else
                    {
                        connectionCount++;

                        CommPortSocket socketPort = new CommPortSocket(newRemoteSocket)
                        {
                            ChannelGuid = ChannelGuid,
                            SerialNumber = SerialNumber,
                            BeforeSocketClose = onSocketClosed,
                            AfterSocketClose = AfterSocketClosed,
                            AfterPairedPortUniqueIdSet = AfterPairedPortUniqueIdSet
                        };

                        lock (connectedSockets)
                        {
                            connectedSockets.Add(socketPort);
                        }

                        CallEventCommMessageOccured("TCPServer已经接受与客户端“" + newRemoteSocket.RemoteEndPoint.ToString() + "”的连接。");

                        onSocketConnected.BeginInvoke(ChannelGuid, socketPort, PortParam, null, null);

                        CallChannelStateChangeEvent(true);
                    }
                }
            }
            catch (Exception ex)
            {
                CheckSockes();
                CallEventCommMessageOccured("TCPServer接受连接处理过程中异常：" + ex.Message);
            }

            CheckListenerConnectStatus();  // 检查监听socket的连接状态

            if (Unloading || PortClosed || listeningSocket == null)
            {
                return;
            }

            try
            {
                listeningSocket.BeginAccept(AcceptCallback, listeningSocket);//继续侦听
            }
            catch (Exception ex)
            {
                CallEventCommMessageOccured("TCPServer listener.BeginAccept处异常：" + ex.Message);
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

        protected virtual void CheckListenerConnectStatus()
        {
        }

        /// <summary>
        /// 关闭所有Socket连接
        /// </summary>
        protected void CloseClientSocket()
        {
            lock (connectedSockets)
            {
                for (int i = connectedSockets.Count - 1; i >= 0; i--)
                {
                    if (enableDebugMessage)
                    {
                        UtMessageBase.ShowOneMessage("通讯测试", string.Format("关闭接入【{0}】的Socket:{1}", PortDescription, connectedSockets[i].PortDescription), PopupMessageType.Info, 0);
                    }

                    connectedSockets[i].CloseSocket(PortClosedReasons.LocalClosedNormally);
                    Thread.Sleep(0);
                }

                connectedSockets.Clear();
            }
        }

        private void AfterSocketClosed(CommPortSocket skt)
        {
            CheckSockes();

            CallChannelStateChangeEvent(false);
        }

        private void CheckSockes()
        {
            if (Unloading)
            {
                return;
            }

            lock (connectedSockets)
            {
                for (int i = connectedSockets.Count - 1; i >= 0; i--)
                {
                    if (connectedSockets[i] == null || (connectedSockets[i].ConnectedSocket == null || !connectedSockets[i].ConnectedSocket.Connected))
                    {
                        connectedSockets.RemoveAt(i);
                    }
                }
            }
        }

        /// <summary>
        /// 判断接入请求是否超出接入数量限制
        /// </summary>
        /// <param name="remoteEndPointNew"></param>
        /// <returns></returns>
        private bool ExcessConnectionLimit(IPEndPoint remoteEndPointNew)
        {
            if (ListeningTCPServerPort.ConfiguredProtocolCount(ListingPort) > 1)
            {
                // 监听端口对应了多个规约（或者是一个规约多个允许接入的地址）
                lock (connectedSockets)
                {
                    foreach (CommPortSocket skt in connectedSockets)
                    {
                        IPAddress addrOld = ((IPEndPoint)skt.ConnectedSocket.RemoteEndPoint).Address;

                        if (remoteEndPointNew.Address.Equals(addrOld))
                        {
                            UtMessageBase.ShowOneMessage("通讯测试", string.Format("监听端口:{0} 已经存在来自地址：{1} 的链接，本次接入请求将被忽略。", ListingPort, addrOld), PopupMessageType.Info, 0);
                           
                            return true;
                        }
                    }
                }

                return false;
            }

            if (connectedSockets.Count >= portParam.AllowedConnections)
            {
                CallEventCommMessageOccured(string.Format("Socket连接数已经满：{0} 个，{1}的连接请求将被忽略。", connectedSockets.Count, remoteEndPointNew.ToString()));

                return true;
            }

            return false;
        }

        /// <summary>
        /// 关闭接入的socket链接
        /// </summary>
        /// <param name="socketToClose"></param>
        private void CloseRemoteSocket(Socket socketToClose)
        {
            if (socketToClose != null)
            {
                CallEventCommMessageOccured(string.Format("关闭Socket连接：{0}", socketToClose.RemoteEndPoint.ToString()));
                socketToClose.Shutdown(SocketShutdown.Both);
                socketToClose.Close();
            }
        }
    }

    public class SocketInfo
    {
        public Socket Socket;

        public int ChannelNo;

        public string ChannelName;
    }

    #region 辅助类
    /// <summary>
    /// TCP客户端规约协议信息
    /// </summary>
    public class TcpClientsProtocolInfo
    {
        public TcpClientsProtocolInfo(IPEndPoint c, XmlElement configElement, string[] funcs)
        {
            ConnetingIP = c;
            ChannelConfigElement = configElement;
            ProtocolFunctions = funcs;
        }

        public IPEndPoint ConnetingIP { get; set; }

        /// <summary>
        /// 通道配置参数（创建Socket通道时使用）
        /// </summary>
        public XmlElement ChannelConfigElement { get; set; }

        /// <summary>
        /// 协议应具备的功能（创建Socket通道时使用）
        /// </summary>
        public string[] ProtocolFunctions { get; set; }
    }

    /// <summary>
    /// 某监听TCP Server端口及其允许接入的IP及相关协议
    /// </summary>
    public class ListeningTCPServerPort
    {
        /// <summary>
        /// TCPServer监听端口
        /// </summary>
        private readonly int _listeningPort;

        private readonly AddressFamily _listeningAddressType;

        /// <summary>
        /// TCP Server监听地址类型
        /// </summary>
        public AddressFamily ListeningAddressType
        {
            get
            {
                return _listeningAddressType;
            }
        }

        /// <summary>
        /// 可以接入监听端口的IP及其协议
        /// </summary>
        private readonly List<TcpClientsProtocolInfo> _clientProtocols = new List<TcpClientsProtocolInfo>();

        public ListeningTCPServerPort(int port, AddressFamily listeningAddressFamily)
        {
            _listeningPort = port;
            _listeningAddressType = listeningAddressFamily;
        }

        /// <summary>
        /// 增加一可以接入的IP及协议
        /// </summary>
        /// <param name="tcp"></param>
        public void Add(TcpClientsProtocolInfo tcp)
        {
            foreach (TcpClientsProtocolInfo t in _clientProtocols)
            {
                if ((t.ConnetingIP == null && tcp.ConnetingIP == null) ||
                    (t.ConnetingIP != null && tcp.ConnetingIP != null && PortParamFormatService.IsIPBytesMatch(t.ConnetingIP.Address, tcp.ConnetingIP.Address)))
                {
                    return;
                }
            }

            _clientProtocols.Add(tcp);
        }

        /// <summary>
        /// 根据端口号及客户端IP查找对应的协议定义
        /// </summary>
        /// <param name="port"></param>
        /// <param name="connectingIP"></param>
        /// <param name="exactMatch">是否是精确匹配，如果不是精确匹配是指127.0.0.1与任何一个本地IP比较都为真</param>
        /// <returns></returns>
        public TcpClientsProtocolInfo FindProtocol(int port, IPEndPoint connectingIP, ref bool exactMatch)
        {
            if (_listeningPort != port)
            {
                return null;
            }

            TcpClientsProtocolInfo noip = null; // 没有限定客户端IP的协议

            foreach (TcpClientsProtocolInfo t in _clientProtocols)
            {
                if (t.ConnetingIP == null)
                {
                    noip = t;
                    exactMatch = false;
                }
                else
                {
                    if (PortParamFormatService.IsIPBytesMatch(t.ConnetingIP.Address, connectingIP.Address))
                    {
                        return t;
                    }

                    if (!exactMatch && IpAddressMatch(t.ConnetingIP.Address, connectingIP.Address))
                    {
                        // 如果不是精确匹配是指127.0.0.1与任何一个本地IP比较都为真
                        return t;
                    }
                }
            }

            return noip;
        }

        static private readonly IPHostEntry LocalIpAddress = Dns.GetHostEntry(Environment.MachineName);

        /// <summary>
        /// 判断IP是否是本地IP
        /// </summary>
        /// <param name="adr"></param>
        /// <returns></returns>
        static private bool IsLocalIpAddress(IPAddress adr)
        {
            if (adr == null)
            {
                return false;
            }

            foreach (IPAddress ip in LocalIpAddress.AddressList)
            {
                if (PortParamFormatService.IsIPBytesMatch(ip, adr))
                {
                    return true;
                }
            }

            return false;
        }

        /// <summary>
        /// 判断两个IP地址是否相同，判断时增加对本地地址的识别，同是本地地址是认为是相同的。
        /// </summary>
        /// <param name="adr1"></param>
        /// <param name="adr2"></param>
        /// <returns></returns>
        private static bool IpAddressMatch(IPAddress adr1, IPAddress adr2)
        {
            if (adr1 == null || adr2 == null)
            {
                return false;
            }

            return ((adr1.Equals(PortParamFormatService.IP127001) || adr1.Equals(PortParamFormatService.IPV6127001)) && IsLocalIpAddress(adr2)) ||
                   ((adr2.Equals(PortParamFormatService.IP127001) || adr2.Equals(PortParamFormatService.IPV6127001)) && IsLocalIpAddress(adr1)) || PortParamFormatService.IsIPBytesMatch(adr1, adr2);
        }

        /// <summary>
        /// TCP Sever 所打开的所有端口列表
        /// </summary>
        static public List<ListeningTCPServerPort> Listeningtcps = new List<ListeningTCPServerPort>();

        /// <summary>
        /// 记录一TCP监听端口
        /// </summary>
        /// <param name="tcpPort"></param>
        /// <param name="listeningAddressFamily"></param>
        /// <returns></returns>
        static public ListeningTCPServerPort AddToListeningTcps(int tcpPort, AddressFamily listeningAddressFamily)
        {
            lock (Listeningtcps)
            {
                foreach (ListeningTCPServerPort u in Listeningtcps)
                {
                    if (tcpPort == u._listeningPort)
                    {
                        return u;
                    }
                }

                ListeningTCPServerPort c = new ListeningTCPServerPort(tcpPort, listeningAddressFamily);

                Listeningtcps.Add(c);

                return c;
            }
        }

        /// <summary>
        /// 判断TCP监听端口是否已经存在
        /// </summary>
        /// <param name="tcpPort"></param>
        /// <returns></returns>
        static public ListeningTCPServerPort ExistListeningTcpServer(int tcpPort)
        {
            lock (Listeningtcps)
            {
                if (Listeningtcps.Count <= 0)
                {
                    return null;
                }

                foreach (ListeningTCPServerPort u in Listeningtcps)
                {
                    if (tcpPort == u._listeningPort)
                    {
                        return u;
                    }
                }

                return null;
            }
        }

        /// <summary>
        /// 同一监听端口配置的协议数量
        /// </summary>
        /// <param name="tcpPort"></param>
        /// <returns></returns>
        static internal int ConfiguredProtocolCount(int tcpPort)
        {
            lock (Listeningtcps)
            {
                if (Listeningtcps.Count <= 0)
                {
                    return 0;
                }

                foreach (ListeningTCPServerPort u in Listeningtcps)
                {
                    if (tcpPort == u._listeningPort)
                    {
                        return u._clientProtocols.Count;
                    }
                }

                return 0;
            }
        }

        /// <summary>
        /// 删除正在监听的端口列表，仅用于全部规约重新加载时
        /// </summary>
        static public void ClearListeningTcpServer()
        {
            lock (Listeningtcps)
            {
                Listeningtcps.Clear();
            }
        }

        /// <summary>
        /// 移除当某端口仅有一个协议配置时对本机地址的绑定（客户端），以防止老配置数据造成无法接入
        /// </summary>
        static public void CheckTcpClientBinding()
        {
            lock (Listeningtcps)
            {
                foreach (ListeningTCPServerPort t in Listeningtcps)
                {
                    if (t._clientProtocols.Count == 1 && t._clientProtocols[0].ConnetingIP != null && t._clientProtocols[0].ConnetingIP.Address.Equals(PortParamFormatService.IP127001))
                    {
                        t._clientProtocols[0].ConnetingIP = null;
                    }
                }
            }
        }

        /// <summary>
        /// 查找某接入的ＩＰ应使用的协议
        /// </summary>
        /// <param name="port"></param>
        /// <param name="remoteIp"></param>
        /// <returns></returns>
        static internal TcpClientsProtocolInfo FindRelatedIPProtocol(int port, IPEndPoint remoteIp, out bool exactMatch)
        {
            exactMatch = true;
            TcpClientsProtocolInfo c = null;

            if (remoteIp != null)
            {
                c = FindIpProtocol(port, remoteIp, ref exactMatch);

                if (c == null && (remoteIp.Address.Equals(PortParamFormatService.IP127001) || IsLocalIpAddress(remoteIp.Address)) || remoteIp.Address.Equals(PortParamFormatService.IPV6127001))
                {
                    exactMatch = false;
                    c = FindIpProtocol(port, remoteIp, ref exactMatch);
                }
            }

            return c;
        }

        /// <summary>
        /// 根据监听端口与接入地址查找一匹配的协议
        /// </summary>
        /// <param name="port"></param>
        /// <param name="remoteIp"></param>
        /// <param name="exactMatch">是否是精确匹配，如果不是精确匹配是指127.0.0.1与任何一个本地IP比较都为真</param>
        /// <returns></returns>
        static private TcpClientsProtocolInfo FindIpProtocol(int port, IPEndPoint remoteIp, ref bool exactMatch)
        {
            lock (Listeningtcps)
            {
                foreach (ListeningTCPServerPort ltcp in Listeningtcps)
                {
                    TcpClientsProtocolInfo c = ltcp.FindProtocol(port, remoteIp, ref exactMatch);

                    if (c != null)
                    {
                        return c;
                    }
                }
            }

            return null;
        }

        /// <summary>
        /// 本端口使用的所有协议
        /// 总协议数为零时，是通讯测试工具在使用
        /// </summary>
        /// <returns></returns>
        static public int TotalProtocols()
        {
            lock (Listeningtcps)
            {
                int t = 0;

                foreach (ListeningTCPServerPort udp in Listeningtcps)
                {                   
                    if (udp._clientProtocols.Count == 0)
                    {
                        return 0;
                    }

                    t += udp._clientProtocols.Count;
                }

                return t;
            }
        }
    }
    #endregion
}
