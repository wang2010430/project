/***************************************************************************************************
* copyright : 芯迈微半导体（珠海）有限公司
* version   : 1.00
* file      : BitHelper.cs
* date      : 2023/04/19
* author    : haozhe.ni
* brief     : FileHelper
* section Modification History
* - 1.0 : Initial version - haozhe.ni
***************************************************************************************************/

namespace Common
{
    public static class BitHelper
    {
        /// <summary>
        /// 在范围内递增bit值
        /// </summary>
        /// <param name="i_Val">输入值</param>
        /// <param name="startBit">起始bit</param>
        /// <param name="stopBit">结束bit</param>
        /// <param name="o_Val">返回的值</param>
        /// <returns>false,未进位；true,进位</returns>
        public static bool AddBitValue(uint i_Val, int startBit, int stopBit, out uint o_Val)
        {
            bool isCarry = false;
            uint sVal = ClearBitValue_Neg(i_Val, startBit, stopBit);

            sVal += (uint)(1 << startBit);

            if (stopBit == 31 && sVal == 0)
            {
                isCarry = true;
            }
            else
            {
                if ((sVal >> (stopBit + 1)) == 1)
                {
                    isCarry = true;
                    sVal = ClearBitValue_Neg(sVal, startBit, stopBit);
                }
            }

            o_Val = (sVal | ClearBitValue(i_Val, startBit, stopBit));

            return isCarry;
        }

        /// <summary>
        /// 清除范围内的Bit位
        /// </summary>
        /// <param name="val">值</param>
        /// <param name="startBit">开始bit</param>
        /// <param name="stopBit">结束bit</param>
        /// <returns></returns>
        public static uint ClearBitValue(uint val, int startBit, int stopBit)
        {
            for (int i = startBit; i <= stopBit; i++)
            {
                val &= ~(uint)(1 << i);
            }

            return val;
        }

        /// <summary>
        /// 清除范围外的bit位
        /// </summary>
        /// <param name="val">值</param>
        /// <param name="startBit">开始位</param>
        /// <param name="stopBit">结束位</param>
        /// <returns></returns>
        public static uint ClearBitValue_Neg(uint val, int startBit, int stopBit)
        {
            for (int i = 0; i < startBit; i++)
            {
                val &= ~(uint)(1 << i);
            }

            for (int i = stopBit + 1; i < 32; i++)
            {
                val &= ~(uint)(1 << i);
            }

            return val;
        }

        /// <summary>
        /// 合并值的bit
        /// </summary>
        /// <param name="val1">值1，保留范围内的bit</param>
        /// <param name="val2">值2，保留范围外的bit</param>
        /// <param name="startBit">起始bit</param>
        /// <param name="stopBit">结束bit</param>
        /// <returns></returns>
        public static uint MergeBitValue(uint val1, uint val2, int startBit, int stopBit)
        {
            return ClearBitValue_Neg(val1, startBit, stopBit) | ClearBitValue(val2, startBit, stopBit);
        }

        /// <summary>
        /// 将值合并到sourVal的bit位上
        /// </summary>
        /// <param name="sourVal">原值</param>
        /// <param name="startBit">起始位</param>
        /// <param name="stopBit">停止位</param>
        /// <param name="mergeVal">合并值</param>
        /// <returns></returns>
        public static uint MergeValue(uint sourVal,int startBit,int stopBit,uint mergeVal)
        {
            uint val = ClearBitValue(sourVal, startBit, stopBit);
            mergeVal = mergeVal & (GetBitValue(stopBit - startBit + 1));
            val += (mergeVal << startBit);
            return val;
        }

        private static uint GetBitValue(int bitNum)
        {
            uint val = 0;
            for(int i = 0; i <= bitNum; i++)
            {
                val += ((uint)1 << i);
            }

            return val;
        }
    }
}
