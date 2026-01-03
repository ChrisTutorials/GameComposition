using Xunit;
using NetArchTest.Rules;
using Shouldly;
using System.Linq;
using System.Reflection;
using System.Collections.Generic;
using BarkMoon.GameComposition.Tests.Common;

// Explicitly alias the NetArchTest Types class to avoid conflict
using ArchTypes = NetArchTest.Rules;

namespace BarkMoon.GameComposition.Tests.Architectural
{
    /// <summary>
    /// Test to verify GlobalEventDispatcher prohibition is working.
    /// DEPRECATED: Use ConfigurationDrivenArchitectureTests instead.
    /// This test is maintained for backward compatibility but will be removed in future versions.
    /// </summary>
    public class GlobalEventDispatcherProhibitionTests
    {
        [Fact]
        [Trait("Category", "Architectural")]
        public void GlobalEventDispatcher_Pattern_Must_Be_Prohibited()
        {
            // Arrange - Load configuration
            var config = ArchitectureConfigLoader.LoadConfig();
            var globalEventBusConfig = config.ProhibitedPatterns.GlobalEventBus;
            
            if (!globalEventBusConfig.Enabled)
            {
                // Rule disabled - skip test
                return;
            }

            // Load GridPlacement assembly specifically
            var gridPlacementAssembly = Assembly.LoadFrom("g:\\dev\\game\\plugins\\gameplay\\GridPlacement\\cs\\Core\\bin\\Debug\\net10.0\\BarkMoon.GridPlacement.Core.dll");
            var assemblies = new[] { gridPlacementAssembly };
            var allViolations = new List<string>();

            foreach (var assembly in assemblies)
            {
                // Check for GlobalEventBus classes (prohibited pattern)
                var globalEventBusClasses = ArchTypes.InAssembly(assembly)
                    .That()
                    .HaveNameContaining(globalEventBusConfig.Pattern, StringComparison.OrdinalIgnoreCase)
                    .And()
                    .AreClasses()
                    .GetTypes().ToArray();

                foreach (var globalEventBusClass in globalEventBusClasses)
                {
                    // Skip allowed domain-specific event buses
                    if (globalEventBusConfig.AllowedDomainSpecific.Any(allowed => 
                        globalEventBusClass.Name.Contains(allowed, StringComparison.OrdinalIgnoreCase)))
                        continue;
                        
                    allViolations.Add(
                        $"Assembly {assembly.GetName().Name}: GlobalEventBus pattern {globalEventBusClass.Name} is prohibited. {globalEventBusConfig.Message}");
                }
            }

            // Assert - This should fail if violations exist
            if (allViolations.Count > 0)
            {
                var errorMessage = $"{globalEventBusConfig.Message}\nViolations:\n{string.Join("\n", allViolations)}";
                throw new System.InvalidOperationException(errorMessage);
            }
        }
    }
}
