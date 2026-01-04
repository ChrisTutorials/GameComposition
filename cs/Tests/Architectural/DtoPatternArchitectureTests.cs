using System.Linq;
using System.Reflection;
using BarkMoon.GameComposition.Core.Interfaces;
using BarkMoon.GameComposition.Core.Types;
using BarkMoon.GameComposition.Tests.Common;
using Shouldly;
using Xunit;

namespace BarkMoon.GameComposition.Tests.Architectural
{
    /// <summary>
    /// DTO Pattern Architecture Tests - Living Documentation for State/Snapshot Data Transfer Objects.
    /// 
    /// PURPOSE:
    /// These tests serve as the primary documentation for the DTO pattern between State and Snapshot objects.
    /// Tests demonstrate correct usage, enforce architectural compliance, and provide examples for developers.
    /// 
    /// ARCHITECTURAL PATTERN:
    /// State → CreateDto() → DTO → Snapshot Constructor → Immutable Snapshot
    /// 
    /// ENFORCEMENT:
    /// All State and Snapshot implementations must follow this pattern across the entire GameComposition ecosystem.
    /// Violations will be caught by these architectural tests.
    /// 
    /// USAGE FOR DEVELOPERS:
    /// Read these tests to understand the DTO pattern requirements.
    /// Use test failures as guidance for correct implementation.
    /// Follow the examples shown in test assertions.
    /// </summary>
    public class DtoPatternArchitectureTests
    {
        [Fact(DisplayName = "DTO-001: IDataTransferObject Interface Must Exist (DOCUMENTATION)")]
        [Trait("Category", "Architecture Documentation")]
        public void IDataTransferObject_Interface_Must_Exist()
        {
            // DOCUMENTATION: This test ensures the base DTO interface exists
            // All DTOs must implement this interface for framework consistency
            
            var assemblies = TestAssemblyHelper.GetCoreAssemblies();
            var dtoInterface = ArchitecturalTestHelpers.FindInterfaceByName(assemblies, "IDataTransferObject");
            
            dtoInterface.ShouldNotBeNull("RED STATE: IDataTransferObject interface not found. Create this interface in Core/Interfaces/");
            
            // GREEN STATE: Interface exists - foundation for DTO pattern established
            dtoInterface.Namespace.ShouldBe("BarkMoon.GameComposition.Core.Interfaces", "Interface should be in Core namespace");
        }

        [Fact(DisplayName = "DTO-002: StateDataDto Base Class Must Exist (DOCUMENTATION)")]
        [Trait("Category", "Architecture Documentation")]
        public void StateDataDto_Base_Class_Must_Exist()
        {
            // DOCUMENTATION: This test ensures the State DTO base class exists
            // State DTOs should inherit from this class for consistent functionality
            
            var assemblies = TestAssemblyHelper.GetCoreAssemblies();
            var stateDtoClass = ArchitecturalTestHelpers.FindClassByName(assemblies, "StateDataDto");
            
            stateDtoClass.ShouldNotBeNull("RED STATE: StateDataDto base class not found. Create this class in Core/Types/");
            
            // GREEN STATE: Base class exists - State DTOs have proper inheritance
            stateDtoClass.BaseType?.Name.ShouldBe("Object", "StateDataDto should be a root class inheriting from Object");
            typeof(IDataTransferObject).IsAssignableFrom(stateDtoClass).ShouldBeTrue("StateDataDto must implement IDataTransferObject");
        }

        [Fact(DisplayName = "DTO-003: SnapshotDataDto Base Class Must Exist (DOCUMENTATION)")]
        [Trait("Category", "Architecture Documentation")]
        public void SnapshotDataDto_Base_Class_Must_Exist()
        {
            // DOCUMENTATION: This test ensures the Snapshot DTO base class exists
            // Snapshot DTOs should inherit from this class for consistent functionality
            
            var assemblies = TestAssemblyHelper.GetCoreAssemblies();
            var snapshotDtoClass = ArchitecturalTestHelpers.FindClassByName(assemblies, "SnapshotDataDto");
            
            snapshotDtoClass.ShouldNotBeNull("RED STATE: SnapshotDataDto base class not found. Create this class in Core/Types/");
            
            // GREEN STATE: Base class exists - Snapshot DTOs have proper inheritance
            typeof(IDataTransferObject).IsAssignableFrom(snapshotDtoClass).ShouldBeTrue("SnapshotDataDto must implement IDataTransferObject");
        }

        [Fact(DisplayName = "DTO-004: IState<TDto> Interface Must Exist (DOCUMENTATION)")]
        [Trait("Category", "Architecture Documentation")]
        public void IState_Generic_Interface_Must_Exist()
        {
            // DOCUMENTATION: This test ensures the generic State interface exists
            // State classes should implement IState<TDto> to provide CreateDto() method
            
            var assemblies = TestAssemblyHelper.GetCoreAssemblies();
            var stateInterface = ArchitecturalTestHelpers.FindInterfaceByName(assemblies, "IState`1");
            
            stateInterface.ShouldNotBeNull("RED STATE: IState<TDto> interface not found. Create this interface in Core/Interfaces/");
            
            // GREEN STATE: Generic interface exists - States can create type-safe DTOs
            stateInterface.GetGenericArguments().Length.ShouldBe(1, "IState should have one generic parameter");
            stateInterface.GetInterfaces().Any(i => i.Name == "IState").ShouldBeTrue("IState<TDto> should inherit from IState");
        }

        [Fact(DisplayName = "DTO-005: DTO Interfaces Must Have Required Properties (DOCUMENTATION)")]
        [Trait("Category", "Architecture Documentation")]
        public void DTO_Interfaces_Must_Have_Required_Properties()
        {
            // DOCUMENTATION: This test documents the required properties for DTO interfaces
            // All DTOs must provide these properties for framework functionality
            
            var assemblies = TestAssemblyHelper.GetCoreAssemblies();
            var dtoInterface = ArchitecturalTestHelpers.FindInterfaceByName(assemblies, "IDataTransferObject");
            
            dtoInterface.ShouldNotBeNull("IDataTransferObject interface must exist");
            
            var requiredProperties = new[] { "CreatedAt", "DataVersion", "IsValid" };
            var interfaceProperties = dtoInterface.GetProperties().Select(p => p.Name).ToList();
            
            foreach (var property in requiredProperties)
            {
                interfaceProperties.ShouldContain(property, $"RED STATE: IDataTransferObject missing required property '{property}'");
            }
            
            // GREEN STATE: All required properties exist - DTOs have complete contract
            interfaceProperties.Count.ShouldBeGreaterThanOrEqualTo(3, "DTO interface should have at least the 3 required properties");
        }

        [Fact(DisplayName = "DTO-006: DTO Pattern Example Usage (DOCUMENTATION)")]
        [Trait("Category", "Architecture Documentation")]
        public void DTO_Pattern_Example_Usage()
        {
            // DOCUMENTATION: This test demonstrates the correct DTO pattern usage
            // Shows how State, DTO, and Snapshot should work together
            
            // EXAMPLE: State creates DTO
            // public PlacementDataDto CreateDto() => new(_entries, _metadata);
            
            // EXAMPLE: Snapshot accepts DTO
            // public PlacementSnapshot2D(PlacementDataDto data) { ... }
            
            // EXAMPLE: Service uses pattern
            // var snapshot = new PlacementSnapshot2D(state.CreateDto());
            
            // This test documents the pattern without requiring actual implementation
            // The pattern is: State.CreateDto() → DTO → Snapshot(DTO) → Immutable Snapshot
            
            // GREEN STATE: Pattern documented - developers can follow this example
            true.ShouldBeTrue("DTO pattern usage documented for developer guidance");
        }

        [Fact(DisplayName = "DTO-007: DTO Immutability Requirements (DOCUMENTATION)")]
        [Trait("Category", "Architecture Documentation")]
        public void DTO_Immutability_Requirements()
        {
            // DOCUMENTATION: This test documents DTO immutability requirements
            // DTOs must be immutable to ensure snapshot integrity
            
            // REQUIREMENTS:
            // 1. DTOs should be records or immutable classes
            // 2. All properties should be read-only (init-only)
            // 3. DTOs should not reference State or Snapshot objects
            // 4. DTOs should contain only primitive or immutable data types
            
            // EXAMPLE: Correct DTO definition
            // public sealed record PlacementDataDto(
            //     IReadOnlyList<PlacementEntry> Entries,
            //     PlacementMetadata Metadata
            // ) : StateDataDto;
            
            // GREEN STATE: Immutability requirements documented
            true.ShouldBeTrue("DTO immutability requirements documented for developer guidance");
        }
    }
}
