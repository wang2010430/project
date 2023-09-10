/***************************************************************************************************
* copyright : 芯迈微半导体（珠海）有限公司
* version   : 1.00
* file      : NVSSysManage.cs
* date      : 2023/8/5 9:27:58
* author    : jinlong.wang
* brief     : 
* section Modification History
* - 1.0 : Initial version - jinlong.wang
***************************************************************************************************/

using Common;
using log4net;
using NVParam.DAL;
using NVParam.Helper;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;

namespace NVParam.BLL
{
    public class NVSSysManage
    {
        #region Attribute
        Dictionary<int, SectorInfo> ListSectorInfo = new Dictionary<int, SectorInfo>();

        int ateSize = Marshal.SizeOf(typeof(NVSAte)); // 计算NVS_ATE结构的大小
        #endregion

        #region Constructor
        public NVSSysManage()
        {

        }
        #endregion

        #region Normal Function



        private BoolQResult WriteDataToSector(NVSAte ate, byte[] data, SectorInfo sectorInfo)
        {
            // 检查输入数据是否有效
            if (data == null || sectorInfo == null)
            {
                return new BoolQResult(false, "Data or Sector is NULL");
            }

            // 确保偏移地址在有效范围内
            if (ate.offset + ate.len <= sectorInfo.Datas.Length)
            {
                // 写入 NVSAte 数据


                // 写入 Data 数据，从 offset 处开始写入
                //Array.Copy(data, 0, sector, ate.offset, Math.Min(data.Length, ate.len));

                return new BoolQResult(true);
            }
            else
            {
                // 处理偏移地址超出范围的情况
                return new BoolQResult(false, "Offset and length exceed sector size.");
            }
        }

        public Dictionary<int, byte[]> ParseFromData(NVSParam param)
        {
            Dictionary<int, byte[]> itemIDs = new Dictionary<int, byte[]>();

            // 将数据分成n个分区
            for (int i = 0; i < param.SectorCount; i++)
            {
                byte[] partitions = NVCommon.GetSubArray(param.SectorData, i * param.SectorSize, param.SectorSize);
                SectorType type = GetSectorType(partitions);
                SectorInfo sectorInfo = new SectorInfo()
                {
                    Type = type,
                    Datas = partitions,
                };

                ListSectorInfo[i] = sectorInfo;
            }

            //查找WriteSector的Index
            int indexWriteSector = ListSectorInfo.FirstOrDefault(kv => kv.Value.Type == SectorType.WriteSector).Key;

            if (indexWriteSector > -1)
            {
                int index = 0;
                for (int count = 0; count < ListSectorInfo.Count; count++)
                {
                    index = indexWriteSector + count;
                    if (index >= ListSectorInfo.Count)
                    {
                        index -= ListSectorInfo.Count;
                    }

                    if (ListSectorInfo[index].Type == SectorType.WriteSector)
                    {
                        ReadWriteSector(ListSectorInfo[index].Datas, itemIDs);
                    }
                    else if (ListSectorInfo[index].Type == SectorType.CloseSector)
                    {
                        ReadCloseSector(ListSectorInfo[index].Datas, itemIDs);
                    }
                }
            }
            else
            {
                LogNetHelper.Warn("No WriteSector found.");
            }

            return itemIDs;
        }

        //读取WriteSector的ItemValue
        private void ReadWriteSector(byte[] sector, Dictionary<int, byte[]> listValue)
        {
            List<NVSAte> listAte = ExtractValidBlocks(sector);

            if (listAte.Count > 0)
            {
                for (int count = 0; count < listAte.Count; count++)
                {
                    int itemId = listAte[count].id;
                    int length = listAte[count].len;
                    int offset = listAte[count].offset;
                    byte[] itemData = NVCommon.GetSubArray(sector, offset, length);
                    if (itemData != null)
                    {
                        NVCommon.AddOrUpdateValue(listValue, itemId, itemData);
                    }
                    else
                    {
                        LogNetHelper.Warn($"ItemID = {itemId} Data is null");
                    }
                }
            }
        }


        //解析Close Sector，获取有效的ItemValue
        private void ReadCloseSector(byte[] sector, Dictionary<int, byte[]> listValue)
        {
            try
            {
                // 初始化closeATE
                byte[] dataAte = NVCommon.GetSubArray(sector, (sector.Length - ateSize), ateSize);
                if (dataAte == null)
                {
                    return;
                }

                NVSAte closeATE;
                ConvertToATE(dataAte, out closeATE);

                listValue = new Dictionary<int, byte[]>();
                int validCount = (sector.Length - closeATE.offset) / ateSize;

                NVSAte tempAte = new NVSAte();
                for (int count = 3; count < validCount; count++)
                {
                    byte[] ateData = new byte[ateSize];
                    Buffer.BlockCopy(sector, (sector.Length - (count + 1) * ateSize), ateData, 0, ateSize);
                    tempAte = ByteArrayToStructure<NVSAte>(ateData);
                    byte[] itemData = new byte[tempAte.len];
                    Buffer.BlockCopy(sector, tempAte.offset, itemData, 0, tempAte.len);
                    NVCommon.AddOrUpdateValue(listValue, tempAte.id, itemData);
                }
            }
            catch (Exception ex)
            {
                LogNetHelper.Warn(ex.Message);
            }
        }

        /// <summary>
        /// 获取有效的ATE列表
        /// </summary>
        /// <param name="sector"></param>
        /// <returns></returns>
        private List<NVSAte> ExtractValidBlocks(byte[] sector)
        {
            if (sector == null || sector.Length == 0)
            {
                return default;
            }

            List<NVSAte> validBlocks = new List<NVSAte>();
            int blockSize = ateSize;
            int endIndex = sector.Length - 1;

            for (int i = endIndex - 23; i >= 0; i -= blockSize)
            {
                byte[] AteData = new byte[blockSize];
                Array.Copy(sector, i, AteData, 0, blockSize);

                //判断全为FF
                if (IsByteArrayAllFF(AteData))
                {
                    break;
                }

                NVSAte ateData;
                bool bConvert = ConvertToATE(AteData, out ateData);
                if (bConvert)
                    validBlocks.Add(ateData);
            }

            return validBlocks;
        }
        /// <summary>
        /// 判断Byte[]是否全为FF
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        public bool IsByteArrayAllFF(byte[] data)
        {
            return data.All(b => b == 0xFF);
        }

        /// <summary>
        /// Convert Byte Array to ATE
        /// </summary>
        /// <param name="data"></param>
        /// <param name="ateData"></param>
        /// <returns></returns>
        private bool ConvertToATE(byte[] data, out NVSAte ateData)
        {
            ateData = new NVSAte();
            if (data.Length != ateSize)
            {
                return false;
            }

            ateData = ByteArrayToStructure<NVSAte>(data);
            return true;
        }

        private SectorType GetSectorType(byte[] sector)
        {
            if (IsByteArrayAllFF(sector))
            {
                return SectorType.EmptySector;
            }

            //get the ate
            byte[] dataAte = NVCommon.GetSubArray(sector, (sector.Length - ateSize), ateSize);
            if (dataAte == null)
            {
                return SectorType.EmptySector;
            }

            NVSAte nvsAte;
            ConvertToATE(dataAte, out nvsAte);

            if (IsAllFieldsFF(nvsAte))
            {
                return SectorType.WriteSector;
            }

            if (nvsAte.id == 0xFFFF && nvsAte.len == 0)
            {
                return SectorType.CloseSector;
            }

            return SectorType.NullSector;
        }


        /// <summary>
        /// 获取分区类型
        /// </summary>
        private SectorType GetSectorType(NVSAte ate)
        {
            if (IsAllFieldsFF(ate))
            {
                return SectorType.WriteSector;
            }

            if (ate.id == 0xFFFF && ate.len == 0)
            {
                return SectorType.CloseSector;
            }

            return SectorType.NullSector;
        }

        //是否全部为1
        private bool IsAllFieldsFF(NVSAte ate)
        {
            return ate.id == 0xFFFF && ate.offset == 0xFFFF && ate.len == 0xFFFF &&
                   ate.part == 0xFF && ate.crc8 == 0xFF;
        }


        // 将字节数组转换为结构体并处理小端模式
        private T ByteArrayToStructure<T>(byte[] bytes) where T : struct
        {
            T result;
            int size = Marshal.SizeOf(typeof(T));

            if (size > bytes.Length)
            {
                throw new ArgumentException("Byte array is smaller than the size of the target structure.");
            }

            // 如果是大端模式，反转字节数组
            if (!BitConverter.IsLittleEndian)
            {
                Array.Reverse(bytes);
            }

            GCHandle handle = GCHandle.Alloc(bytes, GCHandleType.Pinned);
            try
            {
                result = (T)Marshal.PtrToStructure(handle.AddrOfPinnedObject(), typeof(T));
            }
            finally
            {
                handle.Free();
            }
            return result;
        }

        #endregion
    }
}
