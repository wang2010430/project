/***************************************************************************************************
* copyright : 芯微半导体（珠海）有限公司
* version   : 1.00
* file      : FormatterReaderBase.cs
* date      : 2023/03/16
* author    : haozhe.ni
* brief     : FormatterReaderBase
* section Modification History
* - 1.0 : Initial version - haozhe.ni
***************************************************************************************************/

using Common;
using System;
using System.Collections.Generic;
using System.Data;
using System.Security;
using System.Xml;

namespace Channel
{
    /// <summary>
    /// 参数格式化的基类
    /// </summary>
    public class XmlParamterFormatterBase
    {
        /// <summary>
        /// xml根元素名
        /// </summary>
        private string XmlTableName = "ProtocolParameters";

        protected XmlDocument xmlDoc;
        protected XmlElement docElement;

        public XmlParamterFormatterBase(string elementName)
        {
            XmlTableName = elementName;
        }

        protected void StartFormat()
        {
            // 创建XmlDocument及其根元素
            xmlDoc = new XmlDocument();
            docElement = xmlDoc.CreateElement(XmlTableName);
            xmlDoc.AppendChild(docElement);
        }

        private void EndFormat() { }

        public void FormatStringParameter(string paraName, string paraValue)
        {
            if (xmlDoc != null)
            {
                XmlElement sub1 = xmlDoc.CreateElement(paraName);
                sub1.InnerText = paraValue;
                docElement.AppendChild(sub1);
            }
        }

        protected virtual void FormatAllParameters()
        {

        }

        virtual public string FormatedParameters
        {
            get
            {
                StartFormat();
                FormatAllParameters();
                EndFormat();
                return xmlDoc.InnerXml;
            }
        }
    }

    /// <summary>
    /// 参数格式化的基类
    /// 每个协议重写一个类，通过重写函数FormatAllParameters()来完成参数格式化
    /// </summary>
    public class ProtocolXmlParamterFormatterBase : XmlParamterFormatterBase
    {
        public ProtocolXmlParamterFormatterBase()
            : base("ProtocolParameters")
        {

        }

        protected void FormatSubchannls(List<string> sbchls)
        {
            if (sbchls != null)
            {
                foreach (string s in sbchls)
                {
                    FormatStringParameter("Subchannel", s);
                }
            }
        }

    }

    sealed public class ProtocolXmlParamterReader
    {
        /// <summary>
        /// xml根元素名
        /// </summary>
        const string XmlTableName = "ProtocolParameters";

        DataRow parameterRow;
        DataSet paramDataSet = new DataSet();

        public ProtocolXmlParamterReader(string xmlParams)
        {
            try
            {
                if (!string.IsNullOrEmpty(xmlParams))
                {
                    // 将xml字符串转换为DataTable，并获取参数值
                    paramDataSet.ReadXml(new System.IO.StringReader(xmlParams));
                    int tableIdx = paramDataSet.Tables.IndexOf(XmlTableName);
                    DataTable dt = tableIdx >= 0 ? paramDataSet.Tables[tableIdx] : null;
                    if (dt != null && dt.Rows.Count > 0)
                    {
                        parameterRow = dt.Rows[0];
                        ////下面的receivingTableName不要改为属性，这样会有缺省值
                        //configedParam.receivingTableName = UTTableBasic.GetDataRowFieldValue(dt.Rows[0], tableReceiving, "");
                        //configedParam.sendingTableName = UTTableBasic.GetDataRowFieldValue(dt.Rows[0], tableSending, "");
                        //configedParam.DualPointStatusCfgString = UTTableBasic.GetDataRowFieldValue(dt.Rows[0], dualPointCfgName, "");
                        //return configedParam;
                    }
                }
            }
            catch (SecurityException se)
            {
                UtMessageBase.ShowOneMessage(se.ToString(), PopupMessageType.Info);
            }
            catch (Exception ex)
            {
                UtMessageBase.ShowOneMessage(ex.ToString(), PopupMessageType.Info);
            }

        }

        static private string DataRowFieldValue(System.Data.DataRow dataRow, string fieldName, string defaultValue)
        {
            if (dataRow == null)
            {
                return defaultValue;
            }
            if (dataRow.RowState == DataRowState.Deleted)// || dataRow.RowState == DataRowState.Detached)
            {
                return defaultValue;
            }
            lock (dataRow.Table)
            {

                if (dataRow.Table.Columns.Contains(fieldName))
                {
                    string v = dataRow[fieldName].ToString().Trim();
                    if (v.Length == 0)
                    {
                        return defaultValue;
                    }
                    return v;
                }
                else
                {
                    // ShowOneMessage("字段【" + fieldName + "】不存在", PopupMessageType.msgError);
                    return defaultValue;
                }
            }
        }

        /// <summary>
        /// 读取字符串参数
        /// </summary>
        /// <param name="paraName"></param>
        /// <param name="defaultName"></param>
        /// <returns></returns>
        public string ReadStringParameter(string paraName, string defaultName)
        {
            return DataRowFieldValue(parameterRow, paraName, defaultName);
        }

        public List<string> Subchannels
        {
            get
            {
                List<string> lst = new List<string>();
                if (paramDataSet != null)
                {
                    foreach (DataTable dt in paramDataSet.Tables)
                    {
                        if (dt != null && dt.Rows.Count > 0)
                        {
                            foreach (DataColumn column in dt.Columns)
                            {
                                if (column.Caption.Contains("Subchannel"))
                                {
                                    foreach (DataRow r in dt.Rows)
                                    {
                                        if (r[column] != DBNull.Value)
                                        {
                                            //Replace("﹤", "<").Replace("﹥", ">")是为了可以读取老的配置数据，新的已经不需要
                                            lst.Add(r[column].ToString().Replace("﹤", "<").Replace("﹥", ">"));
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
                return lst;
            }
        }
    }
}
