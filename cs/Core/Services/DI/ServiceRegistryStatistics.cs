using System;
using System.Collections.Generic;

namespace GameComposition.Core.Services.DI;

/// <summary>
/// Statistics about the service registry.
/// </summary>
public class ServiceRegistryStatistics
{
    /// <summary>
    /// Number of singleton services.
    /// </summary>
    public int SingletonCount { get; set; }

    /// <summary>
    /// Number of factory services.
    /// </summary>
    public int FactoryCount { get; set; }

    /// <summary>
    /// Number of scoped factory services.
    /// </summary>
    public int ScopedFactoryCount { get; set; }

    /// <summary>
    /// Number of disposable services.
    /// </summary>
    public int DisposableCount { get; set; }

    /// <summary>
    /// Number of health check services.
    /// </summary>
    public int HealthCheckCount { get; set; }

    /// <summary>
    /// List of all registered service types.
    /// </summary>
    public List<Type> RegisteredTypes { get; set; } = new();

    /// <summary>
    /// Total number of registered services.
    /// </summary>
    public int TotalServices => RegisteredTypes.Count;
}
