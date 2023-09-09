/***************************************************************************************************
* copyright : CMIND-SEMI
* version   : 1.00
* file      : ProjectManage.cs
* date      : 2023/8/1 20:11:34
* author    : jinlong.wang
* brief     : 
* section Modification History
* - 1.0 : Initial version - jinlong.wang
***************************************************************************************************/

using CmindProtocol.DLL;
using NVParam.BLL;
using NVParam.DAL;
using NVParam.DAL.NVTable;
using NVParam.Helper;

namespace NVTool.BLL
{
    class ProjectManage
    {
        /// <summary>
        /// Create Project File
        /// </summary>
        /// <returns></returns>
        static public (bool, ItemDataNode) CreateProjectFile()
        {
            NVRamManage nvRamManage = new NVRamManage();
            NV_Project NV_Porject = new NV_Project();
            int id = 1;
            ItemDataNode rootNode = ConverterClassToNode.ConvertToNode(NV_Porject, ref id, string.Empty, 0);

            ItemDataNode rfCaliCfg = nvRamManage.FindItemNodeByItemIndex(rootNode, "Calibration$rf_cali_cfg");
            if (rfCaliCfg != null)
            {
                rfCaliCfg.ItemID = NVCommon.GenerateItemID((byte)PartitionType.RW, (byte)Domain.Calibration, 0x07).ToString();
            }

            string index = string.Empty;
            ItemDataNode tempNode = null;
            for (int i = 0; i < 24; i++)
            {
                index = $"rf_cali_tbl[{i}]$st_agc_cali_tbl";
                tempNode = nvRamManage.FindItemNodeByItemIndex(rootNode, index);
                if (tempNode != null)
                {
                    int tempId = 1 + i * 8;
                    tempNode.ItemID = NVCommon.GenerateItemID((byte)PartitionType.RW, (byte)Domain.Calibration, (ushort)tempId).ToString();
                }

                index = $"rf_cali_tbl[{i}]$st_apt_cali_tbl";
                tempNode = nvRamManage.FindItemNodeByItemIndex(rootNode, index);
                if (tempNode != null)
                {
                    int tempId = 2 + i * 8;
                    tempNode.ItemID = NVCommon.GenerateItemID((byte)PartitionType.RW, (byte)Domain.Calibration, (ushort)tempId).ToString();
                }

                index = $"rf_cali_tbl[{i}]$st_apc_cali_tbl";
                tempNode = nvRamManage.FindItemNodeByItemIndex(rootNode, index);
                if (tempNode != null)
                {
                    int tempId = 3 + i * 8;
                    tempNode.ItemID = NVCommon.GenerateItemID((byte)PartitionType.RW, (byte)Domain.Calibration, (ushort)tempId).ToString();
                }

                index = $"rf_cali_tbl[{i}]$st_lte_apt_tbl_backup";
                tempNode = nvRamManage.FindItemNodeByItemIndex(rootNode, index);
                if (tempNode != null)
                {
                    int tempId = 4 + i * 8;
                    tempNode.ItemID = NVCommon.GenerateItemID((byte)PartitionType.RW, (byte)Domain.Calibration, (ushort)tempId).ToString();
                }

                index = $"rf_cali_tbl[{i}]$st_afc_cali_tbl";
                tempNode = nvRamManage.FindItemNodeByItemIndex(rootNode, index);
                if (tempNode != null)
                {
                    int tempId = 5 + i * 8;
                    tempNode.ItemID = NVCommon.GenerateItemID((byte)PartitionType.RW, (byte)Domain.Calibration, (ushort)tempId).ToString();
                }

                index = $"rf_nv_tbl$APT[{i}]";
                tempNode = nvRamManage.FindItemNodeByItemIndex(rootNode, index);
                if (tempNode != null)
                {
                    int tempId = 0x180 + i;
                    tempNode.ItemID = NVCommon.GenerateItemID((byte)PartitionType.RW, (byte)Domain.Calibration, (ushort)tempId).ToString();
                }
            }

            index = $"rf_nv_tbl$u8_agc_rf_ctrl_word";
            tempNode = nvRamManage.FindItemNodeByItemIndex(rootNode, index);
            if (tempNode != null)
            {
                ushort tempId = 0x120;
                tempNode.ItemID = NVCommon.GenerateItemID((byte)PartitionType.RW, (byte)Domain.Calibration, tempId).ToString();
            }

            index = $"rf_nv_tbl$u16_apc_rf_ctrl_word";
            tempNode = nvRamManage.FindItemNodeByItemIndex(rootNode, index);
            if (tempNode != null)
            {
                ushort tempId = 0x140;
                tempNode.ItemID = NVCommon.GenerateItemID((byte)PartitionType.RW, (byte)Domain.Calibration, tempId).ToString();
            }

            index = $"rf_nv_tbl$st_lte_temperature_compensation";
            tempNode = nvRamManage.FindItemNodeByItemIndex(rootNode, index);
            if (tempNode != null)
            {
                ushort tempId = 0x1A0;
                tempNode.ItemID = NVCommon.GenerateItemID((byte)PartitionType.RW, (byte)Domain.Calibration, tempId).ToString();
            }

            index = $"rf_nv_tbl$st_lte_rx_compensation";
            tempNode = nvRamManage.FindItemNodeByItemIndex(rootNode, index);
            if (tempNode != null)
            {
                ushort tempId = 0x200;
                tempNode.ItemID = NVCommon.GenerateItemID((byte)PartitionType.RW, (byte)Domain.Calibration, tempId).ToString();
            }

            index = $"rf_nv_tbl$st_rf_sys_nv";
            tempNode = nvRamManage.FindItemNodeByItemIndex(rootNode, index);
            if (tempNode != null)
            {
                ushort tempId = 0x210;
                tempNode.ItemID = NVCommon.GenerateItemID((byte)PartitionType.RW, (byte)Domain.Calibration, tempId).ToString();
            }

            index = $"rf_nv_tbl$st_rf_common_nv";
            tempNode = nvRamManage.FindItemNodeByItemIndex(rootNode, index);
            if (tempNode != null)
            {
                ushort tempId = 0x220;
                tempNode.ItemID = NVCommon.GenerateItemID((byte)PartitionType.RW, (byte)Domain.Calibration, tempId).ToString();
            }

            index = $"phy_cfg";
            tempNode = nvRamManage.FindItemNodeByItemIndex(rootNode, index);
            if (tempNode != null)
            {
                ushort tempId = 0x002;
                tempNode.ItemID = NVCommon.GenerateItemID((byte)PartitionType.RW, (byte)Domain.PHY, tempId).ToString();
            }

            index = $"modem_common";
            tempNode = nvRamManage.FindItemNodeByItemIndex(rootNode, index);
            if (tempNode != null)
            {
                ushort tempId = 0x002;
                tempNode.ItemID = NVCommon.GenerateItemID((byte)PartitionType.RW, (byte)Domain.Commmon, tempId).ToString();
            }

            return (true, rootNode);
        }

    }
}
