/***************************************************************************************************
* copyright : CMIND-SEMI
* version   : 1.00
* file      : Class1.cs
* date      : 2023/9/7 13:59:29
* author    : jinlong.wang
* brief     : 
* section Modification History
* - 1.0 : Initial version - jinlong.wang
***************************************************************************************************/

using NVParam.DAL;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NVTool.Helper
{
    public class ItemDataNodeHelper
    {
        private int currentID = 1;

        public void AssignIDsAndParentIDs(ItemDataNode rootNode, ItemDataNode parent = null)
        {
            if (rootNode == null)
            {
                return;
            }

            // 设置当前节点的 ID
            rootNode.ID = currentID++;

            // 设置当前节点的 ParentID
            if (parent != null)
            {
                rootNode.ParentID = parent.ID;
            }
            else
            {
                rootNode.ParentID = 0; // 如果没有父节点，设置为 0
            }

            // 递归处理子节点
            foreach (var childNode in rootNode.Children)
            {
                AssignIDsAndParentIDs(childNode, rootNode);
            }
        }
    }

}
