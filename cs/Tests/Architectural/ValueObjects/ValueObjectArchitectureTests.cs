using System.Linq;
using NetArchTest.Rules;
using Shouldly;
using Xunit;
using BarkMoon.GameComposition.Tests.Common;

// Explicitly alias the NetArchTest Types class to avoid conflict
using ArchTypes = NetArchTest.Rules.Types;

namespace BarkMoon.GameComposition.Tests.Architectural
{
    /// <summary>
    /// Architectural tests enforcing value object patterns for DDD compliance.
    /// Ensures immutability, structural equality, and proper value object design.
    /// </summary>
    public class ValueObjectArchitectureTests
    {
        [Fact(DisplayName = "VALUE-001: Value Objects Should Be Immutable")]
        [Trait("Category", "Architectural")]
        public void Value_Objects_Should_Be_Immutable()
        {
            // Arrange - Use SSOT helper for assembly loading
            var assemblies = ArchitecturalTestHelpers.GetAllAssemblies()
                .Where(a => a.GetName().Name.Contains("GridPlacement") || 
                           a.GetName().Name.Contains("GameComposition"))
                .ToArray();

            var violations = new List<string>();

            // Act & Assert - Check value object candidates for immutability
            foreach (var assembly in assemblies)
            {
                // Find potential value objects (types in Types namespace, records, or small data classes)
                var valueObjectCandidates = Types.InAssembly(assembly)
                    .That().ResideInNamespace("*.Types.*")
                    .And().AreClasses()
                    .GetTypes()
                    .Where(t => 
                        // Records are naturally immutable
                        t.IsRecord ||
                        // Or classes that look like value objects (small, data-focused)
                        (t.GetFields().Length + t.GetProperties().Length <= 6 &&
                         t.Name.EndsWith("Data") || 
                         t.Name.EndsWith("Info") ||
                         t.Name.EndsWith("Config")))
                    .ToList();

                foreach (var candidate in valueObjectCandidates)
                {
                    // Skip if it's a record (already immutable)
                    if (candidate.IsRecord) continue;

                    // Check for mutable fields
                    var mutableFields = candidate.GetFields()
                        .Where(f => !f.IsInitOnly && !f.IsLiteral)
                        .Where(f => !f.FieldType.IsValueType || f.FieldType.IsGenericType)
                        .ToList();

                    if (mutableFields.Any())
                    {
                        violations.Add($"{candidate.FullName}: Has mutable fields: {string.Join(", ", mutableFields.Select(f => f.Name))}");
                    }

                    // Check for mutable properties
                    var mutableProperties = candidate.GetProperties()
                        .Where(p => p.CanWrite)
                        .Where(p => !p.PropertyType.IsValueType || p.PropertyType.IsGenericType)
                        .ToList();

                    if (mutableProperties.Any())
                    {
                        violations.Add($"{candidate.FullName}: Has mutable properties: {string.Join(", ", mutableProperties.Select(p => p.Name))}");
                    }
                }
            }

            violations.ShouldBeEmpty("Value objects should be immutable");
        }

        [Fact(DisplayName = "VALUE-002: Value Objects Should Have Structural Equality")]
        [Trait("Category", "Architectural")]
        public void Value_Objects_Should_Have_Structural_Equality()
        {
            // Arrange
            var assemblies = new[]
            {
                typeof(BarkMoon.GridPlacement.Core.Types.GridDimensions).Assembly,
                typeof(BarkMoon.GameComposition.Core.Types.Vector2).Assembly,
            };

            var violations = new List<string>();

            // Act & Assert - Check value objects have proper equality implementation
            foreach (var assembly in assemblies)
            {
                // Find potential value objects
                var valueObjectCandidates = Types.InAssembly(assembly)
                    .That().ResideInNamespace("*.Types.*")
                    .And().AreClasses()
                    .GetTypes()
                    .Where(t => 
                        t.IsRecord ||
                        (t.GetFields().Length + t.GetProperties().Length <= 6 &&
                         (t.Name.EndsWith("Data") || t.Name.EndsWith("Info") || t.Name.EndsWith("Config"))))
                    .ToList();

                foreach (var candidate in valueObjectCandidates)
                {
                    // Records automatically have structural equality
                    if (candidate.IsRecord) continue;

                    // Check if class overrides Equals and GetHashCode
                    var equalsMethod = candidate.GetMethod("Equals", new[] { typeof(object) });
                    var getHashCodeMethod = candidate.GetMethod("GetHashCode");

                    if (equalsMethod == null || equalsMethod.DeclaringType == typeof(object))
                    {
                        violations.Add($"{candidate.FullName}: Should override Equals for structural equality");
                    }

                    if (getHashCodeMethod == null || getHashCodeMethod.DeclaringType == typeof(object))
                    {
                        violations.Add($"{candidate.FullName}: Should override GetHashCode for structural equality");
                    }

                    // Check if implements IEquatable<T>
                    var equatableInterface = candidate.GetInterfaces()
                        .FirstOrDefault(i => i.IsGenericType && i.GetGenericTypeDefinition() == typeof(IEquatable<>));

                    if (equatableInterface == null)
                    {
                        violations.Add($"{candidate.FullName}: Should implement IEquatable<{candidate.Name}> for type-safe equality");
                    }
                }
            }

            violations.ShouldBeEmpty("Value objects should have structural equality");
        }

        [Fact(DisplayName = "VALUE-003: Value Objects Should Not Have Identity")]
        [Trait("Category", "Architectural")]
        public void Value_Objects_Should_Not_Have_Identity()
        {
            // Arrange
            var assemblies = new[]
            {
                typeof(BarkMoon.GridPlacement.Core.Types.GridDimensions).Assembly,
                typeof(BarkMoon.GameComposition.Core.Types.Vector2).Assembly,
            };

            var violations = new List<string>();

            // Act & Assert - Check value objects don't have identity fields
            foreach (var assembly in assemblies)
            {
                // Find potential value objects
                var valueObjectCandidates = Types.InAssembly(assembly)
                    .That().ResideInNamespace("*.Types.*")
                    .And().AreClasses()
                    .GetTypes()
                    .Where(t => 
                        t.IsRecord ||
                        (t.GetFields().Length + t.GetProperties().Length <= 6 &&
                         (t.Name.EndsWith("Data") || t.Name.EndsWith("Info") || t.Name.EndsWith("Config"))))
                    .ToList();

                foreach (var candidate in valueObjectCandidates)
                {
                    // Check for identity-related field names
                    var identityFields = candidate.GetFields()
                        .Where(f => f.Name.Contains("Id", StringComparison.OrdinalIgnoreCase))
                        .Where(f => f.FieldType == typeof(string) || f.FieldType == typeof(Guid))
                        .ToList();

                    if (identityFields.Any())
                    {
                        violations.Add($"{candidate.FullName}: Should not have identity fields: {string.Join(", ", identityFields.Select(f => f.Name))}");
                    }

                    // Check for identity-related properties
                    var identityProperties = candidate.GetProperties()
                        .Where(p => p.Name.Contains("Id", StringComparison.OrdinalIgnoreCase))
                        .Where(p => p.PropertyType == typeof(string) || p.PropertyType == typeof(Guid))
                        .ToList();

                    if (identityProperties.Any())
                    {
                        violations.Add($"{candidate.FullName}: Should not have identity properties: {string.Join(", ", identityProperties.Select(p => p.Name))}");
                    }
                }
            }

            violations.ShouldBeEmpty("Value objects should not have identity");
        }

        [Fact(DisplayName = "VALUE-004: Value Objects Should Be Small and Focused")]
        [Trait("Category", "Architectural")]
        public void Value_Objects_Should_Be_Small_and_Focused()
        {
            // Arrange
            var assemblies = new[]
            {
                typeof(BarkMoon.GridPlacement.Core.Types.GridDimensions).Assembly,
                typeof(BarkMoon.GameComposition.Core.Types.Vector2).Assembly,
            };

            var violations = new List<string>();

            // Act & Assert - Check value objects are appropriately sized
            foreach (var assembly in assemblies)
            {
                // Find potential value objects
                var valueObjectCandidates = Types.InAssembly(assembly)
                    .That().ResideInNamespace("*.Types.*")
                    .And().AreClasses()
                    .GetTypes()
                    .Where(t => 
                        t.IsRecord ||
                        (t.Name.EndsWith("Data") || t.Name.EndsWith("Info") || t.Name.EndsWith("Config")))
                    .ToList();

                foreach (var candidate in valueObjectCandidates)
                {
                    var fieldCount = candidate.GetFields().Length;
                    var propertyCount = candidate.GetProperties().Length;
                    var totalMembers = fieldCount + propertyCount;

                    // Value objects should be small (typically < 10 members)
                    if (totalMembers > 10)
                    {
                        violations.Add($"{candidate.FullName}: Has {totalMembers} members, value objects should be small and focused (< 10)");
                    }

                    // Check for complex nested objects (might indicate entity rather than value object)
                    var complexFields = candidate.GetFields()
                        .Where(f => f.FieldType.IsClass && 
                                 f.FieldType != typeof(string) && 
                                 !f.FieldType.IsValueType &&
                                 !f.FieldType.Namespace?.Contains("Types") == true)
                        .ToList();

                    if (complexFields.Any())
                    {
                        violations.Add($"{candidate.FullName}: Contains complex object references, might be an entity instead of value object: {string.Join(", ", complexFields.Select(f => $"{f.Name}:{f.FieldType.Name}"))}");
                    }
                }
            }

            violations.ShouldBeEmpty("Value objects should be small and focused");
        }
    }
}
