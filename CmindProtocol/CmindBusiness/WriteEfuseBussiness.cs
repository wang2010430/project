/***************************************************************************************************
* copyright : 芯迈微半导体（珠海）有限公司
* version   : 1.00
* file      : WriteEfuseBussiness.cs
* date      : 2023/04/27
* author    : haozhe.ni
* brief     : 
* section Modification History
* - 1.0 : Initial version - haozhe.ni
***************************************************************************************************/

using Channel;
using Common;
using System.Collections.Generic;

namespace CmindProtocol.CmindBusiness
{
    class WriteEfuseParam
    {
        public uint Addr;
        public uint Value;
    }

    class WriteEfuseBussiness : BusinessBase
    {
        public WriteEfuseBussiness(ProtocolTask task, ProtocolBase ownerProtocol)
            : base(task, ownerProtocol)
        {

            WriteEfuseParam para = (WriteEfuseParam)task.Param;

            List<byte> datas = new List<byte>();
            datas.AddRange(DataConvert.UIntToByte(para.Addr, CmindCommon.DataEndian));
            datas.AddRange(DataConvert.UIntToByte(para.Value, CmindCommon.DataEndian));

            CmindFrame frame = new CmindFrame()
            {
                Command = (byte)CmindCommand.WriteEFuse,
                Data = datas.ToArray(),
                Desc = string.Format("Write Efuse,Addr:0x{0:X8},Size:0x{1:X8}", para.Addr, para.Value),
            };

            Sender.FrameBeSent = frame;
            Sender.BeginSend();
        }

        public override bool ProcessDLFrame(CmindFrame recFrame)
        {
            if (recFrame.IsHand || (byte)(recFrame.Command & 0x7f) != (byte)CmindCommand.WriteEFuse)
            {
                return false;
            }

            if ((recFrame.Command & 0x80) > 0)
            {
                string msg = GetErrorMsg(recFrame.Data[0]);
                CallRec(recFrame, string.Format("Write Efuse Failed,Case:{0}", msg));
                return SetDead(msg);
            }

            if (recFrame.Data.Length != 0)
            {
                string msg = "Data Format Error";
                CallRec(recFrame, string.Format("Write Efuse Failed,Case:{0}", msg));
                return SetDead(msg);
            }

            CallRec(recFrame, "Write Efuse Succeed");
            return SetDead(true);
        }
    }
}
