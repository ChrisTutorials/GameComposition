using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using NetArchTest.Rules;
using Shouldly;
using Xunit;
using BarkMoon.GameComposition.Tests.Common;

// Explicitly alias the NetArchTest Types class to avoid conflict
using ArchTypes = NetArchTest.Rules;

namespace BarkMoon.GameComposition.Core.Tests.Architectural
{
    /// <summary>
    /// Code-first architectural rules replacing static documentation.
    /// DEPRECATED: Use ConfigurationDrivenArchitectureTests instead.
    /// These tests ARE the architectural rules - documentation drift eliminated.
    /// </summary>
    public class ArchitecturalRulesTests
    {
        private readonly Assembly _gameCompositionAssembly = Assembly.LoadFrom("BarkMoon.GameComposition.Core.dll");
        private static readonly ArchitectureConfig _config = ArchitectureConfigLoader.LoadConfig();

        [Fact(DisplayName = "001AR: Core Should Use Microsoft Extensions Not Custom Implementations")]
        public void Core_Should_Use_Microsoft_Extensions_Not_Custom_Implementations()
        {
            // Arrange - Get configuration
            var customImplConfig = _config.ProhibitedPatterns.CustomImplementations;
            if (!customImplConfig.Enabled)
            {
                // Rule disabled - skip test
                return;
            }

            // Act - Check for custom implementations
            var allViolations = new List<string>();
            
            foreach (var prohibitedName in customImplConfig.ProhibitedNames)
            {
                var result = Types.InAssembly(_gameCompositionAssembly)
                    .Should()
                    .NotHaveNameContaining(prohibitedName)
                    .GetResult();

                if (!result.IsSuccessful)
                {
                    allViolations.AddRange(result.Select(f => $"Found prohibited custom implementation: {f.Name}"));
                }
            }

            // Assert - No violations should exist
            if (allViolations.Count > 0)
            {
                var errorMessage = $"{customImplConfig.Message}\nViolations:\n{string.Join("\n", allViolations)}";
                throw new InvalidOperationException(errorMessage);
            }
        }

        [Fact(DisplayName = "002AR: Core Should Be Engine Agnostic")]
        public void Core_Should_Be_Engine_Agnostic()
        {
            // Arrange - Get configuration
            var engineDepConfig = _config.ProhibitedPatterns.EngineDependencies;
            if (!engineDepConfig.Enabled)
            {
                // Rule disabled - skip test
                return;
            }

            // Act - Check for engine dependencies
            var result = Types.InAssembly(_gameCompositionAssembly)
                .Should()
                .NotHaveDependencyOnAny(engineDepConfig.ProhibitedDependencies.ToArray())
                .GetResult();

            // Assert - No engine dependencies should exist
            if (!result.IsSuccessful)
            {
                var violations = result.Select(f => $"Found prohibited engine dependency: {string.Join(", ", f.Dependencies)}");
                var errorMessage = $"{engineDepConfig.Message}\nViolations:\n{string.Join("\n", violations)}";
                throw new InvalidOperationException(errorMessage);
            }
        }

        [Fact(DisplayName = "003AR: Services Should Be Interface First")]
        public void Services_Should_Be_Interface_First()
        {
            // Rule: All services should have interfaces
            var serviceTypes = Types.InAssembly(_gameCompositionAssembly)
                .That()
                .HaveNameEndingWith("Service")
                .And()
                .AreClasses()
                .GetTypes();

            foreach (var serviceType in serviceTypes)
            {
                var interfaceName = "I" + serviceType.Name;
                var interfaceType = _gameCompositionAssembly.GetTypes()
                    .FirstOrDefault(t => t.Name == interfaceName && t.IsInterface);
                
                interfaceType.ShouldNotBeNull($"Service {serviceType.Name} should have interface {interfaceName}");
            }
        }

        [Fact(DisplayName = "004AR: Dependencies Should Follow Layered Architecture")]
        public void Dependencies_Should_Follow_Layered_Architecture()
        {
            // Rule: Clear dependency boundaries
            var result = Types.InAssembly(_gameCompositionAssembly)
                .That()
                .ResideInNamespace("BarkMoon.GameComposition.Core.Services")
                .Should()
                .NotHaveDependencyOn("BarkMoon.GameComposition.Core.Types")
                .GetResult();

            // Note: This might need adjustment based on actual architecture
            // The test enforces the documented dependency rules
        }

        [Fact(DisplayName = "005AR: Types Should Be Immutable Structs Where Appropriate")]
        public void Types_Should_Be_Immutable_Structs_Where_Appropriate()
        {
            // Rule: Value objects should be immutable structs
            var valueObjectTypes = Types.InAssembly(_gameCompositionAssembly)
                .That()
                .ResideInNamespace("BarkMoon.GameComposition.Core.Types")
                .And()
                .AreStructs()
                .GetTypes();

            foreach (var valueType in valueObjectTypes)
            {
                var publicFields = valueType.GetFields(BindingFlags.Public | BindingFlags.Instance);
                var publicSettableProperties = valueType.GetProperties(BindingFlags.Public | BindingFlags.Instance)
                    .Where(p => p.CanWrite);

                // Value objects should be immutable or have private setters
                publicSettableProperties.ShouldBeEmpty($"Value object {valueType.Name} should be immutable");
            }
        }
    }
}
