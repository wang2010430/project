/***************************************************************************************************
* copyright : 芯迈微半导体（珠海）有限公司
* version   : 1.00
* file      : BusinessResult.cs
* date      : 2023/03/16
* author    : haozhe.ni
* brief     : BusinessResult
* section Modification History
* - 1.0 : Initial version - haozhe.ni
***************************************************************************************************/

using System;

namespace Channel
{
    [Serializable]
    public class BusinessResult
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

        public BusinessResult(bool r)
        {
            _result = r;
        }

        public BusinessResult(string m)
        {
            _msg = m;
        }

        public BusinessResult(bool r, string m)
        {
            _result = r;
            _msg = m;
        }

        public void Set(bool r,string msg)
        {
            _result = r;
            Msg = msg;
        }

        public BusinessResult()
            : this(false)
        {
        }

        public static BusinessResult operator &(BusinessResult b1, BusinessResult b2)
        {
            if (!b1._result)
            {
                return b1;
            }

            return !b2._result ? b2 : new BusinessResult(true);
        }

        public static BusinessResult operator |(BusinessResult b1, BusinessResult b2)
        {
            if (b1._result)
            {
                return b1;
            }

            return b2._result ? b2 : new BusinessResult(false, string.Format("{0}并且{1}", b1.Msg, b2.Msg));
        }
    }
}
