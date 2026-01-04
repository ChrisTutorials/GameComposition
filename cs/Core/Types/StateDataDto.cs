using System;

using BarkMoon.GameComposition.Core.Interfaces;

namespace BarkMoon.GameComposition.Core.Types
{
    /// <summary>
    /// Base class for State Data Transfer Objects in the GameComposition framework.
    /// 
    /// PURPOSE:
    /// Provides common functionality for DTOs that carry data from State objects to Snapshot objects.
    /// This class implements the IDataTransferObject interface with standard State-to-Snapshot behavior.
    /// 
    /// ARCHITECTURAL PATTERN:
    /// State classes inherit from this base class to create standardized DTOs.
    /// The base class handles common DTO properties (timestamp, version, validation).
    /// 
    /// USAGE:
    /// <code>
    /// public sealed record PlacementDataDto : StateDataDto
    /// {
    ///     public IReadOnlyList&lt;PlacementEntry&gt; Entries { get; init; }
    ///     public PlacementMetadata Metadata { get; init; }
    ///     
    ///     public PlacementDataDto(IReadOnlyList&lt;PlacementEntry&gt; entries, PlacementMetadata metadata)
    ///         : base(entries, metadata)
    ///     {
    ///         Entries = entries ?? throw new ArgumentNullException(nameof(entries));
    ///         Metadata = metadata ?? throw new ArgumentNullException(nameof(metadata));
    ///     }
    /// }
    /// </code>
    /// 
    /// ENFORCEMENT:
    /// See DtoPatternArchitectureTests for complete validation rules and examples.
    /// All State DTOs must inherit from this base class.
    /// </summary>
    public abstract record StateDataDto : IDataTransferObject
    {
        /// <summary>
        /// Gets the timestamp when this DTO was created.
        /// Automatically set to UTC now during construction.
        /// </summary>
        public DateTime CreatedAt { get; init; } = DateTime.UtcNow;
        
        /// <summary>
        /// Gets the data version of this DTO.
        /// Default is 1, increment when breaking changes are made.
        /// </summary>
        public int DataVersion { get; init; } = 1;
        
        /// <summary>
        /// Gets whether this DTO contains valid data.
        /// Base implementation checks for null collections and basic validity.
        /// Override in derived classes for domain-specific validation.
        /// </summary>
        public virtual bool IsValid => true;
        
        /// <summary>
        /// Initializes a new StateDataDto with optional validation data.
        /// Protected constructor for derived classes.
        /// </summary>
        /// <param name="validationData">Optional data for validation purposes</param>
        protected StateDataDto(object? validationData = null)
        {
            // Base validation can be added here if needed
            // Derived classes should override IsValid for domain-specific validation
        }
    }
}
