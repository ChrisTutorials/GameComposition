using BarkMoon.GameComposition.Core.Types;
using System;

namespace BarkMoon.GameComposition.Core.Interfaces
{
    /// <summary>
    /// Base interface for all snapshot data.
    /// Snapshots provide immutable, point-in-time views of service state.
    /// Each concrete snapshot type exposes its own state via explicit properties.
    /// </summary>
    /// <remarks>
    /// Design Decision: Non-generic base interface with concrete snapshot implementations.
    /// This eliminates confusing generic constraints and allows each snapshot to be self-documenting.
    /// Services return their specific snapshot type (e.g., PlacementSnapshot2D) directly.
    /// </remarks>
    public interface ISnapshot
    {
        /// <summary>
        /// Gets the timestamp when this snapshot was created.
        /// </summary>
        DateTime SnapshotTimestamp { get; }

        /// <summary>
        /// Gets the state version at the time of snapshot creation.
        /// Used for change detection and cache invalidation.
        /// </summary>
        double StateVersion { get; }

        /// <summary>
        /// Gets whether this snapshot represents a valid state.
        /// </summary>
        bool IsValid { get; }

        /// <summary>
        /// Gets the composition data associated with this snapshot.
        /// This allows for extensible attributes without modifying the core snapshot structure.
        /// </summary>
        ICompositionData? Data { get; }
    }
}
