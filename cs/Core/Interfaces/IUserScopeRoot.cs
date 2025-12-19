using GameComposition.Core.Services.DI;
using GameComposition.Core.Types;

namespace GameComposition.Core.Interfaces;

/// <summary>
/// Composition root for a specific user scope.
/// </summary>
public interface IUserScopeRoot : IServiceCompositionRoot
{
    /// <summary>
    /// The user identifier associated with this scope.
    /// </summary>
    UserId UserId { get; }

    /// <summary>
    /// The service registry backing this scope.
    /// </summary>
    ServiceRegistry ServiceRegistry { get; }
}
