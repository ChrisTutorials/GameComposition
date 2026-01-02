using BarkMoon.GameComposition.Core.Types;

namespace BarkMoon.GameComposition.Core.Interfaces;

/// <summary>
/// Interface for immutable state snapshots.
/// Provides a unified contract for accessing snapshot data and validity.
/// </summary>
public interface ISnapshot
{
    /// <summary>
    /// Gets the composition data associated with this snapshot.
    /// This allows for extensible attributes without modifying the core snapshot structure.
    /// </summary>
    ICompositionData? Data { get; }
    
    /// <summary>
    /// Gets whether this snapshot represents a valid state.
    /// </summary>
    bool IsValid { get; }
}
