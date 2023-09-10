/***************************************************************************************************
* copyright : 芯迈微半导体（珠海）有限公司
* version   : 1.00
* file      : NVSParam.cs
* date      : 2023/8/5 11:54:42
* author    : jinlong.wang
* brief     : 
* section Modification History
* - 1.0 : Initial version - jinlong.wang
***************************************************************************************************/

namespace NVParam.DAL
{
    /// <summary>
    /// Sector Info
    /// </summary>
    public class SectorInfo
    {
        public SectorType Type { get; set; } = SectorType.EmptySector;
        public int Length
        {
            get { return Datas != null ? Datas.Length : 0; }
        }
        public byte[] Datas { get; set; }
        public ushort ATEIndex { get; set; }
        public ushort DataIndex { get; set; }

        // 计算剩余空间
        public ushort GetRemainingSpace()
        {
            return (ushort)(DataIndex > ATEIndex ? 0 : (ATEIndex - DataIndex));
        }
    }
}
