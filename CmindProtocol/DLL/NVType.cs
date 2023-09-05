/***************************************************************************************************
* copyright : CMIND-SEMI
* version   : 1.00
* file      : NVCommand.cs
* date      : 2023/7/11 
* author    : jinlong.wang
* brief     : NV Command
* section Modification History
* - 1.0 : Initial version  - jinlong.wang
***************************************************************************************************/

namespace CmindProtocol.DLL
{
    /// <summary>
    /// NV Commond
    /// </summary>
    public enum NVCommand
    {
        ConnectToWrite = 1,
        WriteNVData = 2,
        ConnectToRead = 3,
        ReadNVData = 4
    }

    /// <summary>
    /// NV Download Mode
    /// </summary>
    public enum NVDownloadMode
    {
        NORMAL_MODE = 0,
        RF_PRIVILEGE_MODE = 1,
        NVEDITOR_PRIVILEGE_MODE = 3,
        AFTER_SALES_MODE = 4
    }

    /// <summary>
    /// NV Operate Mode
    /// </summary>
    public enum NVOperateMode
    {
        OneByOneMode = 0,
        WholeMode = 1
    }

    public enum PartitionType
    {
        RO = 0,
        RW = 1
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
