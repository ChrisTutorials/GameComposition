using System;

namespace BarkMoon.GameComposition.Core.Interfaces
{
    /// <summary>
    /// Base interface for all Data Transfer Objects (DTOs) in the GameComposition framework.
    /// 
    /// PURPOSE:
    /// DTOs provide immutable data contracts between State objects and Snapshot objects,
    /// ensuring clean separation between mutable state and immutable snapshots.
    /// 
    /// ARCHITECTURAL PATTERN:
    /// State → CreateDto() → DTO → Snapshot Constructor → Immutable Snapshot
    /// 
    /// USAGE:
    /// - State classes implement CreateDto() methods that return DTOs
    /// - Snapshot constructors accept DTOs as parameters
    /// - DTOs contain only immutable data (records, structs, immutable classes)
    /// - DTOs never reference State or Snapshot types directly
    /// 
    /// ENFORCEMENT:
    /// This pattern is enforced by DtoPatternArchitectureTests and CrossDomainSnapshotArchitectureTests.
    /// See those test files for complete documentation and examples.
    /// 
    /// EXAMPLES:
    /// <code>
    /// // State creates DTO
    /// public PlacementDataDto CreateDto() => new(_entries, _metadata);
    /// 
    /// // Snapshot accepts DTO
    /// public PlacementSnapshot2D(PlacementDataDto data) { ... }
    /// </code>
    /// </summary>
    public interface IDataTransferObject
    {
        /// <summary>
        /// Gets the timestamp when this DTO was created.
        /// Used for tracking data freshness and debugging.
        /// </summary>
        DateTime CreatedAt { get; }
        
        /// <summary>
        /// Gets the data version of this DTO.
        /// Helps detect schema changes and compatibility issues.
        /// </summary>
        int DataVersion { get; }
        
        /// <summary>
        /// Gets whether this DTO contains valid data.
        /// Invalid DTOs should be rejected by snapshot constructors.
        /// </summary>
        bool IsValid { get; }
    }
}
