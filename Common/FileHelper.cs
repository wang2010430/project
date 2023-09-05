/***************************************************************************************************
* copyright : 芯迈微半导体（珠海）有限公司
* version   : 1.00
* file      : FileHelper.cs
* date      : 2023/03/16
* author    : haozhe.ni
* brief     : FileHelper
* section Modification History
* - 1.0 : Initial version - haozhe.ni
***************************************************************************************************/

using System;

namespace Common
{
    public class FileHelper : IComparable
    {
        public FileHelper(string fullPath)
        {
            FullPath = fullPath;
        }

        /// <summary>
        /// 简单文件名称（不包含后缀）
        /// </summary>
        public string SimpleName
        {
            get
            {
                return Name.Remove(Name.LastIndexOf('.'));
            }
        }

        /// <summary>
        /// 文件后缀
        /// </summary>
        public string NameSuffix
        {
            get
            {
                if (Name.Contains("."))
                {
                    int ind = Name.LastIndexOf('.');
                    return Name.Substring(ind, Name.Length - ind);
                }
                else
                {
                    return "";
                }
            }
        }

        /// <summary>
        /// 文件名
        /// </summary>
        public string Name
        {
            get
            {
                int ind = FullPath.LastIndexOf("\\");

                return FullPath.Substring(ind + 1);
            }
        }

        /// <summary>
        /// 路径（不包含文件）
        /// </summary>
        public string Path
        {
            get
            {
                int ind = FullPath.LastIndexOf("\\");

                return FullPath.Substring(0, ind);
            }
        }

        /// <summary>
        /// 路径
        /// </summary>
        public string FullPath { get; set; }

        /// <summary>
        /// 创建时间
        /// </summary>
        public DateTime CreationTime;

        public string CreationTimeString
        {
            get
            {
                return CreationTime.ToString("yy/MM/dd HH:mm:ss.fff");
            }
        }

        /// <summary>
        /// 按时间排序
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        public int CompareTo(object obj)
        {
            FileHelper o = obj as FileHelper;

            int ret = DateTime.Compare(CreationTime, o.CreationTime);

            if (ret > 0)
            {
                return 1;
            }
            else if (ret == 0)
            {
                return 0;
            }
            else
            {
                return -1;
            }
        }


    }
}
