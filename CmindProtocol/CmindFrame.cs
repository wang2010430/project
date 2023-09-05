/***************************************************************************************************
* copyright : 芯迈微半导体（珠海）有限公司
* version   : 1.00
* file      : DLFrame.cs
* date      : 2023/04/27
* author    : haozhe.ni
* brief     : 
* section Modification History
* - 1.0 : Initial version - haozhe.ni
***************************************************************************************************/

using Channel;
using Common;
using log4net;
using System;
using System.Linq;

namespace CmindProtocol
{
    public class CmindFrame : FrameBase
    {
        /// <summary>
        /// 帧解析
        /// </summary>
        /// <param name="bytes">帧字节</param>
        /// <param name="readSize">读取字节数</param>
        /// <returns>帧解析结果</returns>
        public override ResultOfParsingFrame ParseToFrame(byte[] bytes, out int readSize)
        {
            try
            {
                readSize = 0;

                if (bytes.Length == 0 || (bytes[0] != 0x7f && bytes[0] != 0x76))
                {
                    readSize = bytes.Length;
#if DEBUG
                    LogNetHelper.Error("Frame is not Matched");
#endif
                    return ResultOfParsingFrame.FormatNotMatched;
                }

                if(bytes[0] == 0x76)
                {
                    if (bytetime.Over)
                    {
                        readSize = bytes.Length;
                        return ResultOfParsingFrame.ReceivedOverTime;
                    }

                    if (bytes.Length >= 4)
                    {
                        if((bytes[1] == 0x80)&& (bytes[2] == 0x86) && (bytes[3] == 0x7C))
                        {
                            IsHand = true;
                            readSize = 4;
                            ResizeFrameBytes(readSize);
                            Array.Copy(bytes, 0, frameBytes, 0, readSize);
                            return ResultOfParsingFrame.ReceivingCompleted;
                        }
                        else
                        {
                            readSize = bytes.Length;
#if DEBUG
                            LogNetHelper.Error("No Matched");
#endif
                            return ResultOfParsingFrame.FormatNotMatched;
                        }
                    }
                    else
                    {
                        bytetime.Reset();
                        bytetime.Start();
                        return ResultOfParsingFrame.WaitingForNextData; 
                    }
                }

                if (parsingState == ParsingState.ParsingHead)
                {
                    bytetime.Reset();
                    bytetime.Start();

                    parsingState = ParsingState.ParsingControl;
                }
                else
                {//超时判断
                    if (bytetime.Over)
                    {
#if DEBUG
                        LogNetHelper.Error("Over Time");
#endif 
                        readSize = bytes.Length;
                        return ResultOfParsingFrame.ReceivedOverTime;
                    }
                }

                if (bytes.Length < 19)
                {
                    return ResultOfParsingFrame.WaitingForNextData;
                }

                if (bytes[1] != 0x7f || bytes[2] != 0x7f || bytes[3] != 0x7f)
                {
                    readSize = bytes.Length;
#if DEBUG
                    LogNetHelper.Error("Frame is wrong");
#endif
                    return ResultOfParsingFrame.FormatNotMatched;
                }

                int totalLen = DataConvert.ByteToInt(bytes, 4, CmindCommon.DataEndian) + 8;

                if (bytes.Length < totalLen)
                {
                    return ResultOfParsingFrame.WaitingForNextData;
                }

                if ((bytes[10] & 0x80) > 0 && bytes.Length == 20)
                {
                    readSize = totalLen;
                    // 错误应答，长度为20
#if DEBUG
                    LogNetHelper.Error("Wrong Answer!");
#endif
                    return ResultOfParsingFrame.FormatNotMatched;
                }

                readSize = totalLen;
                ResizeFrameBytes(readSize);
                Array.Copy(bytes, 0, frameBytes, 0, readSize);

                return ResultOfParsingFrame.ReceivingCompleted;
            }
            catch (Exception ex)
            {
                throw new ApplicationException("在解析通讯端口收到的数据时，发生异常：" + ex.Message);
            }
        }

        public bool IsHand = false;

        /// <summary>
        /// 数据区长度
        /// </summary>
        public int DataLen
        {
            get
            {
                return DataConvert.ByteToInt(FrameBytes, 4, CmindCommon.DataEndian);
            }
            set
            {
                Array.Copy(DataConvert.IntToByte(value, CmindCommon.DataEndian), 0, frameBytes, 4, 4);
            }
        }

        public byte Pad1
        {
            get
            {
                return FrameBytes[8];
            }
            set
            {
                ResizeFrameBytes(19);
                FrameBytes[8] = value;
            }
        }

        /// <summary>
        /// 远方类型
        /// </summary>
        internal OperationType OperationType
        {
            get
            {
                foreach (int val in Enum.GetValues(typeof(OperationType)))
                {
                    if (val == FrameBytes[9])
                    {
                        return (OperationType)val;
                    }
                }

                return OperationType.UnKnown;
            }
            set
            {
                ResizeFrameBytes(19);
                FrameBytes[9] = (byte)value;
            }
        }

        /// <summary>
        /// 命令码
        /// </summary>
        public byte Command
        {
            get
            {
                return frameBytes[10];
            }
            set
            {
                ResizeFrameBytes(19);
                frameBytes[0] = 0x7F;
                frameBytes[1] = 0x7F;
                frameBytes[2] = 0x7F;
                frameBytes[3] = 0x7F;
                frameBytes[10] = value;
                DataLen = 11;
                //RemoteType = RemoteType.Platform;
            }
        }

        /// <summary>
        /// 数据区
        /// </summary>
        public byte[] Data
        {
            get
            {
                return FrameBytes.Skip(19).ToArray();
            }
            set
            {
                ResizeFrameBytes(19 + value.Length);
                Array.Copy(value, 0, frameBytes, 19, value.Length);
                DataLen = value.Length + 11;
            }
        }

        protected override int GetControlLength()
        {
            return -1;
        }

        protected override int GetFrameLength(byte[] bytes)
        {
            return -1;
        }

        /// <summary>
        /// 配置Hand帧
        /// </summary>
        public void SetHandFrame()
        {
            ResizeFrameBytes(4);
            frameBytes[0] = 0xFE;
            frameBytes[1] = 0xFE;
            frameBytes[2] = 0xFE;
            frameBytes[3] = 0xFE;

            Desc = "Hand";
        }
    }
}