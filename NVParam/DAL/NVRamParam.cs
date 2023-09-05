/***************************************************************************************************
* copyright : 芯迈微半导体（珠海）有限公司
* version   : 1.00
* file      : NVItemData.cs
* date      : 2023/7/8 8:29:32
* author    : jinlong.wang
* brief     : NV Item Data Class 
* section Modification History
* - 1.0 : Initial version - jinlong.wang
***************************************************************************************************/

namespace NVParam.DAL
{
    public class NVRamParam
    {
        private ItemDataNode nvItem = new ItemDataNode();
        public ItemDataNode Item { get => nvItem; set => nvItem = value; }
        public string Version { get; set; }
        public string Project { get; set; }
    }
}
