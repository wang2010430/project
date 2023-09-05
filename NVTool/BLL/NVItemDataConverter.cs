/***************************************************************************************************
* copyright : CMIND-SEMI
* version   : 1.00
* file      : NVItemDataConverter.cs
* date      : 2023/7/8 18:00:12
* author    : jinlong.wang
* brief     : 
* section Modification History
* - 1.0 : Initial version (2023/7/8 18:00:12) - jinlong.wang
***************************************************************************************************/

using NVParam.DAL;
using System;
using System.Collections.Generic;
using System.Reflection;

namespace NVTool.BLL
{
    class NVItemDataConverter
    {
        public static List<NVItemData> ConvertClassToNVItemDataList<T>(T obj, int parentID, int id)
        {
            List<NVItemData> nvItemDataList = new List<NVItemData>();

            Type objectType = obj.GetType();
            PropertyInfo[] properties = objectType.GetProperties(BindingFlags.Public | BindingFlags.Instance);

            // 创建类节点
            NVItemData classNode = new NVItemData
            {
                ParentID = parentID,
                ID = id++,
                ItemID = objectType.Name,
                ItemName = objectType.Name,
                ItemValue = string.Empty,
                ItemContent = string.Empty,
                DataType = EDataType.Class
            };
            nvItemDataList.Add(classNode);

            foreach (PropertyInfo property in properties)
            {
                object value = property.GetValue(obj);
                Type valueType = property.PropertyType;

                if (valueType.IsValueType || valueType == typeof(string))
                {
                    // 处理值类型或字符串属性
                    NVItemData nvItemData = new NVItemData();
                    nvItemData.ParentID = classNode.ID; // 使用类节点的ID作为父节点ID
                    nvItemData.ID = id++;
                    nvItemData.ItemID = property.Name;
                    nvItemData.ItemName = property.Name;
                    nvItemData.ItemValue = value?.ToString() ?? string.Empty;
                    nvItemData.ItemContent = string.Empty; // 根据属性值自定义内容
                    nvItemData.DataType = GetDataType(valueType);

                    nvItemDataList.Add(nvItemData);
                }
                else if (valueType.IsArray)
                {
                    // 处理数组类型
                    if (valueType.GetArrayRank() == 1)
                    {
                        // 处理一维数组
                        Array array = (Array)value;
                        Type elementType = valueType.GetElementType();

                        // 创建数组节点
                        NVItemData arrayNode = new NVItemData
                        {
                            ParentID = classNode.ID, // 使用类节点的ID作为父节点ID
                            ID = id++,
                            ItemID = property.Name,
                            ItemName = property.Name,
                            ItemValue = string.Empty,
                            ItemContent = string.Empty,
                            DataType = EDataType.Array
                        };
                        nvItemDataList.Add(arrayNode);

                        for (int i = 0; i < array.Length; i++)
                        {
                            object arrayElement = array.GetValue(i);

                            NVItemData nvItemData = new NVItemData();
                            nvItemData.ParentID = arrayNode.ID; // 使用数组节点的ID作为父节点ID
                            nvItemData.ID = id++;
                            nvItemData.ItemID = $"{property.Name}[{i}]";
                            nvItemData.ItemName = $"{property.Name}[{i}]";
                            nvItemData.ItemValue = arrayElement?.ToString() ?? string.Empty;
                            nvItemData.ItemContent = string.Empty; // 根据元素值自定义内容
                            nvItemData.DataType = GetDataType(elementType);

                            nvItemDataList.Add(nvItemData);
                        }
                    }
                    else if (valueType.GetArrayRank() == 2)
                    {
                        // 处理二维数组
                        Array array = (Array)value;
                        Type elementType = valueType.GetElementType().GetElementType();

                        // 创建数组节点
                        NVItemData arrayNode = new NVItemData
                        {
                            ParentID = classNode.ID, // 使用类节点的ID作为父节点ID
                            ID = id++,
                            ItemID = property.Name,
                            ItemName = property.Name,
                            ItemValue = string.Empty,
                            ItemContent = string.Empty,
                            DataType = EDataType.Array
                        };
                        nvItemDataList.Add(arrayNode);

                        int rowCount = array.GetLength(0);
                        int colCount = array.GetLength(1);

                        for (int i = 0; i < rowCount; i++)
                        {
                            for (int j = 0; j < colCount; j++)
                            {
                                object arrayElement = array.GetValue(i, j);

                                NVItemData nvItemData = new NVItemData();
                                nvItemData.ParentID = arrayNode.ID; // 使用数组节点的ID作为父节点ID
                                nvItemData.ID = id++;
                                nvItemData.ItemID = $"{property.Name}[{i}][{j}]";
                                nvItemData.ItemName = $"{property.Name}[{i}][{j}]";
                                nvItemData.ItemValue = arrayElement?.ToString() ?? string.Empty;
                                nvItemData.ItemContent = string.Empty; // 根据元素值自定义内容
                                nvItemData.DataType = GetDataType(elementType);

                                nvItemDataList.Add(nvItemData);
                            }
                        }
                    }
                }
                else if (valueType.IsClass)
                {
                    // 处理嵌套类
                    List<NVItemData> childNodes = ConvertClassToNVItemDataList(value, classNode.ID, id); // 使用类节点的ID作为父节点ID
                    nvItemDataList.AddRange(childNodes);
                }
            }

            return nvItemDataList;
        }

        private static EDataType GetDataType(Type valueType)
        {
            if (valueType == typeof(byte))
                return EDataType.BYTE;
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

        public static T ConvertNVItemDataListToClass<T>(List<NVItemData> nvItemDataList)
        {
            Type objectType = typeof(T);
            T obj = Activator.CreateInstance<T>();

            foreach (NVItemData nvItemData in nvItemDataList)
            {
                SetProperty(obj, objectType, nvItemData.ItemID, nvItemData.ItemValue);
            }

            return obj;
        }

        private static void SetProperty(object obj, Type objectType, string propertyName, string propertyValue)
        {
            PropertyInfo property = objectType.GetProperty(propertyName);

            if (property != null && property.CanWrite)
            {
                Type propertyType = property.PropertyType;

                if (propertyType.IsArray)
                {
                    Type elementType = propertyType.GetElementType();
                    Array array = CreateArrayFromString(propertyValue, elementType);
                    property.SetValue(obj, array);
                }
                else if (propertyType.IsClass)
                {
                    object nestedObj = Activator.CreateInstance(propertyType);
                    SetProperty(nestedObj, propertyType, propertyName, propertyValue);
                    property.SetValue(obj, nestedObj);
                }
                else
                {
                    object value = Convert.ChangeType(propertyValue, propertyType);
                    property.SetValue(obj, value);
                }
            }
        }

        private static Array CreateArrayFromString(string arrayString, Type elementType)
        {
            string[] elements = arrayString.Split(',');
            Array array = Array.CreateInstance(elementType, elements.Length);

            for (int i = 0; i < elements.Length; i++)
            {
                string elementString = elements[i].Trim();
                object elementValue = Convert.ChangeType(elementString, elementType);
                array.SetValue(elementValue, i);
            }

            return array;
        }

    }
}
