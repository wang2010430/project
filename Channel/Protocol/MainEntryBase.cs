/***************************************************************************************************
* copyright : 芯迈微半导体（珠海）有限公司
* version   : 1.00
* file      : MainEntryBase.cs
* date      : 2023/03/16
* author    : haozhe.ni
* brief     : MainEntryBase
* section Modification History
* - 1.0 : Initial version - haozhe.ni
***************************************************************************************************/

using Common;
using System.Collections.Generic;

namespace Channel.Protocol
{
    /// <summary>
    /// 规约程序集中规约的信息汇总
    /// </summary>
    public class MainEntryBase
    {
        protected string className = "";
        protected string protocolDescription = "";
        protected string protocolClass = "未分类";

        /// <summary>
        ///  判断是否是当前协议名
        /// </summary>
        /// <param name="protocolName"></param>
        /// <returns></returns>
        protected bool IsThisProtocol(string protocolName)
        {
            return string.Compare(protocolName, className, true) == 0;
        }

        /// <summary>
        /// 获取协议分类名称
        /// </summary>
        /// <param name="protocolName"></param>
        /// <returns></returns>
        public string GetProtocolClassification(string protocolName)
        {
            if (IsThisProtocol(protocolName))
            {
                if (string.IsNullOrEmpty(protocolClass))
                {
                    return "未分类";
                }

                return protocolClass;
            }

            return "";
        }

        /// <summary>
        /// 取得协议的描述信息
        /// </summary>
        /// <param name="protocolname"></param>
        /// <returns></returns>
        public string GetProtocolDescription(string protocolName)
        {
            if (IsThisProtocol(protocolName))
            {
                return protocolDescription;
            }

            return "";
        }

        /// <summary>
        /// 描述：返回指定的协议是否为虚拟协议。
        /// 参数：协议类名。
        /// 备注：虚拟协议不存在通讯端口，而是自己模拟与远端通讯，可接收数据输入和输出操作结果。
        /// </summary>
        /// <param name="protocolName"></param>
        /// <returns></returns>
        virtual public bool ProtocolIsVirtual(string protocolName)
        {
            return false;
        }

        /// <summary>
        /// 缺省端口参数
        /// </summary>
        /// <returns></returns>
        virtual public string DefaultPortParameter()
        {
            return "";
        }

        /// <summary>
        /// 确定动态库名字
        /// </summary>
        public string[] GetAllProtocolNames()
        {
            string[] protocols = { className };

            return protocols;
        }

        /// <summary>
        /// 协议支持的操作系统
        /// 2016-11-24 增加 zsw
        /// </summary>       
        public virtual List<OsFamily> SupportedOperationSystem()
        {
            return new List<OsFamily>(new OsFamily[] { OsFamily.Windows });
        }

        /// <summary>
        /// 支撑协议配置的系统
        /// </summary>
        /// <returns></returns>
        public virtual List<OsFamily> ProtocolConfigureOperationSystem()
        {
            return new List<OsFamily>(new OsFamily[] { OsFamily.Windows});
        }
    }
}
