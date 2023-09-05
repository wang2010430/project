/***************************************************************************************************
* copyright : CMIND-SEMI
* version   : 1.00
* file      : ConverterClassToNode.cs
* date      : 2023/7/10
* author    : jinlong.wang
* brief     : 转换类，实现类的转换
* section Modification History
* - 1.0 : Initial version (2023/7/8 12:14:57) - jinlong.wang
***************************************************************************************************/

using NVParam.DAL;
using System;
using System.Linq;

namespace NVParam.BLL
{
    public static class ConverterClassToNode
    {
        /// <summary>
        /// classt to node
        /// </summary>
        /// <param name="obj"></param>
        /// <param name="nodeName"></param>
        /// <param name="parentID"></param>
        /// <param name="id"></param>
        /// <returns></returns>
        public static ItemDataNode ConvertToNode(object obj, ref int id, string nodeName = "", int parentID = 0)
        {
            Type objectType = obj.GetType();
            if (nodeName == string.Empty)
                nodeName = objectType.Name;
            var node = new ItemDataNode(nodeName, string.Empty)
            {
                ParentID = parentID,
                ID = id++,
                DataType = GetDataType(objectType),
            };

            var properties = obj.GetType().GetProperties();

            foreach (var property in properties)
            {
                Type valueType = property.PropertyType;
                if (valueType.IsArray && valueType.GetElementType().IsClass && valueType.GetElementType() != typeof(string)) // class[]
                {
                    var propertyInfo = objectType.GetProperty(property.Name);//property is null
                    var arrayObject = propertyInfo.GetValue(obj);
                    if (arrayObject != null)
                    {
                        var arrayNode = new ItemDataNode(property.Name, string.Empty)
                        {
                            ParentID = node.ID,
                            ID = id++,
                            DataType = EDataType.Array
                        };

                        Array array = (Array)arrayObject;
                        for (int i = 0; i < array.Length; i++)
                        {
                            var objectValue = array.GetValue(i);
                            if (objectValue != null)
                            {
                                var arrayItemNode = ConvertToNode(array.GetValue(i), ref id, $"{property.Name}[{i}]", arrayNode.ID);
                                arrayNode.Children.Add(arrayItemNode);
                            }
                        }

                        node.Children.Add(arrayNode);
                    }
                }
                else if (property.PropertyType.IsArray) //Array
                {
                    var arrayNode = new ItemDataNode(property.Name, string.Empty)
                    {
                        ParentID = node.ID,
                        ID = id++,
                        DataType = EDataType.Array
                    };

                    var array = property.GetValue(obj) as Array;
                    if (array != null)
                    {
                        Type elementType = valueType.GetElementType();
                        int[] lengths = Enumerable.Range(0, array.Rank).Select(array.GetLength).ToArray();
                        if (array.Rank == 1) // 1D array
                        {
                            for (int i = 0; i < array.Length; i++)
                            {
                                if (elementType.IsClass)
                                {
                                    var arrayItemNode = ConvertToNode(array.GetValue(i), ref id, $"{property.Name}[{i}]", arrayNode.ID);
                                    arrayNode.Children.Add(arrayItemNode);
                                }
                                else
                                {
                                    var arrayItemNode = new ItemDataNode($"{property.Name}[{i}]", array.GetValue(i).ToString())
                                    {
                                        ParentID = arrayNode.ID,
                                        ID = id++,
                                        DataType = GetDataType(elementType),
                                    };
                                    arrayNode.Children.Add(arrayItemNode);
                                }
                            }
                        }
                        else if (array.Rank == 2) // 2D array
                        {
                            for (int i = 0; i < lengths[0]; i++)
                            {
                                for (int j = 0; j < lengths[1]; j++)
                                {
                                    var arrayItemNode = new ItemDataNode($"{property.Name}[{i},{j}]", array.GetValue(i, j).ToString())
                                    {
                                        ParentID = arrayNode.ID,
                                        ID = id++,
                                        DataType = GetDataType(elementType),
                                    };
                                    arrayNode.Children.Add(arrayItemNode);
                                }
                            }
                        }
                    }

                    node.Children.Add(arrayNode);
                }
                else
                {
                    var propertyValue = property.GetValue(obj);

                    if (propertyValue != null)
                    {
                        if (property.PropertyType.IsClass && property.PropertyType != typeof(string))
                        {
                            var childNode = ConvertToNode(propertyValue, ref id, property.Name, node.ID);
                            node.Children.Add(childNode);
                        }

                        else
                        {
                            var childNode = new ItemDataNode(property.Name, propertyValue.ToString())
                            {
                                ParentID = node.ID,
                                ID = id++,
                                DataType = GetDataType(valueType)
                            };
                            node.Children.Add(childNode);
                        }
                    }
                }
            }

            return node;
        }

        /// <summary>
        /// 打印节点名称和数据
        /// </summary>
        /// <param name="node"></param>
        /// <param name="depth"></param>
        public static void PrintNode(ItemDataNode node, int depth = 0)
        {
            Console.WriteLine(new string(' ', depth * 2) + node.ItemName + ": " + node.ItemValue);

            foreach (var childNode in node.Children)
            {
                PrintNode(childNode, depth + 1);
            }
        }

        /// <summary>
        /// 获取数据类型
        /// </summary>
        /// <param name="valueType"></param>
        /// <returns></returns>
        private static EDataType GetDataType(Type valueType)
        {
            if (valueType.IsClass)
                return EDataType.Class;
            if (valueType.IsArray)
                return EDataType.Array;
            if (valueType == typeof(byte))
                return EDataType.BYTE;
            if (valueType == typeof(sbyte))
                return EDataType.SBYTE;
            if (valueType == typeof(short))
                return EDataType.SHORT;
            if (valueType == typeof(ushort))
                return EDataType.USHORT;
            if (valueType == typeof(int))
                return EDataType.INT;
            if (valueType == typeof(uint))
                return EDataType.UINT;
            if (valueType == typeof(long))
                return EDataType.LONG;

            return EDataType.Unknown;
        }

    }
}