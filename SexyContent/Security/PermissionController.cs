﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using DotNetNuke.Entities.Modules;
using DotNetNuke.Security;
using ToSic.Eav;
using ToSic.SexyContent.Security;

namespace ToSic.SexyContent.Security
{
    /// <summary>
    /// Permissions object which checks if the user is allowed to do soemthing based on specific permission
    /// </summary>
    public class PermissionController
    {
        public const int AssignmentObjectId = 4;
        public const string ContentTypeName = "PermissionConfiguration";
        public const string Condition = "Condition";
        public const string Grant = "Grant";
        public string CustomPermissionKey = ""; // "CONTENT";
        readonly string _salPrefix = "SecurityAccessLevel.".ToLower();

        public int AppId { get; private set; }
        public int ZoneId { get; private set; }
        public Guid TargetGuid { get; private set; }
        private IEnumerable<IEntity> _permissionList;

        public IEnumerable<IEntity> PermissionList
        {
            get
            {
                if (_permissionList == null)
                {
                    var ds = DataSource.GetMetaDataSource(ZoneId, AppId);
                    _permissionList = ds.GetAssignedEntities(AssignmentObjectId, TargetGuid, ContentTypeName);
                }
                return _permissionList;
            }
            private set { _permissionList = value; }
        }

        public ModuleInfo Module { get; private set; }

        /// <summary>
        /// Initialize this object so it can then give information regarding the permissions of an entity.
        /// Uses a GUID as identifier because that survives export/import. 
        /// </summary>
        /// <param name="zoneId">EAV Zone</param>
        /// <param name="appId">EAV APP</param>
        /// <param name="targetGuid">Entity GUID to check permissions against</param>
        /// <param name="module">DNN Module - necessary for SecurityAccessLevel checks</param>
        public PermissionController(int zoneId, int appId, Guid targetGuid, ModuleInfo module = null)
        {
            ZoneId = zoneId;
            AppId = appId;
            TargetGuid = targetGuid;
            Module = module;
        }

        /// <summary>
        /// Check if a user may do something based on the permissions in the background. 
        /// </summary>
        /// <param name="actionCode">Short-code for r=read, u=update etc.</param>
        /// <returns></returns>
        public bool UserMay(char actionCode)
        {
            return DoesPermissionsListAllow(actionCode);
        }

        public bool UserMay(PermissionGrant action)
        {
            return DoesPermissionsListAllow((Char)action);
        }

        /// <summary>
        /// Check if the permission-list would allow such an action
        /// </summary>
        /// <param name="desiredActionCode">The desired action like c, r, u, d etc.</param>
        /// <returns></returns>
        private bool DoesPermissionsListAllow(char desiredActionCode)
        {
            var allow = false;
            foreach (var perm in PermissionList)
            {
                allow = DoesPermissionAllow(perm, desiredActionCode);
                if (allow) // stop checking if permission ok
                    break;
            }
            return allow;
        }

        /// <summary>
        /// Check if a specific permission entity allows for the desired permission
        /// </summary>
        /// <param name="permissionEntity">The entity describing a permission</param>
        /// <param name="desiredActionCode">A key like r (for read), u (for update) etc. which is the level you want to check</param>
        /// <returns></returns>
        private bool DoesPermissionAllow(IEntity permissionEntity, char desiredActionCode)
        {

            // Check if it's a grant-read permission - otherwise stop here
            var grnt = permissionEntity.GetBestValue(Grant).ToString();
            if (grnt.IndexOf(desiredActionCode) == -1) // Grant doesn't contain read, so stop here
                return false;

            // Check if the current user fits the reason for this grant
            try
            {
                var rsn = permissionEntity.GetBestValue(Condition).ToString();
                if (rsn.ToLower().StartsWith(_salPrefix))
                {
                    var salWord = rsn.Substring(_salPrefix.Length);
                    var sal = (SecurityAccessLevel)Enum.Parse(typeof(SecurityAccessLevel), salWord);
                    return DotNetNuke.Security.Permissions.ModulePermissionController
                        .HasModuleAccess(sal, CustomPermissionKey, Module);
                }
            }
            catch
            {
                // something happened, assume that this rule cannot describe a "is allowed"
                return false;
            }

            // If the code gets here, we apparently don't know what the rule is about - return false
            return false;
        }
    }
}