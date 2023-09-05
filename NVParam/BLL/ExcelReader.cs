/***************************************************************************************************
* copyright : CMIND-SEMI
* version   : 1.00
* file      : ExcelReader.cs
* date      : 2023/7/31 16:25:00
* author    : jinlong.wang
* brief     : ExcelReader
* section Modification History
* - 1.0 : Initial version - jinlong.wang
***************************************************************************************************/

using OfficeOpenXml;
using System.Collections.Generic;
using System.IO;

namespace NVParam.BLL
{
    internal class ExcelReader
    {
        /// <summary>
        /// Read Data From Excel
        /// </summary>
        /// <param name="filePath"></param>
        /// <returns></returns>
        public List<string> ReadDataFromExcel(string filePath)
        {
            List<string> columnAData = new List<string>();
            List<string> columnCData = new List<string>();

            FileInfo fileInfo = new FileInfo(filePath);

            using (ExcelPackage package = new ExcelPackage(fileInfo))
            {
                ExcelWorksheets worksheets = package.Workbook.Worksheets; // 获取工作表集合
                if (worksheets.Count > 0)
                {
                    ExcelWorksheet worksheet = worksheets["Sheet1"]; // 获取第一个工作表

                    // 获取 A 列的数据
                    int rowCount = worksheet.Dimension.Rows;
                    for (int row = 1; row <= rowCount; row++)
                    {
                        string cellValueA = worksheet.Cells[row, 1].Text;
                        columnAData.Add(cellValueA);
                    }

                    // 获取 C 列的数据
                    for (int row = 1; row <= rowCount; row++)
                    {
                        string cellValueC = worksheet.Cells[row, 3].Text;
                        columnCData.Add(cellValueC);
                    }
                }
            }

            return columnCData;
        }
    }
}
