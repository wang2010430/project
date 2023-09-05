/***************************************************************************************************
* copyright : 芯迈微半导体（珠海）有限公司
* version   : 1.00
* file      : ReadEfuseBussiness.cs
* date      : 2023/04/27
* author    : haozhe.ni
* brief     : 
* section Modification History
* - 1.0 : Initial version - haozhe.ni
***************************************************************************************************/

using Channel;
using Common;
using System;

namespace CmindProtocol.CmindBusiness
{
    [Serializable]
    public class ReadEfuseResult : BusinessResult
    {
        public uint Value;
    }

    class ReadEfuseBussiness : BusinessBase
    {
        public ReadEfuseBussiness(ProtocolTask task, ProtocolBase ownerProtocol)
            : base(task, ownerProtocol)
        {

            uint addr = (uint)task.Param;
            CmindFrame frame = new CmindFrame()
            {
                Command = (byte)CmindCommand.ReadEfuse,
                Data = DataConvert.UIntToByte(addr, CmindCommon.DataEndian),
                Desc = string.Format("Read Efuse,Addr:0x{0:X8}", addr),
            };

            Sender.FrameBeSent = frame;
            Sender.BeginSend();
        }

        protected override BusinessResult GetBusinessResultClass()
        {
            return new ReadEfuseResult();
        }

        public override bool ProcessDLFrame(CmindFrame recFrame)
        {
            if (recFrame.IsHand || (byte)(recFrame.Command & 0x7f) != (byte)CmindCommand.ReadEfuse)
            {
                return false;
            }

            if ((recFrame.Command & 0x80) > 0)
            {
                string msg = GetErrorMsg(recFrame.Data[0]);
                CallRec(recFrame, string.Format("Read Efuse Failed,Case:{0}", msg));
                return SetDead(msg);
            }

            if (recFrame.Data.Length != 4)
            {
                string msg = "Data Format Error";
                CallRec(recFrame, string.Format("Read Efuse Failed,Case:{0}", msg));
                return SetDead(msg);
            }

            ReadEfuseResult ret = (ReadEfuseResult)Result;
            ret.Result = true;
            ret.Value = DataConvert.ByteToUInt(recFrame.Data, 0, CmindCommon.DataEndian);
            CallRec(recFrame, string.Format("Read Efuse Succeed,Value:0x{0:X8}", ret.Value));

            return SetDead(true);
        }
    }
}
