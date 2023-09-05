/***************************************************************************************************
* copyright : 芯迈微半导体（珠海）有限公司
* version   : 1.00
* file      : CRC16X25.cs
* date      : 2023/8/20 10:20:35
* author    : jinlong.wang
* brief     : CRC-16/X25
* section Modification History
* - 1.0 : Initial version - jinlong.wang
***************************************************************************************************/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Common
{
    public class CRC16X25
    {
        private ushort[] crcTable = new ushort[256];
        public CRC16X25()
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

        public ushort Calculate(byte[] data)
        {
            ushort crc = 0xFFFF;
            foreach (byte b in data)
            {
                crc = (ushort)((crc << 8) ^ crcTable[((crc >> 8) ^ b) & 0xFF]);
            }
            return crc;
        }
    }
}
