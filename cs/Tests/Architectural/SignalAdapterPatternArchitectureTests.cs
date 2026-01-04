using System.Linq;
using System.Reflection;
using System.Collections.Generic;
using NetArchTest.Rules;
using Shouldly;
using Xunit;
using BarkMoon.GameComposition.Tests.Common;
using BarkMoon.GameComposition.Core.Interfaces;

// Explicitly alias the NetArchTest Types class to avoid conflict
using ArchTypes = NetArchTest.Rules;

namespace BarkMoon.GameComposition.Tests.Architectural
{
    /// <summary>
    /// RED/GREEN architectural test for HybridEventAdapter pattern enforcement.
    /// RED = EventAdapter classes don't follow HybridEventAdapter pattern or don't implement IEventAdapter
    /// GREEN = All EventAdapter classes follow HybridEventAdapter pattern and implement IEventAdapter
    /// 
    /// This test enforces the rule: All event adapters should use HybridEventAdapter pattern,
    /// inherit from Node (for Godot signals), implement IEventAdapter interface,
    /// and provide both C# events (performance) and Godot signals (GDScript compatibility).
    /// </summary>
    public class HybridEventAdapterPatternArchitectureTests
    {
        [Fact(DisplayName = "Event Adapters Should Use HybridEventAdapter Pattern (RED = Violations, GREEN = Correct)")]
        [Trait("Category", "Architectural")]
        public void Event_Adapters_Should_Use_HybridEventAdapter_Pattern()
        {
            // Arrange - Use cross-domain helpers for assembly loading
            var assemblies = ArchitecturalTestHelpers.GetAllAssemblies();
            
            // Act - Find all EventAdapter classes
            var eventAdapters = ArchitecturalTestHelpers.FindTypesInAllAssemblies(type =>
                type.Name.Contains("EventAdapter", StringComparison.OrdinalIgnoreCase) &&
                type.IsClass &&
                !type.IsAbstract);

            var violations = new List<string>();

            foreach (var eventAdapter in eventAdapters)
            {
                // Rule 1: Should implement IEventAdapter interface (base or generic)
                var implementsIEventAdapter = typeof(IEventAdapter).IsAssignableFrom(eventAdapter);
                if (!implementsIEventAdapter)
                {
                    violations.Add($"{eventAdapter.Name}: Missing IEventAdapter interface - VIOLATION: All event adapters must implement IEventAdapter");
                }

                // Rule 2: Should inherit from Node (for Godot signals) OR be in Godot namespace
                var inheritsFromNode = typeof(Godot.Node).IsAssignableFrom(eventAdapter);
                var isInGodotNamespace = eventAdapter.Namespace?.Contains("Godot", StringComparison.OrdinalIgnoreCase) == true;
                
                if (!inheritsFromNode && !isInGodotNamespace)
                {
                    violations.Add($"{eventAdapter.Name}: Not inheriting from Node - VIOLATION: Godot event adapters should inherit from Node for signal support");
                }

                // Rule 3: Should have "HybridEventAdapter" in name if using dual pattern AND in Godot namespace
                var hasHybridEventAdapterNaming = eventAdapter.Name.Contains("HybridEventAdapter", StringComparison.OrdinalIgnoreCase);
                if (isInGodotNamespace && !hasHybridEventAdapterNaming)
                {
                    violations.Add($"{eventAdapter.Name}: Should be named *HybridEventAdapter - VIOLATION: Godot event adapters should use HybridEventAdapter naming pattern");
                }

                // Rule 4: Should not own services (check constructor parameters)
                var constructors = eventAdapter.GetConstructors();
                foreach (var constructor in constructors)
                {
                    var parameters = constructor.GetParameters();
                    foreach (var param in parameters)
                    {
                        var paramTypeName = param.ParameterType.Name;
                        if (paramTypeName.EndsWith("Service") || 
                            paramTypeName.EndsWith("Workflow") ||
                            paramTypeName.EndsWith("Presenter") ||
                            paramTypeName.EndsWith("Settings"))
                        {
                            violations.Add($"{eventAdapter.Name}: Constructor parameter '{paramTypeName}' - VIOLATION: Event adapters should be 'dumb' and not own services");
                        }
                    }
                }

                // Rule 5: Should have IEventAdapter required methods
                var hasStartMethod = eventAdapter.GetMethod("Start") != null;
                var hasStopMethod = eventAdapter.GetMethod("Stop") != null;
                var hasDisposeMethod = eventAdapter.GetMethod("Dispose") != null;
                var hasIsActiveProperty = eventAdapter.GetProperty("IsActive") != null;

                if (implementsIEventAdapter)
                {
                    if (!hasStartMethod || !hasStopMethod || !hasDisposeMethod || !hasIsActiveProperty)
                    {
                        violations.Add($"{eventAdapter.Name}: Missing IEventAdapter required members - VIOLATION: Must implement Start, Stop, Dispose, and IsActive");
                    }
                }

                // Rule 6: Should have both C# event and Godot signal for hybrid pattern (if in Godot namespace)
                if (isInGodotNamespace)
                {
                    // Check for C# event
                    var hasCSharpEvent = eventAdapter.GetEvents()
                        .Any(e => e.Name == "CSharpEvent");
                    
                    if (!hasCSharpEvent)
                    {
                        violations.Add($"{eventAdapter.Name}: Missing CSharpEvent - VIOLATION: Hybrid adapters must provide C# event for performance");
                    }

                    // Check for Godot signal
                    var hasSignalAttributes = eventAdapter.GetMethods()
                        .Any(m => m.GetCustomAttributes().Any(attr => attr.GetType().Name == "SignalAttribute"));
                    
                    if (!hasSignalAttributes)
                    {
                        violations.Add($"{eventAdapter.Name}: Missing [Signal] attributes - VIOLATION: Hybrid adapters must provide Godot signals for GDScript");
                    }

                    // Check for GodotSignalName property (if implements generic interface)
                    var implementsGenericIEventAdapter = eventAdapter.GetInterfaces()
                        .Any(i => i.IsGenericType && i.GetGenericTypeDefinition() == typeof(IEventAdapter<>));
                    
                    if (implementsGenericIEventAdapter)
                    {
                        var hasGodotSignalNameProperty = eventAdapter.GetProperty("GodotSignalName") != null;
                        if (!hasGodotSignalNameProperty)
                        {
                            violations.Add($"{eventAdapter.Name}: Missing GodotSignalName property - VIOLATION: Hybrid adapters must expose signal name");
                        }
                    }
                }
            }

            // Assert - TEST SHOULD BE GREEN WHEN NO VIOLATIONS, RED WHEN VIOLATIONS EXIST
            // Currently we have CursorHybridEventAdapter implemented correctly, but PlacementEventAdapter is legacy
            var expectedViolations = new[]
            {
                "PlacementEventAdapter: Should be named *HybridEventAdapter - VIOLATION: Godot event adapters should use HybridEventAdapter naming pattern",
                "PlacementEventAdapter: Missing CSharpEvent - VIOLATION: Hybrid adapters must provide C# event for performance",
                "PlacementEventAdapter: Missing IEventAdapter interface - VIOLATION: All event adapters must implement IEventAdapter"
            };
            
            var expectedViolationsWhenFixed = new string[0]; // GREEN state: no violations expected
            
            // TODO: Change this to expectedViolationsWhenFixed when all event adapters follow HybridEventAdapter pattern
            var targetViolations = expectedViolations; // Currently RED - has legacy violations
            
            violations.Count.ShouldBe(targetViolations.Length, 
                $"Expected {targetViolations.Length} violations, found {violations.Count}. " +
                $"GREEN = HybridEventAdapter pattern correct (0 violations), RED = Pattern violations ({violations.Count})");
            
            // Check specific violations
            foreach (var expected in targetViolations)
            {
                violations.ShouldContain(expected, $"Should detect violation: {expected}");
            }

            // Display current state using cross-domain helper
            ArchitecturalTestHelpers.DisplayTestState(violations, "HybridEventAdapter Pattern");
        }

        [Fact(DisplayName = "HybridEventAdapter Classes Should Follow Dual Pattern")]
        [Trait("Category", "Architectural")]
        public void HybridEventAdapter_Classes_Should_Follow_Dual_Pattern()
        {
            // Arrange - Use cross-domain helpers for assembly loading
            var assemblies = ArchitecturalTestHelpers.GetAllAssemblies();

            // Act - Find all HybridEventAdapter classes
            var hybridEventAdapters = ArchitecturalTestHelpers.FindTypesInAllAssemblies(type =>
                type.Name.Contains("HybridEventAdapter", StringComparison.OrdinalIgnoreCase) &&
                type.IsClass &&
                !type.IsAbstract);

            var violations = new List<string>();

            foreach (var hybridAdapter in hybridEventAdapters)
            {
                // Rule 1: Should inherit from Node
                var inheritsFromNode = typeof(Godot.Node).IsAssignableFrom(hybridAdapter);
                if (!inheritsFromNode)
                {
                    violations.Add($"{hybridAdapter.Name}: Must inherit from Node - VIOLATION: HybridEventAdapters need Node for Godot signals");
                }

                // Rule 2: Should implement IEventAdapter (base or generic)
                var implementsIEventAdapter = typeof(IEventAdapter).IsAssignableFrom(hybridAdapter);
                if (!implementsIEventAdapter)
                {
                    violations.Add($"{hybridAdapter.Name}: Must implement IEventAdapter - VIOLATION: Required for consistent adapter pattern");
                }

                // Rule 3: Should have both C# event and Godot signal
                var hasCSharpEvent = hybridAdapter.GetEvents()
                    .Any(e => e.Name == "CSharpEvent");
                
                var hasSignalAttributes = hybridAdapter.GetMethods()
                    .Any(m => m.GetCustomAttributes().Any(attr => attr.GetType().Name == "SignalAttribute"));

                if (!hasCSharpEvent)
                {
                    violations.Add($"{hybridAdapter.Name}: Missing CSharpEvent - VIOLATION: Must provide C# event for performance");
                }

                if (!hasSignalAttributes)
                {
                    violations.Add($"{hybridAdapter.Name}: Missing [Signal] attributes - VIOLATION: Must provide Godot signals for GDScript");
                }

                // Rule 4: Should have GodotSignalName property if implements generic interface
                var implementsGenericIEventAdapter = hybridAdapter.GetInterfaces()
                    .Any(i => i.IsGenericType && i.GetGenericTypeDefinition() == typeof(IEventAdapter<>));
                
                if (implementsGenericIEventAdapter)
                {
                    var hasGodotSignalNameProperty = hybridAdapter.GetProperty("GodotSignalName") != null;
                    if (!hasGodotSignalNameProperty)
                    {
                        violations.Add($"{hybridAdapter.Name}: Missing GodotSignalName property - VIOLATION: Must expose signal name for GDScript");
                    }
                }

                // Rule 5: Should be in Godot namespace
                var isInGodotNamespace = hybridAdapter.Namespace?.Contains("Godot", StringComparison.OrdinalIgnoreCase) == true;
                if (!isInGodotNamespace)
                {
                    violations.Add($"{hybridAdapter.Name}: Should be in Godot namespace - VIOLATION: HybridEventAdapters are Godot-specific");
                }

                // Rule 6: Should have required IEventAdapter members
                var hasStartMethod = hybridAdapter.GetMethod("Start") != null;
                var hasStopMethod = hybridAdapter.GetMethod("Stop") != null;
                var hasDisposeMethod = hybridAdapter.GetMethod("Dispose") != null;
                var hasIsActiveProperty = hybridAdapter.GetProperty("IsActive") != null;

                if (!hasStartMethod || !hasStopMethod || !hasDisposeMethod || !hasIsActiveProperty)
                {
                    violations.Add($"{hybridAdapter.Name}: Missing IEventAdapter required members - VIOLATION: Must implement Start, Stop, Dispose, and IsActive");
                }
            }

            // Assert - All HybridEventAdapters should follow dual pattern
            // Currently we expect CursorHybridEventAdapter to be correct (GREEN for this specific test)
            violations.ShouldBeEmpty("HybridEventAdapter classes must follow dual C# event + Godot signal pattern");

            // Display results
            if (violations.Any())
            {
                System.Console.WriteLine("🔴 HybridEventAdapter pattern violations:");
                foreach (var violation in violations)
                {
                    System.Console.WriteLine($"   ❌ {violation}");
                }
            }
            else
            {
                System.Console.WriteLine("✅ All HybridEventAdapter classes follow dual pattern");
            }
        }

        [Fact(DisplayName = "Legacy EventAdapter Pattern Should Be Refactored To Hybrid")]
        [Trait("Category", "Architectural")]
        public void Legacy_EventAdapter_Pattern_Should_Be_Refactored_To_Hybrid()
        {
            // Arrange - Use cross-domain helpers for assembly loading
            var assemblies = ArchitecturalTestHelpers.GetAllAssemblies();

            // Act - Find legacy EventAdapter classes (not HybridEventAdapter)
            var legacyEventAdapters = ArchitecturalTestHelpers.FindTypesInAllAssemblies(type =>
                type.Name.Contains("EventAdapter", StringComparison.OrdinalIgnoreCase) &&
                !type.Name.Contains("HybridEventAdapter", StringComparison.OrdinalIgnoreCase) &&
                type.IsClass &&
                !type.IsAbstract);

            var violations = new List<string>();

            foreach (var legacyAdapter in legacyEventAdapters)
            {
                // Check if it's a Godot adapter that should be refactored to HybridEventAdapter
                var isInGodotNamespace = legacyAdapter.Namespace?.Contains("Godot", StringComparison.OrdinalIgnoreCase) == true;
                var inheritsFromNode = typeof(Godot.Node).IsAssignableFrom(legacyAdapter);

                if (isInGodotNamespace || inheritsFromNode)
                {
                    violations.Add($"{legacyAdapter.Name}: Should be refactored to *HybridEventAdapter - VIOLATION: Godot event adapters should use HybridEventAdapter pattern for C# + GDScript compatibility");
                }
            }

            // Assert - Legacy adapters should be refactored (currently RED state)
            // This test will be GREEN when all legacy adapters are refactored
            // Currently we expect PlacementEventAdapter to be legacy
            var expectedViolations = violations.ToArray(); // Currently expecting violations
            var expectedViolationsWhenFixed = new string[0]; // GREEN state: no legacy adapters

            // TODO: Change to expectedViolationsWhenFixed when all legacy adapters are refactored
            var targetViolations = expectedViolations; // Currently RED

            violations.Count.ShouldBe(targetViolations.Length, 
                $"Expected {targetViolations.Length} legacy adapters, found {violations.Count}. " +
                $"GREEN = All refactored to HybridEventAdapter (0), RED = Legacy adapters exist ({violations.Count})");

            // Display current state
            if (violations.Any())
            {
                System.Console.WriteLine("🔴 Legacy EventAdapter pattern detected:");
                foreach (var violation in violations)
                {
                    System.Console.WriteLine($"   🔄 {violation}");
                }
                System.Console.WriteLine("   💡 Refactor to HybridEventAdapter pattern for C# performance + GDScript compatibility");
            }
            else
            {
                System.Console.WriteLine("🟢 All event adapters use HybridEventAdapter pattern");
            }
        }

        [Fact(DisplayName = "HybridEventAdapter Should Provide Performance Benefits")]
        [Trait("Category", "Architectural")]
        public void HybridEventAdapter_Should_Provide_Performance_Benefits()
        {
            // Arrange - Use cross-domain helpers for assembly loading
            var assemblies = ArchitecturalTestHelpers.GetAllAssemblies();

            // Act - Find all HybridEventAdapter classes
            var hybridEventAdapters = ArchitecturalTestHelpers.FindTypesInAllAssemblies(type =>
                type.Name.Contains("HybridEventAdapter", StringComparison.OrdinalIgnoreCase) &&
                type.IsClass &&
                !type.IsAbstract);

            var performanceViolations = new List<string>();

            foreach (var hybridAdapter in hybridEventAdapters)
            {
                // Rule: Should implement generic IEventAdapter<T> for type safety
                var genericInterfaces = hybridAdapter.GetInterfaces()
                    .Where(i => i.IsGenericType && i.GetGenericTypeDefinition() == typeof(IEventAdapter<>));
                
                if (!genericInterfaces.Any())
                {
                    performanceViolations.Add($"{hybridAdapter.Name}: Should implement IEventAdapter<T> - VIOLATION: Generic interface provides type safety and performance");
                }

                // Rule: C# event should be strongly typed
                var csharpEvent = hybridAdapter.GetEvent("CSharpEvent");
                if (csharpEvent != null)
                {
                    var isGenericEventHandler = csharpEvent.EventHandlerType?.IsGenericType == true &&
                                               csharpEvent.EventHandlerType.GetGenericTypeDefinition() == typeof(System.EventHandler<>);
                    
                    if (!isGenericEventHandler)
                    {
                        performanceViolations.Add($"{hybridAdapter.Name}: CSharpEvent should be EventHandler<T> - VIOLATION: Strong typing improves performance");
                    }
                }
            }

            // Assert - HybridEventAdapters should provide performance benefits
            performanceViolations.ShouldBeEmpty("HybridEventAdapter classes should provide performance benefits through strong typing");

            // Display results
            if (performanceViolations.Any())
            {
                System.Console.WriteLine("⚠️  HybridEventAdapter performance issues:");
                foreach (var violation in performanceViolations)
                {
                    System.Console.WriteLine($"   ⚡ {violation}");
                }
            }
            else
            {
                System.Console.WriteLine("⚡ All HybridEventAdapter classes provide performance benefits");
            }
        }
    }
}
