/***************************************************************************************************
* copyright : 芯迈微半导体（珠海）有限公司
* version   : 1.00
* file      : LocalCfgHelper.cs
* date      : 2023/03/16
* author    : haozhe.ni
* brief     : 本地配置帮忙类
* section Modification History
* - 1.0 : Initial version - haozhe.ni
***************************************************************************************************/

using System;
using System.IO;
using System.Text;

namespace Common
{
    public static class LocalCfgHelper
    {
        #region 备份参数
        public static string ModelType = "";
        #endregion

        public static string RFTesterFile
        {
            get
            {
                return Path.Combine(DefaultConfigPath, "RFTesterBackup.xml");
            }
        }

        public static string CycleTemperatureFile
        {
            get
            {
                return Path.Combine(DefaultConfigPath, "CycleTemperatureBackup.xml");
            }
        }

        public static string RFTesterNetFile 
        {
            get
            {
                return Path.Combine(DefaultConfigPath, "RFTesterNet.xml");
            }
        }

        private static string _systemConfigPath;

        public static string DefaultConfigPath
        {
            get
            {
                if (!string.IsNullOrEmpty(_systemConfigPath))
                {
                    return _systemConfigPath;
                }

                try
                {
                    var location = Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location);

                    if (string.IsNullOrEmpty(location))
                    {
                        location = Environment.CurrentDirectory;
                    }

                    _systemConfigPath = Path.Combine(location, "CMData");

                    if (!Directory.Exists(_systemConfigPath))
                    {
                        Directory.CreateDirectory(_systemConfigPath);
                    }

                    return _systemConfigPath;
                }
                catch (Exception)
                {
                    _systemConfigPath = Path.Combine(Environment.CurrentDirectory, "CMData");

                    return _systemConfigPath;
                }
            }
        }

        public static string CfgPath
        {
            get
            {
                string path = Path.Combine(DefaultConfigPath, "System");

                if (!Directory.Exists(path))
                {
                    Directory.CreateDirectory(path);
                }

                return Path.Combine(path, "Config");
            }
        }

        public static bool LocalCfgSave()
        {
            try
            {
                StringBuilder sb = new StringBuilder();

                sb.AppendLine(string.Format("{0},{1}", "ModelType", ModelType));

                using (StreamWriter writer = new StreamWriter(CfgPath))
                {
                    writer.Write(sb.ToString());
                    writer.Flush();
                    writer.Close();
                }

                return true;
            }
            catch (Exception ex)
            {
                UtMessageBase.ShowOneMessage(string.Format("调用【LocalCfgSave】方法时发生异常：{0}", ex.Message), PopupMessageType.Exception);

                return false;
            }
        }

        public static bool LocalCfgLoad()
        {
            try
            {
                if (!string.IsNullOrEmpty(CfgPath) && File.Exists(CfgPath))
                {
                    using (StreamReader reader = new StreamReader(CfgPath))
                    {
                        string content = reader.ReadToEnd();
                        reader.Close();

                        string[] lines = content.Split(new[] { "\r\n" }, StringSplitOptions.RemoveEmptyEntries);

                        foreach (string item in lines)
                        {
                            string[] strs = item.Split(new[] { "," }, StringSplitOptions.RemoveEmptyEntries);

                            if (strs.Length < 1)
                            {
                                continue;
                            }

                            if (strs[0] == "ModelType")
                            {
                                if (strs.Length >= 2)
                                {
                                    ModelType = strs[1];
                                    continue;
                                }
                            }
                        }
                    }
                }

                return true;
            }
            catch (Exception ex)
            {
                UtMessageBase.ShowOneMessage(string.Format("调用【LocalCfgLoad】方法时发生异常：{0}", ex.Message), PopupMessageType.Exception);

                return false;
            }
        }
    }
}
