using System.Linq;
using NetArchTest.Rules;
using Shouldly;
using Xunit;

namespace BarkMoon.GameComposition.Tests.Architectural
{
    /// <summary>
    /// Architectural tests enforcing domain event patterns for DDD compliance.
    /// Ensures immutability, proper event structure, and event sourcing patterns.
    /// </summary>
    public class DomainEventArchitectureTests
    {
        [Fact(DisplayName = "EVENT-001: Domain Events Should Be Immutable")]
        [Trait("Category", "Architectural")]
        public void Domain_Events_Should_Be_Immutable()
        {
            // Arrange
            var assemblies = new[]
            {
                typeof(BarkMoon.GridPlacement.Core.Events.GridEvents).Assembly,
                typeof(BarkMoon.GameComposition.Core.Events.ServiceEvent).Assembly,
            };

            var violations = new List<string>();

            // Act & Assert - Check domain events for immutability
            foreach (var assembly in assemblies)
            {
                // Find domain event candidates
                var domainEvents = Types.InAssembly(assembly)
                    .That().ResideInNamespace("*.Events.*")
                    .And().AreClasses()
                    .And().HaveNameEndingWith("Event")
                    .GetTypes()
                    .ToList();

                foreach (var eventType in domainEvents)
                {
                    // Skip base event classes
                    if (eventType.Name == "ServiceEvent" || eventType.IsAbstract) continue;

                    // Check for mutable fields
                    var mutableFields = eventType.GetFields()
                        .Where(f => !f.IsInitOnly && !f.IsLiteral)
                        .ToList();

                    if (mutableFields.Any())
                    {
                        violations.Add($"{eventType.FullName}: Has mutable fields: {string.Join(", ", mutableFields.Select(f => f.Name))}");
                    }

                    // Check for mutable properties (only setters, no init-only)
                    var mutableProperties = eventType.GetProperties()
                        .Where(p => p.CanWrite && p.SetMethod?.IsPublic == true)
                        .Where(p => p.SetMethod?.IsInitOnly != true)
                        .ToList();

                    if (mutableProperties.Any())
                    {
                        violations.Add($"{eventType.FullName}: Has mutable properties: {string.Join(", ", mutableProperties.Select(p => p.Name))}");
                    }
                }
            }

            violations.ShouldBeEmpty("Domain events should be immutable");
        }

        [Fact(DisplayName = "EVENT-002: Domain Events Should Have Timestamp")]
        [Trait("Category", "Architectural")]
        public void Domain_Events_Should_Have_Timestamp()
        {
            // Arrange
            var assemblies = new[]
            {
                typeof(BarkMoon.GridPlacement.Core.Events.GridEvents).Assembly,
                typeof(BarkMoon.GameComposition.Core.Events.ServiceEvent).Assembly,
            };

            var violations = new List<string>();

            // Act & Assert - Check domain events have timestamp
            foreach (var assembly in assemblies)
            {
                // Find domain event candidates
                var domainEvents = Types.InAssembly(assembly)
                    .That().ResideInNamespace("*.Events.*")
                    .And().AreClasses()
                    .And().HaveNameEndingWith("Event")
                    .GetTypes()
                    .Where(t => t.Name != "ServiceEvent" && !t.IsAbstract)
                    .ToList();

                foreach (var eventType in domainEvents)
                {
                    // Check for timestamp property
                    var timestampProperty = eventType.GetProperties()
                        .FirstOrDefault(p => 
                            p.Name.Contains("Timestamp", StringComparison.OrdinalIgnoreCase) ||
                            p.Name.Contains("Time", StringComparison.OrdinalIgnoreCase) ||
                            p.Name.Contains("Occurred", StringComparison.OrdinalIgnoreCase));

                    if (timestampProperty == null)
                    {
                        violations.Add($"{eventType.FullName}: Should have timestamp property");
                    }
                    else if (timestampProperty.PropertyType != typeof(DateTime) && 
                             timestampProperty.PropertyType != typeof(DateTimeOffset))
                    {
                        violations.Add($"{eventType.FullName}: Timestamp should be DateTime or DateTimeOffset, not {timestampProperty.PropertyType.Name}");
                    }
                }
            }

            violations.ShouldBeEmpty("Domain events should have timestamp");
        }

        [Fact(DisplayName = "EVENT-003: Domain Events Should Have Source Information")]
        [Trait("Category", "Architectural")]
        public void Domain_Events_Should_Have_Source_Information()
        {
            // Arrange
            var assemblies = new[]
            {
                typeof(BarkMoon.GridPlacement.Core.Events.GridEvents).Assembly,
                typeof(BarkMoon.GameComposition.Core.Events.ServiceEvent).Assembly,
            };

            var violations = new List<string>();

            // Act & Assert - Check domain events have source information
            foreach (var assembly in assemblies)
            {
                // Find domain event candidates
                var domainEvents = Types.InAssembly(assembly)
                    .That().ResideInNamespace("*.Events.*")
                    .And().AreClasses()
                    .And().HaveNameEndingWith("Event")
                    .GetTypes()
                    .Where(t => t.Name != "ServiceEvent" && !t.IsAbstract)
                    .ToList();

                foreach (var eventType in domainEvents)
                {
                    // Check for source-related properties
                    var sourceProperties = eventType.GetProperties()
                        .Where(p => 
                            p.Name.Contains("Source", StringComparison.OrdinalIgnoreCase) ||
                            p.Name.Contains("UserId", StringComparison.OrdinalIgnoreCase) ||
                            p.Name.Contains("AggregateId", StringComparison.OrdinalIgnoreCase) ||
                            p.Name.Contains("EntityId", StringComparison.OrdinalIgnoreCase))
                        .ToList();

                    if (!sourceProperties.Any())
                    {
                        violations.Add($"{eventType.FullName}: Should have source information (Source, UserId, or AggregateId)");
                    }
                }
            }

            violations.ShouldBeEmpty("Domain events should have source information");
        }

        [Fact(DisplayName = "EVENT-004: Domain Events Should Not Have Complex Behavior")]
        [Trait("Category", "Architectural")]
        public void Domain_Events_Should_Not_Have_Complex_Behavior()
        {
            // Arrange
            var assemblies = new[]
            {
                typeof(BarkMoon.GridPlacement.Core.Events.GridEvents).Assembly,
                typeof(BarkMoon.GameComposition.Core.Events.ServiceEvent).Assembly,
            };

            var violations = new List<string>();

            // Act & Assert - Check domain events don't have complex behavior
            foreach (var assembly in assemblies)
            {
                // Find domain event candidates
                var domainEvents = Types.InAssembly(assembly)
                    .That().ResideInNamespace("*.Events.*")
                    .And().AreClasses()
                    .And().HaveNameEndingWith("Event")
                    .GetTypes()
                    .Where(t => t.Name != "ServiceEvent" && !t.IsAbstract)
                    .ToList();

                foreach (var eventType in domainEvents)
                {
                    // Check for complex methods (beyond simple constructors)
                    var methods = eventType.GetMethods()
                        .Where(m => !m.IsConstructor && 
                                   !m.IsSpecialName && 
                                   m.IsPublic &&
                                   m.Name != "ToString" &&
                                   m.Name != "GetHashCode" &&
                                   m.Name != "Equals")
                        .ToList();

                    if (methods.Any())
                    {
                        violations.Add($"{eventType.FullName}: Should not have complex behavior methods: {string.Join(", ", methods.Select(m => m.Name))}");
                    }
                }
            }

            violations.ShouldBeEmpty("Domain events should not have complex behavior");
        }

        [Fact(DisplayName = "EVENT-005: Domain Events Should Have Proper Naming")]
        [Trait("Category", "Architectural")]
        public void Domain_Events_Should_Have_Proper_Naming()
        {
            // Arrange
            var assemblies = new[]
            {
                typeof(BarkMoon.GridPlacement.Core.Events.GridEvents).Assembly,
                typeof(BarkMoon.GameComposition.Core.Events.ServiceEvent).Assembly,
            };

            var violations = new List<string>();

            // Act & Assert - Check domain events have proper naming
            foreach (var assembly in assemblies)
            {
                // Find domain event candidates
                var domainEvents = Types.InAssembly(assembly)
                    .That().ResideInNamespace("*.Events.*")
                    .And().AreClasses()
                    .And().HaveNameEndingWith("Event")
                    .GetTypes()
                    .Where(t => t.Name != "ServiceEvent" && !t.IsAbstract)
                    .ToList();

                foreach (var eventType in domainEvents)
                {
                    var eventName = eventType.Name;

                    // Check for past tense naming (events should describe something that happened)
                    if (!eventName.StartsWith("Completed") && 
                        !eventName.StartsWith("Placed") && 
                        !eventName.StartsWith("Moved") && 
                        !eventName.StartsWith("Removed") && 
                        !eventName.StartsWith("Changed") && 
                        !eventName.StartsWith("Created") && 
                        !eventName.StartsWith("Deleted") && 
                        !eventName.StartsWith("Selected") && 
                        !eventName.StartsWith("Deselected") &&
                        !eventName.StartsWith("Rotated") &&
                        !eventName.StartsWith("Validated"))
                    {
                        // Allow some common event patterns but flag suspicious ones
                        if (!eventName.Contains("Input") && 
                            !eventName.Contains("Request") &&
                            !eventName.Contains("Service"))
                        {
                            violations.Add($"{eventType.FullName}: Event name should be in past tense (e.g., 'Placed', 'Moved', 'Completed')");
                        }
                    }
                }
            }

            violations.ShouldBeEmpty("Domain events should have proper past-tense naming");
        }
    }
}
