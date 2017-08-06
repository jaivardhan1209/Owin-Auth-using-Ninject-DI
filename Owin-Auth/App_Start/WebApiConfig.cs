using System.Web.Http;

using Newtonsoft.Json.Serialization;

using WebApi.Exception;

namespace WebApi
{
    /// <summary>The web api config.</summary>
    public static class WebApiConfig
    {
        /// <summary>The register.</summary>
        /// <param name="config">The config.</param>
        public static void Register(HttpConfiguration config)
        {
            // Web API routes
            config.MapHttpAttributeRoutes();
            
            // Contract resolver is getting overriden during Owin Startup, need to reset to camel case
            config.Formatters.JsonFormatter.SerializerSettings.ContractResolver = new CamelCasePropertyNamesContractResolver();

            config.Routes.MapHttpRoute(name: "DefaultApi",
                                       routeTemplate: "api/{controller}/{id}",
                                       defaults: new
                                                     {
                                                         id = RouteParameter.Optional
                                                     });
        }
    }
}