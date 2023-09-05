/***************************************************************************************************
* copyright : 芯迈微半导体（珠海）有限公司
* version   : 1.00
* file      : HandshakeBussiness.cs
* date      : 2023/05/05
* author    : haozhe.ni
* brief     : 
* section Modification History
* - 1.0 : Initial version- haozhe.ni
***************************************************************************************************/

using Channel;
using Common;
using System.Threading;

namespace CmindProtocol.CmindBusiness
{
    class HandshakeBussiness : BusinessBase
    {
        Thread thread;
        CmindFrame frame;

        public HandshakeBussiness(ProtocolTask task, ProtocolBase ownerProtocol)
            : base(task, ownerProtocol)
        {
            frame = new CmindFrame();
            frame.SetHandFrame();

            if (!dlProtocol.ProtocolIsRunning)
            {
                SetDead(false, "Protocol not running");
                return;
            }

            BusinessTimeOut = 15000;
            ResetTime();
            LogHelper.Log("Bussiness Hand Start");
            thread = new Thread(HandThread);
            thread.IsBackground = true;
            thread.Start();
        }

        private void HandThread()
        {
            while (!Dead)
            {
                Sender.JustSendImmediately(frame);
            }
        }

        public override bool ProcessDLFrame(CmindFrame recFrame)
        {
            if (!recFrame.IsHand)
            {
                return false;
            }

            CallRec(recFrame, "Hand Succeed");
            return SetDead(true);
        }
    }
}
