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

    /// <summary>
    /// Generic snapshot interface for typed state access.
    /// Provides strongly-typed access to state data while maintaining immutability.
    /// </summary>
    /// <typeparam name="TState">The type of state this snapshot represents.</typeparam>
    public interface ISnapshot<TState> : ISnapshot where TState : IState
    {
        /// <summary>
        /// Gets the timestamp when this snapshot was created.
        /// Used for change detection and cache invalidation.
        /// </summary>
        DateTime SnapshotTimestamp { get; }
        
        /// <summary>
        /// Gets the state version at the time of snapshot creation.
        /// Used to detect if the underlying state has changed.
        /// </summary>
        double StateVersion { get; }
        
        /// <summary>
        /// Gets the immutable state data from this snapshot.
        /// </summary>
        new TState Data { get; }
    }
}
