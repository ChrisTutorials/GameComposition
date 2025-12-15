using System;

namespace GameComposition.Core.Services.DI;

/// <summary>
/// Exception thrown when a requested service is not registered in the service registry.
/// </summary>
public class ServiceNotRegisteredException : Exception
{
    /// <summary>
    /// Creates an exception indicating that <paramref name="serviceType"/> is not registered.
    /// </summary>
    /// <param name="serviceType">The missing service type.</param>
    public ServiceNotRegisteredException(Type serviceType) 
        : base($"Service '{serviceType.Name}' is not registered. Check service configuration in the composition container.")
    {
        ServiceType = serviceType;
    }

    /// <summary>
    /// Creates an exception indicating that <paramref name="serviceType"/> is not registered.
    /// </summary>
    /// <param name="serviceType">The missing service type.</param>
    /// <param name="message">The exception message.</param>
    public ServiceNotRegisteredException(Type serviceType, string message) 
        : base(message)
    {
        ServiceType = serviceType;
    }

    /// <summary>
    /// Creates an exception indicating that <paramref name="serviceType"/> is not registered.
    /// </summary>
    /// <param name="serviceType">The missing service type.</param>
    /// <param name="message">The exception message.</param>
    /// <param name="innerException">The inner exception.</param>
    public ServiceNotRegisteredException(Type serviceType, string message, Exception innerException) 
        : base(message, innerException)
    {
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
