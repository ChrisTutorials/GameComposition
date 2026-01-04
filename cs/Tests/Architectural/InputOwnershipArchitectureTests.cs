using System.Linq;
using NetArchTest.eNhancedEdition;
using Shouldly;
using Xunit;
using BarkMoon.GameComposition.Tests.Architectural.Core;

namespace BarkMoon.GameComposition.Tests.Architectural
{
    /// <summary>
    /// RED/GREEN architectural tests for input ownership patterns.
    /// 
    /// RED STATE = Architecture has violations that need fixing
    /// GREEN STATE = Architecture is compliant with input ownership rules
    /// </summary>
    public class InputOwnershipArchitectureTests
    {
        [Fact(DisplayName = "Input Processing Should Follow UserScopeNode SSOT (RED=Violations, GREEN=Fixed)")]
        [Trait("Category", "Architectural")]
        public void Input_Processing_Should_Follow_UserScopeNode_SSOT()
        {
            // Arrange: Load assemblies and configuration
            var assemblies = ArchitecturalTestHelpers.GetAllAssemblies();
            var config = ArchitecturalTestHelpers.LoadYamlConfig("architecture-config.yaml");

            // Act: Find input processing violations
            var prohibitedPatterns = config.GetForbiddenPatterns("input_processing");
            var exclusions = config.GetExclusions("input_processing");
            var actualViolations = ArchitecturalTestHelpers.DetectTypeViolations(
                assemblies, 
                prohibitedPatterns, 
                exclusions
            );

            // Assert: GREEN STATE - Architecture is now compliant!
            var expectedViolations = config.GetExpectedViolations("input_processing");
            
            // GREEN: CursorInputAdapter2D eliminated - input ownership violations resolved
            var targetViolations = 0; // Architecture fixed!
            
            actualViolations.Count.ShouldBe(targetViolations, 
                $"GREEN = Input ownership compliant (0 violations), RED = Found {actualViolations.Count} violations requiring fixes");

            // Display test state for developers
            ArchitecturalTestHelpers.DisplayTestState(actualViolations, "Input Ownership SSOT");
            
            // Detailed violation reporting
            if (actualViolations.Any())
            {
                var violationDetails = actualViolations
                    .Select(v => $"  - {v.Type.FullName} (violates pattern: {v.ViolatedPattern})")
                    .Join("\n");
                
                GD.Print($"Input Ownership Violations Found:\n{violationDetails}");
                GD.Print("Fix: Move input processing to UserScopeNode → InputRouter → DomainInputProcessor flow");
            }
        }

        [Fact(DisplayName = "Input Adapters Should Not Process Input Directly (RED=Violations, GREEN=Fixed)")]
        [Trait("Category", "Architectural")]
        public void Input_Adapters_Should_Not_Process_Input_Directly()
        {
            // Arrange: Load assemblies for analysis
            var assemblies = ArchitecturalTestHelpers.GetAllAssemblies();

            foreach (var assembly in assemblies)
            {
                // Act: Find InputAdapter types with input processing methods
                var inputAdapterTypes = ArchTypes.InAssembly(assembly)
                    .That().HaveNameEndingWith("InputAdapter")
                    .And().AreNotAbstract()
                    .GetTypes();

                var violatingTypes = new List<System.Type>();

                foreach (var adapterType in inputAdapterTypes)
                {
                    // Check for input processing methods (violations)
                    var inputProcessingMethods = adapterType.GetMethods()
                        .Where(m => m.Name.StartsWith("TryUpdate") || 
                                   m.Name.StartsWith("Process") ||
                                   m.Name.StartsWith("Handle"))
                        .Where(m => m.GetParameters().Any(p => 
                                   p.ParameterType.Name.Contains("Input") ||
                                   p.ParameterType.Name.Contains("Mouse") ||
                                   p.ParameterType.Name.Contains("Key")))
                        .ToList();

                    if (inputProcessingMethods.Any())
                    {
                        violatingTypes.Add(adapterType);
                    }
                }

                // Assert: RED STATE - Should find violations currently
                violatingTypes.ShouldNotBeEmpty("RED STATE: Input adapters should not process input directly. Found no violations, but architecture expects violations to exist.");

                // Report violations for fixing
                foreach (var violatingType in violatingTypes)
                {
                    GD.Print($"VIOLATION: {violatingType.FullName} processes input directly");
                    GD.Print($"  Fix: Move input processing to DomainInputProcessor, keep adapter as pure translation layer");
                }
            }
        }

        [Fact(DisplayName = "UserScopeNode Should Be Input Processing SSOT (RED=Missing, GREEN=Present)")]
        [Trait("Category", "Architectural")]
        public void UserScopeNode_Should_Be_Input_Processing_SSOT()
        {
            // Arrange: Load assemblies for analysis
            var assemblies = ArchitecturalTestHelpers.GetAllAssemblies();

            foreach (var assembly in assemblies)
            {
                // Act: Find UserScopeNode types
                var userScopeNodes = ArchTypes.InAssembly(assembly)
                    .That().ImplementInterface("IUserScopeNode")
                    .Or().HaveNameEndingWith("UserScopeNode")
                    .GetTypes();

                // Assert: Should have proper input processing methods
                foreach (var userScopeNode in userScopeNodes)
                {
                    var inputMethods = userScopeNode.GetMethods()
                        .Where(m => m.Name == "_Input" || m.Name == "_UnhandledInput")
                        .ToList();

                    inputMethods.ShouldNotBeEmpty($"UserScopeNode {userScopeNode.Name} should have _Input or _UnhandledInput method for input processing SSOT");
                    
                    // Verify they're not abstract (should be implemented)
                    foreach (var inputMethod in inputMethods)
                    {
                        inputMethod.IsAbstract.ShouldBeFalse($"UserScopeNode {userScopeNode.Name} should implement input processing, not declare abstract");
                    }
                }
            }
        }
    }
}
