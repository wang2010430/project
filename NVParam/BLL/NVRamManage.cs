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

using Channel;
using CmindProtocol;
using CmindProtocol.CmindBusiness;
using CmindProtocol.DLL;
using Common;
using log4net;
using NVParam.DAL;
using NVParam.Helper;
using NVSSystem.BLL;
using NVSSystem.DLL;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace NVParam.BLL
{
    public partial class NVRamManage //: INVRamManage
    {
        #region Attribute
        private NVRamParam nvRamParam = new NVRamParam();
        public NVRamParam NvRamParam { get => nvRamParam; set => nvRamParam = value; }
        public Cmind CmindProtocol { get; set; } = new Cmind();

        private bool IsLittleEndian = true;

        internal bool IsConnected = false;
        public string ProjectFilePath { get; set; }

        #endregion

        #region Constructor
        public NVRamManage()
        {
            //init param
            NvRamParam.Version = "V1.0.0";
            NvRamParam.Project = "NanH";
        }
        #endregion


        #region Function
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
            //backup nvs sys
            BoolQResult ret = BackupNVSFromBoard(progressCallBack, backupFilePath);
            if (!ret.Result)
            {
                return ret;
            }

            // 

            //PartitionType partitionType = PartitionType.RO;
            ////
            //Action<double> callBack = (progress) =>
            //{
            //    string result = progress.ToString("0.00");
            //    string callbackMsg = $"Write {partitionType} Sectors, Progress = {result}%";
            //    progressCallBack?.Invoke(callbackMsg);
            //};

            //NVWriteParam param = new NVWriteParam()
            //{
            //    DownloadMode = (byte)NVDownloadMode.NORMAL_MODE,
            //    OperationMode = (byte)NVOperateMode.WholeMode,
            //};

            ////读取Partition文件
            //byte[] partitionData = FileUtils.ReadFileBytes("partitionsRO.bin");
            ////Read RO Sector
            //param.Length = (uint)partitionData.Length;
            //param.ItemID = NVCommon.GenerateItemID((byte)partitionType, (byte)Domain.Commmon, 0);
            //partitionType = PartitionType.RO;
            //BusinessResult bResult = CmindProtocol.WriteNVItemDataByID(param, callBack, partitionData);
            //if (bResult.Result == false)
            //{
            //    return new BoolQResult(false, $"Write {partitionType.ToString()} Sector Failed: case <{bResult.Msg}>");
            //}

            ////读取Partition文件
            //partitionData = FileUtils.ReadFileBytes("partitions.bin");
            ////Read RW Sector
            //partitionType = PartitionType.RW;
            //param.Length = (uint)partitionData.Length;
            //param.ItemID = NVCommon.GenerateItemID((byte)partitionType, (byte)Domain.Commmon, 0);

            //bResult = CmindProtocol.WriteNVItemDataByID(param, callBack, partitionData);
            //if (bResult.Result == false)
            //{
            //    return new BoolQResult(false, $"Write {partitionType.ToString()} Sector Failed: case <{bResult.Msg}>");
            //}

            return new BoolQResult(true);
        }
        /// <summary>
        /// 从手机中加载image
        /// </summary>
        /// <param name="ItemProgressCallBack"></param>
        /// <returns></returns>
        public BoolQResult LoadImageFromPhone(Action<string> ItemProgressCallBack)
        {
            PartitionType partitionType = PartitionType.RO;
            Action<double> progressCallBack = (progress) =>
            {
                string result = progress.ToString("0.00");
                ItemProgressCallBack?.Invoke($"Read {partitionType} Sectors , Progress = {result}%");
            };

            NVReadParam param = new NVReadParam()
            {
                ItemID = NVCommon.GenerateItemID((byte)partitionType, (byte)Domain.Commmon, 0),
                OperationMode = (byte)NVOperateMode.WholeMode,
                Length = 0
            };

            //读取RO分区
            ReadNVDataResult bResult = null;
            bResult = CmindProtocol.ReadNVDataFromPhone(param, progressCallBack);
            if (bResult.Result == false)
            {
                return new BoolQResult(false, $"Read From Phone Failed: case <{bResult.Msg}>");
            }
            FileUtils.WriteFileBytes(bResult.datas, $"bin/partitionsRO.bin");

            for (int count = 0; count < bResult.sectorCount; count++)
            {
                byte[] data = new byte[bResult.sectorSize];
                Array.Copy(bResult.datas, bResult.sectorSize * count, data, 0, bResult.sectorSize);
                FileUtils.WriteFileBytes(data, $"bin/partitionsRO{count}.bin");
            }

            //读取RW分区
            partitionType = PartitionType.RW;
            param.ItemID = NVCommon.GenerateItemID((byte)partitionType, (byte)Domain.Commmon, 0);
            bResult = CmindProtocol.ReadNVDataFromPhone(param, progressCallBack);
            if (bResult.Result == false)
            {
                return new BoolQResult(false, $"Read From Phone Failed: case <{bResult.Msg}>");
            }

            FileUtils.WriteFileBytes(bResult.datas, $"bin/partitionsRW.bin");
            for (int count = 0; count < bResult.sectorCount; count++)
            {
                byte[] data = new byte[bResult.sectorSize];
                Array.Copy(bResult.datas, bResult.sectorSize * count, data, 0, bResult.sectorSize);
                FileUtils.WriteFileBytes(data, $"bin/partitionsRW{count}.bin");
            }

            NVSParam nvsParam = new NVSParam()
            {
                SectorSize = bResult.sectorSize, // bResult.sectorSize,
                SectorCount = bResult.sectorCount,//bResult.sectorCount,
                SectorData = bResult.datas//bResult.datas,
            };

            NVSSysManage nvsSysManage = new NVSSysManage();
            Dictionary<int, byte[]> listItemData = nvsSysManage.ParseFromData(nvsParam);

            List<ushort> listItemID = FindAllItemIDs(nvRamParam.Item);
            List<ushort> listNVSID = new List<ushort>();

            // Get all keys and print them
            var keys = listItemData.Keys;
            foreach (int key in keys)
            {
                listNVSID.Add((ushort)key);
            }
            var differentValues = listItemID.Except(listNVSID);
            foreach (ushort key in differentValues)
            {
                LogNetHelper.Debug($"Diff Item = {key}");
            }

            foreach (var kvp in listItemData)
            {
                ushort itemId = (ushort)kvp.Key;
                byte[] itemData = kvp.Value;
                if (listItemID.Contains(itemId))
                {
                    ItemDataNode node = FindItemDataNodeById(nvRamParam.Item, itemId);
                    if (node != null)
                    {
                        int nodeByteCount = GetTotalByteCount(node);
                        if (nodeByteCount == itemData.Length)
                            SetItemValuesFromBytes(node, itemData);
                        else
                        {
                            LogNetHelper.Error($"ItemId = {itemId}, Different lengths NVS:{itemData.Length} Project:{itemData.Length}");
                        }
                    }
                    else
                    {
                        LogNetHelper.Error($"ItemId = {itemId}, node is null");
                    }
                }
                else
                {
                    LogNetHelper.Error($"ItemId = {itemId} is null");
                }
            }


            return new BoolQResult(true);
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
                if (!IsLittleEndian)
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
        /// Convert node value to byte[]
        /// </summary>
        /// <param name="node"></param>
        /// <returns></returns>
        public byte[] ConvertToByte(ItemDataNode node)
        {
            List<byte> byteList = new List<byte>();

            // Convert the value of the current node to little endian byte array
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
                    // Handle unknown data type
                    break;
            }

            if (valueBytes != null)
            {
                if (!IsLittleEndian)
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
        #endregion

        #region Common Function

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
        private void ConvertNodeToNVBin(ItemDataNode node)
        {
            //
        }

        #endregion
    }
}
