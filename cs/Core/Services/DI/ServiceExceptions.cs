using System;

namespace BarkMoon.GameComposition.Core.Services.DI;

/// <summary>
/// Exception thrown when a requested service is not registered in the service registry.
/// </summary>
public class ServiceNotRegisteredException : Exception
{
    /// <summary>
    /// Creates an exception indicating that <paramref name="serviceType"/> is not registered.
    /// </summary>
    /// <param name="serviceType">The missing service type.</param>
    /// <exception cref="System.ArgumentNullException">Thrown when serviceType is null</exception>
    public ServiceNotRegisteredException(Type serviceType) 
        : base($"Service '{serviceType?.Name ?? "null"}' is not registered. Check service configuration in the composition container.")
    {
        ArgumentNullException.ThrowIfNull(serviceType);
        ServiceType = serviceType;
    }

    /// <summary>
    /// Creates an exception indicating that <paramref name="serviceType"/> is not registered.
    /// </summary>
    /// <param name="serviceType">The missing service type.</param>
    /// <param name="message">The exception message.</param>
    /// <exception cref="System.ArgumentNullException">Thrown when serviceType is null</exception>
    public ServiceNotRegisteredException(Type serviceType, string message) 
        : base(message)
    {
        ArgumentNullException.ThrowIfNull(serviceType);
        ServiceType = serviceType;
    }

    /// <summary>
    /// Creates an exception indicating that <paramref name="serviceType"/> is not registered.
    /// </summary>
    /// <param name="serviceType">The missing service type.</param>
    /// <param name="message">The exception message.</param>
    /// <param name="innerException">The inner exception.</param>
    /// <exception cref="System.ArgumentNullException">Thrown when serviceType is null</exception>
    public ServiceNotRegisteredException(Type serviceType, string message, Exception innerException) 
        : base(message, innerException)
    {
        ArgumentNullException.ThrowIfNull(serviceType);
        ServiceType = serviceType;
    }

    /// <summary>
    /// The requested service type that was not registered.
    /// </summary>
    public Type ServiceType { get; }
}

/// <summary>
/// Exception thrown when there's an error during service registration or creation.
/// </summary>
public class ServiceRegistrationException : Exception
{
    /// <summary>
    /// Creates an exception indicating service registration or creation failed.
    /// </summary>
    /// <param name="message">The exception message.</param>
    public ServiceRegistrationException(string message) : base(message) { }
    
    /// <summary>
    /// Creates an exception indicating service registration or creation failed.
    /// </summary>
    /// <param name="message">The exception message.</param>
    /// <param name="innerException">The inner exception.</param>
    public ServiceRegistrationException(string message, Exception innerException) 
        : base(message, innerException) { }
}
