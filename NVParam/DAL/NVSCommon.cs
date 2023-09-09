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

namespace NVParam.DAL
{

    //ATE 的类型
    public enum ATEType
    {
        Close,
        GC,
        Delete
    }

    //分区类型
    public enum SectorType
    {
        OpenSector,
        CloseSector,
        WriteSector,
        EmptySector,
        NullSector
    }
}
