using System;
using System.Collections.Generic;
using System.Linq;
using System.Diagnostics;
using BarkMoon.GameComposition.Core.Interfaces;

namespace BarkMoon.GameComposition.Core.Services.DI;

/// <summary>
/// Core service registry - pure C# implementation with no Godot dependencies.
/// Supports singleton, factory, and scoped service patterns for cross-platform compatibility.
/// Enhanced with lifetime management, health checks, and validation.
/// </summary>
public class ServiceRegistry : IDisposable
{
    private readonly Dictionary<Type, object> _singletons = new();
    private readonly Dictionary<Type, Func<object>> _factories = new();
    private readonly Dictionary<Type, Func<IServiceScope, object>> _scopedFactories = new();
    private readonly Dictionary<Type, ServiceLifetime> _lifetimes = new();
    private readonly List<IDisposable> _disposables = new();
    private readonly List<IServiceHealthCheck> _healthChecks = new();
    private bool _disposed;

    private void ThrowIfDisposed()
    {
        ObjectDisposedException.ThrowIf(_disposed, nameof(ServiceRegistry));
    }

    /// <summary>
    /// Registers a singleton service instance that will be shared across all requests.
    /// Use sparingly - primarily for infrastructure and read-only services.
    /// </summary>
    /// <typeparam name="TService">Service interface type</typeparam>
    /// <typeparam name="TImplementation">Implementation type</typeparam>
    /// <param name="instance">Service instance</param>
    public void RegisterSingleton<TService, TImplementation>(TImplementation instance) 
        where TService : class 
        where TImplementation : class, TService
    {
        ThrowIfDisposed();
        var serviceType = typeof(TService);
        
        if (_singletons.ContainsKey(serviceType))
        {
            throw new ServiceRegistrationException($"Singleton service {serviceType.Name} is already registered");
        }

        _singletons[serviceType] = instance;
        _lifetimes[serviceType] = ServiceLifetime.Singleton;
        
        // Track disposable services for cleanup
        if (instance is IDisposable disposable)
        {
            _disposables.Add(disposable);
        }

        // Register health check if service supports it
        if (instance is IServiceHealthCheck healthCheck)
        {
            _healthChecks.Add(healthCheck);
        }
    }

    /// <summary>
    /// Registers a singleton service instance that will be shared across all requests.
    /// Use sparingly - primarily for infrastructure and read-only services.
    /// </summary>
    /// <typeparam name="T">Service type</typeparam>
    /// <param name="instance">Service instance</param>
    public void RegisterSingleton<T>(T instance) where T : class
    {
        ThrowIfDisposed();
        var serviceType = typeof(T);
        
        if (_singletons.ContainsKey(serviceType))
        {
            throw new ServiceRegistrationException($"Singleton service {serviceType.Name} is already registered");
        }

        _singletons[serviceType] = instance;
        _lifetimes[serviceType] = ServiceLifetime.Singleton;
        
        // Track disposable services for cleanup
        if (instance is IDisposable disposable)
        {
            _disposables.Add(disposable);
        }

        // Register health check if service supports it
        if (instance is IServiceHealthCheck healthCheck)
        {
            _healthChecks.Add(healthCheck);
        }
    }

    /// <summary>
    /// Registers a factory function for stateless services that can be safely shared.
    /// Creates a new instance on each request but uses the same factory function.
    /// </summary>
    /// <typeparam name="T">Service type</typeparam>
    /// <param name="factory">Factory function</param>
    public void RegisterFactory<T>(Func<T> factory) where T : class
    {
        ThrowIfDisposed();
        var serviceType = typeof(T);
        
        if (_factories.ContainsKey(serviceType) || _scopedFactories.ContainsKey(serviceType))
        {
            throw new ServiceRegistrationException($"Factory service {serviceType.Name} is already registered");
        }

        _factories[serviceType] = () => factory();
        _lifetimes[serviceType] = ServiceLifetime.Transient;
    }

    /// <summary>
    /// Registers a scoped service factory for per-player or per-session instances.
    /// Creates a new instance on each request, ensuring isolation in multiplayer scenarios.
    /// </summary>
    /// <typeparam name="T">Service type</typeparam>
    /// <param name="factory">Factory function</param>
    public void RegisterScoped<T>(Func<T> factory) where T : class
    {
        ThrowIfDisposed();
        var serviceType = typeof(T);
        
        if (_factories.ContainsKey(serviceType) || _scopedFactories.ContainsKey(serviceType))
        {
            throw new ServiceRegistrationException($"Scoped service {serviceType.Name} is already registered");
        }

        _scopedFactories[serviceType] = (scope) => factory();
        _lifetimes[serviceType] = ServiceLifetime.Scoped;
    }

    /// <summary>
    /// Registers a scoped service factory that uses the current scope to resolve dependencies.
    /// </summary>
    /// <typeparam name="T">Service type</typeparam>
    /// <param name="factory">Factory function receiving the scope</param>
    public void RegisterScoped<T>(Func<IServiceScope, T> factory) where T : class
    {
        ThrowIfDisposed();
        var serviceType = typeof(T);
        
        if (_factories.ContainsKey(serviceType) || _scopedFactories.ContainsKey(serviceType))
        {
            throw new ServiceRegistrationException($"Scoped service {serviceType.Name} is already registered");
        }

        _scopedFactories[serviceType] = (scope) => factory(scope);
        _lifetimes[serviceType] = ServiceLifetime.Scoped;
    }

    /// <summary>
    /// Registers a scoped service with auto-wiring.
    /// Dependencies will be resolved from the active scope.
    /// </summary>
    /// <typeparam name="T">Service type</typeparam>
    public void RegisterScoped<T>() where T : class
    {
        ThrowIfDisposed();
        var serviceType = typeof(T);
        
        if (_factories.ContainsKey(serviceType) || _scopedFactories.ContainsKey(serviceType))
        {
            throw new ServiceRegistrationException($"Service {serviceType.Name} is already registered");
        }

        _scopedFactories[serviceType] = (scope) => CreateInstanceWithScope(serviceType, scope);
        _lifetimes[serviceType] = ServiceLifetime.Scoped;
    }

    /// <summary>
    /// Registers a scoped service implementation for an interface with auto-wiring.
    /// </summary>
    /// <typeparam name="TService">Service interface</typeparam>
    /// <typeparam name="TImplementation">Service implementation</typeparam>
    public void RegisterScoped<TService, TImplementation>() 
        where TService : class 
        where TImplementation : class, TService
    {
        ThrowIfDisposed();
        var serviceType = typeof(TService);
        var implementationType = typeof(TImplementation);
        
        if (_factories.ContainsKey(serviceType) || _scopedFactories.ContainsKey(serviceType))
        {
            throw new ServiceRegistrationException($"Service {serviceType.Name} is already registered");
        }

        _scopedFactories[serviceType] = (scope) => CreateInstanceWithScope(implementationType, scope);
        _lifetimes[serviceType] = ServiceLifetime.Scoped;
    }

    private static object CreateInstanceWithScope(Type type, IServiceScope scope)
    {
        var ctors = type.GetConstructors();
        if (ctors.Length == 0) throw new InvalidOperationException($"No public constructors for {type.Name}");
        
        // Pick first constructor (simple DI)
        var ctor = ctors[0];
        var parameters = ctor.GetParameters();
        var args = new object[parameters.Length];
        
        for (int i = 0; i < parameters.Length; i++)
        {
            args[i] = scope.GetService(parameters[i].ParameterType) 
                ?? throw new InvalidOperationException($"Could not resolve dependency {parameters[i].ParameterType.Name} for {type.Name}");
        }
        
        return ctor.Invoke(args);
    }

    /// <summary>
    /// Creates a scoped service instance.
    /// </summary>
    /// <param name="serviceType">Service type</param>
    /// <param name="scope">The active service scope</param>
    /// <returns>Service instance or null if not registered</returns>
    internal object? CreateScopedService(Type serviceType, IServiceScope scope)
    {
        ThrowIfDisposed();
        if (_scopedFactories.TryGetValue(serviceType, out var scopedFactory))
        {
            return scopedFactory(scope);
        }

        // Fall back to factory for scoped services
        if (_factories.TryGetValue(serviceType, out var factory))
        {
            return factory();
        }

        return null;
    }

    /// <summary>
    /// Gets a service instance with automatic resolution based on registration type.
    /// Singleton: Returns the shared instance
    /// Factory: Creates new instance using factory function
    /// Scoped: Creates new instance using scoped factory
    /// </summary>
    /// <typeparam name="T">Service type</typeparam>
    /// <returns>Service instance</returns>
    /// <exception cref="ServiceNotRegisteredException">Thrown when service is not registered</exception>
    public T GetService<T>() where T : class
    {
        ThrowIfDisposed();
        var serviceType = typeof(T);

        // Check singleton services first
        if (_singletons.TryGetValue(serviceType, out var singletonInstance))
        {
            return (T)singletonInstance;
        }

        // Check factory services
        if (_factories.TryGetValue(serviceType, out var factory))
        {
            try
            {
                return (T)factory();
            }
            catch (Exception ex)
            {
                throw new ServiceRegistrationException($"Factory for {serviceType.Name} failed to create instance", ex);
            }
        }

        // Check scoped services
        if (_scopedFactories.TryGetValue(serviceType, out var scopedFactory))
        {
            try
            {
                return (T)scopedFactory(null!);
            }
            catch (Exception ex)
            {
                throw new ServiceRegistrationException($"Scoped factory for {serviceType.Name} failed to create instance", ex);
            }
        }

        throw new ServiceNotRegisteredException(serviceType);
    }

    /// <summary>
    /// Tries to get a service instance.
    /// </summary>
    /// <typeparam name="T">Service type</typeparam>
    /// <param name="service">Service instance or null</param>
    /// <returns>True if service was found</returns>
    public bool TryGetService<T>(out T? service) where T : class
    {
        ThrowIfDisposed();
        var serviceType = typeof(T);
        service = null;

        if (_singletons.TryGetValue(serviceType, out var singletonInstance))
        {
            service = (T)singletonInstance;
            return true;
        }

        if (_factories.TryGetValue(serviceType, out var factory))
        {
            try
            {
                service = (T)factory();
                return true;
            }
            catch (Exception ex) when (ex is InvalidOperationException or TypeAccessException)
            {
                return false;
            }
        }

        if (_scopedFactories.TryGetValue(serviceType, out var scopedFactory))
        {
            try
            {
                service = (T)scopedFactory(null!);
                return true;
            }
            catch (Exception ex) when (ex is InvalidOperationException or TypeAccessException)
            {
                return false;
            }
        }

        return false;
    }

    /// <summary>
    /// Checks if a service type is registered.
    /// </summary>
    /// <param name="serviceType">Service type to check</param>
    /// <returns>True if service is registered, false otherwise</returns>
    public bool IsRegistered(Type serviceType)
    {
        ThrowIfDisposed();
        return _singletons.ContainsKey(serviceType) || 
               _factories.ContainsKey(serviceType) || 
               _scopedFactories.ContainsKey(serviceType);
    }

    /// <summary>
    /// Checks if a service type is registered.
    /// </summary>
    /// <typeparam name="T">Service type to check</typeparam>
    /// <returns>True if service is registered, false otherwise</returns>
    public bool IsRegistered<T>() where T : class
    {
        ThrowIfDisposed();
        var serviceType = typeof(T);
        return _singletons.ContainsKey(serviceType) || 
               _factories.ContainsKey(serviceType) || 
               _scopedFactories.ContainsKey(serviceType);
    }

    /// <summary>
    /// Gets all registered singleton services for debugging or testing purposes.
    /// </summary>
    /// <returns>Dictionary of singleton services</returns>
    public Dictionary<Type, object> GetAllSingletons()
    {
        ThrowIfDisposed();
        return new Dictionary<Type, object>(_singletons);
    }
    /// <summary>
    /// Validates that required services are registered.
    /// </summary>
    /// <param name="requiredServices">Required service types</param>
    /// <exception cref="System.ArgumentNullException">Thrown when requiredServices is null</exception>
    public void ValidateServices(params Type[] requiredServices)
    {
        ArgumentNullException.ThrowIfNull(requiredServices);
        ThrowIfDisposed();
        var missingServices = new List<Type>();

        foreach (var serviceType in requiredServices)
        {
            if (!IsRegistered(serviceType))
            {
                missingServices.Add(serviceType);
            }
        }

        if (missingServices.Count > 0)
        {
            var missingNames = string.Join(", ", missingServices.Select(t => t.Name));
            throw new InvalidOperationException($"Critical services not registered: {missingNames}");
        }
    }

    /// <summary>
    /// Gets the service lifetime for a registered service type.
    /// </summary>
    /// <typeparam name="T">Service type</typeparam>
    /// <returns>Service lifetime</returns>
    public ServiceLifetime GetServiceLifetime<T>() where T : class
    {
        ThrowIfDisposed();
        var serviceType = typeof(T);
        return _lifetimes.GetValueOrDefault(serviceType, ServiceLifetime.Transient);
    }

    /// <summary>
    /// Performs health checks on all registered services that support health monitoring.
    /// </summary>
    /// <returns>Health check results</returns>
    public ServiceHealthCheckResult PerformHealthChecks()
    {
        ThrowIfDisposed();
        var result = new ServiceHealthCheckResult();
        
        foreach (var healthCheck in _healthChecks)
        {
            try
            {
                var checkResult = healthCheck.CheckHealth();
                result.ServiceResults.Add(healthCheck.GetType().Name, checkResult);
                
                if (!checkResult.IsHealthy)
                {
                    result.UnhealthyServices.Add(healthCheck.GetType().Name);
                }
            }
            catch (Exception ex) when (ex is InvalidOperationException or System.Reflection.ReflectionTypeLoadException)
            {
                result.ServiceResults.Add(healthCheck.GetType().Name, 
                    new ServiceHealthStatus { IsHealthy = false, Message = ex.Message });
                result.UnhealthyServices.Add(healthCheck.GetType().Name);
            }
        }
        
        result.IsHealthy = result.UnhealthyServices.Count == 0;
        return result;
    }

    // NOTE: Intentionally no deep graph validation in GameComposition.Core.
    // Composition is meant to be lightweight and engine-agnostic; host applications
    // can add their own validation layer if desired.

    /// <summary>
    /// Creates a new scope for scoped services (useful for multiplayer isolation).
    /// </summary>
    /// <returns>Service scope instance</returns>
    public IServiceScope CreateScope()
    {
        ThrowIfDisposed();
        return new ServiceScope(this);
    }

    /// <summary>
    /// Gets service statistics for monitoring and debugging.
    /// </summary>
    /// <returns>Service registry statistics</returns>
    public ServiceRegistryStatistics GetStatistics()
    {
        ThrowIfDisposed();
        return new ServiceRegistryStatistics
        {
            SingletonCount = _singletons.Count,
            FactoryCount = _factories.Count,
            ScopedFactoryCount = _scopedFactories.Count,
            DisposableCount = _disposables.Count,
            HealthCheckCount = _healthChecks.Count,
            RegisteredTypes = _singletons.Keys
                .Concat(_factories.Keys)
                .Concat(_scopedFactories.Keys)
                .ToList()
        };
    }

    /// <summary>
    /// Clears all registered services and disposes of disposable instances.
    /// Use this for cleanup during testing or when restarting the service registry.
    /// </summary>
    public void Clear()
    {
        ThrowIfDisposed();
        DisposeAll();
        _singletons.Clear();
        _factories.Clear();
        _scopedFactories.Clear();
        _lifetimes.Clear();
        _healthChecks.Clear();
    }


    /// <summary>
    /// Disposes all disposable services and clears the registry.
    /// </summary>
    protected virtual void Dispose(bool disposing)
    {
        if (_disposed)
        {
            return;
        }

        if (disposing)
        {
            DisposeAll();
        }

        _singletons.Clear();
        _factories.Clear();
        _scopedFactories.Clear();
        _lifetimes.Clear();
        _healthChecks.Clear();

        _disposed = true;
    }

    /// <summary>
    /// Disposes all disposable services and clears the registry.
    /// </summary>
    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    private void DisposeAll()
    {
        foreach (var disposable in _disposables)
        {
            try
            {
                disposable.Dispose();
            }
            catch (Exception ex) when (ex is ObjectDisposedException or InvalidOperationException)
            {
                // Log disposal errors but don't throw
                // In a real implementation, you'd use a logger here
                Trace.TraceError($"Error disposing service: {ex.Message}");
            }
        }
        _disposables.Clear();
    }
}
