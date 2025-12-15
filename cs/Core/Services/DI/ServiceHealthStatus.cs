using System;
using System.Collections.Generic;

namespace GameComposition.Core.Services.DI;

/// <summary>
/// Health status of a service.
/// </summary>
public class ServiceHealthStatus
{
    /// <summary>
    /// Whether the service is healthy.
    /// </summary>
    public bool IsHealthy { get; set; } = true;

    /// <summary>
    /// Health status message.
    /// </summary>
    public string Message { get; set; } = "Service is healthy";

    /// <summary>
    /// Optional additional data about the health check.
    /// </summary>
    public Dictionary<string, object> Data { get; set; } = new();

    /// <summary>
    /// Time when the health check was performed.
    /// </summary>
    public DateTime CheckTime { get; set; } = DateTime.UtcNow;
}
