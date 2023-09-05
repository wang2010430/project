/***************************************************************************************************
* copyright : 芯迈微半导体（珠海）有限公司
* version   : 1.00
* file      : EraseFlashBussiness.cs
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
    class EraseFlashParam
    {
        public uint Addr;
        public uint Size;
    }

    class EraseFlashBussiness : BusinessBase
    {
        public EraseFlashBussiness(ProtocolTask task, ProtocolBase ownerProtocol)
            : base(task, ownerProtocol)
        {
            EraseFlashParam para = (EraseFlashParam)task.Param;

            List<byte> datas = new List<byte>();
            datas.AddRange(DataConvert.UIntToByte(para.Addr, CmindCommon.DataEndian));
            datas.AddRange(DataConvert.UIntToByte(para.Size, CmindCommon.DataEndian));

            CmindFrame frame = new CmindFrame()
            {
                Command = (byte)CmindCommand.EraseFlash,
                Data = datas.ToArray(),
                Desc = string.Format("Erase Flash,Addr:0x{0:X8},Size:0x{1:X8}", para.Addr, para.Size),
            };

            Sender.FrameBeSent = frame;
            Sender.BeginSend();
        }

        public override bool ProcessDLFrame(CmindFrame recFrame)
        {
            if (recFrame.IsHand || (byte)(recFrame.Command & 0x7f) != (byte)CmindCommand.EraseFlash)
            {
                return false;
            }

            if ((recFrame.Command & 0x80) > 0)
            {
                string msg = GetErrorMsg(recFrame.Data[0]);
                CallRec(recFrame, string.Format("Erase Flash Failed,Case:{0}", msg));
                return SetDead(msg);
            }

            if (recFrame.Data.Length != 0)
            {
                string msg = "Data Format Error";
                CallRec(recFrame, string.Format("Erase Flash Failed,Case:{0}", msg));
                return SetDead(msg);
            }

            CallRec(recFrame, "Erase Flash Succeed");
            return SetDead(true);
        }
    }
}