/***************************************************************************************************
* copyright : 芯迈微半导体（珠海）有限公司
* version   : 1.00
* file      : Sm4Helper.cs
* date      : 2023/03/16
* author    : haozhe.ni
* brief     : 
* section Modification History
* - 1.0 : Initial version - haozhe.ni
***************************************************************************************************/

using System;
using System.Collections.Generic;

namespace Common
{
    internal class Sm4
    {
        #region 变量定义
        public const int Sm4Encrypt = 1;
        public const int Sm4Decrypt = 0;
        private readonly uint[] _fk = { 0xa3b1bac6, 0x56aa3350, 0x677d9197, 0xb27022dc };

        private readonly byte[] _sboxTable =
        { 0xd6, 0x90, 0xe9, 0xfe, 0xcc, 0xe1, 0x3d, 0xb7,
            0x16, 0xb6, 0x14, 0xc2, 0x28, 0xfb, 0x2c, 0x05,
            0x2b, 0x67, 0x9a, 0x76, 0x2a, 0xbe, 0x04, 0xc3,
            0xaa, 0x44, 0x13, 0x26, 0x49, 0x86, 0x06, 0x99,
            0x9c, 0x42, 0x50, 0xf4, 0x91, 0xef, 0x98, 0x7a,
            0x33, 0x54, 0x0b, 0x43, 0xed, 0xcf, 0xac, 0x62,
            0xe4, 0xb3, 0x1c, 0xa9, 0xc9, 0x08, 0xe8, 0x95,
            0x80, 0xdf, 0x94, 0xfa, 0x75, 0x8f, 0x3f, 0xa6,
            0x47, 0x07, 0xa7, 0xfc, 0xf3, 0x73, 0x17, 0xba,
            0x83, 0x59, 0x3c, 0x19, 0xe6, 0x85, 0x4f, 0xa8,
            0x68, 0x6b, 0x81, 0xb2, 0x71, 0x64, 0xda, 0x8b,
            0xf8, 0xeb, 0x0f, 0x4b, 0x70, 0x56, 0x9d, 0x35,
            0x1e, 0x24, 0x0e, 0x5e, 0x63, 0x58, 0xd1, 0xa2,
            0x25, 0x22, 0x7c, 0x3b, 0x01, 0x21, 0x78, 0x87,
            0xd4, 0x00, 0x46, 0x57, 0x9f, 0xd3, 0x27, 0x52,
            0x4c, 0x36, 0x02, 0xe7, 0xa0, 0xc4, 0xc8, 0x9e,
            0xea, 0xbf, 0x8a, 0xd2, 0x40, 0xc7, 0x38, 0xb5,
            0xa3, 0xf7, 0xf2, 0xce, 0xf9, 0x61, 0x15, 0xa1,
            0xe0, 0xae, 0x5d, 0xa4, 0x9b, 0x34, 0x1a, 0x55,
            0xad, 0x93, 0x32, 0x30, 0xf5, 0x8c, 0xb1, 0xe3,
            0x1d, 0xf6, 0xe2, 0x2e, 0x82, 0x66, 0xca, 0x60,
            0xc0, 0x29, 0x23, 0xab, 0x0d, 0x53, 0x4e, 0x6f,
            0xd5, 0xdb, 0x37, 0x45, 0xde, 0xfd, 0x8e, 0x2f,
            0x03, 0xff, 0x6a, 0x72, 0x6d, 0x6c, 0x5b, 0x51,
            0x8d, 0x1b, 0xaf, 0x92, 0xbb, 0xdd, 0xbc, 0x7f,
            0x11, 0xd9, 0x5c, 0x41, 0x1f, 0x10, 0x5a, 0xd8,
            0x0a, 0xc1, 0x31, 0x88, 0xa5, 0xcd, 0x7b, 0xbd,
            0x2d, 0x74, 0xd0, 0x12, 0xb8, 0xe5, 0xb4, 0xb0,
            0x89, 0x69, 0x97, 0x4a, 0x0c, 0x96, 0x77, 0x7e,
            0x65, 0xb9, 0xf1, 0x09, 0xc5, 0x6e, 0xc6, 0x84,
            0x18, 0xf0, 0x7d, 0xec, 0x3a, 0xdc, 0x4d, 0x20,
            0x79, 0xee, 0x5f, 0x3e, 0xd7, 0xcb, 0x39, 0x48 };

        private readonly uint[] _ck = { 0x00070e15, 0x1c232a31, 0x383f464d, 0x545b6269,
                             0x70777e85, 0x8c939aa1, 0xa8afb6bd, 0xc4cbd2d9,
                             0xe0e7eef5, 0xfc030a11, 0x181f262d, 0x343b4249,
                             0x50575e65, 0x6c737a81, 0x888f969d, 0xa4abb2b9,
                             0xc0c7ced5, 0xdce3eaf1, 0xf8ff060d, 0x141b2229,
                             0x30373e45, 0x4c535a61, 0x686f767d, 0x848b9299,
                             0xa0a7aeb5, 0xbcc3cad1, 0xd8dfe6ed, 0xf4fb0209,
                             0x10171e25, 0x2c333a41, 0x484f565d, 0x646b7279 };
        #endregion

        #region 私有方法
        private byte Sm4Sbox(byte inch)
        {
            var i = inch & 0xFF;
            var retVal = _sboxTable[i];

            return retVal;
        }

        private static long GET_ULONG_BE(IList<byte> b, int i)
        {
            var n = (long)(b[i] & 0xFF) << 24 | (long)(b[i + 1] & 0xFF) << 16 | (long)(b[i + 2] & 0xFF) << 8 | (long)b[i + 3] & 0xFF & 0xFFFFFFFFL;

            return n;
        }

        private static void PUT_ULONG_BE(long n, IList<byte> b, int i)
        {
            b[i] = (byte)(int)(0xFF & n >> 24);
            b[i + 1] = (byte)(int)(0xFF & n >> 16);
            b[i + 2] = (byte)(int)(0xFF & n >> 8);
            b[i + 3] = (byte)(int)(0xFF & n);
        }

        private static long Shl(long x, int n)
        {
            return (x & 0xFFFFFFFF) << n;
        }

        private static long Rotl(long x, int n)
        {
            return Shl(x, n) | x >> (32 - n);
        }

        private static void Swap(IList<long> sk, int i)
        {
            var t = sk[i];

            sk[i] = sk[(31 - i)];
            sk[(31 - i)] = t;
        }

        private long Sm4Lt(long ka)
        {
            var a = new byte[4];
            var b = new byte[4];

            PUT_ULONG_BE(ka, a, 0);
            b[0] = Sm4Sbox(a[0]);
            b[1] = Sm4Sbox(a[1]);
            b[2] = Sm4Sbox(a[2]);
            b[3] = Sm4Sbox(a[3]);

            var bb = GET_ULONG_BE(b, 0);
            var c = bb ^ Rotl(bb, 2) ^ Rotl(bb, 10) ^ Rotl(bb, 18) ^ Rotl(bb, 24);

            return c;
        }

        private long Sm4F(long x0, long x1, long x2, long x3, long rk)
        {
            return x0 ^ Sm4Lt(x1 ^ x2 ^ x3 ^ rk);
        }

        private long Sm4CalciRk(long ka)
        {
            var a = new byte[4];
            var b = new byte[4];

            PUT_ULONG_BE(ka, a, 0);
            b[0] = Sm4Sbox(a[0]);
            b[1] = Sm4Sbox(a[1]);
            b[2] = Sm4Sbox(a[2]);
            b[3] = Sm4Sbox(a[3]);

            var bb = GET_ULONG_BE(b, 0);
            var rk = bb ^ Rotl(bb, 13) ^ Rotl(bb, 23);

            return rk;
        }

        private void sm4_setkey(IList<long> sk, IList<byte> key)
        {
            var i = 0;
            var mk = new long[4];
            var k = new long[36];

            mk[0] = GET_ULONG_BE(key, 0);
            mk[1] = GET_ULONG_BE(key, 4);
            mk[2] = GET_ULONG_BE(key, 8);
            mk[3] = GET_ULONG_BE(key, 12);
            k[0] = mk[0] ^ _fk[0];
            k[1] = mk[1] ^ _fk[1];
            k[2] = mk[2] ^ _fk[2];
            k[3] = mk[3] ^ _fk[3];

            for (; i < 32; i++)
            {
                k[(i + 4)] = (k[i] ^ Sm4CalciRk(k[(i + 1)] ^ k[(i + 2)] ^ k[(i + 3)] ^ _ck[i]));
                sk[i] = k[(i + 4)];
            }
        }

        private void sm4_one_round(IList<long> sk, IList<byte> input, IList<byte> output)
        {
            var i = 0;
            var ulbuf = new long[36];

            ulbuf[0] = GET_ULONG_BE(input, 0);
            ulbuf[1] = GET_ULONG_BE(input, 4);
            ulbuf[2] = GET_ULONG_BE(input, 8);
            ulbuf[3] = GET_ULONG_BE(input, 12);

            while (i < 32)
            {
                ulbuf[(i + 4)] = Sm4F(ulbuf[i], ulbuf[(i + 1)], ulbuf[(i + 2)], ulbuf[(i + 3)], sk[i]);
                i++;
            }

            PUT_ULONG_BE(ulbuf[35], output, 0);
            PUT_ULONG_BE(ulbuf[34], output, 4);
            PUT_ULONG_BE(ulbuf[33], output, 8);
            PUT_ULONG_BE(ulbuf[32], output, 12);
        }

        private static byte[] Padding(byte[] input, int mode)
        {
            if (input == null)
            {
                return null;
            }

            byte[] ret;

            if (mode == Sm4Encrypt)
            {
                var p = 16 - input.Length % 16;

                ret = new byte[input.Length + p];
                Array.Copy(input, 0, ret, 0, input.Length);

                for (var i = 0; i < p; i++)
                {
                    ret[input.Length + i] = (byte)p;
                }
            }
            else
            {
                int p = input[input.Length - 1];
                ret = new byte[input.Length - p];
                Array.Copy(input, 0, ret, 0, input.Length - p);
            }

            return ret;
        }
        #endregion

        #region 公共方法
        /// <summary>
        /// 国密SM4设置加密密钥
        /// </summary>
        /// <param name="ctx"></param>
        /// <param name="key"></param>
        public void sm4_setkey_enc(Sm4Context ctx, byte[] key)
        {
            ctx.Mode = Sm4Encrypt;
            sm4_setkey(ctx.Sk, key);
        }

        /// <summary>
        /// 国密SM4设置解密密钥
        /// </summary>
        /// <param name="ctx"></param>
        /// <param name="key"></param>
        public void sm4_setkey_dec(Sm4Context ctx, byte[] key)
        {
            int i;

            ctx.Mode = Sm4Decrypt;
            sm4_setkey(ctx.Sk, key);

            for (i = 0; i < 16; i++)
            {
                Swap(ctx.Sk, i);
            }
        }

        /// <summary>
        /// 国密SM4 ECB加密方式
        /// </summary>
        /// <param name="ctx">国密对象</param>
        /// <param name="input">需要加密的字节数组</param>
        /// <returns></returns>
        public byte[] sm4_crypt_ecb(Sm4Context ctx, byte[] input)
        {
            if ((ctx.IsPadding) && (ctx.Mode == Sm4Encrypt))
            {
                input = Padding(input, Sm4Encrypt);
            }

            var length = input.Length;
            var bins = new byte[length];

            Array.Copy(input, 0, bins, 0, length);

            var bous = new byte[length];

            for (var i = 0; length > 0; length -= 16, i++)
            {
                var inBytes = new byte[16];
                var outBytes = new byte[16];

                Array.Copy(bins, i * 16, inBytes, 0, length > 16 ? 16 : length);
                sm4_one_round(ctx.Sk, inBytes, outBytes);
                Array.Copy(outBytes, 0, bous, i * 16, length > 16 ? 16 : length);
            }

            if (ctx.IsPadding && ctx.Mode == Sm4Decrypt)
            {
                bous = Padding(bous, Sm4Decrypt);
            }

            return bous;
        }
        #endregion
    }
}
