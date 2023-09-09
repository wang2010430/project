/***************************************************************************************************
* copyright : 芯迈微半导体（珠海）有限公司
* version   : 1.00
* file      : NVSParam.cs
* date      : 2023/8/5 11:54:42
* author    : jinlong.wang
* brief     : 
* section Modification History
* - 1.0 : Initial version - jinlong.wang
***************************************************************************************************/

using System.Runtime.InteropServices;

namespace NVParam.DAL
{
    /// <summary>
    /// 保存属性
    /// </summary>
    public enum SectorAttribute
    {
        RO = 0, //可读
        RW = 1 //可读写
    };

    /// <summary>
    /// 保存参数
    /// </summary>
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct StorageParam
    {
        public SectorAttribute SAttribute;
        public int SectorSize;
        public int SectorCount;
        public ushort CRC;
        public byte[] SectorData;
    }
}
