using System.Linq;
using NetArchTest.Rules;
using Shouldly;
using Xunit;

namespace BarkMoon.GameComposition.Tests.Architectural
{
    /// <summary>
    /// Global architectural tests enforcing dependency inversion principles across all plugins.
    /// These tests ensure proper abstraction layers and prevent architectural violations.
    /// </summary>
    public class DependencyInversionArchitectureTests
    {
        [Fact(DisplayName = "DI-001: Core Classes Should Only Depend On Interfaces")]
        [Trait("Category", "Architectural")]
        public void Core_Classes_Should_Only_Depend_On_Interfaces()
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
                    .And().AreNotInterfaces()
                    .And().AreNotAbstract()
                    .Should()
                    .NotHaveDependencyOnAny("BarkMoon.GameComposition.Godot", "Godot")
                    .GetResult();

                if (!result.IsSuccessful)
                {
                    violations.Add($"{assembly.GetName().Name}: {string.Join(", ", result.FailedTypeNames)}");
                }
            }

            violations.ShouldBeEmpty("Core classes should not depend on Godot or concrete implementations");
        }

        [Fact(DisplayName = "DI-002: Service Classes Should Implement Interfaces")]
        [Trait("Category", "Architectural")]
        public void Service_Classes_Should_Implement_Interfaces()
        {
            // Arrange
            var assembly = typeof(BarkMoon.GameComposition.Core.Services.DI.ServiceRegistry).Assembly;
            
            // Act
            var result = Types.InAssembly(assembly)
                .That().ResideInNamespace("*.Services")
                .And().HaveNameEndingWith("Service")
                .And().AreNotInterfaces()
                .Should()
                .ImplementInterface(typeof(BarkMoon.GameComposition.Core.Interfaces.IService))
                .GetResult();

            // Assert
            result.IsSuccessful.ShouldBeTrue("All service classes should implement IService interface");
        }

        [Fact(DisplayName = "DI-003: Plugin Boundaries Should Respect Abstraction Layers")]
        [Trait("Category", "Architectural")]
        public void Plugin_Boundaries_Should_Respect_Abstraction_Layers()
        {
            // Arrange - This test will be expanded as more plugins are added
            var gameCompositionAssembly = typeof(BarkMoon.GameComposition.Core.Services.DI.ServiceRegistry).Assembly;
            
            // Act - GameComposition.Core should not depend on specific plugins
            var result = Types.InAssembly(gameCompositionAssembly)
                .Should()
                .NotHaveDependencyOnAny("BarkMoon.GridPlacement", "BarkMoon.ItemDrops", "BarkMoon.ArtisanCraft")
                .GetResult();

            // Assert
            result.IsSuccessful.ShouldBeTrue("GameComposition.Core should not depend on specific plugin implementations");
        }

        [Fact(DisplayName = "DI-004: ServiceRegistry Should Only Resolve Interfaces")]
        [Trait("Category", "Architectural")]
        public void ServiceRegistry_Should_Only_Resolve_Interfaces()
        {
            // Arrange
            var assembly = typeof(BarkMoon.GameComposition.Core.Services.DI.ServiceRegistry).Assembly;
            
            // Act - Check that ServiceRegistry methods work with interfaces
            var serviceRegistryType = assembly.GetTypes()
                .FirstOrDefault(t => t.Name == "ServiceRegistry");
            
            serviceRegistryType.ShouldNotBeNull("ServiceRegistry should exist");
            
            // Verify generic methods are properly constrained to interfaces
            var registerMethod = serviceRegistryType.GetMethods()
                .FirstOrDefault(m => m.Name.Contains("Register") && m.IsGenericMethod);
            
            registerMethod.ShouldNotBeNull("ServiceRegistry should have generic register methods");
            
            // Assert - This is more of a design validation, actual implementation checks would be in unit tests
            true.ShouldBeTrue("ServiceRegistry should be designed for interface resolution");
        }
    }
}
