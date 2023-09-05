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

using System;
using System.Net;
using System.Net.Sockets;

namespace Channel
{
    public abstract class SocketPortBase : CommPortBase
    {
        protected bool isOpened = false;
        protected Socket connectedSocket = null;				// 接受连接的远端套接字

        public Socket ConnectedSocket
        {
            get
            {
                return connectedSocket;
            }
         }

        public override bool CanReopen
        {
            get
            {
                return false;
            }
        }

        public SocketPortBase(Socket skt)
        {
            connectedSocket = skt;
            ReadBuffLimit = 1024 * 1024 * 8;
            sendBuffLimit = 0;
            isOpened = false;
        }

        override public string PortDescription
        {
            get
            {
                try
                {
                    if (connectedSocket != null && connectedSocket.Connected)
                    {
                        IPEndPoint loc = (IPEndPoint)connectedSocket.LocalEndPoint;
                        IPEndPoint rmt = (IPEndPoint)connectedSocket.RemoteEndPoint;
                        return string.Format("接入IP:{0},监听端口:{1},远方端口:{2}", rmt.Address, loc.Port, rmt.Port);
                    }
                }
                catch
                { 
                }

                return base.PortDescription;
            }
        }

        public override bool Open()
        {
            if (connectedSocket == null || !connectedSocket.Connected)
            {
                return false;
            }

            Address = connectedSocket.RemoteEndPoint.ToString();
            
            if (isOpened)
            {
                return true;
            }

            isOpened = true;

            TcpSocketKeepAlive.KeepAlive(connectedSocket, (uint)1, (uint)bKeepAlive, (uint)bKeepAliveInterl);

            try
            {
                connectedSocket.BeginReceive(portBuffer, 0, portBuffer.Length, 0, new AsyncCallback(ReceiveCallback), connectedSocket);
            }
            catch (Exception ex)
            {
                CallEventCommMessageOccured(string.Format("Open() BeginReceive异常：{0}", ex.Message));
            }
            
            return true;
        }

        public override void Close()
        {
            if (connectedSocket == null || !connectedSocket.Connected)
            {
                return;
            }

            isOpened = false;
            connectedSocket.Shutdown(SocketShutdown.Both);
            connectedSocket.Close();
            connectedSocket = null;
        }

        public override bool Send(byte[] buffer, int offset, int sendsize)
        {
            if (ReadWriteMode == PortReadWriteMode.ReadOnly ||//仅写端口，不做读取处理
                connectedSocket == null || buffer == null || buffer.Length == 0 || buffer.Length < offset + sendsize || !IsConnected)
            {
                return false;
            }

            int num = sendsize;

            while (num > 0)
            {
                if (bLimitedTraffic)
                {
                    sendsize = Math.Min(num, sendbyte);
                    SocketAsyncSend(connectedSocket, buffer, offset, sendsize);
                }
                else
                {
                    int leftdata = sendsize;
                    int startAt = offset;
                    int frameCount = 0;
                    int packetLimit = 512 * 1024;

                    while (leftdata > 0)
                    {
                        if (leftdata <= packetLimit)
                        {
                            SocketAsyncSend(connectedSocket, buffer, startAt, leftdata);
                            leftdata = 0;
                        }
                        else
                        {
                            SocketAsyncSend(connectedSocket, buffer, startAt, packetLimit);
                            startAt += packetLimit;
                            leftdata -= packetLimit;
                        }

                        frameCount++;
                    }
                }

                num -= sendsize;
                offset += sendsize;
            }

            return true;
        }

        internal int SocketAsyncSend(Socket s, byte[] buff, int offset, int size)
        {
            // 增加try catch 捕捉异常
            try
            {
                IAsyncResult ar = s.BeginSend(buff, offset, size, SocketFlags.None, null, null);
                CallEventBytesSent(buff, offset, size);

                if (ar != null)
                {
                    ar.AsyncWaitHandle.WaitOne();
                }
            }
            catch (Exception ex)
            {
                CallEventCommMessageOccured(string.Format("SocketAsyncSend BeginSend异常：{0}", ex.Message));
            }

            return size;
        }

        virtual internal void CloseSocket(PortClosedReasons reason)
        {
            try
            {
                if (connectedSocket == null || !connectedSocket.Connected)
                {
                    return;
                }

                connectedSocket.Shutdown(SocketShutdown.Both);
                connectedSocket.Close();
                connectedSocket = null;
                isOpened = false;
            }
            catch (Exception ex)
            {
                CallEventCommMessageOccured(string.Format("{0}关闭错误：{1}", PortParam.PortTypeName, ex.Message));
                connectedSocket = null;
                isOpened = false; // 解决关闭错误时，重新链接不能收到数据
            }
        }

        void ReceiveCallback(IAsyncResult ar)
        {
            bool continueRead = true;
            PortClosedReasons closeReason = PortClosedReasons.RemoteClosed;
            Socket sock = (Socket)ar.AsyncState;    // sock即远程Socket
           
            try
            {
                if (sock.Connected)
                {
                    int receivedSize = sock.EndReceive(ar);

                    if (receivedSize > 0)
                    {
                        bufferAccessor.Write(portBuffer, 0, receivedSize);
                        CallEventBytesReceived(portBuffer, 0, receivedSize);
                    }

                    if (receivedSize == 0)
                    {
                        continueRead = TcpSocketKeepAlive.IsSocketConnected(sock, EnableDebugMessage, PortDescription);
                    }
                }
                else
                {
                    continueRead = false;
                }
            }
            catch (Exception ex)
            {
                continueRead = false;
                closeReason = PortClosedReasons.PortIsPhysicallyAbnormal;
                CallEventCommMessageOccured(string.Format("{0}接收数据错误：{1}主动关闭连接。", PortParam.PortTypeName, ex.Message));
            }

            if (sock.Connected && continueRead)
            {
                TcpSocketKeepAlive.KeepAlive(sock, 1, bKeepAlive, bKeepAliveInterl);

                //增加try catch 捕捉异常并把相应连接关掉
                try
                {
                    sock.BeginReceive(portBuffer, 0, portBuffer.Length, 0, ReceiveCallback, sock);
                }
                catch (Exception ex)
                {
                    CallEventCommMessageOccured(string.Format("ReceiveCallback BeginReceive异常：{0}", ex.Message));
                    continueRead = false;
                    closeReason = PortClosedReasons.PortIsPhysicallyAbnormal;
                    CloseSocket(closeReason);
                }

                System.Threading.Thread.Sleep(0);
            }
            else
            {
                CloseSocket(closeReason);
            }
        }
    }
}