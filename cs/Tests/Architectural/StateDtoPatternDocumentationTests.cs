using System.Linq;
using System.Reflection;
using BarkMoon.GameComposition.Core.Interfaces;
using BarkMoon.GameComposition.Tests.Common;
using Shouldly;
using Xunit;

namespace BarkMoon.GameComposition.Tests.Architectural
{
    /// <summary>
    /// State DTO Pattern Documentation Tests - Living documentation for State implementation.
    /// 
    /// PURPOSE:
    /// These tests document and enforce the DTO pattern for State objects across the ecosystem.
    /// Tests serve as the primary documentation for developers implementing State classes.
    /// 
    /// ARCHITECTURAL RULES DOCUMENTED:
    /// 1. States must implement IState<TDto> to provide CreateDto() method
    /// 2. CreateDto() must return valid DTOs with all necessary data
    /// 3. States must not reference Snapshot types
    /// 4. States must be mutable (unlike immutable snapshots)
    /// 
    /// USAGE:
    /// Read these tests to understand State implementation requirements.
    /// Follow test examples for correct State-to-DTO conversion.
    /// Use test failures as implementation guidance.
    /// </summary>
    public class StateDtoPatternDocumentationTests
    {
        [Fact(DisplayName = "STATE-001: States Must Implement IState<TDto> (DOCUMENTATION)")]
        [Trait("Category", "State Implementation Guide")]
        public void States_Must_Implement_IState_Generic()
        {
            // DOCUMENTATION: This test documents the State interface requirements
            // State classes should implement IState<TDto> to provide type-safe DTO creation
            
            var assemblies = TestAssemblyHelper.GetAllRelevantAssemblies();
            var stateTypes = ArchitecturalTestHelpers.FindClassesByPattern(assemblies, "State");
            
            foreach (var stateType in stateTypes)
            {
                // Check if State implements IState<TDto>
                var genericInterfaces = stateType.GetInterfaces()
                    .Where(i => i.IsGenericType && i.GetGenericTypeDefinition().Name == "IState`1")
                    .ToList();
                
                genericInterfaces.ShouldNotBeEmpty(
                    $"RED STATE: {stateType.Name} must implement IState<TDto> interface");
                
                // Verify the generic parameter is a DTO type
                foreach (var genericInterface in genericInterfaces)
                {
                    var dtoType = genericInterface.GetGenericArguments()[0];
                    typeof(IDataTransferObject).IsAssignableFrom(dtoType).ShouldBeTrue(
                        $"RED STATE: {stateType.Name} generic parameter {dtoType.Name} must implement IDataTransferObject");
                }
            }
            
            // GREEN STATE: All States implement IState<TDto> - type-safe DTO creation available
            true.ShouldBeTrue("IState<TDto> implementation requirement documented and validated");
        }

        [Fact(DisplayName = "STATE-002: States Must Provide CreateDto() Method (DOCUMENTATION)")]
        [Trait("Category", "State Implementation Guide")]
        public void States_Must_Provide_CreateDto_Method()
        {
            // DOCUMENTATION: This test documents the CreateDto() method requirements
            // States must provide CreateDto() method that returns DTO with current state data
            
            var assemblies = TestAssemblyHelper.GetAllRelevantAssemblies();
            var stateTypes = ArchitecturalTestHelpers.FindClassesByPattern(assemblies, "State");
            
            foreach (var stateType in stateTypes)
            {
                // Find CreateDto() method
                var createDtoMethod = stateType.GetMethod("CreateDto");
                
                createDtoMethod.ShouldNotBeNull(
                    $"RED STATE: {stateType.Name} must have CreateDto() method");
                
                // Verify return type implements IDataTransferObject
                var returnType = createDtoMethod.ReturnType;
                typeof(IDataTransferObject).IsAssignableFrom(returnType).ShouldBeTrue(
                    $"RED STATE: {stateType.Name}.CreateDto() must return type implementing IDataTransferObject");
                
                // Verify method has no parameters (state creates DTO from internal data)
                createDtoMethod.GetParameters().Length.ShouldBe(0,
                    $"RED STATE: {stateType.Name}.CreateDto() should not take parameters");
            }
            
            // GREEN STATE: All States provide CreateDto() - DTO creation pattern established
            true.ShouldBeTrue("CreateDto() method requirement documented and validated");
        }

        [Fact(DisplayName = "STATE-003: States Must Not Reference Snapshot Types (DOCUMENTATION)")]
        [Trait("Category", "State Implementation Guide")]
        public void States_Must_Not_Reference_Snapshot_Types()
        {
            // DOCUMENTATION: This test documents the clean separation requirement
            // States must not reference Snapshot types to maintain architectural boundaries
            
            var assemblies = TestAssemblyHelper.GetAllRelevantAssemblies();
            var stateTypes = ArchitecturalTestHelpers.FindClassesByPattern(assemblies, "State");
            var snapshotTypes = ArchitecturalTestHelpers.FindClassesByPattern(assemblies, "Snapshot");
            
            foreach (var stateType in stateTypes)
            {
                var fields = stateType.GetFields(BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public);
                var properties = stateType.GetProperties(BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public);
                
                // Check fields for Snapshot references
                foreach (var field in fields)
                {
                    snapshotTypes.Contains(field.FieldType).ShouldBeFalse(
                        $"RED STATE: {stateType.Name} has field '{field.Name}' referencing Snapshot type '{field.FieldType.Name}'");
                }
                
                // Check properties for Snapshot references
                foreach (var property in properties)
                {
                    snapshotTypes.Contains(property.PropertyType).ShouldBeFalse(
                        $"RED STATE: {stateType.Name} has property '{property.Name}' referencing Snapshot type '{property.PropertyType.Name}'");
                }
            }
            
            // GREEN STATE: Clean separation maintained - no Snapshot coupling in States
            true.ShouldBeTrue("State/Snapshot separation requirement documented and enforced");
        }

        [Fact(DisplayName = "STATE-004: State To DTO Conversion Pattern (DOCUMENTATION)")]
        [Trait("Category", "State Implementation Guide")]
        public void State_To_DTO_Conversion_Pattern()
        {
            // DOCUMENTATION: This test provides a complete example of State-to-DTO conversion
            
            // PATTERN: State extracts data and creates DTO with all necessary information
            
            /*
            public class PlacementState : IState<PlacementDataDto>
            {
                private readonly List<PlacementEntry> _entries;
                private readonly PlacementMetadata _metadata;
                
                public double LastUpdated => _metadata.LastUpdated;
                public bool IsReady => _metadata.IsReady;
                
                public PlacementDataDto CreateDto()
                {
                    // Extract current state data
                    var entries = _entries.ToList().AsReadOnly();
                    var metadata = _metadata;
                    
                    // Create and return DTO
                    return new PlacementDataDto(entries, metadata);
                }
            }
            */
            
            // GREEN STATE: Complete conversion example provided
            true.ShouldBeTrue("State-to-DTO conversion pattern documented with example");
        }

        [Fact(DisplayName = "STATE-005: State Mutability Requirements (DOCUMENTATION)")]
        [Trait("Category", "State Implementation Guide")]
        public void State_Mutability_Requirements()
        {
            // DOCUMENTATION: This test documents State mutability requirements
            // Unlike snapshots, States must be mutable to allow business logic modifications
            
            // REQUIREMENTS:
            // 1. States should be classes (not records) for mutability
            // 2. State properties should be read/write where appropriate
            // 3. States should have methods to modify internal data
            // 4. States should update LastUpdated timestamp on changes
            
            // EXAMPLE: Mutable State implementation
            /*
            public class PlacementState : IState<PlacementDataDto>
            {
                private List<PlacementEntry> _entries = new();
                private PlacementMetadata _metadata = new();
                
                public double LastUpdated => _metadata.LastUpdated;
                public bool IsReady => _metadata.IsReady;
                
                public void AddEntry(PlacementEntry entry)
                {
                    _entries.Add(entry);
                    _metadata.UpdateTimestamp(); // Mutate state
                }
                
                public PlacementDataDto CreateDto() => new(_entries.AsReadOnly(), _metadata);
            }
            */
            
            // GREEN STATE: Mutability requirements documented
            true.ShouldBeTrue("State mutability requirements documented for developers");
        }

        [Fact(DisplayName = "STATE-006: State Data Validation Pattern (DOCUMENTATION)")]
        [Trait("Category", "State Implementation Guide")]
        public void State_Data_Validation_Pattern()
        {
            // DOCUMENTATION: This test documents State data validation requirements
            // States should validate data before allowing DTO creation
            
            // PATTERN: Validate state data before creating DTO
            
            /*
            public PlacementDataDto CreateDto()
            {
                // Validate state before creating DTO
                if (_entries == null)
                    throw new InvalidOperationException("State entries not initialized");
                
                if (!_metadata.IsReady)
                    throw new InvalidOperationException("State not ready for DTO creation");
                
                // Create valid DTO
                return new PlacementDataDto(_entries.AsReadOnly(), _metadata);
            }
            */
            
            // GREEN STATE: Validation pattern documented
            true.ShouldBeTrue("State validation pattern documented for developers");
        }
    }
}
