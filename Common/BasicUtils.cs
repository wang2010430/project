/***************************************************************************************************
* copyright : 芯迈微半导体（珠海）有限公司
* version   : 1.00
* file      : BasicUtils.cs
* date      : 2023/03/16
* author    : haozhe.ni
* brief     : 字符串、字节处理用静态函数
* section Modification History
* - 1.0 : Initial version - haozhe.ni
***************************************************************************************************/

using System;
using System.IO;
using System.Text;
using System.Diagnostics;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace Common
{
    /// <summary>
    /// 字符串、字节处理用静态函数
    /// </summary>
    public class StringUtils
    {
        #region 文本、字符串处理的函数

        static public char[] BlankChars = new char[] { ' ' };

        /// <summary>
        /// 获取字符串后面的数字
        /// </summary>
        /// <param name="str">字串</param>
        /// <param name="intValue">返回整形值</param>
        /// <returns>字串后面是否是数字</returns>
        static public bool GetTrailingInteger(string str, out int trailingValue)
        {
            return GetTrailingInteger(str, out trailingValue);
        }

        /// <summary>
        /// 获取字符串后面的数字
        /// </summary>
        /// <param name="str">字串</param>
        /// <param name="intValue">返回整形值</param>
        /// <returns>字串后面是否是数字</returns>
        static public bool GetTrailingInteger(string str, out string sufstr, out int trailingValue)
        {
            sufstr = str;
            trailingValue = -1;

            if (str == null || str.Length == 0)
            {
                return false;
            }

            int charIndex = str.Length - 1;

            while (charIndex >= 0 && ((str[charIndex] >= '0' && str[charIndex] <= '9') || str[charIndex] == '-'))
            {
                charIndex--;
            }

            charIndex++;

            if (charIndex == str.Length)
            {
                return false;
            }

            try
            {
                sufstr = str.Substring(0, charIndex);
                trailingValue = int.Parse(str.Substring(charIndex));
            }
            catch
            {
                // 可能会出现溢出的情况
                trailingValue = int.MaxValue;
            }

            return true;
        }

        /// <summary>
        /// 从字符串中返回被特殊字符括起来的内容
        /// </summary>
        /// <param name="strSource">源字符串</param>
        /// <param name="leftSeperator">左侧的特殊字符（串）</param>
        /// <param name="rightSeperator">右侧的特殊字符（串）</param>
        /// <param name="strResult">结果</param>
        /// <returns>成功返回真，否则表示无特殊字符（串）对</returns>
        static public bool GetQuotedValue(string strSource, string leftSeperator, string rightSeperator, ref string strResult, bool multiQuoted)
        {
            int indexFirst = strSource.IndexOf(leftSeperator);

            if (indexFirst < 0)
            {
                return false;
            }

            int indexLast;

            if (!multiQuoted || leftSeperator == rightSeperator)
            {
                indexLast = strSource.LastIndexOf(rightSeperator);
            }
            else
            {
                indexLast = strSource.IndexOf(rightSeperator);
            }

            if (indexLast <= indexFirst)
            {
                return false;
            }

            strResult = strSource.Substring(indexFirst + leftSeperator.Length, indexLast - indexFirst - leftSeperator.Length);

            return true;
        }

        /// <summary>
        /// 只去掉字符串前后的ascii空格，保留全角空格
        /// </summary>
        /// <param name="str"></param>
        /// <returns></returns>
        public static string TrimAscii(string str)
        {
            return str.Trim(BlankChars);
        }

        /// <summary>
        /// 据说是一个必正则表达式替换更快字符串替换函数
        /// </summary>
        /// <param name="original"></param>
        /// <param name="pattern"></param>
        /// <param name="replacement"></param>
        /// <param name="ignoreCase"></param>
        /// <returns></returns>
        public static string ReplaceEx(string original, string pattern, string replacement, bool ignoreCase)
        {
            if (string.IsNullOrEmpty(pattern))
            {
                return original;
            }

            int count = 0;
            int position1;
            int position0 = 0;
            int inc = (original.Length / pattern.Length) * (replacement.Length - pattern.Length);
            char[] chars = new char[original.Length + Math.Max(0, inc)];

            while ((position1 = original.IndexOf(pattern, position0, (ignoreCase) ? StringComparison.OrdinalIgnoreCase : StringComparison.Ordinal)) != -1)
            {
                for (int i = position0; i < position1; ++i)
                {
                    chars[count++] = original[i];
                }

                for (int i = 0; i < replacement.Length; ++i)
                {
                    chars[count++] = replacement[i];
                }

                position0 = position1 + pattern.Length;
            }

            if (position0 == 0)
            {
                return original;
            }

            for (int i = position0; i < original.Length; ++i)
            {
                chars[count++] = original[i];
            }

            return new string(chars, 0, count);
        }

        static public string JoinStringArray(List<string> list, string separator)
        {
            if (list == null || list.Count == 0)
            {
                return "";
            }

            StringBuilder resultStr = new StringBuilder();

            if (list != null)
            {
                for (int i = 0; i < list.Count; i++)
                {
                    if (i > 0)
                    {
                        resultStr.Append(separator);
                    }

                    resultStr.Append(list[i]);
                }
            }

            return resultStr.ToString();
        }

        public static string JoinStringCollection(List<string> list)
        {
            if (list == null || list.Count == 0)
            {
                return "";
            }

            StringBuilder sb = new StringBuilder();

            if (list != null)
            {
                for (int i = 0; i < list.Count; i++)
                {
                    if (i > 0)
                    {
                        sb.Append(",");
                    }

                    sb.Append("'");
                    sb.Append(list[i]);
                    sb.Append("'");
                }
            }

            return sb.ToString();
        }

        public static string JoinStringCollection(List<string> list, string separator)
        {
            if (list == null || list.Count == 0)
            {
                return "";
            }

            StringBuilder sb = new StringBuilder();

            if (list != null)
            {
                for (int i = 0; i < list.Count; i++)
                {
                    if (i > 0)
                    {
                        sb.Append(separator);
                    }

                    sb.Append(list[i]);
                }
            }

            return sb.ToString();
        }

        /// <summary>
        /// 判断一队列中字符串是否存在
        /// </summary>
        /// <param name="testList">队列</param>
        /// <param name="testValue">比较值</param>
        /// <param name="ignoreCase">是否区分大小写</param>
        /// <returns></returns>
        static public bool IsExistInCollection(Collection<string> testList, string testValue, bool ignoreCase)
        {
            return IsExistInCollection(testList, testValue, ignoreCase, false);
        }

        /// <summary>
        /// 判断一队列中字符串是否存在
        /// </summary>
        /// <param name="testList">队列</param>
        /// <param name="testValue">比较值</param>
        /// <param name="ignoreCase">是否区分大小写</param>
        /// <param name="remove">是否从队列中移除</param>
        /// <returns></returns>
        static public bool IsExistInCollection(Collection<string> testList, string testValue, bool ignoreCase, bool remove)
        {
            if (testList != null && testList.Count > 0)
            {
                for (int i = 0; i < testList.Count; i++)
                {
                    if (string.Compare(testList[i], testValue, ignoreCase) == 0)
                    {
                        if (remove)
                        {
                            testList.RemoveAt(i);
                        }

                        return true;
                    }
                }
            }

            return false;
        }

        static public bool IsExistInCollection(string[] testList, string testValue, bool ignoreCase)
        {
            if (testList != null && testList.Length > 0)
            {

                for (int i = 0; i < testList.Length; i++)
                {
                    if (string.Compare(testList[i], testValue, ignoreCase) == 0)
                    {
                        return true;
                    }
                }
            }

            return false;
        }

        /// <summary>
        /// 将数据转换成十六进制字符串
        /// </summary>
        /// <param name="dataBytes"></param>
        /// <returns></returns>
        static public string DumpBytes(byte[] dataBytes)
        {
            if (dataBytes == null)
            {
                return "";
            }

            return DumpBytes(dataBytes, 0, dataBytes.Length);
        }

        static public string DumpBytes(byte[] dataBytes, int resultType)
        {
            if (dataBytes == null)
            {
                return "";
            }

            return DumpBytes(dataBytes, resultType, dataBytes.Length);
        }

        /// <summary>
        /// 将十六进制字符串转换成数据，返回null表示是无效串
        /// </summary>
        /// <param name="dumpedText"></param>
        /// <returns></returns>
        static public Byte[] Undump(string dumpedText)
        {
            if (String.IsNullOrEmpty(dumpedText) || dumpedText.Length % 2 != 0)
            {
                return null;
            }

            byte[] undumpedByte = new byte[dumpedText.Length / 2];

            try
            {
                for (int i = 0; i < dumpedText.Length / 2; i++)
                {
                    string hexText = dumpedText.Substring(i * 2, 2);
                    undumpedByte[i] = byte.Parse(hexText, System.Globalization.NumberStyles.HexNumber);
                }
            }
            catch
            {
                return null;
            }

            return undumpedByte;
        }

        /// <summary>
        /// 返回数据的十六进制表示格式，以便调试使用
        /// 0 十六进制格式连续显示
        /// 1 十六进制格式，每八个之间用“-”做分割符，其余空格做分割符 
        /// 2 十六进制格式，空格做分割符
        /// 3 字节定义的格式
        /// </summary>
        /// <param name="dataBytes">要显示的数据</param>
        /// <param name="resultType">要显示的数据</param>
        /// <param name="dumpLength">转换数据长度</param>
        /// <returns></returns>
        static public string DumpBytes(byte[] dataBytes, int resultType, int dumpLength)
        {
            if (dataBytes == null || dataBytes.Length == 0)
            {
                return "";
            }

            if (dumpLength < 0 || dumpLength > dataBytes.Length)
            {
                dumpLength = dataBytes.Length;
            }

            StringBuilder resultStr = new StringBuilder();

            switch (resultType)
            {
                case 0:
                case 1:
                case 2:

                    for (int i = 0; i < dumpLength; i++)
                    {
                        resultStr.Append(dataBytes[i].ToString("X2"));

                        if (resultType == 1)
                        {
                            if ((i + 1) % 16 == 0)
                            {
                                resultStr.Append(System.Environment.NewLine);
                            }
                            else if ((i + 1) % 8 == 0)
                            {
                                resultStr.Append("-");
                            }
                            else
                            {
                                resultStr.Append(" ");
                            }
                        }
                        else if (resultType == 2)
                        {
                            resultStr.Append(" ");
                        }
                    }

                    break;

                case 3:

                    resultStr.Append("{");

                    for (int i = 0; i < dumpLength - 1; i++)
                    {
                        resultStr.AppendFormat("{0},", dataBytes[i]);

                        if ((i + 1) % 32 == 0)
                        {
                            resultStr.Append(System.Environment.NewLine);
                        }
                    }

                    resultStr.AppendFormat("{0}{1}", dataBytes[dumpLength - 1], "};");
                    break;

                case 4:

                    for (int i = 0; i < dumpLength; i++)
                    {
                        resultStr.AppendFormat("{0}={1}", i, dataBytes[i]);

                        if (i != dumpLength - 1)
                        {
                            resultStr.Append(",");

                            if ((i + 1) % 10 == 0)
                            {
                                resultStr.Append(Environment.NewLine);
                            }
                        }
                    }

                    break;
            }

            return resultStr.ToString();
        }

        /// <summary>
        /// 判断某字符串是否全部由数字组成
        /// </summary>
        /// <param name="str"></param>
        /// <returns></returns>
        public static bool IsNumberString(string str)
        {
            if (str == null || str.Length == 0)
            {
                return false;
            }

            int digitalCount = 0;

            foreach (Char c in str)
            {
                if (!Char.IsNumber(c))
                {
                    if (c == '-')
                    {
                        if (digitalCount > 0)
                        {
                            return false;
                        }
                    }
                    else
                    {
                        return false;
                    }
                }

                digitalCount++;
            }

            return true;
        }

        /// <summary>
        /// 判断是否是十六进制字符串
        /// </summary>
        /// <param name="str"></param>
        /// <returns></returns>
        public static bool IsHexString(string str)
        {
            if (str == null || str.Length == 0)
            {
                return false;
            }

            foreach (Char c in str)
            {
                Char cc = Char.ToUpper(c);

                if (!Char.IsNumber(c) && (cc < 'A' || cc > 'F'))
                {
                    return false;
                }
            }

            return true;
        }
        #endregion 文本、字符串处理的函数

        /// <summary>
        /// 用逗号分隔的字段值的过滤条件（适用于DataTable.Select命令和SQL SELECT 命令）
        /// </summary>
        /// <param name="fieldName"></param>
        /// <param name="fieldValue"></param>
        /// <returns></returns>
        static public string CommaSeperatedNameFilter(string fieldName, string fieldValue)
        {
            return string.Format("{0}='{1}' OR {0} LIKE '{1},%' OR {0} LIKE '%,{1}' OR {0} LIKE '%,{1},%'", fieldName, fieldValue);
        }

        /// <summary>
        /// 获取文件的全名
        /// </summary>
        /// <param name="filePath">文件路径</param>
        /// <param name="fileName">文件名</param>
        /// <returns></returns>
        static public string ConstructFullFilePath(string filePath, string fileName)
        {
            if (string.IsNullOrEmpty(filePath))
            {
                return fileName;
            }

            return Path.Combine(filePath, fileName);
        }

        /// <summary>
        /// 按指定的顺序增加到队列中
        /// </summary>
        /// <param name="l"></param>
        /// <param name="s"></param>
        /// <param name="ascending"></param>
        /// <param name="removeDuplicate">是否移除相同项</param>
        static public void AddToList(List<string> l, string s, bool ascending, bool removeDuplicate)
        {
            if (l != null && s != null)
            {
                if (l.Count == 0)
                {
                    l.Add(s);
                }
                else
                {
                    for (int i = 0; i < l.Count; i++)
                    {
                        int v = string.Compare(l[i], s, false);

                        if (removeDuplicate && v == 0)
                        {
                            return;
                        }

                        bool currentIsLarger = v > 0;

                        if (l[i].Length != s.Length)
                        {
                            currentIsLarger = l[i].Length > s.Length;
                        }

                        if ((ascending && currentIsLarger) || (!ascending && !currentIsLarger))
                        {
                            l.Insert(i, s);
                            return;
                        }
                    }

                    l.Add(s);

                    return;
                }
            }
        }

        /// <summary>
        /// 分析字串(GB MB KB)，返回以字节表示的大小
        /// </summary>
        /// <param name="s"></param>
        /// <returns></returns>
        static public ulong ParseSize(string s)
        {
            if (s == null)
            {
                return 0;
            }

            s = ReplaceEx(s, " ", "", true).Trim();

            if (string.IsNullOrEmpty(s))
            {
                return 0;
            }

            uint multiplier = 1;

            if (s.EndsWith("MB", StringComparison.InvariantCultureIgnoreCase))
            {
                multiplier = 1024 * 1024;
                s = s.Substring(0, s.Length - 2);
            }

            if (s.EndsWith("KB", StringComparison.InvariantCultureIgnoreCase))
            {
                multiplier = 1024;
                s = s.Substring(0, s.Length - 2);
            }

            if (s.EndsWith("GB", StringComparison.InvariantCultureIgnoreCase))
            {
                multiplier = 1024 * 1024 * 1024;
                s = s.Substring(0, s.Length - 2);
            }

            if (s.EndsWith("M", StringComparison.InvariantCultureIgnoreCase))
            {
                multiplier = 1024 * 1024;
                s = s.Substring(0, s.Length - 1);
            }

            if (s.EndsWith("B", StringComparison.InvariantCultureIgnoreCase))
            {
                s = s.Substring(0, s.Length - 1);
            }

            try
            {
                return (ulong)double.Parse(s.Trim()) * multiplier;
            }
            catch (Exception e)
            {
                UtMessageBase.ShowOneMessage("分析数据库表大小时异常", e.ToString(), PopupMessageType.Info, 0);
                UtMessageBase.ShowOneMessage("错误串:" + s, PopupMessageType.Info);
            }

            return 0;
        }

        /// <summary>
        /// 将文件大小以GB或MB或KB的方式表达
        /// </summary>
        /// <param name="v"></param>
        /// <returns></returns>
        static public string FormatSize(ulong v)
        {
            double d = v;

            if (v > 1024 * 1024 * 1024)
            {
                return string.Format("{0:0.##} GB", d / (1024 * 1024 * 1024));
            }

            if (v > 1024 * 1024)
            {
                return string.Format("{0:0.##} MB", d / (1024 * 1024));
            }

            if (v > 1024)
            {
                return string.Format("{0:0.##} KB", d / 1024);
            }

            return v.ToString();
        }

        /// <summary>
        /// 转换字符串为整数
        /// </summary>
        /// <param name="numStr"></param>
        /// <param name="defaultValue"></param>
        /// <returns></returns>
        public static int ParseInt(string numStr, int defaultValue)
        {
            int num;

            if (!int.TryParse(numStr, out num))
            {
                num = defaultValue;
            }

            return num;
        }

        /// <summary>
        /// 排除字符串中间的数字数字及空格
        /// </summary>
        /// <param name="s"></param>
        /// <returns></returns>
        static public string StripSpaceAndInteger(string s)
        {
            StringBuilder sb = new StringBuilder();

            for (int i = 0; i < s.Length; i++)
            {
                if ((i >= 0 && i < s.Length && !(s[i] >= '0' && s[i] <= '9') && s[i] != ' '))
                {
                    sb.Append(s[i]);
                }
            }

            return sb.ToString();
        }

        static public string ResetTagName(string tagName, bool firstIn)
        {
            if (string.IsNullOrEmpty(tagName))
            {
                tagName = "MODBUS";
            }

            string fileExt = Path.GetExtension(tagName);
            string filePath = Path.GetDirectoryName(tagName);
            string fileName = Path.GetFileNameWithoutExtension(tagName);
            fileName = StripSpaceAndInteger(fileName);
            fileName = ReplaceEx(fileName, "Basic", "", true);
            int idx = fileName.IndexOf("-");

            if (idx < 0)
            {
                idx = fileName.IndexOf("_");
            }

            if (idx >= 0)
            {
                fileName = fileName.Substring(0, idx);
            }

            if (fileName.Length == 0 ||
                fileName.Equals("DB", StringComparison.InvariantCultureIgnoreCase) ||
                fileName.Equals("MODBUS", StringComparison.InvariantCultureIgnoreCase))
            {
                fileName = "MODBUS";
            }

            if (firstIn)
            {
                fileName = string.Format("{0}{1}", fileName, DateTime.Now.ToString("yyyyMMdd"));
            }
            else
            {
                fileName = string.Format("{0}{1}", fileName, DateTime.Now.ToString("yyyyMMddHmmss"));
            }

            if (fileExt.Length > 0)
            {
                fileName = Path.ChangeExtension(fileName, fileExt);
            }

            return Path.Combine(filePath, fileName);
        }

        /// <summary>
        /// 在文件名的后面增加一个标记
        /// </summary>
        /// <param name="fileName">原文件名</param>
        /// <param name="fileTag">标记</param>
        /// <returns>更改后的文件名</returns>
        static public string AttachTagToFileName(string fileName, string fileTag)
        {
            if (fileTag == null || fileTag.Length == 0)
            {
                return fileName;
            }

            string fileExt = Path.GetExtension(fileName);
            string filePath = Path.GetDirectoryName(fileName);

            fileName = Path.GetFileNameWithoutExtension(fileName) + fileTag;

            if (fileExt.Length > 0)
            {
                fileName = Path.ChangeExtension(fileName, fileExt);
            }

            return ConstructFullFilePath(filePath, fileName);
        }

        /// <summary>
        /// 从字符串中返回被特殊字符括起来的内容
        /// </summary>
        /// <param name="strSource">源字符串</param>
        /// <param name="leftSeperator">左侧的特殊字符（串）</param>
        /// <param name="rightSeperator">右侧的特殊字符（串）</param>
        /// <param name="strResult">结果</param>
        /// <returns>成功返回真，否则表示无特殊字符（串）对</returns>
        static public bool GetQuotedValue(string strSource, string leftSeperator, string rightSeperator, ref string strResult)
        {
            return GetQuotedValue(strSource, leftSeperator, rightSeperator, ref strResult, false);
        }

        /// <summary>
        /// 将特殊字符及其特殊字符包括起来的字符串用新的内容替换
        /// </summary>
        /// <param name="strSource">源字符串</param>
        /// <param name="leftSeperator">左侧的特殊字符（串）</param>
        /// <param name="rightSeperator">右侧的特殊字符（串）</param>
        /// <param name="newValue">新内容</param>
        /// <returns>返回替换后的结果</returns>
        static public string ReplaceQuotedValue(string strSource, string leftSeperator, string rightSeperator, string newValue)
        {
            return ReplaceQuotedValue(strSource, leftSeperator, rightSeperator, newValue, false);
        }

        /// <summary>
        /// 将特殊字符及其特殊字符包括起来的字符串用新的内容替换
        /// </summary>
        /// <param name="strSource">源字符串</param>
        /// <param name="leftSeperator">左侧的特殊字符（串）</param>
        /// <param name="rightSeperator">右侧的特殊字符（串）</param>
        /// <param name="newValue">新内容</param>
        /// <param name="multiQuoted">是否是多个嵌套</param>
        /// <returns>返回替换后的结果</returns>
        static public string ReplaceQuotedValue(string strSource, string leftSeperator, string rightSeperator, string newValue, bool multiQuoted)
        {
            int indexLast;
            int indexFirst = strSource.IndexOf(leftSeperator);

            if (indexFirst < 0)
            {
                return strSource;
            }

            if (!multiQuoted || leftSeperator == rightSeperator)
            {
                indexLast = strSource.LastIndexOf(rightSeperator);
            }
            else
            {
                indexLast = strSource.IndexOf(rightSeperator);
            }

            if (indexLast <= indexFirst)
            {
                return strSource;
            }

            string oldValue = strSource.Substring(indexFirst, indexLast - indexFirst + rightSeperator.Length);

            return strSource.Replace(oldValue, newValue);
        }
    }

    sealed public class MiscellaneousUtils
    {
        static public string BuildCallingStackInfo()
        {
            return BuildCallingStackInfo(1);
        }

        static public string BuildCallingStackInfo(int idx)
        {
            if (idx < 0)
            {
                idx = 0;
            }

            StackTrace tr = new StackTrace();
            StringBuilder sb = new StringBuilder();

            try
            {
                for (int i = idx; i < tr.FrameCount; i++)
                {
                    sb.AppendFormat("{0}<-", tr.GetFrame(i).GetMethod().Name);
                }
            }
            catch
            {

            }

            return sb.ToString();
        }

        /// <summary>
        /// 取代DateTime的ToString()函数，以排除在Parse时间串时，时间格式设置的影响
        /// 注意该函数不返回毫秒值
        /// </summary>
        /// <param name="dt"></param>
        /// <returns></returns>
        static public string DateTimeStringStandard(DateTime dt)
        {
            return dt.ToString("yyyy/MM/dd HH:mm:ss");
        }
    }

    public class DataProcessor
    {
        /// <summary>
        /// 查找以Separator开头和结束的有效数组
        /// </summary>
        /// <param name="data"></param>
        /// <param name="sequenceData"></param>
        /// <returns></returns>
        public static List<byte[]> FindValidDataWithSeparator(byte[] data, byte[] separator)
        {
            if (data == null || separator == null || separator.Length == 0)
            {
                return null;
            }

            List<byte[]> subArrays = new List<byte[]>();
            int startIndex = 0;

            while (startIndex < data.Length)
            {
                int startIndexOfSubArray = FindIndexOfSubArray(data, separator, startIndex);

                if (startIndexOfSubArray == -1)
                {
                    break;
                }

                int endIndexOfSubArray = FindIndexOfSubArray(data, separator, startIndexOfSubArray + separator.Length);

                if (endIndexOfSubArray == -1)
                {
                    break;
                }

                byte[] subArray = new byte[endIndexOfSubArray - (startIndexOfSubArray + separator.Length)];
                Array.Copy(data, startIndexOfSubArray + separator.Length, subArray, 0, subArray.Length);
                subArrays.Add(subArray);

                startIndex = endIndexOfSubArray + separator.Length;
            }

            return subArrays;
        }

        public static int FindIndexOfSubArray(byte[] source, byte[] subArray, int startIndex)
        {
            for (int i = startIndex; i <= source.Length - subArray.Length; i++)
            {
                bool found = true;

                for (int j = 0; j < subArray.Length; j++)
                {
                    if (source[i + j] != subArray[j])
                    {
                        found = false;
                        break;
                    }
                }

                if (found)
                {
                    return i;
                }
            }

            return -1;
        }

    }
}
