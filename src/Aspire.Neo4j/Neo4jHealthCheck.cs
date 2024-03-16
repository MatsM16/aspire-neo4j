using Neo4j.Driver;
using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace Aspire.Neo4j;

internal sealed class Neo4jHealthCheck(IDriver driver) : IHealthCheck
{
    public async Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = default)
    {
        try
        {
            await driver.VerifyConnectivityAsync();
            return HealthCheckResult.Healthy();
        }
        catch (Exception exception)
        {
            return new HealthCheckResult(context.Registration.FailureStatus, exception:exception);
        }
    }
}
