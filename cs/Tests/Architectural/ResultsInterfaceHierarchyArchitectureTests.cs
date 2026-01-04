using System.Linq;
using System.Reflection;
using NetArchTest.Rules;
using Shouldly;
using Xunit;
using BarkMoon.GameComposition.Tests.Common;

// Explicitly alias the NetArchTest Types class to avoid conflict
using ArchTypes = NetArchTest.Rules;

namespace BarkMoon.GameComposition.Tests.Architectural
{
    /// <summary>
    /// RED/GREEN architectural test for Results interface hierarchy.
    /// RED = Interface hierarchy violations (missing inheritance, wrong contracts)
    /// GREEN = Interface hierarchy is architecturally correct (proper inheritance chain)
    /// 
    /// This test enforces the operation result pattern: IValidationResult should inherit from IOperationResult,
    /// and domain results should inherit from ValidationResult. This is fundamentally different from
    /// DomainSnapshot patterns which capture immutable state for rollback purposes.
    /// 
    /// PATTERNS:
    /// - Operation Results: DomainResult → ValidationResult → IValidationResult → IOperationResult
    /// - State Snapshots: DomainSnapshot → ISnapshot{TSnapshot} (separate pattern)
    /// </summary>
    public class ResultsInterfaceHierarchyArchitectureTests
    {
        [Fact(DisplayName = "IValidationResult Should Inherit From IOperationResult (RED = Missing inheritance, GREEN = Correct hierarchy)")]
        [Trait("Category", "Architectural")]
        public void IValidationResult_Should_Inherit_From_IOperationResult()
        {
            // Arrange - Get all framework assemblies
            var assemblies = TestAssemblyHelper.GetCoreAssemblies();
            
            // Act - Find IValidationResult interface
            var validationResultInterface = assemblies
                .SelectMany(assembly => assembly.GetTypes())
                .FirstOrDefault(type => 
                    type.IsInterface && 
                    type.Name == "IValidationResult" &&
                    type.Namespace?.Contains("Results") == true);

            // Assert - RED STATE: Interface doesn't exist or doesn't inherit from IOperationResult
            if (validationResultInterface == null)
            {
                // RED: IValidationResult interface doesn't exist
                Assert.True(false, "RED STATE: IValidationResult interface not found. Create IValidationResult : IOperationResult for operation result pattern");
                return;
            }

            var operationResultInterface = assemblies
                .SelectMany(assembly => assembly.GetTypes())
                .FirstOrDefault(type => 
                    type.IsInterface && 
                    type.Name == "IOperationResult");

            if (operationResultInterface == null)
            {
                // RED: IOperationResult interface doesn't exist
                Assert.True(false, "RED STATE: IOperationResult interface not found. This should exist in GameComposition.Core.Results as the base for all operation results");
                return;
            }

            // Check inheritance
            var inheritsFromOperationResult = validationResultInterface.GetInterfaces()
                .Any(iface => iface.Name == "IOperationResult");

            if (!inheritsFromOperationResult)
            {
                // RED: IValidationResult doesn't inherit from IOperationResult
                Assert.True(false, $"RED STATE: IValidationResult doesn't inherit from IOperationResult. Current inheritance: {string.Join(", ", validationResultInterface.GetInterfaces().Select(i => i.Name))}. Operation results need this hierarchy for cross-plugin compatibility.");
                return;
            }

            // GREEN: Interface hierarchy is correct
            inheritsFromOperationResult.ShouldBeTrue("GREEN STATE: IValidationResult correctly inherits from IOperationResult - operation result pattern established");
        }

        [Fact(DisplayName = "ValidationResult Should Implement IValidationResult (RED = Missing implementation, GREEN = Correct implementation)")]
        [Trait("Category", "Architectural")]
        public void ValidationResult_Should_Implement_IValidationResult()
        {
            // Arrange - Get all framework assemblies
            var assemblies = TestAssemblyHelper.GetCoreAssemblies();
            
            // Act - Find ValidationResult class and IValidationResult interface
            var validationResultClass = assemblies
                .SelectMany(assembly => assembly.GetTypes())
                .FirstOrDefault(type => 
                    !type.IsInterface && 
                    type.Name == "ValidationResult" &&
                    type.Namespace?.Contains("Results") == true);

            var validationResultInterface = assemblies
                .SelectMany(assembly => assembly.GetTypes())
                .FirstOrDefault(type => 
                    type.IsInterface && 
                    type.Name == "IValidationResult");

            // Assert - RED STATE: ValidationResult doesn't implement IValidationResult
            if (validationResultClass == null)
            {
                // RED: ValidationResult class doesn't exist
                Assert.True(false, "RED STATE: ValidationResult class not found. This should exist in GameComposition.Core.Results");
                return;
            }

            if (validationResultInterface == null)
            {
                // RED: IValidationResult interface doesn't exist
                Assert.True(false, "RED STATE: IValidationResult interface not found. Create IValidationResult : IOperationResult");
                return;
            }

            var implementsValidationResultInterface = validationResultClass.GetInterfaces()
                .Any(iface => iface.Name == "IValidationResult");

            if (!implementsValidationResultInterface)
            {
                // RED: ValidationResult doesn't implement IValidationResult
                Assert.True(false, $"RED STATE: ValidationResult doesn't implement IValidationResult. Current interfaces: {string.Join(", ", validationResultClass.GetInterfaces().Select(i => i.Name))}");
                return;
            }

            // GREEN: ValidationResult correctly implements IValidationResult
            implementsValidationResultInterface.ShouldBeTrue("GREEN STATE: ValidationResult correctly implements IValidationResult");
        }

        [Fact(DisplayName = "Domain Results Should Inherit From ValidationResult (RED = Wrong inheritance, GREEN = Correct hierarchy)")]
        [Trait("Category", "Architectural")]
        public void Domain_Results_Should_Inherit_From_ValidationResult()
        {
            // Arrange - Get GridPlacement assemblies
            var gridPlacementAssemblies = TestAssemblyHelper.GetAssembliesWithGodot();
            
            // Act - Find domain result classes (operation results, NOT snapshots)
            var domainResultClasses = gridPlacementAssemblies
                .SelectMany(assembly => assembly.GetTypes())
                .Where(type => 
                    !type.IsInterface && 
                    type.Name.EndsWith("Result") &&
                    type.Namespace?.Contains("Results") == true &&
                    type.Name != "ValidationResult") // Exclude the framework class
                .ToList();

            // Assert - Check each domain result follows operation result pattern
            foreach (var domainResult in domainResultClasses)
            {
                var inheritsFromValidationResult = domainResult.BaseType?.Name == "ValidationResult";
                
                if (!inheritsFromValidationResult)
                {
                    // RED: Domain result doesn't inherit from ValidationResult
                    Assert.True(false, $"RED STATE: {domainResult.Name} doesn't inherit from ValidationResult. Current base type: {domainResult.BaseType?.Name ?? "null"}. Domain results should follow operation result pattern, not snapshot pattern.");
                    return;
                }
            }

            if (domainResultClasses.Any())
            {
                // GREEN: All domain results correctly inherit from ValidationResult
                foreach (var domainResult in domainResultClasses)
                {
                    var inheritsFromValidationResult = domainResult.BaseType?.Name == "ValidationResult";
                    inheritsFromValidationResult.ShouldBeTrue($"GREEN STATE: {domainResult.Name} correctly inherits from ValidationResult - operation result pattern followed");
                }
            }
            else
            {
                // No domain results found - this is OK for now
                Console.WriteLine("INFO: No domain result classes found in GridPlacement assemblies");
            }
        }

        [Fact(DisplayName = "Results Should Follow Naming Convention (RED = Inconsistent naming, GREEN = Correct naming)")]
        [Trait("Category", "Architectural")]
        public void Results_Should_Follow_Naming_Convention()
        {
            // Arrange - Get all assemblies
            var assemblies = TestAssemblyHelper.GetAllRelevantAssemblies();
            
            // Act - Find all result types (operation results and reports)
            var resultTypes = assemblies
                .SelectMany(assembly => assembly.GetTypes())
                .Where(type => 
                    type.Namespace?.Contains("Results") == true &&
                    (type.Name.EndsWith("Result") || type.Name.EndsWith("Report")))
                .ToList();

            // Assert - Check naming conventions for operation result pattern
            var namingViolations = new List<string>();

            foreach (var resultType in resultTypes)
            {
                // Interfaces should start with 'I'
                if (resultType.IsInterface && !resultType.Name.StartsWith("I"))
                {
                    namingViolations.Add($"{resultType.Name} - Interface should start with 'I'");
                }

                // Classes should not start with 'I' (except ValidationResult which is framework-level)
                if (!resultType.IsInterface && resultType.Name.StartsWith("I") && resultType.Name != "ValidationResult")
                {
                    namingViolations.Add($"{resultType.Name} - Class should not start with 'I'");
                }

                // Result classes should end with 'Result' (operation result pattern)
                if (!resultType.IsInterface && resultType.Name.EndsWith("Report") && resultType.Name != "PlacementReport")
                {
                    namingViolations.Add($"{resultType.Name} - Report classes should follow operation result pattern with 'Result' suffix");
                }
            }

            if (namingViolations.Any())
            {
                // RED: Naming convention violations found
                Assert.True(false, $"RED STATE: Naming convention violations found:\n{string.Join("\n", namingViolations)}");
                return;
            }

            // GREEN: All naming conventions are correct
            namingViolations.ShouldBeEmpty("GREEN STATE: All result types follow operation result naming conventions");
        }
    }
}
