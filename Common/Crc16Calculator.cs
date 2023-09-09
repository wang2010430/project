/***************************************************************************************************
* copyright : 芯迈微半导体（珠海）有限公司
* version   : 1.00
* file      : Crc16Calculator.cs
* date      : 2023/8/20 10:20:35
* author    : jinlong.wang
* brief     : Provides methods for calculating CRC-16/X25 checksums.
* section Modification History
* - 1.0 : Initial version - jinlong.wang
***************************************************************************************************/

namespace Common
{ 
    public static class Crc16Calculator
    {
        #region Attribute
        private static ushort[] crcTable = new ushort[256];
        #endregion

        #region Constructor
        static Crc16Calculator()
        {
            InitializeCRCTable();
        }
        #endregion

        #region Function
        /// <summary>
        /// Initializes the CRC table with precomputed values.
        /// </summary>
        private static void InitializeCRCTable()
        {
            ushort polynomial = 0x1021; // CRC-16/X25 polynomial
            for (ushort i = 0; i < 256; i++)
            {
                ushort crc = 0;
                ushort c = (ushort)(i << 8);
                for (int j = 0; j < 8; j++)
                {
                    if (((crc ^ c) & 0x8000) != 0)
                    {
                        crc = (ushort)((crc << 1) ^ polynomial);
                    }
                    else
                    {
                        crc <<= 1;
                    }
                    c <<= 1;
                }
                crcTable[i] = crc;
            }
        }

        /// <summary>
        /// Calculates the CRC-16/X25 checksum for the given byte array.
        /// </summary>
        /// <param name="data">The input byte array.</param>
        /// <returns>The CRC-16/X25 checksum as a ushort value.</returns>
        public static ushort Calculate(byte[] data)
        {
            ushort crc = 0xFFFF;
            foreach (byte b in data)
            {
                crc = (ushort)((crc << 8) ^ crcTable[((crc >> 8) ^ b) & 0xFF]);
            }
            return crc;
        }
        #endregion
    }
}
