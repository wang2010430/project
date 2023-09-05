/***************************************************************************************************
* copyright : 芯迈微半导体（珠海）有限公司
* version   : 1.00
* file      : DLType.cs
* date      : 2023/04/27
* author    : haozhe.ni
* brief     : 
* section Modification History
* - 1.0 : Initial version - haozhe.ni
***************************************************************************************************/
namespace CmindProtocol
{
    /// <summary>
    /// 下载命令码
    /// </summary>
    internal enum CmindCommand
    {
        Connect = 0x01,

        WriteData = 0x02,

        ReadData = 0x03,

        Execute = 0x04,

        ChangeBaud = 0x05,

        EraseFlash = 0x06,

        ReadEfuse = 0x07,

        WriteEFuse = 0x08,

        Log = 0x09,

        Crc = 0x0A,

        Assert = 0x0B,

        AssertOperation = 0x0C,

        AssertRead = 0x0D,

        QueryPower = 0x0E,

        Prodution = 0x0F,
    }

    /// <summary>
    /// 下载否定应答原因
    /// </summary>
    internal enum CmindNegReason
    {
        GeneralError = 0x01,

        UnknowCMDType = 0x02,

        UnknowRemoteType = 0x03,

        InvalidCrc = 0x04,

        InvalidTotalSequence = 0x05,

        InvalidPresentSequence = 0x06,

        InvalidFileSize = 0x07,

        InvalidAddress = 0x08,
    }

    /// <summary>
    /// 远方类型
    /// </summary>
    internal enum OperationType
    {
        UnKnown = -1,
        Ps = 0,
        Phy = 1,
        Platform = 2,
        PHY_Log = 0x20,
        PLAT_Log = 0x40,
        PLAT_Diag_NV = 0x41,
        PLAT_Diag_AT = 0x42,
        PLAT_Diag_Log = 0x43,
        PLAT_Diag_Assert = 0x44,
        PLAT_Diag_Shell = 0x45,
        PLAT_Diag_CFT = 0x46,
        PLAT_Diag_WriteNum = 0x47,
        PLAT_Diag_VeriftTest = 0x48,
        PLAT_Diag_Common = 0x5F
    }

    /// <summary>
    /// Log类型
    /// </summary>
    public enum LogType
    {
        Msg = 0,
        Hex,
        Print
    }

    public enum ProductPhyStrutType
    {
        SetMode = 0,

        ConstPower = 1,

        DynamicPower = 2,

        ReadTBL = 3,
    }

}
