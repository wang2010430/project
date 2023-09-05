/***************************************************************************************************
* copyright : 芯迈微半导体（珠海）有限公司
* version   : 1.00
* file      : BusinessCase.cs
* date      : 2023/03/16
* author    : haozhe.ni
* brief     : BusinessCase
* section Modification History
* - 1.0 : Initial version - haozhe.ni
***************************************************************************************************/

using Common;
using System;
using System.Collections.Generic;

namespace Channel
{
    /// <summary>业务管理器。
	/// </summary>
    public sealed class CaseManager
    {
        readonly List<CaseBase> caseList = new List<CaseBase>();

        internal IList<CaseBase> BusinessCaseList
        {
            get { return caseList.AsReadOnly(); }
        }

        /// <summary>添加Case对象到发送列表中。
		/// </summary>
        /// <param name="caseObj">业务对象。</param>
        internal void Add(CaseBase caseObj)
        {
            if (caseObj != null && !caseList.Contains(caseObj))
            {
                lock (caseList)
                {
                    caseList.Insert(0, caseObj);
                }
            }
        }

        /// <summary>在收发列表中清除业务对象。
        /// 当业务对象被设置死亡时会调用此函数。</summary>
        internal void Remove(CaseBase caseObj)
        {
            lock (caseList)
            {
                caseList.Remove(caseObj);
            }
        }

        public bool SetCaseAsDead(ProtocolTask task)
        {
            CaseBase csObj = null;

            foreach (CaseBase cs in caseList)
            {
                if (cs.Task == task)
                {
                    csObj = cs;
                    break;
                }
            }

            if (csObj != null)
            {
                csObj.Dead = true;
                return true;
            }

            return false;
        }

        /// <summary>
        /// 发送业务列表对象的所有要发送的帧。
		/// </summary>
        public void ProcessCase()
        {
            try
            {
                lock (caseList)
                {
                    for (int i = caseList.Count - 1; i >= 0 && caseList.Count > 0; i--)
                    {
                        CaseBase cs = caseList[i];

                        if (cs.PollingProcessable)
                        {
                            cs.InternalPolling();

                            if (cs.Sender != null)
                            {
                                cs.Sender.Send();
                            }
                        }
                    }
                }
            }
            catch
            {
            }
        }

        /// <summary>
        /// 处理收到的帧对象。
        /// </summary>
        /// <param name="receivedFrame">收到的帧对象。</param>
        internal bool ProcessFrame(FrameBase receivedFrame)
        {
            try
            {
                lock (caseList)
                {
                    for (int i = caseList.Count - 1; i >= 0 && caseList.Count > 0; i--)
                    {
                        if (caseList[i].ProcessFrame(receivedFrame))
                        {
                            return true;
                        }
                    }
                }
            }
            catch (Exception e)
            {
                UtMessageBase.ShowOneMessage(e.ToString(), PopupMessageType.Info);
            }

            return false;
        }
    }

    /// <summary>业务类。
    /// 它负责处理一个完整的业务（因此命名为Case），与业务相关的所有帧之间的前后逻辑关系都由此类控制完成
    /// </summary>
    public abstract class CaseBase
    {
        /// <summary>
        /// 是否可以通过Polling来发送数据
        /// </summary>
        public bool PollingProcessable
        {
            get;
            set;
        }

        protected ProtocolBase ownerProtocol;
        FrameSender frameSender = null;
        bool caseIsDead = false;
        bool isJudgeChannelState = false;
        bool isReceivedData = false;

        protected int BusinessTimeOut = 1000;
        protected BusinessResult Result = null;

        /// <summary>
        /// 业务超时定时器
        /// </summary>
        protected TimeCounter timeCounter;

        /// <summary>，
        /// 重新设置定时器
        /// </summary>
        public void ResetTime()
        {
            if (timeCounter == null)
            {
                timeCounter = new TimeCounter();
                timeCounter.Timeout = BusinessTimeOut;
            }

            timeCounter.Reset();
            timeCounter.Start();
        }

        /// <summary>实例化业务对象。</summary>
        /// <param name="task">Case所要处理的任务。</param>
        /// <param name="ownerProtocol">业务所属的协议,不能为空。</param>
        protected CaseBase(ProtocolTask task, ProtocolBase ownerProtocol)
        {
            if (ownerProtocol == null)
            {
                throw new ApplicationException("初始化业务对象失败，所属的协议不能为空。");
            }
            Result = GetBusinessResultClass();
            PollingProcessable = true;
            Task = task;
            this.ownerProtocol = ownerProtocol;
            Dead = false;
            frameSender = new FrameSender(ownerProtocol, this);
        }

        protected bool SetDead(string msg = "")
        {
            return SetDead(false, msg);
        }

        protected bool SetDead(bool r, string msg = "")
        {
            Result.Set(r, msg);
            Dead = true;
            return true;
        }

        /// <summary> 获取处理的任务对象。
        /// </summary>
        public ProtocolTask Task { get; set; }

        /// <summary> 获取或设置此业务对象是否死亡（即无效）。
        /// </summary>
        public virtual bool Dead
        {
            get
            {
                return caseIsDead;
            }

            set
            {
                caseIsDead = value;

                if (value)
                {
                    ownerProtocol.RemoveCase(this);
                    Task.RetCallBack?.BeginInvoke(Result, null, null);
                }
                else
                {
                    ownerProtocol.AddCase(this);
                }

                DeadChange(value);
            }
        }

        protected virtual void DeadChange(bool status)
        {
            if (status && timeCounter != null)
            {
                timeCounter.Stop();
            }
        }

        /// <summary>
        /// pengwei 是否判断所在通道的通讯状态
        /// </summary>
        public virtual bool IsJudgeChannelState
        {
            get
            {
                return isJudgeChannelState;
            }

            set
            {
                isJudgeChannelState = value;
            }
        }

        /// <summary>
        /// 是否收到数据,若在发送的过程中能收到对方发送的数据则判断接收状态正常，
        /// </summary>
        public virtual bool IsReceivedData
        {
            get
            {
                return isReceivedData;
            }

            set
            {
                isReceivedData = value;
            }
        }

        protected virtual BusinessResult GetBusinessResultClass()
        {
            return new BusinessResult();
        }

        /// <summary>处理收到的帧对象。
        /// 若已处理该帧对象则返回true，否则返回false。</summary>
        /// <param name="receivedFrame">收到的帧对象。</param>
        public abstract bool ProcessFrame(FrameBase receivedFrame);

        /// <summary>
        /// 帧数据发送后，等待回复超时的处理
        /// </summary>
        public virtual void ProcessOvertime()
        {
            if (IsJudgeChannelState == true && IsReceivedData == false && frameSender != null && frameSender.TotalSendTimes != -1)
            {
                ownerProtocol.CommReceDataState = DataCommunicateState.LoseConnect;
            }

            Result.Set(false, string.Format("Timeout(SendNum:{0},OverTime:{1}ms)", Sender.TotalSendTimes, Sender.Interval));
            Dead = true;

            if (Task != null)
            {
                if (Task.Result == null)
                {
                    Task.Result = new TaskResult();
                }

                Task.Result.Success = false;
                Task.Result.Description = "任务等待帧回复超时。";
                Task.Complete();
            }
        }

        /// <summary>获取帧发送器。
        /// </summary>
        public virtual FrameSender Sender
        {
            get
            {
                return frameSender;
            }
        }

        /// <summary>帧在即将发送时将调用此函数。
        /// 在自动控制发送时才会调用此函数。
        /// </summary>
        /// <param name="frameBeSent">即将被发送的帧。</param>
        /// <param name="times">即将被发送的次数。</param>
        protected virtual void FrameBeginSend(FrameBase frameBeSent, int times) { }

        /// <summary>帧在发送成功后将调用此函数。
        /// 在自动控制发送时才会调用此函数。
        /// </summary>
        /// <param name="frameBeSent">即将被发送的帧。</param>
        /// <param name="times">即将被发送的次数。</param>
        protected virtual void FrameEndSend(FrameBase frameBeSent, int times) { }

        /// <summary>协议线程轮询调用的函数。
        /// 线程每隔一段时间将调用此函数，频率和协议的OnPolling()函数一样，
        /// 时间间隔可以通过协议类的pollingInterval设置，该时间间隔默认值为100毫秒。
        /// </summary>
        protected virtual void OnPolling()
        {
            if (timeCounter != null && timeCounter.Over)
            {
                Result.Set(false, string.Format("Request timeout({0}ms)", BusinessTimeOut));
                Dead = true;
            }
        }

        internal void InternalPolling()
        {
            OnPolling();
        }

        internal void InternalFrameBeginSend(FrameBase frameBeSent, int times)
        {
            FrameBeginSend(frameBeSent, times);
        }

        internal void InternalFrameEndSend(FrameBase frameBeSent, int times)
		{
			FrameEndSend(frameBeSent, times);
		}

        protected void CallRec(byte[] bytes, string describe)
        {
            ownerProtocol.CallBytesSendReceiveThrow(BytesType.Receive, bytes, describe);
        }

        protected void CallRec(FrameBase frame,string describe)
        {
            ownerProtocol.CallBytesSendReceiveThrow(BytesType.Receive, frame.FrameBytes, describe);
        }

    }

    /// <summary>帧发送器，控制帧的发送。
    /// 职责有：
    /// 1、自动定时发送（并未设定时器和单独线程）；
    /// 2、控制发送的次数；
    /// 3、若超时则会通知所属Case类。
    /// 注：它只负责发送，若要取消发送，则要在所属Case中设置。
    /// </summary>
    public sealed class FrameSender
    {
        int totalSendTimes = 3;
        FrameBase frame = null;
        readonly TimeCounter timeCounter;
        bool whetherSend = false;

        /// <summary>只用于超时时设置它为Dead状态。
        /// </summary>
        readonly CaseBase caseObj;

        /// <summary> 实例化帧发送器。</summary>
        /// <param name="protocol">要使用此协议对象发送数据。</param>
        internal FrameSender(ProtocolBase protocol, CaseBase caseObj)
        {
            this.Protocol = protocol;
            this.caseObj = caseObj;

            timeCounter = new TimeCounter();
        }

        /// <summary>获取或设置要发送的帧。
        /// </summary>
        public FrameBase FrameBeSent
        {
            get
            {
                return frame;
            }

            set
            {
                frame = value;
                SentTimes = 0;
            }
        }

        /// <summary>获取或设置发送间隔（默认值为3000毫秒）。
        /// </summary>
        /// <returns></returns>
        public int Interval
        {
            get
            {
                return timeCounter.Timeout;
            }

            set
            {
                timeCounter.Timeout = value > 0 ? value : 0;
            }
        }

        /// <summary>将当前的发送计时器设置为重新开始计时。
        /// </summary>
        public void ResetCalculagraph()
        {
            timeCounter.Reset();
            timeCounter.Start();
        }

        /// <summary>获取或设置已经发送的次数。
        /// </summary>
        public int SentTimes { get { return _sentTimes; } set { _sentTimes = value; } }
        int _sentTimes = 0;

        /// <summary>获取或设置要发送帧数据的次数。
        /// 若设置值为-1，表示不限制发送的次数。
        /// </summary>
        public int TotalSendTimes
        {
            get
            {
                return totalSendTimes;
            }

            set
            {
                totalSendTimes = value >= 0 ? value : -1;
            }
        }

        /// <summary> 开始发送帧数据。
        /// </summary>
        public bool BeginSend()
        {
            SentTimes = 0;
            whetherSend = true;

            return SendFrame();
        }

        public ProtocolBase Protocol { get; set; }

        public string Name
        {
            get
            {
                return Protocol.Name;
            }
        }

        /// <summary>立即发送一个帧，而不计算发送次数。
        /// 它不会影响FrameBeSent帧的发送。
        /// 发送成功则返回true.</summary>
        /// <param name="frame">要发送的帧对象。</param>
        public bool JustSendImmediately(FrameBase frame)
        {
            return Protocol.SendFrame(frame);
        }

        public void CallEventProtocoMessageOccured(string messageText)
        {
            Protocol.CallEventProtocoMessageOccured(messageText);
        }

        /// <summary>结束帧数据的发送。
        /// </summary>
        public void EndSend()
        {
            whetherSend = false;
        }

        bool SendFrame()
        {
            bool ret = false;
            caseObj.InternalFrameBeginSend(frame, SentTimes + 1);

            if (frame != null)
            {
                ret = Protocol.SendFrame(frame);

                if (ret)
                {
                    if (caseObj.Task != null)
                    {
                        caseObj.Task.CalculateSendOutTime();
                    }

                    caseObj.InternalFrameEndSend(frame, ++SentTimes);
                }
                else
                {
                    ++SentTimes;
                }

                timeCounter.Reset();
                timeCounter.Start();
            }

            return ret;
        }

        /// <summary>
        /// 在协议类的轮询中将执行此函数。
        /// </summary>
        internal void Send()
        {
            if (whetherSend && timeCounter.Over)
            {
                if (SentTimes > 0)
                {
                    // 超时，打日志
                    LogHelper.Log(string.Format("Send Timeout,Num:{0},Timeout:{1}ms,Desc:{2},Bytes:{3}",
                        SentTimes,
                        Interval,
                        frame.Desc,
                        StringHelper.BytesToString(frame.FrameBytes))
                        , LogMsgType.Notice);
                }

                if (SentTimes < totalSendTimes || totalSendTimes == -1)
                {
                    SendFrame();
                }
                else
                {
                    caseObj.ProcessOvertime();
                }
            }
        }
    }

    /// <summary>时限计时器
    /// </summary>
    public class TimeCounter : System.Diagnostics.Stopwatch
    {
        /// <summary>
        ///  时限，单位毫秒
        /// </summary>
        int timeout = 5000;

        /// <summary>
        /// 获得或设置时限（单位为毫秒）
        /// </summary>
        public int Timeout
        {
            get
            {
                return timeout;
            }

            set
            {
                timeout = value;
            }
        }

        /// <summary>
        /// 获取或设置是否已经超时。
        /// </summary>
        public bool Over
        {
            get
            {
                return ElapsedMilliseconds >= timeout;
            }
        }

        public TimeCounter()
        {

        }

        /// <summary>
        /// 实例化一个时限计时器。
        /// 对象默认状态为超时状态（即计时器值为0）。
        /// </summary>
        /// <param name="timeout">时限（单位毫秒）</param>
        public TimeCounter(int timeout)
        {
            this.timeout = timeout;
        }
    }
}
