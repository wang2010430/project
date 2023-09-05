/***************************************************************************************************
* copyright : CMIND-SEMI
* version   : 1.00
* file      : PermissionManager.cs
* date      : 2023/9/5 10:25:45
* author    : jinlong.wang
* brief     : 
* section Modification History
* - 1.0 : Initial version - jinlong.wang
***************************************************************************************************/

using NVTool.DAL;
using NVTool.DAL.Permission;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NVTool.BLL
{
    class PermissionManager
    {
        #region Attribute
        private static PermissionManager instance = null;
        private static readonly object lockObject = new object();

        PermissionContainer permissionContainer = new PermissionContainer();
        UserRole userRole = UserRole.Administrator;

        internal UserRole UserRole { get => userRole; set => userRole = value; }
        #endregion

        public static PermissionManager Instance
        {
            get
            {
                lock (lockObject)
                {
                    if (instance == null)
                    {
                        instance = new PermissionManager();
                    }
                    return instance;
                }
            }
        }

        public PermissionManager()
        {
          
        }

    }
}
