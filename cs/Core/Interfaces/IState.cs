using System;

namespace BarkMoon.GameComposition.Core.Interfaces
{
    /// <summary>
    /// Interface for pure data state objects.
    /// State contains only data properties - NO methods, events, or service logic.
    /// Services own the state and provide snapshots for external access.
    /// </summary>
    public interface IState
    {
        /// <summary>
        /// Gets the timestamp of the last state update.
        /// Used for change detection and cache invalidation.
        /// </summary>
        double LastUpdated { get; }
        
        /// <summary>
        /// Gets whether the state is currently ready for use.
        /// Indicates if all required dependencies and data are properly initialized.
        /// </summary>
        bool IsReady { get; }
    }
}
