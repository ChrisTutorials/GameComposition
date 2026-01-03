using NetArchTest.Rules;
using Shouldly;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using BarkMoon.GameComposition.Core.Types;
using BarkMoon.GameComposition.Tests.Common;
using Xunit;

namespace BarkMoon.GameComposition.Core.Tests.Architectural
{
    /// <summary>
    /// Architectural tests that enforce front-end node adapter patterns across ALL plugins.
    /// Front-end nodes (Godot Nodes) should only own adapters, never services or business logic.
    /// This maintains clean separation between presentation layer and business logic layer.
    /// </summary>
    public class FrontEndNodeAdapterPatternTests : ArchitecturalTestBase
    {
        // DRY: Forbidden dependency patterns for front-end nodes
        private static readonly ForbiddenDependencyPattern[] _forbiddenPatterns = new[]
        {
            new ForbiddenDependencyPattern
            {
                Pattern = "Service",
                Description = "business logic services",
                Reason = "Front-end nodes should use adapters, not direct service dependencies"
            },
            new ForbiddenDependencyPattern
            {
                Pattern = "Workflow",
                Description = "workflow orchestrators",
                Reason = "Cross-domain coordination belongs in Core layer, not front-end nodes"
            },
            new ForbiddenDependencyPattern
            {
                Pattern = "Presenter",
                Description = "presenters",
                Reason = "Presenters should be owned by adapters, not front-end nodes"
            },
            new ForbiddenDependencyPattern
            {
                Pattern = "Settings",
                Description = "configuration settings",
                Reason = "Settings should be injected through adapters, not owned directly"
            },
            new ForbiddenDependencyPattern
            {
                Pattern = "Interpreter",
                Description = "input interpreters",
                Reason = "Input processing belongs in adapters, not front-end nodes"
            },
            new ForbiddenDependencyPattern
            {
                Pattern = "Processor",
                Description = "domain processors",
                Reason = "Business logic processors belong in services layer"
            },
            new ForbiddenDependencyPattern
            {
                Pattern = "State",
                Description = "domain state objects",
                Reason = "State management belongs in services, not front-end nodes"
            }
        };

        // DRY: Allowed adapter patterns for front-end nodes
        private static readonly AllowedAdapterPattern[] _allowedPatterns = new[]
        {
            new AllowedAdapterPattern
            {
                Pattern = "Adapter",
                Description = "adapters that bridge engine and Core layers"
            },
            new AllowedAdapterPattern
            {
                Pattern = "EventBus",
                Description = "event bus for communication"
            },
            new AllowedAdapterPattern
            {
                Pattern = "Registry",
                Description = "service registry for dependency injection"
            }
        };

        /// <summary>
        /// Rule 1: Front-end nodes should only depend on adapters, never services or business logic.
        /// This applies to ALL plugins in the ecosystem.
        /// </summary>
        [Fact]
        [Trait("Category", "Architectural")]
        public void Front_End_Nodes_Should_Only_Depend_On_Adapters_Across_All_Plugins()
        {
            // Arrange - Load all plugin assemblies
            var assemblies = AssembliesWithGodot;
            var allViolations = new List<string>();

            foreach (var assembly in assemblies)
            {
                var frontEndNodes = GetFrontEndNodes(assembly);
                
                foreach (var nodeType in frontEndNodes)
                {
                    var violations = ValidateFrontEndNodeDependencies(nodeType);
                    allViolations.AddRange(violations);
                }
            }

            // Assert across all plugins
            if (allViolations.Count > 0)
            {
                var errorMessage = $"Front-end nodes must only depend on adapters across all plugins. Violations:\n{string.Join("\n", allViolations)}";
                throw new InvalidOperationException(errorMessage);
            }
        }

        /// <summary>
        /// Rule 2: Front-end nodes should emit only ViewModelUpdated signals, not business events.
        /// This ensures clean separation between presentation and business logic.
        /// </summary>
        [Fact]
        [Trait("Category", "Architectural")]
        public void Front_End_Nodes_Should_Only_Emit_ViewModelUpdated_Signals_Across_All_Plugins()
        {
            // Arrange - Load all plugin assemblies
            var assemblies = AssembliesWithGodot;
            var allViolations = new List<string>();

            foreach (var assembly in assemblies)
            {
                var frontEndNodes = GetFrontEndNodes(assembly);
                
                foreach (var nodeType in frontEndNodes)
                {
                    var violations = ValidateSignalPatterns(nodeType);
                    allViolations.AddRange(violations);
                }
            }

            // Assert across all plugins
            if (allViolations.Count > 0)
            {
                var errorMessage = $"Front-end nodes must only emit ViewModelUpdated signals across all plugins. Violations:\n{string.Join("\n", allViolations)}";
                throw new InvalidOperationException(errorMessage);
            }
        }

        #region DRY Helper Methods

        /// <summary>
        /// Gets front-end node types from an assembly (Godot Nodes).
        /// </summary>
        private static IEnumerable<Type> GetFrontEndNodes(Assembly assembly)
        {
            return assembly.GetTypes()
                .Where(t => t.IsClass)
                .Where(t => typeof(Godot.Node).IsAssignableFrom(t))
                .Where(t => !t.IsAbstract)
                .Where(t => !t.Name.EndsWith("Tests")) // Exclude test classes
                .Where(t => !t.Name.EndsWith("Adapter")); // Adapters are not front-end nodes
        }

        /// <summary>
        /// Validates that a front-end node only depends on allowed adapter patterns.
        /// </summary>
        private static List<string> ValidateFrontEndNodeDependencies(Type nodeType)
        {
            var violations = new List<string>();
            var fieldTypes = nodeType.GetFields()
                .Where(f => !f.IsStatic)
                .Select(f => f.FieldType)
                .Concat(nodeType.GetProperties()
                    .Where(p => !p.GetIndexParameters().Any())
                    .Select(p => p.PropertyType));

            foreach (var fieldType in fieldTypes)
            {
                var fieldTypeName = fieldType.Name ?? string.Empty;
                
                // Check for forbidden patterns
                foreach (var forbidden in _forbiddenPatterns)
                {
                    if (fieldTypeName.Contains(forbidden.Pattern, StringComparison.OrdinalIgnoreCase))
                    {
                        violations.Add(
                            $"{nodeType.FullName}: Contains forbidden dependency '{fieldTypeName}' ({forbidden.Description}). " +
                            $"{forbidden.Reason}");
                    }
                }
            }

            return violations;
        }

        /// <summary>
        /// Validates that front-end nodes only emit ViewModelUpdated signals.
        /// </summary>
        private static List<string> ValidateSignalPatterns(Type nodeType)
        {
            var violations = new List<string>();
            
            // Check for [Signal] attributes (Godot signal pattern)
            var signalProperties = nodeType.GetProperties()
                .Where(p => p.GetCustomAttributes().Any(attr => 
                    attr.GetType().Name == "SignalAttribute"))
                .Select(p => p.Name);

            foreach (var signalName in signalProperties)
            {
                if (!signalName.Equals("ViewModelUpdated", StringComparison.OrdinalIgnoreCase))
                {
                    violations.Add(
                        $"{nodeType.FullName}: Contains forbidden signal '{signalName}'. " +
                        "Front-end nodes should only emit 'ViewModelUpdated' signals.");
                }
            }

            // Also check for method-based signals (common pattern)
            var signalMethods = nodeType.GetMethods()
                .Where(m => m.GetCustomAttributes().Any(attr => 
                    attr.GetType().Name == "SignalAttribute"))
                .Select(m => m.Name);

            foreach (var signalName in signalMethods)
            {
                if (!signalName.Equals("ViewModelUpdated", StringComparison.OrdinalIgnoreCase))
                {
                    violations.Add(
                        $"{nodeType.FullName}: Contains forbidden signal method '{signalName}'. " +
                        "Front-end nodes should only emit 'ViewModelUpdated' signals.");
                }
            }

            return violations;
        }

        #endregion
    }

    #region DRY Data Structures

    /// <summary>
    /// Defines a forbidden dependency pattern for front-end nodes.
    /// </summary>
    public class ForbiddenDependencyPattern
    {
        public string Pattern { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string Reason { get; set; } = string.Empty;
    }

    /// <summary>
    /// Defines an allowed adapter pattern for front-end nodes.
    /// </summary>
    public class AllowedAdapterPattern
    {
        public string Pattern { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
    }

    #endregion
}
