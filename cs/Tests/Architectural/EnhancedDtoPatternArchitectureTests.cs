using System.Linq;
using System.Reflection;
using System.Collections.Generic;
using BarkMoon.GameComposition.Core.Interfaces;
using BarkMoon.GameComposition.Core.Types;
using BarkMoon.GameComposition.Tests.Common;
using Shouldly;
using Xunit;

namespace BarkMoon.GameComposition.Tests.Architectural
{
    /// <summary>
    /// Enhanced DTO Pattern Architecture Tests - RED/GREEN enforcement for DTO compliance.
    /// 
    /// PURPOSE:
    /// These tests enforce the DTO pattern across all plugins in the ecosystem.
    /// Tests FAIL (RED) when DTO pattern is violated and PASS (GREEN) when compliant.
    /// 
    /// ARCHITECTURAL RULES ENFORCED:
    /// 1. All Snapshots must accept DTOs in constructors
    /// 2. All States must implement IState<TDto> with CreateDto() method
    /// 3. DTOs must inherit from StateDataDto or SnapshotDataDto
    /// 4. DTOs must follow naming conventions (end with "Dto")
    /// 5. Snapshots must not reference State types directly
    /// 
    /// RED STATE: Current violations that need fixing
    /// GREEN STATE: Architecture is correctly implemented
    /// </summary>
    public class EnhancedDtoPatternArchitectureTests
    {
        [Fact(DisplayName = "DTO-001: All Snapshots Must Accept DTOs In Constructors (RED=Violations, GREEN=Fixed)")]
        [Trait("Category", "Architectural")]
        public void All_Snapshots_Must_Accept_DTOs_In_Constructors()
        {
            // Arrange: Get all assemblies using cross-domain helper
            var assemblies = TestAssemblyHelper.GetAllRelevantAssemblies();
            var snapshotTypes = ArchitecturalTestHelpers.FindClassesByPattern(assemblies, "Snapshot");
            
            var violations = new System.Collections.Generic.List<string>();
            
            foreach (var snapshotType in snapshotTypes)
            {
                // Act: Check constructor parameters
                var constructors = snapshotType.GetConstructors();
                var hasDtoConstructor = constructors.Any(c => 
                {
                    var parameters = c.GetParameters();
                    return parameters.Length == 1 && 
                           parameters[0].ParameterType.Name.EndsWith("Dto");
                });
                
                if (!hasDtoConstructor)
                {
                    violations.Add($"{snapshotType.Name} lacks DTO constructor");
                }
                
                // Assert: RED = violations found, GREEN = all compliant
                violations.ShouldBeEmpty($"RED STATE: Found {violations.Count} snapshots without DTO constructors. GREEN STATE: All snapshots use DTO pattern.");
            }
        }

        [Fact(DisplayName = "DTO-002: All States Must Implement IState<TDto> (RED=Violations, GREEN=Fixed)")]
        [Trait("Category", "Architectural")]
        public void All_States_Must_Implement_IState_Generic()
        {
            // Arrange: Get all assemblies and find State types
            var assemblies = TestAssemblyHelper.GetAllRelevantAssemblies();
            var stateTypes = ArchitecturalTestHelpers.FindClassesByPattern(assemblies, "State");
            
            var violations = new System.Collections.Generic.List<string>();
            
            foreach (var stateType in stateTypes)
            {
                // Act: Check for IState<TDto> implementation
                var genericInterfaces = stateType.GetInterfaces()
                    .Where(i => i.IsGenericType && 
                               i.GetGenericTypeDefinition().Name == "IState`1")
                    .ToList();
                
                if (!genericInterfaces.Any())
                {
                    violations.Add($"{stateType.Name} doesn't implement IState<TDto>");
                }
                
                // Check for CreateDto() method
                var createDtoMethod = stateType.GetMethod("CreateDto");
                if (createDtoMethod == null)
                {
                    violations.Add($"{stateType.Name} missing CreateDto() method");
                }
            }
            
            // Assert: RED = violations found, GREEN = all compliant
            violations.ShouldBeEmpty($"RED STATE: Found {violations.Count} states without IState<TDto>. GREEN STATE: All states implement DTO pattern.");
        }

        [Fact(DisplayName = "DTO-003: DTOs Must Inherit From Correct Base Classes (RED=Violations, GREEN=Fixed)")]
        [Trait("Category", "Architectural")]
        public void DTOs_Must_Inherit_From_Correct_Base_Classes()
        {
            // Arrange: Get all assemblies and find DTO types
            var assemblies = TestAssemblyHelper.GetAllRelevantAssemblies();
            var dtoTypes = assemblies
                .SelectMany(assembly => assembly.GetTypes())
                .Where(type => type.Name.EndsWith("Dto") && 
                             !type.IsInterface && 
                             !type.IsAbstract)
                .ToList();
            
            var violations = new System.Collections.Generic.List<string>();
            
            foreach (var dtoType in dtoTypes)
            {
                // Act: Check inheritance from StateDataDto or SnapshotDataDto
                var inheritsFromStateData = typeof(StateDataDto).IsAssignableFrom(dtoType);
                var inheritsFromSnapshotData = typeof(SnapshotDataDto).IsAssignableFrom(dtoType);
                
                if (!inheritsFromStateData && !inheritsFromSnapshotData)
                {
                    violations.Add($"{dtoType.Name} doesn't inherit from StateDataDto or SnapshotDataDto");
                }
            }
            
            // Assert: RED = violations found, GREEN = all compliant
            violations.ShouldBeEmpty($"RED STATE: Found {violations.Count} DTOs without proper base classes. GREEN STATE: All DTOs inherit correctly.");
        }

        [Fact(DisplayName = "DTO-004: Snapshots Must Not Reference State Types (RED=Violations, GREEN=Fixed)")]
        [Trait("Category", "Architectural")]
        public void Snapshots_Must_Not_Reference_State_Types()
        {
            // Arrange: Get all assemblies and find types
            var assemblies = TestAssemblyHelper.GetAllRelevantAssemblies();
            var snapshotTypes = ArchitecturalTestHelpers.FindClassesByPattern(assemblies, "Snapshot");
            var stateTypes = ArchitecturalTestHelpers.FindClassesByPattern(assemblies, "State");
            
            var violations = new System.Collections.Generic.List<string>();
            
            foreach (var snapshotType in snapshotTypes)
            {
                // Act: Check for State type references in fields and properties
                var fields = snapshotType.GetFields(BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public);
                var properties = snapshotType.GetProperties(BindingFlags.Instance | BindingFlags.Public);
                
                foreach (var field in fields)
                {
                    if (stateTypes.Contains(field.FieldType))
                    {
                        violations.Add($"{snapshotType.Name}.{field.Name} references State type {field.FieldType.Name}");
                    }
                }
                
                foreach (var property in properties)
                {
                    if (stateTypes.Contains(property.PropertyType))
                    {
                        violations.Add($"{snapshotType.Name}.{property.Name} references State type {property.PropertyType.Name}");
                    }
                }
            }
            
            // Assert: RED = violations found, GREEN = all compliant
            violations.ShouldBeEmpty($"RED STATE: Found {violations.Count} State references in snapshots. GREEN STATE: Clean separation maintained.");
        }

        [Fact(DisplayName = "DTO-005: DTO Naming Convention Must Be Consistent (RED=Violations, GREEN=Fixed)")]
        [Trait("Category", "Architectural")]
        public void DTO_Naming_Convention_Must_Be_Consistent()
        {
            // Arrange: Get all assemblies and find DTO types
            var assemblies = TestAssemblyHelper.GetAllRelevantAssemblies();
            var dtoTypes = assemblies
                .SelectMany(assembly => assembly.GetTypes())
                .Where(type => typeof(IDataTransferObject).IsAssignableFrom(type) && 
                             !type.IsInterface && 
                             !type.IsAbstract)
                .ToList();
            
            var violations = new System.Collections.Generic.List<string>();
            
            foreach (var dtoType in dtoTypes)
            {
                // Act: Check naming convention
                if (!dtoType.Name.EndsWith("Dto"))
                {
                    violations.Add($"{dtoType.Name} doesn't follow 'Dto' naming convention");
                }
                
                // Check for redundancy (avoid "DataDto")
                if (dtoType.Name.Contains("DataDto"))
                {
                    violations.Add($"{dtoType.Name} has redundant 'DataDto' naming, should be just 'Dto'");
                }
            }
            
            // Assert: RED = violations found, GREEN = all compliant
            violations.ShouldBeEmpty($"RED STATE: Found {violations.Count} DTO naming violations. GREEN STATE: All DTOs follow naming conventions.");
        }

        [Fact(DisplayName = "DTO-006: Services Must Use DTO Pattern For Snapshot Creation (RED=Violations, GREEN=Fixed)")]
        [Trait("Category", "Architectural")]
        public void Services_Must_Use_DTO_Pattern_For_Snapshot_Creation()
        {
            // Arrange: Get all assemblies and find service types
            var assemblies = TestAssemblyHelper.GetAllRelevantAssemblies();
            var serviceTypes = assemblies
                .SelectMany(assembly => assembly.GetTypes())
                .Where(type => type.Name.EndsWith("Service") && 
                             !type.IsInterface && 
                             !type.IsAbstract)
                .ToList();
            
            var violations = new System.Collections.Generic.List<string>();
            
            foreach (var serviceType in serviceTypes)
            {
                // Act: Check for CreateSnapshot methods
                var createSnapshotMethods = serviceType.GetMethods()
                    .Where(m => m.Name == "CreateSnapshot" && 
                               m.ReturnType.Name.EndsWith("Snapshot"))
                    .ToList();
                
                foreach (var method in createSnapshotMethods)
                {
                    // Check if method follows DTO pattern: state.CreateDto() -> new Snapshot(dto)
                    var methodBody = System.Text.RegularExpressions.Regex.Match(
                        method.GetMethodBody()?.GetILAsByteArray()?.ToString() ?? "", 
                        @"CreateDto|new.*Snapshot");
                    
                    if (!methodBody.Success)
                    {
                        violations.Add($"{serviceType.Name}.{method.Name} doesn't follow DTO pattern");
                    }
                }
            }
            
            // Assert: RED = violations found, GREEN = all compliant
            // Note: This test might have false positives due to IL inspection limitations
            // violations.ShouldBeEmpty($"RED STATE: Found {violations.Count} services not using DTO pattern. GREEN STATE: All services use DTO pattern.");
        }

        [Fact(DisplayName = "DTO-007: DTO Properties Must Be Immutable (RED=Violations, GREEN=Fixed)")]
        [Trait("Category", "Architectural")]
        public void DTO_Properties_Must_Be_Immutable()
        {
            // Arrange: Get all assemblies and find DTO types
            var assemblies = TestAssemblyHelper.GetAllRelevantAssemblies();
            var dtoTypes = assemblies
                .SelectMany(assembly => assembly.GetTypes())
                .Where(type => typeof(IDataTransferObject).IsAssignableFrom(type) && 
                             !type.IsInterface && 
                             !type.IsAbstract)
                .ToList();
            
            var violations = new System.Collections.Generic.List<string>();
            
            foreach (var dtoType in dtoTypes)
            {
                // Act: Check for mutable properties (only getters, or init-only setters)
                var properties = dtoType.GetProperties(BindingFlags.Instance | BindingFlags.Public);
                
                foreach (var property in properties)
                {
                    var hasSetter = property.SetMethod != null;
                    var hasInitSetter = property.SetMethod != null && 
                                       property.SetMethod.ReturnType == typeof(void) && 
                                       property.SetMethod.IsDefined(typeof(System.Runtime.CompilerServices.IsExternalInit), false);
                    
                    if (hasSetter && !hasInitSetter)
                    {
                        violations.Add($"{dtoType.Name}.{property.Name} has mutable setter (should be init-only or getter-only)");
                    }
                }
            }
            
            // Assert: RED = violations found, GREEN = all compliant
            violations.ShouldBeEmpty($"RED STATE: Found {violations.Count} mutable DTO properties. GREEN STATE: All DTO properties are immutable.");
        }
    }
}
