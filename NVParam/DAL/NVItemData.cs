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
    public class NVItemData
    {
        public int ID { get; set; }
        public int ParentID { get; set; }
        public string ItemName { get; set; }
        public string ItemValue { get; set; }
        public string ItemID { get; set; }
        public string ItemContent { get; set; }
        public string Property { get; set; }
        public EDataType DataType { get; set; }
    }
}
