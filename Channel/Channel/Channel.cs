/***************************************************************************************************
* copyright : 芯迈微半导体（珠海）有限公司
* version   : 1.00
* file      : Channel.cs
* date      : 2023/03/16
* author    : haozhe.ni
* brief     : Channel
* section Modification History
* - 1.0 : Initial version - haozhe.ni
***************************************************************************************************/

using Common;
using System;

namespace Channel
{
    /// <summary>
    /// 协议切换事件委托。
    /// </summary>
    /// <param name="oldProtocol">切换前的协议对象。</param>
    public delegate void ProtocolSwitchedEventHandler(object sender, IProtocol oldProtocol);

    public class Channel
    {
        protected int channelId = 0;

        /// <summary>
        /// 获取通道编号。
        /// </summary>
        public int ChannelId
        {
            get { return channelId; }
        }

        private Guid guid = Guid.NewGuid();
        public Guid ChannelGuid
        {
            get
            {
                return guid;
            }
        }

        IProtocol _protocol;
        private string _name = "";

        /// <summary>
        /// 事件句柄无效错误。
        /// </summary>
        public const string EventHandleInvalidError = "注册的事件对象可能已经不存在，也可能是事件执行体自身错误。";

        /// <summary>
        /// 协议的由停止变成运行状态产生的事件。
        /// </summary>
        public event FlagEventHandler WorkStarted;

        /// <summary>
        /// 协议的由运行变成停止状态产生的事件。
        /// </summary>
        public event FlagEventHandler WorkStoped;

        /// <summary>
        /// 协议切换成功后将产生此事件。
        /// </summary>
        public event ProtocolSwitchedEventHandler ProtocolSwitched;

        /// <summary>
        /// 协议收发数据状态改变将产生此事件
        /// </summary>
        public event WorkStateChangeEventHandler WorkStateChanged;

        /// <summary>
        /// 屏蔽所有事件
        /// </summary>
        private bool _disableEvent;

        /// <summary>
        /// 屏蔽所有事件
        /// </summary>
        public bool DisableEvent
        {
            get
            {
                return _disableEvent;
            }

            set
            {
                _disableEvent = value;
            }
        }

        /// <summary>
        /// 通道状态
        /// </summary>
        private ChannelState _channelWorkState;

        public ChannelState ChannelWorkState
        {
            get
            {
                return _channelWorkState;
            }

            set
            {
                if (_channelWorkState == value)
                {
                    return;
                }

                ChannelState oldchannelWorkState = _channelWorkState;

                if (oldchannelWorkState != null)
                {
                    oldchannelWorkState.WorkStateSwitched -= CallEventWorkStateSwitched;

                    ChannelState temp = _channelWorkState;

                    if (temp != null)
                    {
                        temp.channel = null;
                    }
                }

                _channelWorkState = value;

                if (_channelWorkState == null)
                {
                    return;
                }

                _channelWorkState.WorkStateSwitched += CallEventWorkStateSwitched;

                ChannelState channel = _channelWorkState;

                if (channel != null)
                {
                    channel.channel = this;
                }
            }
        }

        private string _baseChannelName;

        /// <summary>
        /// 基础通道名，对于连接到TCPServer的Socket通道，就算TCPServer通道名
        /// </summary>
        public string BaseChanelName
        {
            get
            {
                return string.IsNullOrEmpty(_baseChannelName) ? _name : _baseChannelName;
            }

            set
            {
                _baseChannelName = value;
            }
        }

        private string _baseUniqueChannelName;

        /// <summary>
        /// 基础唯一通道名，对于连接到TCPServer的Socket通道，就算TCPServer通道名+连接段IP(不含端口号），Name则是包含端口号的
        /// </summary>
        public string BaseUniqueChanelName
        {
            get
            {
                return string.IsNullOrEmpty(_baseUniqueChannelName) ? _name : _baseUniqueChannelName;
            }

            set
            {
                _baseUniqueChannelName = value;
            }
        }

        public Channel()
        {

        }

        public Channel(string channelName)
        {
            if (channelName != null)
            {
                _name = channelName;
            }
        }

        /// <summary>
        /// 获取或设置通道名。
        /// </summary>
        public virtual string Name
        {
            get
            {
                return _name;
            }

            set
            {
                _name = value;
            }
        }

        /// <summary>
        /// 获取或设置通道运行的协议对象。
        /// </summary>
        public IProtocol Protocol
        {
            get
            {
                return _protocol;
            }

            set
            {
                if (_protocol == value)
                {
                    return;
                }


                IProtocol oldProtocol = _protocol;

                if (oldProtocol != null)
                {
                    oldProtocol.ProtocolStateChanged -= protocol_ProtocolStateChanged;

                    ProtocolBase temp = _protocol as ProtocolBase;

                    if (temp != null)
                    {
                        temp.channel = null;
                    }
                }

                _protocol = value;

                if (_protocol != null)
                {
                    _protocol.ProtocolStateChanged += protocol_ProtocolStateChanged;

                    ProtocolBase temp = _protocol as ProtocolBase;

                    if (temp != null)
                    {
                        temp.channel = this;
                    }
                }

                // 通知相关事件
                CallEventProtocolSwitched(oldProtocol);

                if ((oldProtocol == null || !oldProtocol.ProtocolIsRunning) && IsRunning)
                {
                    CallEventWorkStateSwitched();
                }

                if ((oldProtocol != null && oldProtocol.ProtocolIsRunning) && !IsRunning)
                {
                    CallEventWorkStateSwitched();
                }
            }
        }

        void protocol_ProtocolStateChanged(object sender, ProtocolState currentState, ProtocolState oldState)
        {
            if (currentState == ProtocolState.PortDisconnected || oldState == ProtocolState.PortDisconnected)
            {
                CallEventWorkStateSwitched();
            }
        }

        /// <summary>
        /// 通道是否正在运行。
        /// </summary>
        public virtual bool IsRunning
        {
            get { return _protocol != null && _protocol.ProtocolIsRunning; }
        }

        #region 通道的启动与关闭
        /// <summary>
        /// 启动通道运行。
        /// </summary>
        public virtual bool StartWork()
        {
            if (_protocol == null)
            {
                return false;
            }

            ChannelWorkState = new ChannelState(this);

            _protocol.StartWork();

            return _protocol.ProtocolIsRunning;
        }

        /// <summary>
        /// 停止通道。它将停止协议的运行。
        /// </summary>
        /// <param name="waitForStop">它表示是否阻塞当前线程，直至通道停止工作。（阻塞时间不会超过1秒）</param>
        public virtual void StopWork(bool waitForStop)
        {
            if (_protocol != null)
            {
                _protocol.StopWork(waitForStop);
            }
        }
        #endregion

        /// <summary>
        /// 同步执行任务。若任务执行超时则返回false，否则返回true。
        /// </summary>
        /// <param name="task">要执行的任务。</param>
        /// <param name="timeout">执行任务的等待时限毫秒（值为-1表示无限时）。</param>
        public virtual bool ExecuteTaskSync(ProtocolTask task, int timeout)
        {
            return _protocol != null && _protocol.ExecuteTaskSync(task, timeout);
        }

        /// <summary>
        /// 异步执行任务。
        /// </summary>
        /// <param name="task">传递的对象。</param>
        /// <param name="asyncCallback">异步回调对象。</param>
        /// <param name="timeout">执行任务的时限毫秒（值为-1表示无限时），若超时将调用asyncCallback的回调函数。</param>
        public virtual void ExecuteTaskAsync(ProtocolTask task, AsyncCallback asyncCallback, int timeout)
        {
            if (_protocol != null)
            {
                _protocol.ExecuteTaskAsync(task, asyncCallback, timeout);
            }
        }

        #region 事件的启动
        internal void CallEventWorkStarted()
        {
            try
            {
                if (WorkStarted != null && !_disableEvent)
                {
                    WorkStarted(this);
                }
            }
            catch (Exception ex)
            {
                UtMessageBase.ShowOneMessage(string.Format("通道启动的响应事件执行异常：{0}{1}", EventHandleInvalidError, ex), PopupMessageType.Exception);
            }
        }

        internal void CallEventWorkStoped()
        {
            try
            {
                if (WorkStoped != null && !_disableEvent)
                {
                    WorkStoped(this);
                }
            }
            catch (Exception ex)
            {
                UtMessageBase.ShowOneMessage(string.Format("通道停止工作的响应事件执行异常：{0}{1}", EventHandleInvalidError, ex), PopupMessageType.Exception);
            }
        }

        void CallEventProtocolSwitched(IProtocol oldProtocol)
        {
            try
            {
                if (ProtocolSwitched != null && !_disableEvent)
                {
                    ProtocolSwitched(this, oldProtocol);
                }
            }
            catch (Exception ex)
            {
                UtMessageBase.ShowOneMessage(string.Format("通道协议切换的响应事件执行异常：{0}{1}", EventHandleInvalidError, ex), PopupMessageType.Exception);
            }
        }

        /// <summary>
        /// 通道收发状态切换事件
        /// </summary>
        public void CallEventWorkStateSwitched()
        {
            try
            {
                if (WorkStateChanged != null && ChannelWorkState != null && !_disableEvent)
                {
                    WorkStateChanged(this, ChannelWorkState);
                }
            }
            catch (Exception ex)
            {
                UtMessageBase.ShowOneMessage(string.Format("通道状态切换响应事件执行异常：{0}{1}", EventHandleInvalidError, ex), PopupMessageType.Exception);
            }
        }
        #endregion
    }
}
