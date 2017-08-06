using System;
using System.Configuration;

using Microsoft.AspNet.Identity;
using Microsoft.Owin;
using Microsoft.Owin.Security;
using Microsoft.Owin.Security.Cookies;
using Microsoft.Owin.Security.DataHandler;
using Microsoft.Owin.Security.DataProtection;
using Microsoft.Owin.Security.Infrastructure;
using Microsoft.Owin.Security.OAuth;

using Ninject;

using Owin;

using WebApi.Security;

namespace WebApi
{
    /// <summary>The OAuth configuration class.</summary>
    public class OAuthConfig
    {
        /// <summary>
        /// Configures oauth.
        /// For more information on configuring authentication, please visit http://go.microsoft.com/fwlink/?LinkId=301864
        /// </summary>
        /// <param name="app">The app.</param>
        /// <param name="kernel">The kernel</param>
        public static void Configure(IAppBuilder app, IKernel kernel)
        {
            app.Use<InvalidAuthenticationMiddleware>();

            // Token Generation Server Configuration
            var simpleAuthServer = kernel.Get<IOAuthAuthorizationServerProvider>();
            var authTokenProvider = kernel.Get<IAuthenticationTokenProvider>();
            var accessTokenFormat =
                new TicketDataFormat(app.CreateDataProtector(typeof(OAuthAuthorizationServerMiddleware).Namespace, "Access_Token", "v1"));
            int accessTokenExpireTimeSpan = Convert.ToInt32(ConfigurationManager.AppSettings["accessTokenTimeoutInMinutes"] ?? "30");
            var oauthServerOptions = ConfigureAuthorization(simpleAuthServer, authTokenProvider, accessTokenFormat, accessTokenExpireTimeSpan);
            app.UseOAuthAuthorizationServer(oauthServerOptions);

            // Enable the application to use a token to store information for the signed in user
            var oauthBearerOptions = new OAuthBearerAuthenticationOptions();
            app.UseOAuthBearerAuthentication(oauthBearerOptions);

            // Enable the application to use a cookie to store information for the signed in user
            app.UseCookieAuthentication(ConfigureCookies(oauthServerOptions));
        }

        /// <summary>The configure authorization.</summary>
        /// <param name="simpleAuthServer">The simple auth server.</param>
        /// <param name="authTokenProvider">The auth token provider.</param>
        /// <param name="accessTokenFormat">The access token format.</param>
        /// <param name="accessTokenExpireTimeSpan">The access token expire time span.</param>
        /// <returns>The <see cref="OAuthAuthorizationServerOptions"/>.</returns>
        private static OAuthAuthorizationServerOptions ConfigureAuthorization(IOAuthAuthorizationServerProvider simpleAuthServer,
                                                                              IAuthenticationTokenProvider authTokenProvider,
                                                                              TicketDataFormat accessTokenFormat,
                                                                              int accessTokenExpireTimeSpan)
        {
            var oauthServerOptions = new OAuthAuthorizationServerOptions
                                         {
                                             AllowInsecureHttp = true,
                                             TokenEndpointPath = new PathString("/token"),
                                             AccessTokenExpireTimeSpan = TimeSpan.FromMinutes(accessTokenExpireTimeSpan),
                                             Provider = simpleAuthServer,
                                             AccessTokenFormat = accessTokenFormat,
                                             RefreshTokenProvider = authTokenProvider
                                         };
            return oauthServerOptions;
        }

        /// <summary>The configure cookies.</summary>
        /// <param name="oauthServerOptions">The oauth server options.</param>
        /// <returns>The <see cref="CookieAuthenticationOptions"/>.</returns>
        private static CookieAuthenticationOptions ConfigureCookies(OAuthAuthorizationServerOptions oauthServerOptions)
        {
            return new CookieAuthenticationOptions
                       {
                           CookieName = "Token",
                           AuthenticationType = DefaultAuthenticationTypes.ApplicationCookie,
                           LoginPath = new PathString("/Home/Login"),
                           AuthenticationMode = AuthenticationMode.Active,
                           TicketDataFormat = oauthServerOptions.AccessTokenFormat,
                           Provider = new CookieAuthenticationProvider
                                          {
                                              OnException = context =>
                                                                {
                                                                    var cookieToExpire = context.Request.Cookies["Token"];
                                                                    context.Response.Cookies.Append("Token",
                                                                                                    cookieToExpire,
                                                                                                    new CookieOptions
                                                                                                        {
                                                                                                            Expires = DateTime.Now.AddDays(-1)
                                                                                                        });
                                                                }
                                          }
                       };
        }
    }
}