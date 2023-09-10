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

using System;
using System.Linq;

namespace NVParam.DAL
{
    /// <summary>
    /// NVS Param
    /// </summary>
    public class NVSParam
    {
        public int SectorSize = 0;
        public int SectorCount = 0;
        public byte[] SectorData;
    }
}
