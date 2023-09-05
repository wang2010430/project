/***************************************************************************************************
* copyright : CMIND-SEMI
* version   : 1.00
* file      : ItemIDType.cs
* date      : 2023/8/1 10:08:49
* author    : jinlong.wang
* brief     : 
* section Modification History
* - 1.0 : Initial version - jinlong.wang
***************************************************************************************************/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NVParam.DAL
{
    public enum PartitionType
    {
        FIXNV_RO = 0,
        FIXNV_RW = 1
    }

    public enum Domain
    {
        Commmon = 0,
        RF = 1,
        HW = 2,
        PS = 3,
        PHY = 4,
        Platform = 5,
        Calibration = 6,
    }

}
