using Common;
using NVParam.BLL;
using System.Collections.Generic;

namespace NVParam.DAL.NVTable
{
    public class rf_nv_tbl
    {
        public byte[,] u8_agc_rf_ctrl_word { get; set; } = new byte[24, 30];
        public ushort[,] u16_apc_rf_ctrl_word { get; set; } = new ushort[3, 640];
        public APT[] APT { get; set; }

        public st_lte_temperature_compensation st_lte_temperature_compensation { get; set; } = new st_lte_temperature_compensation();

        public st_lte_rx_compensation st_lte_rx_compensation { get; set; } = new st_lte_rx_compensation();

        public st_rf_sys_nv st_rf_sys_nv { get; set; } = new st_rf_sys_nv();

        public st_rf_common_nv st_rf_common_nv { get; set; } = new st_rf_common_nv();

        public rf_nv_tbl()
        {
            APT = new APT[24];
            for (int i = 0; i < APT.Length; i++)
            {
                APT[i] = new APT();
            }

            string filePath = "TX_APC_table.xlsx";
            ExcelReader excelReader = new ExcelReader();
            List<string> columnCData = excelReader.ReadDataFromExcel(filePath);
            for (int i = 0; i < columnCData.Count; i++)
            {
                if (i < 640)
                {
                    ushort value = 0;
                    bool bResult = DataConvert.StringToUshort(columnCData[i], out value);
                    if (bResult)
                    {
                        u16_apc_rf_ctrl_word[0, i] = value;
                    }
                }
            }
        }
    }

    public class APT
    {
        public ushort u16_apt_status { get; set; }

        public ushort[] u16_rsvd { get; set; } = new ushort[3];

        public ushort[] u16_apt_seg_dac { get; set; } = new ushort[8];

        public ushort[] u16_power_seg { get; set; } = new ushort[6];

        public st_apt_mode[] st_apt_mode { get; set; }

        public APT()
        {
            st_apt_mode = new st_apt_mode[3];
            for (int i = 0; i < st_apt_mode.Length; i++)
            {
                st_apt_mode[i] = new st_apt_mode();
            }
        }
    }

    public class st_apt_mode
    {
        public byte u8_power_seg_num { get; set; }

        public byte u8_rsvd { get; set; }

        public st_power_seg[] st_power_seg { get; set; } = new st_power_seg[8];

        public st_apt_mode()
        {
            st_power_seg = new st_power_seg[8];
            for (int count = 0; count < st_power_seg.Length; count++)
            {
                st_power_seg[count] = new st_power_seg();
            }
        }
    }

    public class st_power_seg
    {
        public short u16_start_power { get; set; }

        public short u16_end_power { get; set; }

        public ushort u16_pa_voltage { get; set; }
    }

    public class st_lte_temperature_compensation
    {
        public ushort[] u16_rsvd { get; set; } = new ushort[16];

        public st_lte_tx_compensation[] st_lte_tx_compensation { get; set; }

        //public st_lte_rx_compensation[] st_lte_rx_compensation { get; set; }

        public st_lte_temperature_compensation()
        {
            st_lte_tx_compensation = new st_lte_tx_compensation[24];
            for (int i = 0; i < st_lte_tx_compensation.Length; i++)
            {
                st_lte_tx_compensation[i] = new st_lte_tx_compensation();
            }

            //st_lte_rx_compensation = new st_lte_rx_compensation[24];
            //for (int i = 0; i < st_lte_rx_compensation.Length; i++)
            //{
            //    st_lte_rx_compensation[i] = new st_lte_rx_compensation();
            //}
        }
    }

    public class st_lte_tx_compensation
    {
        public ushort[] u16_tx_compensation_val { get; set; } = new ushort[12];
    }

    public class st_lte_rx_compensation
    {
        public ushort[] u16_rsvd { get; set; } = new ushort[2];
        public ushort[,] u16_tx_compensation_val { get; set; } = new ushort[24, 108];
    }

    public class st_rf_sys_nv
    {
        public ushort[] u16_rsvd { get; set; } = new ushort[1000];
    }

    public class st_rf_common_nv
    {
        public ushort[] u16_rsvd { get; set; } = new ushort[1000];
    }
}

