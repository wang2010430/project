/***************************************************************************************************
* copyright : CMIND-SEMI
* version   : 1.00
* file      : NVSStorage.cs
* date      : 2023/8/18 15:24:46
* author    : jinlong.wang
* brief     : 
* section Modification History
* - 1.0 : Initial version - jinlong.wang
***************************************************************************************************/

using Common;
using NVSSystem.DLL;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;

namespace NVSSystem.BLL
{
    public class NVSStorage
    {
        byte[] key = null; // 16, 24, or 32 bytes for AES-128, AES-192, or AES-256
        byte[] iv = null; // 16 bytes

        public NVSStorage()
        {
            key = new byte[16];
            iv = new byte[16];

            Array.Copy(Encoding.ASCII.GetBytes("CmindKey1234567890"), key, key.Length);
            Array.Copy(Encoding.ASCII.GetBytes("CmindIV1234567890"), iv, iv.Length);
        }


        public BoolQResult SaveData(string filePath, StorageParam roParam, StorageParam rwParam)
        {
            try
            {
                List<byte> listROData = new List<byte>();
                byte[] roData = ConvertToByteArray(roParam);

                listROData.AddRange(NVSCommon.Separator);
                listROData.AddRange(roData);
                listROData.AddRange(NVSCommon.Separator);

                List<byte> listRWData = new List<byte>();
                byte[] rwData = ConvertToByteArray(rwParam);
                listRWData.AddRange(NVSCommon.Separator);
                listRWData.AddRange(rwData);
                listRWData.AddRange(NVSCommon.Separator);

                listROData.AddRange(listRWData);

                byte[] fileData = listROData.ToArray();
                SymmetricEncryption symmetricEncryption = new SymmetricEncryption(key, iv);
                byte[] DEncrypted = symmetricEncryption.Encrypt(fileData);

                return FileUtils.WriteFileBytes(DEncrypted, filePath);
            }
            catch (Exception ex)
            {
                return new BoolQResult(false, ex.Message);
            }
        }

        /// <summary>
        /// 读取NVS的bin文件
        /// </summary>
        /// <param name="filePath"></param>
        /// <param name="roParam"></param>
        /// <param name="rwParam"></param>
        public BoolQResult ReadData(string filePath, StorageParam roParam, StorageParam rwParam)
        {
            try
            {
                byte[] data = FileUtils.ReadFileBytes(filePath);
                if (data == null)
                {
                    return new BoolQResult(false, "File Data is NULL");
                }

                SymmetricEncryption symmetricEncryption = new SymmetricEncryption(key, iv);
                byte[] DDecryptData = symmetricEncryption.Decrypt(data);
                //Find Separator
                List<byte[]> validData = DataProcessor.FindValidDataWithSeparator(DDecryptData, NVSCommon.Separator);
                for (int count = 0; count < validData.Count; count++)
                {
                    StorageParam param = ConvertToStorageParam(validData[count]);
                    if (param.SAttribute == SectorAttribute.RO)
                    {
                        roParam = param;
                    }
                    else if (param.SAttribute == SectorAttribute.RW)
                    {
                        rwParam = param;
                    }
                }

                return new BoolQResult(true, "Read Data Successed");
            }
            catch (Exception ex)
            {
                return new BoolQResult(false, ex.Message);
            }
        }

        public byte[] ConvertToByteArray(StorageParam param)
        {
            int structSize = Marshal.SizeOf(param) - 4;
            int arraySize = param.SectorData.Length;
            int totalSize = structSize + arraySize;

            byte[] byteArray = new byte[totalSize];

            using (MemoryStream stream = new MemoryStream(byteArray))
            using (BinaryWriter writer = new BinaryWriter(stream))
            {
                writer.Write((int)param.SAttribute);
                writer.Write(param.SectorSize);
                writer.Write(param.SectorCount);
                writer.Write(param.CRC);
                writer.Write(param.SectorData);
            }

            return byteArray;
        }

        public StorageParam ConvertToStorageParam(byte[] byteArray)
        {
            StorageParam param = new StorageParam();

            using (MemoryStream stream = new MemoryStream(byteArray))
            using (BinaryReader reader = new BinaryReader(stream))
            {
                param.SAttribute = (SectorAttribute)reader.ReadInt32();
                param.SectorSize = reader.ReadInt32();
                param.SectorCount = reader.ReadInt32();
                param.CRC = reader.ReadUInt16();

                int sectorDataLength = byteArray.Length - Marshal.SizeOf(param) + 4;
                param.SectorData = reader.ReadBytes(sectorDataLength);
            }

            return param;
        }
    }
}
