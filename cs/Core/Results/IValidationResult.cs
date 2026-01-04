using System.Collections.Generic;

namespace BarkMoon.GameComposition.Core.Results
{
    /// <summary>
    /// Interface contract for validation results across the BarkMoon ecosystem.
    /// Extends IOperationResult to provide validation-specific semantics.
    /// 
    /// Usage:
    /// - Framework: ValidationResult implements IValidationResult
    /// - Plugins: PlacementReport : ValidationResult (inherits IValidationResult)
    /// </summary>
    public interface IValidationResult : IOperationResult
    {
        /// <summary>
        /// Indicates whether validation passed successfully.
        /// Use !IsValid for validation failure checks.
        /// </summary>
        bool IsValid { get; }

        /// <summary>
        /// Collection of validation errors.
        /// Empty if validation succeeded without errors.
        /// </summary>
        IReadOnlyList<ValidationError> Errors { get; }

        /// <summary>
        /// Collection of validation warnings.
        /// Warnings indicate potential issues that don't prevent success.
        /// </summary>
        IReadOnlyList<string> Warnings { get; }

        /// <summary>
        /// Collection of validation notes for additional context.
        /// Notes provide supplementary information without affecting validation outcome.
        /// </summary>
        IReadOnlyList<string> Notes { get; }

        /// <summary>
        /// Additional context information for validation.
        /// Provides metadata about the validation scenario.
        /// </summary>
        IReadOnlyDictionary<string, object> Context { get; }
    }
}
