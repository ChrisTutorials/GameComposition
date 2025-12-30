using System;
using System.Collections.Generic;

namespace BarkMoon.GameComposition.Core.Services.DI;

/// <summary>
/// Results from performing health checks on multiple services.
/// </summary>
public class ServiceHealthCheckResult
{
    /// <summary>
    /// Overall health status (all services must be healthy).
    /// </summary>
    public bool IsHealthy { get; set; } = true;

    /// <summary>
    /// Individual service health results.
    /// </summary>
    public Dictionary<string, ServiceHealthStatus> ServiceResults { get; set; } = new();

    /// <summary>
    /// List of unhealthy service names.
    /// </summary>
    public List<string> UnhealthyServices { get; set; } = new();

    /// <summary>
    /// Time when the health checks were performed.
    /// </summary>
    public DateTime CheckTime { get; set; } = DateTime.UtcNow;
}
