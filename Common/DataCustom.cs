/***************************************************************************************************
* copyright : 芯迈微半导体（珠海）有限公司
* version   : 1.00
* file      : DataCustom.cs
* date      : 2023/06/02
* author    : haozhe.ni
* brief     : 自定义的基础数据，用于UI界面数据源的绑定
* section Modification History
* - 1.0 : Initial version - haozhe.ni
***************************************************************************************************/

using System;
using System.Text;
using System.Text.RegularExpressions;

namespace Common
{
    [Serializable]
    public class Hex_e
    {
        public Hex_e() { }

        public Hex_e(uint value = 0)
        {
            _valueString = string.Format("0x{0:X8}", value);
            Value = value;
        }

        public string _valueString = "0";

        public uint Value { get; set; } = 0;

        public string Data
        {
            get
            {
                return _valueString;
            }
            set
            {
                if (string.IsNullOrEmpty(value))
                {
                    _valueString = "";
                    Value = 0;
                    return;
                }

                if (DataConvert.StringToUint(value, out uint val))
                {
                    _valueString = value;
                    Value = val;
                }
            }
        }
    }

    [Serializable]
    public class int_e
    {
        public int_e() { }

        public int_e(int value)
        {
            _valueString = value.ToString();
            Value = value;
        }

        public string _valueString = "0";

        public int Value { get; set; } = 0;

        public string Data
        {
            get
            {
                return _valueString;
            }
            set
            {
                if (string.IsNullOrEmpty(value))
                {
                    _valueString = "";
                    Value = 0;
                }
                else if (int.TryParse(value, out int val))
                {
                    _valueString = value;
                    Value = val;
                }
            }
        }
    }

    [Serializable]
    public class uint_e
    {
        public uint_e(uint value = 0)
        {
            _valueString = value.ToString();
            Value = value;
        }

        public string _valueString = "0";

        public uint Value { get; set; } = 0;

        public string Data
        {
            get
            {
                return _valueString;
            }
            set
            {
                if (string.IsNullOrEmpty(value))
                {
                    _valueString = "";
                    Value = 0;
                }
                else if (uint.TryParse(value, out uint val))
                {
                    _valueString = value;
                    Value = val;
                }
            }
        }
    }

    [Serializable]
    public class uint16_e
    {
        public uint16_e(ushort value = 0)
        {
            _valueString = value.ToString();
            Value = value;
        }

        public string _valueString = "0";

        public uint Value { get; set; } = 0;

        public string Data
        {
            get
            {
                return _valueString;
            }
            set
            {
                if (string.IsNullOrEmpty(value))
                {
                    _valueString = "";
                    Value = 0;
                }
                else if (uint.TryParse(value, out uint val))
                {
                    _valueString = value;
                    Value = val;
                }
            }
        }
    }

    [Serializable]
    public class float_e
    {
        public float_e() { }

        public float_e(float value)
        {
            _valueString = value.ToString();
            Value = value;
        }

        public string _valueString;

        public float Value { get; set; } = 0;

        public string Data
        {
            get
            {
                return _valueString;
            }
            set
            {
                if (string.IsNullOrEmpty(value))
                {
                    _valueString = "";
                    Value = 0;
                }
                else if (float.TryParse(value, out float val))
                {
                    _valueString = value;
                    Value = val;
                }
            }
        }
    }

    [Serializable]
    public class double_e
    {
        public double_e() { }

        public double_e(double value)
        {
            _valueString = value.ToString();
            Value = value;
        }

        public string _valueString;

        public double Value { get; set; } = 0;

        public string Data
        {
            get
            {
                return _valueString;
            }
            set
            {
                if (string.IsNullOrEmpty(value))
                {
                    _valueString = "";
                    Value = 0;
                }
                else if (double.TryParse(value, out double val))
                {
                    _valueString = value;
                    Value = val;
                }
            }
        }
    }

    public class string_e
    {
        string _string = "";

        int _len = 100;

        public string_e() { }

        public string_e(string str, int len)
        {
            _len = len;
            Data = str;
        }

        public string Data
        {
            get
            {
                return _string;
            }
            set
            {
                byte[] bs = Encoding.ASCII.GetBytes(value);
                Array.Resize(ref bs, _len);
                bs[bs.Length - 1] = 0;
                Value = bs;
                _string = Encoding.ASCII.GetString(Value);
            }
        }

        public byte[] Value { get; set; } = new byte[100];
    }
}
