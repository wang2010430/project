/***************************************************************************************************
* copyright : 芯迈微半导体（珠海）有限公司
* version   : 1.00
* file      : IProtocol.cs
* date      : 2023/03/16
* author    : haozhe.ni
* brief     : IProtocol
* section Modification History
* - 1.0 : Initial version - haozhe.ni
***************************************************************************************************/

using Common;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace Channel
{
    /// <summary>
    /// 通讯端口切换成另一个端口的事件委托。
    /// </summary>
    /// <param name="oldPort">切换前的通讯端口。</param>
    public delegate void PortSwitchedEventHandler(object sender, ICommPort oldPort);

    /// <summary>
    /// 协议状态改变时将产生的事件的委托。
    /// </summary>
    /// <param name="currentState">协议当前状态。</param>
    /// <param name="oldState">协议状态改变之前的状态。</param>
	public delegate void ProtocolStateChangedEventHandler(object sender, ProtocolState currentState, ProtocolState oldState);
   
    /// <summary>
    /// 发送或接收一个帧产生的事件的委托。
    /// </summary>
    /// <param name="frame">已发送或接收的帧对象。</param>
    public delegate void FrameSentReceivedEventHandler(object sender, FrameBase frame);

    /// <summary>
    /// 协议主动向应用处理事务时产生此事件的事件委托。
    /// </summary>
    /// <param name="sender">发送者</param>
    /// <param name="datas">协议传来的数据。</param>
    public delegate void ProtocolProcessActivelyEventHandler(object sender, object[] datas);

    /// <summary>
    /// 数据包接收、发送、丢弃事件
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="time"></param>
    /// <param name="type"></param>
    /// <param name="messageBytes"></param>
    public delegate void BytesSendReceiveThrowEventHandler(object sender, DateTime time, BytesType type, byte[] bytes, string message);

    /// <summary>
    /// 协议对外的统一接口。
    /// </summary>
    public interface IProtocol
    {
        #region 属性

        /// <summary>
        /// 协议是否是可以工作的
        /// </summary>
        bool WorkableProtocol { get;}

		/// <summary>可以通过设置此对象来关联使对象与本协议对象关联。
		/// 它对通讯业务不产生任何影响。
		/// </summary>
		object Tag { get;set;}

        /// <summary>
        /// 协议用公共数据
        /// </summary>
        object CommonData { get;set;}

        /// <summary>
        /// 获取或设置协议通讯使用的端口。
        /// 若协议在运行时设置端口，将会导致协议停止工作。
        /// </summary>
        ICommPort Port { get; set; }

        /// <summary>
        /// 与其它协议共享的端口，为多协议共享同一端口而增加 2012-11-1 zsw
        /// </summary>
        IProtocol SharedProtocol { get; set; }
        /// <summary>
        /// 获取或设置协议实体名。
        /// </summary>
        string Name { get; set; }

        /// <summary>获取协议所属通道。
        /// </summary>
        Channel Channel { get;}

        /// <summary>
        /// 返回协议是否处于运行状态（一般协议线程及通讯端口都处于运行状态时返回true）。
        /// </summary>
        bool ProtocolIsRunning { get; }

        /// <summary>
        /// 返回协议线程是否处于活动状态。
        /// </summary>
        bool ProtocolThreadIsAlive { get; }

        /// <summary>
        /// 获取协议所具有的功能名（它是只读）。
        /// </summary>
        ReadOnlyCollection<string> FunctionNames { get;}

        /// <summary>获取协议状态。
        /// </summary>
        ProtocolState ProtocolState { get;}

        /// <summary>获取或设置是否需要端口（默认值是需要）。
        /// 若不需要，则启动协议后，协议的运行将只是一个线程在运行。
        /// 通常只会在需要模拟一个端口协议的运作时才设置为不需要端口。这里将这种运行情况称为虚拟协议。
        /// 在虚拟协议运行模式下，若仍然设置了有效端口，则端口依然会被运行，这样协议和应用程序将有更高的可控性。
        /// </summary>
        bool NeedPort { get; set;}
        /// <summary>
        /// 协议某次任务传送对象的特征，如钥匙蓝牙ID、无线ID、电话号码等
        /// </summary>
        string PropertyForTransObject { get; set; }

        /// <summary>
        /// 传送时是否指定到特定钥匙
        /// </summary>
        bool NeedToDesignatedKey { get; set; }

        /// <summary>
        /// 协议线程的优先级
        /// </summary>
        System.Threading.ThreadPriority ProtocolPriority { get; set; }

        /// <summary>协议状态改变时产生此事件。
        /// </summary>
        event ProtocolStateChangedEventHandler ProtocolStateChanged;
        /// <summary>通讯端口切换成另一个端口将产生此事件。
        /// </summary>
        event PortSwitchedEventHandler PortSwitched;
        /// <summary>协议消息发生的事件。
        /// </summary>
        event MessageOccuredEventHandler ProtocolMessageOccured;
        /// <summary>
        /// 协议发送数据后产生此事件。
        /// </summary>
        event BytesTransferEventHandler BytesSent;
		/// <summary>协议的端口收到一个数据包时产生此事件。
		/// </summary>
		event BytesTransferEventHandler BytesReceived;
        /// <summary>
        /// 发送一个帧对象数据后产生此事件。
        /// </summary>
        event FrameSentReceivedEventHandler FrameSent;
        /// <summary>
        /// 接收一个有效帧后产生此事件。
        /// </summary>
        event FrameSentReceivedEventHandler FrameReceived;
        /// <summary>协议主动向应用处理事务时产生此事件。
        /// </summary>
        event ProtocolProcessActivelyEventHandler ProtocolProcessActivelyOccured;
		
        /// <summary>
        /// 数据包发送、接收、丢弃事件
        /// </summary>
        event BytesSendReceiveThrowEventHandler BytesSendReceiveThrow;
        #endregion

        #region 协议的启动与关闭

        /// <summary>
        /// 初始化公共数据
        /// </summary>
        void InitCommonData();

        /// <summary>
        /// 启动协议。内部操作首先打开通讯端口，再启动协议线程。
        /// 若端口能打开连接，并协议线程启动正常，则返回true，否则返回false。
        /// 若调用了此函数，不论协议是否正常启动，程序都应该调用StopWork来停止协议。
        /// </summary>
        bool StartWork();
        bool StartWork(System.Threading.ThreadPriority priority);
        /// <summary>
        /// 启动协议。内部操作首先打开通讯端口，再启动协议线程。
        /// 首先将根据portParam创建通讯端口，若端口能打开连接，并协议线程启动正常，则将当前端口设置未port，返回true，否则返回false。
        /// 若调用了此函数，不论协议是否正常启动，程序都应该调用StopWork来停止协议。
        /// </summary>
        /// <param name="portParam">通讯端口参数</param>
        bool StartWork(PortParamBase portParam);

        /// <summary>
        /// 启动协议。内部操作首先打开通讯端口，再启动协议线程。
        /// 若端口能打开连接，并协议线程启动正常，则将当前端口设置未port，返回true，否则返回false。
        /// 若调用了此函数，不论协议是否正常启动，程序都应该调用StopWork来停止协议。
        /// </summary>
        /// <param name="port">协议使用的通讯端口</param>
        bool StartWork(ICommPort port);

        /// <summary>
        /// 停止协议。
        /// 首先关闭通讯端口，再关闭协议线程。
        /// </summary>
        /// <param name="waitForStop">它表示是否愿意阻塞当前线程，直至协议线程退出，协议完全停止工作。但阻塞时间不会超过1秒，否则协议线程将被强行关闭。</param>
        bool StopWork(bool waitForStop);

        bool CanRestart { get; }

        #endregion

        #region 任务相关

        /// <summary>
        /// 获取任务列表。
        /// 返回值为只读列表。
        /// </summary>
        IList<ProtocolTask> TaskList { get; }

        /// <summary>
        /// 同步执行任务。若任务执行失败或超时则返回false，否则返回true（执行成功后通过task处理执行结果）。
        /// </summary>
        /// <param name="task">要执行的任务。</param>
        /// <param name="timeout">执行任务的等待时限毫秒（值为-1表示无限时）。</param>
        bool ExecuteTaskSync(ProtocolTask task, int timeout);

        /// <summary>
        /// 异步执行任务。调用此函数后，函数将立即返回。若任务执行完成或超时将会调用回调函数asyncCallback。
        /// 若关心任务的执行结果，可以通过回调函数传递的task的task.TaskState和task.Result查看执行结果信息。
        /// </summary>
        /// <param name="task">要执行的任务。</param>
        /// <param name="asyncCallback">异步回调对象。</param>
        /// <param name="timeout">执行任务的时限毫秒（值为-1表示无限时），若超时将调用asyncCallback的回调函数。</param>
        void ExecuteTaskAsync(ProtocolTask task, AsyncCallback asyncCallback, int timeout);

        /// <summary>
        /// 发送数据。发送成功则返回true，否则返回false。
        /// </summary>
        /// <param name="buffer">byte类型的数组，它包含发送的数据。</param>
        /// <param name="offset">数据缓冲区中开始发送数据的位置。</param>
        /// <param name="size">要发送的字节数。</param>
        bool Send(byte[] buffer, int offset, int size);
        bool Send(byte[] buffer, int offset, int size, IProtocol p);

        #endregion

        /// <summary>重新寻找和处理端口缓冲区中未处理的数据。
        /// 当端口有收到数据时，再将端口设置到协议中，此时应该调用此函数。
        /// </summary>
        //void RetrieveReceivedData();

        /// <summary>
        /// 配置协议运行时需要的参数。
        /// 配置成功则返回true.
        /// </summary>
        /// <param name="paramFormat">协议配置参数的格式。</param>
        /// <param name="param">要配置的协议参数。</param>
        bool Config(ProtocolConfigurationParameterFormat paramFormat, object param);

        /// <summary>
        /// 配置协议运行时需要的参数。
        /// 配置成功则返回true.
        /// </summary>
        /// <param name="paramFormat">协议配置参数的格式。</param>
        /// <param name="param">要配置的协议参数。</param>
        bool Config(ProtocolConfigurationParameterFormat paramFormat, object param, object param1);

        /// <summary>获取协议的配置参数。
        /// </summary>
        /// <param name="paramFormat">协议配置参数的格式。</param>
        /// <returns></returns>
        object GetConfigurationParameter(ProtocolConfigurationParameterFormat paramFormat);

        /// <summary>
        /// 返回是否包含指定的协议功能。
        /// </summary>
        /// <param name="functionName">功能名。</param>
        bool ContainsFunction(string functionName);

        /// <summary>
        /// 注册子协议
        /// </summary>
        /// <param name="?"></param>
        void RegisterSubprotocol(IProtocol p);
        /// <summary>
        /// 停止规约的上行链接  lym
        /// </summary>
        void StopUpLink();

        /// <summary>
        /// 重启规约的上行链接  lym
        /// </summary>
        void ResumeUpLink();
    }

    /// <summary>
    /// 协议信息接口。通过此接口的实例可获得协议相关的信息，以动态创建协议对象。
    /// </summary>
    public interface IProtocolsInfo
    {
        /// <summary>
        /// 获取所有的协议类名。
        /// </summary>
        /// <returns></returns>
        string[] GetAllProtocolNames();

        /// <summary>
        /// 获取指定协议名的协议描述。
        /// </summary>
        /// <param name="protocolname">协议名。</param>
        string GetProtocolDescription(string protocolName);

        /// <summary>
        /// 设置指定协议的参数到param中。设置成功返回true.
        /// </summary>
        /// <param name="protocolName">协议名</param>
        /// <param name="param">协议参数</param>
        bool ParamsSetting(string protocolName, ref byte[] param);

        /// <summary>
        /// wjh Added 使得客户端可修改其它客户端的规约配置参数
        /// </summary>
        /// <param name="protocolName"></param>
        /// <param name="clientName"></param>
        /// <param name="xmlFormatParam"></param>
        /// <param name="param"></param>
        /// <returns></returns>
        bool ParamsSetting(string protocolName,string clientName, ref string xmlFormatParam, object param);

        /// <summary>
        /// 设置指定协议的参数到xmlFormatParam中。设置成功返回true.
        /// </summary>
        /// <param name="protocolName">协议名</param>
        /// <param name="xmlFormatParam">Xml格式的协议参数</param>
        /// <param name="param"></param>
        /// <returns></returns>
        bool ParamsSetting(string protocolName, ref string xmlFormatParam,object param);

        /// <summary>返回指定的协议是否为虚拟协议。
        /// 虚拟协议不存在通讯端口，而是自己模拟与远端通讯，可接收数据输入和输出操作结果。
        /// </summary>
        /// <param name="protocolName">指定的协议名。</param>
        bool ProtocolIsVirtual(string protocolName);

        /// <summary>
        /// 缺省端口参数
        /// </summary>
        /// <returns></returns>
        string DefaultPortParameter();

        /// <summary>
        /// 协议支持的操作系统
        /// </summary>
        List<OsFamily> SupportedOperationSystem();

        /// <summary>
        /// 支撑协议配置的系统
        /// </summary>
        /// <returns></returns>
        List<OsFamily> ProtocolConfigureOperationSystem();
    }

    /// <summary>协议的配置参数类型。
    /// </summary>
    public enum ProtocolConfigurationParameterFormat
    {
        /// <summary>二进制数组格式。
        /// </summary>
        BinaryArray,
        /// <summary>Xml字符串格式。
        /// </summary>
        XmlString
    }

    /// <summary>
    /// 协议状态。
    /// </summary>
    public enum ProtocolState
    {
        /// <summary>
        /// 协议处于空闲状态。
        /// </summary>
        Idle,
        /// <summary>
        /// 正忙于某个任务。此时应用程序不应该再给协议添加其他任务。
        /// </summary>
        BusyForTask,
        /// <summary>
        /// 与远程设备失去通讯，此时端口应该仍然处于连接状态。
        /// </summary>
        LostTouchWithRemote,
        /// <summary>
        /// 协议的通讯端口已经关闭（连接中断）。
        /// </summary>
        PortDisconnected
    }
}
