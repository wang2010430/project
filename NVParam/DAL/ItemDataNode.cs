/***************************************************************************************************
* copyright : 芯迈微半导体（珠海）有限公司
* version   : 1.00
* file      : ItemDataNode.cs
* date      : 2023/7/8
* author    : jinlong.wang
* brief     : NV Item Node  
* section Modification History
* - 1.0 : Initial version - jinlong.wang
***************************************************************************************************/

using Common;
using System;
using System.Collections.Generic;

namespace NVParam.DAL
{
    [Serializable]
    public class ItemDataNode
    {
        #region Attribute
        public int ID { get; set; }
        public int ParentID { get; set; }
        public string ItemName { get; set; }
        public string ItemValue { get; set; }
        public EDataType DataType { get; set; }
        public string ItemID { get; set; }
        public string Content { get; set; }
        public EItemState ItemState { get; set; } = EItemState.active;
        public List<ItemDataNode> Children { get; set; }
        #endregion

        #region Constructor
        /// <summary>
        /// init item data node
        /// </summary>
        public ItemDataNode()
        {
            Children = new List<ItemDataNode>();
        }

        /// <summary>
        /// init item data node
        /// </summary>
        /// <param name="name"></param>
        /// <param name="value"></param>
        public ItemDataNode(string name, string value)
        {
            ItemName = name;
            ItemValue = value;
            Children = new List<ItemDataNode>();
        }
        #endregion

        #region Function    
        // 递归删除子节点的方法
        public void RemoveChild(int idToRemove)
        {       
            Children.RemoveAll(child => child.ID == idToRemove);
            foreach (var child in Children)
            {
                child.RemoveChild(idToRemove);
            }
        }

        /// <summary>
        /// Add Child Node
        /// </summary>
        /// <param name="childNode"></param>
        public void AddChild(ItemDataNode childNode)
        {
            if (Children == null)
            {
                Children = new List<ItemDataNode>();
            }

            Children.Add(childNode);
        }

        
        public List<ItemDataNode> GetAllChildNodes(ItemDataNode parentNode)
        {
            List<ItemDataNode> allChildNodes = new List<ItemDataNode>();
            foreach (ItemDataNode childNode in parentNode.Children)
            {
                allChildNodes.Add(childNode);
                List<ItemDataNode> grandChildNodes = GetAllChildNodes(childNode);
                allChildNodes.AddRange(grandChildNodes);
            }

            return allChildNodes;
        }

        /// <summary>
        /// Get All nodes
        /// </summary>
        /// <returns></returns>
        public List<ItemDataNode> GetAllNodes()
        {
            List<ItemDataNode> allChildNodes = new List<ItemDataNode>();
            allChildNodes.Add(this);
            foreach (ItemDataNode childNode in this.Children)
            {
                allChildNodes.Add(childNode);
                List<ItemDataNode> grandChildNodes = GetAllChildNodes(childNode);
                allChildNodes.AddRange(grandChildNodes);
            }

            return allChildNodes;
        }

        /// <summary>
        /// Find All ItemID
        /// </summary>
        /// <param name="node"></param>
        /// <returns></returns>
        public List<ushort> FindAllItemIDs(ItemDataNode node)
        {
            List<ushort> itemIDs = new List<ushort>();

            ushort parsedID = 0;
            // 检查当前节点的 ItemID
            if (TryParseItemID(node.ItemID, out parsedID))
            {
                itemIDs.Add(parsedID);
            }

            // 递归遍历子节点
            foreach (var childNode in node.Children)
            {
                List<ushort> childItemIDs = FindAllItemIDs(childNode);
                itemIDs.AddRange(childItemIDs);
            }

            return itemIDs;
        }

        /// <summary>
        /// Parse ItemID
        /// </summary>
        /// <param name="itemID"></param>
        /// <param name="parsedID"></param>
        /// <returns></returns>
        public bool TryParseItemID(string itemID, out ushort parsedID)
        {
            bool isParsed = ushort.TryParse(itemID, out ushort result);

            if (isParsed && result > 0)
            {
                parsedID = result;
                return true;
            }

            parsedID = 0;
            return false;
        }

        /// <summary>
        /// 克隆一个 ItemDataNode 节点及其子节点。
        /// </summary>
        /// <param name="sourceNode">要克隆的源节点。</param>
        /// <returns>克隆后的新节点。</returns>
        public static ItemDataNode CloneItemDataNode(ItemDataNode sourceNode)
        {
            if (sourceNode == null)
            {
                return null;
            }

            ItemDataNode clonedNode = new ItemDataNode
            {
                ID = sourceNode.ID,
                ParentID = sourceNode.ParentID,
                ItemName = sourceNode.ItemName,
                ItemValue = sourceNode.ItemValue,
                DataType = sourceNode.DataType,
                ItemID = sourceNode.ItemID,
                Content = sourceNode.Content,
                ItemState = sourceNode.ItemState,
                Children = new List<ItemDataNode>()
            };

            // 递归复制子节点
            foreach (var childNode in sourceNode.Children)
            {
                ItemDataNode clonedChild = CloneItemDataNode(childNode);
                if (clonedChild != null)
                {
                    clonedNode.Children.Add(clonedChild);
                }
            }

            return clonedNode;
        }

        #endregion
    }
}