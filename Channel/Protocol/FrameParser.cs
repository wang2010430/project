/***************************************************************************************************
* copyright : 芯迈微半导体（珠海）有限公司
* version   : 1.00
* file      : FrameParser.cs
* date      : 2023/03/16
* author    : haozhe.ni
* brief     : FrameParser
* section Modification History
* - 1.0 : Initial version - haozhe.ni
***************************************************************************************************/

using Common;
using System;
using System.Threading;

namespace Channel
{
    public delegate void VoidParameterRoutine();

    internal class FrameParser
    {
        ProtocolBase protocol;
        DateTime lastReceiveTime;
        private IEventWait parseWait;
        private bool parsing = false;
        internal FrameParser(ProtocolBase ptl)
        {
            protocol = ptl;
        }

        internal void StopParse()
        {
            parsing = false;

            if (parseWait != null)
            {
                parseWait.Set();
                parseWait.Close();
            }

            parseWait = null;
        }

        internal void StartParse()
        {
            StopParse();
            parsing = true;
            parseWait = AutoResetEventFactory.CreateAutoResetEvent(true);
            Thread st = new Thread(ParseToValidFrame);
            st.IsBackground = true;
            st.Start();
        }

        internal void ParseData()
        {
            if (parsing)
            {
                parseWait.Set();
            }
        }

        private FrameBase receivingFrame = null;

        private DateTime parseStartTime;

        public int ParseTimeUsed
        {
            get
            {
                if (receivingFrame == null)
                {
                    return 0;
                }

                return (int)(DateTime.Now - parseStartTime).TotalMilliseconds;
            }
        }

        /// <summary>
        /// 允许帧内接收数据包之间的间隔（毫秒），超过该时间即认定超时
        /// </summary>
        private int dataIntervalAllowd = 5000;

        private void ParseToValidFrame()
        {
            TimeSpan intervalAllowed = new TimeSpan(dataIntervalAllowd);
            ResultOfParsingFrame resultValue = ResultOfParsingFrame.WaitingForNextData;

            while (parsing && protocol != null && protocol.ProtocolIsRunning)
            {
                FrameBase retFrame = null;

                try
                {
                    if (protocol.Port != null)
                    {
                        if (protocol.Port.HasData)
                        {
                            if (receivingFrame == null)
                            {
                                receivingFrame = protocol.CreateNewFrameToReceiveData();
                                parseStartTime = DateTime.Now;
                            }

                            int readSize;
                            lastReceiveTime = DateTime.Now;
                            resultValue = receivingFrame.ParseToFrame(protocol.Port.Peek(protocol.Port.GetCacheSize()), out readSize);
                            receivingFrame.ReadDataLength = readSize;
                            lastReceiveTime = DateTime.Now;

                            switch (resultValue)
                            {
                                case ResultOfParsingFrame.ReceivingCompleted:

                                    retFrame = receivingFrame;
                                    protocol.AddTaskWaitTime(ParseTimeUsed); //延长任务的等待时间
                                    receivingFrame = null;

                                    if (protocol.ProcessFrameInSeperateThread)
                                    {
                                        VoidParameterRoutine processFrameThread = delegate ()
                                        {
                                            protocol.CallEventFrameRecieved(retFrame);

                                            // 查看接收到的数据是否是应答，如果是则查找出等待该应答的任务，并结束该任务。
                                            if (!protocol.caseManager.ProcessFrame(retFrame))
                                            {
                                                protocol.ProcessReceivedFrame(retFrame);
                                            }
                                        };

                                        processFrameThread();
                                    }
                                    else
                                    {
                                        protocol.CallEventFrameRecieved(retFrame);

                                        if (!protocol.caseManager.ProcessFrame(retFrame))
                                        {
                                            protocol.ProcessReceivedFrame(retFrame);
                                        }
                                    }

                                    break;

                                case ResultOfParsingFrame.FormatNotMatched:

                                    protocol.CallEventProtocoMessageOccured(string.Format("协议“{0}”收到不匹配的帧数据（{1}）。", protocol.Name, FrameBase.GetBytesText(protocol.Port.Peek(readSize))));
                                    receivingFrame = null;
                                    break;

                                case ResultOfParsingFrame.ControlCheckError:

                                    protocol.CallEventProtocoMessageOccured(string.Format("协议“{0}”收到的帧控制部分校验错误（{1}）。", protocol.Name, FrameBase.GetBytesText(protocol.Port.Peek(readSize))));
                                    receivingFrame = null;
                                    break;

                                case ResultOfParsingFrame.CrcCheckError:

                                    protocol.CallEventProtocoMessageOccured(string.Format("协议“{0}”收到的帧校验错误（{1}）。", protocol.Name, FrameBase.GetBytesText(protocol.Port.Peek(readSize))));
                                    receivingFrame = null;
                                    break;

                                case ResultOfParsingFrame.ReceivedPartHead:
                                    break;

                                case ResultOfParsingFrame.WaitingForNextData:
                                    break;

                                case ResultOfParsingFrame.ReceivedOverTime:

                                    protocol.CallEventProtocoMessageOccured(string.Format("协议“{0}”收到的帧超时（{1}）。", protocol.Name, FrameBase.GetBytesText(protocol.Port.Peek(readSize))));
                                    receivingFrame = null;
                                    break;

                                default: break;
                            }

                            if (resultValue != ResultOfParsingFrame.ReceivingCompleted && readSize > 0)
                            {//丢弃的报文
                                byte[] data = protocol.Port.Peek(readSize);
                                string desc = "";

                                if (resultValue == ResultOfParsingFrame.FormatNotMatched)
                                {
                                    desc = "帧格式错误";
                                }
                                else if (resultValue == ResultOfParsingFrame.CrcCheckError)
                                {
                                    desc = "校验码校验错误";
                                }
                                else if (resultValue == ResultOfParsingFrame.ReceivedOverTime)
                                {
                                    desc = "接收超时";
                                }

                                protocol.CallBytesSendReceiveThrow(BytesType.Throw, data, string.Format(desc, data[0]));
                            }

                            protocol.Port.RemoveData(readSize);
                        }
                        else
                        {
                            if (receivingFrame != null && DateTime.Now - lastReceiveTime > intervalAllowed)
                            {
                                UtMessageBase.ShowOneMessage("ParseToValidFrame()", "接收帧数据超时，数据做废。", PopupMessageType.Info, 0);
                            }
                        }
                    }
                }
                catch (Exception ee)
                {
                    UtMessageBase.ShowOneMessage("ParseToValidFrame()", ee.ToString(), PopupMessageType.Info, 0);
                }

                try
                {
                    if (protocol.Port != null)
                    {
                        if (protocol.Port.HasData)
                        {
                            // 还有数据需要处理
                            Thread.Sleep(0);
                        }
                        else
                        {
                            if (parseWait != null)
                            {
                                parseWait.WaitOne((receivingFrame == null) ? -1 : dataIntervalAllowd + 100);
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    UtMessageBase.ShowOneMessage("ParseToValidFrame()", ex.ToString(), PopupMessageType.Info, 0);
                }
            }
        }
    }
}
