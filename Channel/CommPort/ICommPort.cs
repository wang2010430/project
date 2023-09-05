/***************************************************************************************************
* copyright : 芯迈微半导体（珠海）有限公司
* version   : 1.00
* file      : ICommPort.cs
* date      : 2023/03/16
* author    : haozhe.ni
* brief     : ICommPort
* section Modification History
* - 1.0 : Initial version - haozhe.ni
***************************************************************************************************/

using Common;
using System;
using System.Collections.Generic;

namespace Channel
{
    public delegate void FlagEventHandler(object sender);
    public delegate void PortConnectedEventHandler(ICommPort sender);
    public delegate void PortOpenedEventHandler(object sender, PortOpenReasons openReason);
    public delegate void PortClosedEventHandler(object sender, PortClosedReasons closedReason);

    /// <summary>
    /// 数据传输的委托。
    /// </summary>
    /// <param name="sender">发送者通常都是通讯端口对象。</param>
    /// <param name="buffer">byte类型的数组，它包含要处理的有效数据。</param>
    /// <param name="offset">数据缓冲区中有效数据的开始位置。</param>
    /// <param name="size">有效数据的字节数。</param>
    public delegate void BytesTransferEventHandler(object sender, byte[] buffer, int offset, int size);

    /// <summary>
    /// 提示信息文本出现（如有些异常信息或其他提示信息）
    /// </summary>
    /// <param name="messageText">文本信息</param>
	public delegate void MessageOccuredEventHandler(object sender, string messageText);

    /// <summary>
    /// 通讯端口的统一接口。
    /// </summary>
    public interface ICommPort
    {        
        int SerialNumber { get; set; }

        string ChannelGuid { get; set; }

        string PairedPortUniqueId { get; set; }

        string PortDescription
        {
            get;
        }

        event PortOpenedEventHandler OnOpened;

        /// <summary>
        /// 端口状态改变为连接状态时产生此事件。
        /// </summary>
        event FlagEventHandler Connected;

        /// <summary>
        /// 端口由打开（连接）状态变位关闭状态时产生此事件。
        /// </summary>
        event PortClosedEventHandler Closed;

        /// <summary>
        /// 数据即将成功发送时产生此事件。
        /// </summary>
        event BytesTransferEventHandler BytesSent;

        /// <summary>
        /// 端口缓冲区收到一个数据包时产生此事件。
        /// </summary>
        event BytesTransferEventHandler BytesReceived;

        /// <summary>
        /// 通讯端口产生信息需要提示时产生此事件。
        /// （信息可能是异常信息或其他提示信息。）
        /// </summary>
        //event MessageOccuredEventHandler CommMessageOccured;

        MessageOccuredEventHandler AfterPairedPortUniqueIdSet { get; set; }

        bool NeedProtocol { get;set;}

        bool EnableDebugMessage { get; set; }

        /// <summary>
        /// 是否自己加上帧头,为方便应用层应用
        /// </summary>
        byte[] FrameHead { get; set; }

        /// <summary>返回端口是否已经被不正常地被释放了。
        /// </summary>
        bool DisposedAbnormally { get;}

        /// <summary>
        /// 网络流量单位KB/S,如果是0就不限速
        /// </summary>
        int NetworkTraffic { set;}

        /// <summary>
        /// 端口接收数据限制字节数
        /// </summary>
        int ReadBuffLimit { get; set; }

        /// <summary>
        /// 端口发送缓冲区限制字节数
        /// </summary>
        int SendBuffLimit { get; set; }

        UInt32 KeepAliveTime { set; }

        PortReadWriteMode ReadWriteMode { get; set; }

        /// <summary>
        /// 远方IP地址,适用于设备没有标示，只有IP地址的情况，add by youxinping 2017-7-21
        /// </summary>
        string Address { get; set; }

        /// <summary>
        /// 获取端口的状态（目前只有打开、连接、关闭状态）。
        /// </summary>
        PortStatus PortState { get; }

         /// <summary>
        /// 返回端口是否打开或连接。
        /// 若端口是TCPServer，则表示端口是否已处于侦听状态；
        /// 若端口是TCPClient，则表示端口是否与远端TCPServer建立了有效的连接。
        /// </summary>
        bool IsConnected { get; }

        /// <summary>
        /// 进一步判断连接是否中断
        /// 这一判断通常会额外消耗时间
        /// </summary>
        /// <returns></returns>
        bool IsConnectionBroken();

        /// <summary>
        /// 返回端口参数类型。
        /// 当端口处在连接或打开等运行状态时，若尝试更改端口参数，程序将会抛出“端口操作时不能更改参数”的异常。
        /// </summary>
        PortParamBase PortParam { get; }

        /// <summary>
        /// 打开端口。
        /// 打开成功则返回true，否则返回false。
        /// 注：若端口为TCPServer，则将设置Socket为侦听状态；若端口为TCPClient，则将与远端建立连接。
        /// </summary>
        bool Open();

        /// <summary>
        /// 关闭端口。
        /// </summary>
        void Close();

        void Close(bool canReopen);

        /// <summary>
        /// 端口是否可以关闭后重新打开
        /// </summary>
        bool CanReopen
        {
            get;
        }

        /// <summary>
        /// 发送数据。发送成功则返回true，否则返回false。
        /// </summary>
        /// <param name="data">要发送的数据。</param>
        bool Send(byte[] data);

        /// <summary>
        /// 发送数据。发送成功则返回true，否则返回false。
        /// </summary>
        /// <param name="buffer">byte类型的数组，它包含发送的数据。</param>
        /// <param name="offset">数据缓冲区中开始发送数据的位置。</param>
        /// <param name="size">要发送的字节数。</param>
        bool Send(byte[] buffer, int offset, int size);

        /// <summary>
        /// 端口支持的操作系统
        /// </summary>
        List<OsFamily> SupportedOperationSystem
        {
            get;
        }

        void WriteDebugInfo(string info, bool log);

        #region 端口缓冲区的属性及操作

		/// <summary>可以通过设置此对象来关联使对象与本端口对象关联。
		/// 它对通讯业务不产生任何影响。
		/// </summary>
		object Tag { get;set;}

        /// <summary>
        /// 返回缓冲区是否有数据。
        /// </summary>
        bool HasData { get;}

        /// <summary>
        /// 查看并返回缓冲区的数据（不改变缓冲区数据）。
        /// 若缓冲区中的数据少于size则返回null。
        /// </summary>
        /// <param name="size">要查看的数据大小</param>
        byte[] Peek(int size);

        /// <summary>
        /// 查看并返回缓冲区的数据（从缓冲区删除已返回的数据）。
        /// 若缓冲区中的数据少于size则返回null。
        /// </summary>
        /// <param name="size">要查看的数据大小</param>
        byte[] Read(int size);

        /// <summary>
        /// 清除端口缓冲区数据。
        /// </summary>
        /// <param name="size">要清除数据的大小</param>
        void RemoveData(int size);

        /// <summary>
        /// 获得缓冲区数据长度。
        /// </summary>
        int GetCacheSize();

        /// <summary>
        /// 清空缓冲区。
        /// </summary>
        void ClearBuffer();

        /// <summary>
        /// 共享数据接收
        /// </summary>
        /// <param name="buffer"></param>
        /// <param name="offset"></param>
        /// <param name="size"></param>
        void ShareDataReceived(byte[] buffer, int offset, int size);
        #endregion
    }

    /// <summary>
    /// 端口打开、连接、关闭等状态。
    /// </summary>
    public enum PortStatus
    {
        /// <summary>
        /// 表示端口已经打开（如TCPServer若已经处于侦听状态，则它的值为true）。
        /// </summary>
        Opened,
        /// <summary>
        /// 表示端口与远端已经建立了有效的连接。
        /// </summary>
        Connected,
        /// <summary>
        /// 表示端口已经关闭。
        /// </summary>
        Closed
    }

    public enum PortOpenReasons
    {
        /// <summary>
        /// 正常打开
        /// </summary>
        Normal,
        /// <summary>
        /// 重新打开，异常关闭后自动重新打开
        /// </summary>
        AutoOpenAfterClose
    }

    /// <summary>
    /// 端口关闭的原因。
    /// </summary>
    public enum PortClosedReasons
    {
        /// <summary>
        /// 本地正常关闭。
        /// </summary>
        LocalClosedNormally,
        /// <summary>
        /// 远端主动关闭端口。
        /// </summary>
        RemoteClosed,
        /// <summary>
        /// 物理端口异常。
        /// </summary>
        PortIsPhysicallyAbnormal
    }

    /// <summary>
    /// 端口读写模式
    /// </summary>
    public enum PortReadWriteMode
    {
        ReadWrite,
        ReadOnly,
        WriteOnly
    }
}
