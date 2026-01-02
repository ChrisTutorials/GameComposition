using Xunit;
using NetArchTest.Rules;
using Shouldly;
using System.Linq;
using System.Reflection;
using BarkMoon.GameComposition.Core.Interfaces;

// Explicitly alias the NetArchTest Types class to avoid conflict with BarkMoon.GameComposition.Core.Types namespace
using ArchTypes = NetArchTest.Rules.Types;

namespace BarkMoon.GameComposition.Core.Tests.Architectural
{
    /// <summary>
    /// Architectural tests that enforce service ownership patterns.
    /// Services must own state, snapshots must not own state.
    /// </summary>
    public class ServiceOwnershipTests
    {
        /// <summary>
        /// Rule 1: State classes should be internal to prevent external instantiation.
        /// </summary>
        [Fact]
        public void State_Classes_Should_Be_Internal()
        {
            // Arrange
            var assembly = LoadCoreAssembly();
            var stateTypes = ArchTypes.InAssembly(assembly)
                .That()
                .ImplementInterface(typeof(IState))
                .And()
                .AreClasses();

            if (!stateTypes.GetTypes().Any())
                return;

            // Act & Assert
            var result = stateTypes
                .Should()
                .NotBePublic()
                .GetResult();

            result.IsSuccessful.ShouldBeTrue("State classes should be internal to prevent direct instantiation outside the assembly.");
        }

        /// <summary>
        /// Rule 1: Service classes must implement a service interface (pattern check).
        /// Excludes exception classes and other utilities that happen to have 'Service' in the name.
        /// </summary>
        [Fact]
        public void Service_Classes_Must_Implement_Service_Interface()
        {
            // Arrange
            var assembly = LoadCoreAssembly();
            var serviceTypes = ArchTypes.InAssembly(assembly)
                .That()
                .HaveNameEndingWith("Service")
                .And()
                .AreClasses()
                .And()
                .AreNotAbstract()
                .And()
                .DoNotInherit(typeof(Exception)) // Exclude exception classes
                .GetTypes();

            if (!serviceTypes.Any())
                return;

            // Act & Assert
            foreach (var serviceType in serviceTypes)
            {
                // Check if the service implements any interface that ends with "Service" or is a well-known service interface
                var hasServiceInterface = serviceType.GetInterfaces()
                    .Any(i => i.Name.EndsWith("Service") || 
                             i.Name == "IServiceScope" || 
                             i.Name == "IServiceCompositionRoot" ||
                             i.Name.StartsWith("I") && i.Name.Contains("Service"));

                hasServiceInterface.ShouldBeTrue(
                    $"Service class {serviceType.Name} must implement a service interface. " +
                    "Services should follow the interface-implementation pattern for testability and DI.");
            }
        }

        /// <summary>
        /// Rule 3: Snapshot classes must be sealed to prevent inheritance attacks.
        /// </summary>
        [Fact]
        public void Snapshot_Classes_Must_Be_Sealed()
        {
            // Arrange
            var assembly = LoadCoreAssembly();
            var snapshotTypes = ArchTypes.InAssembly(assembly)
                .That()
                .HaveNameEndingWith("Snapshot")
                .And()
                .AreClasses();

            if (!snapshotTypes.GetTypes().Any())
                return; // No snapshots to test

            // Act & Assert
            var result = snapshotTypes
                .Should()
                .BeSealed()
                .GetResult();

            result.IsSuccessful.ShouldBeTrue("All snapshot classes must be sealed to prevent inheritance-based state exposure attacks.");
        }

        /// <summary>
        /// Rule 4: Interface names should follow conventions.
        /// </summary>
        [Fact]
        public void Interface_Names_Should_Follow_Conventions()
        {
            // Arrange
            var assembly = LoadCoreAssembly();
            var publicInterfaces = ArchTypes.InAssembly(assembly)
                .That()
                .AreInterfaces()
                .And()
                .ArePublic()
                .GetTypes();

            if (!publicInterfaces.Any())
                return;

            // Act & Assert
            foreach (var interfaceType in publicInterfaces)
            {
                // Core interfaces should start with 'I'
                interfaceType.Name.StartsWith("I").ShouldBeTrue(
                    $"Interface {interfaceType.Name} should start with 'I' following C# conventions.");
            }
        }

        private static Assembly LoadCoreAssembly()
        {
            // First try to load from current directory (test output)
            var localPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "BarkMoon.GameComposition.Core.dll");
            if (File.Exists(localPath))
            {
                return Assembly.LoadFrom(localPath);
            }

            // Fallback to relative path for design-time/build scenarios
            var basePath = Path.GetFullPath(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "..", ".."));
            var path = Path.Combine(basePath, "Core", "bin", "Debug", "net10.0", "BarkMoon.GameComposition.Core.dll");
            return Assembly.LoadFrom(path);
        }
    }
}
