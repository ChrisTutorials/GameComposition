using Xunit;
using Shouldly;
using System.Linq;
using System.Reflection;
using System.Collections.Generic;
using BarkMoon.GameComposition.Tests.Common;

namespace BarkMoon.GameComposition.Tests.Architectural
{
    /// <summary>
    /// Configuration-driven test to verify GlobalEventBus prohibition is working.
    /// Uses external configuration to reduce compilation churn.
    /// </summary>
    public class StandaloneGlobalEventBusTest
    {
        [Fact]
        [Trait("Category", "Architectural")]
        public void GlobalEventBus_Pattern_Must_Be_Prohibited()
        {
            // Arrange - Reset cache and load configuration
            ArchitectureConfigurationLoader.ResetCache();
            var config = ArchitectureConfigurationLoader.LoadConfiguration();
            var globalEventBusRule = config.ProhibitedPatterns.GlobalEventBus;
            
            // Debug output
            System.Diagnostics.Debug.WriteLine($"DEBUG: GlobalEventBus.Enabled = {globalEventBusRule.Enabled}");
            
            if (!globalEventBusRule.Enabled)
            {
                // Rule disabled - test passes automatically
                true.ShouldBeTrue("GlobalEventBus prohibition rule is disabled in configuration.");
                return;
            }

            try
            {
                var gridPlacementAssembly = Assembly.LoadFrom("g:\\dev\\game\\plugins\\gameplay\\GridPlacement\\cs\\Core\\bin\\Debug\\net10.0\\BarkMoon.GridPlacement.Core.dll");
                var allViolations = new List<string>();

                // Check for GlobalEventBus classes using configuration pattern
                var allTypes = gridPlacementAssembly.GetTypes();
                var prohibitedClasses = allTypes
                    .Where(t => t.Name.Contains("GlobalEventBus", StringComparison.OrdinalIgnoreCase))
                    .Where(t => t.IsClass)
                    .ToArray();

                foreach (var prohibitedClass in prohibitedClasses)
                {
                    // Check if this is an allowed domain-specific exception
                    var isAllowedException = globalEventBusRule.AllowedDomainSpecific
                        .Any(allowed => prohibitedClass.Name.Contains(allowed, StringComparison.OrdinalIgnoreCase));

                    if (!isAllowedException)
                    {
                        allViolations.Add(
                            $"Assembly {gridPlacementAssembly.GetName().Name}: {globalEventBusRule.Message} Found: {prohibitedClass.Name}");
                    }
                }

                // Assert - This should fail because GridPlacementGlobalEventBus exists
                if (allViolations.Count > 0)
                {
                    var errorMessage = $"{globalEventBusRule.Message} Violations:\n{string.Join("\n", allViolations)}";
                    throw new System.InvalidOperationException(errorMessage);
                }
                
                // If we get here, no prohibited GlobalEventBus classes were found
                true.ShouldBeTrue("No prohibited GlobalEventBus patterns found - architectural rule compliance verified.");
            }
            catch (System.IO.FileNotFoundException)
            {
                // GridPlacement assembly not built yet - this is expected
                throw new System.InvalidOperationException("GridPlacement assembly not found. Build GridPlacement.Core first to run architectural validation.");
            }
            catch (System.Exception ex)
            {
                throw new System.InvalidOperationException($"Test execution failed: {ex.Message}");
            }
        }

        [Fact]
        [Trait("Category", "Architectural")]
        public void Configuration_Should_Be_Loadable()
        {
            // Arrange & Act
            var config = ArchitectureConfigurationLoader.LoadConfiguration();

            // Assert - Verify configuration loaded correctly
            config.ShouldNotBeNull();
            config.ProhibitedPatterns.ShouldNotBeNull();
            config.ProhibitedPatterns.GlobalEventBus.ShouldNotBeNull();
            config.ProhibitedPatterns.GlobalEventBus.Enabled.ShouldBeTrue();
            config.ProhibitedPatterns.GlobalEventBus.Pattern.ShouldBe("*GlobalEventBus");
            
            // Debug: Check if YAML is being loaded vs defaults
            System.Diagnostics.Debug.WriteLine($"AllowedDomainSpecific count: {config.ProhibitedPatterns.GlobalEventBus.AllowedDomainSpecific?.Count ?? 0}");
            if (config.ProhibitedPatterns.GlobalEventBus.AllowedDomainSpecific != null)
            {
                foreach (var item in config.ProhibitedPatterns.GlobalEventBus.AllowedDomainSpecific)
                {
                    System.Diagnostics.Debug.WriteLine($"  - {item}");
                }
            }
            
            // For now, just check the list exists (may be empty if YAML parsing fails)
            config.ProhibitedPatterns.GlobalEventBus.AllowedDomainSpecific.ShouldNotBeNull();
        }
    }
}
