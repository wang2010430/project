/***************************************************************************************************
* copyright : 芯迈微半导体（珠海）有限公司
* version   : 1.00
* file      : WriteDataBussiness.cs
* date      : 2023/05/04
* author    : haozhe.ni
* brief     : 
* section Modification History
* - 1.0 : Initial version- haozhe.ni
***************************************************************************************************/

using Channel;
using Common;
using System;
using System.Collections.Generic;
using System.Linq;

namespace CmindProtocol.CmindBusiness
{
    class WriteDataParam
    {
        public uint StartAddr;                          // 起始地址
        public byte[] Datas;                            // 数据
        public Action<double> ProgressCallBack = null;  // 进度回调
    }

    class WriteDataBussiness : BusinessBase
    {
        CmindCommand command;

        WriteDataParam para;

        int totalDataSeq;
        int preDataSeq;
        int rawDataMaxSize;

        public WriteDataBussiness(ProtocolTask task, ProtocolBase ownerProtocol)
            : base(task, ownerProtocol)
        {
            para = (WriteDataParam)task.Param;

            rawDataMaxSize = dlProtocol.DataMaxBytes - 8;
            preDataSeq = 1;
            totalDataSeq = (para.Datas.Length - 1) / rawDataMaxSize + 1;

            SendWriteDataFrame();
        }

        public override bool ProcessDLFrame(CmindFrame recFrame)
        {
            if (recFrame.IsHand || (byte)(recFrame.Command & 0x7f) != (byte)command)
            {
                return false;
            }

            if ((recFrame.Command & 0x80) > 0)
            {
                string msg = GetErrorMsg(recFrame.Data[0]);
                CallRec(recFrame, string.Format("{0} Failed,Case:{1}", command, msg));
                return SetDead(msg);
            }

            if (command == CmindCommand.WriteData)
            {
                if (recFrame.Data.Length != 0)
                {
                    string msg = "Data Format Error";
                    CallRec(recFrame, string.Format("{0} Failed,Case:{1}", command, msg));
                    return SetDead(msg);
                }

                CallRec(recFrame, string.Format("{0} Succeeded", command));

                // 进度回调
                double progress = ((preDataSeq - 1) / (double)totalDataSeq) * 100;
                para.ProgressCallBack?.Invoke(progress);

                if (SendWriteDataFrame())
                {
                    return true;
                }
                else
                {
                    SendCrcFrame(para.StartAddr, para.Datas.Length);
                }
            }
            else
            {
                if (recFrame.Data.Length != 2)
                {
                    string msg = "Data Format Error";
                    CallRec(recFrame, string.Format("{0} Failed,Case:{1}", command, msg));
                    return SetDead(msg);
                }

                ushort crc = CmindCommon.Check_Crc16(0, para.Datas);
                ushort recCrc = DataConvert.ByteToUInt16(recFrame.Data, 0, CmindCommon.DataEndian);

#if DEBUG
                // 调试模式用，跳过校验。
                CallRec(recFrame, string.Format("{0} Succeeded,Crc:0x{1:X4}", command, crc));
                return SetDead(true);
#else

                if (crc != recCrc)
                {
                    string msg = string.Format("Crc Error,RecCrc:0x{0:X4},DataCrc:0x{1:X4}", recCrc, crc);;
                    CallRec(recFrame, msg);
                    return SetDead(msg);
                }
                else
                {
                    CallRec(recFrame, string.Format("{0} Succeeded,Crc:0x{1:X4}", command, crc));
                    return SetDead(true);
                }
#endif
            }

            return true;
        }


        private bool SendWriteDataFrame()
        {
            if (preDataSeq > totalDataSeq)
            {
                return false;
            }

            command = CmindCommand.WriteData;
            int size;
            uint startAddr = para.StartAddr + (uint)(rawDataMaxSize * (preDataSeq - 1));

            if (preDataSeq == totalDataSeq)
            {
                size = para.Datas.Length % rawDataMaxSize;
                if (size == 0)
                {
                    size = rawDataMaxSize;
                }
            }
            else
            {
                size = rawDataMaxSize;
            }

            List<byte> datas = new List<byte>();
            datas.AddRange(DataConvert.UIntToByte(startAddr));
            datas.AddRange(DataConvert.IntToByte(size, CmindCommon.DataEndian));
            datas.AddRange(para.Datas.Skip(rawDataMaxSize * (preDataSeq - 1)).Take(size).ToArray());

            CmindFrame frame = new CmindFrame()
            {
                Command = (byte)command,
                Data = datas.ToArray(),
                Desc = string.Format("{0},StartAddress:0x{1:X4},Size:{2}(Total:{3},Pre:{4})",
                    command,
                    startAddr,
                    size,
                    totalDataSeq,
                    preDataSeq
                    ),
            };
            Sender.FrameBeSent = frame;
            Sender.BeginSend();
            preDataSeq++;

            return true;
        }

        private void SendCrcFrame(uint addr, int size)
        {
            command = CmindCommand.Crc;

            List<byte> datas = new List<byte>();
            datas.AddRange(DataConvert.UIntToByte(addr));
            datas.AddRange(DataConvert.IntToByte(size));

            CmindFrame frame = new CmindFrame()
            {
                Command = (byte)command,
                Data = datas.ToArray(),
                Desc = string.Format("{0},Start:{1},Size:{2}", command, addr, size),
            };
            Sender.FrameBeSent = frame;
            Sender.BeginSend();
        }
    }
}
