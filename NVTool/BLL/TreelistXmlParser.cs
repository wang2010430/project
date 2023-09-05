/***************************************************************************************************
* copyright : CMIND-SEMI
* version   : 1.00
* file      : TreelistXmlParser.cs
* date      : 2023/7/7 12:09:15
* author    : jinlong.wang
* brief     : 解析xml的Parser类
* section Modification History
* - 1.0 : Initial version (2023/7/7 12:09:15) - jinlong.wang
***************************************************************************************************/

using Common;
using DevExpress.XtraEditors;
using NVParam.DAL;
using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Windows.Forms;
using System.Xml;

namespace NVTool.BLL
{
    public class TreelistXmlParser
    {
        #region Define ID
        internal const string FieldID = "ID";
        internal const string FieldParentID = "ParentID";
        internal const string FieldItemName = "ItemName";
        internal const string FieldItemID = "ItemID";
        internal const string FieldContent = "Content";
        internal const string FieldDataType = "DataType";
        internal const string FieldItemValue = "ItemValue";
        #endregion
        DataTable sourceTable;

        public DataTable SourceTable { get => sourceTable; set => sourceTable = value; }

        List<NVItemData> nvTables;

        public List<NVItemData> NVTables { get => nvTables; set => nvTables = value; }

        public TreelistXmlParser()
        {
            sourceTable = CreateDataTable();
            nvTables = new List<NVItemData>();
        }

        /// <summary>
        /// Get the Source Table
        /// </summary>
        /// <param name="filePath"></param>
        /// <returns></returns>
        public BoolQResult ParseXml(string filePath)
        {
            // check the file whether existed
            if (!File.Exists(filePath))
            {
                return new BoolQResult(false, "File is not existed!");
            }

            nvTables = ReadItemParameters(filePath);
            PopulateDataTable(nvTables);
            XmlHelper.SaveXML(sourceTable, "myobject.xml");

            return new BoolQResult(true, string.Empty);
        }

        /// <summary>
        /// Create Data Table
        /// </summary>
        /// <returns></returns>
        public DataTable CreateDataTable()
        {
            DataTable dataTable = new DataTable();
            dataTable.Columns.Add(FieldID, typeof(int));
            dataTable.Columns.Add(FieldParentID, typeof(int));
            dataTable.Columns.Add(FieldItemName, typeof(string));
            dataTable.Columns.Add(FieldItemID, typeof(string));
            dataTable.Columns.Add(FieldContent, typeof(string));
            dataTable.Columns.Add(FieldDataType, typeof(string));
            dataTable.Columns.Add(FieldItemValue, typeof(string));
            return dataTable;
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="itemNodes"></param>
        private List<NVItemData> ReadItemParameters(string xmlFilePath)
        {
            try
            {
                XmlDocument xmlDoc = new XmlDocument();
                xmlDoc.Load(xmlFilePath);

                XmlNodeList itemNodes = xmlDoc.SelectNodes("/NVItem/Item");

                List<NVItemData> nvTables = new List<NVItemData>();

                foreach (XmlNode itemNode in itemNodes)
                {
                    NVItemData nvTable = new NVItemData();

                    nvTable.ID = Convert.ToInt32(itemNode.Attributes["id"].Value);
                    nvTable.ParentID = Convert.ToInt32(itemNode.Attributes["parentID"].Value);
                    nvTable.ItemName = itemNode.Attributes["name"].Value;
                    nvTable.ItemValue = itemNode.Attributes["value"].Value;
                    nvTable.ItemID = itemNode.Attributes["itemID"].Value;
                    nvTable.ItemContent = itemNode.Attributes["content"].Value;

                    string type = itemNode.Attributes["type"].Value;
                    nvTable.DataType = (EDataType)Enum.Parse(typeof(EDataType), type);

                    nvTables.Add(nvTable);
                }

                return nvTables;
            }
            catch (Exception ex)
            {
                XtraMessageBox.Show(ex.Message, "Waring", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return new List<NVItemData>();
            }
        }

        /// <summary>
        /// 转换
        /// </summary>
        /// <param name="nvTables"></param>
        /// <param name="dataTable"></param>
        private void PopulateDataTable(List<NVItemData> nvTables)
        {
            sourceTable.Clear();
            foreach (NVItemData nvTable in nvTables)
            {
                DataRow dataRow = sourceTable.NewRow();
                dataRow[FieldID] = nvTable.ID;
                dataRow[FieldParentID] = nvTable.ParentID;
                dataRow[FieldItemName] = nvTable.ItemName;
                dataRow[FieldItemValue] = nvTable.ItemValue;
                dataRow[FieldItemID] = nvTable.ItemID;
                dataRow[FieldContent] = nvTable.ItemContent;
                dataRow[FieldDataType] = nvTable.DataType;

                sourceTable.Rows.Add(dataRow);
            }
        }
    }
}
