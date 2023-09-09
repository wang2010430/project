/***************************************************************************************************
* copyright : CMIND-SEMI
* version   : 1.00
* file      : CRCCalculator.cs
* date      : 2023/9/9 13:55:47
* author    : jinlong.wang
* brief     : CRCCalculator
* section Modification History
* - 1.0 : Initial version - jinlong.wang
***************************************************************************************************/

namespace NVParam.Helper
{
    class CRCCalculator
    {
        private static readonly byte val = 0xFF;
        // CRC-8 CCITT lookup table
        private static readonly byte[] LookupTable = {  0x00, 0x07, 0x0E, 0x09,
                                                        0x1C, 0x1B, 0x12, 0x15,
                                                        0x38, 0x3F, 0x36, 0x31,
                                                        0x24, 0x23, 0x2A, 0x2D };

        /// <summary>
        /// Calculates CRC-8 checksum.
        /// </summary>
        /// <param name="initialValue">Initial CRC value.</param>
        /// <param name="data">Data buffer.</param>
        /// <param name="length">Length of the data buffer.</param>
        /// <returns>CRC-8 CCITT checksum.</returns>
        public static byte Calculate(byte initialValue, byte[] data, int length)
        {
            byte crc = initialValue;

            for (int i = 0; i < length; i++)
            {
                crc ^= data[i];
                crc = (byte)((crc << 4) ^ LookupTable[crc >> 4]);
                crc = (byte)((crc << 4) ^ LookupTable[crc >> 4]);
            }

            return crc;
        }

        public static byte Calculate(byte[] data)
        {
            return Calculate(val, data, data.Length);
        }
    }
}
