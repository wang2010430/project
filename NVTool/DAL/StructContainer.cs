/***************************************************************************************************
* copyright : CMIND-SEMI
* version   : 1.00
* file      : StructContainer.cs
* date      : 2023/7/6 10:32:32
* author    : jinlong.wang
* brief     : 结构体容器，可根据ItemID查询，ItemID具有唯一性
* section Modification History
* - 1.0 : Initial version (2023/7/6 10:32:32) - jinlong.wang
***************************************************************************************************/

using System.Collections.Generic;

namespace NVTool.DAL
{
    class StructContainer
    {
        private Dictionary<int, object> data;

        /// <summary>
        /// 
        /// </summary>
        public StructContainer()
        {
            data = new Dictionary<int, object>();
        }

        /// <summary>
        /// 增加Struct
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="itemId"></param>
        /// <param name="item"></param>
        public void AddItem<T>(int itemId, T item) where T : struct
        {
            data[itemId] = item;
        }

        /// <summary>
        /// 获取符合ItemID的Struct
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="itemId"></param>
        /// <returns></returns>
        public T GetItem<T>(int itemId) where T : struct
        {
            if (data.ContainsKey(itemId))
            {
                object item = data[itemId];
                if (item is T typedItem)
                {
                    return typedItem;
                }
            }
            return default(T);
        }
    }
}
