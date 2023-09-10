/***************************************************************************************************
* copyright : CMIND-SEMI
* version   : 1.00
* file      : INVRamManage.cs
* date      : 2023/7/10
* author    : jinlong.wang
* brief     :  NV Param Manage
* section Modification History
* - 1.0 : Initial version - jinlong.wang
***************************************************************************************************/
#define PC_DEBUG
using Channel;
using CmindProtocol;
using CmindProtocol.CmindBusiness;
using CmindProtocol.DLL;
using Common;
using log4net;
using NVParam.DAL;
using NVParam.Helper;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;

namespace NVParam.BLL
{
    public partial class NVRamManage //: INVRamManage
    {
        #region Attribute
        private NVRamParam nvRamParam = new NVRamParam();
        public NVRamParam NvRamParam { get => nvRamParam; set => nvRamParam = value; }
        public Cmind CmindProtocol { get; set; } = new Cmind();
        public string ProjectFilePath { get; set; }

        private int ATESize = Marshal.SizeOf(typeof(NVSAte));
        #endregion

        #region Constructor
        public NVRamManage()
        {
            //init param
            NvRamParam.Version = "V1.0.0";
            NvRamParam.Project = "NanH";
        }
        #endregion


        #region Normal Function
        /// <summary>
        /// loading source 
        /// </summary>
        /// <param name="filePath"></param>
        /// <returns></returns>
        public BoolQResult LoadSource(string filePath)
        {
            try
            {
                if (!File.Exists(filePath))
                    return new BoolQResult(false, "File is not existed");

                ProjectFilePath = filePath;

                nvRamParam = XmlHelper.OpenXML<NVRamParam>(filePath);
                return new BoolQResult(true, "Successed");
            }
            catch (Exception ex)
            {
                return new BoolQResult(false, ex.Message);
            }
        }
        /// <summary>
        /// Upload project
        /// </summary>
        /// <returns></returns>
        public BoolQResult UploadProject(Action<int, double> ItemIDCallBack)
        {
            if (nvRamParam.Item.ItemName == null)
            {
                return new BoolQResult(false, "Empty Project");
            }

            if (!CmindProtocol.ProtocolIsRunning)
            {
                return new BoolQResult(false, "Please configure the serial port!");
            }

            //start to write project
            List<ushort> listItemID = FindAllItemIDs(nvRamParam.Item);

            NVWriteParam param = new NVWriteParam()
            {
                DownloadMode = (byte)NVDownloadMode.NORMAL_MODE,
                OperationMode = (byte)NVOperateMode.OneByOneMode,
            };

            foreach (ushort Id in listItemID)
            {
                ItemDataNode node = FindItemDataNodeById(nvRamParam.Item, Id);
                if (node != null)
                {
                    Action<double> progressCallBack = (progress) =>
                    {
                        // 在这里处理进度更新的逻辑
                        ItemIDCallBack?.Invoke(Id, progress);
                        //
                    };
                    param.ItemID = Id;
                    param.Length = (uint)GetTotalByteCount(node);
                    byte[] sendData = ConvertToByte(node);
                    LogNetHelper.Debug($"NodeName = {node.ItemName} ItemId = {Id} sendData = {sendData.Length}  param.Length = { param.Length}");

                    if (param.Length == sendData.Length)
                    {
                        BusinessResult bResult = CmindProtocol.WriteNVItemDataByID(param, progressCallBack, sendData);
                        if (bResult.Result == false)
                        {
                            return new BoolQResult(false, $"Upload ItemID = {Id} Failed: case <{bResult.Msg}>");
                        }
                    }
                    else
                    {
                        return new BoolQResult(false, $"Length is wrong, param.Length = {param.Length} sendData.Length = {sendData.Length}");
                    }
                }
            }

            return new BoolQResult(true, "Upload Successed");
        }

        /// <summary>
        /// Dowload  Project
        /// </summary>
        /// <returns></returns>
        public BoolQResult DownloadProject(Action<int, double> ItemProgressCallBack)
        {
            try
            {
                if (nvRamParam.Item.ItemName == null)
                {
                    return new BoolQResult(false, "Empty Project");
                }
                if (!CmindProtocol.ProtocolIsRunning)
                {
                    return new BoolQResult(false, "Please configure the serial port!");
                }

                List<ushort> listItemID = FindAllItemIDs(nvRamParam.Item);
                NVReadParam param = new NVReadParam()
                {
                    OperationMode = (byte)NVOperateMode.OneByOneMode,
                };

                foreach (ushort Id in listItemID)
                {
                    ItemDataNode node = FindItemDataNodeById(nvRamParam.Item, Id);
                    if (node != null)
                    {
                        Action<double> progressCallBack = (progress) =>
                        {
                            // 在这里处理进度更新的逻辑
                            ItemProgressCallBack?.Invoke(Id, progress);
                        };
                        param.ItemID = Id;
                        param.Length = (ushort)GetTotalByteCount(node);
                        ReadNVDataResult bResult = CmindProtocol.ReadNVItemDataByID(param, progressCallBack, param.Length);
                        if (bResult.Result == false)
                        {
                            return new BoolQResult(false, $"Download ItemID = {Id} Failed: case <{bResult.Msg}>");
                        }

                        BoolQResult setResult = SetItemValuesFromBytes(node, bResult.datas);
                        if (setResult.Result == false)
                        {
                            return setResult;
                        }
                    }
                }

                return new BoolQResult(true, "Download Successed");
            }
            catch (Exception ex)
            {
                return new BoolQResult(false, ex.Message);
            }
        }
        /// <summary>
        /// save Image To Phone 
        /// </summary>
        /// <param name="progressCallBack"></param>
        /// <returns></returns>
        public BoolQResult SaveImageToPhone(Action<string> progressCallBack, string backupFilePath)
        {
            //Backup nvs sys
            progressCallBack?.Invoke("Backup in progress ......");
            BoolQResult ret = BackupNVSFromBoard(progressCallBack, backupFilePath);
            if (!ret.Result)
            {
                return ret;
            }

            NVSParam rwNVSParam = null;
            NVSParam roNVSParam = null;
            progressCallBack?.Invoke("Convert to Bin in progress ......");
            ConvertNodeToNVBin(nvRamParam.Item, out rwNVSParam, out roNVSParam);

            NVWriteParam param = new NVWriteParam()
            {
                DownloadMode = (byte)NVDownloadMode.NORMAL_MODE,
                OperationMode = (byte)NVOperateMode.WholeMode,
            };

            //RW
            PartitionType partitionType = PartitionType.RW;
            param.Length = (uint)rwNVSParam.SectorData.Length;
            param.ItemID = NVCommon.GenerateItemID((byte)partitionType, (byte)Domain.Commmon, 0);
   
            Action<double> callBack = (progress) =>
            {
                string result = progress.ToString("0.00");
                string callbackMsg = $"Write {partitionType} Sectors, Progress = {result}%";
                progressCallBack?.Invoke(callbackMsg);
            };

            BusinessResult bResult = CmindProtocol.WriteNVItemDataByID(param, callBack, rwNVSParam.SectorData);
            if (bResult.Result == false)
            {
                return new BoolQResult(false, $"Write {partitionType.ToString()} Sector Failed: case <{bResult.Msg}>");
            }

            return new BoolQResult(true, "Saved bin to phone successfully");
        }

        /// <summary>
        /// Loads image data from a phone and processes it.
        /// </summary>
        /// <param name="callBack">A callback action to report progress or status.</param>
        /// <param name="DiffIDs">A list of different IDs that are updated during processing (will be cleared).</param>
        /// <returns>A BoolQResult indicating the success or failure of the operation.</returns>
        public BoolQResult LoadImageFromPhone(Action<string> callBack, ref List<int> DiffIDs)
        {
            DiffIDs.Clear();
#if PC_DEBUG
            ReadNVDataResult rwResult = new ReadNVDataResult()
            {
                sectorCount = NVCommon.RWSectorCount,
                sectorSize = NVCommon.RWSectorSize,
                datas = FileUtils.ReadFileBytes("bin/partitionsRW.bin")
            };
#else
            ReadNVDataResult roResult = ReadSector(callBack, PartitionType.RO);
            if (roResult.Result == false)
            {
                return new BoolQResult(false, $"Read From Phone Failed: case <{roResult.Msg}>");
            }
#if DEBUG
            SavePartitionROData(roResult, PartitionType.RO);
#endif
            //读取RW分区
            ReadNVDataResult rwResult = ReadSector(callBack, PartitionType.RW);
            if (rwResult.Result == false)
            {
                return new BoolQResult(false, $"Read From Phone Failed: case <{rwResult.Msg}>");
            }

#if DEBUG
            SavePartitionROData(roResult, PartitionType.RW);
#endif
#endif
            Dictionary<int, byte[]> sectorData = ParseSectorData(rwResult);
            Dictionary<int, byte[]> validData = FindAndHandleNodeDiff(sectorData, nvRamParam.Item);

            DiffIDs = validData.Keys.ToList();
            return new BoolQResult(true, "Load data from phone successfully");
        }

        /// <summary>
        /// Converts item node data to a binary file and saves it.
        /// </summary>
        /// <param name="callBack">A callback action to report progress or status.</param>
        /// <param name="binFilePath">The file path where the binary data will be saved.</param>
        /// <returns>A BoolQResult indicating the success or failure of the operation.</returns>
        public BoolQResult ConvertItemNodeToBin(Action<string> callBack, string binFilePath)
        {
            // Initialize NVSParam objects for RW and RO sectors.
            NVSParam rwNVSParam = null;
            NVSParam roNVSParam = null;

            // Invoke the callback to report progress.
            callBack?.Invoke("Convert to Bin in progress ......");

            // Convert the item node data to NVSParam format.
            ConvertNodeToNVBin(nvRamParam.Item, out rwNVSParam, out roNVSParam);

            // Create an NVSStorage instance.
            NVSStorage nvsStorage = new NVSStorage();

            // Save the data to the specified binary file.
            BoolQResult ret = nvsStorage.SaveData(binFilePath,
                NVCommon.ConvertParameter(rwNVSParam, SectorAttribute.RW), NVCommon.ConvertParameter(roNVSParam, SectorAttribute.RO));

            return ret;
        }
        #endregion

        #region Common Function
        /// <summary>
        /// Finds and handles differences between ItemDataNode IDs and a dictionary of node data.
        /// </summary>
        /// <param name="listNodeData">A dictionary containing node data with IDs as keys.</param>
        /// <param name="parentNode">The parent ItemDataNode to search for child nodes.</param>
        /// <returns>A dictionary containing valid node data based on differences.</returns>
        public Dictionary<int, byte[]> FindAndHandleNodeDiff(Dictionary<int, byte[]> listNodeData, ItemDataNode parentNode)
        {
            List<ushort> listItemID = FindAllItemIDs(nvRamParam.Item);
            List<ushort> listNVSID = new List<ushort>();

            // Get all keys from listNodeData and convert them to a list of ushort
            var keys = listNodeData.Keys;
            foreach (int key in keys)
            {
                listNVSID.Add((ushort)key);
            }

            // Find common ItemDataNode IDs between listItemID and listNVSID
            var differentValues = listItemID.Intersect(listNVSID);

            Dictionary<int, byte[]> validData = new Dictionary<int, byte[]>();

            foreach (ushort key in differentValues)
            {
                // Find the corresponding ItemDataNode by ID
                ItemDataNode diffNode = FindItemDataNodeById(parentNode, key);

                // Convert the ItemDataNode to bytes
                byte[] nodeData = ConvertToByte(diffNode);

                // Check if the byte length matches the expected length
                if (nodeData.Length == listNodeData[key].Length)
                {
#if DEBUG
                    LogNetHelper.Debug($"NodeData Length = {nodeData.Length}, Key = {listNodeData[key].Length}");
#endif

                    // Add valid node data to the result dictionary
                    validData.Add(key, listNodeData[key]);

                    // Set the ItemDataNode values from the byte data
                    BoolQResult result = SetItemValuesFromBytes(diffNode, listNodeData[key]);

                    // Log a message if setting values fails
                    if (!result.Result)
                    {
                        LogNetHelper.Info(result.Msg);
                    }
                }
                else
                {
#if DEBUG
                    LogNetHelper.Info($"Different NodeData Length = {nodeData.Length}, Key = {listNodeData[key].Length}");
#endif
                }
            }

            return validData;
        }


        /// <summary>
        /// Parses sector data retrieved from a ReadNVDataResult and returns a dictionary 
        /// </summary>
        /// <param name="result">A ReadNVDataResult object containing sector data.</param>
        /// <returns>A dictionary representing the parsed sector data.</returns>
        private Dictionary<int, byte[]> ParseSectorData(ReadNVDataResult result)
        {
            // Check if the input parameters are valid
            if (result == null || result.datas == null || result.datas.Length == 0)
            {
                return null;
            }

            // Create an NVSParam object to pass sector data information
            NVSParam nvsParam = new NVSParam()
            {
                SectorSize = result.sectorSize * 1024,
                SectorCount = result.sectorCount,
                SectorData = result.datas
            };

            // Create an NVSSysManage object for data parsing
            NVSSysManage nvsSysManage = new NVSSysManage();

            try
            {
                // Call the NVSSysManage.ParseFromData method to parse sector data
                Dictionary<int, byte[]> itemDatas = nvsSysManage.ParseFromData(nvsParam);
                return itemDatas;
            }
            catch (Exception ex)
            {
                // Catch exceptions during parsing and log error messages
                LogNetHelper.Error("Error parsing sector data: " + ex.Message);
                return null;
            }
        }

        /// <summary>
        /// 赋值到ItemDataNode中
        /// </summary>
        /// <param name="node"></param>
        /// <param name="valueBytes"></param>
        public BoolQResult SetItemValuesFromBytes(ItemDataNode node, byte[] valueBytes)
        {
            // 设置当前节点的值
            try
            {
                int byteCount = GetByteCountByType(node.DataType);
                byte[] itemValue = SubArray(valueBytes, 0, byteCount);
                if (!(NVCommon.DataEndian == Endian.LittleEndian))
                {
                    Array.Reverse(itemValue);
                }

                switch (node.DataType)
                {
                    case EDataType.BYTE:
                        node.ItemValue = itemValue[0].ToString();
                        break;
                    case EDataType.SBYTE:
                        sbyte sbyteValue = (sbyte)itemValue[0];
                        node.ItemValue = sbyteValue.ToString();
                        break;
                    case EDataType.SHORT:
                        short shortValue = BitConverter.ToInt16(itemValue, 0);
                        node.ItemValue = shortValue.ToString();
                        break;
                    case EDataType.USHORT:
                        ushort ushortValue = BitConverter.ToUInt16(itemValue, 0);
                        node.ItemValue = ushortValue.ToString();
                        break;
                    case EDataType.INT:
                        int intValue = BitConverter.ToInt32(itemValue, 0);
                        node.ItemValue = intValue.ToString();
                        break;
                    case EDataType.UINT:
                        uint uintValue = BitConverter.ToUInt32(itemValue, 0);
                        node.ItemValue = uintValue.ToString();
                        break;
                    case EDataType.LONG:
                        long longValue = BitConverter.ToInt64(itemValue, 0);
                        node.ItemValue = longValue.ToString();
                        break;
                    default:
                        // Handle unknown data type
                        break;
                }

                byte[] childrensValue = valueBytes.Skip(byteCount).ToArray();
                // 设置子节点的值
                int startIndex = 0;
                for (int i = 0; i < node.Children.Count && i < valueBytes.Length; i++)
                {
                    int nodeByteCount = GetTotalByteCount(node.Children[i]);
                    byte[] childBytes = SubArray(childrensValue, startIndex, nodeByteCount);
                    SetItemValuesFromBytes(node.Children[i], childBytes);
                    startIndex += nodeByteCount;
                }

                return new BoolQResult(true, "Successed");
            }
            catch (Exception ex)
            {
                return new BoolQResult(false, ex.Message);
            }
        }


        /// <summary>
        /// Saves partition data from a ReadNVDataResult to binary files, both as a whole and divided by sectors.
        /// </summary>
        /// <param name="result">The ReadNVDataResult containing the partition data.</param>
        /// <param name="type">The type of partition data (e.g., "RO", "RW").</param>
        public void SavePartitionData(ReadNVDataResult result, PartitionType type)
        {
            // Check if the input parameters are valid
            if (result == null || result.datas == null || result.datas.Length == 0)
            {
                LogNetHelper.Info("Invalid or empty result object");
                return;
            }

            // Save the complete partition data
            FileUtils.WriteFileBytes(result.datas, $"bin/partitions{type}.bin");

            // Split and save the data by sectors
            for (int count = 0; count < result.sectorCount; count++)
            {
                byte[] data = new byte[result.sectorSize];
                Array.Copy(result.datas, result.sectorSize * count, data, 0, result.sectorSize);
                FileUtils.WriteFileBytes(data, $"bin/partitions{type}{count}.bin");
            }
        }


        /// <summary>
        /// Reads a  sector from the specified partition and reports progress.
        /// </summary>
        /// <param name="callBack">A callback function to report progress.</param>
        /// <param name="partitionType">The type of partition to read (e.g., System, Data).</param>
        /// <returns>A <see cref="ReadNVDataResult"/> object containing the result of the read operation.</returns>
        private ReadNVDataResult ReadSector(Action<string> callBack, PartitionType partitionType)
        {
            //read RO Sector
            Action<double> progressCallBack = (progress) =>
            {
                string result = progress.ToString("0.00");
                callBack?.Invoke($"Read {partitionType} Sectors , Progress = {result}%");
            };

            NVReadParam param = new NVReadParam()
            {
                ItemID = NVCommon.GenerateItemID((byte)partitionType, (byte)Domain.Commmon, 0),
                OperationMode = (byte)NVOperateMode.WholeMode,
                Length = 0
            };

            //读取RO分区
            ReadNVDataResult bResult = new ReadNVDataResult();
            bResult = CmindProtocol.ReadNVDataFromPhone(param, progressCallBack);
            if (bResult.Result == false)
            {
                bResult.Result = false;
                bResult.Msg = $"Read Failed: case <{bResult.Msg}>";
                return bResult;
            }

            return bResult;
        }

        /// <summary>
        /// Backs up NVS data from the board to a file.
        /// </summary>
        /// <param name="callBack">A callback function to report progress.</param>
        /// <param name="filePath">The path where the NVS data will be saved.</param>
        /// <returns>indicating the success or failure of the backup operation.</returns>
        private BoolQResult BackupNVSFromBoard(Action<string> callBack, string filePath)
        {
            // Read the Read-Only (RO) sector.
            try
            {
                ReadNVDataResult roResult = ReadSector(callBack, PartitionType.RO);
                if (!roResult.Result)
                {
                    return new BoolQResult(false, roResult.Msg);
                }

                // Prepare storage parameters for the RO sector.
                StorageParam ROParam = new StorageParam()
                {
                    SAttribute = SectorAttribute.RO,
                    SectorCount = roResult.sectorCount,
                    SectorSize = roResult.sectorSize * 1024,
                    CRC = Crc16Calculator.Calculate(roResult.datas),
                    SectorData = roResult.datas
                };

                // Read the Read-Write (RW) sector.
                ReadNVDataResult rwResult = ReadSector(callBack, PartitionType.RW);
                if (!rwResult.Result)
                {
                    return new BoolQResult(false, rwResult.Msg);
                }

                // Prepare storage parameters for the RW sector.
                StorageParam RWParam = new StorageParam()
                {
                    SAttribute = SectorAttribute.RW,
                    SectorCount = rwResult.sectorCount,
                    SectorSize = rwResult.sectorSize * 1024,
                    CRC = Crc16Calculator.Calculate(rwResult.datas),
                    SectorData = rwResult.datas
                };

                // Create an NVSStorage instance and save data to the specified file.
                NVSStorage nvsSystem = new NVSStorage();
                BoolQResult bResult = nvsSystem.SaveData(filePath, ROParam, RWParam);

                // Return the backup result.
                return bResult;
            }
            catch (Exception ex)
            {
                return new BoolQResult(false, ex.Message);
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="node"></param>
        public BoolQResult ConvertNodeToNVBin(ItemDataNode node, out NVSParam rwParam, out NVSParam roParam)
        {
            if (node == null)
            {
                rwParam = default;
                roParam = default;
                return new BoolQResult(true, "Node is NULL");
            }

            //RW
            List<SectorInfo> sectors = new List<SectorInfo>();
            for (int count = 0; count < NVCommon.RWSectorCount; count++)
            {
                SectorInfo sectorInfo = new SectorInfo();
                sectorInfo.Datas = Enumerable.Repeat((byte)0xFF, NVCommon.RWSectorSize* 1024).ToArray();
                sectorInfo.ATEIndex = (ushort)sectorInfo.Length;
                sectors.Add(sectorInfo);
            }

            List<ushort> listIDs = FindAllItemIDs(node);
            int curIndex = 0;
            SectorInfo curSectorInfo = sectors[curIndex];
          
            foreach (var itemID in listIDs)
            {
                ItemDataNode itemNode = FindItemDataNodeById(node, itemID);
                byte[] datas = ConvertToByte(itemNode);

                NVSAte nvsATE = new NVSAte()
                {
                    id = itemID,
                    len = (ushort)datas.Length,
                    part = 0xFF,
                    crc8 = CRCCalculator.Calculate(datas),
                };

                if ((ATESize + datas.Length) >= curSectorInfo.GetRemainingSpace())
                {
                    curIndex++;
                    curSectorInfo = sectors[curIndex];
                }

                WriteDataToSector(nvsATE, datas, curSectorInfo);
            }


            curIndex = 0;
       
            List<byte[]> sectorData = new List<byte[]>();
            foreach (var sectorInfo in sectors)
            {
                sectorData.Add(sectorInfo.Datas);
#if DEBUG
                FileUtils.WriteFileBytes(sectorInfo.Datas, $"Partition{curIndex}.bin");
                curIndex++;
#endif
            }
            rwParam = new NVSParam()
            {
                SectorCount = NVCommon.RWSectorCount,
                SectorSize = NVCommon.RWSectorSize,
                SectorData = sectorData.SelectMany(data => data).ToArray()
            };

            //RO
            byte[] ROData = Enumerable.Repeat((byte)0xFF, NVCommon.ROSectorCount * NVCommon.ROSectorSize * 1024).ToArray();
            roParam = new NVSParam()
            {
                SectorCount = NVCommon.ROSectorCount,
                SectorSize = NVCommon.ROSectorSize,
                SectorData = ROData
            };


            return new BoolQResult(true, "Convert Successed!");
        }

        /// <summary>
        /// Writes data to a sector, considering alignment requirements.
        /// </summary>
        /// <param name="ate">The NVSAte structure to write.</param>
        /// <param name="data">The data to write.</param>
        /// <param name="sector">The SectorInfo representing the sector to write to.</param>
        /// <returns>A BoolQResult indicating whether the write operation was successful.</returns>
        private BoolQResult WriteDataToSector(NVSAte ate, byte[] data, SectorInfo sector)
        {
            // 检查输入数据是否有效
            if (data == null || sector == null)
            {
                return new BoolQResult(false, "NULL");
            }

            ate.offset = sector.DataIndex;

            // 计算 Data 数据应该对齐到的偏移量
            int alignmentOffset = data.Length % 16 != 0 ? 16 - (data.Length % 16) : 0;

            // 确保偏移地址在有效范围内
            if ((ate.offset + data.Length + ate.len + alignmentOffset) <= sector.Length)
            {
                // 写入 NVSAte 数据
                byte[] ateData = DataConvert.ConvertToByteArray(ate);
                Array.Copy(ateData, 0, sector.Datas, sector.ATEIndex - ATESize, ATESize);

                // 写入 Data 数据
                Array.Copy(data, 0, sector.Datas, ate.offset, data.Length);

                sector.ATEIndex = (ushort)(sector.ATEIndex - ateData.Length);
                sector.DataIndex = (ushort)(sector.DataIndex + data.Length + alignmentOffset);
                return new BoolQResult(true);
            }
            else
            {
                // 处理偏移地址超出范围的情况
                LogNetHelper.Warn("Insufficientlength");
                return new BoolQResult(false, "Insufficientlength");
            }
        }

        /// <summary>
        /// Converts the value of an ItemDataNode and its child nodes to byte array.
        /// </summary>
        /// <param name="node">The ItemDataNode to convert.</param>
        /// <returns>A little endian byte array representing the converted value.</returns>
        public byte[] ConvertToByte(ItemDataNode node)
        {
            List<byte> byteList = new List<byte>();

            // Convert the value of the current node to byte array
            byte[] valueBytes = null;
            switch (node.DataType)
            {
                case EDataType.BYTE:
                    valueBytes = new byte[] { byte.Parse(node.ItemValue) };
                    break;
                case EDataType.SBYTE:
                    sbyte sbyteValue = sbyte.Parse(node.ItemValue);
                    valueBytes = new byte[] { (byte)sbyteValue };
                    break;
                case EDataType.SHORT:
                    short shortValue = short.Parse(node.ItemValue);
                    valueBytes = BitConverter.GetBytes(shortValue);
                    break;
                case EDataType.USHORT:
                    ushort ushortValue = ushort.Parse(node.ItemValue);
                    valueBytes = BitConverter.GetBytes(ushortValue);
                    break;
                case EDataType.INT:
                    int intValue = int.Parse(node.ItemValue);
                    valueBytes = BitConverter.GetBytes(intValue);
                    break;
                case EDataType.UINT:
                    uint uintValue = uint.Parse(node.ItemValue);
                    valueBytes = BitConverter.GetBytes(uintValue);
                    break;
                case EDataType.LONG:
                    long longValue = long.Parse(node.ItemValue);
                    valueBytes = BitConverter.GetBytes(longValue);
                    break;
                default:
                    // Handle unknown data type, you may want to log an error or throw an exception
#if DEBUG
                    //LogNetHelper.Error($"Unknown data type: {node.DataType}");
#endif
                    break;
            }

            if (valueBytes != null)
            {
                if (!(NVCommon.DataEndian == Endian.LittleEndian))
                {
                    Array.Reverse(valueBytes);
                }

                byteList.AddRange(valueBytes);
            }

            // Recursively process child nodes
            foreach (ItemDataNode childNode in node.Children)
            {
                byte[] childBytes = ConvertToByte(childNode);
                byteList.AddRange(childBytes);
            }

            return byteList.ToArray();
        }

        public bool TryParseItemID(string itemID, out ushort parsedID)
        {
            bool isParsed = ushort.TryParse(itemID, out ushort result);

            if (isParsed && result > 0)
            {
                parsedID = result;
                return true;
            }

            parsedID = 0;
            return false;
        }


        /// <summary>
        /// Find All ItemID
        /// </summary>
        /// <param name="node"></param>
        /// <returns></returns>
        public List<ushort> FindAllItemIDs(ItemDataNode node)
        {
            List<ushort> itemIDs = new List<ushort>();

            ushort parsedID = 0;
            // 检查当前节点的 ItemID
            if (TryParseItemID(node.ItemID, out parsedID))
            {
                itemIDs.Add(parsedID);
            }

            // 递归遍历子节点
            foreach (var childNode in node.Children)
            {
                List<ushort> childItemIDs = FindAllItemIDs(childNode);
                itemIDs.AddRange(childItemIDs);
            }

            return itemIDs;
        }


        /// <summary>
        /// Find data based on ID
        /// </summary>
        private ItemDataNode FindItemDataNodeByNodeID(ItemDataNode parentNode, int ID)
        {
            try
            {
                if (parentNode == null)
                    return null;

                if (parentNode.ID == ID)
                    return parentNode;

                foreach (ItemDataNode childNode in parentNode.Children)
                {
                    ItemDataNode foundNode = FindItemDataNodeByNodeID(childNode, ID);
                    if (foundNode != null)
                        return foundNode;
                }

                return null;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                return null;
            }
        }
        /// <summary>
        /// Get New Item ID
        /// </summary>
        /// <param name="node"></param>
        /// <returns></returns>
        public int GetNewItemID(ItemDataNode node)
        {
            List<ItemDataNode> listNode = node.GetAllNodes();
            return (listNode.Max(item => item.ID) + 1);
        }

        /// <summary>
        /// 截取字节数组的前几个字节
        /// </summary>
        /// <param name="sourceArray"></param>
        /// <param name="startIndex"></param>
        /// <param name="length"></param>
        /// <returns></returns>
        public byte[] SubArray(byte[] sourceArray, int startIndex, int length)
        {
            byte[] subArray = new byte[length];
            Array.Copy(sourceArray, startIndex, subArray, 0, length);
            return subArray;
        }
        /// <summary>
        /// Append Item Data Node
        /// </summary>
        /// <param name="parentID"></param>
        /// <param name="childNode"></param>
        /// <returns></returns>
        public BoolQResult AppendItemDataNode(int parentID, ItemDataNode childNode)
        {
            if (nvRamParam.Item == null)
            {
                return new BoolQResult(false, "ItemData is Null");
            }

            // 找到父节点ID为3的节点
            ItemDataNode parentNode = FindItemDataNodeByNodeID(nvRamParam.Item, parentID);
            if (nvRamParam.Item == null)
            {
                return new BoolQResult(false, "Parent node not found");
            }

            parentNode.AddChild(childNode);

            return new BoolQResult(true, "Add child node successfully");
        }
        /// <summary>
        /// File node by ID
        /// </summary>
        /// <param name="parentNode"></param>
        /// <param name="itemId"></param>
        /// <returns></returns>
        public ItemDataNode FindItemDataNodeById(ItemDataNode parentNode, int itemId)
        {
            try
            {
                if (parentNode == null)
                    return null;

                if (parentNode.ItemID == itemId.ToString())
                    return parentNode;

                foreach (ItemDataNode childNode in parentNode.Children)
                {
                    ItemDataNode foundNode = FindItemDataNodeById(childNode, itemId);
                    if (foundNode != null)
                        return foundNode;
                }

                return null;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                return null;
            }
        }

        /// <summary>
        /// Get byte count
        /// </summary>
        /// <param name="EDataType"></param>
        /// <param name="type"></param>
        /// <returns></returns>
        public int GetByteCountByType(EDataType dataType)
        {
            switch (dataType)
            {
                case EDataType.BYTE:
                case EDataType.SBYTE:
                    return sizeof(byte);
                case EDataType.SHORT:
                case EDataType.USHORT:
                    return sizeof(ushort);
                case EDataType.INT:
                case EDataType.UINT:
                    return sizeof(uint);
                case EDataType.LONG:
                    return sizeof(long);
                default:
                    // Handle unknown data type
                    // None
                    return 0;
            }
        }
        /// <summary>
        /// get the length of node
        /// </summary>
        /// <param name="node">data node</param>
        /// <returns>byte count</returns>
        public int GetTotalByteCount(ItemDataNode node)
        {
            int totalByteCount = 0;

            try
            {
                if (node != null)
                {
                    // 计算当前节点的 ItemValue 的字节数
                    int currentNodeByteCount = GetByteCountByType(node.DataType);

                    totalByteCount += currentNodeByteCount;

                    // 遍历当前节点的子节点，并累加子节点的字节数
                    foreach (ItemDataNode childNode in node.Children)
                    {
                        int childNodeByteCount = GetTotalByteCount(childNode);
                        totalByteCount += childNodeByteCount;
                    }
                }

                return totalByteCount;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                return 0;
            }
        }



        /// <summary>
        /// Find Item Node byte ItemName
        /// </summary>
        /// <param name="parentNode"></param>
        /// <param name="ItemIndex">child1Name$child2Name</param>
        /// <returns></returns>
        public ItemDataNode FindItemNodeByItemIndex(ItemDataNode parentNode, string itemIndex)
        {
            string[] itemNames = itemIndex.Split('$');

            // 检查当前节点是否匹配
            if (parentNode.ItemName == itemNames[0])
            {
                // 检查是否已达到最后一个子节点
                if (itemNames.Length == 1)
                {
                    return parentNode;
                }
                else
                {
                    // 递归查找下一级子节点
                    foreach (ItemDataNode childNode in parentNode.Children)
                    {
                        ItemDataNode foundNode = FindItemNodeByItemIndex(childNode, string.Join("$", itemNames, 1, itemNames.Length - 1));
                        if (foundNode != null)
                        {
                            return foundNode;
                        }
                    }
                }
            }
            else
            {
                // 在子节点中查找
                foreach (ItemDataNode childNode in parentNode.Children)
                {
                    ItemDataNode foundNode = FindItemNodeByItemIndex(childNode, itemIndex);
                    if (foundNode != null)
                    {
                        return foundNode;
                    }
                }
            }

            return null; // 未找到匹配的节点
        }

   

        #endregion
    }
}
