/***************************************************************************************************
* copyright : 芯迈微半导体（珠海）有限公司
* version   : 1.00
* file      : BusinessBase.cs
* date      : 2023/04/27
* author    : haozhe.ni
* brief     : 
* section Modification History
* - 1.0 : Initial version- haozhe.ni
***************************************************************************************************/

using Channel;

namespace CmindProtocol.CmindBusiness
{
    public abstract class BusinessBase : CaseBase
    {
        public BusinessBase(ProtocolTask task, ProtocolBase ownerProtocol)
            : base(task, ownerProtocol)
        {
            BusinessTimeOut = 3000;
        }

        public Cmind dlProtocol
        {
            get
            {
                return (Cmind)ownerProtocol;
            }
        }

        public override bool ProcessFrame(FrameBase receivedFrame)
        {
            CmindFrame DLFrame = (CmindFrame)receivedFrame;
            return ProcessDLFrame(DLFrame);
        }

        /// <summary>
        /// 处理完整的RF帧
        /// </summary>
        /// <param name="frame"></param>
        /// <returns></returns>
        public abstract bool ProcessDLFrame(CmindFrame recFrame);

        protected string GetErrorMsg(byte errCode)
        {
            return ((CmindNegReason)errCode).ToString();
        }
    }
}
