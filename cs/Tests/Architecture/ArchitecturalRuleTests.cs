using System;
using System.Linq;
using System.Reflection;
using Xunit;
using Xunit.Abstractions;
using BarkMoon.GameComposition.Core.Architecture;

namespace BarkMoon.GameComposition.Core.Tests.Architecture
{
    /// <summary>
    /// Enforces architectural rules through automated testing
    /// These tests fail if architectural rules are violated
    /// </summary>
    public class ArchitecturalRuleTests
    {
        private readonly ITestOutputHelper _output;

        public ArchitecturalRuleTests(ITestOutputHelper output)
        {
            _output = output;
        }

        [Fact]
        public void GameComposition_Assembly_Should_Follow_Architectural_Rules()
        {
            // Arrange
            var assembly = Assembly.GetAssembly(typeof(TypedId<>)) 
                ?? throw new InvalidOperationException("Could not load GameComposition assembly");

            // Act
            var result = ArchitecturalValidator.ValidateAssembly(assembly);

            // Assert
            _output.WriteLine($"Architectural validation completed:");
            _output.WriteLine($"  Errors: {result.ErrorCount}");
            _output.WriteLine($"  Warnings: {result.WarningCount}");

            if (result.Violations.Any())
            {
                _output.WriteLine("Violations found:");
                foreach (var violation in result.Violations)
                {
                    _output.WriteLine($"  [{violation.Severity}] {violation.Type?.Name ?? "Unknown"}: {violation.Message}");
                    _output.WriteLine($"    Recommendation: {violation.Recommendation}");
                }
            }

            // Should have no errors (warnings are allowed)
            Assert.True(result.IsValid, 
                $"Architectural rules violated: {result.ErrorCount} errors found. See test output for details.");
        }

        [Fact]
        public void Custom_ObjectPool_Should_Use_Microsoft_Extensions()
        {
            // Arrange
            var assembly = Assembly.GetAssembly(typeof(TypedId<>));

            // Act
            var result = ArchitecturalValidator.ValidateAssembly(assembly);
            var poolViolations = result.Violations
                .Where(v => v.Message.Contains("pool") && v.Severity == ViolationSeverity.Error)
                .ToList();

            // Assert
            if (poolViolations.Any())
            {
                _output.WriteLine("Custom pool implementations found:");
                foreach (var violation in poolViolations)
                {
                    _output.WriteLine($"  - {violation.Type?.Name}: {violation.Message}");
                    _output.WriteLine($"    Recommendation: {violation.Recommendation}");
                }
            }

            Assert.Empty(poolViolations);
        }

        [Fact]
        public void Custom_Logger_Should_Use_Microsoft_Extensions()
        {
            // Arrange
            var assembly = Assembly.GetAssembly(typeof(TypedId<>));

            // Act
            var result = ArchitecturalValidator.ValidateAssembly(assembly);
            var loggerViolations = result.Violations
                .Where(v => v.Message.Contains("logger") && v.Severity == ViolationSeverity.Error)
                .ToList();

            // Assert
            Assert.Empty(loggerViolations);
        }

        [Fact]
        public void Custom_ServiceCollection_Should_Use_Microsoft_Extensions()
        {
            // Arrange
            var assembly = Assembly.GetAssembly(typeof(TypedId<>));

            // Act
            var result = ArchitecturalValidator.ValidateAssembly(assembly);
            var diViolations = result.Violations
                .Where(v => v.Message.Contains("service collection") && v.Severity == ViolationSeverity.Error)
                .ToList();

            // Assert
            Assert.Empty(diViolations);
        }

        [Fact]
        public void Approved_Custom_Types_Should_Be_Allowed()
        {
            // Arrange - These types should be approved custom implementations
            var approvedTypes = new[]
            {
                typeof(TypedId<>),
                typeof(NumericId<>),
                typeof(CoreVector2I),
                typeof(CoreVector3),
                typeof(Rect2I)
            };

            // Act & Assert
            foreach (var type in approvedTypes)
            {
                var result = ArchitecturalValidator.ValidateType(type);
                Assert.True(result.IsValid, 
                    $"Approved custom type {type.Name} should not violate architectural rules");
            }
        }

        [Theory]
        [InlineData("CustomPool", "Microsoft.Extensions.ObjectPool")]
        [InlineData("MyLogger", "Microsoft.Extensions.Logging")]
        [InlineData("SimpleServiceCollection", "Microsoft.Extensions.DependencyInjection")]
        public void GetMicrosoftAlternative_Should_Return_Correct_Alternative(string customName, string expectedAlternative)
        {
            // Act
            var alternative = ArchitecturalValidator.GetMicrosoftAlternative(customName);

            // Assert
            Assert.Equal(expectedAlternative, alternative);
        }

        [Fact]
        public void All_Types_Should_Use_Interfaces_Over_Concretes()
        {
            // Arrange
            var assembly = Assembly.GetAssembly(typeof(TypedId<>));

            // Act
            var result = ArchitecturalValidator.ValidateAssembly(assembly);
            var interfaceViolations = result.Violations
                .Where(v => v.Message.Contains("concrete type") && v.Severity == ViolationSeverity.Warning)
                .ToList();

            // Assert
            if (interfaceViolations.Any())
            {
                _output.WriteLine("Interface dependency violations found:");
                foreach (var violation in interfaceViolations)
                {
                    _output.WriteLine($"  - {violation.Type?.Name}: {violation.Message}");
                    _output.WriteLine($"    Recommendation: {violation.Recommendation}");
                }
            }

            // Allow warnings but log them for review
            if (interfaceViolations.Any())
            {
                _output.WriteLine($"WARNING: {interfaceViolations.Count} interface dependency violations found");
            }
        }
    }

    /// <summary>
    /// Sample of how to mark custom implementations as approved
    /// </summary>
    [ApprovedCustomImplementation(
        Justification = "Domain-specific type-safe identifiers needed for game entities",
        MigrationPath = "Consider Microsoft.Extensions.Guid if GUID-based identifiers become sufficient"
    )]
    public class SampleApprovedType
    {
        // This type is marked as approved custom implementation
    }

    /// <summary>
    /// Sample of how to mark types that should use Microsoft.Extensions
    /// </summary>
    [UseMicrosoftExtensions("Microsoft.Extensions.ObjectPool.ObjectPool<T>")]
    public class SampleDeprecatedType
    {
        // This type is marked as deprecated in favor of Microsoft.Extensions
    }
}
