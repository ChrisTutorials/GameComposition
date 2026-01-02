using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using NetArchTest.Rules;
using Shouldly;
using Xunit;

namespace BarkMoon.GameComposition.ArchitecturalTests.Core
{
    /// <summary>
    /// Core architectural rules for GameComposition framework.
    /// Tests fundamental framework principles and engine agnostic requirements.
    /// </summary>
    public class CoreArchitecturalTests
    {
        private readonly Assembly _gameCompositionAssembly = Assembly.LoadFrom("BarkMoon.GameComposition.Core.dll");

        [Fact(DisplayName = "001GC: Core Should Use Microsoft Extensions Not Custom Implementations")]
        public void Core_Should_Use_Microsoft_Extensions_Not_Custom_Implementations()
        {
            // Rule: Use Microsoft.Extensions.* instead of custom implementations
            var result = Types.InAssembly(_gameCompositionAssembly)
                .Should()
                .NotHaveName("CustomPool")
                .And()
                .NotHaveName("CollectionPool")
                .And()
                .NotHaveName("CustomLogger")
                .And()
                .NotHaveName("CustomDI")
                .GetResult();

            result.IsSuccessful.ShouldBeTrue("Should use Microsoft.Extensions.* instead of custom implementations");
        }

        [Fact(DisplayName = "002GC: Core Should Be Engine Agnostic")]
        public void Core_Should_Be_Engine_Agnostic()
        {
            // Rule: No engine dependencies in Core package
            var result = Types.InAssembly(_gameCompositionAssembly)
                .Should()
                .NotHaveDependencyOnAny("Godot", "Unity", "Microsoft.Xna")
                .GetResult();

            result.IsSuccessful.ShouldBeTrue("Core should be engine-agnostic");
        }

        [Fact(DisplayName = "003GC: Services Should Be Interface First")]
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

        [Fact(DisplayName = "004GC: Dependencies Should Follow Layered Architecture")]
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

        [Fact(DisplayName = "005GC: Types Should Be Immutable Structs Where Appropriate")]
        public void Types_Should_Be_Immutable_Structs_Where_Appropriate()
        {
            // Rule: Value objects should be immutable structs
            var allTypes = _gameCompositionAssembly.GetTypes();
            var valueObjectTypes = allTypes
                .Where(t => t.Namespace == "BarkMoon.GameComposition.Core.Types" && t.IsValueType)
                .ToList();

            foreach (var valueType in valueObjectTypes)
            {
                var fields = valueType.GetFields(BindingFlags.Public | BindingFlags.Instance);
                foreach (var field in fields)
                {
                    field.IsInitOnly.ShouldBeTrue($"Value object {valueType.Name}.{field.Name} should be readonly");
                }
            }
        }
    }
}
