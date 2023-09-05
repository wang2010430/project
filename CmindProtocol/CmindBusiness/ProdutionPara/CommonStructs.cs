/***************************************************************************************************
* copyright : 芯迈微半导体（珠海）有限公司
* version   : 1.00
* file      : CommonStructs.cs
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
    public class AMT_LTE_UL_Cfg
    {
        public byte[] GetBytes()
        {
            List<byte> bytes = new List<byte>();
            bytes.Add(Hopping_Flag);
            bytes.Add(Hopping_Bit);
            bytes.Add(RB_Start);
            bytes.Add(RB_Num);
            bytes.Add(Q_M);
            bytes.Add(DMRS_CS);
            bytes.AddRange(DataConvert.UInt16ToByte(Tbs));
            bytes.AddRange(DataConvert.UInt16ToByte(Dai));
            bytes.AddRange(DataConvert.UInt16ToByte(CQI_REQ_FLAG));
            bytes.AddRange(DataConvert.UInt16ToByte(Tx_Ant_Bitmap));
            bytes.AddRange(DataConvert.UInt16ToByte(UL_Index));
            bytes.Add(NDI);
            bytes.Add(RV);
            bytes.AddRange(DataConvert.UInt16ToByte(Alpha2));
            bytes.AddRange(DataConvert.UInt16ToByte(Alpha3));
            bytes.AddRange(DataConvert.UInt16ToByte(Alpha5));
            bytes.AddRange(DataConvert.UInt16ToByte(Alpha2_Pwr));
            bytes.AddRange(DataConvert.UInt16ToByte(Alpha3_Pwr));
            bytes.AddRange(DataConvert.UInt16ToByte(Alpha5_Pwr));
            bytes.AddRange(DataConvert.UInt16ToByte(Rnti_Type));
            bytes.AddRange(DataConvert.UInt16ToByte(Rnti_Value));

            return bytes.ToArray();
        }

        /// <summary>
        /// 跳频标志[0,1]
        /// </summary>
        public byte Hopping_Flag = 0;

        /// <summary>
        /// 跳频比特[0,1,2,3]
        /// </summary>
        public byte Hopping_Bit = 0;

        /// <summary>
        /// UL分配的起始RB
        /// </summary>
        public byte RB_Start;

        /// <summary>
        /// UL分配的RB数
        /// </summary>
        public byte RB_Num;

        /// <summary>
        /// 调制除数
        /// QPSK
        /// 16QAM
        /// </summary>
        public byte Q_M = 0;

        /// <summary>
        /// Cyclic shift for DMRS:3Bits
        /// [0,7]
        /// </summary>
        public byte DMRS_CS = 0;

        /// <summary>
        /// TB Size
        /// [0,5160]
        /// </summary>
        public ushort Tbs = 1;

        /// <summary>
        /// Downlink Assignment Index
        /// </summary>
        public ushort Dai;

        /// <summary>
        /// CQI Request
        /// </summary>
        public ushort CQI_REQ_FLAG = 0;

        /// <summary>
        /// 暂不填
        /// </summary>
        public ushort Tx_Ant_Bitmap = 0;

        /// <summary>
        /// UL index:DCI0
        /// </summary>
        public ushort UL_Index = 0;

        /// <summary>
        /// New Data Indicator
        /// </summary>
        public byte NDI;

        /// <summary>
        /// Redundancy Version(inclded in mcs)
        /// </summary>
        public byte RV;

        /// <summary>
        /// Rserved
        /// </summary>
        public ushort Alpha2;

        /// <summary>
        /// Rserved
        /// </summary>
        public ushort Alpha3;

        /// <summary>
        /// Rserved
        /// </summary>
        public ushort Alpha5;

        /// <summary>
        /// Rserved
        /// </summary>
        public ushort Alpha2_Pwr;

        /// <summary>
        /// Rserved
        /// </summary>
        public ushort Alpha3_Pwr;

        /// <summary>
        /// Rserved
        /// </summary>
        public ushort Alpha5_Pwr;

        /// <summary>
        /// Rserved
        /// </summary>
        public ushort Rnti_Type;

        /// <summary>
        /// Rserved
        /// </summary>
        public ushort Rnti_Value;
    }

    public class AMT_LTE_CELL_INFO_T
    {
        public byte[] GetBytes()
        {
            List<byte> bytes = new List<byte>();
            bytes.AddRange(DataConvert.UInt16ToByte(Earfcn));
            bytes.Add(PHY_CELL_ID);
            bytes.Add(POWER_CLASS);

            return bytes.ToArray();
        }

        /// <summary>
        /// 频点号
        /// </summary>
        public ushort Earfcn = 0;

        /// <summary>
        /// 小区ID
        /// </summary>
        public byte PHY_CELL_ID = 0;

        /// <summary>
        /// Maximum RF output power of UE(dbm) according to the UE power class
        /// 目前只支持power class 3
        /// [1,2,3,4]
        /// </summary>
        public byte POWER_CLASS = 0;
    }

    public class AMT_LTE_MIB_INFO_T
    {
        public byte[] GetBytes()
        {
            List<byte> bytes = new List<byte>();
            bytes.Add(DL_Bandwidth);
            bytes.Add(Phich_Duration);
            bytes.Add(Phich_Resource);
            bytes.Add(Rsvd);

            return bytes.ToArray();
        }

        /// <summary>
        /// [6,15,25,50,75,100]
        /// </summary>
        public byte DL_Bandwidth = 6;

        /// <summary>
        /// PHICH-Duration
        /// 0:Normal;
        /// 1:Extended
        /// </summary>
        public byte Phich_Duration = 0;

        /// <summary>
        /// 0:OneSixth
        /// 1:Half
        /// 2:One
        /// 3:Two
        /// </summary>
        public byte Phich_Resource = 0;

        public byte Rsvd = 0;
    }
}
