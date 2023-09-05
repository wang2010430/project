/***************************************************************************************************
* copyright : 芯迈微半导体（珠海）有限公司
* version   : 1.00
* file      : CommPortCom.cs
* date      : 2023/03/16
* author    : haozhe.ni
* brief     : CommPortCom
* section Modification History
* - 1.0 : Initial version - haozhe.ni
***************************************************************************************************/

using Common;
using System;
using System.IO.Ports;

namespace Channel.CommPort
{
    /// <summary>
    /// 串口类
    /// </summary>
    public class CommPortCom : CommPortBase
    {
        ComParam portParam;
        protected SerialPort serialPort;
        public CommPortCom(ComParam portParam)
        {
            ReadBuffLimit = 1024 * 1024 * 10;
            SendBuffLimit = 1024 * 1024 * 10;
            serialPort = new SerialPort();
            this.portParam = portParam;
            serialPort.PortName = portParam.PortName;
            serialPort.BaudRate = portParam.BaudRate;
            serialPort.DataBits = portParam.DataBits;
            serialPort.Parity = portParam.Parity;
            serialPort.StopBits = portParam.StopBits;
            serialPort.ReadBufferSize = ReadBuffLimit;
            serialPort.WriteBufferSize = SendBuffLimit;
            serialPort.DataReceived += SerialPortDataReceived;
        }

        private void SerialPortDataReceived(object sender, SerialDataReceivedEventArgs e)
        {
            // 当数据接收事件触发时，处理接收到的数据
            SerialPort sp = (SerialPort)sender;
            int bytesToRead = sp.BytesToRead;
            byte[] buffer = new byte[bytesToRead];
            sp.Read(buffer, 0, bytesToRead);

            bufferAccessor.Write(buffer, 0, bytesToRead);
            base.CallEventBytesReceived(buffer, 0, bytesToRead);
        }

        public override bool CanReopen
        {
            get
            {
                return true;
            }
        }

        public override bool Open()
        {
            try
            {
                if (!IsConnected)
                {
                    UtMessageBase.ShowOneMessage(string.Format("尝试打开串口{0}", serialPort.PortName), PopupMessageType.Info);

                    bool isSysPort = false;
                    string[] serialPortList = SerialPort.GetPortNames();

                    foreach (string port in serialPortList)
                    {
                        if (string.Equals(port, serialPort.PortName, StringComparison.OrdinalIgnoreCase))
                        {
                            isSysPort = true;

                            try
                            {
                                serialPort.Open();
                                break;
                            }
                            catch (UnauthorizedAccessException)
                            {
                                UtMessageBase.ShowOneMessage(string.Format("尝试打开串口{0}时，对端口“{0}”的访问被拒绝", serialPort.PortName), PopupMessageType.Info);
                            }
                            catch (Exception ex)
                            {
                                UtMessageBase.ShowOneMessage(string.Format("尝试打开串口{0}时，发生异常{1}", serialPort.PortName, ex), PopupMessageType.Exception);
                            }
                        }
                    }

                    if (!isSysPort)
                    {
                        UtMessageBase.ShowOneMessage(string.Format("非法串口{0}，串口打开失败", serialPort.PortName), PopupMessageType.Info);
                    }

                    if (serialPort.IsOpen)
                    {
                        CallEventConnected();
                        CallEventCommMessageOccured("串口 " + serialPort.PortName + " 已经打开。");
                        //ReadComPortData();
                    }
                }

            }
            catch (Exception ex)
            {
                CallEventCommMessageOccured(string.Format("打开串口异常信息：{0}", ex.ToString()));
                
                return false;
            }

            return IsConnected;
        }

        /// <summary>
        /// CDMA短信通讯需要这样设置
        /// </summary>
        public void SetMessageParam()
        {
            serialPort.Handshake = Handshake.RequestToSend;
            serialPort.ReadTimeout = 200;
            serialPort.WriteTimeout = 200;
        }

        private void ReadComPortData()
        {
            if (ReadWriteMode == PortReadWriteMode.WriteOnly)
            {
                // 仅写端口，不做读取处理
                return;
            }

            VoidParameterRoutine kickoffRead = null;

            kickoffRead = delegate
            {
                try
                {
                    if (serialPort != null && serialPort.IsOpen)
                    {
                        serialPort.BaseStream.BeginRead(portBuffer, 0, portBuffer.Length, delegate (IAsyncResult ar)
                        {

                            byte[] buffer = (byte[])ar.AsyncState;
                            try
                            {
                                int actualLength = serialPort.BaseStream.EndRead(ar);
                                bufferAccessor.Write(portBuffer, 0, actualLength);
                                base.CallEventBytesReceived(portBuffer, 0, actualLength);
                            }
                            catch (Exception)
                            {

                            }

                            if (IsConnected)
                            {
                                System.Threading.Thread.Sleep(1); //这里要适当延迟，否则就会出现仅读取几个字节处理一次。
                                kickoffRead();
                            }
                        }, null);
                    }
                }
                catch (UnauthorizedAccessException ex)
                {
                    //MOXA的usb转串口线拔掉之后会出现异常，serialPort对象仍然有效，但它不能访问实际端口
                    //这里通过捕获异常的方式返回结果
                    UtMessageBase.ShowOneMessage(ex.ToString(), PopupMessageType.Exception);
                }
                catch
                {
                }
            };

            if (serialPort != null && serialPort.IsOpen)
            {
                kickoffRead();
            }
        }

        #region ICommPort 成员

        public override PortStatus PortState
        {
            get { return IsConnected ? PortStatus.Connected : PortStatus.Closed; }
        }

        public override bool IsConnected
        {
            get { return serialPort.IsOpen; }
        }

        public override bool DisposedAbnormally
        {
            get
            {
                try
                {
                    // 如果是可插拔串口，可能会在串口拔掉后serialPort对象仍然有效，但它不能访问实际端口。
                    // 这里通过捕获异常的方式返回结果。
                    int tmp;

                    if (IsConnected)
                    {
                        tmp = serialPort.BytesToRead;
                    }
                }
                catch (UnauthorizedAccessException ex)
                {
                    UtMessageBase.ShowOneMessage(ex.ToString(), PopupMessageType.Exception);
                    return true;
                }
                catch { }
                return false;
            }
        }

        public override PortParamBase PortParam
        {
            get { return portParam; }
        }

        public override void Close()
        {
            try
            {
                if (IsConnected)
                {
                    serialPort.Close();
                    base.CallEventClosed(PortClosedReasons.LocalClosedNormally);
                    base.CallEventCommMessageOccured("串口 " + serialPort.PortName + " 已经关闭。");
                }
            }
            catch (Exception ex)  //socket had closed
            {
                base.CallEventCommMessageOccured("关闭串口时异常信息：" + ex.Message);
            }
        }

        public override bool Send(byte[] data)
        {
            if (data != null)
            {
                return Send(data, 0, data.Length);
            }

            return false;
        }

        public override bool Send(byte[] buffer, int offset, int size)
        {
            if (!IsConnected)
            {
                return false;
            }

            try
            {
                if (FrameHead != null)
                {
                    if (CommonHelper.OperationSystem == OsFamily.Uniux)
                    {
                        // linux下BeginRead和BeginWrite不能同时使用
                        serialPort.Write(buffer, offset + FrameHead.Length, size - FrameHead.Length);
                    }
                    else
                    {
                        SerialPortAsyncSend(buffer, offset + FrameHead.Length, size - FrameHead.Length);
                    }
                }
                else
                {
                    if (CommonHelper.OperationSystem == OsFamily.Uniux)
                    {
                        serialPort.Write(buffer, offset, size);
                    }
                    else
                    {
                        SerialPortAsyncSend(buffer, offset, size);
                    }
                }

                CallEventBytesSent(buffer, offset, size);
            }
            catch (Exception ex)
            {
                CallEventCommMessageOccured("向串口发送数据时发生异常：" + ex.Message);
                return false;
            }

            return true;
        }

        private int SerialPortAsyncSend(byte[] buff, int offset, int size)
        {
            IAsyncResult ar = serialPort.BaseStream.BeginWrite(buff, offset, size, null, null);
            ar.AsyncWaitHandle.WaitOne();
            
            return size;
        }
        #endregion
    }
}
