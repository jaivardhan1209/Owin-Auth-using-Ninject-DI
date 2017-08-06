using System;
using System.Linq;
using System.Net.Http;
using System.Security.Claims;
using System.Security.Principal;
using System.Web.Http;
using System.Web.Http.Controllers;

namespace WebApi.Security
{
    /// <summary>
    /// The custom authorize attribute.
    /// </summary>
    public class CustomAuthorizeAttribute : AuthorizeAttribute
    {
        /// <summary>
        /// The user.
        /// </summary>
        private IPrincipal User;

        /// <summary>
        /// Initializes a new instance of the <see cref="CustomAuthorizeAttribute"/> class.
        /// </summary>
        /// <param name="user">
        /// The user.
        /// </param>
        public CustomAuthorizeAttribute(ClaimsIdentity user)
        {
            // User = user;
        }

        /// <summary>
        /// The on authorization.
        /// </summary>
        /// <param name="actionContext">
        /// The action context.
        /// </param>
        /// <exception cref="Exception">
        /// </exception>
        public override void OnAuthorization(HttpActionContext actionContext)
        {
            base.OnAuthorization(actionContext);

            if (actionContext.Request.Headers.Authorization != null)
            {
            }
            else
            {
                actionContext.Response = new HttpResponseMessage(System.Net.HttpStatusCode.Unauthorized);
                return;
            }

            var userIdentity = (ClaimsIdentity)this.User.Identity;

            var role = userIdentity.Claims.FirstOrDefault(c => c.Type == ClaimTypes.Role && c.Value == "Viewr.Viewr.Viewr.F");
            if (role == null)
            {
                throw new System.Exception("you do not have privilege to access this");
            }
        }
    }
}