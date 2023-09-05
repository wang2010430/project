/***************************************************************************************************
* copyright : 芯迈微半导体（珠海）有限公司
* version   : 1.00
* file      : DLCommon.cs
* date      : 2023/04/27
* author    : haozhe.ni
* brief     : 
* section Modification History
* - 1.0 : Initial version - haozhe.ni
***************************************************************************************************/

using CmindProtocol.DLL;
using Common;

namespace CmindProtocol
{
    public static class CmindCommon
    {
        /// <summary>
        /// 大小端配置
        /// </summary>
        public static Endian DataEndian = Endian.LittleEndian;

        /// <summary>
        /// 擦除分区时间
        /// </summary>
        public static int EraseSectorInterval = 10000;

        /// <summary>
        /// 文件校验
        /// </summary>
        /// <param name="bytes"></param>
        /// <returns></returns>
        public static byte[] CheckFile(byte[] bytes)
        {
            ushort crc = Check_Crc16(0, bytes);
            return DataConvert.UInt16ToByte(crc, DataEndian);
        }

        /// <summary>
        /// CRC16校验
        /// </summary>
        /// <param name="crc"></param>
        /// <param name="buf"></param>
        /// <returns></returns>
        public static ushort Check_Crc16(ushort crc,byte[] buf)
        {
            uint i;

            crc = (ushort)~crc;
            foreach(var b in buf)
            {
                crc ^= b;
                for(i = 0; i < 8; ++i)
                {
                    if ((crc & 1) > 0)
                    {
                        crc = (ushort)((crc >> 1) ^ 0x8408);
                    }
                    else
                    {
                        crc = (ushort)(crc >> 1);
                    }
                }
            }

            return (ushort)~crc;
        }

        public static PartitionType GetPartitionType(ushort itemID)
        {
            // Extract the high 3 bits (bits 13, 14, and 15) from the ushort.
            ushort partitionBits = (ushort)(itemID >> 14);

            // Map the extracted value to the corresponding PartitionType using a switch statement.
            switch (partitionBits)
            {
                case 0:
                    return PartitionType.RO;
                case 1:
                    return PartitionType.RW;
                // Handle other cases or throw an exception if needed.
            }

            return PartitionType.RW;
        }
    }
}
