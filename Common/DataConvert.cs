/***************************************************************************************************
* copyright : 芯迈微半导体（珠海）有限公司
* version   : 1.00
* file      : DataConvert.cs
* date      : 2023/06/02
* author    : haozhe.ni
* brief     : 数据转换类
* section Modification History
* - 1.0 : Initial version - haozhe.ni
***************************************************************************************************/

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Runtime.InteropServices;

namespace Common
{
    public enum Endian
    {
        Null,
        BigEndian,     // 高字节在前
        LittleEndian,  // 低字节在前
    }

    public static class DataConvert
    {
        static Endian _endian = Endian.Null;

        /// <summary>
        /// 字符串转Uint
        /// </summary>
        /// <param name="str">in:字符串</param>
        /// <param name="val">out:Uint值</param>
        /// <returns>转换结果</returns>
        public static bool StringToUint(string str, out uint val)
        {
            bool isHex = false;
            if (str.Contains("H") || str.Contains("0x") || str.Contains("0X"))
            {
                isHex = true;
                str = str.Replace("H", "").Replace("0x", "").Replace("0X", "");
            }

            if (isHex)
            {
                if (string.IsNullOrEmpty(str))
                {
                    val = 0;
                    return true;
                }
                return uint.TryParse(str, NumberStyles.HexNumber, null, out val);
            }
            else
            {
                return uint.TryParse(str, out val);
            }
        }

        public static bool StringToByte(string str, out byte val)
        {
            bool isHex = false;
            if (str.Contains("H") || str.Contains("0x") || str.Contains("0X"))
            {
                isHex = true;
                str = str.Replace("H", "").Replace("0x", "").Replace("0X", "");
            }

            if (isHex)
            {
                if (string.IsNullOrEmpty(str))
                {
                    val = 0;
                    return true;
                }
                return byte.TryParse(str, NumberStyles.HexNumber, null, out val);
            }
            else
            {
                return byte.TryParse(str, out val);
            }
        }

        public static bool StringToUshort(string str, out ushort val)
        {
            bool isHex = false;
            if (str.Contains("H") || str.Contains("0x") || str.Contains("0X"))
            {
                isHex = true;
                str = str.Replace("H", "").Replace("0x", "").Replace("0X", "");
            }

            if (isHex)
            {
                if (string.IsNullOrEmpty(str))
                {
                    val = 0;
                    return true;
                }
                return ushort.TryParse(str, NumberStyles.HexNumber, null, out val);
            }
            else
            {
                return ushort.TryParse(str, out val);
            }
        }

        public static Endian SysEndian
        {
            get
            {
                if (_endian != Endian.Null)
                {
                    return _endian;
                }

                byte[] buf = BitConverter.GetBytes(1);
                _endian = buf[0] == 1 ? Endian.LittleEndian : Endian.BigEndian;

                return _endian;
            }
        }

        public static ulong ByteToULong(byte[] data, Endian endian = Endian.LittleEndian)
        {
            if (data.Length < 8)
            {
                return 0;
            }

            byte[] temp = new byte[8];
            Array.Copy(data, temp, 8);

            if(endian != SysEndian)
            {
                Array.Reverse(temp);
            }

            return BitConverter.ToUInt64(temp,0);
        }

        public static byte[] ULongToByte(ulong data, Endian endian = Endian.LittleEndian)
        {
            byte[] temp = BitConverter.GetBytes(data);

            if (endian != SysEndian)
            {
                Array.Reverse(temp);
            }

            return temp;
        }

        public static ushort ByteToUInt16(byte[] data, int startIndex, Endian endian = Endian.LittleEndian)
        {
            if (data.Length < 2)
            {
                return 0;
            }

            byte[] temp = new byte[2];
            Array.Copy(data, startIndex, temp, 0, 2);

            if (endian != SysEndian)
            {
                Array.Reverse(temp);
            }

            return BitConverter.ToUInt16(temp, 0);
        }

        public static byte[] UInt16ToByte(ushort data, Endian endian = Endian.LittleEndian)
        {
            byte[] temp = BitConverter.GetBytes(data);

            if (endian != SysEndian)
            {
                Array.Reverse(temp);
            }

            return temp;
        }

        public static short ByteToInt16(byte[] data, int startIndex, Endian endian = Endian.LittleEndian)
        {
            if (data.Length < 2)
            {
                return 0;
            }

            byte[] temp = new byte[2];
            Array.Copy(data, startIndex, temp, 0, 2);

            if (endian != SysEndian)
            {
                Array.Reverse(temp);
            }

            return BitConverter.ToInt16(temp, 0);
        }

        public static byte[] Int16ToByte(short data, Endian endian = Endian.LittleEndian)
        {
            byte[] temp = BitConverter.GetBytes(data);

            if (endian != SysEndian)
            {
                Array.Reverse(temp);
            }

            return temp;
        }

        public static uint ByteToUInt(byte[] data, int startIndex, Endian endian = Endian.LittleEndian)
        {
            if (data.Length < 4)
            {
                return 0;
            }

            byte[] temp = new byte[4];
            Array.Copy(data, startIndex, temp, 0, 4);

             if (endian != SysEndian)
            {
                Array.Reverse(temp);
            }

            return BitConverter.ToUInt32(temp, 0);
        }

        public static byte[] UIntToByte(uint data, Endian endian = Endian.LittleEndian)
        {
            byte[] temp = BitConverter.GetBytes(data);

            if (endian != SysEndian)
            {
                Array.Reverse(temp);
            }

            return temp;
        }

        public static int ByteToInt(byte[] data, int startIndex, Endian endian = Endian.LittleEndian)
        {
            if (data.Length < 4)
            {
                return 0;
            }

            byte[] temp = new byte[4];
            Array.Copy(data, startIndex, temp, 0, 4);

            if (endian != SysEndian)
            {
                Array.Reverse(temp);
            }

            return BitConverter.ToInt32(temp, 0);
        }

        public static byte[] IntToByte(int data, Endian endian = Endian.LittleEndian)
        {
            byte[] temp = BitConverter.GetBytes(data);

            if (endian != SysEndian)
            {
                Array.Reverse(temp);
            }

            return temp;
        }

        static public byte[] CombineArrays(byte[] array1, byte[] array2)
        {
            byte[] combinedArray = new byte[array1.Length + array2.Length];
            Array.Copy(array1, combinedArray, array1.Length);
            Array.Copy(array2, 0, combinedArray, array1.Length, array2.Length);
            return combinedArray;
        }

        static public byte[] CombineArraysEndianAware(byte[] array1, byte[] array2, Endian endian = Endian.LittleEndian)
        {
            byte[] combinedArray = CombineArrays(array1, array2);

            if (endian != SysEndian)
            {
                Array.Reverse(combinedArray);
            }

            return combinedArray;
        }

        /// <summary>
        /// convert struct to bytes
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="obj"></param>
        /// <param name="endian"></param>
        /// <returns></returns>
        static public byte[] ConvertToByteArray<T>(T obj, Endian endian = Endian.LittleEndian) where T : struct
        {
            int size = Marshal.SizeOf(obj);
            byte[] byteArray = new byte[size];
            IntPtr ptr = Marshal.AllocHGlobal(size);

            try
            {
                Marshal.StructureToPtr(obj, ptr, true);
                Marshal.Copy(ptr, byteArray, 0, size);

                if (endian != SysEndian)
                    Array.Reverse(byteArray);
            }
            finally
            {
                Marshal.FreeHGlobal(ptr);
            }

            return byteArray;
        }

        // 将字节数组转换为结构体并处理小端模式
        static public T ByteArrayToStructure<T>(byte[] bytes) where T : struct
        {
            T result;
            int size = Marshal.SizeOf(typeof(T));

            if (size > bytes.Length)
            {
                throw new ArgumentException("Byte array is smaller than the size of the target structure.");
            }

            // 如果是大端模式，反转字节数组
            if (!BitConverter.IsLittleEndian)
            {
                Array.Reverse(bytes);
            }

            GCHandle handle = GCHandle.Alloc(bytes, GCHandleType.Pinned);
            try
            {
                result = (T)Marshal.PtrToStructure(handle.AddrOfPinnedObject(), typeof(T));
            }
            finally
            {
                handle.Free();
            }
            return result;
        }
    }
}
