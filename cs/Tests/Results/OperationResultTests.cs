using System;
using System.Collections.Generic;
using Xunit;
using BarkMoon.GameComposition.Core.Results;

namespace BarkMoon.GameComposition.Core.Tests.Results
{
    public class OperationResultTests
    {
        [Fact]
        public void OperationResult_Success_CreatesSuccessfulResult()
        {
            // Act
            var result = OperationResult.Success();

            // Assert
            Assert.True(result.IsSuccess);
            Assert.False(result.IsFailure);
            Assert.Null(result.ErrorMessage);
            Assert.Null(result.ErrorCode);
            Assert.Null(result.ErrorDetails);
        }

        [Fact]
        public void OperationResult_Failure_WithMessage_CreatesFailureResult()
        {
            // Arrange
            var errorMessage = "Test error message";

            // Act
            var result = OperationResult.Failure(errorMessage);

            // Assert
            Assert.False(result.IsSuccess);
            Assert.True(result.IsFailure);
            Assert.Equal(errorMessage, result.ErrorMessage);
            Assert.Null(result.ErrorCode);
            Assert.NotNull(result.ErrorDetails);
            Assert.Empty(result.ErrorDetails);
        }

        [Fact]
        public void OperationResult_Failure_WithMessageAndCode_CreatesFailureResult()
        {
            // Arrange
            var errorMessage = "Test error message";
            var errorCode = "TEST_ERROR";

            // Act
            var result = OperationResult.Failure(errorMessage, errorCode);

            // Assert
            Assert.False(result.IsSuccess);
            Assert.True(result.IsFailure);
            Assert.Equal(errorMessage, result.ErrorMessage);
            Assert.Equal(errorCode, result.ErrorCode);
            Assert.NotNull(result.ErrorDetails);
            Assert.Empty(result.ErrorDetails);
        }

        [Fact]
        public void OperationResult_Failure_WithDetails_CreatesFailureResult()
        {
            // Arrange
            var errorMessage = "Test error message";
            var errorCode = "TEST_ERROR";
            var errorDetails = new Dictionary<string, object>
            {
                ["Property1"] = "Value1",
                ["Property2"] = 42,
                ["Property3"] = true
            };

            // Act
            var result = OperationResult.Failure(errorMessage, errorCode, errorDetails);

            // Assert
            Assert.False(result.IsSuccess);
            Assert.True(result.IsFailure);
            Assert.Equal(errorMessage, result.ErrorMessage);
            Assert.Equal(errorCode, result.ErrorCode);
            Assert.Same(errorDetails, result.ErrorDetails);
            Assert.Equal(3, result.ErrorDetails.Count);
            Assert.Equal("Value1", result.ErrorDetails["Property1"]);
            Assert.Equal(42, result.ErrorDetails["Property2"]);
            Assert.True((bool)result.ErrorDetails["Property3"]);
        }

        [Fact]
        public void OperationResult_Failure_WithNullDetails_CreatesEmptyDetails()
        {
            // Arrange
            var errorMessage = "Test error message";

            // Act
            var result = OperationResult.Failure(errorMessage, null, null);

            // Assert
            Assert.False(result.IsSuccess);
            Assert.True(result.IsFailure);
            Assert.Equal(errorMessage, result.ErrorMessage);
            Assert.NotNull(result.ErrorDetails);
            Assert.Empty(result.ErrorDetails);
        }
    }

    public class OperationResultGenericTests
    {
        [Fact]
        public void OperationResultT_Success_WithValue_CreatesSuccessfulResult()
        {
            // Arrange
            var value = "Test Value";

            // Act
            var result = OperationResult<string>.Success(value);

            // Assert
            Assert.True(result.IsSuccess);
            Assert.False(result.IsFailure);
            Assert.Equal(value, result.Value);
            Assert.Null(result.ErrorMessage);
            Assert.Null(result.ErrorCode);
            Assert.Null(result.ErrorDetails);
        }

        [Fact]
        public void OperationResultT_Failure_CreatesFailureResult()
        {
            // Arrange
            var errorMessage = "Test error message";
            var errorCode = "TEST_ERROR";

            // Act
            var result = OperationResult<string>.Failure(errorMessage, errorCode);

            // Assert
            Assert.False(result.IsSuccess);
            Assert.True(result.IsFailure);
            Assert.Equal(default(string), result.Value);
            Assert.Equal(errorMessage, result.ErrorMessage);
            Assert.Equal(errorCode, result.ErrorCode);
            Assert.NotNull(result.ErrorDetails);
            Assert.Empty(result.ErrorDetails);
        }

        [Theory]
        [InlineData(42)]
        [InlineData("test")]
        [InlineData(true)]
        [InlineData(3.14)]
        public void OperationResultT_Success_WithVariousTypes_HandlesCorrectly<T>(T value)
        {
            // Act
            var result = OperationResult<T>.Success(value);

            // Assert
            Assert.True(result.IsSuccess);
            Assert.Equal(value, result.Value);
        }

        [Fact]
        public void OperationResultT_ValueType_Success_CreatesSuccessfulResult()
        {
            // Arrange
            var value = 123;

            // Act
            var result = OperationResult<int>.Success(value);

            // Assert
            Assert.True(result.IsSuccess);
            Assert.Equal(123, result.Value);
        }
    }

    public class ValidationResultTests
    {
        [Fact]
        public void ValidationResult_Success_CreatesValidResult()
        {
            // Act
            var result = ValidationResult.Success();

            // Assert
            Assert.True(result.IsValid);
            Assert.NotNull(result.Errors);
            Assert.Empty(result.Errors);
            Assert.NotNull(result.Warnings);
            Assert.Empty(result.Warnings);
            Assert.NotNull(result.Notes);
            Assert.Empty(result.Notes);
            Assert.NotNull(result.Context);
            Assert.Empty(result.Context);
        }

        [Fact]
        public void ValidationResult_Failure_WithErrors_CreatesInvalidResult()
        {
            // Arrange
            var errors = new[]
            {
                new ValidationError { Message = "Error 1", Code = "ERR1" },
                new ValidationError { Message = "Error 2", Code = "ERR2" }
            };

            // Act
            var result = ValidationResult.Failure(errors);

            // Assert
            Assert.False(result.IsValid);
            Assert.Equal(2, result.Errors.Count);
            Assert.Equal("Error 1", result.Errors[0].Message);
            Assert.Equal("ERR1", result.Errors[0].Code);
            Assert.Equal("Error 2", result.Errors[1].Message);
            Assert.Equal("ERR2", result.Errors[1].Code);
        }

        [Fact]
        public void ValidationResult_Failure_WithMessage_CreatesInvalidResult()
        {
            // Arrange
            var message = "Validation failed";

            // Act
            var result = ValidationResult.Failure(message);

            // Assert
            Assert.False(result.IsValid);
            Assert.Single(result.Errors);
            Assert.Equal(message, result.Errors[0].Message);
        }

        [Fact]
        public void ValidationResult_AddError_AddsErrorAndMarksInvalid()
        {
            // Arrange
            var result = ValidationResult.Success();

            // Act
            result.AddError("Test error", "TEST_CODE", "TestProperty");

            // Assert
            Assert.False(result.IsValid);
            Assert.Single(result.Errors);
            var error = result.Errors[0];
            Assert.Equal("Test error", error.Message);
            Assert.Equal("TEST_CODE", error.Code);
            Assert.Equal("TestProperty", error.PropertyName);
        }

        [Fact]
        public void ValidationResult_AddWarning_AddsWarning()
        {
            // Arrange
            var result = ValidationResult.Success();

            // Act
            result.AddWarning("Test warning");

            // Assert
            Assert.True(result.IsValid);
            Assert.Single(result.Warnings);
            Assert.Equal("Test warning", result.Warnings[0]);
        }

        [Fact]
        public void ValidationResult_AddNote_AddsNote()
        {
            // Arrange
            var result = ValidationResult.Success();

            // Act
            result.AddNote("Test note");

            // Assert
            Assert.True(result.IsValid);
            Assert.Single(result.Notes);
            Assert.Equal("Test note", result.Notes[0]);
        }

        [Fact]
        public void ValidationResult_MultipleOperations_HandlesCorrectly()
        {
            // Arrange
            var result = ValidationResult.Success();

            // Act
            result.AddWarning("Warning 1");
            result.AddNote("Note 1");
            result.AddError("Error 1", "ERR1", "Prop1");
            result.AddWarning("Warning 2");
            result.AddNote("Note 2");
            result.Context["TestKey"] = "TestValue";

            // Assert
            Assert.False(result.IsValid);
            Assert.Single(result.Errors);
            Assert.Equal(2, result.Warnings.Count);
            Assert.Equal(2, result.Notes.Count);
            Assert.Single(result.Context);
            Assert.Equal("TestValue", result.Context["TestKey"]);
        }
    }
}
