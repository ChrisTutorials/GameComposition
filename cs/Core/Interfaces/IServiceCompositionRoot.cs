namespace GameComposition.Core.Interfaces;

/// <summary>
/// Minimal composition root surface used to access services created/configured by a host.
/// </summary>
public interface IServiceCompositionRoot
{
    /// <summary>
    /// Resolves a service instance from the composition root.
    /// </summary>
    /// <typeparam name="T">The service type to resolve.</typeparam>
    /// <returns>The service instance, or <see langword="null"/> when the service is not available.</returns>
    T? GetService<T>() where T : class;

    /// <summary>
    /// Returns the underlying service registry object for advanced integration scenarios.
    /// </summary>
    /// <returns>The service registry instance as an opaque object.</returns>
    object GetServiceRegistry();
}
