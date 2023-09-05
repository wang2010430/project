/***************************************************************************************************
* copyright : 芯迈微半导体（珠海）有限公司
* version   : 1.00
* file      : Prodution_ConstPower.cs
* date      : 2023/07/11
* author    : haozhe.ni
* brief     : 
* section Modification History
* - 1.0 : Initial version - haozhe.ni
***************************************************************************************************/

using Common;
using System.Collections.Generic;

namespace CmindProtocol.CmindBusiness.ProdutionPara
{
    public enum Bandwidth
    {
        LTE_BW_1P4M = 0,

        LTE_BW_3M,

        LTE_BW_5M,

        LTE_BW_10M,

        LTE_BW_15M,

        LTE_BW_20M,
    }

    public enum APCType
    {
        /// <summary>
        /// 控制字
        /// </summary>
        ControlWord = 0x0000,

        /// <summary>
        /// Dbm值
        /// </summary>
        DbmValue = 0x1111,

        /// <summary>
        /// Index值
        /// </summary>
        Index = 0x2222,
    }

    public enum SignalType
    {
        /// <summary>
        /// 单音信号
        /// </summary>
        SingleTone = 0,

        /// <summary>
        /// 调制信号
        /// </summary>
        ModulatedSignal = 1,
    }

    public class Prodution_ConstPower : IProduction
    {
        public ProductPhyStrutType StructType { get; } = ProductPhyStrutType.ConstPower;

        public byte[] GetBytes()
        {
            List<byte> bytes = new List<byte>();
            bytes.Add(Band);
            bytes.Add((byte)Bandwidth);
            bytes.AddRange(DataConvert.UIntToByte(Earfcn));
            bytes.AddRange(DataConvert.UInt16ToByte((ushort)APCType));
            ushort code = Code_TxGainControlWord;
            code += (ushort)((Code_PAMode ? 1 : 0) << 11);
            code += (ushort)(Code_APTSection<< 12);
            bytes.AddRange(DataConvert.UInt16ToByte(code));
            bytes.Add((byte)SignalType);
            bytes.Add(Tx_Frame_Bitmap);
            bytes.Add(RF_CH_Ind);
            bytes.AddRange(AMT_LTE_UL_Cfg.GetBytes());
            bytes.AddRange(AMT_LTE_CELL_INFO_T.GetBytes());
            bytes.AddRange(AMT_LTE_MIB_INFO_T.GetBytes());

            return bytes.ToArray();
        }

        /// <summary>
        /// 频段号
        /// </summary>
        public byte Band = 0;

        /// <summary>
        /// 系统带宽
        /// </summary>
        public Bandwidth Bandwidth = Bandwidth.LTE_BW_1P4M;

        /// <summary>
        /// 频点号
        /// </summary>
        public uint Earfcn = 0;

        /// <summary>
        /// APC类型指示
        /// </summary>
        public APCType APCType = APCType.ControlWord;

        /// <summary>
        /// APT分段Indx
        /// </summary>
        public byte Code_APTSection = 0;

        /// <summary>
        /// PA Mode切换Flag
        /// </summary>
        public bool Code_PAMode = false;

        /// <summary>
        /// Tx增益控制字
        /// </summary>
        public ushort Code_TxGainControlWord = 0;

        /// <summary>
        /// 信号类型
        /// </summary>
        public SignalType SignalType = SignalType.SingleTone;

        /// <summary>
        /// 10个子帧TX bitmap,10bits有效，
        /// bit:
        /// 1:发送
        /// 0:不发送
        /// 对于TDD,TX最多连续3个1
        /// 对于FDD,10bits可都为1,即0x无限制
        /// </summary>
        public byte Tx_Frame_Bitmap = 0;

        /// <summary>
        /// RF通道,暂不填
        /// </summary>
        public byte RF_CH_Ind = 0;

        /// <summary>
        /// UL配置信息
        /// </summary>
        public AMT_LTE_UL_Cfg AMT_LTE_UL_Cfg = new AMT_LTE_UL_Cfg();

        /// <summary>
        /// CELL配置信息
        /// </summary>
        public AMT_LTE_CELL_INFO_T AMT_LTE_CELL_INFO_T = new AMT_LTE_CELL_INFO_T();

        /// <summary>
        /// MIB配置信息
        /// </summary>
        public AMT_LTE_MIB_INFO_T AMT_LTE_MIB_INFO_T = new AMT_LTE_MIB_INFO_T();
    }
}
