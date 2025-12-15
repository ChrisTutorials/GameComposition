namespace GameComposition.Core.Services.DI;

/// <summary>
/// Interface for service health monitoring.
/// Services can implement this to provide health status information.
/// </summary>
public interface IServiceHealthCheck
{
    /// <summary>
    /// Checks the health of the service.
    /// </summary>
    /// <returns>Health status result</returns>
    ServiceHealthStatus CheckHealth();
}
