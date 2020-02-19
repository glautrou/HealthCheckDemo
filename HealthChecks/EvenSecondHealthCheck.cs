using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace HealthCheckDemo.HealthChecks
{
    public class EvenSecondHealthCheck : IHealthCheck
    {
        public Task<HealthCheckResult> CheckHealthAsync(
            HealthCheckContext context,
            CancellationToken cancellationToken = default(CancellationToken))
        {
            var isOdd = DateTime.Now.Second % 2 == 0;

            var result = isOdd
                ? HealthCheckResult.Healthy("Current second was even")
                : HealthCheckResult.Unhealthy("Current second was odd");

            return Task.FromResult(result);
        }
    }
}
