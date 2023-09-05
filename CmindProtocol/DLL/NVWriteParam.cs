/***************************************************************************************************
* copyright : CMIND-SEMI
* version   : 1.00
* file      : ConnectToWriteBusiness.cs
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
    //需要跟协议对齐，不可随便更改
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct NVWriteParam
    {
        public ushort ItemID;
        public byte DownloadMode;
        public byte OperationMode;
        public uint Length;
    }

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct NVWriteData
    {
        public ushort Total;
        public ushort Current;
        public ushort Length;
        public ushort CRC;
    }
}
