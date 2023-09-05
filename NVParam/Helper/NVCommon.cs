/***************************************************************************************************
* copyright : CMIND-SEMI
* version   : 1.00
* file      : NVCommon.cs
* date      : 2023/8/1 10:38:39
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

namespace NVParam.Helper
{
    public class NVCommon
    {
        /// <summary>
        /// 生成ItemID，格式为分区(15-14), 领域(13-10)，其余自定义。
        /// </summary>
        /// <param name="partition"></param>
        /// <param name="domain"></param>
        /// <param name="customData"></param>
        /// <returns></returns>
        public static ushort GenerateItemID(byte partition, byte domain, ushort customData)
        {
            // Perform bitwise operations to combine partition, domain, and customData
            ushort itemID = (ushort)(
                ((partition & 0x03) << 14) |   // Extract high 2 bits of partition and move to bits 15-14
                ((domain & 0x0F) << 10) |     // Extract high 4 bits of domain and move to bits 13-10
                (customData & 0x3FF)           // Use the remaining 10 bits for customData
            );

            return itemID;
        }
    }
}
