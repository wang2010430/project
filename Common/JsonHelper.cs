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
using Newtonsoft.Json;

namespace Common
{
    public class JsonHelper
    {
        public static bool SaveJson<T>(T obj, string savePath)
        {
            try
            {
                if (!File.Exists(savePath))
                {
                    using (FileStream fs = new FileStream(savePath, FileMode.Create, FileAccess.ReadWrite))
                    {
                        fs.Close();
                    }
                }

                var settings = new JsonSerializerSettings()
                {
                    TypeNameHandling = TypeNameHandling.All,
                };

                File.WriteAllText(savePath, JsonConvert.SerializeObject(obj, settings));

                return true;
            }
            catch (Exception ex)
            {
                UtMessageBase.ShowOneMessage(string.Format("调用【SaveJson】时，发生异常：{0}", ex), PopupMessageType.Exception);

                return false;
            }
        }

        public static T OpenJson<T>(string openPath) where T : class
        {
            try
            {
                if (!File.Exists(openPath))
                {
                    return default;
                }

                var settings = new JsonSerializerSettings()
                {
                    TypeNameHandling = TypeNameHandling.All,
                };

                return JsonConvert.DeserializeObject<T>(File.ReadAllText(openPath), settings);
            }
            catch (Exception ex)
            {
                UtMessageBase.ShowOneMessage(string.Format("调用【OpenJson】时，发生异常：{0}", ex), PopupMessageType.Exception);

                return default;
            }
        }
    }
}
