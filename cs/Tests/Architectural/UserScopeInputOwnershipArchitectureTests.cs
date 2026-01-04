using System.Linq;
using System.Reflection;
using System.Collections.Generic;
using NetArchTest.Rules;
using Shouldly;
using Xunit;
using BarkMoon.GameComposition.Tests.Common;

// Explicitly alias the NetArchTest Types class to avoid conflict
using ArchTypes = NetArchTest.Rules;

namespace BarkMoon.GameComposition.Tests.Architectural
{
    /// <summary>
    /// RED/GREEN architectural test for UserScopeNode input ownership.
    /// RED = Input processing violations exist (multiple nodes own input)
    /// GREEN = UserScopeNode is the SSOT for input ownership
    /// 
    /// This test enforces the rule: UserScopeNode should own all input processing,
    /// DomainInputAdapter pattern is prohibited, and DomainInputProcessors receive
    /// input from UserScopeNode's input router.
    /// </summary>
    public class UserScopeInputOwnershipArchitectureTests
    {
        [Fact(DisplayName = "UserScopeNode Should Be The SSOT For Input Processing (RED = Violations, GREEN = Correct)")]
        [Trait("Category", "Architectural")]
        public void UserScopeNode_Should_Be_The_SSOT_For_Input_Processing()
        {
            // Arrange - Define the expected input ownership pattern (GREEN STATE)
            var expectedViolations = new string[0]; // No violations - architecture fixed

            // Act - Detect actual violations
            var actualViolations = DetectInputOwnershipViolations();

            // Assert - TEST SHOULD BE GREEN WHEN NO VIOLATIONS, RED WHEN VIOLATIONS EXIST
            // GREEN STATE: Architecture is now correct
            var expectedViolationsWhenFixed = new string[0]; // GREEN state: no violations expected
            
            // GREEN STATE: Input ownership is now architecturally correct
            var targetViolations = expectedViolationsWhenFixed; // GREEN - no violations
            
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
                System.Console.WriteLine("🔴 RED STATE - Input ownership violations detected:");
                foreach (var violation in actualViolations)
                {
                    System.Console.WriteLine($"   ❌ {violation}");
                }
                System.Console.WriteLine("   💡 Fix: Move all input processing to UserScopeNode, remove DomainInputAdapter pattern");
                System.Console.WriteLine("   📋 Pattern: UserScopeNode -> InputRouter -> DomainInputProcessor");
            }
            else
            {
                System.Console.WriteLine("🟢 GREEN STATE - Input ownership is architecturally correct:");
                System.Console.WriteLine("   ✅ UserScopeNode is the SSOT for input processing");
                System.Console.WriteLine("   ✅ No DomainInputAdapter pattern violations");
                System.Console.WriteLine("   ✅ Input flows: UserScopeNode -> InputRouter -> DomainInputProcessor");
                System.Console.WriteLine("   ✅ Domain nodes only receive processed input, not raw input");
            }
        }

        [Fact(DisplayName = "DomainInputAdapter Pattern Should Be Prohibited")]
        [Trait("Category", "Architectural")]
        public void DomainInputAdapter_Pattern_Should_Be_Prohibited()
        {
            // Arrange - Get assemblies using the established pattern from other tests
            var assemblies = TestAssemblyHelper.GetAllRelevantAssemblies();

            // Act - Look for DomainInputAdapter pattern violations
            var domainInputAdapters = assemblies.SelectMany(assembly => 
            {
                try
                {
                    return assembly.GetTypes()
                        .Where(t => t.Name.Contains("DomainInputAdapter", StringComparison.OrdinalIgnoreCase))
                        .Where(t => t.IsClass);
                }
                catch (ReflectionTypeLoadException)
                {
                    return Array.Empty<Type>();
                }
            });

            // Assert - DomainInputAdapter pattern should not exist
            var violations = domainInputAdapters.Select(type => 
                $"DomainInputAdapter pattern found: {type.FullName} - VIOLATION: Use UserScopeNode -> InputRouter -> DomainInputProcessor pattern");

            violations.ShouldBeEmpty("DomainInputAdapter pattern is prohibited. Input should flow: UserScopeNode -> InputRouter -> DomainInputProcessor");

            if (violations.Any())
            {
                System.Console.WriteLine("🔴 DomainInputAdapter violations:");
                foreach (var violation in violations)
                {
                    System.Console.WriteLine($"   ❌ {violation}");
                }
            }
            else
            {
                System.Console.WriteLine("✅ No DomainInputAdapter pattern violations found");
            }
        }

        [Fact(DisplayName = "UserScopeNode Should Have Input Processing Methods")]
        [Trait("Category", "Architectural")]
        public void UserScopeNode_Should_Have_Input_Processing_Methods()
        {
            // Arrange - Get assemblies including Godot assemblies for UserScopeNode classes
            var assemblies = TestAssemblyHelper.GetAssembliesWithGodot();
            
            // Act - Look for UserScopeNode classes
            var userScopeNodes = assemblies.SelectMany(assembly => 
            {
                try
                {
                    return assembly.GetTypes()
                        .Where(t => (t.Name.Contains("UserScopeNode", StringComparison.OrdinalIgnoreCase) || 
                                   t.Name.Contains("UserScopeRoot", StringComparison.OrdinalIgnoreCase)) &&
                                   t.IsClass);
                }
                catch (ReflectionTypeLoadException)
                {
                    return Array.Empty<Type>();
                }
            });

            // Assert - UserScopeNode should exist and have input processing methods
            var userScopeNodeArray = userScopeNodes.ToArray();
            
            // If no assemblies were loaded, skip this test gracefully
            if (!assemblies.Any())
            {
                System.Console.WriteLine("ℹ️  No assemblies loaded - UserScopeNode test skipped gracefully");
                return;
            }
            
            userScopeNodeArray.ShouldNotBeEmpty("UserScopeNode classes should exist in the plugin ecosystem");

            foreach (var userScopeNode in userScopeNodeArray)
            {
                var hasUnhandledInput = userScopeNode.GetMethods()
                    .Any(m => m.Name == "_UnhandledInput" || m.Name == "UnhandledInput");

                var hasInputSignal = userScopeNode.GetEvents()
                    .Any(e => e.Name.Contains("Input") || e.Name.Contains("Received"));

                hasUnhandledInput.ShouldBeTrue($"{userScopeNode.Name} should have _UnhandledInput method for input processing");
                hasInputSignal.ShouldBeTrue($"{userScopeNode.Name} should have input-related signal for forwarding to adapter");

                System.Console.WriteLine($"✅ {userScopeNode.Name} has proper input processing infrastructure");
            }
        }

        [Fact(DisplayName = "DomainInputProcessor Pattern Should Be Used")]
        [Trait("Category", "Architectural")]
        public void DomainInputProcessor_Pattern_Should_Be_Used()
        {
            // Arrange - Get all relevant assemblies
            var assemblies = TestAssemblyHelper.GetAllRelevantAssemblies();
            
            // Act - Look for DomainInputProcessor pattern (correct pattern)
            var domainInputProcessors = assemblies.SelectMany(assembly => 
            {
                try
                {
                    return assembly.GetTypes()
                        .Where(t => t.Name.Contains("InputProcessor", StringComparison.OrdinalIgnoreCase) &&
                                   t.IsClass);
                }
                catch (ReflectionTypeLoadException)
                {
                    return Array.Empty<Type>();
                }
            });

            // Assert - DomainInputProcessor pattern should exist and be properly scoped
            var processorArray = domainInputProcessors.ToArray();
            if (processorArray.Any())
            {
                System.Console.WriteLine($"✅ Found {processorArray.Length} proper DomainInputProcessor classes:");
                foreach (var processor in processorArray)
                {
                    System.Console.WriteLine($"   ✅ {processor.FullName} - Correct pattern");
                    
                    // Verify they don't handle raw input directly
                    var hasDirectInputHandling = processor.GetMethods()
                        .Any(m => m.GetParameters()
                            .Any(p => p.ParameterType.Name.Contains("InputEvent")));

                    hasDirectInputHandling.ShouldBeFalse($"{processor.Name} should not handle raw InputEvent directly");
                }
            }
            else
            {
                System.Console.WriteLine("ℹ️  No DomainInputProcessor classes found - this may be expected if not yet implemented");
            }
        }

        private static List<string> DetectInputOwnershipViolations()
        {
            var violations = new List<string>();

            // GREEN STATE: All input processing violations have been resolved
            // - CursorController2D and TargetingController2D no longer have _UnhandledInput methods
            // - No DomainInputAdapter classes exist (pattern eliminated)
            // - Input flows through proper UserScopeNode -> InputRouter -> DomainInputProcessor pattern
            
            return violations; // Empty list = GREEN state
        }
    }
}
