using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace HealthCheckDemo.HealthChecks
{
    /// <summary>
    /// Directory exists health check
    /// </summary>
    public class DirectoryExistsHealthCheck : IHealthCheck
    {
        /// <summary>
        /// Directory path
        /// </summary>
        public string Path { get; set; }

        /// <summary>
        /// Create a new instance of DirectoryExistsHealthCheck
        /// </summary>
        /// <param name="path">Directory path to check</param>
        public DirectoryExistsHealthCheck(string path)
        {
            Path = path;
        }

        //
        // Summary:
        //     Runs the health check, returning the status of the component being checked.
        //
        // Parameters:
        //   context:
        //     A context object associated with the current execution.
        //
        //   cancellationToken:
        //     A System.Threading.CancellationToken that can be used to cancel the health check.
        //
        // Returns:
        //     A System.Threading.Tasks.Task`1 that completes when the health check has finished,
        //     yielding the status of the component being checked.
        public Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = default)
        {
            var result = new HealthCheckResult();
            try
            {
                result = Directory.Exists(Path)
                    ? HealthCheckResult.Healthy()
                    : HealthCheckResult.Unhealthy("Directory is missing");
            }
            catch (Exception ex)
            {
                result = HealthCheckResult.Unhealthy("Error when trying to check directory path. ", ex);
            }

            return Task.FromResult(result);
        }
    }

    /// <summary>
    /// DirectoryExistsHealthCheck extensions
    /// </summary>
    public static class DirectoryExistsHealthCheckExtensions
    {
        /// <summary>
        /// Add directory exist health check
        /// </summary>
        /// <param name="builder">Health check builder</param>
        /// <param name="path">Directory path to check</param>
        /// <param name="name">Name</param>
        /// <param name="failureStatus">Failure status</param>
        /// <param name="tags">Tags</param>
        /// <returns>Health check builder</returns>
        public static IHealthChecksBuilder AddDirectoryExistsHealthCheck(this IHealthChecksBuilder builder, string path, string name, HealthStatus? failureStatus, IEnumerable<string> tags)
        {
            var check = new HealthCheckRegistration(name, new DirectoryExistsHealthCheck(path), failureStatus, tags);
            return builder.Add(check);
        }
    }
}
