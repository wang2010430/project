/***************************************************************************************************
* copyright : 芯迈微半导体（珠海）有限公司
* version   : 1.00
* file      : Sm4Context.cs
* date      : 2023/03/16
* author    : haozhe.ni
* brief     : 
* section Modification History
* - 1.0 : Initial version - haozhe.ni
***************************************************************************************************/

namespace Common
{
    /// <summary>
    /// 国密对象
    /// </summary>
    internal class Sm4Context
    {
        /// <summary>
        /// 运用模式 （1加密，0解密）
        /// </summary>
        public int Mode { get; set; }

        public long[] Sk { get; private set; }

        public bool IsPadding { get; set; }

        public Sm4Context()
        {
            Mode = 1;
            IsPadding = true;
            Sk = new long[32];
        }
    }
}
