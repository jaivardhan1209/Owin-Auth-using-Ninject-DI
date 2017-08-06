using System.Web;
using System.Web.Configuration;
using System.Web.Http;

using DependencyResolver;

using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace WebApi
{
    /// <summary>The web api application.</summary>
    public class WebApiApplication : HttpApplication
    {
        /// <summary>The application_ start.</summary>
        protected void Application_Start()
        {
            this.InitializeCache();

            GlobalConfiguration.Configuration.Formatters.JsonFormatter.SerializerSettings = this.GetJsonSettings();
        }

        /// <summary>The initialize cache.</summary>
        private void InitializeCache()
        {
            var ignitePath = HttpContext.Current.Server.MapPath(@"~\bin\");
            var igniteServerHostName = WebConfigurationManager.AppSettings["IgniteServerHostName"];
            var igniteServerPort = int.Parse(WebConfigurationManager.AppSettings["IgniteServerPort"]);
            CacheModule.Initialize(ignitePath, igniteServerHostName, igniteServerPort);
        }

        /// <summary>The application_ end.</summary>
        protected void Application_End()
        {
            // Due to a known problem with IIS, when the web app is restarted (due to code changes or manual restart),
            // the application pool process remains alive, while AppDomain gets recycled.
            // This causes the .NET part of Ignite node to be destroyed, but the Java part remains within the worker process.
            // To fix this issue, all Ignite instances when have to be stopped when the Web Application stops
            // https://gridgain.freshdesk.com/helpdesk/tickets/3157
            // https://apacheignite-net.readme.io/docs/deployment#section-aspnet-deployment
            CacheModule.Shutdown();
        }

        /// <summary>The get json settings.</summary>
        /// <returns>The <see cref="JsonSerializerSettings"/>.</returns>
        private JsonSerializerSettings GetJsonSettings()
        {
            // Customization Code to make Json serialization default for all client
            // Using Newtonsoft Json serializer, tweaking the serializer settings
            return new JsonSerializerSettings
                       {
                           // Ignore the Self Reference looping
                           ReferenceLoopHandling = ReferenceLoopHandling.Ignore,

                           // Do not Preserve the Reference Handling
                           PreserveReferencesHandling = PreserveReferencesHandling.None,

                           // Make All properties Camel Case
                           ContractResolver = new CamelCasePropertyNamesContractResolver(),

                           // Setting the Formatting for the Json output to be indented
                           Formatting = Formatting.Indented
                       };
        }
    }
}