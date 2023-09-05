/***************************************************************************************************
* copyright : 芯微半导体（珠海）有限公司
* version   : 1.00
* file      : FrameBase.cs
* date      : 2023/03/16
* author    : haozhe.ni
* brief     : FrameBase
* section Modification History
* - 1.0 : Initial version - haozhe.ni
***************************************************************************************************/

using System;
using System.Text;

namespace Channel
{
    /// <summary>
    /// 帧接收时，解析数据到一个阶段时返回的结果。
    /// </summary>
    public enum ResultOfParsingFrame
    {
        /// <summary>
        /// 帧超时
        /// </summary>
        ReceivedOverTime = -6,
        /// <summary>
        /// 已接收到部分数据，等待后面新接收到的数据。
        /// </summary>
        WaitingForNextData = -5,
        /// <summary>
        /// 收到了部分帧头数据。
        /// </summary>
        ReceivedPartHead = -4,
        /// <summary>
        /// 帧控制部分校验错误。
        /// </summary>
        ControlCheckError = -3,
        /// <summary>
        /// 帧数据校验错误。
        /// </summary>
        CrcCheckError = -2,
        /// <summary>
        /// 帧结构不匹配，如帧头不对。
        /// </summary>
        FormatNotMatched = -1,
        /// <summary>
        /// 接收到一个有效帧。
        /// </summary>
        ReceivingCompleted = 0
    }

    /// <summary>
    /// 帧基类。
    /// </summary>
    [Serializable()]
	public abstract class FrameBase
    {
        /// <summary>
        /// 当前解析状态（正在解析那一部分数据）。
        /// </summary>
        protected enum ParsingState
        {
            /// <summary>
            /// 正在解析帧头。
            /// </summary>
            ParsingHead,
            /// <summary>
            /// 正在解析帧控制部分。
            /// </summary>
            ParsingControl,
            /// <summary>
            /// 正在解析除帧头和控制部分以外的部分。
            /// </summary>
            ParsingRemainder,
            /// <summary>
            /// 帧解析完成，已接收到一个有效帧。
            /// </summary>
            ParsingFinished
        }

        /// <summary>
        /// 帧头数据。
        /// </summary>
		[NonSerialized()]
		protected byte[] frameHead;
        protected byte[] frameBytes;

        /// <summary>
        /// 计算校验的对象。若校验对象==null则表示不需要校验。
        /// </summary>
		[NonSerialized()]
		protected CrcCheckBase crcCheck = null;
        protected ParsingState parsingState = ParsingState.ParsingHead;

        /// <summary>
        /// 获得一个帧的数据(仅对发送数据有效)。
        /// </summary>
        public virtual byte[] FrameBytes
        {
            get { return frameBytes; }    
        }

        /// <summary>
        /// 读取数据帧长度（仅对接收数据有效）
        /// </summary>
        public int ReadDataLength
        {
            get;
            set;
        }

        /// <summary>
        /// 设置校验码字节数据。
        /// </summary>
        public virtual void SetCheckBytes()
        {
            if (crcCheck != null)
            {
                crcCheck.GetCheckCode(ref frameBytes);
            }
        }

        /// <summary>
        /// 可以对不同的口设定超时时间
        /// </summary>
        [NonSerialized()]
        protected TimeCounter bytetime = new TimeCounter(3000);

        /// <summary>
        /// 解析数据成一个帧。
        /// 接收到一个有效帧则返回0，不是该规约帧返回-1，帧数据校验不对则返回-2，无用的帧则返回-3，若返回值大于0则表示下一步要接收的字节长度。
        /// </summary>
        /// <param name="bytes">要解析的数据。</param>
        /// <param name="readSize">在bytes中从0开始要删除的数据长度。</param>
        public virtual ResultOfParsingFrame ParseToFrame(byte[] bytes, out int readSize)
        {
            try
            {
                readSize = 0;

                #region 为兼容而保留的代码
                // 需要有新的数据到来才会知道帧数据超时，新方法是在调用此函数的前即判断是否超时
                if (parsingState == ParsingState.ParsingHead)
                {
                    bytetime.Reset();
                    bytetime.Start();
                }
                else
                {
                    if (bytetime.Over)
                    {
                        readSize = frameHead.Length;
                        parsingState = ParsingState.ParsingHead;
                        return ResultOfParsingFrame.ReceivedOverTime;
                    }
                    else
                    {
                        // 使用字节超时
                        bytetime.Reset();
                        bytetime.Start();
                    }
                }
                #endregion 为兼容而保留的代码

                // 1、获得帧头
                if (parsingState == ParsingState.ParsingHead)
                {
                    int headIdx = 0, i = 0;

                    for (; i < bytes.Length; ++i)
                    {
                        if (bytes[i] == frameHead[headIdx])
                        {
                            headIdx++;

                            if (headIdx >= frameHead.Length)
                            {
                                i++;
                                break;
                            }
                        }
                        else
                        {
                            headIdx = 0;
                        }
                    }

                    if (headIdx != 0 && headIdx != frameHead.Length)    // 只收到帧头的一部分
                    {
                        readSize += bytes.Length - headIdx;
                        return ResultOfParsingFrame.ReceivedPartHead;
                    }

                    readSize += i;

                    if (headIdx == 0)   // 还未发现任何有效数据。比如这样的数据 01 02 02 A5 5A A5 5A 前面的 01 02 02这个时候不会删除也不是提示收到无用帧
                    {
                        return ResultOfParsingFrame.FormatNotMatched;
                    }

                    parsingState = ParsingState.ParsingControl;
                }

                // 2、获得帧控制部分
                if (parsingState == ParsingState.ParsingControl)
                {
                    int ctrlLen = GetControlLength();

                    if (ctrlLen < frameHead.Length)
                    {
                        throw new ApplicationException("帧控制部分的数据长度错误：帧控制部分的数据包含帧头数据，因此它的长度不可能小于帧头长度。");
                    }

                    if (ctrlLen - frameHead.Length > bytes.Length - readSize)
                    {
                        return ResultOfParsingFrame.WaitingForNextData;
                    }

                    frameBytes = new byte[ctrlLen];
                    frameHead.CopyTo(frameBytes, 0);
                    Array.Copy(bytes, readSize, frameBytes, frameHead.Length, ctrlLen - frameHead.Length);
                    readSize += ctrlLen - frameHead.Length; // 可以去掉在下面如果有问题去点侦头就好

                    if (!CheckControlData())
                    {
                        parsingState = ParsingState.ParsingHead;
                        
                        return ResultOfParsingFrame.ControlCheckError;
                    }

                    parsingState = ParsingState.ParsingRemainder;
                }

                // 3、获得帧的剩余部分
                if (parsingState == ParsingState.ParsingRemainder)
                {
                    int remainderLen = GetFrameLength(frameBytes) - frameBytes.Length;

                    if (remainderLen <= bytes.Length - readSize)
                    {
                        Array.Resize<byte>(ref frameBytes, frameBytes.Length + remainderLen);
                        Array.Copy(bytes, readSize, frameBytes, frameBytes.Length - remainderLen, remainderLen);
                        readSize += remainderLen; // 可以去掉在下面如果有问题去点侦头就好

                        if (crcCheck != null && !crcCheck.Check(frameBytes))    // 若校验对象==null则表示不需要校验。
                        {
                            parsingState = ParsingState.ParsingHead;

                            return ResultOfParsingFrame.CrcCheckError;
                        }

                        parsingState = ParsingState.ParsingFinished;
                    }
                    else
                    {
                        return ResultOfParsingFrame.WaitingForNextData; // remainderLen;
                    }
                }

                return ResultOfParsingFrame.ReceivingCompleted;
            }
            catch (Exception ex)
            {
                throw new ApplicationException("在解析通讯端口收到的数据时，发生异常：" + ex.Message + "错误原因可能是协议帧的控制长度或帧长度设置不正确，导致帧解析混乱。");
            }
        }

        /// <summary>
        /// 重新设置FrameBytes的长度。若当前长度大于等于新的长度则不做任何处理。
        /// </summary>
        /// <param name="newLength">新的长度。</param>
        protected void ResizeFrameBytes(int newLength)
        {
            if (frameBytes == null)
            {
                frameBytes = new byte[newLength];
            }
            else if (frameBytes.Length < newLength)
            {
                Array.Resize<byte>(ref frameBytes, newLength);
            }
        }

        /// <summary>
        /// 检查控制部分的合法性。合法则返回true。（基类直接返回true）
        /// </summary>
        protected virtual bool CheckControlData()
        {
            return true;
        }

        /// <summary>
        /// 返回帧控制部分的长度（包含帧头）。
        /// </summary>
        protected abstract int GetControlLength();

        /// <summary>
        /// 返回整个帧数据长度。
        /// </summary>
        /// <param name="bytes">从帧头开始，包含帧控制部分的帧数据。</param>
        protected abstract int GetFrameLength(byte[] bytes);

        protected bool containFormatedText = false;

        /// <summary>
        /// 帧中是否包含已经格式化好的信息，用于直接显示
        /// </summary>
        public bool ContainFormatedText
        {
            get
            {
                return containFormatedText;
            }
        }

        protected string formatedText = "";

        /// <summary>
        /// 已经格式化好的信息，用于直接显示
        /// </summary>
        public string FormatedText
        {
            get
            {
                return formatedText;
            }
        }

        /// <summary>
        /// 描述，用于报文监视时显示
        /// </summary>
        public string Desc { get; set; }

        /// <summary>
        /// 异常标志，用于判断接收帧解析是否异常
        /// </summary>
        public bool IsException { get; set; } = false;


        #region 共用静态方法

        /// <summary>
        /// 将字节数组转变成字符串形式返回。如bytes值为0xA5、0x5A时返回A5 5A.
        /// </summary>
        /// <param name="bytes">字节数组</param>
        public static string GetBytesText(byte[] bytes)
        {
            if (bytes == null || bytes.Length == 0)
            {
                return "";
            }

            int padlen = (bytes.Length > 1000) ? 1000 : bytes.Length;
            StringBuilder ret = new StringBuilder();
            
            for (int i = 0; i < padlen; ++i)
            {
                if(i>0)
                {
                    ret.Append(" ");
                }

                ret.Append(bytes[i].ToString("X2"));
            }

            return ret.ToString();
        }

        /// <summary>
        /// 获得byte[]型字节数据所对应的字符串。byte[]中的数据编码方式应为GB2312
        /// </summary>
        /// <param name="bytes">byte[]型字节数据</param>
        public static string GetUnicodeString(byte[] bytes)
        {
            if (bytes == null || bytes.Length == 0)
            {
                return "";
            }

            // 查找第一个0字节 2011-10-18 zsw
            int zeroIdx = 0;

            foreach (byte b in bytes)
            {
                if (b != 0)
                {
                    zeroIdx++;

                }
            }

            Encoding gb2312Encoding = Encoding.GetEncoding("GB2312");
            byte[] unicodeByte = Encoding.Convert(gb2312Encoding, Encoding.Unicode, bytes, 0, zeroIdx);
            
            return Encoding.Unicode.GetString(unicodeByte);
        }

        /// <summary>
        /// 获得字符串所对应的byte[]型字节数据。
        /// </summary>
        /// <param name="unicodeString">字符串</param>
        public static byte[] GetBytes(string unicodeString)
        {
            byte[] GB2312Bytes = Encoding.Convert(Encoding.Unicode,
                                        Encoding.GetEncoding("GB2312"),
                                        Encoding.Unicode.GetBytes(unicodeString));

            return GB2312Bytes;
        }

        #endregion
    }

    // A class ===================================================================
    /// <summary>
    /// 一般常用帧结构的类，它表示符合一定规律的帧结构的一系列帧。
    /// </summary>
	[Serializable()]
	public class FrameGenerallyUsed : FrameBase
    {
        /// <summary>
        /// 帧结构
        /// </summary>
        public struct FrameStructure
        {
            /// <summary>
            /// “帧头”数据
            /// </summary>
            public byte[] FrameHead;
            /// <summary>
            /// “报文长度”的开始位置
            /// </summary>
            public int PositionOfLength;
            /// <summary>
            /// “报文长度”的字节个数
            /// </summary>
            public int ByteCountOfLength;
            /// <summary>
            /// “报文长度”所对应的报文开始位置。
            /// </summary>
            public int PositionLengthPointed;
            /// <summary>
            /// “命令码”的位置
            /// </summary>
            public int CmdNoBit;    //...
            /// <summary>
            /// “校验和”长度
            /// </summary>
            public int LengthOfCrcCheck;
            /// <summary>
            /// “报文长度”所指的长度是否包含校验和（默认值为false）。
            /// </summary>
            public bool LengthContainCrcCheck;
            /// <summary>
            /// CRC校验对象。
            /// </summary>
            public CrcCheckBase CrcCheckObj;
            /// <summary>
            /// “报文长度”多字节时,先低后高（默认值为false）。 lirui 2019.09.27
            /// </summary>
            public bool LengthByteLowHigh;
        }

		[NonSerialized()]
		FrameStructure frameStruct;          // 帧结构

        #region Attributes

        /// <summary>
        /// 获取或设置帧结构。
        /// </summary>
        protected FrameStructure FrameStruct
        {
            get 
            {
                return frameStruct;
            }

            set
            {
                frameStruct = value;
                frameHead = frameStruct.FrameHead;
                crcCheck = frameStruct.CrcCheckObj;
            }
        }

        /// <summary>
        /// 获得帧头数据。
        /// </summary>
        public virtual byte[] FrameHead
        {
            get { return frameStruct.FrameHead; }
        }

        /// <summary>
        /// 获取或设置命令码的字节内容。
        /// </summary>
        public virtual byte CommandCodeByte
        {
            get
            {
                return frameBytes.Length > frameStruct.CmdNoBit ? frameBytes[frameStruct.CmdNoBit] : (byte)0;
            }

            set
            {
                // 确认有足够的长度，没有则重新分配
                int lenOfTextEndInTheory = frameStruct.CmdNoBit + 1;
                ResizeFrameBytes(lenOfTextEndInTheory);

                // 设置数据
                frameBytes[frameStruct.CmdNoBit] = value;
            }
        }

        /// <summary>获取或设置“报文长度”的字节内容。
        /// 这里是左边为高字节。
        /// </summary>
        public virtual int TextLenght
        {
            get
            {
                if (frameBytes.Length < frameStruct.PositionOfLength + frameStruct.ByteCountOfLength)
                {
                    return 0;
                }

                int len = 0;

                if (!frameStruct.LengthByteLowHigh)
                {
                    // 先高后低
                    for (int i = 0; i < frameStruct.ByteCountOfLength; i++)
                    {
                        len += (frameBytes[frameStruct.PositionOfLength + i] << (frameStruct.ByteCountOfLength - i - 1) * 8);
                    }
                }
                else
                {
                    // 先低后高
                    for (int i = 0; i < frameStruct.ByteCountOfLength; i++)
                    {
                        len += (frameBytes[frameStruct.PositionOfLength + i] << i * 8);
                    }
                }

                return len;
            }
            set
            {
                // 创建整个帧长度数组。
                int lenOfTextEndInTheory = frameStruct.PositionLengthPointed + value + (frameStruct.LengthContainCrcCheck ? 0 : frameStruct.LengthOfCrcCheck);
                ResizeFrameBytes(lenOfTextEndInTheory);

                // 设置帧长度数据。
                if (!frameStruct.LengthByteLowHigh)
                {
                    // 先高后低
                    for (int i = 0; i < frameStruct.ByteCountOfLength; i++)
                    {
                        frameBytes[frameStruct.PositionOfLength + i] = (byte)(value >> (frameStruct.ByteCountOfLength - i - 1) * 8);
                    }
                }
                else
                {
                    // 先低后高
                    for (int i = 0; i < frameStruct.ByteCountOfLength; i++)
                    {
                        frameBytes[frameStruct.PositionOfLength + i] = (byte)(value >> i * 8);
                    }
                }

                // 设置帧头数据。
                SetFrameHead();
            }
        }

        /// <summary>
        /// 获取数据正文的拷贝或设置数据正文的字节内容。若源数据不够长则返回null.
        /// </summary>
        public virtual byte[] TextBytes
        {
            get
            {
                // 确认有足够的长度，没有则返回null
                int lenOfText = TextLenght;

                if (frameBytes.Length - frameStruct.PositionLengthPointed < lenOfText)
                {
                    return null;
                }

                // 返回数据正文的拷贝
                byte[] bytes = new byte[lenOfText];
                Array.Copy(frameBytes, frameStruct.PositionLengthPointed, bytes, 0, lenOfText);
               
                return bytes;
            }

            set
            {
                // 确认有足够的长度，没有则重新分配
                int lenOfTextEndInTheory = frameStruct.PositionLengthPointed + TextLenght;
                ResizeFrameBytes(lenOfTextEndInTheory);

                // 设置数据
                if (value.Length > TextLenght)
                {
                    throw new Exception("设置帧正文逻辑错误：要设置的正文数据超出您设置的正文长度。");
                }
                
                value.CopyTo(frameBytes, frameStruct.PositionLengthPointed);
            }
        }

        #endregion

        #region Operations

        public FrameGenerallyUsed()
        {
        }

        public FrameGenerallyUsed(FrameStructure frameStruct)
        {
            FrameStruct = frameStruct;
        }

        /// <summary>
        /// 设置帧头数据
        /// </summary>
        void SetFrameHead()
        {
            if (frameBytes == null || frameBytes.Length < frameStruct.FrameHead.Length)
            {
                frameBytes = new byte[frameStruct.FrameHead.Length];
            }

            frameStruct.FrameHead.CopyTo(frameBytes, 0);
        }

        /// <summary>
        /// 设置校验码字节数据。
        /// </summary>
        public override void SetCheckBytes()
        {
            // 确认有足够的长度，没有则重新分配
            int lenOfTextEndInTheory = GetFrameLength(frameBytes);
            ResizeFrameBytes(lenOfTextEndInTheory);

            // 设置数据
            if (crcCheck != null)
            {
                crcCheck.GetCheckCode(ref frameBytes);
            }
        }

        protected override int GetControlLength()
        {
            return frameStruct.PositionLengthPointed;
        }

        protected override int GetFrameLength(byte[] bytes)
        {
            int ret = GetControlLength();

			ret += TextLenght;

            if (!frameStruct.LengthContainCrcCheck)
            {
                ret += frameStruct.LengthOfCrcCheck;
            }

            return ret;
        }

        #endregion
    }

    /// <summary>
    /// 直接信息帧
    /// </summary>
    [Serializable()]
    public class FrameTextMessage : FrameBase
    {
        public FrameTextMessage(string msg)
        {
            containFormatedText = true;
            formatedText = msg;
        }

        protected override int GetControlLength()
        {
            return 0;
        }

        protected override int GetFrameLength(byte[] bytes)
        {
            return 0;
        }
    }
}
