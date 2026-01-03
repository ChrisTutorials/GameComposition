using System;
using System.Collections.Generic;

namespace BarkMoon.GameComposition.Core.Services.DI;

/// <summary>
/// Implementation of service scope for scoped services.
/// </summary>
/// <param name="registry">The registry used to create scoped service instances.</param>
public class ServiceScope(ServiceRegistry registry) : IServiceScope
{
    private readonly ServiceRegistry _registry = registry;
    private readonly Dictionary<Type, object> _scopedServices = [];
    private readonly List<IDisposable> _scopedDisposables = [];
    private bool _disposed;

    /// <summary>
    /// Resolves a scoped service instance of type <typeparamref name="T"/>.
    /// </summary>
    /// <typeparam name="T">The service type.</typeparam>
    /// <returns>The scoped service instance.</returns>
    public T GetService<T>() where T : class
    {
        var serviceType = typeof(T);
        return (T)GetService(serviceType);
    }

    /// <summary>
    /// Resolves a scoped service instance for the given <paramref name="serviceType"/>.
    /// </summary>
    /// <param name="serviceType">The service type.</param>
    /// <returns>The scoped service instance.</returns>
    /// <exception cref="System.ArgumentNullException">Thrown when serviceType is null</exception>
    public object GetService(Type serviceType)
    {
        ArgumentNullException.ThrowIfNull(serviceType);
        ObjectDisposedException.ThrowIf(_disposed, nameof(ServiceScope));

        if (_scopedServices.TryGetValue(serviceType, out var existing))
        {
            return existing;
        }

        var instance = _registry.CreateScopedService(serviceType, this)
            ?? throw new InvalidOperationException($"Scoped service {serviceType.Name} is not registered");

        _scopedServices[serviceType] = instance;

        if (instance is IDisposable disposable)
        {
            _scopedDisposables.Add(disposable);
        }

        return instance;
    }

    /// <summary>
    /// Disposes any scoped <see cref="IDisposable"/> services created by this scope.
    /// </summary>
    protected virtual void Dispose(bool disposing)
    {
        if (_disposed)
        {
            return;
        }

        if (disposing)
        {
            foreach (var disposable in _scopedDisposables)
            {
                try
                {
                    disposable.Dispose();
                }
                catch (ObjectDisposedException)
                {
                    // Swallow disposal errors; logging is handled elsewhere.
                }
                catch (InvalidOperationException)
                {
                    // Swallow disposal errors; logging is handled elsewhere.
                }
            }

            _scopedDisposables.Clear();
            _scopedServices.Clear();
        }

        _disposed = true;
    }

    /// <summary>
    /// Disposes any scoped <see cref="IDisposable"/> services created by this scope.
    /// </summary>
    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }
}
