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
    /// Check Windows scheduled task status health check
    /// </summary>
    public class WindowsScheduledTaskHealthCheck : IHealthCheck
    {
        /// <summary>
        /// Task name
        /// </summary>
        public string Name { get; set; }
        /// <summary>
        /// Machine name
        /// </summary>
        public string MachineName { get; set; }
        /// <summary>
        /// Username
        /// </summary>
        public string Username { get; set; }
        /// <summary>
        /// Username
        /// </summary>
        public string Password { get; set; }
        /// <summary>
        /// Password
        /// </summary>
        public string Domain { get; set; }

        /// <summary>
        /// Create a new instance of WindowsScheduledTaskHealthCheck
        /// </summary>
        /// <param name="name">Task name</param>
        public WindowsScheduledTaskHealthCheck(string name, string machineName, string username, string password, string domain)
        {
            Name = name;
            MachineName = machineName;
            Username = username;
            Password = password;
            Domain = domain;
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
                //IIS application user is not a local admin account and thus cannot list tasks created by other users.
                //To avoid that we have to impoersonate an admin user account by creating a custom TaskService.
                using (var taskService = new Microsoft.Win32.TaskScheduler.TaskService(MachineName, Username, Domain, Password))
                {
                    using (var task = taskService.GetTask(Name))
                    {
                        if (task == null)
                            result = HealthCheckResult.Unhealthy("Task not found");
                        else if (!task.Enabled)
                            result = HealthCheckResult.Degraded("Task is disabled since " + task.LastRunTime.ToString("g"));
                        else if (task.LastTaskResult != 0)
                            result = HealthCheckResult.Degraded("Error during last run: " + task.LastRunTime.ToString("g"));
                        else
                            result = HealthCheckResult.Healthy("Succeeded at " + task.LastRunTime.ToString("g"));
                    }
                }
            }
            catch (Exception ex)
            {
                result = HealthCheckResult.Unhealthy("Error when trying to check directory path. ", ex);
            }

            return Task.FromResult(result);
        }
    }

    /// <summary>
    /// WindowsScheduledTaskHealthCheck extensions
    /// </summary>
    public static class WindowsScheduledTaskHealthCheckExtensions
    {
        /// <summary>
        /// Add Windows scheduled task status health check
        /// </summary>
        /// <param name="builder">Health check builder</param>
        /// <param name="taskName">Name of the task</param>
        /// <param name="name">Name</param>
        /// <param name="failureStatus">Failure status</param>
        /// <param name="tags">Tags</param>
        /// <returns>Health check builder</returns>
        public static IHealthChecksBuilder AddWindowsScheduledTaskHealthCheck(this IHealthChecksBuilder builder, string taskName, string machineName, string username, string password, string domain, string name, HealthStatus? failureStatus, IEnumerable<string> tags)
        {
            var check = new HealthCheckRegistration(name, new WindowsScheduledTaskHealthCheck(taskName, machineName, username, password, domain), failureStatus, tags);
            return builder.Add(check);
        }
    }
}
