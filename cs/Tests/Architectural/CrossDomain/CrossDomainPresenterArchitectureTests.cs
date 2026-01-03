using Xunit;
using NetArchTest.Rules;
using Shouldly;
using System.Linq;
using System.Reflection;
using System.Collections.Generic;
using BarkMoon.GameComposition.Tests.Common;

// Explicitly alias the NetArchTest Types class to avoid conflict
using ArchTypes = NetArchTest.Rules.Types;

namespace BarkMoon.GameComposition.Core.Tests.Architectural
{
    /// <summary>
    /// Cross-domain architectural tests that enforce presenter patterns across ALL plugins.
    /// Ensures proper Core/Godot separation: Presenters live on Godot side, consume Core events.
    /// Tests are loaded dynamically from all plugin assemblies to enforce universal compliance.
    /// </summary>
    public class CrossDomainPresenterArchitectureTests
    {
        /// <summary>
        /// Rule 1: Presenter interfaces must be defined in Core across ALL plugins.
        /// Godot side implements these interfaces for concrete presenters.
        /// </summary>
        [Fact]
        [Trait("Category", "Architectural")]
        public void Presenter_Interfaces_Must_Be_Defined_In_Core_Across_All_Plugins()
        {
            // Arrange - Use SSOT helper for assembly loading
            var assemblies = ArchitecturalTestHelpers.GetAllAssemblies();
            var allViolations = new List<string>();

            foreach (var assembly in assemblies)
            {
                // Find presenter interfaces
                var presenterInterfaces = ArchTypes.InAssembly(assembly)
                    .That()
                    .HaveNameEndingWith("Presenter", StringComparison.OrdinalIgnoreCase)
                    .And()
                    .AreInterfaces()
                    .GetTypes().ToArray();

                foreach (var presenterInterface in presenterInterfaces)
                {
                    // Presenter interfaces must be in Core namespace
                    if (!presenterInterface.FullName.Contains("Core", StringComparison.OrdinalIgnoreCase))
                    {
                        allViolations.Add(
                            $"Assembly {assembly.GetName().Name}: Presenter interface {presenterInterface.Name} must be defined in Core namespace, found in {presenterInterface.Namespace}");
                    }
                }
            }

            // Assert across all plugins
            if (allViolations.Count > 0)
            {
                var errorMessage = $"Presenter interfaces must be defined in Core across all plugins. Violations:\n{string.Join("\n", allViolations)}";
                throw new System.InvalidOperationException(errorMessage);
            }
        }

        /// <summary>
        /// Rule 2: Presenter implementations must live on Godot side across ALL plugins.
        /// Concrete presenters convert Core events to Godot-compatible data.
        /// </summary>
        [Fact]
        [Trait("Category", "Architectural")]
        public void Presenter_Implementations_Must_Live_On_Godot_Side_Across_All_Plugins()
        {
            // Arrange - Use SSOT helper for assembly loading
            var assemblies = ArchitecturalTestHelpers.GetAllAssemblies();
            var allViolations = new List<string>();

            foreach (var assembly in assemblies)
            {
                // Find presenter implementations
                var presenterImplementations = ArchTypes.InAssembly(assembly)
                    .That()
                    .HaveNameEndingWith("Presenter", StringComparison.OrdinalIgnoreCase)
                    .And()
                    .AreClasses()
                    .And()
                    .AreNotAbstract()
                    .GetTypes().ToArray();

                foreach (var presenterImplementation in presenterImplementations)
                {
                    // Presenter implementations must be in Godot namespace
                    if (!presenterImplementation.Any(r => r.Name.Contains("Godot", StringComparison.OrdinalIgnoreCase)))
                    {
                        allViolations.Add(
                            $"Assembly {assembly.GetName().Name}: Presenter implementation {presenterImplementation.Name} must live on Godot side, found in {presenterImplementation.Namespace}");
                    }

                    // Should implement Core presenter interfaces
                    var corePresenterInterfaces = presenterImplementation.GetInterfaces()
                        .Where(i => i.Name.EndsWith("Presenter", StringComparison.OrdinalIgnoreCase) &&
                                   i.FullName.Contains("Core", StringComparison.OrdinalIgnoreCase))
                        .ToList();

                    if (!corePresenterInterfaces.Any())
                    {
                        allViolations.Add(
                            $"Assembly {assembly.GetName().Name}: Presenter implementation {presenterImplementation.Name} should implement Core presenter interfaces");
                    }
                }
            }

            // Assert across all plugins
            if (allViolations.Count > 0)
            {
                var errorMessage = $"Presenter implementations must live on Godot side across all plugins. Violations:\n{string.Join("\n", allViolations)}";
                throw new System.InvalidOperationException(errorMessage);
            }
        }

        /// <summary>
        /// Rule 3: Presenters must consume Core events and produce Godot-compatible models across ALL plugins.
        /// Validates proper data transformation and architectural boundaries.
        /// </summary>
        [Fact]
        [Trait("Category", "Architectural")]
        public void Presenters_Must_Consume_Core_Events_And_Produce_Godot_Models_Across_All_Plugins()
        {
            // Arrange - Use SSOT helper for assembly loading
            var assemblies = ArchitecturalTestHelpers.GetAllAssemblies();
            var allViolations = new List<string>();

            foreach (var assembly in assemblies)
            {
                // Find presenter implementations on Godot side
                var presenterImplementations = ArchTypes.InAssembly(assembly)
                    .That()
                    .ResideInNamespace("*.Godot.*")
                    .And()
                    .HaveNameEndingWith("Presenter", StringComparison.OrdinalIgnoreCase)
                    .And()
                    .AreClasses()
                    .GetTypes().ToArray();

                foreach (var presenterImplementation in presenterImplementations)
                {
                    // Check for Core event consumption
                    var methods = presenterImplementation.GetMethods();
                    var consumesCoreEvents = false;
                    var producesGodotModels = false;

                    foreach (var method in methods)
                    {
                        // Check if method consumes Core events
                        var parameters = method.GetParameters();
                        if (parameters.Any(p => p.ParameterType.Name.Contains("Event", StringComparison.OrdinalIgnoreCase) && 
                                             p.ParameterType.FullName.Contains("Core", StringComparison.OrdinalIgnoreCase)))
                        {
                            consumesCoreEvents = true;
                        }

                        // Check if method produces Godot-compatible models
                        var returnType = method.ReturnType;
                        if (returnType.Name.Contains("Model") || 
                            returnType.Name.Contains("ViewModel") ||
                            returnType.Any(r => r.Name.Contains("Godot", StringComparison.OrdinalIgnoreCase)))
                        {
                            producesGodotModels = true;
                        }
                    }

                    if (!consumesCoreEvents)
                    {
                        allViolations.Add(
                            $"Assembly {assembly.GetName().Name}: Presenter {presenterImplementation.Name} should consume Core events");
                    }

                    if (!producesGodotModels)
                    {
                        allViolations.Add(
                            $"Assembly {assembly.GetName().Name}: Presenter {presenterImplementation.Name} should produce Godot-compatible models or view models");
                    }
                }
            }

            // Assert across all plugins
            if (allViolations.Count > 0)
            {
                var errorMessage = $"Presenters must consume Core events and produce Godot models across all plugins. Violations:\n{string.Join("\n", allViolations)}";
                throw new System.InvalidOperationException(errorMessage);
            }
        }

        /// <summary>
        /// Rule 4: Presenters must not contain business logic across ALL plugins.
        /// Presenters should only transform data, not implement business rules.
        /// </summary>
        [Fact]
        [Trait("Category", "Architectural")]
        public void Presenters_Must_Not_Contain_Business_Logic_Across_All_Plugins()
        {
            // Arrange - Use SSOT helper for assembly loading
            var assemblies = ArchitecturalTestHelpers.GetAllAssemblies();
            var allViolations = new List<string>();

            foreach (var assembly in assemblies)
            {
                // Find presenter implementations
                var presenterImplementations = ArchTypes.InAssembly(assembly)
                    .That()
                    .HaveNameEndingWith("Presenter", StringComparison.OrdinalIgnoreCase)
                    .And()
                    .AreClasses()
                    .GetTypes().ToArray();

                foreach (var presenterImplementation in presenterImplementations)
                {
                    // Check for complex business logic methods
                    var methods = presenterImplementation.GetMethods()
                        .Where(m => !m.IsConstructor && 
                                   !m.IsSpecialName && 
                                   m.IsPublic &&
                                   m.Name != "ToString" &&
                                   m.Name != "GetHashCode" &&
                                   m.Name != "Equals" &&
                                   m.Name != "Initialize" &&
                                   m.Name != "Cleanup" &&
                                   !m.Name.StartsWith("On") && // Event handlers are expected
                                   !m.Name.StartsWith("Set")) // Property setters are expected
                        .ToList();

                    foreach (var method in methods)
                    {
                        // Check if method has complex logic (multiple statements, loops, etc.)
                        var methodBody = method.GetMethodBody();
                        if (methodBody != null && methodBody.GetILAsByteArray().Length > 100) // Heuristic for complex methods
                        {
                            allViolations.Add(
                                $"Assembly {assembly.GetName().Name}: Presenter {presenterImplementation.Name} method {method.Name} appears to contain complex business logic - presenters should only transform data");
                        }
                    }
                }
            }

            // Assert across all plugins
            if (allViolations.Count > 0)
            {
                var errorMessage = $"Presenters must not contain business logic across all plugins. Violations:\n{string.Join("\n", allViolations)}";
                throw new System.InvalidOperationException(errorMessage);
            }
        }

        /// <summary>
        /// Rule 5: Presenters must properly manage event subscriptions across ALL plugins.
        /// Presenters should subscribe to Core events and cleanup properly.
        /// </summary>
        [Fact]
        [Trait("Category", "Architectural")]
        public void Presenters_Must_Properly_Manage_Event_Subscriptions_Across_All_Plugins()
        {
            // Arrange - Use SSOT helper for assembly loading
            var assemblies = ArchitecturalTestHelpers.GetAllAssemblies();
            var allViolations = new List<string>();

            foreach (var assembly in assemblies)
            {
                // Find presenter implementations
                var presenterImplementations = ArchTypes.InAssembly(assembly)
                    .That()
                    .HaveNameEndingWith("Presenter", StringComparison.OrdinalIgnoreCase)
                    .And()
                    .AreClasses()
                    .GetTypes().ToArray();

                foreach (var presenterImplementation in presenterImplementations)
                {
                    var methods = presenterImplementation.GetMethods();
                    var hasInitialize = false;
                    var hasCleanup = false;
                    var hasEventDispatcher = false;

                    // Check for required lifecycle methods
                    foreach (var method in methods)
                    {
                        if (method.Name == "Initialize")
                            hasInitialize = true;
                        if (method.Name == "Cleanup")
                            hasCleanup = true;
                    }

                    // Check for event dispatcher field/property
                    var fields = presenterImplementation.GetFields();
                    var properties = presenterImplementation.GetProperties();
                    
                    hasEventDispatcher = fields.Any(f => f.FieldType == typeof(IEventDispatcher)) ||
                                       properties.Any(p => p.PropertyType == typeof(IEventDispatcher));

                    if (!hasInitialize)
                    {
                        allViolations.Add(
                            $"Assembly {assembly.GetName().Name}: Presenter {presenterImplementation.Name} should have Initialize method for event subscription setup");
                    }

                    if (!hasCleanup)
                    {
                        allViolations.Add(
                            $"Assembly {assembly.GetName().Name}: Presenter {presenterImplementation.Name} should have Cleanup method for event subscription cleanup");
                    }

                    if (!hasEventDispatcher)
                    {
                        allViolations.Add(
                            $"Assembly {assembly.GetName().Name}: Presenter {presenterImplementation.Name} should have IEventDispatcher field/property for event management");
                    }
                }
            }

            // Assert across all plugins
            if (allViolations.Count > 0)
            {
                var errorMessage = $"Presenters must properly manage event subscriptions across all plugins. Violations:\n{string.Join("\n", allViolations)}";
                throw new System.InvalidOperationException(errorMessage);
            }
        }
    }
}
