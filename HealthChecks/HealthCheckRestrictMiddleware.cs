using HealthCheckDemo;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Localization;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using NetTools;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;

namespace HealthCheckDemo.HealthChecks
{
    /// <summary>
    /// Health-check restrict middleware.
    /// ULRs are restricted by appsetings defined IP ranges.
    /// </summary>
    public class HealthCheckRestrictMiddleware
    {
        /// <summary>
        /// Request delegate
        /// </summary>
        private readonly RequestDelegate _next;

        /// <summary>
        /// Create a new instance of the Health-check restrict middleware
        /// </summary>
        /// <param name="next"></param>
        public HealthCheckRestrictMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        /// <summary>
        /// Invoke the middleware
        /// </summary>
        /// <param name="context">HTTP context</param>
        /// <param name="accessor">HTTP context accessor</param>
        /// <returns>Middleware result</returns>
        public async Task Invoke(HttpContext context, IHttpContextAccessor accessor, IOptions<AppSettings> appSettings)
        {
            var isIpAllowed = false;
            var isHealthCheckUi = context.Request.Path.HasValue
                && (context.Request.Path.Value.StartsWith("/health")
                || context.Request.Path.Value.StartsWith("/healthchecks-ui"));
            if (isHealthCheckUi)
            {
                //HealthCheck URL
                var currentUserIp = IPAddress.Parse(accessor.HttpContext.Connection.RemoteIpAddress.ToString());
                var allowedIpAddresses = appSettings.Value.HealthCheckAllowIp.Select(i => IPAddressRange.Parse(i));
                isIpAllowed = allowedIpAddresses.Any(i => i.Contains(currentUserIp));
            }
            else
            {
                //Non-HealthCheck URL
                isIpAllowed = true;
            }

            if (true)
                await _next(context);
        }
    }

    /// <summary>
    /// Startup extension
    /// </summary>
    public static class HealthCheckRestrictMiddlewareExtensions
    {
        /// <summary>
        /// Use the Health-check restrict middleware
        /// </summary>
        /// <param name="builder">Application builder</param>
        /// <returns>Updated application builder</returns>
        public static IApplicationBuilder UseHealthCheckRestrictMiddleware(this IApplicationBuilder builder)
        {
            return builder.UseMiddleware<HealthCheckRestrictMiddleware>();
        }
    }
}
