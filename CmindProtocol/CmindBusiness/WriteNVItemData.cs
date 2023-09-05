/***************************************************************************************************
* copyright : CMIND-SEMI
* version   : 1.00
* file      : WriteNVItemData.cs
* date      : 2023/7/11 16:03:58
* author    : jinlong.wang
* brief     : 
* section Modification History
* - 1.0 : Initial version - jinlong.wang
***************************************************************************************************/

using Channel;
using CmindProtocol.DLL;
using Common;
using log4net;
using System;

namespace CmindProtocol.CmindBusiness
{
    class WriteItemParam
    {
        public byte[] dataItem;
        public Action<double> ProgressCallBack = null;  
    }
    public class WriteNVItemData : BusinessBase
    {
        #region Attribute
        volatile int preDataSeq = 0;
        int rawDataMaxSize = 0;
        int totalDataSeq = 0;
        WriteItemParam ItemParam;
        NVCommand command = NVCommand.WriteNVData;
        #endregion

        #region Constructor
        public WriteNVItemData(ProtocolTask task, ProtocolBase ownerProtocol) : base(task, ownerProtocol)
        {
            ItemParam = (WriteItemParam)task.Param;
            rawDataMaxSize = dlProtocol.nvsSysInfo.SendMaxSize;
            preDataSeq = 0;
            totalDataSeq = (ItemParam.dataItem.Length - 1) / rawDataMaxSize + 1;
            SendNVDataFrame();
        }
        #endregion

        #region Function

        /// <summary>
        /// ACK Frame
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

            if (recFrame.Data.Length != 0)
            {
                string msg = "Data Format Error";
                CallRec(recFrame, string.Format("{0} Failed,Case:{1}", command.ToString(), msg));
                return SetDead(msg);
            }

            CallRec(recFrame, string.Format("{0} Succeeded", command.ToString()));
            double progress = (preDataSeq / (double)totalDataSeq) * 100;
            ItemParam.ProgressCallBack?.Invoke(progress);
            if (SendNVDataFrame())
            {
                return true;
            }

            return SetDead(true);
        }

        /// <summary>
        /// Send each frame of data
        /// </summary>
        /// <returns></returns>
        private bool SendNVDataFrame()
        {
            try
            {
                if (preDataSeq == totalDataSeq)
                {
                    return false;
                }

                int startIndex = preDataSeq * rawDataMaxSize;
                int length = Math.Min(rawDataMaxSize, ItemParam.dataItem.Length - startIndex);
                // 创建并截取子数组
                byte[] slicedData = new byte[length];
                Array.Copy(ItemParam.dataItem, startIndex, slicedData, 0, length);
                ushort dataCRC = CmindCommon.Check_Crc16(0, slicedData);
                NVWriteData param = new NVWriteData()
                {
                    Total = (ushort)totalDataSeq,
                    Current = (ushort)(preDataSeq+1),
                    Length = (ushort)length,
                    CRC = dataCRC,
                };
                //convert to byte array
                byte[] paramData = DataConvert.ConvertToByteArray(param, CmindCommon.DataEndian);
                byte[] combinedArray = DataConvert.CombineArraysEndianAware(paramData, slicedData, CmindCommon.DataEndian);

                CmindFrame frame = new CmindFrame()
                {
                    Command = (byte)command,
                    OperationType = OperationType.PLAT_Diag_NV,
                    Data = combinedArray,
                    Desc = string.Format("WriteNVData,Length:{0}(Total:{1},Pre:{2})",
                       param.Length,
                       totalDataSeq,
                       (preDataSeq+1)
                       ),
                };
                preDataSeq++;
                Sender.FrameBeSent = frame;
                Sender.Interval = CmindCommon.EraseSectorInterval;
                Sender.BeginSend();
                return true;
            }
            catch (Exception ex)
            {
                LogNetHelper.Error(ex);
                return false;
            }
        }
        #endregion
    }
}
