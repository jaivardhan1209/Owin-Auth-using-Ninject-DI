using System;
using System.Web;
using System.Web.Http;

using DependencyResolver;

using Microsoft.Owin;
using Microsoft.Owin.Security.Infrastructure;
using Microsoft.Owin.Security.OAuth;

using Ninject;
using Ninject.Web.Common;
using Ninject.Web.Common.OwinHost;
using Ninject.Web.WebApi.OwinHost;

using Owin;

using WebApi;
using WebApi.Security;

[assembly: OwinStartup(typeof(Startup))]

namespace WebApi
{
    /// <summary>The startup.</summary>
    public class Startup
    {
        /// <summary>
        /// The configuration.
        /// </summary>
        /// <param name="app">
        /// The app.
        /// </param>
        public void Configuration(IAppBuilder app)
        {
            var config = new HttpConfiguration();
            var kernel = CreateKernel();
            app.UseNinjectMiddleware(() => kernel);

            OAuthConfig.Configure(app, kernel);
            WebApiConfig.Register(config);
            app.UseNinjectWebApi(config);
        }

        /// <summary>The create kernel.</summary>
        /// <returns>The <see cref="IKernel"/>.</returns>
        private static IKernel CreateKernel()
        {
            var kernel = new StandardKernel();
            try
            {
                kernel.Bind<Func<IKernel>>().ToMethod(ctx => () => new Bootstrapper().Kernel);
                kernel.Bind<IHttpModule>().To<HttpApplicationInitializationHttpModule>();
                RegisterServices(kernel);
                return kernel;
            }
            catch
            {
                kernel.Dispose();
                throw;
            }
        }

        /// <summary>
        /// Load your modules or register your services here!
        /// </summary>
        /// <param name="kernel">The kernel.</param>
        private static void RegisterServices(IKernel kernel)
        {
			// For binding server side dependencies 
           // NinjectLoader.LoadNinjectModules(kernel);
            kernel.Bind<IAuthenticationTokenProvider>().To<SimpleRefreshTokenProvider>();
            kernel.Bind<IOAuthAuthorizationServerProvider>().To<SimpleAuthorizationServerProvider>();
        }
    }
}