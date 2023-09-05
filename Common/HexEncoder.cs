/***************************************************************************************************
* copyright : 芯迈微半导体（珠海）有限公司
* version   : 1.00
* file      : HexEncoder.cs
* date      : 2023/03/16
* author    : haozhe.ni
* brief     : 编码处理
* section Modification History
* - 1.0 : Initial version - haozhe.ni
***************************************************************************************************/

using System.IO;
using System.Collections.Generic;

namespace Common
{
    public static class Hex
    {
        private static readonly IEncoder Encoder = new UrlBase64Encoder();

        public static byte[] Decode(string data)
        {
            var memoryStream = new MemoryStream((data.Length + 1) / 2);

            Encoder.DecodeString(data, memoryStream);

            return memoryStream.ToArray();
        }

        public static byte[] Encode(byte[] data)
        {
            return Encode(data, 0, data.Length);
        }

        private static byte[] Encode(byte[] data, int off, int length)
        {
            var memoryStream = new MemoryStream(length * 2);

            Encoder.Encode(data, off, length, memoryStream);

            return memoryStream.ToArray();
        }
    }

    public class UrlBase64Encoder : Base64Encoder
    {
        public UrlBase64Encoder()
        {
            Padding = 46;
            EncodingTable[EncodingTable.Length - 2] = 45;
            EncodingTable[EncodingTable.Length - 1] = 95;

            InitialiseDecodingTable();
        }
    }

    public class Base64Encoder : IEncoder
    {
        protected readonly byte[] EncodingTable =
        {
            65,
            66,
            67,
            68,
            69,
            70,
            71,
            72,
            73,
            74,
            75,
            76,
            77,
            78,
            79,
            80,
            81,
            82,
            83,
            84,
            85,
            86,
            87,
            88,
            89,
            90,
            97,
            98,
            99,
            100,
            101,
            102,
            103,
            104,
            105,
            106,
            107,
            108,
            109,
            110,
            111,
            112,
            113,
            114,
            115,
            116,
            117,
            118,
            119,
            120,
            121,
            122,
            48,
            49,
            50,
            51,
            52,
            53,
            54,
            55,
            56,
            57,
            43,
            47
        };

        protected byte Padding = 61;

        private readonly byte[] _decodingTable = new byte[128];

        private static void Fill(byte[] buf, byte b)
        {
            var num = buf.Length;

            while (num > 0)
            {
                buf[--num] = b;
            }
        }

        protected void InitialiseDecodingTable()
        {
            Fill(_decodingTable, byte.MaxValue);

            for (var i = 0; i < EncodingTable.Length; i++)
            {
                _decodingTable[EncodingTable[i]] = (byte)i;
            }
        }

        public int Encode(byte[] data, int off, int length, Stream outStream)
        {
            var num = length % 3;
            var num2 = length - num;

            for (var i = off; i < off + num2; i += 3)
            {
                var num3 = data[i] & 0xFF;
                var num4 = data[i + 1] & 0xFF;
                var num5 = data[i + 2] & 0xFF;

                outStream.WriteByte(EncodingTable[(uint)num3 >> 2 & 0x3F]);
                outStream.WriteByte(EncodingTable[(num3 << 4 | (int)((uint)num4 >> 4)) & 0x3F]);
                outStream.WriteByte(EncodingTable[(num4 << 2 | (int)((uint)num5 >> 6)) & 0x3F]);
                outStream.WriteByte(EncodingTable[num5 & 0x3F]);
            }

            switch (num)
            {
                case 1:
                    {
                        var num6 = data[off + num2] & 0xFF;
                        var num8 = num6 >> 2 & 0x3F;
                        var num9 = num6 << 4 & 0x3F;

                        outStream.WriteByte(EncodingTable[num8]);
                        outStream.WriteByte(EncodingTable[num9]);
                        outStream.WriteByte(Padding);
                        outStream.WriteByte(Padding);

                        break;
                    }

                case 2:
                    {
                        var num6 = data[off + num2] & 0xFF;
                        var num7 = data[off + num2 + 1] & 0xFF;
                        var num8 = num6 >> 2 & 0x3F;
                        var num9 = (num6 << 4 | num7 >> 4) & 0x3F;
                        var num10 = num7 << 2 & 0x3F;

                        outStream.WriteByte(EncodingTable[num8]);
                        outStream.WriteByte(EncodingTable[num9]);
                        outStream.WriteByte(EncodingTable[num10]);
                        outStream.WriteByte(Padding);

                        break;
                    }
            }

            return num2 / 3 * 4 + ((num != 0) ? 4 : 0);
        }

        private static bool Ignore(char c)
        {
            if (c != '\n' && c != '\r' && c != '\t')
            {
                return c == ' ';
            }

            return true;
        }

        private static int NextI(IList<byte> data, int i, int finish)
        {
            while (i < finish && Ignore((char)data[i]))
            {
                i++;
            }

            return i;
        }

        public int DecodeString(string data, Stream outStream)
        {
            var num = 0;
            var num2 = data.Length;

            while (num2 > 0 && Ignore(data[num2 - 1]))
            {
                num2--;
            }

            var i = 0;
            var num3 = num2 - 4;

            for (i = NextI(data, i, num3); i < num3; i = NextI(data, i, num3))
            {
                var b = _decodingTable[data[i++]];

                i = NextI(data, i, num3);

                var b2 = _decodingTable[data[i++]];

                i = NextI(data, i, num3);

                var b3 = _decodingTable[data[i++]];

                i = NextI(data, i, num3);

                var b4 = _decodingTable[data[i++]];

                if ((b | b2 | b3 | b4) >= 128)
                {
                    throw new IOException("invalid characters encountered in base64 data");
                }

                outStream.WriteByte((byte)(b << 2 | b2 >> 4));
                outStream.WriteByte((byte)(b2 << 4 | b3 >> 2));
                outStream.WriteByte((byte)(b3 << 6 | b4));
                num += 3;
            }

            return num + DecodeLastBlock(outStream, data[num2 - 4], data[num2 - 3], data[num2 - 2], data[num2 - 1]);
        }

        private int DecodeLastBlock(Stream outStream, char c1, char c2, char c3, char c4)
        {
            if (c3 == Padding)
            {
                var b = _decodingTable[c1];
                var b2 = _decodingTable[c2];

                if ((b | b2) >= 128)
                {
                    throw new IOException("invalid characters encountered at end of base64 data");
                }

                outStream.WriteByte((byte)(b << 2 | b2 >> 4));

                return 1;
            }

            if (c4 == Padding)
            {
                var b3 = _decodingTable[c1];
                var b4 = _decodingTable[c2];
                var b5 = _decodingTable[c3];

                if ((b3 | b4 | b5) >= 128)
                {
                    throw new IOException("invalid characters encountered at end of base64 data");
                }

                outStream.WriteByte((byte)(b3 << 2 | b4 >> 4));
                outStream.WriteByte((byte)(b4 << 4 | b5 >> 2));

                return 2;
            }

            var b6 = _decodingTable[c1];
            var b7 = _decodingTable[c2];
            var b8 = _decodingTable[c3];
            var b9 = _decodingTable[c4];

            if ((b6 | b7 | b8 | b9) >= 128)
            {
                throw new IOException("invalid characters encountered at end of base64 data");
            }

            outStream.WriteByte((byte)(b6 << 2 | b7 >> 4));
            outStream.WriteByte((byte)(b7 << 4 | b8 >> 2));
            outStream.WriteByte((byte)(b8 << 6 | b9));

            return 3;
        }

        private static int NextI(string data, int i, int finish)
        {
            while (i < finish && Ignore(data[i]))
            {
                i++;
            }

            return i;
        }
    }

    public interface IEncoder
    {
        int DecodeString(string data, Stream outStream);

        int Encode(byte[] data, int off, int length, Stream outStream);
    }
}
