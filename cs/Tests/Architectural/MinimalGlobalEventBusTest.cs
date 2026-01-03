using Xunit;
using NetArchTest.Rules;
using Shouldly;
using System.Linq;
using System.Reflection;
using System.Collections.Generic;
using BarkMoon.GameComposition.Tests.Common;

namespace BarkMoon.GameComposition.Tests.Architectural
{
    /// <summary>
    /// Test to verify GlobalEventBus prohibition is working.
    /// </summary>
    public class GlobalEventBusProhibitionTests
    {
        [Fact]
        [Trait("Category", "Architectural")]
        public void GlobalEventBus_Pattern_Must_Be_Prohibited()
        {
            // Arrange - Load GridPlacement assembly specifically
            try
            {
                var gridPlacementAssembly = Assembly.LoadFrom("g:\\dev\\game\\plugins\\gameplay\\GridPlacement\\cs\\Core\\bin\\Debug\\net10.0\\BarkMoon.GridPlacement.Core.dll");
                var assemblies = new[] { gridPlacementAssembly };
                var allViolations = new List<string>();

                foreach (var assembly in assemblies)
                {
                    // Check for GlobalEventBus classes (prohibited pattern)
                    var globalEventBusClasses = Types.InAssembly(assembly)
                        .That()
                        .HaveNameContaining("GlobalEventBus", StringComparison.OrdinalIgnoreCase)
                        .And()
                        .AreClasses()
                        .GetTypes().ToArray();

                    foreach (var globalEventBusClass in globalEventBusClasses)
                    {
                        allViolations.Add(
                            $"Assembly {assembly.GetName().Name}: GlobalEventBus pattern {globalEventBusClass.Name} is prohibited. Use IEventDispatcher injection instead.");
                    }
                }

                // Assert - This should fail because GridPlacementGlobalEventBus exists
                if (allViolations.Count > 0)
                {
                    var errorMessage = $"GlobalEventBus pattern is prohibited. Use IEventDispatcher injection instead. Violations:\n{string.Join("\n", allViolations)}";
                    throw new System.InvalidOperationException(errorMessage);
                }
            }
            catch (System.Exception ex)
            {
                // If we can't load the assembly, at least verify our test structure is correct
                throw new System.InvalidOperationException($"Test setup failed: {ex.Message}. This indicates the architectural test framework is working but needs the GridPlacement assembly to be built first.");
            }
        }
    }
}
