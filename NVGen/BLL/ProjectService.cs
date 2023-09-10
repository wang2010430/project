/***************************************************************************************************
* copyright : 芯迈微半导体（珠海）有限公司
* version   : 1.00
* file      : ProjectService.cs
* date      : 2023/9/10 19:37:16
* author    : jinlong.wang
* brief     : 
* section Modification History
* - 1.0 : Initial version - jinlong.wang
***************************************************************************************************/

using Common;
using NVParam.BLL;
using NVParam.DAL;
using NVParam.Helper;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NVGen.BLL
{
    class ProjectService
    {

        #region Narmal Function
        public BoolQResult ConvertFileToBin(string filePath, string binPath)
        {
            try
            {
                if (!File.Exists(filePath))
                    return new BoolQResult(false, "File is not existed");


                NVRamManage nvRamManage = new NVRamManage();
                nvRamManage.NvRamParam = XmlHelper.OpenXML<NVRamParam>(filePath);
                // Initialize NVSParam objects for RW and RO sectors.
                NVSParam rwNVSParam = null;
                NVSParam roNVSParam = null;
                
                // Convert the item node data to NVSParam format.
                nvRamManage.ConvertNodeToNVBin(nvRamManage.NvRamParam.Item, out rwNVSParam, out roNVSParam);

                // Create an NVSStorage instance.
                NVSStorage nvsStorage = new NVSStorage();

                // Save the data to the specified binary file.
                BoolQResult ret = nvsStorage.SaveData(binPath,
                    NVCommon.ConvertParameter(rwNVSParam, SectorAttribute.RW), NVCommon.ConvertParameter(roNVSParam, SectorAttribute.RO));

                return new BoolQResult(true, "Successed");
            }
            catch (Exception ex)
            {
                return new BoolQResult(false, ex.Message);
            }
        }
        #endregion
    }
}
