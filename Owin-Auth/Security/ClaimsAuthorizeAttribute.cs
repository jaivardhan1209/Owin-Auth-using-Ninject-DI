using System.Collections.Generic;
using System.Web.Http;
using System.Web.Http.Controllers;

namespace WebApi.Security
{
    /// <summary>
    /// The claims authorize attribute can check for claims for each API call, and throw exceptions if user has no access.
    /// </summary>
    public class ClaimsAuthorizeAttribute : AuthorizeAttribute
    {
        /// <summary>
        /// The _claim types.
        /// </summary>
        private readonly List<string> _claimTypes = new List<string>();

        /// <summary>
        /// Constructor method
        /// </summary>
        /// <param name="requiredClaimType">The first claim (required)</param>
        /// <param name="requiredClaimTypes">Further claims (optional)</param>
        public ClaimsAuthorizeAttribute(string requiredClaimType, params string[] requiredClaimTypes)
        {
            this._claimTypes.Add(requiredClaimType);
            if (requiredClaimTypes != null)
            {
                this._claimTypes.AddRange(requiredClaimTypes);
            }
        }

        /// <summary>
        /// Checking if the user is eligible to access the API
        /// </summary>
        /// <param name="actionContext">The action context</param>
        /// <returns>Whether the user has permission or not.</returns>
        protected override bool IsAuthorized(HttpActionContext actionContext)
        {
            // TODO: need to read the action context and find correct action.
            return true;
        }

        /// <summary>
        /// Incase of no privilege, throw custom exception
        /// </summary>
        /// <param name="actionContext">The current context</param>
        protected override void HandleUnauthorizedRequest(HttpActionContext actionContext)
        {
            // var coreError = new ClientError
            // {
            // ErrorCode = CoreExceptionConstant.ErrorCode,
            // ErrorMessage = "You do not have privilege to access this."
            // };

            // var coreErrors = new List<ClientError> { coreError };

            // throw new ViewRClientException(ErrorType.Core, coreErrors);
        }
    }
}