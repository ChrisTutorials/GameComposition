using System.Collections.Generic;
using System.Linq;

namespace BarkMoon.GameComposition.Core.Results
{
    /// <summary>
    /// Represents the result of a framework-level operation.
    /// Provides success/failure status and optional error information.
    /// Implements IOperationResult for cross-plugin compatibility.
    /// </summary>
    public class OperationResult : IOperationResult
    {
        private static readonly IReadOnlyList<string> EmptyIssues = new List<string>().AsReadOnly();
        
        public bool IsSuccess { get; protected set; }
        public bool IsFailure => !IsSuccess;
        public string? ErrorMessage { get; protected set; }
        public string? ErrorCode { get; protected set; }
        public Dictionary<string, object> ErrorDetails { get; protected set; } = new();
        
        /// <summary>
        /// Returns issues as a read-only list. For simple OperationResult, contains ErrorMessage if present.
        /// </summary>
        public virtual IReadOnlyList<string> Issues => 
            string.IsNullOrEmpty(ErrorMessage) ? EmptyIssues : new List<string> { ErrorMessage }.AsReadOnly();

        public static OperationResult Success() => new OperationResult { IsSuccess = true };
        
        public static OperationResult Failure(string errorMessage, string? errorCode = null, Dictionary<string, object>? errorDetails = null)
        {
            return new OperationResult
            {
                IsSuccess = false,
                ErrorMessage = errorMessage,
                ErrorCode = errorCode,
                ErrorDetails = errorDetails ?? new Dictionary<string, object>()
            };
        }
    }

    /// <summary>
    /// Represents the result of an operation with a generic return value.
    /// Implements IOperationResult{T} for cross-plugin compatibility.
    /// </summary>
    /// <remarks>
    /// Static factory methods are intentionally on the generic type for discoverability
    /// and type safety, following established .NET patterns like Task.FromResult().
    /// </remarks>
    #pragma warning disable CA1000 // Static members on generic types are intentional for factory patterns
    public class OperationResult<T> : OperationResult, IOperationResult<T>
    {
        public T? Value { get; protected set; }

        public static OperationResult<T> Success(T value) => new OperationResult<T> { IsSuccess = true, Value = value };
        
        public new static OperationResult<T> Failure(string errorMessage, string? errorCode = null, Dictionary<string, object>? errorDetails = null)
        {
            return new OperationResult<T>
            {
                IsSuccess = false,
                ErrorMessage = errorMessage,
                ErrorCode = errorCode,
                ErrorDetails = errorDetails ?? new Dictionary<string, object>()
            };
        }
    }
#pragma warning restore CA1000

    /// <summary>
    /// Represents validation results for framework and domains.
    /// Implements IValidationResult for cross-plugin compatibility.
    /// </summary>
    public class ValidationResult : IValidationResult
    {
        public bool IsValid { get; protected set; }
        protected List<ValidationError> Errors { get; set; } = new();
        protected List<string> Warnings { get; set; } = new();
        protected List<string> Notes { get; set; } = new();
        protected Dictionary<string, object> Context { get; set; } = new();

        // IValidationResult explicit interface implementation
        IReadOnlyList<ValidationError> IValidationResult.Errors => Errors.AsReadOnly();
        IReadOnlyList<string> IValidationResult.Warnings => Warnings.AsReadOnly();
        IReadOnlyList<string> IValidationResult.Notes => Notes.AsReadOnly();
        IReadOnlyDictionary<string, object> IValidationResult.Context => Context.AsReadOnly();

        // IOperationResult implementation
        public bool IsSuccess => IsValid;
        public IReadOnlyList<string> Issues => 
            Errors.Select(e => e.Message).Concat(Warnings).Concat(Notes).ToList().AsReadOnly();

        public static ValidationResult Success() => new ValidationResult { IsValid = true };
        
        public static ValidationResult Failure(params ValidationError[] errors)
        {
            var result = new ValidationResult { IsValid = false };
            result.Errors.AddRange(errors);
            return result;
        }

        public static ValidationResult Failure(string message)
        {
            var result = new ValidationResult { IsValid = false };
            result.AddError(message);
            return result;
        }

        public void AddError(string message, string code = null, string property = null)
        {
            IsValid = false;
            Errors.Add(new ValidationError { Message = message, Code = code, PropertyName = property });
        }

        public void AddWarning(string message)
        {
            Warnings.Add(message);
        }

        public void AddNote(string message)
        {
            Notes.Add(message);
        }
    }

    /// <summary>
    /// Represents a specific validation error.
    /// </summary>
    public class ValidationError
    {
        public string Code { get; set; }
        public string Message { get; set; }
        public string PropertyName { get; set; }
        public object AttemptedValue { get; set; }
    }
}
