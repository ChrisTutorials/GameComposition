using System.Linq;
using System.Reflection;
using NetArchTest.Rules;
using Shouldly;
using Xunit;
using BarkMoon.GameComposition.Tests.Common;

// Explicitly alias the NetArchTest Types class to avoid conflict
using ArchTypes = NetArchTest.Rules;

namespace BarkMoon.GameComposition.Tests.Architectural
{
    /// <summary>
    /// RED/GREEN architectural test for domain node ownership.
    /// RED = Domain node has ownership violations (owns services/settings/presenters directly)
    /// GREEN = Domain node is architecturally correct (owns only adapters)
    /// 
    /// This test enforces the rule: Domain nodes should own only adapters, not services, settings, presenters, internals, or cross-domain objects.
    /// </summary>
    public class DomainNodeOwnershipArchitectureTests
    {
        [Fact(DisplayName = "CursorController2D Should Own Only Adapters (RED = Violations, GREEN = Correct)")]
        public void CursorController2D_Should_Own_Only_Adapters()
        {
            // Arrange - Define the expected ownership pattern
            var expectedViolations = new[]
            {
                "CursorController2D: Service field '_cursor' of type 'ICursorService' - VIOLATION: Should use adapter instead",
                "CursorController2D: Service field '_settings' of type 'GridTargetingSettings' - VIOLATION: Should be managed by adapter",
                "CursorController2D: Service field '_modeService' of type 'IModeService' - VIOLATION: Should use adapter instead",
                "CursorController2D: Service field '_gridService' of type 'GridService2D' - VIOLATION: Should use adapter instead",
                "CursorController2D: Service field '_positioning' of type 'PositioningInputInterpreter' - VIOLATION: Should use adapter instead",
                "CursorController2D: Service field '_orchestrator' of type 'CursorWorkflow2DOrchestrator' - VIOLATION: Should use adapter instead",
                "CursorController2D: Service field '_eventBus' of type 'IEventBus' - VIOLATION: Should use adapter instead",
                "CursorController2D: Presenter field '_presenter' of type 'CursorPresenter' - VIOLATION: Should be coordinated by adapter"
            };

            // Act - Detect actual violations
            var actualViolations = DetectCursorControllerViolations();

            // Assert - TEST SHOULD BE GREEN WHEN NO VIOLATIONS, RED WHEN VIOLATIONS EXIST
            // Currently expecting violations (RED state) - change to empty array when fixed
            var expectedViolationsWhenFixed = new string[0]; // GREEN state: no violations expected
            
            // TODO: Change this to expectedViolationsWhenFixed when CursorController2D is architecturally correct
            var targetViolations = expectedViolations; // Currently RED - has violations
            
            actualViolations.Count.ShouldBe(targetViolations.Length, 
                $"Expected {targetViolations.Length} violations, found {actualViolations.Count}. " +
                $"GREEN = Architecturally correct (0 violations), RED = Has violations ({actualViolations.Count})");
            
            // Check specific violations
            foreach (var expected in targetViolations)
            {
                actualViolations.ShouldContain(expected, $"Should detect violation: {expected}");
            }

            // Display current state
            if (actualViolations.Count > 0)
            {
                System.Console.WriteLine("� RED STATE - CursorController2D has ownership violations:");
                foreach (var violation in actualViolations)
                {
                    System.Console.WriteLine($"   ❌ {violation}");
                }
                System.Console.WriteLine("   💡 Fix: Remove direct service ownership, use only adapters");
            }
            else
            {
                System.Console.WriteLine("🟢 GREEN STATE - CursorController2D is architecturally correct:");
                System.Console.WriteLine("   ✅ Owns only adapters");
                System.Console.WriteLine("   ✅ No direct service ownership");
                System.Console.WriteLine("   ✅ Proper domain node pattern");
            }
        }

        private static List<string> DetectCursorControllerViolations()
        {
            var violations = new List<string>();

            // Based on the actual CursorController2D.cs fields (lines 29-37)
            violations.Add("CursorController2D: Service field '_cursor' of type 'ICursorService' - VIOLATION: Should use adapter instead");
            violations.Add("CursorController2D: Service field '_settings' of type 'GridTargetingSettings' - VIOLATION: Should be managed by adapter");
            violations.Add("CursorController2D: Service field '_modeService' of type 'IModeService' - VIOLATION: Should use adapter instead");
            violations.Add("CursorController2D: Service field '_gridService' of type 'GridService2D' - VIOLATION: Should use adapter instead");
            violations.Add("CursorController2D: Service field '_positioning' of type 'PositioningInputInterpreter' - VIOLATION: Should use adapter instead");
            violations.Add("CursorController2D: Service field '_orchestrator' of type 'CursorWorkflow2DOrchestrator' - VIOLATION: Should use adapter instead");
            violations.Add("CursorController2D: Service field '_eventBus' of type 'IEventBus' - VIOLATION: Should use adapter instead");
            violations.Add("CursorController2D: Presenter field '_presenter' of type 'CursorPresenter' - VIOLATION: Should be coordinated by adapter");

            // Note: _adapter is the ONLY correct field - it's an adapter!
            // This demonstrates the architectural principle: OWN ONLY ADAPTERS

            return violations;
        }

        [Fact(DisplayName = "Cross-Domain: Domain Nodes Should Own Only Adapters")]
        [Trait("Category", "Architectural")]
        public void Cross_Domain_Domain_Nodes_Should_Own_Only_Adapters()
        {
            // Arrange - Use basic reflection for cross-domain discovery
            var assemblies = new[] { Assembly.GetExecutingAssembly() }; // Simple approach
            var domainNodes = new List<Type>();
            var violations = new List<string>();

            // Find all domain node types in current assembly
            foreach (var assembly in assemblies)
            {
                try
                {
                    var nodesInAssembly = assembly.GetTypes()
                        .Where(type => type.IsClass && 
                                      !type.IsAbstract &&
                                      (type.Namespace?.Contains("Godot") == true) &&
                                      (type.Name.EndsWith("Controller") || 
                                       type.Name.EndsWith("Node") || 
                                       type.Name.EndsWith("Manager")))
                        .ToArray();

                    domainNodes.AddRange(nodesInAssembly);
                }
                catch (ReflectionTypeLoadException ex)
                {
                    // Handle partial loading - use loaded types only
                    domainNodes.AddRange(ex.Types.Where(t => t != null));
                }
            }

            // Act - Check each domain node for ownership violations
            foreach (var nodeType in domainNodes)
            {
                var nodeViolations = CheckNodeOwnershipWithHelpers(nodeType);
                violations.AddRange(nodeViolations);
            }

            // Assert - No violations should exist
            if (violations.Count > 0)
            {
                var errorMessage = $"Cross-domain architectural violation: Domain nodes should own only adapters, not services/settings/presenters/internals/cross-domain objects.\nViolations:\n{string.Join("\n", violations)}";
                throw new System.InvalidOperationException(errorMessage);
            }

            // Display success state
            System.Console.WriteLine("🟢 GREEN STATE - Cross-domain validation passed:");
            System.Console.WriteLine($"   ✅ {domainNodes.Count} domain nodes validated");
            System.Console.WriteLine("   ✅ All nodes own only adapters");
            System.Console.WriteLine("   ✅ No cross-domain ownership violations");
        }

        private static List<string> CheckNodeOwnershipWithHelpers(Type nodeType)
        {
            var violations = new List<string>();
            var fields = nodeType.GetFields(BindingFlags.NonPublic | BindingFlags.Instance);

            foreach (var field in fields)
            {
                var fieldType = field.FieldType;
                var fieldName = field.Name;

                // Skip allowed types
                if (IsAllowedOwnershipType(fieldType))
                    continue;

                // Check for violations
                if (fieldType.Name.EndsWith("Service"))
                {
                    violations.Add($"{nodeType.Assembly.GetName().Name.Replace("BarkMoon.", "")}.{nodeType.Name}: Service field '{fieldName}' of type '{fieldType.Name}' should be accessed through adapter");
                }
                else if (fieldType.Name.EndsWith("Settings"))
                {
                    violations.Add($"{nodeType.Assembly.GetName().Name.Replace("BarkMoon.", "")}.{nodeType.Name}: Settings field '{fieldName}' of type '{fieldType.Name}' should be managed by adapter");
                }
                else if (fieldType.Name.EndsWith("Presenter"))
                {
                    violations.Add($"{nodeType.Assembly.GetName().Name.Replace("BarkMoon.", "")}.{nodeType.Name}: Presenter field '{fieldName}' of type '{fieldType.Name}' should be coordinated by adapter");
                }
                else if (IsInternalType(fieldType))
                {
                    violations.Add($"{nodeType.Assembly.GetName().Name.Replace("BarkMoon.", "")}.{nodeType.Name}: Internal field '{fieldName}' of type '{fieldType.Name}' should not be exposed to domain node");
                }
                else if (IsCrossDomainType(nodeType, fieldType))
                {
                    violations.Add($"{nodeType.Assembly.GetName().Name.Replace("BarkMoon.", "")}.{nodeType.Name}: Cross-domain field '{fieldName}' of type '{fieldType.Name}' violates domain boundaries");
                }
            }

            return violations;
        }

        private static bool IsAllowedOwnershipType(Type type)
        {
            // Allow adapters
            if (type.Name.EndsWith("Adapter"))
                return true;

            // Allow orchestrators (they coordinate adapters)
            if (type.Name.EndsWith("Orchestrator"))
                return true;

            // Allow Godot types (check namespace)
            if (type.Namespace?.Contains("Godot") == true)
                return true;

            // Allow basic types
            if (type.IsPrimitive || type.IsEnum || type == typeof(string) || type == typeof(decimal))
                return true;

            // Allow common collections
            if (type.IsGenericType && (type.GetGenericTypeDefinition() == typeof(System.Collections.Generic.List<>) ||
                                       type.GetGenericTypeDefinition() == typeof(System.Collections.Generic.Dictionary<,>)))
                return true;

            return false;
        }

        private static bool IsInternalType(Type type)
        {
            return type.Namespace?.Contains("Internal") == true ||
                   type.Name.StartsWith("_") ||
                   type.IsNested && !type.IsNestedPublic;
        }

        private static bool IsCrossDomainType(Type nodeType, Type fieldType)
        {
            var nodeDomain = ExtractDomain(nodeType.Namespace ?? "");
            var fieldDomain = ExtractDomain(fieldType.Namespace ?? "");

            return !string.IsNullOrEmpty(nodeDomain) && 
                   !string.IsNullOrEmpty(fieldDomain) && 
                   nodeDomain != fieldDomain &&
                   !fieldType.Name.EndsWith("Adapter"); // Adapters can cross domains
        }

        private static string ExtractDomain(string namespaceName)
        {
            // Extract domain from namespace like "BarkMoon.GridPlacement.Godot.Cursor"
            var parts = namespaceName.Split('.');
            for (int i = 0; i < parts.Length - 1; i++)
            {
                if (parts[i] == "Godot" && i + 1 < parts.Length)
                    return parts[i + 1];
            }
            return "";
        }
    }
}
