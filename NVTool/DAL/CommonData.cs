/***************************************************************************************************
* copyright : CMIND-SEMI
* version   : 1.00
* file      : CommonData.cs
* date      : 2023/9/5 10:03:46
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

namespace NVTool.DAL
{
    /// <summary>
    /// 明文和暗文
    /// </summary>
    enum PasswordState
    {
        plaintext,
        ciphertext
    }

    enum UserRole
    {
        User,
        Tester,
        Develeoper,
        Administrator
    }
}
