/***************************************************************************************************
* copyright : CMIND-SEMI
* version   : 1.00
* file      : NVSSystemInfo.cs
* date      : 2023/8/4 15:06:47
* author    : jinlong.wang
* brief     : 
* section Modification History
* - 1.0 : Initial version - jinlong.wang
***************************************************************************************************/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CmindProtocol.DLL
{
    internal class NVSSystemInfo
    {
        public ushort SendMaxSize = 1024;
        public ushort ROSectorCount = 0;
        public ushort ROSectorSize = 0;
        public ushort RWSectorCount = 0;
        public ushort RWSectorSize = 0;
    }
}
