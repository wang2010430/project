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

namespace NVSSystem.DLL
{
    public class NVSConstants
    {
        public const uint ADDR_SECT_MASK = 0xFFFF0000;
        public const int ADDR_SECT_SHIFT = 16;
        public const uint ADDR_OFFS_MASK = 0x0000FFFF;
        public const int NVS_BLOCK_SIZE = 8;
        public const uint NVS_LOOKUP_CACHE_NO_ADDR = 0xFFFFFFFF;
    }
}
