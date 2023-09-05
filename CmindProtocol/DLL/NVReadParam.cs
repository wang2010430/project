/***************************************************************************************************
* copyright : CMIND-SEMI
* version   : 1.00
* file      : NVReadParam.cs
* date      : 2023/7/11 
* author    : jinlong.wang
* brief     : NV Param
* section Modification History
* - 1.0 : Initial version - jinlong.wang
***************************************************************************************************/

using System;
using System.Runtime.InteropServices;

namespace CmindProtocol.DLL
{
    /// <summary>
    /// Connect Param
    /// </summary>
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct NVReadParam
    {
        public ushort ItemID;
        public ushort Length;
        public byte OperationMode;
    }

    /// <summary>
    /// Read ItemData
    /// </summary>
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct NVReadData
    { 
        public ushort Total;
        public ushort Current;
        public ushort Length;
    }
}
