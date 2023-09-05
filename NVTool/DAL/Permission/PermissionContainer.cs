/***************************************************************************************************
* copyright : CMIND-SEMI
* version   : 1.00
* file      : PermissionContainer.cs
* date      : 2023/7/7 10:13:13
* author    : jinlong.wang
* brief     : 权限容器，可配置不同的权限
* section Modification History
* - 1.0 : Initial version (2023/7/7 10:13:13) - jinlong.wang
***************************************************************************************************/

using NVTool.DAL.Model;
using System;
using System.Collections.Generic;

namespace NVTool.DAL.Permission
{
    class PermissionContainer
    {
        #region Attribute
        private Dictionary<String, Dictionary<PermissionType, bool>> permissions;
        #endregion

        #region Constructor
        public PermissionContainer()
        {
            permissions = new Dictionary<String, Dictionary<PermissionType, bool>>();
        }
        #endregion

        /// <summary>
        /// 添加用户类型及其权限
        /// </summary>
        public void AddUserType(String userType, Dictionary<PermissionType, bool> userPermissions)
        {
            permissions[userType] = userPermissions;
        }

        /// <summary>
        /// 获取用户类型的权限
        /// </summary>
        public Dictionary<PermissionType, bool> GetUserPermissions(string userType)
        {
            if (permissions.ContainsKey(userType))
            {
                return permissions[userType];
            }
            else
            {
                throw new ArgumentException("Invalid user type");
            }
        }
    }
}

//userage
//// 创建权限容器
//var permissionContainer = new PermissionContainer();

//// 创建研发用户类型及其权限
//var developerPermissions = new Dictionary<PermissionType, bool>()
//        {
//            { PermissionType.Read, true },
//            { PermissionType.Write, true },
//            { PermissionType.Delete, true },
//            { PermissionType.Execute, true }
//        };

//// 创建普通用户类型及其权限
//var userPermissions = new Dictionary<PermissionType, bool>()
//        {
//            { PermissionType.Read, true },
//            { PermissionType.Write, false },
//            { PermissionType.Delete, false },
//            { PermissionType.Execute, false }
//        };

//// 添加用户类型及其权限到容器
//permissionContainer.AddUserType("Developer", developerPermissions);
//permissionContainer.AddUserType("User", userPermissions);

//// 获取研发用户类型的权限
//var developerType = "Developer";
//var developerTypePermissions = permissionContainer.GetUserPermissions(developerType);

//Console.WriteLine($"Permissions for {developerType}:");
//foreach (var permission in developerTypePermissions)
//{
//    Console.WriteLine($"{permission.Key}: {permission.Value}");
//}

//// 获取普通用户类型的权限
//var userType = "User";
//var userTypePermissions = permissionContainer.GetUserPermissions(userType);

//Console.WriteLine($"\nPermissions for {userType}:");
//foreach (var permission in userTypePermissions)
//{
//    Console.WriteLine($"{permission.Key}: {permission.Value}");
//}