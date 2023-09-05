/***************************************************************************************************
* copyright : 芯迈微半导体（珠海）有限公司
* version   : 1.00
* file      : FileUtils.cs
* date      : 2023/03/16
* author    : haozhe.ni
* brief     : 文件操作类
* section Modification History
* - 1.0 : Initial version - haozhe.ni
***************************************************************************************************/

using System;
using System.IO;
using System.Text;
using System.Collections.Generic;
using log4net;

namespace Common
{
    /// <summary>
    /// 文件处理用静态函数
    /// </summary>
    sealed public class FileUtils
    {
        static private object fileObj = new object();

        /// <summary>
        /// 删除文件，返回是否成功
        /// </summary>
        /// <param name="fileName"></param>
        /// <returns></returns>
        static public BoolQResult DeleteAuthorizedFile(string fileName)
        {
            string msg = "";

            try
            {
                if (File.Exists(fileName))
                {
                    File.Delete(fileName);
                }
            }
            catch (Exception e)
            {
                msg = e.ToString();
            }

            return new BoolQResult(!File.Exists(fileName), msg);
        }

        /// <summary>
        /// 删除某目录中的文件，包括只读文件及隐藏文件
        /// </summary>
        /// <param name="direcotryName"></param>
        static public void DeleteAuthorizedDirectory(string direcotryName)
        {
            if (!string.IsNullOrEmpty(direcotryName))
            {
                string[] sourceFiles = Directory.GetFiles(direcotryName);  //升级目录下的所有文件

                foreach (string f in sourceFiles)
                {
                    DeleteAuthorizedFile(f);
                }
            }
        }

        /// <summary>
        /// 获取纯文件名,该文件名可能包含路径名(Windows或Linux格式）
        /// </summary>
        /// <param name="fileName"></param>
        /// <returns></returns>
        static public string GetFileName(string fileName)
        {
            fileName = Path.GetFileName(fileName);

            if (CommonHelper.OperationSystem == OsFamily.Windows)
            {
                // 原文件名可能是Linux下获取的
                return GetFileName(fileName, '/');
            }
            else if (CommonHelper.OperationSystem == OsFamily.Uniux)
            {
                // 原文件名可能是在Windows下获取的
                return GetFileName(fileName, '\\');
            }

            return fileName;
        }

        /// <summary>
        /// 根据指定的目录分割符来获取纯文件名
        /// </summary>
        /// <param name="fileName"></param>
        /// <param name="pathSeperator"></param>
        /// <returns></returns>
        static public string GetFileName(string fileName, char pathSeperator)
        {
            int k = fileName.LastIndexOf(pathSeperator);

            if (k >= 0)
            {
                return fileName.Substring(k + 1);
            }

            return fileName;
        }

        /// <summary>
        /// 读取文件
        /// </summary>
        /// <param name="fileName"></param>
        /// <returns></returns>
        static public BoolQResult ReadFileBytes(string sourceFileName, int sizeLimit)
        {
            if (string.IsNullOrEmpty(sourceFileName))
            {
                return new BoolQResult(false, "要保存文件名为空");
            }

            FileInfo fi = new FileInfo(sourceFileName);

            if (!fi.Exists)
            {
                return new BoolQResult(false, string.Format("文件：{0} 不存在", sourceFileName));
            }

            MemoryStream ms = new MemoryStream();

            try
            {
                FileStream fs = fi.OpenRead();

                if (fs == null)
                {
                    return new BoolQResult(false, string.Format("打开文件：{0} 失败", sourceFileName));
                }

                int readBufferSize = 10000;
                byte[] readBuffer = new byte[readBufferSize];
                int readCount = readBufferSize;

                while (readCount == readBufferSize && ms.Length <= sizeLimit)
                {
                    readCount = fs.Read(readBuffer, 0, readBufferSize);

                    if (readCount > 0)
                    {
                        ms.Write(readBuffer, 0, readCount);
                    }
                }

                fs.Close();

                if (ms.Length > sizeLimit)
                {
                    return new BoolQResult(false, string.Format("文件：{0} 大小超过{1}", sourceFileName, sizeLimit));
                }

                BoolQResult r = new BoolQResult(true)
                {
                    Tag = ms.ToArray()
                };

                return r;
            }
            catch (Exception e)
            {
                return new BoolQResult(false, e.ToString());
            }
        }

        /// <summary>
        /// 删除用户VirtualStore指定目录下目录下某文件
        /// </summary>
        /// <param name="filePath"></param>
        /// <param name="fileName"></param>
        static public void DeleteVirtualStoreAndRecentFile(string filePath, string fileName)
        {
            string localFileName = Path.Combine(filePath, fileName);
            string fullFileName = Path.Combine(VirtualStoreDataPath(filePath), fileName);

            if (!localFileName.Equals(fullFileName, StringComparison.InvariantCultureIgnoreCase) && File.Exists(fullFileName))
            {
                DeleteAuthorizedFile(fullFileName);
            }

            fullFileName = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Recent), fileName);

            if (!localFileName.Equals(fullFileName, StringComparison.InvariantCultureIgnoreCase) && File.Exists(fullFileName))
            {
                DeleteAuthorizedFile(fullFileName);
            }
        }

        /// <summary>
        /// 将文件从VirtualStore目录或Recent目录下拷贝到当前路径
        /// </summary>
        /// <param name="fileName"></param>
        /// <param name="deleteAfterCopy">是否拷贝后删除原文件</param>
        static public void CopyFileFromVirtualStore(string filePath, string fileName, bool deleteAfterCopy)
        {
            string localFileName = Path.Combine(filePath, fileName);

            if (!File.Exists(localFileName))
            {
                string virtualStoreFileName = Path.Combine(VirtualStoreDataPath(filePath), fileName);

                if (File.Exists(virtualStoreFileName))
                {
                    File.Copy(virtualStoreFileName, localFileName);

                    if (deleteAfterCopy)
                    {
                        DeleteAuthorizedFile(virtualStoreFileName);
                    }
                }
                else
                {
                    string recentFileName = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Recent), fileName);

                    if (File.Exists(recentFileName))
                    {
                        File.Copy(recentFileName, localFileName);

                        if (deleteAfterCopy)
                        {
                            DeleteAuthorizedFile(recentFileName);
                        }
                    }
                }
            }
        }

        static private string VirtualStoreDataPath(string filePath)
        {
            string p1 = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "VirtualStore");
            string p2 = filePath;
            int idx = p2.IndexOf(@":\");

            if (idx >= 0)
            {
                p2 = p2.Substring(idx + 2);
            }

            return Path.Combine(p1, p2);
        }

        /// <summary>
        /// 读取某路径下的子路径，而不是直接使用函数Directory.GetDirectories(,,true)
        /// 应该函数在遇到无权限的目录时会直接
        /// </summary>
        /// <param name="d"></param>
        /// <param name="limit"></param>
        /// <returns></returns>
        static public List<string> RetrieveAuthorizedDirectory(string d, int limit)
        {
            List<string> l = new List<string>();

            try
            {
                string[] all = Directory.GetDirectories(d);

                foreach (string s in all)
                {
                    l.Add(s);
                    try
                    {
                        List<string> n = RetrieveAuthorizedDirectory(s, limit);

                        foreach (string ll in n)
                        {
                            l.Add(ll);
                        }
                    }
                    catch
                    {
                    }

                    if (l.Count > limit)
                    {
                        return l;
                    }
                }
            }
            catch
            {
                return l;
            }

            return l;
        }

        #region 文件处理
        /// <summary>
        /// 根据文本文件的保存格式，返回适当的StreamReader，如无法判定，则使用指定的编码
        /// </summary>
        /// <param name="textFile"></param>
        /// <returns></returns>
        static public StreamReader GetStreamReader(string textFile, string encordingName)
        {
            if (!File.Exists(textFile))
            {
                return null;
            }

            StreamReader sr = new StreamReader(textFile, Encoding.GetEncoding(encordingName));

            return sr;

        }

        /// <summary>
        /// 读取一文件，返回读取的所有字节，文件不存在或读取出错时返回null，该函数不能用于读大文件
        /// </summary>
        /// <param name="fileName">文件完整路径</param>
        /// <returns></returns>
        static public byte[] ReadFileBytes(string fileName)
        {
            byte[] buff = null;

            if (File.Exists(fileName))
            {
                try
                {
                    using (FileStream rs = new FileStream(fileName, FileMode.Open, FileAccess.Read, FileShare.Read))
                    {
                        if (rs != null)
                        {
                            if (rs.Length > 0)
                            {
                                int readLength = (int)rs.Length;

                                buff = new byte[readLength];
                                rs.Read(buff, 0, readLength);
                            }

                            rs.Close();
                        }
                    }
                }
                catch (FileNotFoundException ex)
                {
                    LogNetHelper.Error(ex.Message);
                }
            }

            return buff;
        }

        /// <summary>
        /// 将字节保存成文件，该函数不能用于写大文件
        /// </summary>
        /// <param name="fileName">文件完整路径</param>
        /// <returns></returns>
        static public BoolQResult WriteFileBytes(byte[] data, string fileName)
        {
            BoolQResult ret = new BoolQResult(true, "Write File successed!");

            try
            {
                if (File.Exists(fileName))
                {
                    File.Delete(fileName);
                }

                FileStream fs = new FileStream(fileName, FileMode.CreateNew);
                BinaryWriter bw = new BinaryWriter(fs);
                bw.Write(data, 0, data.Length);
                bw.Close();
                fs.Close();

                return ret;
            }
            catch (Exception ex)
            {
                ret.Result = false;
                ret.Msg = ex.Message;

                return ret;
            }
        }

        /// <summary>
        /// Append File Bytes
        /// </summary>
        /// <param name="data"></param>
        /// <param name="fileName"></param>
        /// <returns></returns>
        static public BoolQResult AppendFileBytes(byte[] data, string fileName)
        {
            BoolQResult ret = new BoolQResult(true, "Append to file successed!");

            try
            {
                FileStream fs;
                if (File.Exists(fileName))
                {
                    fs = new FileStream(fileName, FileMode.Append, FileAccess.Write);
                }
                else
                {
                    fs = new FileStream(fileName, FileMode.CreateNew, FileAccess.Write);
                }

                using (BinaryWriter bw = new BinaryWriter(fs))
                {
                    bw.Write(data, 0, data.Length);
                }

                return ret;
            }
            catch (Exception ex)
            {
                ret.Result = false;
                ret.Msg = ex.Message;

                return ret;
            }
        }

        /// <summary>
        /// 覆盖某文件
        /// </summary>
        /// <param name="fileName">文件名</param>
        /// <param name="fileBytes">文件内容</param>
        /// <returns></returns>
        static public bool OverwriteFile(string fileName, byte[] fileBytes)
        {
            if (fileBytes == null)
            {
                return false;
            }
            try
            {
                using (FileStream ws = new FileStream(fileName, FileMode.Create))
                {
                    ws.Write(fileBytes, 0, fileBytes.Length);
                    ws.Close();

                    return true;
                }
            }
            catch (IOException)
            {
                return false;
            }
        }

        /// <summary>
        /// 读取一文本文件
        /// </summary>
        /// <param name="textFileName"></param>
        /// <returns></returns>
        static public string ReadTextFile(string textFileName)
        {
            lock (fileObj)
            {
                string l = "";

                try
                {
                    if (File.Exists(textFileName))
                    {
                        using (StreamReader sr = new StreamReader(textFileName))
                        {
                            l = sr.ReadToEnd();
                        }
                    }
                }
                catch
                {
                }

                return l;
            }
        }

        /// <summary>
        /// 判断两个时间是否在一定的时间间隔内
        /// </summary>
        /// <param name="t1"></param>
        /// <param name="t2"></param>
        /// <param name="gap"></param>
        /// <returns></returns>
        static public bool WithinSameTimeSpan(DateTime t1, DateTime t2, TimeSpan gap)
        {
            return t1 > t2 - gap && t1 < t2 + gap;
        }

        public static BoolQResult IsFileChanged(string fileName, long fileSize, DateTime lastModifyTimeUtc, FileInfo dataFileInfo)
        {
            if (dataFileInfo.Length != fileSize)
            {
                return new BoolQResult(true, string.Format("文件:{0}大小不同", fileName));
            }
            else if (!WithinSameTimeSpan(dataFileInfo.LastWriteTimeUtc, lastModifyTimeUtc, new TimeSpan(0, 1, 0)))
            {
                return new BoolQResult(true, string.Format("文件:{0} 时间不同,原文件最近一次修改时间:{1},新文件最近一次修改时间:{2}(均为国际标准时)",
                                                         fileName, dataFileInfo.LastWriteTimeUtc, lastModifyTimeUtc));
            }
            else
            {// 文件大小及时间相同
                return new BoolQResult(string.Format("文件:{0} 大小及时间相同", fileName));
            }
        }
        #endregion

        /// <summary>
        /// 根据扩展名获取某目录下的所有文件列表
        /// </summary>
        /// <param name="directory">目录名</param>
        /// <param name="extendName">扩展名</param>
        /// <returns></returns>
        public static List<string> GetFileListByExtension(string directory, string extendName)
        {
            List<string> resList = new List<string>();

            // 获取指定文件夹的所有文件
            if (!string.IsNullOrEmpty(extendName) && extendName.IndexOf('.') < 0)
            {
                extendName = "." + extendName;
            }

            string[] paths = Directory.GetFiles(directory);

            foreach (string item in paths)
            {
                // 获取文件后缀名
                string extension = Path.GetExtension(item);

                if (extension.Equals(extendName, StringComparison.CurrentCultureIgnoreCase))
                {
                    resList.Add(item);
                }
            }

            return resList;
        }

        /// <summary>
        /// 替换文件的路径
        /// </summary>
        /// <param name="fullPathFileName"></param>
        /// <param name="newPath"></param>
        /// <returns></returns>
        public static string GetPathReplacedFile(string fullPathFileName, string newPath)
        {
            if (string.IsNullOrEmpty(newPath))
            {
                return fullPathFileName;
            }

            string fileName = GetFileName(fullPathFileName);
            string newFile = Path.Combine(newPath, fileName);

            return newFile;
        }

        /// <summary>
        /// 获取目录下所有文件大小
        /// </summary>
        /// <param name="path"></param>
        /// <returns></returns>
        public static long GetDirectoryLength(string dirPath)
        {
            //判断给定的路径是否存在,如果不存在则退出
            if (!Directory.Exists(dirPath))
            {
                return 0;
            }

            long len = 0;

            //定义一个DirectoryInfo对象
            DirectoryInfo di = new DirectoryInfo(dirPath);

            //通过GetFiles方法,获取di目录中的所有文件的大小
            foreach (FileInfo fi in di.GetFiles())
            {
                len += fi.Length;
            }

            //获取di中所有的文件夹,并存到一个新的对象数组中,以进行递归
            DirectoryInfo[] dis = di.GetDirectories();

            if (dis.Length > 0)
            {
                for (int i = 0; i < dis.Length; i++)
                {
                    len += GetDirectoryLength(dis[i].FullName);
                }
            }

            return len;
        }

        /// <summary>
        /// 文件夹超过限制删除一半内存文件
        /// </summary>
        /// <param name="dirPath">文件夹目录</param>
        /// <param name="maxMemory">最大内存</param>
        /// <returns></returns>
        public static BoolQResult DeleteFileByMemory(string dirPath, long maxMemory)
        {
            try
            {
                DirectoryInfo directoryInfo = new DirectoryInfo(dirPath);

                if (GetDirectoryLength(dirPath) > maxMemory)
                {
                    List<FileHelper> fileList = new List<FileHelper>();

                    foreach (var fileName in Directory.GetFiles(dirPath))
                    {
                        fileList.Add(new FileHelper(fileName));
                    }

                    fileList.Sort();

                    int i = 0;

                    do
                    {
                        File.Delete(fileList[i++].FullPath);
                    }
                    while (GetDirectoryLength(dirPath) > maxMemory / 2);
                }

                return new BoolQResult(true);
            }
            catch (Exception ex)
            {
                return new BoolQResult(false) { Msg = ex.Message };
            }
        }

        /// <summary>
        /// 修改文件名
        /// </summary>
        /// <param name="filePath">文件路径</param>
        /// <param name="newName">新文件名</param>
        /// <returns></returns>
        public static BoolQResult ModifyFileName(string filePath, string newName, out string newFilePath)
        {
            BoolQResult ret = new BoolQResult(false);

            newFilePath = "";

            if (!File.Exists(filePath))
            {
                ret.Msg = "目录文件不存在";
            }

            try
            {
                string dirPath = filePath.Remove(filePath.LastIndexOf("\\"));
                newFilePath = Path.Combine(dirPath, newName);
                File.Move(filePath, newFilePath);
                ret.Result = true;

                return ret;
            }
            catch (Exception ex)
            {
                ret.Msg = ex.Message;

                return ret;
            }
        }
    }
}
