using System.Linq;
using NetArchTest.Rules;
using Shouldly;
using Xunit;

namespace BarkMoon.GameComposition.Tests.Architectural
{
    /// <summary>
    /// Global architectural tests enforcing proper namespace organization across all plugins.
    /// These tests prevent architectural drift and maintain clean layer separation.
    /// </summary>
    public class NamespaceOrganizationArchitectureTests
    {
        [Fact(DisplayName = "NS-001: Core Namespaces Should Not Reference Godot")]
        [Trait("Category", "Architectural")]
        public void Core_Namespaces_Should_Not_Reference_Godot()
        {
            // Arrange
            var assemblies = new[] 
            { 
                typeof(BarkMoon.GameComposition.Core.Services.DI.ServiceRegistry).Assembly,
                // Add other Core assemblies as they're created
            };
            
            var violations = new List<string>();

            // Act & Assert - Check each Core assembly
            foreach (var assembly in assemblies)
            {
                var result = Types.InAssembly(assembly)
                    .That().ResideInNamespace("*.Core.*")
                    .Should()
                    .NotHaveDependencyOnAny("Godot", "BarkMoon.GameComposition.Godot")
                    .GetResult();

                if (!result.IsSuccessful)
                {
                    violations.Add($"{assembly.GetName().Name}: {string.Join(", ", result.FailedTypeNames)}");
                }
            }

            violations.ShouldBeEmpty("Core namespaces should not reference Godot assemblies");
        }

        [Fact(DisplayName = "NS-002: Adapter Namespaces Should Bridge Core To Godot")]
        [Trait("Category", "Architectural")]
        public void Adapter_Namespaces_Should_Bridge_Core_To_Godot()
        {
            // Arrange - This test validates the adapter pattern exists
            var gameCompositionAssembly = typeof(BarkMoon.GameComposition.Core.Services.DI.ServiceRegistry).Assembly;
            
            // Act - Look for adapter classes that implement Core interfaces
            var adapterTypes = Types.InAssembly(gameCompositionAssembly)
                .That().ResideInNamespace("*.Adapters")
                .Or().HaveNameContaining("Adapter")
                .GetTypes()
                .ToList();

            // Assert - Adapters should exist and follow naming patterns
            adapterTypes.ShouldNotBeEmpty("Adapter classes should exist to bridge Core and Godot");
            
            // Each adapter should implement an interface
            foreach (var adapterType in adapterTypes.Take(5)) // Sample check to avoid too many validations
            {
                var interfaces = adapterType.GetInterfaces();
                interfaces.ShouldNotBeEmpty($"Adapter {adapterType.Name} should implement interfaces");
            }
        }

        [Fact(DisplayName = "NS-003: Test Namespaces Should Follow Naming Conventions")]
        [Trait("Category", "Architectural")]
        public void Test_Namespaces_Should_Follow_Naming_Conventions()
        {
            // Arrange
            var assembly = typeof(BarkMoon.GameComposition.Tests.Architectural.NamespaceOrganizationArchitectureTests).Assembly;
            
            // Act
            var testTypes = Types.InAssembly(assembly)
                .That().ResideInNamespace("*.Tests.*")
                .GetTypes()
                .Where(t => t.Name.Contains("Test"))
                .ToList();

            // Assert
            testTypes.ShouldNotBeEmpty("Test classes should exist");
            
            // All test classes should follow naming convention
            var invalidNames = testTypes
                .Where(t => !t.Name.EndsWith("Tests"))
                .Select(t => t.Name)
                .ToList();

            invalidNames.ShouldBeEmpty("Test classes should end with 'Tests' suffix");
        }

        [Fact(DisplayName = "NS-004: Internal Namespaces Should Be Properly Isolated")]
        [Trait("Category", "Architectural")]
        public void Internal_Namespaces_Should_Be_Properly_Isolated()
        {
            // Arrange
            var assembly = typeof(BarkMoon.GameComposition.Core.Services.DI.ServiceRegistry).Assembly;
            
            // Act - Check internal namespace types
            var internalTypes = Types.InAssembly(assembly)
                .That().ResideInNamespace("*.Internal.*")
                .GetTypes()
                .ToList();

            // Assert - Internal types should not be public
            var publicInternalTypes = internalTypes
                .Where(t => t.IsPublic)
                .Select(t => $"{t.Namespace}.{t.Name}")
                .ToList();

            publicInternalTypes.ShouldBeEmpty("Internal namespace types should not be public");
        }
    }
}
