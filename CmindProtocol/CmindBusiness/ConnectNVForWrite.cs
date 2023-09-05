/***************************************************************************************************
* copyright : CMIND-SEMI
* version   : 1.00
* file      : ConnectToWriteBusiness.cs
* date      : 2023/7/11 
* author    : jinlong.wang
* brief     : Connect Command
* section Modification History
* - 1.0 : Initial version - jinlong.wang
***************************************************************************************************/

using Channel;
using CmindProtocol.DLL;
using Common;
using System;

namespace CmindProtocol.CmindBusiness
{

    public class ConnectNVForWrite : BusinessBase
    {
        PartitionType partitionType = PartitionType.RO;
        NVCommand command = NVCommand.ConnectToWrite;
        public ConnectNVForWrite(ProtocolTask task, ProtocolBase ownerProtocol) : base(task, ownerProtocol)
        {
            NVWriteParam param = (NVWriteParam)task.Param;
            partitionType = CmindCommon.GetPartitionType(param.ItemID);

            //convert to byte array
            byte[] paramData = DataConvert.ConvertToByteArray(param, CmindCommon.DataEndian);
            
            //convert operate mode
            NVOperateMode operateMode = (NVOperateMode)param.OperationMode;
            string sOperateMode = Enum.GetName(typeof(NVOperateMode), operateMode);

            //Convert download mode
            NVDownloadMode dlMode = (NVDownloadMode)param.DownloadMode;
            string sDLMode = Enum.GetName(typeof(NVDownloadMode), dlMode);

            CmindFrame frame = new CmindFrame()
            {
                Command = (byte)command,
                OperationType = OperationType.PLAT_Diag_NV,
                Data = paramData,
                Desc = string.Format("ConnectToWrite: ItemID {0}, Size: 0x{1:X8} DLMode = {2} OPMode = {3}",
                        param.ItemID, param.Length, sDLMode, sOperateMode)
            };

            // send the frame
            Sender.FrameBeSent = frame;
            Sender.Interval = CmindCommon.EraseSectorInterval;
            Sender.BeginSend();
        }

        /// <summary>
        /// ACK Respond
        /// </summary>
        /// <param name="recFrame"></param>
        /// <returns></returns>
        public override bool ProcessDLFrame(CmindFrame recFrame)
        {
            if (recFrame.IsHand || (byte)(recFrame.Command & 0x7f) != (byte)NVCommand.ConnectToWrite)
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
            CallRec(recFrame, string.Format("Connect to Write Succeed"));
            return SetDead(true);
        }

        //public override void ProcessOvertime()
        //{
        //    //CallRec(command.ToString(), string.Format("ACK TimeOut"));

        //    this.ProcessOvertime();
        //}
    }
}
