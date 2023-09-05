/***************************************************************************************************
* copyright : 芯迈微半导体（珠海）有限公司
* version   : 1.00
* file      : CommPortBase.cs
* date      : 2023/03/16
* author    : haozhe.ni
* brief     : CommPortBase
* section Modification History
* - 1.0 : Initial version - haozhe.ni
***************************************************************************************************/

using Common;
using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace Channel
{
    /// <summary>
    /// 端口实例基类。各种具体端口实现类从它继承
    /// </summary>
    public abstract partial class CommPortBase : ICommPort
    {
        public int SerialNumber { get; set; }

        public string ChannelGuid { get; set; }

        static protected bool unloading = false;
       
        /// <summary>
        /// 是否正在卸载通讯通道
        /// </summary>
        static public bool Unloading
        {
            get
            {
                return unloading;
            }

            set
            {
                unloading = value;
            }
        }

        virtual public string PortDescription
        {
            get
            {
                return PortParam.PortDescription;
            }
        }

        /// <summary>
        /// 端口是否已经被关闭
        /// </summary>
        protected bool PortClosed = false;

        protected byte[] portBuffer = null;                     // 读数据缓冲区
        protected PortBufferAccessor bufferAccessor = null;     // 接受数据缓冲区对象

        protected bool needProtocol = true;

        /// <summary>
        /// 该端口是否需要协议协同工作
        /// </summary>
        public bool NeedProtocol
        {
            get
            {
                return needProtocol;
            }

            set
            {
                needProtocol = value;
            }
        }

        #region 流量限制用
        protected bool bLimitedTraffic = false;
        protected int sendtime = 0;
        protected int sendbyte = 0x10000;  
        #endregion 流量限制用

        protected UInt32 bKeepAliveInterl = (Debugger.IsAttached) ? 30000 : (UInt32)3000;
        protected UInt32 bKeepAlive = (Debugger.IsAttached) ? 30000 : (UInt32)5000;
        public bool bTransport = false;
        protected string portUniqueId = "";

        virtual public string PairedPortUniqueId
        {
            get
            {
                return portUniqueId;
            }

            set
            {
                portUniqueId = value;

                if(afterPairedPortUniqueIdSet!=null)
                {
                    afterPairedPortUniqueIdSet(this, value);
                }
            }
        }

        protected int networkTrafficBytes = 0;

        /// <summary>
        /// 网络流量单位KB/S,如果是0就不限速
        /// </summary>
        public int NetworkTraffic
        {
            set
            {
                networkTrafficBytes = value;

                if(networkTrafficBytes<0)
                {
                    networkTrafficBytes = 0;
                }

                if (networkTrafficBytes > 0 && networkTrafficBytes < 50)
                {
                    networkTrafficBytes = 50;
                }

                if (networkTrafficBytes > 0)
                {
                    bLimitedTraffic = true;

                    if (networkTrafficBytes >= 150)
                    {
                        sendbyte = 1024;
                    }
                    else
                    {
                        sendbyte = 500;
                    }

                    sendtime = sendbyte * 1000 / (networkTrafficBytes * 1024);
                }
                else
                {
                    bLimitedTraffic = false;
                }
            }
        }

        protected int readBuffLimit = 1024*1024;

        /// <summary>
        /// 端口接收缓冲区限制字节数
        /// </summary>
        public int ReadBuffLimit
        {
            get
            {
                return readBuffLimit;
            }

            set
            {
                readBuffLimit = value;

                if (readBuffLimit > 0)
                {
                    portBuffer = new byte[readBuffLimit];
                }
            }
        }

        protected int sendBuffLimit = 0;

        /// <summary>
        /// 端口发送缓冲区限制字节数
        /// </summary>
        public int SendBuffLimit
        {
            get
            {
                return sendBuffLimit;
            }

            set
            {
                sendBuffLimit = value;
            }
        }

        public UInt32 KeepAliveTime
        {
            set
            {
                bKeepAlive = value;
                bKeepAliveInterl = value;
            }
        }

        public PortReadWriteMode ReadWriteMode
        {
            get;
            set;
        }

        protected string address;

        /// <summary>
        /// 远端IP地址
        /// </summary>
        public virtual string Address
        {
            get
            {
                return address;
            }

            set
            {
                address = value;
            }
        }

        protected UInt64 totalBytesSend;
        protected UInt64 totalBytesReceived;

        protected CommPortBase()
        {
            Tag = null;
            FrameHead = null;
            ReadWriteMode = PortReadWriteMode.ReadWrite;
            portBuffer = new byte[readBuffLimit];
            bufferAccessor = new PortBufferAccessor();
            totalBytesSend = 0;
            totalBytesReceived = 0;
        }

        /// <summary>
        /// 是否自己加上帧头,为方便应用层应用
        /// </summary>
        public byte[] FrameHead { get; set; }

        protected bool enableDebugMessage = Debugger.IsAttached;

        /// <summary>
        ///获取或设置在调试模式下是否显示调试文本消息。
        /// 它的结果不影响发布版本的性能。
        /// </summary>
        virtual public bool EnableDebugMessage
        {
            get
            {
                return enableDebugMessage;
            }

            set
            {
                enableDebugMessage = value;
            }
        }

        /// <summary>
        /// 端口支持的操作系统
        /// </summary>
        public virtual List<OsFamily> SupportedOperationSystem
        {
            get
            {
                return new List<OsFamily>(new[] { OsFamily.Windows, OsFamily.Uniux });
            }
        }

        #region ICommPort对应的事件实现

        #region 事件声明


        event PortOpenedEventHandler portOpened;

        /// <summary>
        /// 端口打开成功时产生此事件。
        /// </summary>
        event FlagEventHandler connected;

        /// <summary>
        /// 端口关闭成功时产生此事件。
        /// </summary>
        event PortClosedEventHandler closed;

        /// <summary>
        /// 数据即将成功发送时产生此事件。
        /// 注：此事件并不是数据已经成功发送时产生，因为在多线程的处理中，有时接收数据太快以至于先执行收到数据的事件，然后才切换线程执行发送成功事件，这样实际颠倒了收发顺序。
        /// </summary>
        event BytesTransferEventHandler bytesSent;

        /// <summary>
        /// 端口缓冲区收到一个数据包时产生此事件。
        /// </summary>
        event BytesTransferEventHandler bytesReceived;

        event BytesTransferEventHandler tranbytesReceived;
        #endregion

        #region ICommPort事件设置

        public event PortOpenedEventHandler OnOpened
        {
            add { portOpened += value; }
            remove { portOpened -= value; }
        }

        public event FlagEventHandler Connected
        {
            add { connected += value; }
            remove { connected -= value; }
        }

        public event PortClosedEventHandler Closed
        {
            add { closed += value; }
            remove { closed -= value; }
        }

        virtual public event BytesTransferEventHandler BytesSent
        {
            add { bytesSent += value; }
            remove { bytesSent -= value; }
        }

        virtual public event BytesTransferEventHandler BytesReceived
        {
            add { bytesReceived += value; }
            remove { bytesReceived -= value; }
        }

        virtual public event BytesTransferEventHandler TranbytesReceived
        {
            add { tranbytesReceived += value; }
            remove { tranbytesReceived -= value; }
        }

        private MessageOccuredEventHandler afterPairedPortUniqueIdSet;

        public MessageOccuredEventHandler AfterPairedPortUniqueIdSet
        {
            get
            {
                return afterPairedPortUniqueIdSet;
            }

            set
            {
                afterPairedPortUniqueIdSet=value;
            }
        }

        #endregion

        #region 事件调用

        protected void CallEventOnOpened(PortOpenReasons openReason)
        {
            try
            {
                if (portOpened != null)
                {
                    portOpened(this, openReason);
                }
            }
            catch (Exception ex)
            {
                UtMessageBase.ShowOneMessage(string.Format("关闭通讯端口的响应事件执行异常：{0}{1}", Channel.EventHandleInvalidError, ex), PopupMessageType.Exception);
            }
        }

        /// <summary>触发连接事件。
        /// </summary>
        protected void CallEventConnected()
        {
            try
            {
                if (connected != null)
                {
                    connected(this);
                }
            }
            catch (Exception ex)
            {
                UtMessageBase.ShowOneMessage(string.Format("成功建立连接的响应事件执行异常：{0}{1}", Channel.EventHandleInvalidError, ex), PopupMessageType.Exception);
            }
        }

        /// <summary>触发关闭事件。
        /// </summary>
        /// <param name="portClosedReason"></param>
        protected void CallEventClosed(PortClosedReasons portClosedReason)
        {
            try
            {
                if (closed != null)
                {
                    closed(this, portClosedReason);
                }
            }
            catch (Exception ex)
            {
                UtMessageBase.ShowOneMessage(string.Format("关闭通讯端口的响应事件执行异常：{0}{1}", Channel.EventHandleInvalidError, ex), PopupMessageType.Exception);
            }
        }

        /// <summary>触发数据已经发送的事件。
        /// </summary>
        protected void CallEventBytesSent(byte[] buffer, int offset, int size)
        {
            try
            {
                if (bytesSent != null)
                {
                    bytesSent(this, buffer, offset, size);
                }
            }
            catch (Exception ex)
            {
                UtMessageBase.ShowOneMessage(string.Format("{0}发送数据的响应事件执行异常：{1}{2}", PortParam.PortTypeName, Channel.EventHandleInvalidError, ex), PopupMessageType.Exception);
            }
        }

        /// <summary>触发已经接收到数据的事件。
        /// </summary>
        protected void CallEventBytesReceived(byte[] buffer, int offset, int size)
        {
            try
            {
                if (bytesReceived != null)
                {
                    bytesReceived(this, buffer, offset, size);
                }
                else
                {
                    UtMessageBase.ShowOneMessage(string.Format("接收到{0}字节数--无相应的处理事件", size), PopupMessageType.Info);
                }
            }
            catch (Exception ex)
            {
                UtMessageBase.ShowOneMessage(string.Format("{0}接收到数据的响应事件执行异常：{1}{2}", PortParam.PortTypeName, Channel.EventHandleInvalidError, ex), PopupMessageType.Exception);
            }
        }

        protected void CallEventTranBytesReceived(byte[] buffer, int offset, int size)
        {
            try
            {
                if (tranbytesReceived != null)
                {
                    tranbytesReceived(this, buffer, offset, size);
                }
            }
            catch (Exception ex)
            {
                UtMessageBase.ShowOneMessage(string.Format("通讯端口接收到数据的响应事件执行异常：{0}{1}", Channel.EventHandleInvalidError, ex), PopupMessageType.Exception);
            }
        }

        static string GetByteArrayText(byte[] buffer, int offset, int size)
        {
            if (offset == 0 && buffer.Length == size)
            {
                return FrameBase.GetBytesText(buffer);
            }

            byte[] bys = new byte[size];

            Array.Copy(buffer, offset, bys, 0, size);

            return FrameBase.GetBytesText(bys);
        }

        /// <summary>触发通讯端口文本消息事件。
        /// </summary>
        protected void CallEventCommMessageOccured(string exceptionText)
        {
            UtMessageBase.ShowOneMessage(exceptionText, PopupMessageType.Info);
        }

        #endregion

        #endregion

        #region ICommPort其他成员

        public object Tag { get; set; }

        /// <summary>
        /// 获取端口的打开、连接、关闭等状态。
        /// </summary>
        public abstract PortStatus PortState { get; }

        /// <summary>
        /// 返回端口是否打开或连接。
        /// 若端口是TCPServer，则表示端口是否已处于侦听状态；
        /// 若端口是TCPClient，则表示端口是否与远端TCPServer建立了有效的连接。
        /// </summary>
        public abstract bool IsConnected { get; }

        /// <summary>
        /// 进一步判断连接是否中断
        /// 这一判断通常会额外消耗时间
        /// </summary>
        /// <returns></returns>
        public virtual bool IsConnectionBroken()
        {
            return !IsConnected;
        }

        public virtual bool CanReopen
        {
            get
            {
                return false;
            }
        }

        public virtual bool DisposedAbnormally
        {
            get { return false; }
        }

        /// <summary>
        /// 返回端口参数类型。
        /// 当端口处在连接或打开等运行状态时，若尝试更改端口参数，程序将会抛出“端口操作时不能更改参数”的异常。
        /// </summary>
        public abstract PortParamBase PortParam { get; }

        /// <summary>
        /// 打开端口。
        /// 打开成功则返回true，否则返回false。
        /// 注：若端口是TCPServer，则此函数将设置Socket为侦听状态；若端口是TCPClient，则此函数将执行与远端TCPServer建立连接。
        /// </summary>
        public abstract bool Open();

        /// <summary>
        /// 关闭端口。
        /// </summary>
        public abstract void Close();

        protected bool portCanAutoReopen;

        public void Close(bool allowAutoOpen)
        {
            portCanAutoReopen = allowAutoOpen;
            Close();
        }

        protected bool busyWriting = false;

        /// <summary>
        /// 判断端口是否正在写数据
        /// 用于写大数据时不再发送其它数据
        /// </summary>
        virtual public bool BusyWriting
        {
            get
            {
                return busyWriting;
            }

            set
            {
                busyWriting = value;
            }
        }

        /// <summary>
        /// 发送数据。发送成功则返回true，否则返回false。
        /// </summary>
        /// <param name="data">要发送的数据。</param>
        virtual public bool Send(byte[] data)
        {
            if (data == null || !IsConnected)
            {
                return false;
            }

            if (data.Length == 0)
            {
                return true;
            }

            if (sendBuffLimit <= 0)
            {
                return Send(data, 0, data.Length);
            }
            
            CallEventBytesSent(data, 0, data.Length);

            bool result = true;
            int leftdata = data.Length;
            int startAt = 0;
           
            while (leftdata > 0 && result)
            {
                if (leftdata <= sendBuffLimit)
                {
                    Send(data, startAt, leftdata);
                    leftdata = 0;
                }

                result = Send(data, startAt, sendBuffLimit);
                startAt += sendBuffLimit;
                leftdata -= sendBuffLimit;
                System.Threading.Thread.Sleep(0);
            }

            return result;
        }

        /// <summary>
        /// 发送数据。发送成功则返回true，否则返回false。
        /// </summary>
        /// <param name="buffer">byte类型的数组，它包含发送的数据。</param>
        /// <param name="offset">数据缓冲区中开始发送数据的位置。</param>
        /// <param name="size">要发送的字节数。</param>
        public abstract bool Send(byte[] buffer, int offset, int size);

        #region 缓冲区属性及操作

        /// <summary>
        /// 返回缓冲区是否有数据。
        /// </summary>
        virtual public bool HasData
        {
            get { return bufferAccessor.HasData; }
        }

        /// <summary>
        /// 查看并返回缓冲区的数据（不改变缓冲区数据）。
        /// 若缓冲区中的数据少于size则返回null。
        /// </summary>
        /// <param name="size">要查看的数据大小</param>
        virtual public byte[] Peek(int size)
        {
            return bufferAccessor.Peek(size);
        }

        /// <summary>
        /// 查看并返回缓冲区的数据（从缓冲区删除已返回的数据）。
        /// 若缓冲区中的数据少于size则返回null。
        /// </summary>
        /// <param name="size">要查看的数据大小</param>
        virtual public byte[] Read(int size)
        {
            return bufferAccessor.Read(size);
        }

        virtual public void RemoveData(int size)
        {
            bufferAccessor.RemoveData(size);
        }

        /// <summary>
        /// 获得缓冲区数据长度。
        /// </summary>
        virtual public int GetCacheSize()
        {
            return bufferAccessor.GetCacheSize();
        }

        /// <summary>
        /// 共享数据接收
        /// </summary>
        /// <param name="buffer"></param>
        /// <param name="offset"></param>
        /// <param name="size"></param>
        public void ShareDataReceived(byte[] buffer, int offset, int size)
        {
            bufferAccessor.Write(buffer, offset, size);
            CallEventBytesReceived(buffer, offset, size);
        }

        /// <summary>
        /// 清空缓冲区。
        /// </summary>
        virtual public void ClearBuffer()
        {
            bufferAccessor.Clear();
        }
        #endregion
        #endregion

        /// <summary>
        /// 用Debug.WriteLine打印指定信息。
        /// </summary>
        /// <param name="info">打印的文本信息。</param>
        /// <param name="log">是否在调试模式下写入日志</param>
        public void WriteDebugInfo(string info, bool log)
        {
            if (Debugger.IsAttached)
            {
                string nowTime = DateTime.Now.TimeOfDay.ToString(); // nowTime的长度是不确定的。
                
                Debug.WriteLine(string.Format("{0} {1}", nowTime.Substring(0, nowTime.Length < 12 ? nowTime.Length : 12), info));
            }

            if (log && EnableDebugMessage)
            {
                UtMessageBase.ShowOneMessage(info, PopupMessageType.Info);
            }
        }
    }
}
