using BarkMoon.GameComposition.Core.Types;

namespace BarkMoon.GameComposition.Core.Interfaces
{
    /// <summary>
    /// Minimal orchestrator interface for Service-Based Architecture.
    /// Orchestrators coordinate services and event bus, following ownership principles.
    /// </summary>
    public interface IOrchestrator
    {
        /// <summary>
        /// Gets the unique identifier for this orchestrator instance.
        /// </summary>
        OrchestratorId OrchestratorId { get; }
    }
}
