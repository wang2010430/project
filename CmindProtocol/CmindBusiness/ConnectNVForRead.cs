/***************************************************************************************************
* copyright : CMIND-SEMI
* version   : 1.00
* file      : ConnectNVForRead.cs
* date      : 2023/7/11 18:52:19
* author    : jinlong.wang
* brief     : Connect Command
* section Modification History
* - 1.0 : Initial version - jinlong.wang
***************************************************************************************************/

using Channel;
using CmindProtocol.DLL;
using Common;

namespace CmindProtocol.CmindBusiness
{
    public class ConnectNVForRead : BusinessBase
    {
        NVCommand command = NVCommand.ConnectToRead;
        PartitionType partitionType = PartitionType.RO;

        public ConnectNVForRead(ProtocolTask task, ProtocolBase ownerProtocol) : base(task, ownerProtocol)
        {
            NVReadParam param = (NVReadParam)task.Param;
            partitionType = CmindCommon.GetPartitionType(param.ItemID);

            //convert to byte array
            byte[] paramData = DataConvert.ConvertToByteArray(param, CmindCommon.DataEndian);

            CmindFrame frame = new CmindFrame()
            {
                Command = (byte)command,
                OperationType = OperationType.PLAT_Diag_NV,
                Data = paramData,
                Desc = string.Format("ConnectToRead: ItemID {0}, Size: 0x{1:X8} OPMode = {2}",
                       param.ItemID, param.Length,param.OperationMode)
            };

            // send the frame
            Sender.FrameBeSent = frame;
            Sender.BeginSend();
        }

        /// <summary>
        /// ACK Respond
        /// </summary>
        /// <param name="recFrame"></param>
        /// <returns></returns>
        public override bool ProcessDLFrame(CmindFrame recFrame)
        {
            if (recFrame.IsHand || (byte)(recFrame.Command & 0x7f) != (byte)command)
            {
                return false;
            }

            if ((recFrame.Command & 0x80) > 0)
            {
                string msg = GetErrorMsg(recFrame.Data[0]);
                CallRec(recFrame, string.Format("{0} Failed,Case:{1}", command.ToString(), msg));
                return SetDead(msg);
            }

            if (recFrame.Data.Length != 6)
            {
                string msg = "Data Format Error";
                CallRec(recFrame, string.Format("{0} Failed,Case:{1}", command, msg));
                return SetDead(msg);
            }

            dlProtocol.nvsSysInfo.SendMaxSize = DataConvert.ByteToUInt16(recFrame.Data, 0, CmindCommon.DataEndian);
            if (partitionType == PartitionType.RO)
            {
                dlProtocol.nvsSysInfo.ROSectorCount = DataConvert.ByteToUInt16(recFrame.Data, 2, CmindCommon.DataEndian);
                dlProtocol.nvsSysInfo.ROSectorSize = DataConvert.ByteToUInt16(recFrame.Data, 4, CmindCommon.DataEndian);
            }
            else if (partitionType == PartitionType.RW)
            {
                dlProtocol.nvsSysInfo.RWSectorCount = DataConvert.ByteToUInt16(recFrame.Data, 2, CmindCommon.DataEndian);
                dlProtocol.nvsSysInfo.RWSectorSize = DataConvert.ByteToUInt16(recFrame.Data, 4, CmindCommon.DataEndian);
            }

            CallRec(recFrame, string.Format("Connect to Read Succeed"));
            return SetDead(true);
        }
    }
}
