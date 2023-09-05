/***************************************************************************************************
* copyright : CMIND-SEMI
* version   : 1.00
* file      : NodeInfo.cs
* date      : 2023/8/22 10:50:41
* author    : jinlong.wang
* brief     : Item Node Infomation
* section Modification History
* - 1.0 : Initial version - jinlong.wang
***************************************************************************************************/

using NVParam.DAL;

namespace NVTool.DAL.Model
{
    public class ItemNodeInfo
    {
        public string Name { get; set; }
        public string ID { get; set; }
        public string Value { get; set; }
        public EDataType DataType { get; set; }
        public string Content { get; set; }
        public int Array1DLen { get; set; }
        public int Array2DLen { get; set; }
    }
}
