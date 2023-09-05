namespace NVParam.DAL
{
    public class modem_common
    {
        public ushort[,] modem_capability { get; set; } = new ushort[2, 16];
        public ushort[] active_rat_map { get; set; } = new ushort[4];
        public ushort[] ant_rf_ch_map { get; set; } = new ushort[4];
        public ushort[] sim_map { get; set; } = new ushort[4];
        public short[,] sig_level_lte { get; set; } = new short[2, 100];
    }
}

