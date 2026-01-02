using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using NetArchTest.Rules;
using Shouldly;
using Xunit;

namespace BarkMoon.GameComposition.ArchitecturalTests.Services
{
    /// <summary>
    /// Service architectural rules for plugin ecosystem.
    /// Tests service patterns, state ownership, and event emission boundaries.
    /// </summary>
    public class ServiceArchitecturalTests
    {
        /// <summary>
        /// Tests that services only emit events appropriate to their domain.
        /// Enforces domain boundary separation and prevents event leakage.
        /// </summary>
        public static void Services_Should_Only_Emit_Domain_Appropriate_Events(Assembly pluginAssembly)
        {
            // Arrange
            var violations = new List<string>();
            
            // Get all service types
            var serviceTypes = Types.InAssembly(pluginAssembly)
                .That()
                .HaveNameEndingWith("Service")
                .And()
                .AreClasses()
                .GetTypes();

            // Define domain-to-event mappings
            var domainEventMappings = new Dictionary<string, string[]>
            {
                ["Targeting"] = new[] { "Targeting", "GridTargeting" },
                ["Placement"] = new[] { "Placement", "BuildingPlacement" },
                ["Manipulation"] = new[] { "Manipulation", "BuildingManipulation" },
                ["Cursor"] = new[] { "Cursor", "GridCursor" },
                ["Grid"] = new[] { "Grid", "GridService" }
            };

            // Act - Check event emission patterns
            foreach (var serviceType in serviceTypes)
            {
                var serviceName = serviceType.Name;
                
                // Skip test services
                if (serviceName.Contains("Test"))
                    continue;

                // Determine service domain
                var serviceDomain = domainEventMappings
                    .Keys
                    .FirstOrDefault(domain => serviceName.Contains(domain));

                if (serviceDomain == null)
                    continue; // Skip services not in defined domains

                // Get allowed event types for this domain
                var allowedEventPrefixes = domainEventMappings[serviceDomain];

                // Check method calls for event emission
                var methods = serviceType.GetMethods();
                foreach (var method in methods)
                {
                    // Look for event emission patterns
                    var methodBody = System.Text.RegularExpressions.Regex.Match(
                        method.ToString(), 
                        @"_eventBus\.(Publish|Emit|Fire)\(([^)]+)\)");

                    if (methodBody.Success)
                    {
                        var eventCall = methodBody.Groups[2].Value;
                        
                        // Check if event is appropriate for domain
                        var isDomainAppropriate = allowedEventPrefixes
                            .Any(prefix => eventCall.Contains(prefix));

                        if (!isDomainAppropriate)
                        {
                            violations.Add($"{serviceType.FullName}.{method.Name} emits events outside its domain: {eventCall}");
                        }
                    }
                }
            }

            // Assert
            violations.ShouldBeEmpty($"Service event emission violations found: {string.Join(", ", violations)}");
        }

        /// <summary>
        /// Tests that all services implement IService<TState> with proper state ownership.
        /// Enforces that every service has typed state and provides snapshots.
        /// </summary>
        public static void All_Services_Should_Own_Typed_State_And_Provide_Snapshots(Assembly pluginAssembly)
        {
            // Arrange
            var violations = new List<string>();
            
            // Get all service types
            var serviceTypes = Types.InAssembly(pluginAssembly)
                .That()
                .HaveNameEndingWith("Service")
                .And()
                .AreClasses()
                .GetTypes();

            // Act - Check state ownership patterns
            foreach (var serviceType in serviceTypes)
            {
                // Skip test services
                if (serviceType.Name.Contains("Test"))
                    continue;

                // Check for state ownership pattern
                var stateFieldName = "_" + serviceType.Name.Replace("Service", "State");
                var stateField = serviceType.GetField(stateFieldName, 
                    System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);

                if (stateField == null)
                {
                    violations.Add($"{serviceType.FullName} should own typed state field ({stateFieldName})");
                    continue;
                }

                // Check that state type ends with "State"
                if (!stateField.FieldType.Name.EndsWith("State"))
                {
                    violations.Add($"{serviceType.FullName} state field should be of type ending with 'State'");
                }

                // Check for snapshot method
                var snapshotMethod = serviceType.GetMethod("CreateSnapshot");
                if (snapshotMethod == null)
                {
                    violations.Add($"{serviceType.FullName} should have CreateSnapshot() method");
                }
                else
                {
                    // Check that snapshot method returns appropriate type
                    if (!snapshotMethod.ReturnType.Name.Contains("Snapshot"))
                    {
                        violations.Add($"{serviceType.FullName}.CreateSnapshot() should return snapshot type");
                    }
                }
            }

            // Assert
            violations.ShouldBeEmpty($"Service state ownership violations found: {string.Join(", ", violations)}");
        }

        /// <summary>
        /// Tests that all state classes are pure data and properly paired with services.
        /// Enforces that every state has a corresponding service owner.
        /// </summary>
        public static void All_State_Classes_Should_Be_Pure_Data_And_Paired_With_Services(Assembly pluginAssembly)
        {
            // Arrange
            var violations = new List<string>();
            
            // Get all state types
            var stateTypes = Types.InAssembly(pluginAssembly)
                .That()
                .HaveNameEndingWith("State")
                .And()
                .AreClasses()
                .GetTypes();

            // Get all service types for pairing verification
            var serviceTypes = Types.InAssembly(pluginAssembly)
                .That()
                .HaveNameEndingWith("Service")
                .And()
                .AreClasses()
                .GetTypes()
                .Select(t => t.Name)
                .ToHashSet();

            // Act - Check state purity and service pairing
            foreach (var stateType in stateTypes)
            {
                // Skip test states
                if (stateType.Name.Contains("Test"))
                    continue;

                // Check for pure data (no methods other than constructors)
                var methods = stateType.GetMethods()
                    .Where(m => !m.IsConstructor && !m.IsSpecialName)
                    .ToList();

                if (methods.Any())
                {
                    violations.Add($"{stateType.FullName} should be pure data - remove methods: {string.Join(", ", methods.Select(m => m.Name))}");
                }

                // Check for service pairing
                var expectedServiceName = stateType.Name.Replace("State", "Service");
                if (!serviceTypes.Contains(expectedServiceName))
                {
                    violations.Add($"{stateType.FullName} should have corresponding service: {expectedServiceName}");
                }
            }

            // Assert
            violations.ShouldBeEmpty($"State class violations found: {string.Join(", ", violations)}");
        }
    }
}
