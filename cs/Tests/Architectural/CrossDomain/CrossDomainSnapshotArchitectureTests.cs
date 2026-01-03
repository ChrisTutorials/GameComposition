using NetArchTest.Rules;
using Shouldly;
using System.Linq;
using System.Reflection;
using System.IO;
using BarkMoon.GameComposition.Core.Interfaces;
using BarkMoon.GameComposition.Tests.Common;
using Xunit;

// Explicitly alias the NetArchTest Types class to avoid conflict
using ArchTypes = NetArchTest.Rules.Types;

namespace BarkMoon.GameComposition.Core.Tests.Architectural
{
    /// <summary>
    /// Cross-domain architectural tests that enforce snapshot patterns across ALL plugins.
    /// These tests ensure consistent snapshot architecture throughout the entire ecosystem.
    /// Tests are loaded dynamically from all plugin assemblies to enforce universal compliance.
    /// </summary>
    public class CrossDomainSnapshotArchitectureTests : ArchitecturalTestBase
    {
        /// <summary>
        /// Rule 1: ALL snapshots across ALL plugins must implement non-generic ISnapshot interface.
        /// This ensures consistent snapshot contract across the entire ecosystem.
        /// </summary>
        [Fact]
        [Trait("Category", "Architectural")]
        public void All_Snapshots_Must_Implement_Non_Generic_ISnapshot()
        {
            // Arrange - Load all plugin assemblies using SSOT
            var assemblies = AllAssemblies;
            var allSnapshotTypes = new List<Type>();

            foreach (var assembly in assemblies)
            {
                var snapshots = ArchTypes.InAssembly(assembly)
                    .That()
                    .HaveNameEndingWith("Snapshot", StringComparison.OrdinalIgnoreCase)
                    .And()
                    .AreClasses()
                    .And()
                    .AreNotAbstract()
                    .GetTypes();

                allSnapshotTypes.AddRange(snapshots);
            }

            if (allSnapshotTypes.Count == 0)
                return; // No snapshots found in any plugin

            // Act & Assert
            foreach (var snapshotType in allSnapshotTypes)
            {
                var implementsISnapshot = typeof(ISnapshot).IsAssignableFrom(snapshotType);
                implementsISnapshot.ShouldBeTrue(
                    $"Snapshot {snapshotType.Name} in assembly {snapshotType.Assembly.GetName().Name} must implement ISnapshot interface. " +
                    "All snapshots must use the non-generic ISnapshot for ecosystem consistency.");
            }
        }

        /// <summary>
        /// Rule 2: ALL snapshots across ALL plugins must be sealed to prevent inheritance attacks.
        /// Security enforcement applies universally across the plugin ecosystem.
        /// </summary>
        [Fact]
        [Trait("Category", "Architectural")]
        public void All_Snapshots_Must_Be_Sealed()
        {
            // Arrange
            var assemblies = AllAssemblies;
            var allSnapshotTypes = new List<Type>();

            foreach (var assembly in assemblies)
            {
                var snapshots = ArchTypes.InAssembly(assembly)
                    .That()
                    .HaveNameEndingWith("Snapshot", StringComparison.OrdinalIgnoreCase)
                    .And()
                    .AreClasses()
                    .GetTypes();

                allSnapshotTypes.AddRange(snapshots);
            }

            if (allSnapshotTypes.Count == 0)
                return;

            // Act & Assert
            foreach (var snapshotType in allSnapshotTypes)
            {
                snapshotType.IsSealed.ShouldBeTrue(
                    $"Snapshot {snapshotType.Name} in assembly {snapshotType.Assembly.GetName().Name} must be sealed. " +
                    "All snapshots must be sealed to prevent inheritance-based security vulnerabilities across the ecosystem.");
            }
        }

        /// <summary>
        /// Rule 3: ALL snapshots across ALL plugins must not reference State types in fields or properties.
        /// Enforces clean separation between mutable state and immutable snapshots.
        /// </summary>
        [Fact]
        [Trait("Category", "Architectural")]
        public void All_Snapshots_Must_Not_Reference_State_Types()
        {
            // Arrange
            var assemblies = AllAssemblies;
            var allSnapshotTypes = new List<Type>();
            var allStateTypes = new List<Type>();

            // Collect all snapshots and state types
            foreach (var assembly in assemblies)
            {
                var snapshots = ArchTypes.InAssembly(assembly)
                    .That()
                    .HaveNameEndingWith("Snapshot", StringComparison.OrdinalIgnoreCase)
                    .And()
                    .AreClasses()
                    .GetTypes();

                var states = ArchTypes.InAssembly(assembly)
                    .That()
                    .HaveNameEndingWith("State", StringComparison.OrdinalIgnoreCase)
                    .And()
                    .AreClasses()
                    .GetTypes();

                allSnapshotTypes.AddRange(snapshots);
                allStateTypes.AddRange(states);
            }

            if (allSnapshotTypes.Count == 0)
                return;

            // Act & Assert
            foreach (var snapshotType in allSnapshotTypes)
            {
                var fields = snapshotType.GetFields(BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public);
                var properties = snapshotType.GetProperties(BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public);

                // Check fields for State references
                foreach (var field in fields)
                {
                    allStateTypes.Contains(field.FieldType).ShouldBeFalse(
                        $"Snapshot {snapshotType.Name} in assembly {snapshotType.Assembly.GetName().Name} has field '{field.Name}' " +
                        $"that references State type '{field.FieldType.Name}'. Snapshots must not reference State types to maintain immutability.");
                }

                // Check properties for State references (skip Data property which returns snapshot itself)
                foreach (var property in properties)
                {
                    if (property.Name == "Data") continue;
                    
                    allStateTypes.Contains(property.PropertyType).ShouldBeFalse(
                        $"Snapshot {snapshotType.Name} in assembly {snapshotType.Assembly.GetName().Name} has property '{property.Name}' " +
                        $"that references State type '{property.PropertyType.Name}'. Snapshots must not reference State types to maintain immutability.");
                }
            }
        }

        /// <summary>
        /// Rule 4: ALL snapshot constructors across ALL plugins must only accept shared data types.
        /// Prevents State coupling in snapshot construction across the ecosystem.
        /// </summary>
        [Fact]
        [Trait("Category", "Architectural")]
        public void All_Snapshot_Constructors_Must_Accept_Shared_Data_Types_Only()
        {
            // Arrange
            var assemblies = AllAssemblies;
            var allSnapshotTypes = new List<Type>();

            foreach (var assembly in assemblies)
            {
                var snapshots = ArchTypes.InAssembly(assembly)
                    .That()
                    .ImplementInterface(typeof(ISnapshot))
                    .And()
                    .AreClasses()
                    .And()
                    .AreNotAbstract()
                    .GetTypes();

                allSnapshotTypes.AddRange(snapshots);
            }

            if (allSnapshotTypes.Count == 0)
                return;

            // Act & Assert
            foreach (var snapshotType in allSnapshotTypes)
            {
                var constructors = snapshotType.GetConstructors(BindingFlags.Instance | BindingFlags.NonPublic);
                
                constructors.ShouldNotBeEmpty($"Snapshot {snapshotType.Name} must have at least one constructor.");

                foreach (var constructor in constructors)
                {
                    var parameters = constructor.GetParameters();
                    
                    // Should have at least one parameter (data to construct from)
                    parameters.Length.ShouldBeGreaterThan(0, 
                        $"Snapshot {snapshotType.Name} constructor must accept data parameters.");

                    // All parameters should be data types, not state types
                    foreach (var param in parameters)
                    {
                        var paramType = param.ParameterType;
                        
                        // Allow primitive types, collections, and record types
                        var isAllowedType = paramType.IsPrimitive || 
                                         paramType.IsEnum ||
                                         paramType.Name.StartsWith("IReadOnlyList", StringComparison.OrdinalIgnoreCase) ||
                                         paramType.Name.StartsWith("IEnumerable", StringComparison.OrdinalIgnoreCase) ||
                                         paramType.Name.EndsWith("Entry", StringComparison.OrdinalIgnoreCase) ||
                                         paramType.Name.EndsWith("Metadata", StringComparison.OrdinalIgnoreCase) ||
                                         paramType.Name.EndsWith("Data", StringComparison.OrdinalIgnoreCase) ||
                                         paramType.Namespace?.Contains("Data", StringComparison.OrdinalIgnoreCase) == true ||
                                         paramType.Namespace?.EndsWith(".Types", StringComparison.OrdinalIgnoreCase) == true;
                        
                        isAllowedType.ShouldBeTrue(
                            $"Snapshot {snapshotType.Name} constructor parameter '{param.Name}' of type '{paramType.Name}' " +
                            $"should be a shared data type, not a State type. " +
                            "Snapshots across the ecosystem must only accept immutable data types to maintain isolation from mutable state.");
                    }
                }
            }
        }

        /// <summary>
        /// Rule 5: ALL State classes across ALL plugins must not reference Snapshot types.
        /// Prevents circular dependencies and maintains clean architecture.
        /// </summary>
        [Fact]
        [Trait("Category", "Architectural")]
        public void All_State_Classes_Must_Not_Reference_Snapshot_Types()
        {
            // Arrange
            var assemblies = AllAssemblies;
            var allStateTypes = new List<Type>();
            var allSnapshotTypes = new List<Type>();

            // Collect all states and snapshots
            foreach (var assembly in assemblies)
            {
                var states = ArchTypes.InAssembly(assembly)
                    .That()
                    .HaveNameEndingWith("State", StringComparison.OrdinalIgnoreCase)
                    .And()
                    .AreClasses()
                    .GetTypes();

                var snapshots = ArchTypes.InAssembly(assembly)
                    .That()
                    .HaveNameEndingWith("Snapshot", StringComparison.OrdinalIgnoreCase)
                    .And()
                    .AreClasses()
                    .GetTypes();

                allStateTypes.AddRange(states);
                allSnapshotTypes.AddRange(snapshots);
            }

            if (allStateTypes.Count == 0)
                return;

            // Act & Assert
            foreach (var stateType in allStateTypes)
            {
                var fields = stateType.GetFields(BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public);
                var properties = stateType.GetProperties(BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public);

                // Check fields for Snapshot references
                foreach (var field in fields)
                {
                    allSnapshotTypes.Contains(field.FieldType).ShouldBeFalse(
                        $"State {stateType.Name} in assembly {stateType.Assembly.GetName().Name} has field '{field.Name}' " +
                        $"that references Snapshot type '{field.FieldType.Name}'. State classes must not reference Snapshot types to maintain clean architecture.");
                }

                // Check properties for Snapshot references
                foreach (var property in properties)
                {
                    allSnapshotTypes.Contains(property.PropertyType).ShouldBeFalse(
                        $"State {stateType.Name} in assembly {stateType.Assembly.GetName().Name} has property '{property.Name}' " +
                        $"that references Snapshot type '{property.PropertyType.Name}'. State classes must not reference Snapshot types to maintain clean architecture.");
                }
            }
        }

        /// <summary>
        /// Rule 6: ALL shared data types across ALL plugins must be immutable records.
        /// Ensures data contracts are immutable and safe for snapshot consumption.
        /// </summary>
        [Fact]
        [Trait("Category", "Architectural")]
        public void All_Shared_Data_Types_Must_Be_Immutable_Records()
        {
            // Arrange
            var assemblies = AllAssemblies;
            var allDataTypes = new List<Type>();

            // Collect all types in Data namespaces
            foreach (var assembly in assemblies)
            {
                var dataTypes = ArchTypes.InAssembly(assembly)
                    .That()
                    .ResideInNamespace("*Data*")
                    .And()
                    .AreClasses()
                    .GetTypes();

                allDataTypes.AddRange(dataTypes);
            }

            if (allDataTypes.Count == 0)
                return;

            // Act & Assert
            foreach (var dataType in allDataTypes)
            {
                // Should be a record (indicated by IsValueType and appropriate attributes)
                var isRecord = dataType.IsValueType && 
                               dataType.GetMethods().Any(m => m.Name == "Deconstruct") &&
                               dataType.GetFields().All(f => f.IsInitOnly);
                
                isRecord.ShouldBeTrue(
                    $"Data type {dataType.Name} in assembly {dataType.Assembly.GetName().Name} should be an immutable record. " +
                    "Shared data types across the ecosystem must be immutable to ensure thread safety and snapshot compatibility.");
            }
        }
    }
}
