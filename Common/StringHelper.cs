/***************************************************************************************************
* copyright : 芯迈微半导体（珠海）有限公司
* version   : 1.00
* file      : StringHelper.cs
* date      : 2023/03/16
* author    : haozhe.ni
* brief     : 字符串处理
* section Modification History
* - 1.0 : Initial version - haozhe.ni
***************************************************************************************************/

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.Text;
using System.Text.RegularExpressions;

namespace Common
{
    public class StringHelper
    {
        public static bool StringToBytes(string str, out byte[] bytes)
        {
            bytes = null;

            str = new Regex("[\\s,，()（）*\\-\\[\\]]+").Replace(str, "");

            if (str.Length == 0 || str.Length % 2 != 0)
            {
                return false;
            }

            bytes = new byte[str.Length / 2];

            for (int i = 0; i < str.Length / 2; i++)
            {
                if (!byte.TryParse(str.Substring(2 * i, 2), NumberStyles.HexNumber, null, out bytes[i]))
                {
                    bytes = null;

                    return false;
                }
            }

            return true;
        }

        public static string BytesToString(byte[] bytes, bool isCapital = true, bool hasBlank = true)
        {
            if (bytes == null || bytes.Length == 0)
            {
                return "";
            }

            StringBuilder sb = new StringBuilder();

            for (int i = 0; i < bytes.Length; i++)
            {
                sb.Append(bytes[i].ToString(isCapital ? "X2" : "x2"));
                if(hasBlank)
                {
                    sb.Append(i == sb.Length - 1 ? "" : " ");
                }
            }

            return sb.ToString();
        }

        public static bool StringToInts(string str, out List<int> ints)
        {
            ints = new List<int>();

            str = new Regex("[\\s,，]+").Replace(str, ",");
            string[] segments = str.Split(',');

            foreach (var segment in segments)
            {
                if (segment.Contains("-"))
                {
                    string[] values = segment.Split('-');

                    if (values.Length != 2)
                    {
                        return false;
                    }

                    int minVal = 0, maxVal = 0;

                    if (!int.TryParse(values[0], out minVal) || !int.TryParse(values[1], out maxVal))
                    {
                        return false;
                    }

                    if (minVal > maxVal)
                    {
                        return false;
                    }

                    for (int i = minVal; i <= maxVal; i++)
                    {
                        ints.Add(i);
                    }
                }
                else
                {
                    int val;

                    if (!int.TryParse(segment, out val))
                    {
                        return false;
                    }

                    ints.Add(val);
                }
            }
            ints.Sort();

            return true;
        }

        /// <summary>
        /// 获取枚举类型的描述
        /// </summary>
        /// <param name="Value">枚举类型</param>
        /// <returns></returns>
        public static string GetEnumDes(Enum Value)
        {
            try
            {
                string value = Value.ToString();
                System.Reflection.FieldInfo field = Value.GetType().GetField(value);
                object[] objs = field.GetCustomAttributes(typeof(DescriptionAttribute), false);

                if (objs.Length == 0)
                {
                    return value;
                }

                DescriptionAttribute descriptionattribute = (DescriptionAttribute)objs[0];

                return descriptionattribute.Description;
            }
            catch
            {
                return "";
            }
        }
    }
}
