using Xunit;
using NetArchTest.Rules;
using Shouldly;
using System.Linq;
using System.Reflection;
using BarkMoon.GameComposition.Core.Interfaces;
using BarkMoon.GameComposition.Tests.Common;

// Explicitly alias the NetArchTest Types class to avoid conflict with BarkMoon.GameComposition.Core.Types namespace
using ArchTypes = NetArchTest.Rules.Types;

namespace BarkMoon.GameComposition.Core.Tests.Architectural
{
    /// <summary>
    /// Cross-domain architectural tests that enforce service ownership patterns across ALL plugins.
    /// Services must own state, snapshots must not own state, and patterns must be consistent across the ecosystem.
    /// Tests are loaded dynamically from all plugin assemblies to enforce universal compliance.
    /// </summary>
    public class ServiceOwnershipTests
    {
        /// <summary>
        /// Rule 1: State classes should be internal to prevent external instantiation across ALL plugins.
        /// </summary>
        [Fact]
        [Trait("Category", "Architectural")]
        public void State_Classes_Should_Be_Internal_Across_All_Plugins()
        {
            // Arrange - Use SSOT helper for assembly loading
            var assemblies = ArchitecturalTestHelpers.GetAllAssemblies();
            var allViolations = new List<string>();

            foreach (var assembly in assemblies)
            {
                var stateTypes = ArchTypes.InAssembly(assembly)
                    .That()
                    .ImplementInterface(typeof(IState))
                    .And()
                    .AreClasses();

                if (!stateTypes.GetTypes().Any())
                    continue;

                // Act & Assert
                var result = stateTypes
                    .Should()
                    .NotBePublic()
                    .GetResult();

                if (!result.IsSuccessful)
                {
                    allViolations.Add($"Assembly {assembly.GetName().Name}: Architectural rule violation detected");
                }
            }

            // Assert across all plugins
            if (allViolations.Count == 0)
                return;

            // Assert across all plugins
            if (allViolations.Count > 0)
            {
                var errorMessage = $"State classes should be internal across all plugins. Violations:\n{string.Join("\n", allViolations)}";
                throw new InvalidOperationException(errorMessage);
            }
        }

        /// <summary>
        /// Rule 2: Service classes must implement a service interface across ALL plugins.
        /// Excludes exception classes and other utilities that happen to have 'Service' in the name.
        /// </summary>
        [Fact]
        [Trait("Category", "Architectural")]
        public void Service_Classes_Must_Implement_Service_Interface_Across_All_Plugins()
        {
            // Arrange - Use SSOT helper for assembly loading
            var assemblies = ArchitecturalTestHelpers.GetAllAssemblies();
            var allViolations = new List<string>();

            foreach (var assembly in assemblies)
            {
                var serviceTypes = ArchTypes.InAssembly(assembly)
                    .That()
                    .HaveNameEndingWith("Service", StringComparison.OrdinalIgnoreCase)
                    .And()
                    .AreClasses()
                    .And()
                    .AreNotAbstract()
                    .And()
                    .DoNotInherit(typeof(Exception)) // Exclude exception classes
                    .GetTypes().ToArray();

                if (serviceTypes.Length == 0)
                    continue;

                // Act & Assert
                foreach (var serviceType in serviceTypes)
                {
                    // Check if the service implements any interface that ends with "Service" or is a well-known service interface
                    var hasServiceInterface = serviceType.GetInterfaces()
                        .Any(i => i.Name.EndsWith("Service", StringComparison.OrdinalIgnoreCase) || 
                                 i.Name == "IServiceScope" || 
                                 i.Name == "IServiceCompositionRoot" ||
                                 i.Name.StartsWith("I", StringComparison.OrdinalIgnoreCase) && i.Name.Contains("Service", StringComparison.OrdinalIgnoreCase));

                    if (!hasServiceInterface)
                    {
                        allViolations.Add(
                            $"Assembly {assembly.GetName().Name}: Service class {serviceType.Name} must implement a service interface. " +
                            "Services should follow the interface-implementation pattern for testability and DI.");
                    }
                }
            }

            // Assert across all plugins
            if (allViolations.Count > 0)
            {
                var errorMessage = $"Service classes must implement service interfaces across all plugins. Violations:\n{string.Join("\n", allViolations)}";
                throw new InvalidOperationException(errorMessage);
            }
        }

        /// <summary>
        /// Rule 3: Snapshot classes must be sealed to prevent inheritance attacks across ALL plugins.
        /// </summary>
        [Fact]
        [Trait("Category", "Architectural")]
        public void Snapshot_Classes_Must_Be_Sealed_Across_All_Plugins()
        {
            // Arrange - Use SSOT helper for assembly loading
            var assemblies = ArchitecturalTestHelpers.GetAllAssemblies();
            var allViolations = new List<string>();

            foreach (var assembly in assemblies)
            {
                var result = ArchTypes.InAssembly(assembly)
                    .That()
                    .HaveNameEndingWith("Snapshot", StringComparison.OrdinalIgnoreCase)
                    .And()
                    .AreClasses()
                    .Should()
                    .BeSealed()
                    .GetResult();

                if (!result.IsSuccessful)
                {
                    allViolations.Add($"Assembly {assembly.GetName().Name}: Architectural rule violation detected");
                }
            }

            // Assert across all plugins
            if (allViolations.Count > 0)
            {
                var errorMessage = $"All snapshot classes must be sealed across all plugins to prevent inheritance-based state exposure attacks. Violations:\n{string.Join("\n", allViolations)}";
                throw new InvalidOperationException(errorMessage);
            }
        }

        /// <summary>
        /// Rule 4: Interface names should follow conventions across ALL plugins.
        /// </summary>
        [Fact]
        [Trait("Category", "Architectural")]
        public void Interface_Names_Should_Follow_Conventions_Across_All_Plugins()
        {
            // Arrange - Use SSOT helper for assembly loading
            var assemblies = ArchitecturalTestHelpers.GetAllAssemblies();
            var allViolations = new List<string>();

            foreach (var assembly in assemblies)
            {
                var publicInterfaces = ArchTypes.InAssembly(assembly)
                    .That()
                    .AreInterfaces()
                    .And()
                    .ArePublic()
                    .GetTypes().ToArray();

                if (publicInterfaces.Length == 0)
                    continue;

                // Act & Assert
                foreach (var interfaceType in publicInterfaces)
                {
                    // Core interfaces should start with 'I'
                    if (!interfaceType.Name.StartsWith("I", StringComparison.OrdinalIgnoreCase))
                    {
                        allViolations.Add(
                            $"Assembly {assembly.GetName().Name}: Interface {interfaceType.Name} should start with 'I' following C# conventions.");
                    }
                }
            }

            // Assert across all plugins
            if (allViolations.Count > 0)
            {
                var errorMessage = $"Interface names should follow conventions across all plugins. Violations:\n{string.Join("\n", allViolations)}";
                throw new InvalidOperationException(errorMessage);
            }
        }

        }
}
