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

using Common;
using log4net;
using NVParam.DAL;
using System;
using System.Collections.Generic;

namespace NVParam.Helper
{
    public class NVCommon
    {
        #region Attribute
        // 大小端配置
        public static Endian DataEndian = Endian.LittleEndian;
        public static byte[] Separator = new byte[] { 0xFE, 0xFA, 0xFE, 0xFA, 0xFE, 0xFA };
        // NVS Sys
        public const int ROSectorSize = 56;
        public const int ROSectorCount = 2;
        public const int RWSectorSize = 48;
        public const int RWSectorCount = 3;
        #endregion

        #region Common Function
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



        /// <summary>
        /// 根据偏移地址和长度，从数组中截取子数组
        /// </summary>
        /// <param name="source">源头</param>
        /// <param name="offset">偏移地址</param>
        /// <param name="length">长度</param>
        /// <returns></returns>
        public static byte[] GetSubArray(byte[] source, int offset, int length)
        {
            if (offset < 0 || offset + length > source.Length)
            {
                return null;  // 超出范围，返回 null 表示失败
            }

            byte[] subArray = new byte[length];
            Array.Copy(source, offset, subArray, 0, length);
            return subArray;
        }

        /// <summary>
        /// 更新字典的值
        /// </summary>
        /// <param name="dictionary">Item-Value</param>
        /// <param name="key"></param>
        /// <param name="value"></param>
        public static void AddOrUpdateValue(Dictionary<int, byte[]> dictionary, int key, byte[] value)
        {
            if (dictionary.ContainsKey(key))
            {
#if DEBUG
                LogNetHelper.Warn($"Key 0x{key:X2} already exists. Value will be updated.");
#endif
                dictionary[key] = value;
            }
            else
            {
                dictionary.Add(key, value);
            }
        }

        /// <summary>
        /// Converts an NVSParam object to a StorageParam object for a specific sector attribute.
        /// </summary>
        /// <param name="nvsParam">The NVSParam object to convert.</param>
        /// <param name="sectorAttribute">The sector attribute for the StorageParam.</param>
        /// <returns>A StorageParam object representing the converted parameters.</returns>
        public static StorageParam ConvertParameter(NVSParam nvsParam, SectorAttribute sectorAttribute)
        {
            // Prepare storage parameters for the RO sector.
            StorageParam ROParam = new StorageParam()
            {
                // Set the sector attribute.
                SAttribute = sectorAttribute,

                // Copy sector count and adjust sector size (assuming sector size is in KB).
                SectorCount = nvsParam.SectorCount,
                SectorSize = nvsParam.SectorSize * 1024,

                // Calculate CRC for the sector data.
                CRC = Crc16Calculator.Calculate(nvsParam.SectorData),

                // Copy sector data.
                SectorData = nvsParam.SectorData
            };

            return ROParam;
        }
        #endregion
    }
}
