using System;
using System.Collections.Generic;

namespace BarkMoon.GameComposition.Core.Interfaces
{
    /// <summary>
    /// Interface for secure state data access.
    /// State classes must implement this to provide safe data for snapshot creation.
    /// This prevents direct exposure of internal mutable state structures.
    /// </summary>
    public interface ISecureStateDataProvider
    {
        /// <summary>
        /// Provides safe access to state data for snapshot creation.
        /// Returns immutable data tuples without exposing internal structures.
        /// Each plugin should implement this with their specific data types.
        /// </summary>
        /// <returns>Enumeration of immutable state data.</returns>
        IEnumerable<object> GetSnapshotData();
    }
}
