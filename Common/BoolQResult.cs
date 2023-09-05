/***************************************************************************************************
* copyright : 芯迈微半导体（珠海）有限公司
* version   : 1.00
* file      : BoolQResult.cs
* date      : 2023/03/16
* author    : haozhe.ni
* brief     : 布尔类型定义类
* section Modification History
* - 1.0 : Initial version - haozhe.ni
***************************************************************************************************/

using System;

namespace Common
{
    /// <summary>
    /// 用做返布尔型返回结果＋信息
    /// </summary>
    [Serializable]
    public class BoolQResult
    {
        private bool _result;

        public bool Result
        {
            get
            {
                return _result;
            }

            set
            {
                _result = value;
            }
        }

        private string _msg = "";

        public string Msg
        {
            get
            {
                return _msg;
            }

            set
            {
                _msg = value;
            }
        }

        private string _msg2 = "";

        public string Msg2
        {
            get
            {
                return _msg2;
            }

            set
            {
                _msg2 = value;
            }
        }

        private object tag;

        public object Tag
        {
            get
            {
                return tag;
            }

            set
            {
                tag = value;
            }
        }

        public BoolQResult(bool r)
        {
            _result = r;
        }

        public BoolQResult(string m)
        {
            _msg = m;
        }

        public BoolQResult(bool r, string m)
        {
            _result = r;
            _msg = m;
        }

        public BoolQResult()
            : this(false)
        {
        }

        public static BoolQResult operator &(BoolQResult b1, BoolQResult b2)
        {
            if (!b1._result)
            {
                return b1;
            }

            return !b2._result ? b2 : new BoolQResult(true);
        }

        public static BoolQResult operator |(BoolQResult b1, BoolQResult b2)
        {
            if (b1._result)
            {
                return b1;
            }

            return b2._result ? b2 : new BoolQResult(false, string.Format("{0}并且{1}", b1.Msg, b2.Msg));
        }
    }
}
