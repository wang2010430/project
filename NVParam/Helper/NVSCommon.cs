/***************************************************************************************************
* copyright : CMIND-SEMI
* version   : 1.00
* file      : NVSCommon.cs
* date      : 2023/8/11 9:41:46
* author    : jinlong.wang
* brief     : 
* section Modification History
* - 1.0 : Initial version - jinlong.wang
***************************************************************************************************/

using Common;
using log4net;
using System;
using System.Collections.Generic;

namespace NVParam.Helper
{
    internal class NVSCommon
    {
        internal static byte[] Separator = new byte[] { 0xFE, 0xFA, 0xFE, 0xFA, 0xFE, 0xFA };
        /// <summary>
        /// 大小端配置
        /// </summary>
        internal static Endian DataEndian = Endian.LittleEndian;

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
                LogNetHelper.Warn($"Key {key} already exists. Value will be updated.");
                dictionary[key] = value;
            }
            else
            {
                dictionary.Add(key, value);
            }
        }
    }
}
