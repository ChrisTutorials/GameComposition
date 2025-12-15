using System;

namespace GameComposition.Core.Services.DI;

/// <summary>
/// Interface for service scoping (useful for multiplayer isolation).
/// </summary>
public interface IServiceScope : IDisposable
{
    /// <summary>
    /// Gets a service from the scope.
    /// </summary>
    /// <typeparam name="T">Service type</typeparam>
    /// <returns>Service instance</returns>
    T GetService<T>() where T : class;

    /// <summary>
    /// Gets a service from the scope by type.
    /// </summary>
    /// <param name="serviceType">Service type</param>
    /// <returns>Service instance</returns>
    object GetService(Type serviceType);
}
