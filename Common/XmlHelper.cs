/***************************************************************************************************
* copyright : 芯迈微半导体（珠海）有限公司
* version   : 1.00
* file      : XmlHelper.cs
* date      : 2023/03/16
* author    : haozhe.ni
* brief     : XML帮助类
* section Modification History
* - 1.0 : Initial version - haozhe.ni
***************************************************************************************************/

using System;
using System.IO;
using System.Text;
using System.Xml;
using System.Xml.Serialization;

namespace Common
{
    public class XmlHelper
    {
        public static bool SaveXML<T>(T obj, string savePath)
        {
            try
            {
                using (FileStream fs = new FileStream(savePath, FileMode.Create))
                {
                    XmlSerializer xl = new XmlSerializer(typeof(T));
                    xl.Serialize(fs, obj);
                    fs.Close();
                }

                return true;
            }
            catch (Exception ex)
            {
                UtMessageBase.ShowOneMessage(string.Format("调用【SaveXML】时，发生异常：{0}", ex), PopupMessageType.Exception);
                return false;
            }
        }

        public static T OpenXML<T>(string openPath) where T : class
        {
            try
            {
                if (!File.Exists(openPath))
                {
                    return default;
                }

                using (FileStream fs = new FileStream(openPath, FileMode.Open))
                {
                    T obj;
                    XmlSerializer serializer = new XmlSerializer(typeof(T));
                    obj = (T)serializer.Deserialize(fs);
                    fs.Close();

                    return obj;
                }
            }
            catch (Exception ex)
            {
                UtMessageBase.ShowOneMessage(string.Format("调用【OpenXML】时，发生异常：{0}", ex), PopupMessageType.Exception);

                return default;
            }
        }

        public static string XmlToString<T>(T obj)
        {
            XmlSerializer xl = new XmlSerializer(typeof(T));

            using (MemoryStream ms = new MemoryStream())
            {
                xl.Serialize(ms, obj);
                return Encoding.UTF8.GetString(ms.ToArray());
            }
        }

        public static T StringToXml<T>(string xmlString)
        {
            XmlSerializer xl = new XmlSerializer(typeof(T));

            using (Stream stream = new MemoryStream(Encoding.GetEncoding("gbk").GetBytes(xmlString)))
            {
                return (T)xl.Deserialize(stream);
            }
        }

        public static XmlAttribute GetXmlNodeAttribute(XmlNode node, string attributeName)
        {
            foreach (XmlAttribute attri in node.Attributes)
            {
                if (attri.Name == attributeName)
                {
                    return attri;
                }
            }

            return null;
        }

        public static XmlNode FindNode(XmlDocument xmlElement, string nodeName)
        {
            if (xmlElement == null)
            {
                return null;
            }

            foreach (XmlNode childNode in xmlElement.ChildNodes)
            {
                XmlNode tmpNode = FindNode(childNode, nodeName);

                if (tmpNode != null)
                {
                    return tmpNode;
                }
            }

            return null;
        }

        public static XmlNode FindNode(XmlNode xmlNode, string nodeName)
        {
            if (xmlNode == null)
            {
                return null;
            }

            if (xmlNode.Name == nodeName)
            {
                return xmlNode;
            }

            foreach (XmlNode childNode in xmlNode.ChildNodes)
            {
                XmlNode tmpNode = FindNode(childNode, nodeName);

                if (tmpNode != null)
                {
                    return tmpNode;
                }
            }

            return null;
        }
    }
}
