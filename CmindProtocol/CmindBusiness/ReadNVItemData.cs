/***************************************************************************************************
* copyright : 芯迈微半导体（珠海）有限公司
* version   : 1.00
* file      : ReadNVItemData.cs
* date      : 2023/7/11 21:58:38
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
    public class ReadItemParam
    {
        public int itemDataLength;
        public Action<double> ProgressCallBack = null;  // 进度回调
    }

    [Serializable]
    public class ReadNVDataResult : BusinessResult
    {
        public int sectorSize;
        public int sectorCount;
        public byte[] datas;
    }

    class ReadNVItemData : BusinessBase
    {
        #region Attribute
        NVCommand command = NVCommand.ReadNVData;
        ReadItemParam ItemParam = null;
        int totalDataSeq = 0;
        int rawDataMaxSize = 0;
        volatile int preDataSeq = 0;
        byte[] datas = new byte[0];
        int preFrameLength = 0;
        #endregion

        #region Constructor
        public ReadNVItemData(ProtocolTask task, ProtocolBase ownerProtocol) : base(task, ownerProtocol)
        {
            ItemParam = (ReadItemParam)task.Param;
            preDataSeq = 0;
            totalDataSeq = (ItemParam.itemDataLength - 1) / dlProtocol.nvsSysInfo.SendMaxSize + 1;
            rawDataMaxSize = dlProtocol.nvsSysInfo.SendMaxSize;
            SendReadDataFrame();
        }
        #endregion

        #region Function
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

            if (recFrame.Data.Length == 0)
            {
                string msg = "Data Format Error";
                CallRec(recFrame, string.Format("{0} Failed,Case:{1}", command.ToString(), msg));
                return SetDead(msg);
            }

            if (recFrame.Data.Length != (preFrameLength + 4))
            {
                string msg = "Size Error";
                CallRec(recFrame, string.Format("{0} Failed,Case:{1}", command.ToString(), msg));
                return SetDead(msg);
            }

            int dataLength = DataConvert.ByteToUInt16(recFrame.Data, 0, CmindCommon.DataEndian);
            if (dataLength != preFrameLength)
            {
                string msg = "Frame Size Error";
                CallRec(recFrame, string.Format("{0} dataLength != FrameLength,Case:{1}", command.ToString(), msg));
                return SetDead(msg);
            }

            //check the crc
            Array.Resize(ref datas, datas.Length + preFrameLength);
            Array.Copy(recFrame.Data, 4, datas, datas.Length - preFrameLength, preFrameLength);
            CallRec(recFrame, "Read NV Data Succeed");

            //trigger callback
            double progress = (preDataSeq / (double)totalDataSeq) * 100;
            ItemParam.ProgressCallBack?.Invoke(progress);

            if (SendReadDataFrame())
            {
                return true;
            }
            ReadNVDataResult ret = (ReadNVDataResult)Result;
            ret.Result = true;
            ret.datas = datas;
            return SetDead(true);
        }

        protected override BusinessResult GetBusinessResultClass()
        {
            return new ReadNVDataResult();
        }

        protected bool SendReadDataFrame()
        {
            try
            {
                if (preDataSeq == totalDataSeq)
                {
                    return false;
                }

                int startIndex = preDataSeq * rawDataMaxSize;
                preFrameLength = Math.Min(rawDataMaxSize, ItemParam.itemDataLength - startIndex);

                NVReadData param = new NVReadData()
                {
                    Total = (ushort)totalDataSeq,
                    Current = (ushort)(preDataSeq + 1),
                    Length = (ushort)preFrameLength
                };

                //convert to byte array
                byte[] paramData = DataConvert.ConvertToByteArray(param, CmindCommon.DataEndian);
                CmindFrame frame = new CmindFrame()
                {
                    Command = (byte)command,
                    OperationType = OperationType.PLAT_Diag_NV,
                    Data = paramData,
                    Desc = string.Format("ReadNVData,Length:{0}(Total:{1},Pre:{2})",
                         param.Length,
                         totalDataSeq,
                         (preDataSeq+1)
                         ),
                };
                Sender.FrameBeSent = frame;
                preDataSeq++;
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
