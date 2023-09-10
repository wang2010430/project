using Common;
using NVGen.BLL;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NVGen
{
    class Program
    {
        static void Main(string[] args)
        {
            string inputFile = null;
            string outputFile = null;

            // 解析命令行参数
            for (int i = 0; i < args.Length; i++)
            {
                if (args[i] == "-F" && i + 1 < args.Length)
                {
                    inputFile = args[i + 1];
                    i++; // 跳过下一个参数，因为已经处理了
                }
                else if (args[i] == "-O" && i + 1 < args.Length)
                {
                    outputFile = args[i + 1];
                    i++; // 跳过下一个参数，因为已经处理了
                }
            }

            // 检查参数是否有效
            if (string.IsNullOrEmpty(inputFile) || string.IsNullOrEmpty(outputFile))
            {
                Console.WriteLine("Usage: NVGen.exe -F inputFileName -O outputFileName");
                return;
            }

            // 执行 ItemDataNode 转换并保存到文件
            try
            {
                ProjectService projectService = new ProjectService();
                BoolQResult ret =  projectService.ConvertFileToBin(inputFile, outputFile);
                if(ret.Result )
                    Console.WriteLine("ItemDataNode conversion completed. Data saved to " + outputFile);
                else
                    Console.WriteLine("Failed ! case : " + ret.Msg);
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error: " + ex.Message);
            }
        }
    }
}
