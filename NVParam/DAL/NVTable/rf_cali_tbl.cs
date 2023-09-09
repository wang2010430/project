namespace NVParam.DAL.NVTable
{
    public class rf_cali_tbl
    {
        public st_agc_cali_tbl st_agc_cali_tbl { get; set; } = new st_agc_cali_tbl();

        public st_apt_cali_tbl st_apt_cali_tbl { get; set; } = new st_apt_cali_tbl();

        public st_apc_cali_tbl st_apc_cali_tbl { get; set; } = new st_apc_cali_tbl();

        public APT st_lte_apt_tbl_backup { get; set; } = new APT();

        public st_afc_cali_tbl st_afc_cali_tbl { get; set; } = new st_afc_cali_tbl();
    }

    public class st_agc_cali_tbl
    {
        public short[] s16_agc_mid_ch { get; set; } = new short[30];

        public sbyte[,] s8_agc_compensation { get; set; } = new sbyte[40, 3];
    }

    public class st_apt_cali_tbl
    {
        public ushort[] u16_apt_common { get; set; } = new ushort[4];

        public ushort[] u16_apt_section { get; set; } = new ushort[8];

        public ushort[,] u16_apt_start_end { get; set; } = new ushort[3, 2];

        public ushort[] u16_apt_config { get; set; } = new ushort[102];
    }

    public class st_apc_cali_tbl
    {
        public ushort[] u16_apc_mid_ch { get; set; } = new ushort[80];

        public sbyte[] s8_apc_compensation { get; set; } = new sbyte[40];
    }

    public class st_afc_cali_tbl
    {
        public ushort[] u16_afc { get; set; } = new ushort[4];
    }
}
