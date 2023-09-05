/***************************************************************************************************
* copyright : 芯迈微半导体（珠海）有限公司
* version   : 1.00
* file      : CommonHelper.cs
* date      : 2023/03/16
* author    : haozhe.ni
* brief     : 公共帮助类
* section Modification History
* - 1.0 : Initial version - haozhe.ni
***************************************************************************************************/

using System;
using System.IO;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;

namespace Common
{
    public class CommonHelper
    {
        /// <summary>
        /// 调试标志
        /// </summary>
        public static bool IsDebug = false;

        #region 判断平台类型

        private static OsFamily _operationSystem = OsFamily.Unknown;

        /// <summary>
        /// 获取操作系统平台
        /// </summary>
        public static OsFamily OperationSystem
        {
            get
            {
                if (_operationSystem != OsFamily.Unknown)
                {
                    return _operationSystem;
                }

                _operationSystem = Environment.OSVersion.Platform.ToString().StartsWith("Win", StringComparison.InvariantCultureIgnoreCase) ? OsFamily.Windows : OsFamily.Uniux;

                return _operationSystem;
            }
        }

        /// <summary>
        /// 判断平台类型是否是 Windows
        /// </summary>
        public static bool IsWindows
        {
            get
            {
                return OperationSystem == OsFamily.Windows;
            }
        }
        #endregion

        public static T ObjDeepCopy<T>(T obj)
        {
            using (MemoryStream memoryStream = new MemoryStream())
            {
                BinaryFormatter formatter = new BinaryFormatter();
                formatter.Serialize(memoryStream, obj);
                memoryStream.Position = 0;
                T t = (T)formatter.Deserialize(memoryStream);

                return t;
            }
        }

        public static T ObjDeepClone<T>(T source)
        {
            if (!typeof(T).IsSerializable)
            {
                throw new ArgumentException("The type must be serializable.", "source");
            }

            if (ReferenceEquals(source, null))
            {
                return default;
            }

            using (Stream stream = new MemoryStream())
            {
                IFormatter formatter = new BinaryFormatter();

                formatter.Serialize(stream, source);
                stream.Seek(0, SeekOrigin.Begin);

                return (T)formatter.Deserialize(stream);
            }
        }

        public static string ByteToString(byte[] data)
        {
            if (data == null || data.Length == 0)
            {
                return "";
            }

            StringBuilder sb = new StringBuilder();
            for (int i = 0; i < data.Length; i++)
            {
                sb.AppendFormat("{0:x2}{1}", data[i], i == data.Length - 1 ? "" : " ");
            }

            return sb.ToString().ToUpper();
        }
    }
}
