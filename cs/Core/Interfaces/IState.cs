using System;

namespace BarkMoon.GameComposition.Core.Interfaces
{
    /// <summary>
    /// Interface for pure data state objects.
    /// State contains only data properties - NO methods, events, or service logic.
    /// Services own the state and provide snapshots for external access.
    /// 
    /// DTO PATTERN:
    /// State objects implement CreateDto() methods to provide data for Snapshot construction.
    /// This ensures clean separation between mutable State and immutable Snapshot objects.
    /// 
    /// USAGE:
    /// <code>
    /// // State creates DTO for snapshot
    /// public PlacementDataDto CreateDto() => new(_entries, _metadata);
    /// 
    /// // Service uses DTO to create snapshot
    /// var snapshot = new PlacementSnapshot2D(state.CreateDto());
    /// </code>
    /// 
    /// ENFORCEMENT:
    /// See DtoPatternArchitectureTests for complete DTO pattern documentation.
    /// All State implementations must provide CreateDto() methods.
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
    /// Generic interface for State objects that can create DTOs.
    /// This extends IState with the DTO pattern capability.
    /// 
    /// PURPOSE:
    /// Provides type-safe DTO creation for State objects.
    /// Implement this interface when your State needs to create specific DTO types.
    /// 
    /// USAGE:
    /// <code>
    /// public class PlacementState : IState&lt;PlacementDataDto&gt;
    /// {
    ///     public PlacementDataDto CreateDto() => new(_entries, _metadata);
    /// }
    /// </code>
    /// </summary>
    /// <typeparam name="TDto">The DTO type this State can create</typeparam>
    public interface IState<TDto> : IState where TDto : IDataTransferObject
    {
        /// <summary>
        /// Creates a Data Transfer Object containing the current state data.
        /// The DTO should contain all data needed to reconstruct the state in a snapshot.
        /// 
        /// IMPLEMENTATION REQUIREMENTS:
        /// - Must return a valid DTO (IsValid = true)
        /// - Must include all necessary state data
        /// - Must not include references to State or Snapshot objects
        /// - Should be immutable and thread-safe
        /// 
        /// ENFORCEMENT:
        /// DtoPatternArchitectureTests validates CreateDto() implementation.
        /// </summary>
        /// <returns>A DTO containing the current state data</returns>
        TDto CreateDto();
    }
}
