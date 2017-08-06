using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

using Application.Facade.DomainHelper;

using Common.Utilities;

using Interface.Application;

using Microsoft.Owin.Security;
using Microsoft.Owin.Security.OAuth;

namespace WebApi.Security
{
    /// <summary>
    /// This class has the token generation logics
    /// </summary>
    public class SimpleAuthorizationServerProvider : OAuthAuthorizationServerProvider
    {
        /// <summary>The auth facade.</summary>
		// Server side Interface for Auth
        private readonly IAuthFacade authFacade;

        /// <summary>The user management.</summary>
        private readonly IUserManagement userManagement;

        /// <summary>Initializes a new instance of the <see cref="SimpleAuthorizationServerProvider"/> class.</summary>
        /// <param name="authFacade">The auth facade.</param>
        /// <param name="userManagement">The user management.</param>
        public SimpleAuthorizationServerProvider(IAuthFacade authFacade, IUserManagement userManagement)
        {
            this.authFacade = authFacade;
            this.userManagement = userManagement;
        }

        /// <summary>The token endpoint.</summary>
        /// <param name="context">The context.</param>
        /// <returns>The <see cref="Task"/>.</returns>
        public override Task TokenEndpoint(OAuthTokenEndpointContext context)
        {
            foreach (KeyValuePair<string, string> property in context.Properties.Dictionary)
            {
                context.AdditionalResponseParameters.Add(property.Key, property.Value);
            }

            return Task.FromResult<object>(null);
        }

        /// <summary>The validate client authentication.</summary>
        /// <param name="context">The context.</param>
        /// <returns>The <see cref="Task"/>.</returns>
        public async override Task ValidateClientAuthentication(OAuthValidateClientAuthenticationContext context)
        {
            // Initiate and start a diagnostic stopwatch
             string clientId;
            string clientSecret;

            if (!context.TryGetBasicCredentials(out clientId, out clientSecret))
            {
                context.TryGetFormCredentials(out clientId, out clientSecret);
            }

            if (context.ClientId == null)
            {
                // Remove the comments from the below line context.SetError, and invalidate context 
                // if you want to force sending clientId/secrects once obtain access tokens. 
                context.Validated();

                // context.SetError("invalid_clientId", "ClientId should be sent.");
               // return Task.FromResult<object>(null);
            }

            // TODO Change the Input to pass contect.ClientId
            // var inputparam = new AuthorizationInput { ClientAppName = context.ClientId };

            // var authorizationJson = JsonConvert.SerializeObject(inputparam);

            // Create Dictionary with Key : filter and Value is JSON String 
            var clientController = AuthHelper.FindClientController(context.ClientId);

            var client = await this.authFacade.FindClient(clientController);

            // TODO this may cause issue due to longer run of code, if fail first check here. make this func aysnc
            if (client == null)
            {
                context.SetError("invalid_clientId", $"Client '{context.ClientId}' is not registered in the system.");
              //  return Task.FromResult<object>(null);
            }

            var b = !client.IsActive;
            if (b != null && (bool)b)
            {
                context.SetError("invalid_clientId", "Client is inactive.");
               // return Task.FromResult<object>(null);
            }

            context.OwinContext.Set<string>("as:clientAllowedOrigin", client.AllowedOrigin);
            context.OwinContext.Set<string>(
            "as:clientRefreshTokenLifeTime",
            client.RefreshTokenLifeTime.ToString(CultureInfo.InvariantCulture));

            context.Validated();

           // return Task.FromResult<object>(null);
        }

        /// <summary>The grant resource owner credentials.</summary>
        /// <param name="context">The context.</param>
        /// <returns>The <see cref="Task"/>.</returns>
        public override async Task GrantResourceOwnerCredentials(OAuthGrantResourceOwnerCredentialsContext context)
        {
            // Adding Header to enable the Cross Origin Calls
             var allowedOrigin = context.OwinContext.Get<string>("as:clientAllowedOrigin") ?? "*";

            if (!context.OwinContext.Response.Headers.ContainsKey("Access-Control-Allow-Origin"))
                context.OwinContext.Response.Headers.Add("Access-Control-Allow-Origin", new[] { allowedOrigin });

            // Create User Credential using Name/ password from Auth context
            var userCredentials = AuthHelper.CheckUserCredential(context.UserName, AuthenticationUtility.GetHash(context.Password));
            var tokenInfoList = await this.userManagement.SelectAll(userCredentials);

            // Get valid userInfo for given credential
            var tokenInfo = tokenInfoList.Values.FirstOrDefault();

            if (tokenInfo == null)
            {
                context.SetError("Wrong username or password.");
                context.Response.Headers.Add("AuthorizationResponse", new[] { "Failed" });
                return;
            }

            if (tokenInfo.IsActive == 0)
            {
                context.SetError("Your account is disabled. Please contact the Admin.");
                context.Response.Headers.Add("AuthorizationResponse", new[] { "Failed" });
                return;
            }

            if (tokenInfo.IsLocked == 1)
            {
                context.SetError("Your account is locked. Please contact the Admin.");
                context.Response.Headers.Add("AuthorizationResponse", new[] { "Failed" });
                return;
            }

            // Set basic user information
            var guid = Guid.NewGuid().ToString();
            var identity = new ClaimsIdentity(context.Options.AuthenticationType);
            identity.AddClaim(new Claim(ClaimTypes.Name, context.UserName));

            identity.AddClaim(new Claim("userName", context.UserName));
            identity.AddClaim(new Claim("UserId", tokenInfo.Id.ToString()));
            identity.AddClaim(new Claim("GUID", guid));
            identity.AddClaim(new Claim("persistence", "jai was here"));

            var props = new AuthenticationProperties(new Dictionary<string, string>
             {
             {
             "GUID", guid
             },
             {
             "as:client_id", context.ClientId ?? null
             },
             {
             "userName", tokenInfo.UserName
             },
             {
             "userId", tokenInfo.Id.ToString()
             },
             {
             "role", tokenInfo.Role
             },
             {
             "isLocked", tokenInfo.IsLocked.ToString()
             },
             {
             "isActive", tokenInfo.IsActive.ToString()
             }
             });

            var ticket = new AuthenticationTicket(identity, props);

            context.Validated(ticket);

            return;
        }

        /// <summary>The grant refresh token.</summary>
        /// <param name="context">The context.</param>
        /// <returns>The <see cref="Task"/>.</returns>
        public override Task GrantRefreshToken(OAuthGrantRefreshTokenContext context)
        {
            var originalClient = context.Ticket.Properties.Dictionary["as:client_id"];
            var currentClient = context.ClientId;

            if (originalClient != currentClient)
            {
                context.SetError("invalid_clientId", "Refresh token is issued to a different clientId.");
                return Task.FromResult<object>(null);
            }

            if (!(context.Ticket.Properties.ExpiresUtc >= DateTime.UtcNow))
            {
                context.SetError("invalid_refreshToken", "Refresh token has expired. Login again.");
                return Task.FromResult<object>(null);
            }

            // Change auth ticket for refresh token requests
            var newIdentity = new ClaimsIdentity(context.Ticket.Identity);
            var newTicket = new AuthenticationTicket(newIdentity, context.Ticket.Properties);
            context.Validated(newTicket);

            return Task.FromResult<object>(null);
        }
    }
}