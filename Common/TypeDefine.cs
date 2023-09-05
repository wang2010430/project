/***************************************************************************************************
* copyright : 芯迈微半导体（珠海）有限公司
* version   : 1.00
* file      : TypeDefine.cs
* date      : 2023/03/16
* author    : haozhe.ni
* brief     : 公共枚举类
* section Modification History
* - 1.0 : Initial version - haozhe.ni
***************************************************************************************************/

using System.ComponentModel;

namespace Common
{
    public enum PopupMessageType
    {
        /// <summary>
        /// 一般提示信息
        /// </summary>
        Info,
        /// <summary>
        /// 重要提示信息
        /// </summary>
        ImportantInfo,
        /// <summary>
        /// 警告信息
        /// </summary>
        Warning,
        /// <summary>
        /// 通知
        /// </summary>
        Notice,
        /// <summary>
        /// 开始进行某种操作
        /// </summary>
        OperationStart,
        /// <summary>
        /// 操作结束
        /// </summary>
        OperationEnd,
        /// <summary>
        /// 运行错误信息
        /// </summary>
        Error,
        /// <summary>
        /// 扑捉到的例外
        /// </summary>
        Exception,
        /// <summary>
        /// 无效的操作
        /// </summary>
        InValidOperation,
        /// <summary>
        /// 顶层捕捉到的异常
        /// </summary>
        TopException
    }

    /// <summary>
    /// 数据包类型
    /// </summary>
    public enum BytesType
    {
        /// <summary>
        /// 发送
        /// </summary>
        Send,

        /// <summary>
        /// 接收
        /// </summary>
        Receive,

        /// <summary>
        /// 丢弃
        /// </summary>
        Throw,
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

    public enum OsFamily
    {
        Unknown,
        Windows,
        Uniux
    };

    public enum ByteOrder
    {
        /// <summary>
        /// 高前低后
        /// </summary>
        [Description("高前低后")]
        HighLow,

        /// <summary>
        /// 低前高后
        /// </summary>
        [Description("低前高后")]
        LowHigh,
    }
}
