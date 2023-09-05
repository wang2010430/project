/***************************************************************************************************
* copyright : 芯迈微半导体（珠海）有限公司
* version   : 1.00
* file      : ChangeBaudBussiness.cs
* date      : 2023/04/27
* author    : haozhe.ni
* brief     : 
* section Modification History
* - 1.0 : Initial version- haozhe.ni
***************************************************************************************************/

using Channel;
using Common;

namespace CmindProtocol.CmindBusiness
{
    class ChangeBaudBussiness : BusinessBase
    {
        public ChangeBaudBussiness(ProtocolTask task, ProtocolBase ownerProtocol)
            : base(task, ownerProtocol)
        {
            int baud = (int)task.Param;
            CmindFrame frame = new CmindFrame()
            {
                Command = (byte)CmindCommand.ChangeBaud,
                Data = DataConvert.IntToByte(baud, CmindCommon.DataEndian),
                Desc = string.Format("Change Baud:{0}", baud),
            };

            Sender.FrameBeSent = frame;
            Sender.BeginSend();
        }

        public override bool ProcessDLFrame(CmindFrame recFrame)
        {
            if (recFrame.IsHand || (byte)(recFrame.Command & 0x7f) != (byte)CmindCommand.ChangeBaud)
            {
                return false;
            }

            if ((recFrame.Command & 0x80) > 0)
            {
                string msg = GetErrorMsg(recFrame.Data[0]);
                CallRec(recFrame, string.Format("Change Baud Failed,Case:{0}", msg));
                return SetDead(msg); 
            }

            if (recFrame.Data.Length != 0)
            {
                string msg = "";
                CallRec(recFrame, string.Format("Change Baud Failed,Case:Data Format Error", msg));
                return SetDead(msg);
            }

            CallRec(recFrame, "Change Baud Succeed");
            return SetDead(true);
        }
    }
}
