/***************************************************************************************************
* copyright : 芯迈微半导体（珠海）有限公司
* version   : 1.00
* file      : Class1.cs
* date      : 2023/8/5 8:47:34
* author    : jinlong.wang
* brief     : 
* section Modification History
* - 1.0 : Initial version - jinlong.wang
***************************************************************************************************/

using System;
using System.Runtime.InteropServices;

namespace NVParam.DAL
{
    /* Allocation Table Entry */
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct NVSAte
    {
        public ushort id;       /* data id */
        public ushort offset;   /* data offset within sector */
        public ushort len;      /* data len within sector */
        public byte part;     /* part of a multipart data - future extension */
        public byte crc8;     /* crc8 check of the entry */
    }
}
