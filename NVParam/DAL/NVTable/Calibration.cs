/***************************************************************************************************
* copyright : 芯迈微半导体（珠海）有限公司
* version   : 1.00
* file      : Class1.cs
* date      : 2023/7/9 14:35:08
* author    : jinlong.wang
* brief     : 
* section Modification History
* - 1.0 : Initial version - jinlong.wang
***************************************************************************************************/

namespace NVParam.DAL.NVTable
{
    public class Calibration
    {
        public rf_cali_cfg rf_cali_cfg { get; set; } = new rf_cali_cfg();
        public rf_cali_tbl[] rf_cali_tbl { get; set; } = new rf_cali_tbl[24];
        public rf_nv_tbl rf_nv_tbl { get; set; } = new rf_nv_tbl();


        public Calibration()
        {
            rf_cali_tbl = new rf_cali_tbl[24];
            for (int i = 0; i < rf_cali_tbl.Length; i++)
            {
                rf_cali_tbl[i] = new rf_cali_tbl();
            }
        }
    }
}
