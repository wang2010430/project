/***************************************************************************************************
* copyright : CMIND-SEMI
* version   : 1.00
* file      : Class1.cs
* date      : 2023/7/8 17:17:51
* author    : jinlong.wang
* brief     : 
* section Modification History
* - 1.0 : Initial version (2023/7/8 17:17:51) - jinlong.wang
***************************************************************************************************/

using System;
using System.Linq;

namespace NVTool.DAL.Model
{
    public class rf_cali_cfg
    {
        public ushort u16_version { get; set; }
        public ushort u16_flag { get; set; }
        public ST_lte_afc_cali_tbl st_lte_afc_cali_tbl { get; set; }
    }

    public class ST_lte_afc_cali_tbl
    {
        public ushort u16_cdac { get; set; }
        public ushort u16_cafc { get; set; }
        public ushort u16_slope { get; set; }
        public ushort u16_rsvd1 { get; set; }
        public ushort[] u16_rsvd { get; set; }
    }
}


