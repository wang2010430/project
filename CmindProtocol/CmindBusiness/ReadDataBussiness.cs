/***************************************************************************************************
* copyright : 芯迈微半导体（珠海）有限公司
* version   : 1.00
* file      : ReadDataBussiness.cs
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
using System.IO;

namespace CmindProtocol.CmindBusiness
{
    class ReadDataParam
    {
        public uint StartAddr = 0;

        public int Size = 0;

        public string File;
    }

    class ReadDataBussiness : BusinessBase
    {
        CmindCommand command;
        ReadDataParam para;

        int totalDataSeq;
        int preDataSeq;
        int size;
        byte[] datas = new byte[0];

        public ReadDataBussiness(ProtocolTask task, ProtocolBase ownerProtocol)
            : base(task, ownerProtocol)
        {
            para = (ReadDataParam)task.Param;

            preDataSeq = 1;
            totalDataSeq = (para.Size - 1) / dlProtocol.DataMaxBytes + 1;

            SendReadDataFrame();
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

            if (command == CmindCommand.ReadData)
            {
                if (recFrame.Data.Length != size)
                {
                    string msg = "Size Error";
                    CallRec(recFrame, string.Format("{0} Failed,Case:{1}", command, msg));
                    return SetDead(msg);
                }

                Array.Resize(ref datas, datas.Length + size);
                Array.Copy(recFrame.Data, 0, datas, datas.Length - size, size);

                CallRec(recFrame, string.Format("{0} Succeeded", command));

                if (SendReadDataFrame())
                {
                    return true;
                }
                else
                {
                    SendCrcFrame(para.StartAddr, para.Size);
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

                ushort crc = CmindCommon.Check_Crc16(0, datas);
                ushort recCrc = DataConvert.ByteToUInt16(recFrame.Data, 0, CmindCommon.DataEndian);

                if (crc != recCrc)
                {
                    string msg = string.Format("Crc Error,RecCrc:0x{0:X4},DataCrc:0x{1:X4}", recCrc, crc);
                    CallRec(recFrame, msg);
                    return SetDead(msg);
                }
                else
                {
                    CallRec(recFrame, string.Format("{0} Succeeded,Crc:0x{1:X4}", command, crc));
                    // 校验成功，保存文件
                    try
                    {
                        var outputStream = File.Create(para.File);
                        using (var writer = new BinaryWriter(outputStream))
                        {
                            writer.Write(datas);
                            writer.Close();
                        }
                        outputStream.Close();
                    }
                    catch
                    {
                        return SetDead(false, "Save File Failed!");
                    }

                    return SetDead(true);
                }
            }

            return true;
        }

        private bool SendReadDataFrame()
        {
            if (preDataSeq > totalDataSeq)
            {
                return false;
            }

            command = CmindCommand.ReadData;

            uint startAddr = para.StartAddr + (uint)(dlProtocol.DataMaxBytes * (preDataSeq - 1));

            if (preDataSeq == totalDataSeq)
            {
                size = para.Size % dlProtocol.DataMaxBytes;
                if (size == 0)
                {
                    size = dlProtocol.DataMaxBytes;
                }
            }
            else
            {
                size = dlProtocol.DataMaxBytes;
            }

            List<byte> datas = new List<byte>();
            datas.AddRange(DataConvert.UIntToByte(startAddr));
            datas.AddRange(DataConvert.IntToByte(size, CmindCommon.DataEndian));

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
                    )
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
