using System.Threading.Tasks;

using Microsoft.Owin;

namespace WebApi.Security
{
    /// <summary>
    /// This has methods to handle invalid tokens 
    /// </summary>
    public class InvalidAuthenticationMiddleware : OwinMiddleware
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="InvalidAuthenticationMiddleware"/> class.
        /// </summary>
        /// <param name="next">
        /// The next.
        /// </param>
        public InvalidAuthenticationMiddleware(OwinMiddleware next)
            : base(next)
        {
        }

        /// <summary>
        /// Checks for authorization response headers
        /// </summary>
        /// <param name="context">OWIN context</param>
        /// <returns>Task</returns>
        public override async Task Invoke(IOwinContext context)
        {
            await this.Next.Invoke(context);

            if (context.Response.StatusCode == 400 && context.Response.Headers.ContainsKey("AuthorizationResponse"))
            {
                context.Response.Headers.Remove("AuthorizationResponse");
                context.Response.StatusCode = 200;
            }
        }
    }
}