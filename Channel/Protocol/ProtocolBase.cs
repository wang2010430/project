/***************************************************************************************************
* copyright : 芯迈微半导体（珠海）有限公司
* version   : 1.00
* file      : ProtocolBase.cs
* date      : 2023/03/16
* author    : haozhe.ni
* brief     : 规约基类
* section Modification History
* - 1.0 : Initial version - haozhe.ni
***************************************************************************************************/

using Common;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Threading;

namespace Channel
{
    /// <summary>
    /// 规约基类。
    /// </summary>
    public abstract class ProtocolBase : IProtocol
    {
        #region 成员变量
        /// <summary>
        /// 当前使用通讯端口。
        /// </summary>
        ICommPort commPort = null;

        /// <summary>
        /// 协议运行的线程。
        /// </summary>
        Thread protocolThread = null;

        /// <summary>
        /// 任务列表。
        /// </summary>
        readonly protected List<ProtocolTask> taskList = new List<ProtocolTask>();

        /// <summary>
        /// 接收帧数据的临时帧。若它不为null，表示当前正在接收数据。
        /// </summary>
        protected FrameBase receivingFrame = null;
        string entityName;
        internal Channel channel = null;

        event PortSwitchedEventHandler portSwitched;
        event ProtocolStateChangedEventHandler protocolStateChanged;
        event MessageOccuredEventHandler protocolMessageOccured;
        event FrameSentReceivedEventHandler frameSent;
        event FrameSentReceivedEventHandler frameReceived;
        event BytesTransferEventHandler bytesSent;
        event BytesTransferEventHandler bytesReceived;
        event ProtocolProcessActivelyEventHandler protocolProcessActivelyOccured = null;   
        event BytesSendReceiveThrowEventHandler bytesSendReceiveThrow;

        /// <summary>
        /// 功能名数组。
        /// </summary>
        protected string[] functionNames = null;

        /// <summary>
        /// 线程轮询的时间间隔（默认值为100毫秒）（单位：毫秒）
        /// </summary>
        protected int pollingInterval = 100;

        protected bool processFrameInSeperateThread = true;

        public bool ProcessFrameInSeperateThread
        {
            get
            {
                return processFrameInSeperateThread;
            }
        }

        public readonly CaseManager caseManager = new CaseManager();

        /// <summary>
        /// 协议是否是可以工作的
        /// </summary>
        public bool WorkableProtocol
        {
            get
            {
                return !NeedPort || (Port != null && Port.NeedProtocol);
            }
        }

        int _workCP = 0;
        public int workCP { get{return _workCP;}set{_workCP = value;} }

        /// <summary>
        ///  通讯发送数据状态
        /// </summary>
        public DataCommunicateState CommSendDataState
        {
            get
            {
                if (channel != null && channel.ChannelWorkState != null)
                {
                    return channel.ChannelWorkState.SendDataState;
                }

                return DataCommunicateState.Working;
            }

            set
            {
                if (channel != null && channel.ChannelWorkState != null)
                {
                    channel.ChannelWorkState.SendDataState = value;
                }
            }
        }

        /// <summary>
        ///  通讯接收数据状态
        /// </summary>
        public DataCommunicateState CommReceDataState
        {
            get
            {
                if (channel != null && channel.ChannelWorkState != null)
                {
                    return channel.ChannelWorkState.ReceDataState;
                }

                return DataCommunicateState.Working;
            }

            set
            {
                if (channel != null && channel.ChannelWorkState != null)
                {
                    channel.ChannelWorkState.ReceDataState = value;
                }
            }
        }

        /// <summary>
        /// 应加重的端口名，仅在显示客户端图标时使用
        /// 当因故端口无法加载时，协议的Port会是null,此时会用此值来决定显示图标类型
        /// </summary>
        public string PortTypeName
        {
            get;
            set;
        }

        public bool CanRestart
        {
            get
            {
                return this.Port == null || Port.CanReopen;
            }
        }

        #endregion

        private IEventWait waitingEvent;

        private ProtocolEvents releasedEvent = ProtocolEvents.Polling;
        protected bool setClosedEnent;

        public ProtocolBase()
            : this(true)
        {
        }

        public ProtocolBase(bool userCloseEvent)
        {
            setClosedEnent = userCloseEvent;
        }

        /// <summary>
        /// 设置关联端口后需执行的操作
        /// 实际规约中重写此函数，来完成不同协议对端口参数的直接设置
        /// </summary>
        public virtual void OnPortSet()
        {
        }

        #region IProtocol接口实现

        #region 其他属性

        public object Tag { get{return _tag;} set{_tag = value;} }
        object _tag = null;

        /// <summary>
        /// 协议公共数据
        /// </summary>
        public object CommonData { get; set; }

        /// <summary>
        /// 是否需要端口
        /// </summary>
        public bool NeedPort { get{return _needPort;} set{_needPort = value;} }
        bool _needPort = true;

        /// <summary>
        /// 协议某次任务传送对象的特征，如钥匙蓝牙ID、无线ID、电话号码等
        /// </summary>
        public virtual string PropertyForTransObject { get; set; }

        /// <summary>
        /// 传送时是否指定到特定钥匙
        /// </summary>
        public bool NeedToDesignatedKey { get; set; }

        public Channel Channel
        {
            get { return channel; }
        }

        /// <summary>
        /// 获取或设置协议通讯使用的端口。
        /// 设置成功后，新设置的端口处于什么状态，协议端口就处于什么状态（哪怕新端口为null也会被设置成功），它会影响协议的通讯状态。
        /// </summary>
        public ICommPort Port
        {
            get
            {
                return commPort;
            }

            set
            {
                ICommPort oldPort = commPort;

                if (oldPort != value)
                {
                    commPort = value;

                    if (commPort != null)
                    {
                        OnPortSet();  //设置了协议与端口的关联关系，协议中重新此函数可以强制改变端口的参数
                    }

                    try
                    {
                        CallEventPortSwitched(oldPort);
                        CallEventProtocoMessageOccured(string.Format("协议“{0}”的通讯端口已设置。", Name));
                    }
                    catch (Exception ex)
                    {
                        UtMessageBase.ShowOneMessage(Name + "协议切换通讯端口时发生异常，异常信息：" + ex.Message + "可能是端口切换的注册事件函数在发生异常，或注册端口切换的对象已经不存在而且也未注销事件，或是事件的响应函数中出现此异常错误。", PopupMessageType.Exception);
                    }
                }
            }
        }

        /// <summary>
        /// 设置与其它协议共享的端口，为多协议共享同一端口而增加 2012-11-1 zsw
        /// </summary>
        public IProtocol SharedProtocol { get{return _sharedProtocol;} set{_sharedProtocol =value;} }
        IProtocol _sharedProtocol = null;

        /// <summary>
        /// 注册子协议
        /// </summary>
        /// <param name="?"></param>
        virtual public void RegisterSubprotocol(IProtocol p)
        {
        }

        /// <summary>
        /// 停止规约的上行链接  lym
        /// </summary>
        virtual public void StopUpLink()
        {

        }

        /// <summary>
        /// 重启规约的上行链接
        /// </summary>
        virtual public void ResumeUpLink()
        { }

        void commPort_OnOpened(object sender, PortOpenReasons openReason)
        {
            if (openReason == PortOpenReasons.AutoOpenAfterClose)
            {
                StartWork();
            }
        }

        void commPort_Closed(object sender, PortClosedReasons portClosedReason)
        {
            StopWork(true, true);
        }

        /// <summary>
        /// 获取或设置协议实体名。
        /// </summary>
        public string Name
        {
            get { return entityName == null ? "" : entityName; }

            set { entityName = value; }
        }

        /// <summary>
        /// 返回协议是否处于运行状态。
        /// </summary>
        public bool ProtocolIsRunning
        {
            get
            {
                bool condition1 = (Port != null && (Port.IsConnected || Port.PortState == PortStatus.Opened)) || !NeedPort;

                if (condition1 && (ProtocolThreadIsAlive || !WorkableProtocol))
                {
                    return true;
                }

                return false;
            }
        }

        /// <summary>
        /// 返回协议线程是否处于活动状态。
        /// </summary>
        public bool ProtocolThreadIsAlive
        {
            get { return protocolThread != null && protocolThread.IsAlive && protocolThread.ThreadState != ThreadState.AbortRequested; }
        }

        public ProtocolState ProtocolState { get{return _protocolState;} private set{_protocolState = value;} }
        ProtocolState _protocolState = ProtocolState.PortDisconnected;

        public event ProtocolStateChangedEventHandler ProtocolStateChanged
        {
            add { protocolStateChanged += value; }
            remove { protocolStateChanged -= value; }
        }

        public event PortSwitchedEventHandler PortSwitched
        {
            add { portSwitched += value; }
            remove { portSwitched -= value; }
        }

        public event MessageOccuredEventHandler ProtocolMessageOccured
        {
            add { protocolMessageOccured += value; }
            remove { protocolMessageOccured -= value; }
        }

        public event FrameSentReceivedEventHandler FrameSent
        {
            add { frameSent += value; }
            remove { frameSent -= value; }
        }

        public event FrameSentReceivedEventHandler FrameReceived
        {
            add { frameReceived += value; }
            remove { frameReceived -= value; }
        }

        public event BytesTransferEventHandler BytesSent
        {
            add { bytesSent += value; }
            remove { bytesSent -= value; }
        }

        public event BytesTransferEventHandler BytesReceived
        {
            add { bytesReceived += value; }
            remove { bytesReceived -= value; }
        }

        public event ProtocolProcessActivelyEventHandler ProtocolProcessActivelyOccured
        {
            add { protocolProcessActivelyOccured += value; }
            remove { protocolProcessActivelyOccured -= value; }
        }

        public event BytesSendReceiveThrowEventHandler BytesSendReceiveThrow
        {
            add { bytesSendReceiveThrow += value; }
            remove { bytesSendReceiveThrow -= value; }
        }
        #endregion

        #region 协议的启动与关闭

        /// <summary>
        /// 启动协议。内部操作首先打开通讯端口，再启动协议线程。
        /// 若端口能打开连接，并协议线程启动正常，则返回true，否则返回false。
        /// 若调用了此函数，不论协议是否正常启动，程序都应该调用StopWork来停止协议。
        /// </summary>
        virtual public bool StartWork()
        {
#if DEBUG
            if (CommonHelper.IsDebug)
            {
                return true;
            }
#endif
            return StartWork(commPort);
        }

        /// <summary>
        /// 初始化公共数据
        /// </summary>
        virtual public void InitCommonData()
        {
        }

        /// <summary>
        /// 启动协议。内部操作首先打开通讯端口，再启动协议线程。
        /// 首先将根据portParam创建通讯端口，若端口能打开连接，并协议线程启动正常，则将当前端口设置为port，返回true，否则返回false。
        /// 若调用了此函数，不论协议是否正常启动，程序都应该调用StopWork来停止协议。
        /// </summary>
        /// <param name="portParam">通讯端口参数</param>
        public bool StartWork(PortParamBase portParam)
        {
            return StartWork(portParam.GetPort());
        }

        public ThreadPriority ProtocolPriority { get { return _protocolPriority; } set { _protocolPriority = value; } }
        ThreadPriority _protocolPriority = ThreadPriority.Normal;

        public bool StartWork(ThreadPriority priority)
        {
            ProtocolPriority = priority;

            return StartWork();
        }

        protected bool working = false;

        private FrameParser portFrameParser;
        /// <summary>
        /// 启动协议。内部操作首先打开通讯端口，再启动协议线程。
        /// 若端口能打开连接，并协议线程启动正常，则将当前端口设置未port，返回true，否则返回false。而之前的端口状态保持不变。
        /// 若调用了此函数，不论协议是否正常启动，程序都应该调用StopWork来停止协议。
        /// </summary>
        /// <param name="port">协议使用的通讯端口</param>
        public bool StartWork(ICommPort port)
        {
            try
            {
                if (!working)
                {
                    receivingFrame = null;
                   
                    // 1、打开新的通讯端口
                    if (port != null)
                    {
                        if (setClosedEnent)
                        {
                            commPort.Closed -= commPort_Closed;
                            commPort.Closed += commPort_Closed;
                            commPort.OnOpened -= commPort_OnOpened;
                            commPort.OnOpened += commPort_OnOpened;
                        }

                        commPort.BytesReceived -= commPort_BytesReceived;
                        commPort.BytesReceived += commPort_BytesReceived;

                        if (!port.IsConnected && port.PortState != PortStatus.Opened)
                        {
                            port.Open();
                            Thread.Sleep(0);   // 等待端口打开完成
                        }
                    }

                    // 2、启动协议线程
                    if (!NeedPort || (Port != null && (Port.IsConnected || Port.PortState == PortStatus.Opened)))  // port.PortState == PortStatus.Opened是针对TcpServer的，其他同样。
                    {
                        // 这里才知道是否已经打开
                        working = true;

                        if (Port != port)
                        {
                            Port = port;
                        }

                        waitingEvent = AutoResetEventFactory.CreateAutoResetEvent(false);
                       
                        if (WorkableProtocol && !ProtocolThreadIsAlive)
                        {
                            protocolThread = new Thread(new ThreadStart(ProtocolThreadProc))
                            {
                                Name = string.IsNullOrEmpty(Name) ? "未命名规约" : Name,
                                IsBackground = true,
                                Priority = ProtocolPriority
                            };

                            threadRunning = true;
                            protocolThread.Start();
                        }

                        if (ProtocolStateAccessor == ProtocolState.PortDisconnected)
                        {
                            ProtocolStateAccessor = ProtocolState.Idle;
                        }

                        if (Port == null)
                        {
                            channel.CallEventWorkStateSwitched();
                        }
                        else if (Port.ReadWriteMode != PortReadWriteMode.WriteOnly)
                        {
                            portFrameParser = new FrameParser(this);
                            portFrameParser.StartParse();
                        }

                        return true;
                    }

                    return false;
                }

                return true;
            }
            catch (Exception ex)
            {
                CallEventProtocoMessageOccured(string.Format("协议“{0}”的协议线程启动时异常：{1}", Name, ex.ToString()));
                
                return false;
            }
        }

        virtual public bool StopWork(bool waitForStop)
        {
            return StopWork(waitForStop, false);
        }

        /// <summary>
        /// 停止协议。
        /// 首先关闭通讯端口，再关闭协议线程。
        /// </summary>
        /// <param name="waitForStop">它表示是否阻塞当前线程，直至协议线程退出。</param>
        /// <param name="allowAutoOpen">是否在检查到设备插入后自动打开</param>
        virtual public bool StopWork(bool waitForStop, bool allowAutoOpen)
        {
            if (working)
            {
                working = false;

                if (portFrameParser != null)
                {
                    portFrameParser.StopParse();
                }

                if (Port != null)
                {
                    commPort.BytesReceived -= commPort_BytesReceived;
                    commPort.Closed -= commPort_Closed;
                  
                    if (!allowAutoOpen)
                    {
                        commPort.OnOpened -= commPort_OnOpened;
                    }

                    Port.Close(allowAutoOpen);
                    Port.ClearBuffer();
                    ProtocolStateAccessor = ProtocolState.PortDisconnected;
                }

                // 1、关闭协议线程
                if (protocolThread != null)
                {
                    threadRunning = false;
                    ReleaseEent(ProtocolEvents.ProtocolExit);

                    foreach (ProtocolTask tsk in taskList)
                    {
                        tsk.Complete(TaskState.Completed);
                    }

                    if (waitForStop)
                    {
                        if (protocolThread != null)
                        {
                            if (!protocolThread.Join(1000))
                            {
                                protocolThread.Abort();    // 强行退出线程。
                                protocolThread.Join(500);  // 若线程还未退出，可能是线程函数中catch到了Abort扔出的ThreadAbortException异常后，让线程继续运行所致。
                            }
                        }
                    }

                    waitingEvent.Close();
                    CallEventProtocoMessageOccured(string.Format("“{0}”的协议线程已退出。", Name));
                }

                UnInitialize();
            }

            return (Port == null) ? true : (Port.PortState == PortStatus.Closed);
        }

        #endregion

        #region 任务相关

        /// <summary>
        /// 获取任务列表。
        /// 返回值为只读列表。
        /// </summary>
        public IList<ProtocolTask> TaskList
        {
            get
            {
                lock (taskList)
                {
                    // 这里以只读的方式返回了taskList在此时间片的一个副本。
                    // 因为有多个线程会访问它，但又不想在协议线程中要lock（taksList）而增加了任务操作的负责度和更多的错误源。
                    List<ProtocolTask> snapshot = new List<ProtocolTask>();
                    snapshot.AddRange(taskList);

                    return snapshot.AsReadOnly();
                }
            }
        }

        public virtual bool ExecuteTaskSync(ProtocolTask task, int timeout)
        {
            if (!ProtocolIsRunning)
            {
                return false;
            }

            task.TaskExecuteSetup(null, null, timeout, true);
            AddTask(task);
            task.WaitToComplete(timeout);

            if (task.TaskState != TaskState.Completed)
            {
                task.SetAsOvertime();
                RemoveTask(task);
                caseManager.SetCaseAsDead(task);
               
                return false;
            }

            return true;
        }

        /// <summary>
        /// 异步执行任务
        /// </summary>
        /// <param name="task"></param>
        /// <param name="asyncCallback"></param>
        /// <param name="timeout"></param>
        public virtual void ExecuteTaskAsync(ProtocolTask task, AsyncCallback asyncCallback, int timeout)
        {
            if (ProtocolIsRunning)
            {
                task.TaskExecuteSetup(asyncCallback, task, timeout, false);
                AddTask(task);
            }
            else
            {
                if (asyncCallback != null)
                {
                    task.TaskExecuteSetup(asyncCallback, task, timeout, false);
                    task.Complete(TaskState.NewTask);
                }
            }
        }

        /// <summary>
        /// 移除一个任务
        /// </summary>
        /// <param name="task">被移除的任务</param>
        internal bool RemoveTask(ProtocolTask task)
        {
            bool result;

            lock (taskList)
            {
                result = taskList.Remove(task);
            }

            if (result)
            {
                CallEventProtocoMessageOccured(string.Format("协议“{0}”中“{1}”任务已结束。", Name, task.Name));
            }

            return result;
        }

        /// <summary>
        /// 给规约增加任务
        /// </summary>
        /// <param name="task">要添加的任务。</param>
        protected void AddTask(ProtocolTask task)
        {
            if (task == null)
            {
                return;
            }

            lock (taskList)
            {
                if (!taskList.Contains(task))
                {
                    task.SetRelatedProtocol(this);
                    taskList.Insert(0, task);
                    CallEventProtocoMessageOccured(string.Format("协议“{0}”增加了一个“{1}”任务。", Name, task.Name));
                    ReleaseEent(ProtocolEvents.TaskArrived);
                }
            }
        }

        #endregion

        #region 业务相关

        /// <summary>获得只读的业务列表。
        /// </summary>
        protected IList<CaseBase> BusinessCaseList
        {
            get { return caseManager.BusinessCaseList; }
        }

        internal void AddCase(CaseBase caseObj)
        {
            caseManager.Add(caseObj);
        }

        internal void RemoveCase(CaseBase caseObj)
        {
            caseManager.Remove(caseObj);
        }

        #endregion

        public virtual bool Config(ProtocolConfigurationParameterFormat paramFormat, object param) { return true; }
        
        public virtual bool Config(ProtocolConfigurationParameterFormat paramFormat, object param, object param1)
        {
            Config(paramFormat, param);

            return true;
        }

        public virtual object GetConfigurationParameter(ProtocolConfigurationParameterFormat paramFormat) { return null; }

        #endregion

        #region 数据的发送与接收

        /// <summary>
        /// 显示发送调试信息
        /// </summary>
        /// <param name="msg"></param>
        public void SendingDebugText(string msg)
        {
            CallEventFrameSent(new FrameTextMessage(msg));
        }

        /// <summary>
        /// 显示接收调试信息
        /// </summary>
        /// <param name="msg"></param>
        public void RecevingDebugText(string msg)
        {
            CallEventFrameRecieved(new FrameTextMessage(msg));
        }

        internal bool SendFrame(FrameBase frame)
        {
            return Send(frame);
        }

        /// <summary>
        /// 发送数据。发送成功则返回true，否则返回false。
        /// </summary>
        /// <param name="buffer">byte类型的数组，它包含发送的数据。</param>
        /// <param name="offset">数据缓冲区中开始发送数据的位置。</param>
        /// <param name="size">要发送的字节数。</param>
        /// <param name="p">发送数据的协议</param>
        /// <returns></returns>
        public virtual bool Send(byte[] buffer, int offset, int size, IProtocol p)
        {
            if (p.SharedProtocol != null)
            {
                return p.SharedProtocol.Send(buffer, offset, size, p);
            }
            else
            {
                bool result = false;

                if (Port != null && Port.IsConnected)
                {
                    if (Port.SendBuffLimit <= 0)
                    {
                        CallEventBytesSent(buffer, offset, size);
                        result = Port.Send(buffer, offset, size);
                    }
                    else
                    {
                        result = true;
                        int leftdata = size;
                        int startAt = offset;
                        int frameCount = 0;

                        while (leftdata > 0 && result)
                        {
                            if (leftdata <= Port.SendBuffLimit)
                            {
                                result = Port.Send(buffer, startAt, leftdata);
                                leftdata = 0;
                            }
                            else
                            {
                                result = Port.Send(buffer, startAt, Port.SendBuffLimit);
                                startAt += Port.SendBuffLimit;
                                leftdata -= Port.SendBuffLimit;
                            }

                            frameCount++;
                            Thread.Sleep(0);
                        }
                    }
                }

                if (result)
                {
                    CommSendDataState = DataCommunicateState.Working;
                }

                return result;
            }
        }

        /// <summary>
        /// 发送数据。发送成功则返回true，否则返回false。
        /// </summary>
        /// <param name="buffer">byte类型的数组，它包含发送的数据。</param>
        /// <param name="offset">数据缓冲区中开始发送数据的位置。</param>
        /// <param name="size">要发送的字节数。</param>
        public virtual bool Send(byte[] buffer, int offset, int size)
        {
            return Send(buffer, offset, size, this);
        }

        /// <summary>
        /// 从通讯端口发送数据。发送成功则返回true，否则返回false。
        /// </summary>
        /// <param name="data">要发送的数据</param>
        protected virtual bool Send(byte[] data)
        {
            if (data != null)
            {
                return Send(data, 0, data.Length);
            }

            return false;
        }

        /// <summary>
        /// 从通讯端口发送帧数据。发送成功则返回true，否则返回false。
        /// </summary>
        /// <param name="frame">要发送数据所在的帧</param>
        protected virtual bool Send(FrameBase frame)
        {
            if (frame != null)
            {
                byte[] sendingbytes = frame.FrameBytes;

                if (sendingbytes != null)
                {
                    // 避免多次直接使用FrameBytes，直接使用FrameBytes会对报文进行解析，多次使用就是多次解析
                    if (Send(sendingbytes, 0, sendingbytes.Length))
                    {
                        CallEventFrameSent(frame);
                        CallBytesSendReceiveThrow(BytesType.Send, frame.FrameBytes, frame.Desc);

                        return true;
                    }
                }
            }

            return false;
        }

        /// <summary>
        /// 获得一个帧（本协议使用到的）实例。
        /// 注：此函数一定要新建并返回一个帧实例，该帧用于每次接收数据时解析数据包成帧。若不存在数据的收发则返回空。
        /// </summary>
        protected abstract FrameBase GetANewFrameToReceiveData();

        /// <summary>
        /// 新增此函数而不改变GetANewFrameToReceiveData的访问级别从而保证老版本规约可以编译过去
        /// </summary>
        /// <returns></returns>
        public FrameBase CreateNewFrameToReceiveData()
        {
            return GetANewFrameToReceiveData();
        }

        void commPort_BytesReceived(object sender, byte[] buffer, int offset, int size)
        {
            CallEventBytesReceived(buffer, offset, size);

            if (portFrameParser != null)
            {
                portFrameParser.ParseData();
            }
            else
            {
                UtMessageBase.ShowOneMessage("临时测试", string.Format("“{0}”在未完成初始化前收到数据", Name), PopupMessageType.Info, 0);
            }
        }

        #endregion

        #region 协议线程的处理过程

        bool threadRunning = true;

        /// <summary>
        /// 协议运行的线程执行体。
        /// </summary>
        void ProtocolThreadProc()
        {
            if (ProtocolThreadIsAlive)
            {
                CallEventProtocoMessageOccured(string.Format("协议“{0}”的协议线程已启动。", Name));

                try
                {
                    Initialize();
                }
                catch (Exception e)
                {
                    UtMessageBase.ShowOneMessage(string.Format("“{0}”异常。", Name), e.Message, PopupMessageType.Info, 0);
                }

                while (working && threadRunning && protocolThread != null && protocolThread.IsAlive)
                {
                    // 2、协议线程的事件处理
                    if (!waitingEvent.WaitOne(pollingInterval))
                    {
                        releasedEvent = ProtocolEvents.Polling;
                    }

                    if (Initialized)
                    {
                        switch (releasedEvent)
                        {
                            case ProtocolEvents.ProtocolExit:

                                threadRunning = false;
                                break;

                            case ProtocolEvents.TaskArrived:

                                // 接收数据（ProtocolEvents.PortReceivedData）与发送数据的处理不要在一个线程中，否则会卡死
                                ProcessNewTask();
                                break;

                            case ProtocolEvents.SendingCaseWaiting:

                                caseManager.ProcessCase();
                                break;

                            default:

                                // 协议的轮询处理
                                Polling();
                                break;
                        }
                    }

                    Thread.Sleep(0);
                }

                protocolThread = null;

                if (Port == null)
                {
                    channel.CallEventWorkStateSwitched();
                }
            }
        }

        virtual protected bool CheckMore()
        {
            if (HasTask(TaskState.NewTask))
            {
                ReleaseEent(ProtocolEvents.TaskArrived);
               
                return true;
            }

            return false;
        }

        virtual public void Polling()
        {
            ProcessAsyncTaskOvertime();
            caseManager.ProcessCase();
            OnPolling();
            CheckMore();
        }

        protected void ReleaseEent(ProtocolEvents ev)
        {
            releasedEvent = ev;
            waitingEvent.Set();
        }

        internal void AddTaskWaitTime(int milliseconds)
        {
            lock (taskList)
            {
                for (int i = TaskList.Count - 1; i >= 0 && TaskList.Count > 0; i--)
                {
                    TaskList[i].OtherTaskWaitTime += milliseconds;
                }
            }
        }

        protected bool HasTask(TaskState testState)
        {
            lock (taskList)
            {
                for (int i = TaskList.Count - 1; i >= 0 && TaskList.Count > 0; i--)
                {
                    if (TaskList[i].TaskState == testState)
                    {
                        return true;
                    }
                }

                return false;
            }
        }

        protected void ProcessNewTask()
        {
            try
            {
                ProtocolTask unprocessedTask = PickNewTask();

                while (unprocessedTask != null)
                {
                    ProcessTask(unprocessedTask);
                    Thread.Sleep(0);
                    unprocessedTask = PickNewTask();
                }

                CheckMore();
            }
            catch (Exception ee)
            {
                UtMessageBase.ShowOneMessage(string.Format("协议{0}处理任务异常:{1}", this.Name, ee.ToString()), PopupMessageType.Exception);
            }
        }

        private ProtocolTask PickNewTask()
        {
            lock (TaskList)
            {
                for (int i = TaskList.Count - 1; i >= 0 && TaskList.Count > 0; i--)
                {
                    if (TaskList[i].TaskState == TaskState.NewTask)
                    {
                        return TaskList[i];
                    }
                }
            }

            return null;
        }

        /// <summary>
        ///  判断异步任务是否超时并处理。
        ///  返回新任务数量
        /// </summary>
        /// <returns>新任务数量</returns>
        protected int ProcessAsyncTaskOvertime()
        {
            int newTaskCount = 0;

            try
            {
                lock (taskList)
                {
                    for (int i = taskList.Count - 1; i >= 0; i--)
                    {
                        if (taskList[i].Dead)
                        {
                            taskList.RemoveAt(i);
                        }
                        else if (taskList[i].TaskState == TaskState.NewTask)
                        {
                            newTaskCount++;
                        }
                        else if (taskList[i].TaskOvertime(portFrameParser.ParseTimeUsed))
                        {
                            UtMessageBase.ShowOneMessage(string.Format("任务:{0}超时", taskList[i].Name), PopupMessageType.Info);
                            caseManager.SetCaseAsDead(taskList[i]);
                            taskList[i].Complete(TaskState.TaskOvertime);
                            taskList.RemoveAt(i);
                        }
                    }
                }
            }
            catch (Exception e)
            {
                UtMessageBase.ShowOneMessage("ProcessAsyncTaskOvertime", e.ToString(), PopupMessageType.Info, 0);
            }

            return newTaskCount;
        }

        /// <summary>
        /// 是否已经完成出生化
        /// 考虑到子类可能并没有调到父类的Initialize函数，此值的缺省值为true
        /// 子类在override函数Initialize时应先将此值设置成false，或调用子类的UnInitialize
        /// </summary>
        protected bool Initialized = true;

        /// <summary>
        /// 协议的初始化工作可以重载此函数来实现。
        /// 协议有一个自己的运作协议线程，当该线程开始运行时会调用此函数。
        /// （因此如果您需要在协议中创建自己的线程或窗体等，可以重载此函数来实现）。
        /// 通过重载UnInitialize()来释放资源。基类中此函数未做任何工作。
        /// </summary>
        protected virtual void Initialize()
        {
            Initialized = true;

            // 处理在初始化过程中被阻止的任务
            ReleaseEent(ProtocolEvents.TaskArrived); 
        }

        /// <summary>
        /// 协议退出时的工作可以重载此函数来实现。
        /// 注：协议有一个自己的运作协议线程，当该线程在退出前会调用此函数。基类中此函数未做任何工作。
        /// </summary>
        protected virtual void UnInitialize() 
        {
            Initialized = false;
        }

        /// <summary>
        /// 重新初始化
        /// </summary>
        public void Reinitialize()
        {
            UnInitialize();
            Initialize();
        }

        /// <summary>协议线程轮询调用的函数。
        /// 线程每隔一段时间将调用此函数，时间间隔可以通过pollingInterval设置，该时间间隔默认值为100毫秒。
        /// </summary>
        protected virtual void OnPolling() { }

        /// <summary>
        /// 协议一收到任务，此函数将被调用。
        /// </summary>
        /// <param name="newTask">收到的任务。</param>
        protected virtual void ProcessTask(ProtocolTask newTask)
        {
            newTask.SetAsBeProccessing();
        }

        /// <summary>
        /// 处理接收的数据。
        /// </summary>
        protected virtual void ProcessFrame(FrameBase frame) { }

        /// <summary>
        /// 新增此函数而不改变ProcessFrame的访问级别从而保证老版本规约可以编译过去
        /// </summary>
        /// <param name="frame"></param>
        public void ProcessReceivedFrame(FrameBase frame)
        {
            // 收到新的帧，保证通讯状态为 Working
            // 对应 CaseBase.ProcessOvertime 方法
            CommReceDataState = DataCommunicateState.Working;
            ProcessFrame(frame);
        }

        #endregion

        /// <summary>
        /// 端口已经切换响应的函数。
        /// </summary>
        /// <param name="oldPort">切换前的通讯端口。</param>
        protected virtual void OnPortSwitched(ICommPort oldPort) { }

        /// <summary>协议状态改变后将调用此函数。
        /// </summary>
        /// <param name="currentState">协议当前状态。</param>
        /// <param name="oldState">改变前的协议状态。</param>
        protected virtual void OnProtocolStateChanged(ProtocolState currentState, ProtocolState oldState) { }

        /// <summary>获取或设置协议状态。
        /// 在设置时若与之前的状态不同则将触发协议状态改变事件。
        /// </summary>
        protected ProtocolState ProtocolStateAccessor
        {
            get 
            {
                return ProtocolState;
            }

            set
            {
                ProtocolState oldState = ProtocolState;
                ProtocolState = value;

                if (oldState != ProtocolState)
                {
                    CallEventProtocolStateChanged(ProtocolState, oldState);
                }
            }
        }

        public ReadOnlyCollection<string> FunctionNames
        {
            get { return functionNames == null ? null : Array.AsReadOnly<string>(functionNames); }
        }

        public bool ContainsFunction(string functionName)
        {
            if (functionNames != null)
            {
                for (int i = 0; i < functionNames.Length; i++)
                {
                    if (string.Compare(functionNames[i], functionName, true) == 0)
                    {
                        return true;
                    }
                }
            }

            return false;
        }

        #region 事件的启动

        void CallEventBytesSent(byte[] buffer, int offset, int size)
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
                UtMessageBase.ShowOneMessage(string.Format("协议完成一组byte数据的发送的响应事件执行异常：{0}{1}", Channel.EventHandleInvalidError, ex.ToString()), PopupMessageType.Exception);
            }
        }

        void CallEventBytesReceived(byte[] buffer, int offset, int size)
        {
            try
            {
                if (bytesReceived != null)
                {
                    bytesReceived(this, buffer, offset, size);
                }
            }
            catch (Exception ex)
            {
                UtMessageBase.ShowOneMessage(string.Format("协议处理一组byte数据的接收的响应事件执行异常：{0}{1}", Channel.EventHandleInvalidError, ex.ToString()), PopupMessageType.Exception);
            }
        }

        void CallEventFrameSent(FrameBase frame)
        {
            try
            {
                frameSent?.Invoke(this, frame);
            }
            catch (Exception ex)
            {
                UtMessageBase.ShowOneMessage(string.Format("协议完成一个帧的发送的响应事件执行异常：{0}{1}", Channel.EventHandleInvalidError, ex.ToString()), PopupMessageType.Exception);
            }
        }

        internal void CallEventFrameRecieved(FrameBase frame)
        {
            try
            {
                if (frameReceived != null)
                {
                    frameReceived(this, frame);
                }
            }
            catch (Exception ex)
            {
                UtMessageBase.ShowOneMessage(string.Format("协议完成一个帧的接收的响应事件执行异常：{0}{1}", Channel.EventHandleInvalidError, ex.ToString()), PopupMessageType.Exception);
            }
        }

        public void CallBytesSendReceiveThrow(BytesType type, byte[] bytes, string describe)
        {
            try
            {
                if (bytesSendReceiveThrow != null)
                {
                    bytesSendReceiveThrow.Invoke(this, DateTime.Now, type, bytes, describe);
                }
            }
            catch (Exception ex)
            {
                UtMessageBase.ShowOneMessage(string.Format("协议完成一个帧的接收的响应事件执行异常：{0}{1}", Channel.EventHandleInvalidError, ex.ToString()), PopupMessageType.Exception);
            }
        }

        protected void CallRec(FrameBase frame,string desc)
        {
            CallBytesSendReceiveThrow(BytesType.Receive, frame.FrameBytes, desc);
        }

        protected void CallThrow(FrameBase frame, string desc)
        {
            CallBytesSendReceiveThrow(BytesType.Throw, frame.FrameBytes, desc);
        }

        void CallEventProtocolStateChanged(ProtocolState currentState, ProtocolState oldState)
        {
            try
            {
                OnProtocolStateChanged(currentState, oldState);

                if (protocolStateChanged != null)
                {
                    protocolStateChanged(this, currentState, oldState);
                }
            }
            catch (Exception ex)
            {
                UtMessageBase.ShowOneMessage(string.Format("协议状态改变的响应事件执行异常：{0}{1}", Channel.EventHandleInvalidError, ex.ToString()), PopupMessageType.Exception);
            }
        }

        void CallEventPortSwitched(ICommPort oldPort)
        {
            try
            {
                OnPortSwitched(oldPort);

                if (portSwitched != null)
                {
                    portSwitched(this, oldPort);
                }
            }
            catch (Exception ex)
            {
                UtMessageBase.ShowOneMessage(string.Format("协议的通讯端口切换的响应事件执行异常：{0}{1}", Channel.EventHandleInvalidError, ex.ToString()), PopupMessageType.Exception);
            }
        }

        /// <summary>
        /// 调用此函数将触发ProtocolMessageOccured事件。
        /// </summary>
        /// <param name="messageText">信息文本。</param>
        public void CallEventProtocoMessageOccured(string messageText)
        {
            try
            {
                if (NeedPort && Port != null)
                {
                    Port.WriteDebugInfo(messageText, false);
                }
				
                if (protocolMessageOccured != null)
                {
                    protocolMessageOccured(this, messageText);
                }
            }
            catch (Exception ex)
            {
                UtMessageBase.ShowOneMessage(string.Format("协议一般信息产生的响应事件执行异常：{0}{1}", Channel.EventHandleInvalidError, ex.ToString()), PopupMessageType.Exception);
            }
        }

        protected void CallEventProtocolProcessActivelyOccured(object sender, object[] datas)
        {
            try
            {
                protocolProcessActivelyOccured?.Invoke(sender, datas);
            }
            catch (Exception ex)
            {
                UtMessageBase.ShowOneMessage(string.Format("协议主动与应用程序通讯的事件执行异常：{0}{1}", Channel.EventHandleInvalidError, ex.ToString()), PopupMessageType.Exception);
            }
        }

        #endregion
    }

    /// <summary>
    /// 协议线程阻塞而等待要处理的事件。
    /// </summary>
    public enum ProtocolEvents : int
    {
        /// <summary>
        /// 退出协议。
        /// </summary>
        ProtocolExit = 0,
        /// <summary>
        /// 收到应用发送的任务。
        /// </summary>
        TaskArrived,
        /// <summary>
        /// 还有等待发送到数据
        /// </summary>
        SendingCaseWaiting,
        /// <summary>
        /// Polling时间
        /// </summary>
        Polling
    }
}
