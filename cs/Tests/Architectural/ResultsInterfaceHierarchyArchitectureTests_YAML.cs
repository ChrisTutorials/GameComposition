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
    /// RED/GREEN architectural test for Results interface hierarchy using YAML configuration.
    /// RED = Interface hierarchy violations (missing inheritance, wrong contracts)
    /// GREEN = Interface hierarchy is architecturally correct (proper inheritance chain)
    /// 
    /// This test enforces the operation result pattern using cross-domain helpers and YAML configuration:
    /// - IValidationResult should inherit from IOperationResult
    /// - ValidationResult should implement IValidationResult
    /// - Domain results should inherit from ValidationResult
    /// </summary>
    public class ResultsInterfaceHierarchyArchitectureTestsYaml
    {
        private readonly ArchitectureConfiguration _config;

        public ResultsInterfaceHierarchyArchitectureTestsYaml()
        {
            // Load YAML configuration for cross-domain compatibility
            _config = ArchitecturalTestHelpers.LoadYamlConfig("architecture-config.yaml");
        }

        [Fact(DisplayName = "IValidationResult Should Inherit From IOperationResult (YAML Config)")]
        [Trait("Category", "Architectural")]
        public void IValidationResult_Should_Inherit_From_IOperationResult_Yaml()
        {
            // Arrange - Use cross-domain assembly loading
            var assemblies = ArchitecturalTestHelpers.GetCoreAssemblies();
            
            // Act - Use configuration-driven validation
            var interfaceConfig = _config.ResultsInterfaceHierarchy;
            var validationResultInterface = ArchitecturalTestHelpers.FindInterfaceByName(
                assemblies, "IValidationResult", "Results");

            // Assert - RED/GREEN testing with YAML configuration
            if (validationResultInterface == null)
            {
                // RED STATE: Interface doesn't exist
                var expectedViolations = interfaceConfig.ExpectedViolations.MissingInterfaces;
                expectedViolations.ShouldContain("IValidationResult", 
                    $"RED STATE: IValidationResult interface not found. {interfaceConfig.InterfaceInheritance.RequiredHierarchy.First().Message}");
                return;
            }

            // Check inheritance using cross-domain helper
            var operationResultInterface = ArchitecturalTestHelpers.FindInterfaceByName(assemblies, "IOperationResult");
            var inheritsFromOperationResult = ArchitecturalTestHelpers.CheckInheritance(
                validationResultInterface, "IOperationResult");

            if (!inheritsFromOperationResult)
            {
                // RED STATE: Wrong inheritance
                var currentInheritance = string.Join(", ", validationResultInterface.GetInterfaces().Select(i => i.Name));
                Assert.True(false, $"RED STATE: IValidationResult doesn't inherit from IOperationResult. Current: {currentInheritance}. {interfaceConfig.InterfaceInheritance.RequiredHierarchy.First().Message}");
                return;
            }

            // GREEN STATE: Correct hierarchy
            inheritsFromOperationResult.ShouldBeTrue("GREEN STATE: IValidationResult correctly inherits from IOperationResult - operation result pattern established");
        }

        [Fact(DisplayName = "ValidationResult Should Implement IValidationResult (YAML Config)")]
        [Trait("Category", "Architectural")]
        public void ValidationResult_Should_Implement_IValidationResult_Yaml()
        {
            // Arrange - Use cross-domain assembly loading
            var assemblies = ArchitecturalTestHelpers.GetCoreAssemblies();
            var interfaceConfig = _config.ResultsInterfaceHierarchy;
            
            // Act - Use configuration-driven validation
            var validationResultClass = ArchitecturalTestHelpers.FindClassByName(
                assemblies, "ValidationResult", "Results");
            var validationResultInterface = ArchitecturalTestHelpers.FindInterfaceByName(assemblies, "IValidationResult");

            // Assert - RED/GREEN testing with YAML configuration
            if (validationResultClass == null)
            {
                // RED STATE: Class doesn't exist
                Assert.True(false, $"RED STATE: ValidationResult class not found. {interfaceConfig.ClassImplementation.Message}");
                return;
            }

            if (validationResultInterface == null)
            {
                // RED STATE: Interface doesn't exist
                var expectedViolations = interfaceConfig.ExpectedViolations.MissingInterfaces;
                expectedViolations.ShouldContain("IValidationResult", 
                    $"RED STATE: IValidationResult interface not found. {interfaceConfig.InterfaceInheritance.RequiredHierarchy.First().Message}");
                return;
            }

            // Check implementation using cross-domain helper
            var implementsInterface = ArchitecturalTestHelpers.CheckInterfaceImplementation(
                validationResultClass, "IValidationResult");

            if (!implementsInterface)
            {
                // RED STATE: Missing implementation
                var currentInterfaces = string.Join(", ", validationResultClass.GetInterfaces().Select(i => i.Name));
                Assert.True(false, $"RED STATE: ValidationResult doesn't implement IValidationResult. Current: {currentInterfaces}. {interfaceConfig.ClassImplementation.Message}");
                return;
            }

            // GREEN STATE: Correct implementation
            implementsInterface.ShouldBeTrue("GREEN STATE: ValidationResult correctly implements IValidationResult");
        }

        [Fact(DisplayName = "Domain Results Should Inherit From ValidationResult (YAML Config)")]
        [Trait("Category", "Architectural")]
        public void Domain_Results_Should_Inherit_From_ValidationResult_Yaml()
        {
            // Arrange - Use cross-domain assembly loading
            var gridPlacementAssemblies = ArchitecturalTestHelpers.GetAssembliesWithGodot();
            var interfaceConfig = _config.ResultsInterfaceHierarchy;
            
            // Act - Use configuration-driven pattern matching
            var domainResultClasses = ArchitecturalTestHelpers.FindClassesByPattern(
                gridPlacementAssemblies, interfaceConfig.DomainResults.Pattern, "Results")
                .Where(t => t.Name != "ValidationResult") // Exclude framework class
                .ToList();

            // Assert - Check each domain result follows operation result pattern
            var violations = ArchitecturalTestHelpers.CheckInheritanceViolations(
                domainResultClasses, interfaceConfig.DomainResults.MustInheritFrom);

            if (violations.Any())
            {
                // RED STATE: Inheritance violations found
                var violationMessages = string.Join("\n", violations);
                Assert.True(false, $"RED STATE: Domain result inheritance violations:\n{violationMessages}\n{interfaceConfig.DomainResults.Message}");
                return;
            }

            if (domainResultClasses.Any())
            {
                // GREEN STATE: All domain results follow pattern
                foreach (var domainResult in domainResultClasses)
                {
                    var inheritsCorrectly = domainResult.BaseType?.Name == interfaceConfig.DomainResults.MustInheritFrom;
                    inheritsCorrectly.ShouldBeTrue($"GREEN STATE: {domainResult.Name} correctly inherits from {interfaceConfig.DomainResults.MustInheritFrom} - operation result pattern followed");
                }
            }
            else
            {
                // No domain results found - this is OK for now
                Console.WriteLine("INFO: No domain result classes found in GridPlacement assemblies");
            }
        }

        [Fact(DisplayName = "Results Should Follow Naming Convention (YAML Config)")]
        [Trait("Category", "Architectural")]
        public void Results_Should_Follow_Naming_Convention_Yaml()
        {
            // Arrange - Use cross-domain assembly loading
            var assemblies = ArchitecturalTestHelpers.GetAllRelevantAssemblies();
            var interfaceConfig = _config.ResultsInterfaceHierarchy;
            
            // Act - Use configuration-driven naming validation
            var resultTypes = ArchitecturalTestHelpers.FindTypesByPattern(
                assemblies, new[] { "*Result", "*Report" }, "Results");

            // Assert - Check naming conventions using YAML configuration
            var namingViolations = ArchitecturalTestHelpers.CheckNamingViolations(
                resultTypes, interfaceConfig.NamingConventions);

            if (namingViolations.Any())
            {
                // RED STATE: Naming convention violations found
                var violationMessages = string.Join("\n", namingViolations);
                Assert.True(false, $"RED STATE: Naming convention violations:\n{violationMessages}");
                return;
            }

            // GREEN STATE: All naming conventions are correct
            namingViolations.ShouldBeEmpty("GREEN STATE: All result types follow operation result naming conventions");
        }

        [Fact(DisplayName = "Results Architecture Should Be Complete (YAML Config)")]
        [Trait("Category", "Architectural")]
        public void Results_Architecture_Should_Be_Complete_Yaml()
        {
            // Arrange - Load configuration and assemblies
            var assemblies = ArchitecturalTestHelpers.GetAllRelevantAssemblies();
            var interfaceConfig = _config.ResultsInterfaceHierarchy;
            
            // Act - Comprehensive architecture validation
            var validationResultInterface = ArchitecturalTestHelpers.FindInterfaceByName(assemblies, "IValidationResult");
            var validationResultClass = ArchitecturalTestHelpers.FindClassByName(assemblies, "ValidationResult");
            var operationResultInterface = ArchitecturalTestHelpers.FindInterfaceByName(assemblies, "IOperationResult");

            // Assert - Complete architecture validation
            var allComponentsExist = validationResultInterface != null && 
                                  validationResultClass != null && 
                                  operationResultInterface != null;

            if (!allComponentsExist)
            {
                // RED STATE: Missing components
                var missingComponents = new List<string>();
                if (validationResultInterface == null) missingComponents.Add("IValidationResult interface");
                if (validationResultClass == null) missingComponents.Add("ValidationResult class");
                if (operationResultInterface == null) missingComponents.Add("IOperationResult interface");

                var fixInstructions = interfaceConfig.ExpectedViolations.FixInstructions;
                Assert.True(false, $"RED STATE: Results architecture incomplete. Missing: {string.Join(", ", missingComponents)}\n\nFix Instructions:\n{fixInstructions}");
                return;
            }

            // GREEN STATE: Complete architecture
            allComponentsExist.ShouldBeTrue("GREEN STATE: Results interface hierarchy is complete and ready for cross-plugin compatibility");
        }
    }
}
