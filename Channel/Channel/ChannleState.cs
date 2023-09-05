/***************************************************************************************************
* copyright : 芯迈微半导体（珠海）有限公司
* version   : 1.00
* file      : ChannleState.cs
* date      : 2023/03/16
* author    : haozhe.ni
* brief     : ChannleState
* section Modification History
* - 1.0 : Initial version - haozhe.ni
***************************************************************************************************/

using Common;
using System;

namespace Channel
{
    /// <summary>
    /// 通道工作状态发生改变事件
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="state"></param>
    public delegate void WorkStateChangeEventHandler(object sender, ChannelState state);

    public delegate void WorkStateSwitchedEventHandler();

    /// <summary>
    /// 工作通道状态类
    /// </summary>
    public class ChannelState
    {
        internal Channel channel;

        event WorkStateSwitchedEventHandler workStateSwitched;

        public event WorkStateSwitchedEventHandler WorkStateSwitched
        {
            add
            {
                workStateSwitched += value;
            }

            remove
            {
                workStateSwitched -= value;
            }
        }

        /// <summary>
        /// 通讯端口是否打开
        /// </summary>
        private bool _isPortOpened;

        public bool IsPortOpened
        {
            get
            {
                if (channel != null)
                {
                    _isPortOpened = (channel.Protocol != null) && (channel.Protocol.Port != null) && (channel.Protocol.Port.PortState != PortStatus.Closed);
                }

                return _isPortOpened;
            }

            set
            {
                _isPortOpened = value;
            }
        }

        /// <summary>
        /// 是否建立连接
        /// </summary>
        private bool _isConnected;

        public bool IsConnected
        {
            get
            {
                if (channel != null)
                {
                    _isConnected = (channel.Protocol != null) && (channel.Protocol.Port != null) && channel.Protocol.Port.IsConnected;
                }

                return _isConnected;
            }

            set
            {
                _isConnected = value;
            }
        }

        /// <summary>
        ///  数据发送状态
        /// </summary>
        private DataCommunicateState _sendDataState = DataCommunicateState.Working;

        public DataCommunicateState SendDataState
        {
            get
            {
                return _sendDataState;
            }

            set
            {
                DataCommunicateState oldState = _sendDataState;

                _sendDataState = value;

                if (oldState != _sendDataState && oldState != DataCommunicateState.UnCheck)
                {
                    CallEventChannelStateChanged();
                }
            }
        }

        /// <summary>
        ///  数据接收状态
        /// </summary>
        private DataCommunicateState _receDataState = DataCommunicateState.Working;

        public DataCommunicateState ReceDataState
        {
            get
            {
                return _receDataState;
            }

            set
            {
                DataCommunicateState oldState = _receDataState;

                _receDataState = value;

                if (oldState != _receDataState && oldState != DataCommunicateState.UnCheck)
                {
                    CallEventChannelStateChanged();
                }
            }
        }

        public ChannelState()
        {

        }

        public ChannelState(Channel channel)
        {
            this.channel = channel;
        }

        void CallEventChannelStateChanged()
        {
            try
            {
                workStateSwitched?.Invoke();
            }
            catch (Exception ex)
            {
                UtMessageBase.ShowOneMessage(string.Format("通道状态响应事件执行异常：{0}{1}", Channel.EventHandleInvalidError, ex), PopupMessageType.Exception);
            }
        }
    }

    /// <summary>
    /// 数据收发状态枚举
    /// </summary>
    public enum DataCommunicateState
    {
        /// <summary>
        ///  不检查， 如果数据收发状态为不检查,则不改变现有状态
        /// </summary>
        UnCheck,
        /// <summary>
        /// 正常工作中
        /// </summary>
        Working,
        /// <summary>
        /// 中断
        /// </summary>
        LoseConnect,
    }
}
