/***************************************************************************************************
* copyright : 芯迈微半导体（珠海）有限公司
* version   : 1.00
* file      : ConnectBussiness.cs
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
    class ConnectBussiness : BusinessBase
    {
        public ConnectBussiness(ProtocolTask task, ProtocolBase ownerProtocol)
            : base(task, ownerProtocol)
        {
            CmindFrame frame = new CmindFrame()
            {
                Command = (byte)CmindCommand.Connect,
                Desc = CmindCommand.Connect.ToString(),
            };

            Sender.FrameBeSent = frame;
            Sender.BeginSend();
        }

        public override bool ProcessDLFrame(CmindFrame recFrame)
        {
            if (recFrame.IsHand || (byte)(recFrame.Command & 0x7f) != (byte)CmindCommand.Connect)
            {
                return false;
            }

            if ((recFrame.Command & 0x80) > 0)
            {
                string msg = GetErrorMsg(recFrame.Data[0]);
                CallRec(recFrame, string.Format("Connect Failed,Case:{0}", msg));
                return SetDead(msg);
            }

            if (recFrame.Data.Length != 8)
            {
                string msg = "Data Format Error";
                CallRec(recFrame, string.Format("Connect Failed,Case:{0}", msg));
                return SetDead(msg);
            }

            dlProtocol.BroadVersion = string.Format("V{0}.{1}.{2}.{3}", 
                recFrame.Data[0], 
                recFrame.Data[1], 
                recFrame.Data[2], 
                recFrame.Data[3]);

            dlProtocol.DataMaxBytes = DataConvert.ByteToInt(recFrame.Data, 4, CmindCommon.DataEndian);

            CallRec(recFrame, string.Format("Connect Succeed,Version:{0},FrameMaxBytes:{1}",
                dlProtocol.BroadVersion,
                dlProtocol.DataMaxBytes));

            return SetDead(true);
        }
    }
}
