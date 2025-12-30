namespace BarkMoon.GameComposition.Core.Services.DI;

/// <summary>
/// Defines the lifetime of a registered service.
/// </summary>
public enum ServiceLifetime
{
    /// <summary>
    /// Single instance shared across all requests.
    /// Use for infrastructure services and stateless objects.
    /// </summary>
    Singleton,

    /// <summary>
    /// New instance created per request from factory.
    /// Use for stateless services that can be safely recreated.
    /// </summary>
    Transient,

    /// <summary>
    /// New instance created per scope/session.
    /// Use for services that need isolation (multiplayer scenarios).
    /// </summary>
    Scoped
}
