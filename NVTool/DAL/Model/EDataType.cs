/***************************************************************************************************
* copyright : CMIND-SEMI
* version   : 1.00
* file      : NVParam.cs
* date      : 2023/7/3 17:09:45
* author    : jinlong.wang
* brief     : 
* section Modification History
* - 1.0 : Initial version (2023/7/3 17:09:45) - jinlong.wang
***************************************************************************************************/

using System;
using System.Linq;

namespace NVTool.DAL.Model
{
    /// <summary>
    /// 数据类型
    /// </summary>
    public enum EDataType
    {
        Project,
        Module,
        Array,
        Struct,
        BYTE,
        SHORT,
        USHORT,
        INT,
        UINT,
        LONG,
    }
}
