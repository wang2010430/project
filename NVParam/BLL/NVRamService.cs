/***************************************************************************************************
* copyright : CMIND-SEMI
* version   : 1.00
* file      : NVRamService.cs
* date      : 2023/9/8 14:08:13
* author    : jinlong.wang
* brief     : 
* section Modification History
* - 1.0 : Initial version - jinlong.wang
***************************************************************************************************/
using Channel;
using CmindProtocol.CmindBusiness;
using CmindProtocol.DLL;
using Common;
using NVParam.DAL;
using System;
using System.IO;

namespace NVParam.BLL
{
    public partial class NVRamManage
    {

        #region Communitation

        /// <summary>
        /// Disconnects the communication and stops protocol operations.
        /// </summary>
        /// <returns>
        /// indicating the success or failure of the disconnection.
        /// </returns>
        public BoolQResult DisconnectCommunication()
        {
            try
            {
                // Stop protocol operations and disconnect.
                bool isSuccessful = CmindProtocol.StopWork(true);
                if (isSuccessful)
                {
                    return new BoolQResult(true, "Disconnect succeeded.");
                }
                return new BoolQResult(false, "Disconnect failed.");
            }
            catch (Exception ex)
            {
                // Handle exceptions and provide an error message.
                return new BoolQResult(false, ex.Message);
            }
        }

        /// <summary>
        /// Connects to the communication port using the provided port parameters.
        /// </summary>
        /// <param name="portParam">The communication port configuration.</param>
        /// <returns>
        /// indicating the success or failure of the connection.
        /// </returns>
        public BoolQResult ConnectCommunication(ICommPort portParam)
        {
            try
            {
                // Set the communication port configuration.
                CmindProtocol.Port = portParam;

                // Start protocol operations and establish a connection.
                bool isSuccessful = CmindProtocol.StartWork();
                if (isSuccessful)
                {
                    return new BoolQResult(true, "Connection succeeded!");
                }
                return new BoolQResult(false, "Connection failed.");
            }
            catch (IOException ex)
            {
                // Handle I/O exceptions and provide an error message.
                return new BoolQResult(false, ex.Message);
            }
        }

        #endregion

        #region Read and Write Param

        /// </summary>
        /// Write NV Param
        /// </summary>
        /// <param name="ItemID"></param>
        /// <param name="node"></param>
        /// <returns></returns>
        public BoolQResult WriteNVParam(ushort ItemID, ItemDataNode node)
        {
            try
            {
                if (node == null)
                {
                    return new BoolQResult(false, "Node is Null");
                }

                NVWriteParam param = new NVWriteParam()
                {
                    ItemID = ItemID,
                    DownloadMode = (byte)NVDownloadMode.NORMAL_MODE,
                    OperationMode = (byte)NVOperateMode.OneByOneMode,
                    Length = (uint)GetTotalByteCount(node),
                };
                Action<double> progressCallBack = (progress) =>
                {
                    //Console.WriteLine($"ItemID = {ItemID} Progress: {progress}%");
                };

                byte[] sendData = ConvertToByte(node);
                if (param.Length == sendData.Length)
                {
                    BusinessResult bResult = CmindProtocol.WriteNVItemDataByID(param, progressCallBack, sendData);
                    if (bResult.Result == false)
                    {
                        return new BoolQResult(false, $"Write ItemID = {ItemID} Failed: case <{bResult.Msg}>");
                    }
                }
                else
                {
                    return new BoolQResult(false, $"Length is wrong, param.Length = {param.Length} sendData.Length = {sendData.Length}");
                }
                return new BoolQResult(true, string.Empty);
            }
            catch (Exception ex)
            {
                return new BoolQResult(false, ex.Message);
            }
        }

        /// <summary>
        /// Read NV Param
        /// </summary>
        /// <param name="ItemID"></param>
        /// <param name="node"></param>
        /// <returns></returns>
        public BoolQResult ReadNVParam(ushort ItemID, ItemDataNode node)
        {
            try
            {
                if (node == null)
                {
                    return new BoolQResult(false, "Node is Null");
                }
                node.ItemID = ItemID.ToString();
                NVReadParam param = new NVReadParam()
                {
                    OperationMode = (byte)NVOperateMode.OneByOneMode,
                    ItemID = ItemID,
                    Length = (ushort)GetTotalByteCount(node)
                };

                Action<double> progressCallBack = (progress) =>
                {
                    // 在这里处理进度更新的逻辑
                    //Console.WriteLine($"Progress: {progress}%");
                };

                ReadNVDataResult bResult = CmindProtocol.ReadNVItemDataByID(param, progressCallBack, param.Length);
                if (bResult.Result == false)
                {
                    return new BoolQResult(false, $"Read ItemID = {ItemID} Failed: case <{bResult.Msg}>");
                }
                //assignment
                BoolQResult setResult = SetItemValuesFromBytes(node, bResult.datas);
                if (setResult.Result == false)
                {
                    return setResult;
                }

                return new BoolQResult(true, string.Empty);
            }
            catch (Exception ex)
            {
                return new BoolQResult(false, ex.Message);
            }
        }
        #endregion
    }
}
