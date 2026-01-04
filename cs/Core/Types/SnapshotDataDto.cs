using System;

using BarkMoon.GameComposition.Core.Interfaces;

namespace BarkMoon.GameComposition.Core.Types
{
    /// <summary>
    /// Base class for Snapshot Data Transfer Objects in the GameComposition framework.
    /// 
    /// PURPOSE:
    /// Provides common functionality for DTOs that are consumed by Snapshot objects.
    /// This class implements the IDataTransferObject interface with Snapshot-specific behavior.
    /// 
    /// ARCHITECTURAL PATTERN:
    /// Snapshot DTOs carry the data that snapshots need for their immutable state.
    /// These DTOs are created by State objects and consumed by Snapshot constructors.
    /// 
    /// USAGE:
    /// <code>
    /// // State creates snapshot DTO
    /// public TargetingDataDto CreateSnapshotDto() => new(IsReady, GridPosition, WorldPosition);
    /// 
    /// // Snapshot consumes DTO
    /// public TargetingSnapshot2D(TargetingDataDto data) : ISnapshot
    /// {
    ///     IsReady = data.IsReady;
    ///     GridPosition = data.GridPosition;
    ///     WorldPosition = data.WorldPosition;
    ///     SnapshotTimestamp = data.CreatedAt;
    /// }
    /// </code>
    /// 
    /// ENFORCEMENT:
    /// See DtoPatternArchitectureTests for complete validation rules and examples.
    /// All Snapshot DTOs must inherit from this base class.
    /// </summary>
    public abstract record SnapshotDataDto : IDataTransferObject
    {
        /// <summary>
        /// Gets the timestamp when this DTO was created.
        /// Used by snapshots to set their SnapshotTimestamp property.
        /// </summary>
        public DateTime CreatedAt { get; init; } = DateTime.UtcNow;
        
        /// <summary>
        /// Gets the data version of this DTO.
        /// Used for compatibility checking between State and Snapshot versions.
        /// </summary>
        public int DataVersion { get; init; } = 1;
        
        /// <summary>
        /// Gets whether this DTO contains valid data for snapshot creation.
        /// Snapshot constructors should reject DTOs where IsValid is false.
        /// </summary>
        public virtual bool IsValid => true;
        
        /// <summary>
        /// Initializes a new SnapshotDataDto.
        /// Protected constructor for derived classes.
        /// </summary>
        /// <param name="validationData">Optional data for validation purposes</param>
        protected SnapshotDataDto(object? validationData = null)
        {
            // Base validation can be added here if needed
            // Derived classes should override IsValid for domain-specific validation
        }
    }
}
