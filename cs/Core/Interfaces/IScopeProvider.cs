using BarkMoon.GameComposition.Core.Services.DI;

namespace BarkMoon.GameComposition.Core.Interfaces
{
    /// <summary>
    /// Interface for objects (often root nodes or bootstrappers) that provide a dependency injection scope.
    /// Used by child components to resolve services from their hierarchy.
    /// </summary>
    public interface IScopeProvider
    {
        /// <summary>
        /// Gets the active service scope provided by this object.
        /// </summary>
        IServiceScope Scope { get; }
    }
}
