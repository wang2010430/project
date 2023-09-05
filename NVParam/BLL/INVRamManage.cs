/***************************************************************************************************
* copyright : CMIND-SEMI
* version   : 1.00
* file      : INVRamManage.cs
* date      : 2023/7/10
* author    : jinlong.wang
* brief     : 接口类 NV参数管理类
* section Modification History
* - 1.0 : Initial version - jinlong.wang
***************************************************************************************************/

using Common;
using NVParam.DAL;

namespace NVParam.BLL
{
    partial interface INVRamManage
    {
        /// <summary>
        /// Loading xml file
        /// </summary>
        /// <param name="filePath"></param>
        BoolQResult LoadSource(string filePath);

        /// <summary>
        /// Read NV Param
        /// </summary>
        BoolQResult ReadNVParam(int ItemID, ItemDataNode node);

        /// <summary>
        /// Setting NV Param
        /// </summary>
        BoolQResult WriteNVParam(int ItemID, ItemDataNode node);
    }
}
