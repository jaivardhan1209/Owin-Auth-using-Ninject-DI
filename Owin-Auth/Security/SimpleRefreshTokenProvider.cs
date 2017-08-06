using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

using Application.Facade.DomainHelper;

using Common.Utilities;

using Entity.Domain;

using Interface.Application;

using Microsoft.Owin.Security.Infrastructure;

namespace WebApi.Security
{
    /// <summary>
    /// Simple refresh token provider class.
    /// This class declares all the functions used by the Authenticator
    /// to verify the user credentials and provide a token.
    /// </summary>
    public class SimpleRefreshTokenProvider : IAuthenticationTokenProvider
    {
        /// <summary>The auth facade.</summary>
        private readonly IAuthFacade authFacade;

        /// <summary>Initializes a new instance of the <see cref="SimpleRefreshTokenProvider"/> class.</summary>
        /// <param name="authFacade">The auth facade.</param>
        public SimpleRefreshTokenProvider(IAuthFacade authFacade)
        {
            this.authFacade = authFacade;
        }

        /// <summary>The create async.</summary>
        /// <param name="context">The context.</param>
        /// <returns>The <see cref="Task"/>.</returns>
        public async Task CreateAsync(AuthenticationTokenCreateContext context)
        {
            var clientid = context.Ticket.Properties.Dictionary["as:client_id"];

            if (string.IsNullOrEmpty(clientid))
            {
                return;
            }

            var refreshTokenId = Guid.NewGuid().ToString("n");

            ClaimsIdentity userIdentity = context.Ticket.Identity;
            var firstOrDefault = userIdentity.Claims.FirstOrDefault(c => c.Type == "GUID");
            if (firstOrDefault != null)
            {
                var sessionGuid = new Guid(firstOrDefault.Value);

                var refreshTokenLifeTime = context.OwinContext.Get<string>("as:clientRefreshTokenLifeTime");

                var clientController = AuthHelper.FindClientController(clientid);
                var clientAppId = await this.authFacade.FindClient(clientController);

                var token = new RefreshToken
                                {
                                    RefreshTokenId = AuthenticationUtility.GetHash(refreshTokenId),
                                    Id = clientAppId.Id,
                                    UserName = context.Ticket.Identity.Name,
                                    IssuedTimestamp = DateTime.Now,
                                    ExpiryTimestamp = DateTime.Now.AddMinutes(Convert.ToDouble(refreshTokenLifeTime)),
                                    SessionGuId = sessionGuid,
                                    LastModifiedUserId = 1,
                                    LastModifiedTs = DateTime.Now,
                                    ClientAppId = (int)clientAppId.Id,
                                    
                                };

                context.Ticket.Properties.IssuedUtc = token.IssuedTimestamp;
                context.Ticket.Properties.ExpiresUtc = token.ExpiryTimestamp;

                token.ProtectedTicket = context.SerializeTicket();

                // Create Dictionary with Key : filter and Value is JSON String
                await this.authFacade.Update(new List<RefreshToken> { token });

                context.SetToken(refreshTokenId);
            }
        }

        /// <summary>The receive async.</summary>
        /// <param name="context">The context.</param>
        /// <returns>The <see cref="Task"/>.</returns>
        /// <exception cref="Exception"></exception>
        public async Task ReceiveAsync(AuthenticationTokenReceiveContext context)
        {
            try
            {
                var allowedOrigin = context.OwinContext.Get<string>("as:clientAllowedOrigin");
                if (!context.OwinContext.Response.Headers.ContainsKey("Access-Control-Allow-Origin"))
                    context.OwinContext.Response.Headers.Add("Access-Control-Allow-Origin", new[] { allowedOrigin });

                var hashedTokenId = AuthenticationUtility.GetHash(context.Token);

                var refreshTokenController = AuthHelper.FindRefreshTokenController(hashedTokenId);
                var refreshTokenDict = await this.authFacade.SelectAll(refreshTokenController);
                var refreshToken = refreshTokenDict.Values.FirstOrDefault();

                if (refreshToken != null)
                {
                    // Get protectedTicket from refreshToken class
                    context.DeserializeTicket(refreshToken.ProtectedTicket);

                    // var expires = context.Ticket.Properties.ExpiresUtc;
                    await this.authFacade.Delete(new List<long> { refreshToken.Id });
                }
            }
            catch (System.Exception ex)
            {

                throw new System.Exception("some error occured while reading the refersh token", ex);
            }
        }

        /// <summary>
        /// This method is called to create a refresh token
        /// </summary>
        /// <param name="context">The Authentication context</param>
        public void Create(AuthenticationTokenCreateContext context)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// This method is called when a refresh token is received.
        /// </summary>
        /// <param name="context">The Authentication context</param>
        public void Receive(AuthenticationTokenReceiveContext context)
        {
            throw new NotImplementedException();
        }
    }
}