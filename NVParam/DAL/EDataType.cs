/***************************************************************************************************
* copyright : CMIND-SEMI
* version   : 1.00
* file      : EDataType.cs
* date      : 2023/7/3 17:09:45
* author    : jinlong.wang
* brief     : Item Data Type
* section Modification History
* - 1.0 : Initial version (2023/7/3 17:09:45) - jinlong.wang
***************************************************************************************************/

namespace NVParam.DAL
{
    /// <summary>
    /// Item Data Type
    /// </summary>
    public enum EDataType
    {
        Class,
        Array,
        SBYTE,
        BYTE,
        SHORT,
        USHORT,
        INT,
        UINT,
        LONG,
        Unknown,
    }

    /// <summary>
    /// Item State
    /// </summary>
    public enum EItemState
    {
        inactive,
        active
    }
}
