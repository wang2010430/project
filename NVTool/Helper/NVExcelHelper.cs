using ExcelDataReader;
using System.Data;
using System.Diagnostics;
using System.IO;

public class ExcelHelper
{
    public static DataSet ExcelToDataSet(string relativeFilePath, bool useHeaderRow = true)
    {
        var filePath = Path.GetFullPath(relativeFilePath);
        if (!File.Exists(filePath))
        {
            throw new FileNotFoundException("找不到文件！");
        }

        var extension = Path.GetExtension(filePath).ToLower();
        using (var stream = new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
        {
            var sw = new Stopwatch();
            sw.Start();
            IExcelDataReader reader = null;
            if (extension == ".xls")
            {
                reader = ExcelReaderFactory.CreateBinaryReader(stream);
            }
            else if (extension == ".xlsx")
            {
                reader = ExcelReaderFactory.CreateOpenXmlReader(stream);
            }
            else if (extension == ".csv")
            {
                reader = ExcelReaderFactory.CreateCsvReader(stream);
            }

            if (reader == null)
                return null;

            var openTiming = sw.ElapsedMilliseconds;

            DataSet ds;

            using (reader)
            {
                ds = reader.AsDataSet(new ExcelDataSetConfiguration()
                {
                    UseColumnDataType = false,
                    ConfigureDataTable = (tableReader) => new ExcelDataTableConfiguration()
                    {
                        UseHeaderRow = useHeaderRow // 第一行包含列名
                    }
                });
            }

            return ds;
        }
    }

    public static DataTable ExcelToDataTable(string relativeFilePath, int sheet = 0, bool useHeaderRow = true)
    {
        var ds = ExcelToDataSet(relativeFilePath, useHeaderRow);
        if (ds == null)
            return null;
        return ds.Tables[sheet];
    }
}
