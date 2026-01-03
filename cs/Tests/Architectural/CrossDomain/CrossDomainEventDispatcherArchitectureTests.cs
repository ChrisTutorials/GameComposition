using Xunit;
using NetArchTest.Rules;
using Shouldly;
using System.Linq;
using System.Reflection;
using System.Collections.Generic;
using BarkMoon.GameComposition.Core.Interfaces;
using BarkMoon.GameComposition.Tests.Common;

// Explicitly alias the NetArchTest Types class to avoid conflict
using ArchTypes = NetArchTest.Rules;

namespace BarkMoon.GameComposition.Core.Tests.Architectural
{
    /// <summary>
    /// Cross-domain architectural tests that enforce event dispatcher patterns across ALL plugins.
    /// Ensures proper Core/Godot separation: Core events → Godot Presenters → Godot Adapters → Views.
    /// Tests are loaded dynamically from all plugin assemblies to enforce universal compliance.
    /// </summary>
    public class CrossDomainEventDispatcherArchitectureTests
    {
        /// <summary>
        /// Rule 1: Event Dispatcher interfaces must live in Core across ALL plugins.
        /// Godot side should only implement/consume, not define event interfaces.
        /// </summary>
        [Fact]
        [Trait("Category", "Architectural")]
        public void Event_Dispatcher_Interfaces_Must_Live_In_Core_Across_All_Plugins()
        {
            // Arrange - Use SSOT helper for assembly loading
            var assemblies = ArchitecturalTestHelpers.GetAllAssemblies();
            var allViolations = new List<string>();

            foreach (var assembly in assemblies)
            {
                // Check for event dispatcher interfaces in non-Core locations
                var eventInterfaces = ArchTypes.InAssembly(assembly)
                    .That()
                    .ImplementInterface(typeof(IEventDispatcher))
                    .And()
                    .AreInterfaces()
                    .GetTypes().ToArray();

                if (eventInterfaces.Length > 0)
                {
                    // Verify they're in Core namespace
                    foreach (var eventInterface in eventInterfaces)
                    {
                        if (!eventInterface.FullName.Contains("Core", StringComparison.OrdinalIgnoreCase))
                        {
                            allViolations.Add(
                                $"Assembly {assembly.GetName().Name}: Event bus interface {eventInterface.Name} must live in Core namespace, found in {eventInterface.Namespace}");
                        }
                    }
                }

                // Check for event-related interfaces outside Core
                var eventRelatedInterfaces = ArchTypes.InAssembly(assembly)
                    .That()
                    .HaveNameEndingWith("Event", StringComparison.OrdinalIgnoreCase)
                    .And()
                    .AreInterfaces()
                    .GetTypes().ToArray();

                foreach (var eventInterface in eventRelatedInterfaces)
                {
                    if (!eventInterface.FullName.Contains("Core", StringComparison.OrdinalIgnoreCase))
                    {
                        allViolations.Add(
                            $"Assembly {assembly.GetName().Name}: Event Dispatcher interface {eventInterface.Name} must live in Core namespace, found in {eventInterface.Namespace}");
                    }
                }
            }

            // Assert across all plugins
            if (allViolations.Count > 0)
            {
                var errorMessage = $"Event Dispatcher interfaces must live in Core across all plugins. Violations:\n{string.Join("\n", allViolations)}";
                throw new System.InvalidOperationException(errorMessage);
            }
        }

        /// <summary>
        /// Rule 2: Presenter classes must live on Godot side across ALL plugins.
        /// Presenters convert Core events into Godot-compatible models.
        /// </summary>
        [Fact]
        [Trait("Category", "Architectural")]
        public void Presenter_Classes_Must_Live_On_Godot_Side_Across_All_Plugins()
        {
            // Arrange - Use SSOT helper for assembly loading
            var assemblies = ArchitecturalTestHelpers.GetAllAssemblies();
            var allViolations = new List<string>();

            foreach (var assembly in assemblies)
            {
                // Find presenter classes
                var presenterClasses = ArchTypes.InAssembly(assembly)
                    .That()
                    .HaveNameEndingWith("Presenter", StringComparison.OrdinalIgnoreCase)
                    .And()
                    .AreClasses()
                    .And()
                    .AreNotAbstract()
                    .GetTypes().ToArray();

                foreach (var presenterClass in presenterClasses)
                {
                    // Presenters should NOT be in Core (they're Godot-side)
                    if (presenterClass.FullName.Contains("Core", StringComparison.OrdinalIgnoreCase))
                    {
                        allViolations.Add(
                            $"Assembly {assembly.GetName().Name}: Presenter class {presenterClass.Name} must live on Godot side, found in Core namespace {presenterClass.Namespace}");
                    }

                    // Presenters should be in Godot namespace
                    if (!presenterClass.FullName.Contains("Godot", StringComparison.OrdinalIgnoreCase))
                    {
                        allViolations.Add(
                            $"Assembly {assembly.GetName().Name}: Presenter class {presenterClass.Name} should be in Godot namespace, found in {presenterClass.Namespace}");
                    }
                }
            }

            // Assert across all plugins
            if (allViolations.Count > 0)
            {
                var errorMessage = $"Presenter classes must live on Godot side across all plugins. Violations:\n{string.Join("\n", allViolations)}";
                throw new System.InvalidOperationException(errorMessage);
            }
        }

        /// <summary>
        /// Rule 3: Adapter classes must properly bridge Core and Godot across ALL plugins.
        /// Adapters should consume presenter outputs and provide view models.
        /// </summary>
        [Fact]
        [Trait("Category", "Architectural")]
        public void Adapter_Classes_Must_Bridge_Core_And_Godot_Across_All_Plugins()
        {
            // Arrange - Use SSOT helper for assembly loading
            var assemblies = ArchitecturalTestHelpers.GetAllAssemblies();
            var allViolations = new List<string>();

            foreach (var assembly in assemblies)
            {
                // Find adapter classes
                var adapterClasses = ArchTypes.InAssembly(assembly)
                    .That()
                    .HaveNameEndingWith("Adapter", StringComparison.OrdinalIgnoreCase)
                    .And()
                    .AreClasses()
                    .And()
                    .AreNotAbstract()
                    .GetTypes().ToArray();

                foreach (var adapterClass in adapterClasses)
                {
                    // Adapters should implement Core interfaces
                    var coreInterfaces = adapterClass.GetInterfaces()
                        .Where(i => i.FullName.Contains("Core", StringComparison.OrdinalIgnoreCase))
                        .ToList();

                    if (!coreInterfaces.Any())
                    {
                        allViolations.Add(
                            $"Assembly {assembly.GetName().Name}: Adapter class {adapterClass.Name} should implement Core interfaces for proper bridging");
                    }

                    // Adapters should have Godot dependencies (signals, nodes, etc.)
                    var godotReferences = adapterClass.GetReferencedAssemblies()
                        .Any(r => r.Name.Contains("Godot", StringComparison.OrdinalIgnoreCase));

                    if (!godotReferences && !adapterClass.FullName.Contains("Core"))
                    {
                        // If it's not in Core and doesn't reference Godot, it might be misplaced
                        allViolations.Add(
                            $"Assembly {assembly.GetName().Name}: Adapter class {adapterClass.Name} should bridge Core and Godot, found in {adapterClass.Namespace}");
                    }
                }
            }

            // Assert across all plugins
            if (allViolations.Count > 0)
            {
                var errorMessage = $"Adapter classes must properly bridge Core and Godot across all plugins. Violations:\n{string.Join("\n", allViolations)}";
                throw new System.InvalidOperationException(errorMessage);
            }
        }

        /// <summary>
        /// Rule 4: Event flow must be Core → Presenter → Adapter → View across ALL plugins.
        /// Validates proper architectural separation and data flow direction.
        /// </summary>
        [Fact]
        [Trait("Category", "Architectural")]
        public void Event_Flow_Must_Follow_Core_Presenter_Adapter_View_Pattern_Across_All_Plugins()
        {
            // Arrange - Use SSOT helper for assembly loading
            var assemblies = ArchitecturalTestHelpers.GetAllAssemblies();
            var allViolations = new List<string>();

            foreach (var assembly in assemblies)
            {
                // Check Core events
                var coreEvents = ArchTypes.InAssembly(assembly)
                    .That()
                    .ResideInNamespace("*.Core.*")
                    .And()
                    .HaveNameEndingWith("Event", StringComparison.OrdinalIgnoreCase)
                    .And()
                    .AreClasses()
                    .GetTypes().ToArray();

                // Check Godot presenters
                var godotPresenters = ArchTypes.InAssembly(assembly)
                    .That()
                    .ResideInNamespace("*.Godot.*")
                    .And()
                    .HaveNameEndingWith("Presenter", StringComparison.OrdinalIgnoreCase)
                    .And()
                    .AreClasses()
                    .GetTypes().ToArray();

                // Check adapters
                var adapters = ArchTypes.InAssembly(assembly)
                    .That()
                    .HaveNameEndingWith("Adapter", StringComparison.OrdinalIgnoreCase)
                    .And()
                    .AreClasses()
                    .GetTypes().ToArray();

                // Validate event flow: Core Events should not directly reference Godot
                foreach (var coreEvent in coreEvents)
                {
                    var godotReferences = coreEvent.GetReferencedAssemblies()
                        .Any(r => r.Name.Contains("Godot", StringComparison.OrdinalIgnoreCase));

                    if (godotReferences)
                    {
                        allViolations.Add(
                            $"Assembly {assembly.GetName().Name}: Core event {coreEvent.Name} should not reference Godot assemblies - breaks event flow pattern");
                    }
                }

                // Validate presenter flow: Presenters should handle Core events
                foreach (var presenter in godotPresenters)
                {
                    var coreEventReferences = presenter.GetMethods()
                        .SelectMany(m => m.GetParameters())
                        .Any(p => p.ParameterType.Name.Contains("Event", StringComparison.OrdinalIgnoreCase) && 
                                   p.ParameterType.FullName.Contains("Core", StringComparison.OrdinalIgnoreCase));

                    if (!coreEventReferences)
                    {
                        allViolations.Add(
                            $"Assembly {assembly.GetName().Name}: Presenter {presenter.Name} should handle Core events for proper event flow");
                    }
                }
            }

            // Assert across all plugins
            if (allViolations.Count > 0)
            {
                var errorMessage = $"Event flow must follow Core → Presenter → Adapter → View pattern across all plugins. Violations:\n{string.Join("\n", allViolations)}";
                throw new System.InvalidOperationException(errorMessage);
            }
        }

        /// <summary>
        /// Rule 5: Event Dispatcher implementations must be properly scoped across ALL plugins.
        /// Core defines interfaces, implementations can be in Core or Godot depending on use case.
        /// </summary>
        [Fact]
        [Trait("Category", "Architectural")]
        public void Event_Dispatcher_Implementations_Must_Be_Properly_Scoped_Across_All_Plugins()
        {
            // Arrange - Use SSOT helper for assembly loading
            var assemblies = ArchitecturalTestHelpers.GetAllAssemblies();
            var allViolations = new List<string>();

            foreach (var assembly in assemblies)
            {
                // Find Event Dispatcher implementations
                var eventDispatcherImplementations = ArchTypes.InAssembly(assembly)
                    .That()
                    .ImplementInterface(typeof(IEventDispatcher))
                    .And()
                    .AreClasses()
                    .GetTypes().ToArray();

                foreach (var implementation in eventDispatcherImplementations)
                {
                    // Event Dispatcher implementations can be in Core (domain-specific) or Godot (engine-specific)
                    // Domain event dispatchers should be in Core
                    if (implementation.Name.Contains("EventDispatcher") && 
                        implementation.Name.Contains("Domain") &&
                        !implementation.FullName.Contains("Core", StringComparison.OrdinalIgnoreCase))
                    {
                        allViolations.Add(
                            $"Assembly {assembly.GetName().Name}: Domain Event Dispatcher implementation {implementation.Name} should be in Core namespace, found in {implementation.Namespace}");
                    }

                    // Should have engine-specific dependencies
                    var hasEngineDependencies = implementation.GetFields()
                        .Any(f => f.FieldType.FullName.Contains("Godot", StringComparison.OrdinalIgnoreCase)) ||
                        implementation.GetMethods()
                            .SelectMany(m => m.GetParameters())
                            .Any(p => p.ParameterType.FullName.Contains("Godot", StringComparison.OrdinalIgnoreCase));

                    if (!hasEngineDependencies && !implementation.FullName.Contains("Core", StringComparison.OrdinalIgnoreCase))
                    {
                        allViolations.Add(
                            $"Assembly {assembly.GetName().Name}: Event Bus implementation {implementation.Name} should have engine-specific dependencies");
                    }
                }
            }

            // Assert across all plugins
            if (allViolations.Count > 0)
            {
                var errorMessage = $"Event Bus implementations must be properly scoped across all plugins. Violations:\n{string.Join("\n", allViolations)}";
                throw new System.InvalidOperationException(errorMessage);
            }
        }

        /// <summary>
        /// Rule 6: Each domain with DomainPresenter interfaces must have a domain-specific event bus.
        /// Enforces hierarchical event bus architecture for proper domain isolation.
        /// </summary>
        [Fact]
        [Trait("Category", "Architectural")]
        public void Domains_With_DomainPresenter_Must_Have_Domain_Event_Bus_Across_All_Plugins()
        {
            // Arrange - Use SSOT helper for assembly loading
            var assemblies = ArchitecturalTestHelpers.GetAllAssemblies();
            var allViolations = new List<string>();

            foreach (var assembly in assemblies)
            {
                // Find all DomainPresenter interfaces
                var domainPresenterInterfaces = ArchTypes.InAssembly(assembly)
                    .That()
                    .HaveNameEndingWith("DomainPresenter", StringComparison.OrdinalIgnoreCase)
                    .And()
                    .AreInterfaces()
                    .GetTypes().ToArray();

                if (domainPresenterInterfaces.Length == 0)
                    continue; // No domain presenters in this assembly

                // For each domain with presenters, check for domain-specific event bus
                foreach (var domainPresenterInterface in domainPresenterInterfaces)
                {
                    // Extract domain name from interface (e.g., IPlacementDomainPresenter -> Placement)
                    var domainName = ExtractDomainNameFromInterface(domainPresenterInterface.Name);
                    
                    // Look for domain-specific event bus implementation
                    var domainEventBusTypes = ArchTypes.InAssembly(assembly)
                        .That()
                        .HaveNameContaining($"{domainName}EventBus", StringComparison.OrdinalIgnoreCase)
                        .Or()
                        .HaveNameContaining($"{domainName}EventDispatcher", StringComparison.OrdinalIgnoreCase)
                        .And()
                        .ImplementInterface(typeof(IEventDispatcher))
                        .And()
                        .AreClasses()
                        .And()
                        .AreNotAbstract()
                        .GetTypes().ToArray();

                    if (domainEventBusTypes.Length == 0)
                    {
                        allViolations.Add(
                            $"Assembly {assembly.GetName().Name}: Domain {domainName} has DomainPresenter interface {domainPresenterInterface.Name} but no domain-specific event bus implementation");
                    }
                }
            }

            // Assert across all plugins
            if (allViolations.Count > 0)
            {
                var errorMessage = $"Domains with DomainPresenter interfaces must have domain-specific event buses across all plugins. Violations:\n{string.Join("\n", allViolations)}";
                throw new System.InvalidOperationException(errorMessage);
            }
        }

        /// <summary>
        /// Rule 7: Each plugin must have a global event bus for cross-domain coordination.
        /// Ensures hierarchical event bus architecture with global coordination capabilities.
        /// </summary>
        [Fact]
        [Trait("Category", "Architectural")]
        public void Each_Plugin_Must_Have_Global_Event_Bus_Across_All_Plugins()
        {
            // Arrange - Use SSOT helper for assembly loading
            var assemblies = ArchitecturalTestHelpers.GetAllAssemblies();
            var allViolations = new List<string>();

            foreach (var assembly in assemblies)
            {
                // Look for global event bus implementations
                var globalEventBusTypes = ArchTypes.InAssembly(assembly)
                    .That()
                    .HaveNameContaining("GlobalEventBus", StringComparison.OrdinalIgnoreCase)
                    .Or()
                    .HaveNameContaining("PluginEventBus", StringComparison.OrdinalIgnoreCase)
                    .Or()
                    .HaveNameContaining("CoordinatorEventBus", StringComparison.OrdinalIgnoreCase)
                    .And()
                    .ImplementInterface(typeof(IEventDispatcher))
                    .And()
                    .AreClasses()
                    .And()
                    .AreNotAbstract()
                    .GetTypes().ToArray();

                if (globalEventBusTypes.Length == 0)
                {
                    // Check if this is a plugin assembly (contains domain presenters)
                    var hasDomainPresenters = ArchTypes.InAssembly(assembly)
                        .That()
                        .HaveNameEndingWith("DomainPresenter", StringComparison.OrdinalIgnoreCase)
                        .And()
                        .AreInterfaces()
                        .GetTypes().Any();

                    if (hasDomainPresenters)
                    {
                        allViolations.Add(
                            $"Assembly {assembly.GetName().Name}: Plugin with DomainPresenter interfaces must have a global event bus for cross-domain coordination");
                    }
                }
            }

            // Assert across all plugins
            if (allViolations.Count > 0)
            {
                var errorMessage = $"Event Dispatcher implementations must be properly scoped across all plugins. Violations:\n{string.Join("\n", allViolations)}";
                throw new System.InvalidOperationException(errorMessage);
            }
        }

        /// <summary>
        /// Rule 6: GlobalEventBus pattern is prohibited across ALL plugins.
        /// Services must use IEventDispatcher injection instead of global state.
        /// </summary>
        [Fact]
        [Trait("Category", "Architectural")]
        public void GlobalEventBus_Pattern_Must_Be_Prohibited_Across_All_Plugins()
        {
            // Arrange - Use SSOT helper for assembly loading
            var assemblies = ArchitecturalTestHelpers.GetAllAssemblies();
            var allViolations = new List<string>();

            foreach (var assembly in assemblies)
            {
                // Check for GlobalEventBus classes (prohibited pattern)
                var globalEventBusClasses = ArchTypes.InAssembly(assembly)
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

                // Check for classes with "EventBus" suffix that might be global patterns
                var eventBusClasses = ArchTypes.InAssembly(assembly)
                    .That()
                    .HaveNameEndingWith("EventBus", StringComparison.OrdinalIgnoreCase)
                    .And()
                    .AreClasses()
                    .And()
                    .ArePublic()
                    .GetTypes().ToArray();

                foreach (var eventBusClass in eventBusClasses)
                {
                    // Allow domain-specific event buses (e.g., PlacementEventBus) but not global ones
                    if (!eventBusClass.Name.Contains("Global", StringComparison.OrdinalIgnoreCase) &&
                        (eventBusClass.Name.StartsWith("Placement", StringComparison.OrdinalIgnoreCase) ||
                         eventBusClass.Name.StartsWith("Grid", StringComparison.OrdinalIgnoreCase) ||
                         eventBusClass.Name.StartsWith("Targeting", StringComparison.OrdinalIgnoreCase)))
                    {
                        // These are acceptable domain-specific event buses
                        continue;
                    }

                    allViolations.Add(
                        $"Assembly {assembly.GetName().Name}: EventBus pattern {eventBusClass.Name} should be domain-specific (e.g., PlacementEventBus) and use IEventDispatcher injection.");
                }
            }

            // Assert across all plugins
            if (allViolations.Count > 0)
            {
                var errorMessage = $"GlobalEventBus pattern is prohibited. Use IEventDispatcher injection instead. Violations:\n{string.Join("\n", allViolations)}";
                throw new System.InvalidOperationException(errorMessage);
            }
        }

        #region Helper Methods

        /// <summary>
        /// Extracts domain name from interface name (e.g., IPlacementDomainPresenter -> Placement).
        /// </summary>
        private static string ExtractDomainNameFromInterface(string interfaceName)
        {
            if (interfaceName.StartsWith("I", StringComparison.OrdinalIgnoreCase))
            {
                interfaceName = interfaceName.Substring(1);
            }

            if (interfaceName.EndsWith("DomainPresenter", StringComparison.OrdinalIgnoreCase))
            {
                return interfaceName.Substring(0, interfaceName.Length - "DomainPresenter".Length);
            }

            return interfaceName;
        }

        #endregion
    }
}
