/***************************************************************************************************
* copyright : 芯迈微半导体（珠海）有限公司
* version   : 1.00
* file      : CommPortTcpServerPackage.cs
* date      : 2023/03/16
* author    : haozhe.ni
* brief     : TCPServer（适应单客户端连接）
* section Modification History
* - 1.0 : Initial version - haozhe.ni
***************************************************************************************************/
using System;

namespace Channel
{
    public delegate void PortStateChangedHandler(PortStatus status);

    public class CommPortTcpServerPackage : CommPortBase
    {
        CommPortTcpServer tcpServer;

        ICommPort socketPort;

        public event PortStateChangedHandler EventPortState;

        void CallEventPortState(PortStatus status)
        {
            if (EventPortState != null)
            {
                EventPortState.Invoke(status);
            }
        }

        public CommPortTcpServerPackage(ICommPort tcpPort)
        {
            tcpServer = tcpPort as CommPortTcpServer;

            if (tcpServer != null)
            {
                tcpServer.OnSocketConnected = SocketConnected;
                tcpServer.OnSocketClosed = SocketDisconnected;
            }
        }

        private void SocketConnected(string channelGuid, ICommPort loadedPort, PortParamBase portParam)
        {
            if (loadedPort != null)
            {
                socketPort = loadedPort;
                socketPort.BytesReceived += SocketPort_BytesReceived;
                socketPort.Open();
                CallEventPortState(PortStatus.Connected);
            }
        }

        private void SocketPort_BytesReceived(object sender, byte[] buffer, int offset, int size)
        {
            CallEventBytesReceived(buffer, offset, size);
        }

        private void SocketDisconnected(CommPortSocket portSocket)
        {
            if (socketPort != null)
            {
                socketPort.BytesReceived -= SocketPort_BytesReceived;
                socketPort.Close();
                socketPort = null;
                CallEventPortState(PortStatus.Opened);
            }
        }

        #region 缓冲区属性及操作

        /// <summary>
        /// 返回缓冲区是否有数据。
        /// </summary>
        override public bool HasData
        {
            get { return socketPort != null && socketPort.HasData; }
        }

        /// <summary>
        /// 查看并返回缓冲区的数据（不改变缓冲区数据）。
        /// 若缓冲区中的数据少于size则返回null。
        /// </summary>
        /// <param name="size">要查看的数据大小</param>
        override public byte[] Peek(int size)
        {
            return socketPort.Peek(size);
        }

        /// <summary>
        /// 查看并返回缓冲区的数据（从缓冲区删除已返回的数据）。
        /// 若缓冲区中的数据少于size则返回null。
        /// </summary>
        /// <param name="size">要查看的数据大小</param>
        override public byte[] Read(int size)
        {
            return socketPort.Read(size);
        }

        override public void RemoveData(int size)
        {
            socketPort.RemoveData(size);
        }

        /// <summary>
        /// 获得缓冲区数据长度。
        /// </summary>
        override public int GetCacheSize()
        {
            return socketPort.GetCacheSize();
        }

        /// <summary>
        /// 清空缓冲区。
        /// </summary>
        override public void ClearBuffer()
        {
            bufferAccessor.Clear();
        }

        #endregion 缓冲区属性及操作

        #region ICommPort 成员

        public override PortStatus PortState
        {
            get { return tcpServer == null ? PortStatus.Closed : tcpServer.PortState; }
        }

        public override bool IsConnected
        {
            get
            {
                { return tcpServer == null ? false : tcpServer.IsConnected; }
            }
        }

        public override PortParamBase PortParam
        {
            get
            {
                return null;
            }
        }

        public override bool Open()
        {
            if (tcpServer != null)
            {
                tcpServer.Open();
            }

            return true;
        }

        public override bool CanReopen
        {
            get
            {
                return tcpServer != null && tcpServer.CanReopen;
            }
        }

        public override void Close()
        {
            try
            {
                bool closed = false;
                if (tcpServer != null)
                {
                    tcpServer.OnSocketConnected = null;
                    tcpServer.Close();
                    tcpServer.OnSocketClosed = null;
                    tcpServer = null;
                    closed = true;
                }

                if (socketPort != null && socketPort.IsConnected)
                {
                    socketPort.Close();
                }

                if (closed)
                {
                    CallEventClosed(PortClosedReasons.LocalClosedNormally);
                }
            }
            catch (Exception ex)
            {
                CallEventCommMessageOccured("关闭端口时异常信息：" + ex.Message);
            }
        }

        public override bool Send(byte[] buffer, int offset, int size)
        {
            if (socketPort == null)
            {
                return false;
            }

            return socketPort.Send(buffer, offset, size);
        }
        #endregion ICommPort 成员
    }
}
