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
    /// Plugin DTO Compliance Documentation Tests - Implementation guide for plugin developers.
    /// 
    /// PURPOSE:
    /// These tests serve as the primary documentation for plugin developers implementing DTO patterns.
    /// Tests demonstrate correct implementation, provide examples, and guide plugin development.
    /// 
    /// PLUGIN DEVELOPMENT GUIDE:
    /// 1. Read these tests to understand DTO requirements
    /// 2. Follow the examples for your plugin's domain
    /// 3. Use test failures as implementation guidance
    /// 4. Ensure your plugin passes all compliance tests
    /// 
    /// ARCHITECTURAL ENFORCEMENT:
    /// These tests validate DTO pattern compliance across all plugins in the ecosystem.
    /// Violations indicate incorrect implementation that needs correction.
    /// </summary>
    public class PluginDtoComplianceDocumentationTests
    {
        [Fact(DisplayName = "PLUGIN-001: All Snapshots Must Use DTO Pattern (PLUGIN GUIDE)")]
        [Trait("Category", "Plugin Development Guide")]
        public void All_Snapshots_Must_Use_Dto_Pattern()
        {
            // DOCUMENTATION: This test serves as a guide for plugin developers
            // Shows exactly how to implement the DTO pattern in plugins
            
            var assemblies = TestAssemblyHelper.GetAllRelevantAssemblies();
            var snapshotTypes = ArchitecturalTestHelpers.FindClassesByPattern(assemblies, "Snapshot");
            
            foreach (var snapshotType in snapshotTypes)
            {
                // Check if snapshot has DTO constructor
                var constructors = snapshotType.GetConstructors();
                var hasDtoConstructor = constructors.Any(c => 
                {
                    var parameters = c.GetParameters();
                    return parameters.Length == 1 && 
                           typeof(IDataTransferObject).IsAssignableFrom(parameters[0].ParameterType);
                });
                
                hasDtoConstructor.ShouldBeTrue(
                    $"RED STATE: {snapshotType.Name} must have constructor accepting DTO type. " +
                    $"Example: public {snapshotType.Name}({snapshotType.Name.Replace("Snapshot", "Dto")} data)");
                
                // GREEN STATE: Snapshot follows DTO pattern
                // Example implementation:
                /*
                public sealed class YourSnapshot : ISnapshot
                {
                    private readonly YourDto _data;
                    
                    public YourSnapshot(YourDto data)
                    {
                        _data = data ?? throw new ArgumentNullException(nameof(data));
                        if (!_data.IsValid)
                            throw new ArgumentException("Invalid DTO data", nameof(data));
                    }
                    
                    public DateTime SnapshotTimestamp => _data.CreatedAt;
                    // ... other properties from DTO
                }
                */
            }
        }

        [Fact(DisplayName = "PLUGIN-002: All States Must Implement IState<TDto> (PLUGIN GUIDE)")]
        [Trait("Category", "Plugin Development Guide")]
        public void All_States_Must_Implement_IState_Generic()
        {
            // DOCUMENTATION: This test guides plugin developers on State implementation
            // States must implement IState<TDto> to provide CreateDto() method
            
            var assemblies = TestAssemblyHelper.GetAllRelevantAssemblies();
            var stateTypes = ArchitecturalTestHelpers.FindClassesByPattern(assemblies, "State");
            
            foreach (var stateType in stateTypes)
            {
                // Check if State implements IState<TDto>
                var genericInterfaces = stateType.GetInterfaces()
                    .Where(i => i.IsGenericType && i.GetGenericTypeDefinition().Name == "IState`1")
                    .ToList();
                
                genericInterfaces.ShouldNotBeEmpty(
                    $"RED STATE: {stateType.Name} must implement IState<TDto>. " +
                    $"Example: public class {stateType.Name} : IState<{stateType.Name.Replace("State", "Dto")}>");
                
                // GREEN STATE: State follows DTO pattern
                // Example implementation:
                /*
                public class YourState : IState<YourDto>
                {
                    private readonly List<YourEntry> _entries = new();
                    private YourMetadata _metadata = new();
                    
                    public double LastUpdated => _metadata.LastUpdated;
                    public bool IsReady => _metadata.IsReady;
                    
                    public YourDto CreateDto()
                    {
                        return new YourDto(_entries.AsReadOnly(), _metadata);
                    }
                }
                */
            }
        }

        [Fact(DisplayName = "PLUGIN-003: DTO Classes Must Follow Naming Convention (PLUGIN GUIDE)")]
        [Trait("Category", "Plugin Development Guide")]
        public void DTO_Classes_Must_Follow_Naming_Convention()
        {
            // DOCUMENTATION: This test documents DTO naming conventions for plugins
            // Consistent naming helps with code readability and maintenance
            
            var assemblies = TestAssemblyHelper.GetAllRelevantAssemblies();
            var dtoTypes = assemblies
                .SelectMany(assembly => assembly.GetTypes())
                .Where(type => typeof(IDataTransferObject).IsAssignableFrom(type) && 
                             !type.IsInterface && 
                             !type.IsAbstract)
                .ToList();
            
            foreach (var dtoType in dtoTypes)
            {
                // Check naming convention: Should end with "Dto"
                dtoType.Name.ShouldEndWith("Dto");
                
                // GREEN STATE: DTO follows naming convention
                // Examples: PlacementDto, TargetingDto, YourDomainDto
            }
        }

        [Fact(DisplayName = "PLUGIN-004: Complete Plugin Implementation Example (PLUGIN GUIDE)")]
        [Trait("Category", "Plugin Development Guide")]
        public void Complete_Plugin_Implementation_Example()
        {
            // DOCUMENTATION: This test provides a complete example for plugin developers
            // Shows the full DTO pattern implementation from start to finish
            
            // STEP 1: Create your DTO
            /*
            public sealed record YourDomainDto : StateDataDto
            {
                public IReadOnlyList<YourEntry> Entries { get; init; }
                public YourMetadata Metadata { get; init; }
                
                public YourDomainDto(IReadOnlyList<YourEntry> entries, YourMetadata metadata)
                    : base(new { entries, metadata })
                {
                    Entries = entries ?? throw new ArgumentNullException(nameof(entries));
                    Metadata = metadata ?? throw new ArgumentNullException(nameof(metadata));
                }
                
                public override bool IsValid => Entries != null && Metadata != null;
            }
            */
            
            // STEP 2: Implement your State
            /*
            public class YourDomainState : IState<YourDomainDto>
            {
                private readonly List<YourEntry> _entries = new();
                private YourMetadata _metadata = new();
                
                public double LastUpdated => _metadata.LastUpdated;
                public bool IsReady => _metadata.IsReady;
                
                public void AddEntry(YourEntry entry) => _entries.Add(entry);
                
                public YourDomainDto CreateDto() => new(_entries.AsReadOnly(), _metadata);
            }
            */
            
            // STEP 3: Implement your Snapshot
            /*
            public sealed class YourDomainSnapshot : ISnapshot
            {
                private readonly YourDomainDto _data;
                
                public YourDomainSnapshot(YourDomainDto data)
                {
                    _data = data ?? throw new ArgumentNullException(nameof(data));
                    if (!_data.IsValid)
                        throw new ArgumentException("Invalid DTO data", nameof(data));
                }
                
                public DateTime SnapshotTimestamp => _data.CreatedAt;
                public double StateVersion => _data.DataVersion;
                public bool IsValid => _data.IsValid;
                public ICompositionData? Data => null;
                
                public int EntryCount => _data.Entries.Count;
                // ... other properties from DTO
            }
            */
            
            // STEP 4: Use in your Service
            /*
            public YourDomainSnapshot CreateSnapshot()
            {
                var dto = _state.CreateDto();
                return new YourDomainSnapshot(dto);
            }
            */
            
            // GREEN STATE: Complete example provided for plugin developers
            true.ShouldBeTrue("Complete plugin DTO implementation example documented");
        }

        [Fact(DisplayName = "PLUGIN-005: Common DTO Implementation Mistakes (PLUGIN GUIDE)")]
        [Trait("Category", "Plugin Development Guide")]
        public void Common_DTO_Implementation_Mistakes()
        {
            // DOCUMENTATION: This test documents common mistakes to avoid
            // Helps plugin developers avoid implementation errors
            
            // MISTAKE 1: Don't accept State in snapshot constructor
            // WRONG: public YourSnapshot(YourState state) { ... }
            // RIGHT: public YourSnapshot(YourDto data) { ... }
            
            // MISTAKE 2: Don't reference State types in DTOs
            // WRONG: public class YourDto { public YourState State { get; init; } }
            // RIGHT: public class YourDto { public IReadOnlyList<YourEntry> Entries { get; init; } }
            
            // MISTAKE 3: Don't make DTOs mutable
            // WRONG: public class YourDto { public List<YourEntry> Entries { get; set; } }
            // RIGHT: public sealed record YourDto { public IReadOnlyList<YourEntry> Entries { get; init; } }
            
            // MISTAKE 4: Don't forget CreateDto() method
            // WRONG: public class YourState : IState { /* no CreateDto() */ }
            // RIGHT: public class YourState : IState<YourDto> { public YourDto CreateDto() => ...; }
            
            // MISTAKE 5: Don't use wrong base classes
            // WRONG: public class YourDto : SomeOtherBase { }
            // RIGHT: public sealed record YourDto : StateDataDto { }
            
            // GREEN STATE: Common mistakes documented for avoidance
            true.ShouldBeTrue("Common DTO implementation mistakes documented for developers");
        }

        [Fact(DisplayName = "PLUGIN-006: DTO Validation Best Practices (PLUGIN GUIDE)")]
        [Trait("Category", "Plugin Development Guide")]
        public void DTO_Validation_Best_Practices()
        {
            // DOCUMENTATION: This test documents DTO validation best practices
            // Helps plugin developers implement robust validation
            
            // VALIDATION PATTERN 1: Constructor validation
            /*
            public YourDto(IReadOnlyList<YourEntry> entries, YourMetadata metadata)
            {
                Entries = entries ?? throw new ArgumentNullException(nameof(entries));
                Metadata = metadata ?? throw new ArgumentNullException(nameof(metadata));
            }
            */
            
            // VALIDATION PATTERN 2: Domain-specific validation
            /*
            public override bool IsValid
            {
                get
                {
                    if (!base.IsValid) return false;
                    if (Entries == null) return false;
                    if (Metadata == null) return false;
                    if (Entries.Count == 0 && Metadata.RequiresEntries) return false;
                    return true;
                }
            }
            */
            
            // VALIDATION PATTERN 3: Snapshot constructor validation
            /*
            public YourSnapshot(YourDto data)
            {
                _data = data ?? throw new ArgumentNullException(nameof(data));
                if (!_data.IsValid)
                    throw new ArgumentException("Invalid DTO data provided", nameof(data));
            }
            */
            
            // GREEN STATE: Validation best practices documented
            true.ShouldBeTrue("DTO validation best practices documented for developers");
        }
    }
}
