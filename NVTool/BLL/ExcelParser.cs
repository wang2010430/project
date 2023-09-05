/***************************************************************************************************
* copyright : CMIND-SEMI
* version   : 1.00
* file      : ExcelParser.cs
* date      : 2023/9/5 17:19:08
* author    : jinlong.wang
* brief     : 
* section Modification History
* - 1.0 : Initial version - jinlong.wang
***************************************************************************************************/

using NVParam.DAL;
using Common;
using NVTool.Helper;
using System.Data;

namespace NVTool.BLL
{
    class ExcelParser
    {
        public BoolQResult ExcelToItemNode(string filePath, out ItemDataNode node)
        {
            node = new ItemDataNode();
            DataTable dataTable =  NVExcelHelper.InputFromExcel(filePath, "RW");
            return new BoolQResult(true, "Successful conversion");
        }
    }
}
