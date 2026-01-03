using System.Linq;
using NetArchTest.Rules;
using Shouldly;
using Xunit;

namespace BarkMoon.GameComposition.Tests.Architectural
{
    /// <summary>
    /// Architectural tests enforcing aggregate root patterns for DDD compliance.
    /// Ensures proper domain modeling and prevents architectural violations.
    /// </summary>
    public class AggregateRootArchitectureTests
    {
        [Fact(DisplayName = "AGGREGATE-001: Domain Aggregates Should Have Single Root Per Bounded Context")]
        [Trait("Category", "Architectural")]
        public void Domain_Aggregates_Should_Have_Single_Root_Per_Bounded_Context()
        {
            // Arrange
            var assemblies = new[]
            {
                typeof(BarkMoon.GridPlacement.Core.State.Targeting.TargetingState2D).Assembly,
                typeof(BarkMoon.GameComposition.Core.Services.DI.ServiceRegistry).Assembly,
            };

            var violations = new List<string>();

            // Act & Assert - Check each assembly for aggregate root patterns
            foreach (var assembly in assemblies)
            {
                // Find potential aggregate root candidates (state classes with multiple entities)
                var aggregateCandidates = Types.InAssembly(assembly)
                    .That().ResideInNamespace("*.State.*")
                    .And().AreClasses()
                    .And().HaveNameEndingWith("State")
                    .And().AreSealed()
                    .GetTypes()
                    .ToList();

                // Verify each candidate follows aggregate root patterns
                foreach (var candidate in aggregateCandidates)
                {
                    // Check if candidate manages collections (aggregate behavior)
                    var hasCollections = candidate.GetFields()
                        .Any(f => f.FieldType.IsGenericType && 
                                 (f.FieldType.GetGenericTypeDefinition() == typeof(List<>) ||
                                  f.FieldType.GetGenericTypeDefinition() == typeof(IList<>) ||
                                  f.FieldType.GetGenericTypeDefinition() == typeof(IEnumerable<>)));

                    if (hasCollections)
                    {
                        // This is likely an aggregate root - verify proper patterns
                        var hasInvariantMethods = candidate.GetMethods()
                            .Any(m => m.Name.StartsWith("Validate") || 
                                     m.Name.StartsWith("Can") ||
                                     m.ReturnType.Name.Contains("Result"));

                        if (!hasInvariantMethods)
                        {
                            violations.Add($"{candidate.Name}: Aggregate root missing invariant validation methods");
                        }
                    }
                }
            }

            violations.ShouldBeEmpty("Aggregate roots should have proper invariant validation");
        }

        [Fact(DisplayName = "AGGREGATE-002: Entities Should Not Reference Other Aggregates Directly")]
        [Trait("Category", "Architectural")]
        public void Entities_Should_Not_Reference_Other_Aggregates_Directly()
        {
            // Arrange
            var assemblies = new[]
            {
                typeof(BarkMoon.GridPlacement.Core.State.Targeting.TargetingState2D).Assembly,
            };

            var violations = new List<string>();

            // Act & Assert - Check for cross-aggregate references
            foreach (var assembly in assemblies)
            {
                // Find all entity types in bounded contexts
                var entityTypes = Types.InAssembly(assembly)
                    .That().ResideInNamespace("*.State.*")
                    .Or().ResideInNamespace("*.Types.*")
                    .And().AreClasses()
                    .GetTypes()
                    .ToList();

                foreach (var entityType in entityTypes)
                {
                    // Check for direct references to other aggregate state classes
                    var fields = entityType.GetFields();
                    var properties = entityType.GetProperties();

                    var allMembers = fields.Cast<MemberInfo>().Concat(properties);
                    
                    var crossAggregateReferences = allMembers
                        .Where(m => m.MemberType == MemberTypes.Field || m.MemberType == MemberTypes.Property)
                        .Where(m => 
                        {
                            var memberType = m.MemberType == MemberTypes.Field 
                                ? ((FieldInfo)m).FieldType 
                                : ((PropertyInfo)m).PropertyType;
                            
                            // Check if reference crosses bounded contexts
                            return memberType.IsClass && 
                                   memberType.Namespace != null &&
                                   (memberType.Namespace.Contains("Targeting") && entityType.Namespace?.Contains("Manipulation") == true ||
                                    memberType.Namespace.Contains("Manipulation") && entityType.Namespace?.Contains("Targeting") == true ||
                                    memberType.Namespace.Contains("Placement") && entityType.Namespace?.Contains("Targeting") == true);
                        })
                        .Select(m => $"{entityType.Name}.{m.Name} references {(m.MemberType == MemberTypes.Field ? ((FieldInfo)m).FieldType.Name : ((PropertyInfo)m).PropertyType.Name)}");

                    violations.AddRange(crossAggregateReferences);
                }
            }

            violations.ShouldBeEmpty("Entities should not reference other aggregates directly");
        }

        [Fact(DisplayName = "AGGREGATE-003: Aggregate Roots Should Be Sealed")]
        [Trait("Category", "Architectural")]
        public void Aggregate_Roots_Should_Be_Sealed()
        {
            // Arrange
            var assemblies = new[]
            {
                typeof(BarkMoon.GridPlacement.Core.State.Targeting.TargetingState2D).Assembly,
            };

            var violations = new List<string>();

            // Act & Assert - Check aggregate root candidates are sealed
            foreach (var assembly in assemblies)
            {
                // Find potential aggregate roots (state classes with collections)
                var aggregateRoots = Types.InAssembly(assembly)
                    .That().ResideInNamespace("*.State.*")
                    .And().AreClasses()
                    .And().HaveNameEndingWith("State")
                    .GetTypes()
                    .Where(t => t.GetFields()
                        .Any(f => f.FieldType.IsGenericType && 
                                 (f.FieldType.GetGenericTypeDefinition() == typeof(List<>) ||
                                  f.FieldType.GetGenericTypeDefinition() == typeof(IList<>))))
                    .ToList();

                foreach (var aggregateRoot in aggregateRoots)
                {
                    if (!aggregateRoot.IsSealed)
                    {
                        violations.Add($"{aggregateRoot.FullName}: Aggregate root should be sealed");
                    }
                }
            }

            violations.ShouldBeEmpty("Aggregate roots should be sealed to prevent inheritance violations");
        }

        [Fact(DisplayName = "AGGREGATE-004: Aggregate Roots Should Have Invariant Validation")]
        [Trait("Category", "Architectural")]
        public void Aggregate_Roots_Should_Have_Invariant_Validation()
        {
            // Arrange
            var assemblies = new[]
            {
                typeof(BarkMoon.GridPlacement.Core.State.Targeting.TargetingState2D).Assembly,
            };

            var violations = new List<string>();

            // Act & Assert - Check aggregate roots have validation methods
            foreach (var assembly in assemblies)
            {
                // Find potential aggregate roots
                var aggregateRoots = Types.InAssembly(assembly)
                    .That().ResideInNamespace("*.State.*")
                    .And().AreClasses()
                    .And().HaveNameEndingWith("State")
                    .GetTypes()
                    .Where(t => t.GetFields()
                        .Any(f => f.FieldType.IsGenericType && 
                                 (f.FieldType.GetGenericTypeDefinition() == typeof(List<>) ||
                                  f.FieldType.GetGenericTypeDefinition() == typeof(IList<>))))
                    .ToList();

                foreach (var aggregateRoot in aggregateRoots)
                {
                    var methods = aggregateRoot.GetMethods();
                    
                    var hasValidationMethods = methods.Any(m => 
                        m.Name.StartsWith("Validate") || 
                        m.Name.StartsWith("Can") ||
                        m.Name.StartsWith("Is") ||
                        m.ReturnType.Name.Contains("Result") ||
                        m.ReturnType.Name.Contains("Validation"));

                    if (!hasValidationMethods)
                    {
                        violations.Add($"{aggregateRoot.FullName}: Aggregate root should have invariant validation methods");
                    }
                }
            }

            violations.ShouldBeEmpty("Aggregate roots should have invariant validation methods");
        }
    }
}
