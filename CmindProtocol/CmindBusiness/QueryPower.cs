/***************************************************************************************************
* copyright : 芯迈微半导体（珠海）有限公司
* version   : 1.00
* file      : ProdutionBussiness.cs
* date      : 2023/07/10
* author    : haozhe.ni
* brief     : 
* section Modification History
* - 1.0 : Initial version - haozhe.ni
***************************************************************************************************/

using Channel;

namespace CmindProtocol.CmindBusiness
{
    class QueryPower : BusinessBase
    {
        public QueryPower(ProtocolTask task, ProtocolBase ownerProtocol)
            : base(task, ownerProtocol)
        {

            CmindFrame frame = new CmindFrame()
            {
                Command = (byte)CmindCommand.QueryPower,
                Desc = string.Format("Query Power"),
            };

            Sender.FrameBeSent = frame;
            Sender.BeginSend();
        }

        public override bool ProcessDLFrame(CmindFrame recFrame)
        {
            if (recFrame.IsHand || (byte)(recFrame.Command & 0x7f) != (byte)CmindCommand.QueryPower)
            {
                return false;
            }

            if ((recFrame.Command & 0x80) > 0)
            {
                string msg = GetErrorMsg(recFrame.Data[0]);
                CallRec(recFrame, string.Format("Query Power Failed,Case:{0}", msg));
                return SetDead(msg);
            }

            if (recFrame.Data.Length != 1)
            {
                string msg = "Data Format Error";
                CallRec(recFrame, string.Format("Query Power Failed,Case:{0}", msg));
                return SetDead(msg);
            }

            CallRec(recFrame, $"Query Power Succeed,Status:{(recFrame.Data[0] == 1 ? "On" : "Off")}");
            return SetDead(true);
        }
    }
}
