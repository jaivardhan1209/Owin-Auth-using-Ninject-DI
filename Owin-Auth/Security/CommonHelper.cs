using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;

namespace WebApi.Security
{
    /// <summary>
    /// Helper class to do tasks associated with authentication
    /// </summary>
    public class CommonHelper
    {
        #region Public Methods

        /// <summary>
        /// Method to get the user roles
        /// </summary>
        /// <param name="userName">username</param>
        /// <returns>user role strings</returns>
        public string GetUserRoles(string userName)
        {
            // ManagementController controller = new ManagementController();
            ////string roles;
            ////UserManagerHelper manager = new UserManagerHelper();
            ////Dictionary<string, List<string>> privileges = manager.GetUserRoles(userName);
            // return JsonConvert.SerializeObject(controller.GetUIRoles(1).RoleData);
            return null;
        }

        /// <summary>
        /// Get UI Privileges
        /// </summary>
        /// <param name="userName">username</param>
        /// <returns>user privilege string</returns>
        public string GetUiPrivileges(string userName)
        {
            // string roles;
            // UserManagerHelper manager = new UserManagerHelper();
            // string privileges = manager.GetUIPrivileges(userName);
            // return privileges;
            return null;
        }

        /// <summary>
        /// The check user claims.
        /// </summary>
        /// <param name="claimsIdentity">
        /// The claims identity.
        /// </param>
        /// <param name="roleHavingAccess">
        /// The role having access.
        /// </param>
        /// <exception cref="Exception">
        /// </exception>
        public static void CheckUserClaims(ClaimsIdentity claimsIdentity, string roleHavingAccess)
        {
            // ClaimsIdentity userIdentity = (ClaimsIdentity)User.Identity;
            Claim role = claimsIdentity.Claims.FirstOrDefault(c => c.Type == ClaimTypes.Role && c.Value.Equals(roleHavingAccess));
            if (role == null)
            {
                throw new System.Exception("you do not have privilege to access this");
            }
        }

        /// <summary>
        /// The check user claims.
        /// </summary>
        /// <param name="claimsIdentity">
        /// The claims identity.
        /// </param>
        /// <param name="roleHavingAccess">
        /// The role having access.
        /// </param>
        /// <exception cref="Exception">
        /// </exception>
        public static void CheckUserClaims(ClaimsIdentity claimsIdentity, List<string> roleHavingAccess)
        {
            // ClaimsIdentity userIdentity = (ClaimsIdentity)User.Identity;
            Claim role = claimsIdentity.Claims.FirstOrDefault(c => c.Type == ClaimTypes.Role && roleHavingAccess.Contains(c.Value));
            if (role == null)
            {
                throw new System.Exception("you do not have privilege to access this");
            }
        }

        #endregion
    }
}