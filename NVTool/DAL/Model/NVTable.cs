/***************************************************************************************************
* copyright : CMIND-SEMI
* version   : 1.00
* file      : NVParam.cs
* date      : 2023/7/3 17:09:45
* author    : jinlong.wang
* brief     : 
* section Modification History
* - 1.0 : Initial version (2023/7/3 17:09:45) - jinlong.wang
***************************************************************************************************/

using System;
using System.Linq;

namespace NVTool.DAL.Model
{
    public partial class NVTable
    {
        public int ID { get; set; }
        public int ParentID { get; set; }
        public string ItemName { get; set; }
        public string ItemValue { get; set; }
        public string ItemID { get; set; }
        public string ItemContent { get; set; }
        public EDataType DataType { get; set; }
    }

}
