/***************************************************************************************************
* copyright : 芯迈微半导体（珠海）有限公司
* version   : 1.00
* file      : Prodution_SetMode.cs
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
    // Modem工作模式
    public enum SetMode_DstMode
    {
        /// <summary>
        /// 正常模式
        /// </summary>
        MODE_NORMAL = 0,  
        
        /// <summary>
        /// LTE非信令测试模式
        /// </summary>
        MODE_AMT_LTE_NON_SIG_LTE = 0x1111,

        /// <summary>
        /// LTE信令测试模式
        /// </summary>
        MODE_AMT_LTE_SIGNAL = 0x2222,

        /// <summary>
        /// NR非信令测试模式
        /// </summary>
        MODE_AMT_NR_NON_SIG = 0x3333,
    }

    public class Prodution_SetMode : IProduction
    {
        public ProductPhyStrutType StructType { get; } = ProductPhyStrutType.SetMode;

        public SetMode_DstMode Mode = SetMode_DstMode.MODE_NORMAL;

        public byte[] GetBytes()
        {
            List<byte> bytes = new List<byte>();
            bytes.AddRange(DataConvert.UInt16ToByte((ushort)Mode, Endian.LittleEndian));
            bytes.AddRange(new byte[2]);

            return bytes.ToArray();
        }
    }
}
