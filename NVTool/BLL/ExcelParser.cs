/***************************************************************************************************
* copyright : CMIND-SEMI
* version   : 1.00
* file      : ExcelParser.cs
* date      : 2023/9/5 17:19:08
* author    : jinlong.wang
* brief     : 
* section Modification History
* - 1.0 : Initial version - jinlong.wang
***************************************************************************************************/

using NVParam.DAL;
using Common;
using System.Data;
using System.IO;
using ExcelDataReader;
using System;
using log4net;
using NVParam.BLL;
using System.Text.RegularExpressions;
using System.Collections.Generic;

namespace NVTool.BLL
{
    class ExcelParser
    {
        #region Normal Function

        private DataTable ConvertExcelToDataTable(string filePath, string worksheetName)
        {
            DataTable dataTable = new DataTable();

            // 打开Excel文件并创建一个数据流
            using (var stream = File.Open(filePath, FileMode.Open, FileAccess.Read))
            {
                // 使用ExcelDataReader读取Excel文件
                using (var reader = ExcelReaderFactory.CreateReader(stream))
                {
                    // 遍历工作表，找到指定名称的工作表
                    while (reader.Name != worksheetName)
                    {
                        if (!reader.NextResult())
                        {
                            throw new ArgumentException($"工作表 '{worksheetName}' 未找到.");
                        }
                    }

                    // 读取表头（第一行）作为DataTable的列名
                    reader.Read();
                    for (int i = 0; i < reader.FieldCount; i++)
                    {
                        string columnName = reader.GetString(i);
                        dataTable.Columns.Add(columnName);
                    }

                    // 读取数据行并填充DataTable
                    while (reader.Read())
                    {
                        DataRow row = dataTable.NewRow();
                        for (int i = 0; i < reader.FieldCount; i++)
                        {
                            row[i] = reader.GetValue(i);
                        }
                        dataTable.Rows.Add(row);
                    }
                }
            }

            return dataTable;
        }

        private ItemDataNode ConvertToItemNode(DataTable dataTable)
        {
            int currentID = 1;
            ItemDataNode rootNode = new ItemDataNode()
            {
                ItemName = "NV Project",
                ID = currentID++,
                ParentID = 0,
                DataType = EDataType.Class
            };

            ItemDataNode currentNode = null;
            int maxLevels = 6; // 最大层级数

            foreach (DataRow row in dataTable.Rows)
            {
                // 寻找当前层次的节点
                ItemDataNode parent = currentNode;
                for (int level = 1; level <= maxLevels; level++)
                {
                    string itemName = row["ItemName_" + level].ToString();

                    if (!string.IsNullOrEmpty(itemName))
                    {
                        List<int> arrayLengths;
                        string arrayName;
                        if (IsArray(itemName, out arrayLengths, out arrayName))
                        {
                            EDataType type = (EDataType)Enum.Parse(typeof(EDataType), row["Type"].ToString());
                            currentNode = new ItemDataNode
                            {
                                ID = currentID++,
                                ParentID = (level > 1) ? currentNode.ParentID : 0,
                                ItemName = arrayName,
                                DataType = EDataType.Array,
                                Content = row["Content"].ToString()
                            };

                            currentID = CreateItemArray(currentNode, arrayLengths, type);

                            if (level == 1)
                            {
                                rootNode.Children.Add(currentNode);
                            }
                            else
                            {
                                parent.Children.Add(currentNode);
                            }
                        }
                        else
                        {
                            currentNode = new ItemDataNode
                            {
                                ID = currentID++,
                                ParentID = (level > 1) ? currentNode.ParentID : 0,
                                ItemName = itemName,
                                DataType = (EDataType)Enum.Parse(typeof(EDataType), row["Type"].ToString()),
                                Content = row["Content"].ToString()
                            };
                            if (level == 1)
                            {
                                rootNode.Children.Add(currentNode);
                            }
                            else
                            {
                                parent.Children.Add(currentNode);
                            }
                        }
                    
                    }
                }
            }

            ConverterClassToNode.PrintNode(rootNode);
            return rootNode;
        }

        private int CreateItemArray(ItemDataNode parentNode, List<int> arrayLengths, EDataType type)
        {
            int currentID = parentNode.ID;
            if (arrayLengths.Count == 1)
            {
                for (int i = 0; i < arrayLengths[0]; i++)
                {
                    currentID++;
                    ItemDataNode newNode = new ItemDataNode
                    {
                        ParentID = parentNode.ID,
                        ID = currentID,
                        ItemName = parentNode.ItemName + "[" + i + "]",
                        ItemValue = "0",
                        DataType = type,
                    };
                    parentNode.Children.Add(newNode);
                }
            }
            else if (arrayLengths.Count == 2)
            {
                for (int i = 0; i < arrayLengths[0]; i++)
                {
                    for (int count = 0; count < arrayLengths[1]; count++)
                    {
                        currentID++;
                        ItemDataNode newNode = new ItemDataNode
                        {
                            ParentID = parentNode.ID,
                            ID = currentID,
                            ItemName = parentNode.ItemName + "[" + i +","+ count +  "]",
                            ItemValue = "0",
                            DataType = type,
                        };
                        parentNode.Children.Add(newNode);
                    }
                 
                }
            }

            return currentID;
        }

        private bool IsArray(string itemName, out List<int> arrayLengths, out string arrayName)
        {
            // 使用正则表达式匹配数组的维度、长度和名称
            MatchCollection matches = Regex.Matches(itemName, @"(\[\d+\])");
            if (matches.Count > 0)
            {
                arrayLengths = new List<int>();

                // 从最外层到最内层逐个提取维度长度
                foreach (Match match in matches)
                {
                    int length;
                    if (int.TryParse(match.Groups[1].Value.Trim('[', ']'), out length))
                    {
                        arrayLengths.Add(length);
                    }
                    else
                    {
                        arrayLengths.Add(0); // 无法解析长度时设置为 0 或者其他默认值
                    }
                }

                // 通过移除维度部分来获取数组名称
                arrayName = Regex.Replace(itemName, @"\[\d+\]", string.Empty);

                return true;
            }
            else
            {
                arrayName = null;
                arrayLengths = null;
                return false;
            }
        }



        public BoolQResult ExcelToItemNode(string filePath, out ItemDataNode node)
        {
            try
            {
                DataTable dataTable = ConvertExcelToDataTable(filePath, "RW");
                node = ConvertToItemNode(dataTable);
                return new BoolQResult(true, "Successful conversion");
            }
            catch (Exception ex)
            {
                node = default;
                return new BoolQResult(false, ex.Message);
            }
        }
        #endregion
    }
}
