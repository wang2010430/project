/***************************************************************************************************
* copyright : 芯迈微半导体（珠海）有限公司
* version   : 1.00
* file      : ExecuteBussiness.cs.cs
* date      : 2023/04/28
* author    : haozhe.ni
* brief     : 
* section Modification History
* - 1.0 : Initial version - haozhe.ni
***************************************************************************************************/

using Channel;
using Common;

namespace CmindProtocol.CmindBusiness
{
    class ExecuteBussiness : BusinessBase
    {
        public ExecuteBussiness(ProtocolTask task, ProtocolBase ownerProtocol)
            : base(task, ownerProtocol)
        {
            uint addr = (uint)task.Param;
            CmindFrame frame = new CmindFrame()
            {
                Command = (byte)CmindCommand.Execute,
                Data = DataConvert.UIntToByte(addr, CmindCommon.DataEndian),
                Desc = string.Format("Execute,Addr:0x{0:X8}", addr),
            };

            Sender.FrameBeSent = frame;
            Sender.BeginSend();
        }

        public override bool ProcessDLFrame(CmindFrame recFrame)
        {
            if (recFrame.IsHand || (byte)(recFrame.Command & 0x7f) != (byte)CmindCommand.Execute)
            {
                return false;
            }

            if ((recFrame.Command & 0x80) > 0)
            {
                string msg = GetErrorMsg(recFrame.Data[0]);
                CallRec(recFrame, string.Format("Execute Failed,Case:{0}", msg));
                return SetDead(msg);
            }

            if (recFrame.Data.Length != 4)
            {
                string msg = "Data Format Error";
                CallRec(recFrame, string.Format("Execute Failed,Case:{0}", msg));
                return SetDead(msg);
            }

            CallRec(recFrame, "Execute Succeed");
            return SetDead(true);
        }
    }
}