/***************************************************************************************************
* copyright : 芯迈微半导体（珠海）有限公司
* version   : 1.00
* file      : PortParam.cs
* date      : 2023/03/16
* author    : haozhe.ni
* brief     : PortParam
* section Modification History
* - 1.0 : Initial version - haozhe.ni
***************************************************************************************************/

using Channel.CommPort;
using Common;
using System;
using System.Globalization;
using System.IO.Ports;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Xml;
using System.Xml.Serialization;

namespace Channel
{
    /// <summary>
    /// 端口参数格式化的基类
    /// 实例化该类后直接调用FormatStringParameter来格式化
    /// </summary>
    public class PortXmlParamterFormatterBase : XmlParamterFormatterBase
    {
        public PortXmlParamterFormatterBase()
            : base("Parameters")
        {
            StartFormat();
        }

        public XmlDocument ParamDocument
        {
            get
            {
                return xmlDoc;
            }
        }

        override public string FormatedParameters
        {
            get
            {
                return xmlDoc.InnerXml;
            }
        }
    }

    public class XmlParameterReaderBase
    {
        protected XmlDocument xmlDoc;

        protected XmlElement docElement;

        public XmlParameterReaderBase(string xmlstr)
        {
            xmlDoc = new XmlDocument();
            xmlDoc.LoadXml(xmlstr);
            docElement = xmlDoc.DocumentElement;
        }

        public string ReadXmlParamValue(string paramName)
        {
            if (docElement == null)
            {
                return null;
            }

            XmlElement elementPortType = docElement[paramName];

            return elementPortType != null ? elementPortType.InnerText : null;
        }

        /// <summary>
        /// 从XML串中读取一端口参数值，无此值时返回缺省值,端口参数XML格式如下：
        /// </summary>
        /// <param name="paramName"></param>
        /// <param name="defaultValue"></param>
        /// <returns></returns>
        public string ReadXmlParamValue(string paramName, string defaultValue)
        {
            string v = ReadXmlParamValue(paramName);

            return string.IsNullOrEmpty(v) ? defaultValue : v;
        }

        /// <summary>
        /// 从XML串中读取一端口参数值，无此值时返回null,端口参数XML格式如下：
        /// <Parameters>......</Parameters>
        /// </summary>
        /// <param name="xmpParam"></param>
        /// <param name="paramName"></param>
        /// <returns></returns>
        public static string XMLParamValue(string xmpParam, string paramName)
        {
            XmlDocument xmlDoc = new XmlDocument();

            xmlDoc.LoadXml(xmpParam);

            XmlElement docElement = xmlDoc.DocumentElement;

            if (docElement == null)
            {
                return null;
            }

            XmlElement elementPortType = docElement[paramName];

            return elementPortType != null ? elementPortType.InnerText : null;
        }

        /// <summary>
        /// 从XML串中读取一端口参数值，无此值时返回缺省值,端口参数XML格式如下：
        /// </summary>
        /// <param name="xmpParam"></param>
        /// <param name="paramName"></param>
        /// <param name="defaultValue"></param>
        /// <returns></returns>
        public static string XMLParamValue(string xmpParam, string paramName, string defaultValue)
        {
            string v = XMLParamValue(xmpParam, paramName);

            return string.IsNullOrEmpty(v) ? defaultValue : v;
        }
    }

    /// <summary>
    /// 端口参数基类。
    /// </summary>
    public abstract class PortParamBase
    {
        public abstract int ChannelNo
        {
            get; set;
        }

        public abstract int ChannelName
        {
            get; set;
        }

        /// <summary>
        /// 保证端口在有效范围之内
        /// </summary>
        /// <param name="portValue"></param>
        /// <returns></returns>
        static protected int ValidPort(int portValue)
        {
            if (portValue > 65535)
            {
                return 65535;
            }

            if (portValue < 1 && portValue != 0)
            {
                return 1025;
            }

            return portValue;
        }

        internal const string ElementParent = "Parameters";

        /// <summary>
        /// 获取端口对象。
        /// </summary>
        public abstract ICommPort GetPort();

        public abstract string PortTypeName
        {
            get;
        }

        /// <summary>
        /// 返回参数是否有效。
        /// </summary>
        protected abstract bool ParamIsValid();

        [XmlIgnore]
        public abstract string XmlFormatParam { get; set; }

        public abstract string PortDescription
        {
            get;
        }
    }

    /// <summary>
    /// 串口参数类。
    /// </summary>
    public class ComParam : PortParamBase
    {
        string _portName = string.Format("{0}{1}", BasePortName, (CommonHelper.OperationSystem == OsFamily.Uniux) ? 0 : 1);

        public override int ChannelNo { get; set; }

        public override int ChannelName { get; set; }

        static public string BasePortName
        {
            get
            {
                return CommonHelper.OperationSystem == OsFamily.Uniux ? "/dev/ttyS" : "COM";
            }
            set
            {

            }
        }

        public override string PortTypeName
        {
            get
            {
                return PortParamFormatService.Com;
            }
        }
        override public string PortDescription
        {
            get
            {
                return string.Format("{0},波特率:{1},数据位:{2},校验:{3}，停止位{4}", _portName, BaudRate, DataBits, Parity, StopBits);
            }
        }

        /// <summary>
        /// 获取或设置波特率。
        /// </summary>
        public int BaudRate { get { return _baudRate; } set { _baudRate = value; } }
        int _baudRate = 57600;

        /// <summary>
        /// 获取或设置数据位。
        /// </summary>
        public int DataBits { get { return _dataBits; } set { _dataBits = value; } }
        int _dataBits = 8;

        /// <summary>
        /// 获取或设置奇偶校验。
        /// </summary>
        public Parity Parity { get { return _parity; } set { _parity = value; } }
        Parity _parity = Parity.None;

        /// <summary>
        /// 获取或设置端口名（如COM1、COM2...）。
        /// </summary>
        public string PortName
        {
            get
            {
                return _portName.Contains(BasePortName) ? ResetPortName(_portName) : _portName;
            }

            set
            {
                _portName = value.Contains(BasePortName) ? ResetPortName(value) : value;
            }
        }

        static public string ResetPortName(string oldName)
        {
            string testBase = BasePortName;

            if (CommonHelper.OperationSystem == OsFamily.Uniux)
            {
                testBase = testBase.Substring(0, testBase.Length - 1);
            }

            if (string.IsNullOrEmpty(oldName) || oldName.StartsWith(testBase, StringComparison.OrdinalIgnoreCase))
            {
                return oldName;
            }

            int comNo;

            StringUtils.GetTrailingInteger(oldName, out comNo);

            return string.Format("{0}{1}", BasePortName, comNo);
        }

        /// <summary>
        /// 获取或设置停止位。
        /// </summary>
        public StopBits StopBits { get { return _stopBits; } set { _stopBits = value; } }
        StopBits _stopBits = StopBits.One;

        protected override bool ParamIsValid()
        {
            return true;
        }

        public override ICommPort GetPort()
        {
            return new CommPortCom(this);
        }

        /// <summary>
        /// 停止位的保存值，没有直接使用枚举值转换是为保持与老版本兼容
        /// </summary>
        /// <param name="stopBitUsed"></param>
        /// <returns></returns>
        static public int StopBitsEditValue(StopBits stopBitUsed)
        {
            switch (stopBitUsed)
            {
                case StopBits.One:

                    return 0;

                case StopBits.OnePointFive:

                    return 1;

                case StopBits.Two:

                    return 2;

                case StopBits.None:

                    return 3;

                default:

                    return 0;
            }
        }

        /// <summary>
        /// 停止位的保存值转换成停止位，没有直接使用枚举值转换是为保持与老版本兼容
        /// </summary>
        /// <param name="idx"></param>
        /// <returns></returns>
        static public StopBits EditValueToStopBit(int idx)
        {
            switch (idx)
            {
                case 0:

                    return StopBits.One;

                case 1:

                    return StopBits.OnePointFive;

                case 2:

                    return StopBits.Two;

                case 3:

                    return StopBits.None;

                default:

                    return StopBits.One;
            }
        }

        /// <summary>
        /// 获得或设置通讯端口参数，端口参数为Xml格式。（注：获得数据时返回null表示获得数据失败；设置参数失败会抛出异常，但端口参数不会改变）
        /// 例如读取或设置的内容可以为：<Parameters><ComNo>1</ComNo><BaudRate>57600</BaudRate><DataBit>8</DataBit><Parity>0</Parity><StopBit>0</StopBit></Parameters>
        /// </summary>
        [XmlIgnore]
        public override string XmlFormatParam
        {
            get
            {
                if (!ParamIsValid())
                {
                    return "";
                }

                PortXmlParamterFormatterBase xmlparam = new PortXmlParamterFormatterBase();

                xmlparam.FormatStringParameter("PortName", _portName);

                int comNo;

                StringUtils.GetTrailingInteger(_portName, out comNo);
                xmlparam.FormatStringParameter("ComNo", comNo.ToString(CultureInfo.InvariantCulture));
                xmlparam.FormatStringParameter("BaudRate", BaudRate.ToString(CultureInfo.InvariantCulture));
                xmlparam.FormatStringParameter("DataBit", DataBits.ToString(CultureInfo.InvariantCulture));
                xmlparam.FormatStringParameter("Parity", ((int)Parity).ToString(CultureInfo.InvariantCulture));
                xmlparam.FormatStringParameter("StopBit", StopBitsEditValue(StopBits).ToString(CultureInfo.InvariantCulture));

                return xmlparam.FormatedParameters;
            }

            set
            {
                SerialPort tmpComPort = new SerialPort();
                XmlParameterReaderBase xmlr = new XmlParameterReaderBase(value);

                _portName = xmlr.ReadXmlParamValue("PortName", "");

                if (string.IsNullOrEmpty(_portName))
                {
                    _portName = BasePortName + xmlr.ReadXmlParamValue("ComNo", "1");
                }

                tmpComPort.PortName = _portName;
                tmpComPort.BaudRate = int.Parse(xmlr.ReadXmlParamValue("BaudRate", "57600"));
                tmpComPort.DataBits = int.Parse(xmlr.ReadXmlParamValue("DataBit", "5"));
                tmpComPort.Parity = (Parity)Enum.ToObject(typeof(Parity), byte.Parse(xmlr.ReadXmlParamValue("Parity", "0")));
                tmpComPort.StopBits = EditValueToStopBit(int.Parse(xmlr.ReadXmlParamValue("StopBit", "1")));
                _portName = tmpComPort.PortName;
                BaudRate = tmpComPort.BaudRate;
                DataBits = tmpComPort.DataBits;
                Parity = tmpComPort.Parity;
                StopBits = tmpComPort.StopBits;
            }
        }
    }

    /// <summary>
    /// TCPIP服务器端的端口基类。
    /// 用于TCPClient,TCPServer,UDP
    /// </summary>
    public class NetParamBase : PortParamBase
    {
        protected int formatVer = 2;

        public override int ChannelNo
        {
            get; set;
        }

        public override int ChannelName
        {
            get; set;
        }

        public override ICommPort GetPort()
        {
            return null;
        }

        public override string PortTypeName
        {
            get
            {
                return "NET";
            }
        }

        override public string PortDescription
        {
            get
            {
                return string.Format("本地{0},目标{1}", LocalIPPort, RemoteIPPort);
            }
        }

        protected string localAddressOriginal = "";  //配置的本地地址值

        virtual public string LocalAddress
        {
            get
            {
                return localAddressOriginal;
            }

            set
            {
                localAddressOriginal = value;
            }
        }

        protected string remoteAddressOriginal = "";//配置的远方地址值

        virtual public string RemoteAddress
        {
            get
            {
                return remoteAddressOriginal;
            }

            set
            {
                remoteAddressOriginal = value;
            }
        }

        protected int localPortOriginal;

        public int LocalPort
        {
            get
            {
                return localPortOriginal;
            }

            set
            {
                localPortOriginal = value;
            }
        }

        protected int remotePortOriginal;

        public int RemotePort
        {
            get
            {
                return remotePortOriginal;
            }

            set
            {
                remotePortOriginal = value;
            }
        }

        protected IPEndPoint localIPPort;

        public IPEndPoint LocalIPPort
        {
            get
            {
                return localIPPort ?? (localIPPort = new IPEndPoint(PortParamFormatService.IP127001, localPortOriginal));
            }

            set
            {
                localIPPort = value;
            }
        }

        protected IPEndPoint remoteIPPort;

        public IPEndPoint RemoteIPPort
        {
            get
            {
                return remoteIPPort ?? (remoteIPPort = new IPEndPoint(PortParamFormatService.IP127001, remotePortOriginal));
            }

            set
            {
                remoteIPPort = value;
            }
        }

        /// <summary>
        /// 根据配置的地址，分析出实际用IP
        /// </summary>
        internal void ResolveIP()
        {
            if (!string.IsNullOrEmpty(LocalAddress))
            {
                AddressFamily ipfamily = PortParamFormatService.ParseIPAddressType(remoteAddressOriginal);
                IPAddress addr = PortParamFormatService.ParseIPAddress(localAddressOriginal, (ipfamily == AddressFamily.Unknown) ? AddressFamily.InterNetwork : ipfamily);

                if (addr != null)
                {
                    localIPPort = new IPEndPoint(addr, localPortOriginal);
                }
            }
            else
            {
                localAddressOriginal = "";
            }

            if (!string.IsNullOrEmpty(RemoteAddress))
            {
                IPAddress ipadr = PortParamFormatService.ParseIPAddress(remoteAddressOriginal, (localIPPort == null) ? AddressFamily.InterNetwork : localIPPort.AddressFamily);

                if (ipadr != null)
                {
                    remoteIPPort = new IPEndPoint(ipadr, remotePortOriginal);
                }
            }
            else
            {
                remoteAddressOriginal = "";
            }
        }

        protected override bool ParamIsValid()
        {
            return LocalIPPort != null;
        }

        protected XmlDocument GeneralDocument
        {
            get
            {
                PortXmlParamterFormatterBase xmlparam = new PortXmlParamterFormatterBase();

                xmlparam.FormatStringParameter("LocalIp", localAddressOriginal);
                xmlparam.FormatStringParameter("LocalPort", localPortOriginal.ToString());
                xmlparam.FormatStringParameter("RemoteIp", remoteAddressOriginal);
                xmlparam.FormatStringParameter("RemotePort", remotePortOriginal.ToString());
                xmlparam.FormatStringParameter("FormatVer", "2");

                return xmlparam.ParamDocument;
            }
        }

        /// <summary>
        /// 通用的参数分析
        /// </summary>
        /// <param name="v"></param>
        protected void GenneralParsing(string v)
        {
            XmlParameterReaderBase xmlr = new XmlParameterReaderBase(v);

            formatVer = int.Parse(xmlr.ReadXmlParamValue("FormatVer", "1"));
            LocalAddress = xmlr.ReadXmlParamValue("LocalIp", "");
            RemoteAddress = xmlr.ReadXmlParamValue("RemoteIp", "");
            localPortOriginal = ValidPort(int.Parse(xmlr.ReadXmlParamValue("LocalPort", "5000")));
            remotePortOriginal = ValidPort(int.Parse(xmlr.ReadXmlParamValue("RemotePort", "5001")));
        }

        /// <summary>
        /// 获得或设置通讯端口参数，端口参数为Xml格式。（注：获得数据时返回null表示获得数据失败；设置参数失败会抛出异常，但端口参数不会改变）
        /// 例如读取或设置的内容可以为：<PortParameters><Type>TCPServer/Udp</Type><Parameters><LocalIp>192.100.0.191</LocalIp><LocalPort>0</LocalPort><RemoteIp>192.100.0.191</RemoteIp><RemotePort>6868</RemotePort></Parameters></PortParameters>
        /// </summary>
        public override string XmlFormatParam
        {
            get
            {
                return GeneralDocument.InnerXml;
            }

            set
            {
                GenneralParsing(value);
                ResolveIP();
            }
        }
    }

    /// <summary>
    /// TCP客户端端口参数。
    /// </summary>
    public class TcpClientParam : NetParamBase
    {
        public override string LocalAddress
        {
            get
            {
                return base.LocalAddress;
            }

            set
            {
                if (formatVer < 2 && value.Equals("127.0.0.1", StringComparison.OrdinalIgnoreCase))
                {
                    base.LocalAddress = "";
                }
                else
                {
                    base.LocalAddress = value;
                }
            }
        }

        public override string PortTypeName
        {
            get
            {
                return PortParamFormatService.Tcpclient;
            }
        }

        override public string PortDescription
        {
            get
            {
                return string.Format("服务器地址:{0}", RemoteIPPort);
            }
        }

        public override ICommPort GetPort()
        {
            return ParamIsValid() ? new CommPortTcpClient(this) : null;
        }

        protected override bool ParamIsValid()
        {
            return remoteIPPort != null; ;
        }

        /// <summary>
        /// 获得或设置通讯端口参数，端口参数为Xml格式。（注：获得数据时返回null表示获得数据失败；设置参数失败会抛出异常，但端口参数不会改变）
        /// 例如读取或设置的内容可以为：<PortParameters><Type>TCPServer/Udp</Type><Parameters><LocalIp>192.100.0.191</LocalIp><LocalPort>0</LocalPort><RemoteIp>192.100.0.191</RemoteIp><RemotePort>6868</RemotePort></Parameters></PortParameters>
        /// </summary>
        public override string XmlFormatParam
        {
            get
            {
                return GeneralDocument.InnerXml;
            }

            set
            {
                GenneralParsing(value);
                ResolveIP();
            }
        }
    }

    /// <summary>
    /// TCP服务器端口参数。
    /// </summary>
    public class TcpServerParam : NetParamBase
    {
        public const int ConnectionsAllowed = 500;
        protected int allowedConnections = 1;    // 允许的连接数 2011-3-21新增

        public int AllowedConnections
        {
            get
            {
                if (allowedConnections > 0 && allowedConnections <= ConnectionsAllowed)
                {
                    return allowedConnections;
                }

                return 1;
            }

            set
            {
                if (value > 0 && value <= ConnectionsAllowed)
                {
                    allowedConnections = value;
                }
            }
        }

        /// <summary>
        /// 允许最大连接数标记名
        /// </summary>
        internal const string AllowedConnectionsName = "AllowedConnections";

        override public string PortDescription
        {
            get
            {
                return string.Format("监听端口:{0},允许连接数:{1}", localPortOriginal, allowedConnections);
            }
        }

        public override string LocalAddress
        {
            get
            {
                return base.LocalAddress;
            }

            set
            {
                if (formatVer < 2 && value.Equals("127.0.0.1", StringComparison.OrdinalIgnoreCase))
                {
                    base.LocalAddress = "";
                }
                else
                {
                    base.LocalAddress = value;
                }
            }
        }

        public override string RemoteAddress
        {
            get
            {
                return base.RemoteAddress;
            }

            set
            {
                if (formatVer < 2 && value.Equals("127.0.0.1", StringComparison.OrdinalIgnoreCase))
                {
                    base.RemoteAddress = "";
                }
                else
                {
                    base.RemoteAddress = value;
                }
            }
        }

        public override ICommPort GetPort()
        {
            return ParamIsValid() ? new CommPortTcpServer(this) : null;
        }

        public override string PortTypeName
        {
            get
            {
                return PortParamFormatService.Tcpserver;
            }
        }

        /// <summary>
        /// 获得或设置通讯端口参数，端口参数为Xml格式。（注：获得数据时返回null表示获得数据失败；设置参数失败会抛出异常，但端口参数不会改变）
        /// 例如读取或设置的内容可以为：<PortParameters><Type>TCPServer/Udp</Type><Parameters><LocalIp>192.100.0.191</LocalIp><LocalPort>0</LocalPort><RemoteIp>192.100.0.191</RemoteIp><RemotePort>6868</RemotePort></Parameters></PortParameters>
        /// </summary>
        public override string XmlFormatParam
        {
            get
            {
                XmlDocument xmlDoc = GeneralDocument;
                XmlElement sub5 = xmlDoc.CreateElement(AllowedConnectionsName);

                sub5.InnerText = allowedConnections.ToString(CultureInfo.InvariantCulture);

                if (xmlDoc.DocumentElement != null)
                {
                    xmlDoc.DocumentElement.AppendChild(sub5);
                }

                return xmlDoc.InnerXml;
            }

            set
            {
                GenneralParsing(value);

                string tmp = XmlParameterReaderBase.XMLParamValue(value, AllowedConnectionsName);

                if (!string.IsNullOrEmpty(tmp))
                {
                    AllowedConnections = int.Parse(tmp);
                }

                ResolveIP();
            }
        }
    }

    /// <summary>
    /// 该类并不实际使用
    /// </summary>
    public class TcpSocketParam : PortParamBase
    {
        public override int ChannelNo
        {
            get; set;
        }

        public override int ChannelName
        {
            get; set;
        }

        public override string PortTypeName
        {
            get
            {
                return "SOCKET";
            }
        }

        public override string PortDescription
        {
            get
            {
                return "SOCKET";
            }
        }

        public override ICommPort GetPort()
        {
            return null;
        }

        protected override bool ParamIsValid()
        {
            return true;
        }

        public override string XmlFormatParam
        {
            get
            {
                return "";
            }

            set
            {

            }
        }
    }

    /// <summary>
    /// 端口参数的特殊格式的服务类。
    /// 它提供由具体格式的端口参数获取端口对象，或由端口获取具体端口参数的服务。
    /// </summary>
    public static class PortParamFormatService
    {
        const string XmlFormatRootElementName = "PortParameters";
        const string XmlFormatPortType = "Type";
        const string XmlFormatPortParam = PortParamBase.ElementParent;

        internal const string Com = "COM";
        internal const string Tcpclient = "TCPCLIENT";
        internal const string Tcpserver = "TCPSERVER";

        /// <summary>
        /// 从端口参数XML串中读取端口类型名，端口参数XML串格式如下：
        /// <PortParameters><Type>TCPCLIENT</Type><Parameters>......</Parameters></PortParameters>
        /// </summary>
        /// <param name="xmlFormatParam"></param>
        /// <returns></returns>
        public static string CommPortTypeName(string xmlFormatParam)
        {
            XmlDocument xmlDoc = new XmlDocument();

            xmlDoc.LoadXml(xmlFormatParam);

            XmlElement docElement = xmlDoc.DocumentElement;

            if (null == docElement)
            {
                return null;
            }

            if (String.Compare(docElement.Name, XmlFormatRootElementName, StringComparison.OrdinalIgnoreCase) != 0)
            {
                return null;
            }

            XmlElement elementPortType = docElement[XmlFormatPortType];

            if (elementPortType == null)
            {
                return null;
            }

            string portType = elementPortType.InnerText;

            return !string.IsNullOrEmpty(portType) ? portType.ToUpper() : null;
        }

        public static string CommPortParams(string xmlFormatParam)
        {
            XmlDocument xmlDoc = new XmlDocument();

            xmlDoc.LoadXml(xmlFormatParam);

            XmlElement docElement = xmlDoc.DocumentElement;

            if (null == docElement)
            {
                return null;
            }

            if (String.Compare(docElement.Name, XmlFormatRootElementName, StringComparison.OrdinalIgnoreCase) != 0)
            {
                return null;
            }

            XmlElement elementPortParameters = docElement[XmlFormatPortParam];

            return elementPortParameters != null ? elementPortParameters.OuterXml : null;
        }

        /// <summary>
        /// 根据Xml格式端口参数返回具体的端口对象。若端口参数格式错误或配置错误都将返回空。
        /// 例如打开TcpClient端口的参数可以是：<PortParameters><Type>TCPCLIENT</Type><Parameters><LocalIp>192.100.0.191</LocalIp><LocalPort>0</LocalPort><RemoteIp>192.100.0.191</RemoteIp><RemotePort>6868</RemotePort></Parameters></PortParameters>
        /// </summary>
        /// <param name="xmlFormatParam">Xml格式端口参数</param>
        public static ICommPort GetCommPort(string xmlFormatParam)
        {
            XmlDocument xmlDoc = new XmlDocument();

            xmlDoc.LoadXml(xmlFormatParam);

            XmlElement docElement = xmlDoc.DocumentElement;

            if (null == docElement)
            {
                return null;
            }

            if (String.Compare(docElement.Name, XmlFormatRootElementName, StringComparison.OrdinalIgnoreCase) != 0)
            {
                return null;
            }

            XmlElement elementPortType = docElement[XmlFormatPortType];
            XmlElement elementPortParameters = docElement[XmlFormatPortParam];

            if (elementPortType == null || elementPortParameters == null)
            {
                return null;
            }

            PortParamBase param = null;

            string portType = elementPortType.InnerText;

            if (!string.IsNullOrEmpty(portType))
            {
                switch (portType.ToUpper())
                {
                    case Com:

                        param = new ComParam();
                        break;

                    case Tcpclient:

                        param = new TcpClientParam();
                        break;

                    case Tcpserver:

                        param = new TcpServerParam();
                        break;
                }
            }

            if (param == null)
            {
                return null;
            }

            param.XmlFormatParam = elementPortParameters.OuterXml;

            return param.GetPort();
        }

        /// <summary>
        /// 获取端口的Xml格式字符串的端口参数。
        /// </summary>
        /// <param name="port">端口对象</param>
        public static string GetXmlFormatParamOfPort(ICommPort port)
        {
            if (port == null)
            {
                return null;
            }

            string portType = null;

            PortParamBase param = port.PortParam;

            if (param is ComParam)
            {
                portType = Com;
            }
            else if (param is TcpClientParam)
            {
                portType = Tcpclient;
            }
            else if (param is TcpServerParam)
            {
                portType = Tcpserver;
            }

            StringBuilder resultXml = new StringBuilder();

            resultXml.Append(string.Format("<{0}>", XmlFormatRootElementName));
            resultXml.Append(string.Format("<{0}>", XmlFormatPortType));
            resultXml.Append(portType);
            resultXml.Append(string.Format("</{0}>", XmlFormatPortType));

            if (param != null)
            {
                resultXml.Append(param.XmlFormatParam);
            }

            resultXml.Append(string.Format("</{0}>", XmlFormatRootElementName));

            return resultXml.ToString();
        }

        static public IPAddress IP127001 = IPAddress.Parse("127.0.0.1");
        static public IPAddress IPV6127001 = IPAddress.Parse("::1");

        /// <summary>
        /// 获取一IP地址，如果是根据机器名获取则需指明优先考虑IPV4还是IPV6
        /// </summary>
        /// <param name="ipAddr"></param>
        /// <param name="prefer"></param>
        /// <returns></returns>
        static public IPAddress ParseIPAddress(string ipAddr, AddressFamily prefer)
        {
            if (string.IsNullOrEmpty(ipAddr))
            {
                return null;
            }

            IPAddress ip;

            if (IPAddress.TryParse(ipAddr, out ip))
            {
                return ip;
            }

            try
            {
                IPAddress[] addrs = Dns.GetHostAddresses(ipAddr);

                if (addrs != null && addrs.Length > 0)
                {
                    IPAddress usedAddr = addrs[0];

                    foreach (IPAddress psip in addrs)
                    {
                        if (psip.AddressFamily == prefer)
                        {
                            usedAddr = psip; ;
                            break;
                        }
                    }

                    UtMessageBase.ShowOneMessage(string.Format("解析{0}得到的地址为:{1}", ipAddr, usedAddr.ToString()), PopupMessageType.Info);
                    return usedAddr;
                }
            }
            catch
            {
                UtMessageBase.ShowOneMessage(string.Format("解析地址:{0}失败！", ipAddr), PopupMessageType.Error);
            }

            return null;
        }

        /// <summary>
        /// 返回IP地址的类型
        /// </summary>
        /// <param name="addr"></param>
        /// <returns></returns>
        static public AddressFamily ParseIPAddressType(string addr)
        {
            if (string.IsNullOrEmpty(addr))
            {
                return AddressFamily.Unknown;
            }

            IPAddress ip;

            return IPAddress.TryParse(addr, out ip) ? ip.AddressFamily : AddressFamily.Unknown;
        }

        /// <summary>
        /// 判断两个IP地址是否相同（IPV4或IPV6
        /// </summary>
        /// <param name="adr1"></param>
        /// <param name="adr2"></param>
        /// <returns></returns>
        static public bool IsIPBytesMatch(IPAddress adr1, IPAddress adr2)
        {
            // IPV6不能使用adr1.Equal来判断，目前该函数的判别结果是错误的
            if (adr1 == null || adr2 == null)
            {
                return false;
            }

            byte[] b1 = adr1.GetAddressBytes();
            byte[] b2 = adr2.GetAddressBytes();

            if (b1.Length != b2.Length)
            {
                return false;
            }

            for (int i = 0; i < b1.Length; i++)
            {
                if (b1[i] != b2[i])
                {
                    return false;
                }
            }

            return true;
        }
    }
}