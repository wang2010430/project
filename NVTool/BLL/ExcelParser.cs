/***************************************************************************************************
* copyright : CMIND-SEMI
* version   : 1.00
* file      : ExcelParser.cs
* date      : 2023/9/5 17:19:08
* author    : jinlong.wang
* brief     : ExcelParser类用于将Excel文件解析为树形结构的ItemDataNode，提供了转换和处理Excel数据的功能。
* section Modification History
* - 1.0 : Initial version - jinlong.wang
***************************************************************************************************/

using NVParam.DAL;
using Common;
using System.Data;
using System.IO;
using ExcelDataReader;
using System;
using NVParam.BLL;
using System.Text.RegularExpressions;
using System.Collections.Generic;
using NVTool.Helper;
using NVTool.DAL;

namespace NVTool.BLL
{
    class ExcelParser
    {
        #region Normal Function

        /// <summary>
        /// 将DataTable转换为ItemDataNode树形结构。
        /// </summary>
        /// <param name="dataTable">要转换的DataTable。</param>
        /// <returns>根节点ItemDataNode。</returns>
        private ItemDataNode ConvertToItemNode(DataTable dataTable)
        {
            ItemDataNode rootNode = new ItemDataNode()
            {
                ItemName = "NV Project",
                DataType = EDataType.Class
            };

            ItemDataNode currentNode = null;
            int maxLevels = 6; // 最大层级数
            int oldLevel = 1;
            Dictionary<int, ItemDataNode> dictItemDataNode = new Dictionary<int, ItemDataNode>();
            foreach (DataRow row in dataTable.Rows)
            {
                // 寻找当前层次的节点
                ItemDataNode parent = currentNode;
                for (int level = 1; level <= maxLevels; level++)
                {
                    string itemName = row[ExcelClumnName.ItemName.ToString()+ "_" + level].ToString();

                    if (!string.IsNullOrEmpty(itemName))
                    {
                        List<int> arrayLengths;
                        string arrayName;
                        EDataType dataType = (EDataType)Enum.Parse(typeof(EDataType), row[ExcelClumnName.Type.ToString()].ToString());
                        if (IsArray(itemName, out arrayLengths, out arrayName) && dataType != EDataType.Class) //Class后面处理
                        {
                            currentNode = new ItemDataNode
                            {
                                ItemName = arrayName,
                                DataType = EDataType.Array,
                                Content = row[ExcelClumnName.Content.ToString()].ToString(),
                            };
                            GetItemID(currentNode, row[ExcelClumnName.ID.ToString()].ToString());
                            CreateItemArray(currentNode, arrayLengths, dataType, row[ExcelClumnName.ItemValue.ToString()].ToString());
                        }
                        else
                        {
                            currentNode = new ItemDataNode
                            {
                                ItemName = itemName,
                                ItemValue = (dataType == EDataType.Array || dataType == EDataType.Class) ? "" : "0",
                                DataType = dataType,
                                Content = row[ExcelClumnName.Content.ToString()].ToString()
                            };

                            GetItemID(currentNode, row[ExcelClumnName.ID.ToString()].ToString());
                            SetItemValue(currentNode, row[ExcelClumnName.ItemValue.ToString()].ToString());
                
                        }
                        if (level == 1)
                        {
                            oldLevel = 1;
                            rootNode.Children.Add(currentNode);
                            dictItemDataNode[level] = currentNode;
                        }
                        else
                        {
                            if (oldLevel < level)
                            {             
                                parent.Children.Add(currentNode);
                                dictItemDataNode[level] = currentNode;
                            }
                            else 
                            {
                                ItemDataNode node = dictItemDataNode[level - 1];
                                node.Children.Add(currentNode);
                            }
                            oldLevel = level;
                        }
                    }
                }
            }

            ProcessClassArraysRecursively(rootNode);

            ItemDataNodeHelper nodeHelper = new ItemDataNodeHelper();
            nodeHelper.AssignIDsAndParentIDs(rootNode);

            ConverterClassToNode.PrintNode(rootNode);

            return rootNode;
        }

        /// <summary>
        /// 为指定的ItemDataNode节点设置ItemID属性。
        /// </summary>
        /// <param name="node">要设置ItemID属性的ItemDataNode节点。</param>
        /// <param name="sItemID">要分配给ItemID属性的字符串表示形式的ItemID。</param>
        private void GetItemID(ItemDataNode node, string sItemID)
        {
            if (node == null)
            {
                return;
            }

            if (string.IsNullOrEmpty(sItemID))
            {
                return;
            }

            uint itemID = 0;
            // 尝试将字符串形式的ItemID转换为无符号整数。
            DataConvert.StringToUint(sItemID, out itemID);

            // 将ItemID属性设置为转换后的值的字符串表示形式。
            node.ItemID = itemID.ToString();
        }

        /// <summary>
        /// 为指定的 ItemDataNode 节点设置 ItemValue 属性。
        /// </summary>
        /// <param name="node">要设置 ItemValue 属性的 ItemDataNode 节点。</param>
        /// <param name="sItemValue">要分配给 ItemValue 属性的字符串表示形式的 ItemValue。</param>
        private void SetItemValue(ItemDataNode node, string sItemValue)
        {
            if (node == null)
            {
                return;
            }

            if (string.IsNullOrEmpty(sItemValue))
            {
                return;
            }

            uint itemValue = 0;
            // 尝试将字符串形式的ItemID转换为无符号整数。
            DataConvert.StringToUint(sItemValue, out itemValue);

            // 将ItemValue属性设置为转换后的值的字符串表示形式。
            node.ItemValue = itemValue.ToString();
        }


        /// <summary>
        /// 从Excel文件中读取数据并将其转换为DataTable。
        /// </summary>
        /// <param name="filePath">Excel文件的路径。</param>
        /// <param name="worksheetName">要读取的工作表名称。</param>
        /// <returns>包含Excel数据的DataTable。</returns>
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
                            throw new ArgumentException($"Worksheet '{worksheetName}' not found.");
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

        /// <summary>
        /// 创建数组类型的ItemDataNode子节点。
        /// </summary>
        /// <param name="parentNode">父节点。</param>
        /// <param name="arrayLengths">数组长度列表。</para
        private void CreateItemArray(ItemDataNode parentNode, List<int> arrayLengths, EDataType type, string sItemValue)
        {
            if (arrayLengths.Count == 1)
            {
                for (int i = 0; i < arrayLengths[0]; i++)
                {
                    ItemDataNode newNode = new ItemDataNode
                    {
                        ItemName = parentNode.ItemName + "[" + i + "]",
                        ItemValue = "0",
                        DataType = type,
                    };
                    SetItemValue(newNode, sItemValue);
                    parentNode.Children.Add(newNode);
                }
            }
            else if (arrayLengths.Count == 2)
            {
                for (int i = 0; i < arrayLengths[0]; i++)
                {
                    for (int count = 0; count < arrayLengths[1]; count++)
                    {
                        ItemDataNode newNode = new ItemDataNode
                        {
                            ParentID = parentNode.ID,
                            ItemName = parentNode.ItemName + "[" + i +","+ count +  "]",
                            ItemValue = "0",
                            DataType = type,
                        };
                        SetItemValue(newNode, sItemValue);
                        parentNode.Children.Add(newNode);
                    }
                }
            }
        }

        /// <summary>
        /// 检查字符串是否表示数组，并获取其维度和名称。
        /// </summary>
        /// <param name="itemName">要检查的字符串。</param>
        /// <param name="arrayLengths">如果是数组，返回维度长度列表。</param>
        /// <param name="arrayName">如果是数组，返回数组名称。</param>
        /// <returns>如果是数组则返回true，否则返回false。</returns>
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

        /// <summary>
        /// 从Excel文件中读取数据并将其转换为ItemDataNode树形结构。
        /// </summary>
        /// <param name="filePath">Excel文件的路径。</param>
        /// <param name="node">输出的ItemDataNode树根节点。</param>
        /// <returns>BoolQResult，表示操作是否成功。</returns>
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

        /// <summary>
        /// 递归查找并处理符合条件的 ItemDataNode。
        /// </summary>
        /// <param name="parentNode">要检查的父节点。</param>
        /// <param name="currentID">当前 ID 值。</param>
        public void ProcessClassArraysRecursively(ItemDataNode parentNode)
        {
            // 检查父节点的 DataType 是否为 Class
            if (parentNode.DataType == EDataType.Class)
            {
                List<int> arrayLengths;
                string arrayName;

                // 检查父节点的 ItemName 是否为数组
                if (IsArray(parentNode.ItemName, out arrayLengths, out arrayName))
                {
                    // 克隆当前父节点
                    ItemDataNode currentNode = ItemDataNode.CloneItemDataNode(parentNode);

                    // 设置父节点的 DataType 为 Array，ItemName 为数组名称，清空子节点
                    parentNode.DataType = EDataType.Array;
                    parentNode.ItemName = arrayName;
                    parentNode.Children.Clear();

                    if (arrayLengths.Count == 1)
                    {
                        // 根据数组长度创建子节点
                        for (int count = 0; count < arrayLengths[0]; count++)
                        {
                            ItemDataNode newNode = ItemDataNode.CloneItemDataNode(currentNode);
                            newNode.ItemName = arrayName + "[" + count + "]";
                            parentNode.Children.Add(newNode);
                        }
                    }
             
                }
            }

            // 递归处理子节点
            foreach (var childNode in parentNode.Children)
            {
                foreach(var ItemNode in childNode.Children)
                    ProcessClassArraysRecursively(ItemNode);
            }
        }
        #endregion
    }
}
