using System.Collections.Generic;

namespace BarkMoon.GameComposition.Core.Results
{
    /// <summary>
    /// Interface contract for operation results across the BarkMoon ecosystem.
    /// All plugin result types should implement this interface for cross-plugin compatibility.
    /// 
    /// Usage:
    /// - Framework: OperationResult, ValidationResult
    /// - Plugins: PlacementResult, TargetingResult, TimeAdvanceResult, etc.
    /// </summary>
    public interface IOperationResult
    {
        /// <summary>
        /// Indicates whether the operation completed successfully.
        /// Use !IsSuccess for failure checks.
        /// </summary>
        bool IsSuccess { get; }

        /// <summary>
        /// Collection of all issues (errors, warnings, validation problems).
        /// Empty if operation succeeded without issues.
        /// Use Issues.FirstOrDefault() for single-error cases.
        /// </summary>
        IReadOnlyList<string> Issues { get; }
    }

    /// <summary>
    /// Interface for operation results that carry a value on success.
    /// </summary>
    /// <typeparam name="T">The type of the result value.</typeparam>
    public interface IOperationResult<out T> : IOperationResult
    {
        /// <summary>
        /// The result value if the operation succeeded.
        /// </summary>
        T? Value { get; }
    }
}
