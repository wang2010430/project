/***************************************************************************************************
* copyright : 芯迈微半导体（珠海）有限公司
* version   : 1.00
* file      : CommPortShare.cs
* date      : 2023/03/16
* author    : haozhe.ni
* brief     : CommPortShare
* section Modification History
* - 1.0 : Initial version - haozhe.ni
***************************************************************************************************/

namespace Channel
{
    /// <summary>
    /// 共享端口类
    /// </summary>
    public class CommPortShare : CommPortBase
    {
        private readonly PortParamBase _portParam = null;

        #region ICommPort 成员

        public override PortStatus PortState
        {
            get
            {
                return IsConnected ? PortStatus.Connected : PortStatus.Closed;
            }
        }

        public override bool IsConnected
        {
            get
            {
                return true;
            }
        }

        public override bool DisposedAbnormally
        {
            get
            {
                return true;
            }
        }

        public override PortParamBase PortParam
        {
            get
            {
                return _portParam;
            }
        }

        public override bool Open()
        {
            return true;
        }

        public override void Close()
        {
           
        }

        public override bool Send(byte[] data)
        {
            return true;
        }

        public override bool Send(byte[] buffer, int offset, int size)
        {
            return true;
        }
        #endregion
    }
}
