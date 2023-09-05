/***************************************************************************************************
* copyright : 芯迈微半导体（珠海）有限公司
* version   : 1.00
* file      : ProdutionBussiness.cs
* date      : 2023/07/05
* author    : haozhe.ni
* brief     : 
* section Modification History
* - 1.0 : Initial version - haozhe.ni
***************************************************************************************************/

using Channel;
using CmindProtocol.CmindBusiness.ProdutionPara;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CmindProtocol.CmindBusiness
{
    class ProdutionBussiness : BusinessBase
    {
        IProduction Para = null;

        public ProdutionBussiness(ProtocolTask task, ProtocolBase ownerProtocol)
            : base(task, ownerProtocol)
        {
            Para = (IProduction)task.Param;

            List<byte> datas = new List<byte>();
            datas.Add((byte)Para.StructType);    // Type
            datas.Add(0x00);                     // Reserve
            datas.AddRange(Para.GetBytes());     // RawDatas

            CmindFrame frame = new CmindFrame()
            {
                Command = (byte)CmindCommand.Prodution,
                OperationType = OperationType.Phy,
                Data = datas.ToArray(),
                Desc = string.Format("Prodution - {0}", Para.StructType),
            };

            Sender.FrameBeSent = frame;
            Sender.BeginSend();
        }

        public override bool ProcessDLFrame(CmindFrame recFrame)
        {
            if (recFrame.IsHand || (byte)(recFrame.Command & 0x7f) != (byte)CmindCommand.Prodution)
            {
                return false;
            }

            if ((recFrame.Command & 0x80) > 0)
            {
                string msg = GetErrorMsg(recFrame.Data[0]);
                CallRec(recFrame, string.Format("Prodution Failed,Case:{0}", msg));
                return SetDead(msg);
            }

            if (recFrame.Data.Length < 2)
            {
                string msg = "Data Format Error";
                CallRec(recFrame, string.Format("Prodution Failed,Case:{0}", msg));
                return SetDead(msg);
            }

            if(recFrame.Data[0] == (byte)Para.StructType)
            {
                string msg = "Struct Type Format Error";
                CallRec(recFrame, string.Format("Prodution Failed,Case:{0}", msg));
                return SetDead(msg);
            }

            if(recFrame.Data[1] == 0)
            {
                CallRec(recFrame, string.Format("Prodution Succeed"));
                return SetDead(true);
            }
            else
            {
                string msg = Encoding.UTF8.GetString(recFrame.Data.Skip(2).ToArray());
                CallRec(recFrame, string.Format("Prodution Failed,Case:{0}", msg));
                return SetDead(msg);
            }
        }
    }
}
