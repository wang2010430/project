/***************************************************************************************************
* copyright : 芯迈微半导体（珠海）有限公司
* version   : 1.00
* file      : DLProtocol.cs
* date      : 2023/04/27
* author    : haozhe.ni
* brief     : 
* section Modification History
* - 1.0 : Initial version - haozhe.ni
***************************************************************************************************/

using Channel;
using Common;
using System;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using CmindProtocol.DLL;
using CmindProtocol.CmindBusiness;
using CmindProtocol.CmindBusiness.ProdutionPara;

namespace CmindProtocol
{
    /// <summary>
    /// 板卡日志委托
    /// </summary>
    /// <param name="type">日志类型</param>
    /// <param name="msg">日志信息</param>
    public delegate void BoardLogHandler(LogType type, string msg);

    public class Cmind : ProtocolBase
    {
        HandshakeBussiness handBussiness = null;

        #region 重写
        /// <summary>
        /// 帧获取
        /// </summary>
        /// <returns></returns>
        protected override FrameBase GetANewFrameToReceiveData()
        {
            return new CmindFrame();
        }

        /// <summary>
        /// 处理帧数据
        /// </summary>
        /// <param name="frame">帧</param>
        protected override void ProcessFrame(FrameBase frame)
        {
            try
            {
                CmindFrame dFrame = (CmindFrame)frame;
                if (!dFrame.IsHand)
                {
                    if (dFrame.Command == (byte)CmindCommand.Log
                        && dFrame.Data.Length > 1)
                    {
                        LogType type = (LogType)dFrame.Data[0];
                        byte[] datas = dFrame.Data.Skip(1).ToArray();

                        if (type == LogType.Msg
                            || type == LogType.Print)
                        {
                            string log = Encoding.ASCII.GetString(datas);
                            CallBoardLog(type, log);
                            CallRec(frame, string.Format("Log {0}:{1}", type.ToString(), log));
                            return;
                        }
                        else if (type == LogType.Hex
                            && datas.Length > 0)
                        {
                            StringBuilder sb = new StringBuilder();
                            sb.Append("0x");
                            for (int i = datas.Length - 1; i >= 0; i--)
                            {
                                sb.Append(string.Format("{0:X2}", datas[i]));
                            }

                            CallBoardLog(type, sb.ToString());
                            CallRec(frame, string.Format("Log Hex:{0}", sb.ToString()));
                            return;
                        }
                    }
                }

                CallThrow(frame, "Unprocessed frames");
            }
            catch (Exception ex)
            {
                UtMessageBase.ShowOneMessage(string.Format("处理报文接收帧时发生异常：{0}", ex), PopupMessageType.Exception);
            }
        }
        #endregion

        #region 事件
        /// <summary>
        /// 板卡日志事件
        /// </summary>
        public event BoardLogHandler EventBoardLog;

        /// <summary>
        /// 通知板卡日志
        /// </summary>
        /// <param name="type">日志类型</param>
        /// <param name="msg">日志信息</param>
        private void CallBoardLog(LogType type, string msg)
        {
            EventBoardLog?.Invoke(type, msg);
        }
        #endregion

        #region 属性
        /// <summary>
        /// 板卡版本（连接时，从装置读取）
        /// </summary>
        public string BroadVersion;

        /// <summary>
        /// Data From NVS System 
        /// </summary>
        //internal int NVSSendMaxSize = 1024;
        internal NVSSystemInfo nvsSysInfo = new NVSSystemInfo();
        /// <summary>
        /// 数据区最大长度（连接时，从装置读取）
        /// </summary>
        internal int DataMaxBytes = 1000;


        #endregion

        #region 应用函数
        /// <summary>
        /// 自动连接（测试用）
        /// </summary>
        /// <param name="addr">地址</param>
        /// <param name="fileName">文件名</param>
        /// <returns></returns>
        public BoolQResult AutoConnect(uint addr, string fileName)
        {
            // Hand
            AutoResetEvent e = new AutoResetEvent(false);

            BusinessResult ret = null;
            StartHand_Asyn((callBack) => { ret = callBack; e.Set(); });
            e.WaitOne();
            if (!ret.Result)
            {
                return new BoolQResult(string.Format("Hand Failed,Case:{0}", ret.Msg));
            }

            // Connect
            Connect_Asyn((callBack) => { ret = callBack; e.Set(); });
            e.WaitOne();
            if (!ret.Result)
            {
                return new BoolQResult(string.Format("Connect Failed,Case:{0}", ret.Msg));
            }

            // WriteData
            WriteData_Asyn(addr, fileName, null, (callBack) => { ret = callBack; e.Set(); });
            e.WaitOne();
            if (!ret.Result)
            {
                return new BoolQResult(string.Format("WriteData Failed,Case:{0}", ret.Msg));
            }

            // Execute
            Execute_Asyc(addr, (callBack) => { ret = callBack; e.Set(); });
            e.WaitOne();
            if (!ret.Result)
            {
                return new BoolQResult(string.Format("Execute Failed,Case:{0}", ret.Msg));
            }

            return new BoolQResult(true);
        }

        /// <summary>
        /// 开始握手（异步）
        /// </summary>
        /// <param name="retCallBack">结果回调</param>
        public void StartHand_Asyn(Action<BusinessResult> retCallBack)
        {
            ProtocolTask task = new ProtocolTask()
            {
                RetCallBack = retCallBack
            };
            handBussiness = new HandshakeBussiness(task, this);
        }

        /// <summary>
        /// 开始握手
        /// </summary>
        /// <returns>结果</returns>
        public BusinessResult StartHand()
        {
            BusinessResult ret = null;
            AutoResetEvent e = new AutoResetEvent(false);
            StartHand_Asyn((callBack) =>
            {
                ret = callBack;
                e.Set();
            });
            e.WaitOne();
            return ret;
        }

        /// <summary>
        /// 停止握手
        /// </summary>
        public void StopHand()
        {
            if (handBussiness != null)
            {
                handBussiness.Dead = true;
            }
        }

        /// <summary>
        /// 连接（异步）
        /// </summary>
        /// <param name="retCallBack">结果回调</param>
        public void Connect_Asyn(Action<BusinessResult> retCallBack)
        {
            ProtocolTask task = new ProtocolTask()
            {
                RetCallBack = retCallBack
            };

            new ConnectBussiness(task, this);
        }

        /// <summary>
        /// 连接
        /// </summary>
        /// <returns>结果</returns>
        public BusinessResult Connect()
        {
            BusinessResult ret = null;
            AutoResetEvent e = new AutoResetEvent(false);
            Connect_Asyn((callBack) =>
            {
                ret = callBack;
                e.Set();
            });
            e.WaitOne();
            return ret;
        }

        /// <summary>
        /// 修改波特率（异步）
        /// </summary>
        /// <param name="baud">波特率</param>
        /// <param name="retCallBack">结果回调</param>
        public void ChangeBaud_Asyn(int baud, Action<BusinessResult> retCallBack)
        {
            ProtocolTask task = new ProtocolTask()
            {
                Param = baud,
                RetCallBack = retCallBack
            };

            new ChangeBaudBussiness(task, this);
        }

        /// <summary>
        /// 修改波特率
        /// </summary>
        /// <param name="baud">波特率</param>
        /// <returns>结果</returns>
        public BusinessResult ChangeBaud(int baud)
        {
            BusinessResult ret = null;
            AutoResetEvent e = new AutoResetEvent(false);
            ChangeBaud_Asyn(baud, (callBack) =>
             {
                 ret = callBack;
                 e.Set();
             });
            e.WaitOne();
            return ret;
        }

        /// <summary>
        /// 探险Flash（异步）
        /// </summary>
        /// <param name="addr">地址</param>
        /// <param name="size">大小</param>
        /// <param name="retCallBack">结果回调</param>
        public void EraseFlash_Asyn(uint addr, uint size, Action<BusinessResult> retCallBack)
        {
            ProtocolTask task = new ProtocolTask()
            {
                Param = new EraseFlashParam()
                {
                    Addr = addr,
                    Size = size,
                },
                RetCallBack = retCallBack
            };

            new EraseFlashBussiness(task, this);
        }

        /// <summary>
        /// 擦除Flash
        /// </summary>
        /// <param name="addr">地址</param>
        /// <param name="size">大小</param>
        /// <returns>结果</returns>
        public BusinessResult EraseFlash(uint addr, uint size)
        {
            BusinessResult ret = null;
            AutoResetEvent e = new AutoResetEvent(false);
            EraseFlash_Asyn(addr, size, (callBack) =>
            {
                ret = callBack;
                e.Set();
            });
            e.WaitOne();
            return ret;
        }

        /// <summary>
        /// 读Efuse（异步）
        /// </summary>
        /// <param name="addr">地址</param>
        /// <param name="retCallBack">结果回调</param>
        public void ReadEfuse_Asyn(uint addr, Action<ReadEfuseResult> retCallBack)
        {
            ProtocolTask task = new ProtocolTask()
            {
                Param = addr,
                RetCallBack = (BusinessResult ret) =>
                {
                    retCallBack((ReadEfuseResult)ret);
                }
            };

            new ReadEfuseBussiness(task, this);
        }

        /// <summary>
        /// 读Efuse
        /// </summary>
        /// <param name="addr">地址</param>
        /// <returns>结果</returns>
        public ReadEfuseResult ReadEfuse(uint addr)
        {
            ReadEfuseResult ret = null;
            AutoResetEvent e = new AutoResetEvent(false);
            ReadEfuse_Asyn(addr, (callBack) =>
            {
                ret = callBack;
                e.Set();
            });
            e.WaitOne();
            return ret;
        }

        /// <summary>
        /// 写Efuse（异步）
        /// </summary>
        /// <param name="addr">地址</param>
        /// <param name="value">值</param>
        /// <param name="retCallBack">结果回调</param>
        public void WriteEfuse_Asyn(uint addr, uint value, Action<BusinessResult> retCallBack)
        {
            ProtocolTask task = new ProtocolTask()
            {
                Param = new WriteEfuseParam()
                {
                    Addr = addr,
                    Value = value,
                },
                RetCallBack = retCallBack
            };

            new WriteEfuseBussiness(task, this);
        }

        /// <summary>
        /// 写Efuse
        /// </summary>
        /// <param name="addr">地址</param>
        /// <param name="value">值</param>
        /// <returns>结果</returns>
        public BusinessResult WriteEfuse(uint addr, uint value)
        {
            BusinessResult ret = null;
            AutoResetEvent e = new AutoResetEvent(false);
            WriteEfuse_Asyn(addr, value, (callBack) =>
             {
                 ret = callBack;
                 e.Set();
             });
            e.WaitOne();
            return ret;
        }

        /// <summary>
        /// 写数据（异步）
        /// </summary>
        /// <param name="startAddr">起始地址</param>
        /// <param name="dataFile">文件</param>
        /// <param name="progressCallBack">写数据进度回调</param>
        /// <param name="retCallBack">结果回调</param>
        public void WriteData_Asyn(uint startAddr, string dataFile, Action<double> progressCallBack, Action<BusinessResult> retCallBack)
        {
            int maxLen = 0x7FFFFFF;
            byte[] buffer = new byte[maxLen];

            try
            {
                using (FileStream fsRead = new FileStream(dataFile, FileMode.Open, FileAccess.Read))
                {
                    int len = fsRead.Read(buffer, 0, maxLen);
                    Array.Resize(ref buffer, len);
                    fsRead.Close();
                }
            }
            catch
            {
                retCallBack(new BusinessResult(false, "Load DataFile Failed"));
                return;
            }

            WriteData_Asyn(startAddr, buffer, progressCallBack, retCallBack);
        }

        /// <summary>
        /// 写数据
        /// </summary>
        /// <param name="startAddr">起始地址</param>
        /// <param name="dataFile">文件</param>
        /// <param name="progressCallBack">写数据进度回调</param>
        /// <returns>结果</returns>
        public BusinessResult WriteData(uint startAddr, string dataFile, Action<double> progressCallBack)
        {
            BusinessResult ret = null;
            AutoResetEvent e = new AutoResetEvent(false);
            WriteData_Asyn(startAddr, dataFile, progressCallBack, (callBack) =>
            {
                ret = callBack;
                e.Set();
            });
            e.WaitOne();
            return ret;
        }

        /// <summary>
        /// 写数据（异步）
        /// </summary>
        /// <param name="startAddr">起始地址</param>
        /// <param name="datas">数据</param>
        /// <param name="progressCallBack">写数据进度回调</param>
        /// <param name="retCallBack">结果回调</param>
        public void WriteData_Asyn(uint startAddr, byte[] datas, Action<double> progressCallBack, Action<BusinessResult> retCallBack)
        {
            if (datas.Length == 0)
            {
                retCallBack(new BusinessResult(false, "DataFile is Empty"));
            }

            WriteDataParam para = new WriteDataParam()
            {
                StartAddr = startAddr,
                Datas = datas,
                ProgressCallBack = progressCallBack,
            };

            ProtocolTask task = new ProtocolTask()
            {
                Param = para,
                RetCallBack = (BusinessResult ret) =>
                {
                    retCallBack(ret);
                }
            };

            new WriteDataBussiness(task, this);
        }

        /// <summary>
        /// 写数据
        /// </summary>
        /// <param name="startAddr">起始地址</param>
        /// <param name="datas">数据</param>
        /// <param name="progressCallBack">写数据进度回调</param>
        /// <returns>结果</returns>
        public BusinessResult WriteData(uint startAddr, byte[] datas, Action<double> progressCallBack)
        {
            BusinessResult ret = null;
            AutoResetEvent e = new AutoResetEvent(false);
            WriteData_Asyn(startAddr, datas, progressCallBack, (callBack) =>
            {
                ret = callBack;
                e.Set();
            });
            e.WaitOne();
            return ret;
        }

        /// <summary>
        /// 读数据（异步）
        /// </summary>
        /// <param name="startAddr">起始地址</param>
        /// <param name="size">大小</param>
        /// <param name="file">保存文件</param>
        /// <param name="retCallBack">结果回调</param>
        public void ReadData_Asyn(uint startAddr, int size, string file, Action<BusinessResult> retCallBack)
        {
            ReadDataParam para = new ReadDataParam()
            {
                StartAddr = startAddr,
                Size = size,
                File = file,
            };

            ProtocolTask task = new ProtocolTask()
            {
                Param = para,
                RetCallBack = (BusinessResult ret) =>
                {
                    retCallBack(ret);
                }
            };

            new ReadDataBussiness(task, this);
        }

        /// <summary>
        /// 读数据
        /// </summary>
        /// <param name="startAddr">起始地址</param>
        /// <param name="size">大小</param>
        /// <param name="file">保存文件</param>
        /// <returns>结果</returns>
        public BusinessResult ReadData(uint startAddr, int size, string file)
        {
            BusinessResult ret = null;
            AutoResetEvent e = new AutoResetEvent(false);
            ReadData_Asyn(startAddr, size, file, (callBack) =>
            {
                ret = callBack;
                e.Set();
            });
            e.WaitOne();
            return ret;
        }

        /// <summary>
        /// 执行（异步）
        /// </summary>
        /// <param name="addr">地址</param>
        /// <param name="retCallBack">结果回调</param>
        public void Execute_Asyc(uint addr, Action<BusinessResult> retCallBack)
        {
            ProtocolTask task = new ProtocolTask()
            {
                Param = addr,
                RetCallBack = (BusinessResult ret) =>
                {
                    retCallBack(ret);
                }
            };

            new ExecuteBussiness(task, this);
        }

        /// <summary>
        /// 执行
        /// </summary>
        /// <param name="addr">地址</param>
        /// <returns>结果</returns>
        public BusinessResult Execute(uint addr)
        {
            BusinessResult ret = null;
            AutoResetEvent e = new AutoResetEvent(false);
            Execute_Asyc(addr, (callBack) =>
            {
                ret = callBack;
                e.Set();
            });
            e.WaitOne();
            return ret;
        }

        /// <summary>
        /// 查询电源
        /// </summary>
        /// <param name="retCallBack"></param>
        public void QueryPower(Action<BusinessResult> retCallBack)
        {
            ProtocolTask task = new ProtocolTask()
            {
                RetCallBack = (BusinessResult ret) =>
                {
                    retCallBack(ret);
                }
            };

            new QueryPower(task, this);
        }

        /// <summary>
        /// 查询电源
        /// </summary>
        /// <returns></returns>
        public BusinessResult QueryPower()
        {
            BusinessResult ret = null;
            AutoResetEvent e = new AutoResetEvent(false);
            QueryPower((callBack) =>
            {
                ret = callBack;
                e.Set();
            });
            e.WaitOne();
            return ret;
        }

        /// <summary>
        /// 生产（异步）
        /// </summary>
        /// <param name="para"></param>
        /// <param name="retCallBack"></param>
        public void Prodution_Asyn(IProduction para, Action<BusinessResult> retCallBack)
        {
            ProtocolTask task = new ProtocolTask()
            {
                Param = para,
                RetCallBack = (BusinessResult ret) =>
                {
                    retCallBack(ret);
                }
            };

            new ExecuteBussiness(task, this);
        }

        /// <summary>
        /// 生产
        /// </summary>
        /// <param name="para"></param>
        /// <returns></returns>
        public BusinessResult Prodution(IProduction para)
        {
            BusinessResult ret = null;
            AutoResetEvent e = new AutoResetEvent(false);
            Prodution_Asyn(para, (callBack) =>
            {
                ret = callBack;
                e.Set();
            });
            e.WaitOne();
            return ret;
        }
        
        #endregion

        #region NVS Function
        /// <summary>
        /// Write data through ItemID
        /// </summary>
        /// <param name="ConnectParam"></param>
        /// <param name="ItemData"></param>
        /// <returns></returns>
        public BusinessResult WriteNVItemDataByID(NVWriteParam param, Action<double> progressCallBack, byte[] ItemData)
        {
            //connect nvs system
            BusinessResult BResult = null;
            Action<BusinessResult> retCallBack = (value) =>
            {
                BResult = value;
            };
            ConnectNVSForWrite(param, retCallBack);
            if (BResult.Result == false)
            {
                return BResult;
            }

            BResult = WriteNVItemData(ItemData, progressCallBack);

            return BResult;
        }

        /// <summary>
        /// Read NV Item Data
        /// </summary>
        /// <param name="param"></param>
        /// <param name="progressCallBack"></param>
        /// <param name="ItemDataLength"></param>
        /// <returns></returns>
        public ReadNVDataResult ReadNVItemDataByID(NVReadParam param, Action<double> progressCallBack, int ItemDataLength)
        {
            //connect nvs system
            BusinessResult connectResult = null;
            Action<BusinessResult> retCallBack = (value) =>
            {
                connectResult = value;
            };
            ConnectNVSForRead(param, retCallBack);

            if (connectResult.Result == false)
            {
                return new ReadNVDataResult()
                {
                    Result = false,
                    Msg = connectResult.Msg
                };
            }

            ReadNVDataResult BResult = null;
            BResult = ReadNVItemData(ItemDataLength, progressCallBack);

            return BResult;
        }

        public BoolQResult WriteNVDataToPhone(NVWriteParam param, Action<double> progressCallBack)
        {
            //Connect nvs system


            return new BoolQResult(true);
        }

        /// <summary>
        /// Read NV Data From Phone
        /// </summary>
        /// <param name="param"></param>
        /// <param name="progressCallBack"></param>
        /// <returns></returns>
        public ReadNVDataResult ReadNVDataFromPhone(NVReadParam param, Action<double> progressCallBack)
        {
            //connect nvs system
            BusinessResult connectResult = null;
            Action<BusinessResult> retCallBack = (value) =>
            {
                connectResult = value;
            };
            ConnectNVSForRead(param, retCallBack);

            if (connectResult.Result == false)
            {
                return new ReadNVDataResult()
                {
                    Result = false,
                    Msg = connectResult.Msg
                };
            }

            ReadNVDataResult BResult = null;
            PartitionType partitionType = CmindCommon.GetPartitionType(param.ItemID);

            if (partitionType == PartitionType.RO)
            {
                int length = nvsSysInfo.ROSectorCount * nvsSysInfo.ROSectorSize;
                BResult = ReadNVItemData(length, progressCallBack);
                BResult.sectorCount = nvsSysInfo.ROSectorCount;
                BResult.sectorSize = nvsSysInfo.ROSectorSize;
            }
            else if(partitionType == PartitionType.RW)
            {
                int length = nvsSysInfo.RWSectorCount * nvsSysInfo.RWSectorSize;
                BResult = ReadNVItemData(length, progressCallBack);
                BResult.sectorCount = nvsSysInfo.RWSectorCount;
                BResult.sectorSize = nvsSysInfo.RWSectorSize;
            }

            return BResult;
        }

        public void ConnectNVSForWrite(NVWriteParam param, Action<BusinessResult> retCallBack)
        {
            AutoResetEvent e = new AutoResetEvent(false);
            ProtocolTask task = new ProtocolTask()
            {
                Param = param,
                RetCallBack = (BusinessResult ret) =>
                {
                    retCallBack(ret);
                    e.Set();
                }
            };
            new ConnectNVForWrite(task, this);

            e.WaitOne();
        }

        public void ConnectNVSForRead(NVReadParam param, Action<BusinessResult> retCallBack)
        {
            AutoResetEvent e = new AutoResetEvent(false);
            ProtocolTask task = new ProtocolTask()
            {
                Param = param,
                RetCallBack = (BusinessResult ret) =>
                {
                    retCallBack(ret);
                    e.Set();
                }
            };
            new ConnectNVForRead(task, this);

            e.WaitOne();
        }

        /// <summary>
        /// WriteNVItemData_Asyc
        /// </summary>
        /// <param name="ItemData"></param>
        /// <param name="progressCallBack"></param>
        /// <param name="retCallBack"></param>
        public void WriteNVItemData_Asyc(byte[] ItemData, Action<double> progressCallBack, Action<BusinessResult> retCallBack)
        {
            WriteItemParam ItemParam = new WriteItemParam()
            {
                dataItem = ItemData,
                ProgressCallBack = progressCallBack
            };

            ProtocolTask task = new ProtocolTask()
            {
                Param = ItemParam,
                RetCallBack = (BusinessResult ret) =>
                {
                    retCallBack(ret);
                }
            };

            new WriteNVItemData(task, this);
        }

        /// <summary>
        /// write nv item data
        /// </summary>
        /// <param name="ItemData"></param>
        /// <param name="progressCallBack"></param>
        /// <returns></returns>
        public BusinessResult WriteNVItemData(byte[] ItemData, Action<double> progressCallBack)
        {
            BusinessResult ret = null;
            AutoResetEvent e = new AutoResetEvent(false);
            WriteNVItemData_Asyc(ItemData, progressCallBack, (callBack) =>
            {
                ret = callBack;
                e.Set();
            });
            e.WaitOne();
            return ret;
        }

        public void ReadNVItemData_Asyc(int itemDataLength, Action<double> progressCallBack, Action<ReadNVDataResult> retCallBack)
        {
            ReadItemParam ItemParam = new ReadItemParam()
            {
                itemDataLength = itemDataLength,
                ProgressCallBack = progressCallBack
            };

            ProtocolTask task = new ProtocolTask()
            {
                Param = ItemParam,
                RetCallBack = (ret) =>
                {
                    retCallBack((ReadNVDataResult)ret);
                }
            };

            new ReadNVItemData(task, this);
        }

        public ReadNVDataResult ReadNVItemData(int itemDataLength, Action<double> progressCallBack)
        {
            ReadNVDataResult ret = null;
            AutoResetEvent e = new AutoResetEvent(false);
            ReadNVItemData_Asyc(itemDataLength, progressCallBack, (callBack) =>
            {
                ret = callBack;
                e.Set();
            });
            e.WaitOne();
            return ret;
        }

        #endregion
    }
}
