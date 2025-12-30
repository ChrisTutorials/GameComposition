using System.Collections.Generic;

namespace BarkMoon.GameComposition.Core.Results
{
    /// <summary>
    /// Represents the result of a framework-level operation.
    /// Provides success/failure status and optional error information.
    /// </summary>
    public class OperationResult
    {
        public bool IsSuccess { get; protected set; }
        public bool IsFailure => !IsSuccess;
        public string ErrorMessage { get; protected set; }
        public string ErrorCode { get; protected set; }
        public Dictionary<string, object> ErrorDetails { get; protected set; }

        public static OperationResult Success() => new OperationResult { IsSuccess = true };
        
        public static OperationResult Failure(string errorMessage, string errorCode = null, Dictionary<string, object> errorDetails = null)
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
    /// </summary>
    public class OperationResult<T> : OperationResult
    {
        public T Value { get; protected set; }

        public static OperationResult<T> Success(T value) => new OperationResult<T> { IsSuccess = true, Value = value };
        
        public new static OperationResult<T> Failure(string errorMessage, string errorCode = null, Dictionary<string, object> errorDetails = null)
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

    /// <summary>
    /// Represents validation results for framework and domains.
    /// </summary>
    public class ValidationResult
    {
        public bool IsValid { get; protected set; }
        public List<ValidationError> Errors { get; protected set; } = new();
        public List<string> Warnings { get; protected set; } = new();
        public List<string> Notes { get; protected set; } = new();
        public Dictionary<string, object> Context { get; protected set; } = new();

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
