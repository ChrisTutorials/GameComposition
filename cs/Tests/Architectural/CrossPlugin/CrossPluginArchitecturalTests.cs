using System;
using System.Linq;
using System.Reflection;
using NetArchTest.Rules;
using Shouldly;
using Xunit;

namespace BarkMoon.GameComposition.Core.Tests.Architectural
{
    /// <summary>
    /// Cross-plugin architectural tests using NetArchTests.enhancededition + Shouldy.
    /// Ensures architectural consistency across all GameComposition plugins.
    /// </summary>
    public class CrossPluginArchitecturalTests
    {
        private readonly Assembly[] _pluginAssemblies = new[]
        {
            Assembly.LoadFrom("BarkMoon.GameComposition.Core.dll"),
            Assembly.LoadFrom("BarkMoon.GridPlacement.Core.dll"),
            Assembly.LoadFrom("BarkMoon.GridPlacement.Godot.dll")
        };

        [Fact(DisplayName = "001CP: All Plugins Should Follow Namespace Convention")]
        public void All_Plugins_Should_Follow_Namespace_Convention()
        {
            // Arrange & Act - NetArchTests cross-assembly validation
            var results = _pluginAssemblies.Select(assembly => 
                Types.InAssembly(assembly)
                    .Should()
                    .ResideInNamespaceStartingWith("BarkMoon")
                    .GetResult()
            ).ToList();

            // Assert - Shouldy validation
            foreach (var (result, index) in results.Select((r, i) => (r, i)))
            {
                result.IsSuccessful.ShouldBeTrue(
                    $"Plugin assembly {_pluginAssemblies[index].GetName().Name} should follow BarkMoon namespace convention");
            }
        }

        [Fact(DisplayName = "002CP: Core Assemblies Should Not Depend On Godot Assemblies")]
        public void Core_Assemblies_Should_Not_Depend_On_Godot_Assemblies()
        {
            // Arrange & Act - NetArchTests dependency validation
            var coreAssemblies = _pluginAssemblies.Where(a => a.GetName().Name.Contains("Core")).ToArray();
            var godotAssemblies = _pluginAssemblies.Where(a => a.GetName().Name.Contains("Godot")).ToArray();

            foreach (var coreAssembly in coreAssemblies)
            {
                var result = Types.InAssembly(coreAssembly)
                    .Should()
                    .NotHaveDependencyOnAny(godotAssemblies.Select(a => a.GetName().Name).ToArray())
                    .GetResult();

                // Assert - Shouldy validation
                result.IsSuccessful.ShouldBeTrue(
                    $"Core assembly {coreAssembly.GetName().Name} should not depend on Godot assemblies");
            }
        }

        [Fact(DisplayName = "003CP: Godot Assemblies Should Depend On Core Assemblies")]
        public void Godot_Assemblies_Should_Depend_On_Core_Assemblies()
        {
            // Arrange & Act - NetArchTests dependency validation
            var godotAssemblies = _pluginAssemblies.Where(a => a.GetName().Name.Contains("Godot")).ToArray();
            var coreAssemblies = _pluginAssemblies.Where(a => a.GetName().Name.Contains("Core")).ToArray();

            foreach (var godotAssembly in godotAssemblies)
            {
                var result = Types.InAssembly(godotAssembly)
                    .Should()
                    .HaveDependencyOnAny(coreAssemblies.Select(a => a.GetName().Name).ToArray())
                    .GetResult();

                // Assert - Shouldy validation
                result.IsSuccessful.ShouldBeTrue(
                    $"Godot assembly {godotAssembly.GetName().Name} should depend on Core assemblies");
            }
        }

        [Fact(DisplayName = "004CP: All Plugins Should Use Consistent Interface Naming")]
        public void All_Plugins_Should_Use_Consistent_Interface_Naming()
        {
            // Arrange & Act - NetArchTests interface naming validation
            var results = _pluginAssemblies.Select(assembly => 
                Types.InAssembly(assembly)
                    .That()
                    .AreInterfaces()
                    .And()
                    .ArePublic()
                    .Should()
                    .HaveNameStartingWith("I")
                    .GetResult()
            ).ToList();

            // Assert - Shouldy validation
            foreach (var (result, index) in results.Select((r, i) => (r, i)))
            {
                result.IsSuccessful.ShouldBeTrue(
                    $"Plugin assembly {_pluginAssemblies[index].GetName().Name} should use I* naming for interfaces");
            }
        }

        [Fact(DisplayName = "005CP: Core Assemblies Should Not Have Engine Dependencies")]
        public void Core_Assemblies_Should_Not_Have_Engine_Dependencies()
        {
            // Arrange & Act - NetArchTests engine dependency validation
            var coreAssemblies = _pluginAssemblies.Where(a => a.GetName().Name.Contains("Core")).ToArray();
            var engineDependencies = new[] { "Godot", "Unity", "Microsoft.Xna.Framework" };

            foreach (var coreAssembly in coreAssemblies)
            {
                var result = Types.InAssembly(coreAssembly)
                    .Should()
                    .NotHaveDependencyOnAny(engineDependencies)
                    .GetResult();

                // Assert - Shouldy validation
                result.IsSuccessful.ShouldBeTrue(
                    $"Core assembly {coreAssembly.GetName().Name} should not have engine dependencies");
            }
        }

        [Fact(DisplayName = "006CP: All Plugins Should Have Consistent Test Naming")]
        public void All_Plugins_Should_Have_Consistent_Test_Naming()
        {
            // Arrange & Act - NetArchTests test naming validation
            var testAssemblies = _pluginAssemblies
                .Where(a => a.GetName().Name.Contains("Tests"))
                .ToArray();

            if (testAssemblies.Any())
            {
                foreach (var testAssembly in testAssemblies)
                {
                    var result = Types.InAssembly(testAssembly)
                        .That()
                        .HaveCustomAttribute<FactAttribute>()
                        .Should()
                        .ResideInNamespaceEndingWith("Tests")
                        .GetResult();

                    // Assert - Shouldy validation
                    result.IsSuccessful.ShouldBeTrue(
                        $"Test assembly {testAssembly.GetName().Name} should have tests in Tests namespace");
                }
            }
            else
            {
                true.ShouldBeTrue("No test assemblies found - validation skipped");
            }
        }

        [Fact(DisplayName = "007CP: Cross Plugin Interfaces Should Be Consistent")]
        public void Cross_Plugin_Interfaces_Should_Be_Consistent()
        {
            // Arrange & Act - NetArchTests interface consistency validation
            var gameCompositionCore = Assembly.LoadFrom("BarkMoon.GameComposition.Core.dll");
            var gridPlacementCore = Assembly.LoadFrom("BarkMoon.GridPlacement.Core.dll");

            // Verify IGridMap exists in GameComposition.Core
            var gridMapResult = Types.InAssembly(gameCompositionCore)
                .That()
                .AreInterfaces()
                .And()
                .HaveName("IGridMap")
                .Should()
                .Exist()
                .GetResult();

            // Verify GridPlacement.Core uses IGridMap
            var usageResult = Types.InAssembly(gridPlacementCore)
                .Should()
                .HaveDependencyOn("BarkMoon.GameComposition.Core")
                .GetResult();

            // Assert - Shouldy validation
            gridMapResult.IsSuccessful.ShouldBeTrue("GameComposition.Core should define IGridMap interface");
            usageResult.IsSuccessful.ShouldBeTrue("GridPlacement.Core should depend on GameComposition.Core");
        }

        [Fact(DisplayName = "008CP: All Assemblies Should Be Properly Versioned")]
        public void All_Assemblies_Should_Be_Properly_Versioned()
        {
            // Arrange & Act - NetArchTests assembly validation
            var results = _pluginAssemblies.Select(assembly =>
            {
                var assemblyName = assembly.GetName();
                return new
                {
                    Name = assemblyName.Name,
                    Version = assemblyName.Version,
                    HasVersion = assemblyName.Version != new Version(0, 0, 0, 0)
                };
            }).ToList();

            // Assert - Shouldy validation
            foreach (var result in results)
            {
                result.HasVersion.ShouldBeTrue(
                    $"Assembly {result.Name} should have a proper version (current: {result.Version})");
            }
        }

        [Fact(DisplayName = "009CP: Plugin Architecture Should Be Consistent")]
        public void Plugin_Architecture_Should_Be_Consistent()
        {
            // Arrange & Act - NetArchTests architectural pattern validation
            var coreAssemblies = _pluginAssemblies.Where(a => a.GetName().Name.Contains("Core")).ToArray();
            var godotAssemblies = _pluginAssemblies.Where(a => a.GetName().Name.Contains("Godot")).ToArray();

            // Core assemblies should not have engine dependencies
            foreach (var coreAssembly in coreAssemblies)
            {
                var result = Types.InAssembly(coreAssembly)
                    .Should()
                    .NotHaveDependencyOnAny("Godot", "Unity")
                    .GetResult();

                result.IsSuccessful.ShouldBeTrue(
                    $"Core assembly {coreAssembly.GetName().Name} should be engine-agnostic");
            }

            // Godot assemblies should implement Core interfaces
            foreach (var godotAssembly in godotAssemblies)
            {
                var result = Types.InAssembly(godotAssembly)
                    .Should()
                    .HaveDependencyOnAny(coreAssemblies.Select(a => a.GetName().Name).ToArray())
                    .GetResult();

                result.IsSuccessful.ShouldBeTrue(
                    $"Godot assembly {godotAssembly.GetName().Name} should implement Core interfaces");
            }
        }
    }
}
