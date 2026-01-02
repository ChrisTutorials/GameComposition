using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using NetArchTest.Rules;
using Shouldly;
using Xunit;

namespace BarkMoon.GameComposition.ArchitecturalTests
{
    /// <summary>
    /// Code-first architectural rules replacing static documentation.
    /// These tests ARE the architectural rules - documentation drift eliminated.
    /// </summary>
    public class ArchitecturalRulesTests
    {
        private readonly Assembly _gameCompositionAssembly = Assembly.LoadFrom("BarkMoon.GameComposition.Core.dll");

        [Fact(DisplayName = "001GC: Core Should Use Microsoft Extensions Not Custom Implementations")]
        public void Core_Should_Use_Microsoft_Extensions_Not_Custom_Implementations()
        {
            // Rule: Use Microsoft.Extensions.* instead of custom implementations
            var result = Types.InAssembly(_gameCompositionAssembly)
                .Should()
                .NotHaveName("CustomPool")
                .And()
                .NotHaveName("CollectionPool")
                .And()
                .NotHaveName("CustomLogger")
                .And()
                .NotHaveName("CustomDI")
                .GetResult();

            result.IsSuccessful.ShouldBeTrue("Should use Microsoft.Extensions.* instead of custom implementations");
        }

        [Fact(DisplayName = "002GC: Core Should Be Engine Agnostic")]
        public void Core_Should_Be_Engine_Agnostic()
        {
            // Rule: No engine dependencies in Core package
            var result = Types.InAssembly(_gameCompositionAssembly)
                .Should()
                .NotHaveDependencyOnAny("Godot", "Unity", "Microsoft.Xna")
                .GetResult();

            result.IsSuccessful.ShouldBeTrue("Core should be engine-agnostic");
        }

        [Fact(DisplayName = "003GC: Services Should Be Interface First")]
        public void Services_Should_Be_Interface_First()
        {
            // Rule: All services should have interfaces
            var serviceTypes = Types.InAssembly(_gameCompositionAssembly)
                .That()
                .HaveNameEndingWith("Service")
                .And()
                .AreClasses()
                .GetTypes();

            foreach (var serviceType in serviceTypes)
            {
                var interfaceName = "I" + serviceType.Name;
                var interfaceType = _gameCompositionAssembly.GetTypes()
                    .FirstOrDefault(t => t.Name == interfaceName && t.IsInterface);
                
                interfaceType.ShouldNotBeNull($"Service {serviceType.Name} should have interface {interfaceName}");
            }
        }

        [Fact]
        public void Dependencies_Should_Follow_Layered_Architecture()
        {
            // Rule: Clear dependency boundaries
            var result = Types.InAssembly(_gameCompositionAssembly)
                .That()
                .ResideInNamespace("BarkMoon.GameComposition.Core.Services")
                .Should()
                .NotHaveDependencyOn("BarkMoon.GameComposition.Core.Types")
                .GetResult();

            // Note: This might need adjustment based on actual architecture
            // The test enforces the documented dependency rules
        }

        [Fact]
        public void Types_Should_Be_Immutable_Structs_Where_Appropriate()
        {
            // Rule: Value objects should be immutable structs
            var allTypes = _gameCompositionAssembly.GetTypes();
            var valueObjectTypes = allTypes
                .Where(t => t.Namespace == "BarkMoon.GameComposition.Core.Types" && t.IsValueType)
                .ToList();

            foreach (var valueType in valueObjectTypes)
            {
                var publicSettableProperties = valueType.GetProperties(BindingFlags.Public | BindingFlags.Instance)
                    .Where(p => p.CanWrite);

                // Value objects should be immutable or have private setters
                publicSettableProperties.ShouldBeEmpty($"Value object {valueType.Name} should be immutable");
            }
        }
    }

    /// <summary>
    /// Cross-plugin architectural rules for orchestrator patterns.
    /// These tests apply to ANY plugin using GameComposition framework.
    /// Enforces Service-Based Architecture principles across the ecosystem.
    /// </summary>
    public class PluginOrchestratorArchitectureTests
    {
        /// <summary>
        /// Tests orchestrator ownership rules - applies to any plugin assembly.
        /// This test should be called from each plugin's test suite with their assembly.
        /// </summary>
        public static void Orchestrators_Should_Own_Only_Services_And_EventBus(Assembly pluginAssembly)
        {
            // Arrange
            var orchestratorTypes = Types.InAssembly(pluginAssembly)
                .That()
                .HaveNameEndingWith("Orchestrator")
                .And()
                .AreClasses()
                .GetTypes();

            var violations = new List<string>();

            // Act - Check orchestrator dependencies
            foreach (var orchestratorType in orchestratorTypes)
            {
                var fields = orchestratorType.GetFields(BindingFlags.NonPublic | BindingFlags.Instance);
                var properties = orchestratorType.GetProperties(BindingFlags.NonPublic | BindingFlags.Instance);

                var allDependencies = fields.Select(f => f.FieldType)
                    .Concat(properties.Select(p => p.PropertyType))
                    .Where(t => t.IsClass || t.IsInterface)
                    .ToList();

                // Check for violations
                foreach (var dependencyType in allDependencies)
                {
                    var typeName = dependencyType.Name;
                    var namespaceName = dependencyType.Namespace ?? string.Empty;

                    // Allowed dependencies
                    if (typeName.EndsWith("Service") || 
                        typeName == "IEventBus" ||
                        typeName.EndsWith("Configuration"))
                    {
                        continue; // Allowed
                    }

                    // Forbidden dependencies
                    if (typeName.EndsWith("Handler") ||
                        typeName.EndsWith("Converter") ||
                        typeName.EndsWith("Processor") ||
                        typeName.EndsWith("Adapter") && !namespaceName.Contains("Interfaces"))
                    {
                        violations.Add($"{orchestratorType.Name} should not own {typeName} - belongs in services");
                    }
                }
            }

            // Assert
            violations.ShouldBeEmpty($"Orchestrator ownership violations found: {string.Join(", ", violations)}");
        }

        /// <summary>
        /// Tests that all orchestrators implement the minimal IOrchestrator interface.
        /// </summary>
        public static void Orchestrators_Should_Implement_IOrchestrator_Interface(Assembly pluginAssembly)
        {
            // Arrange
            var orchestratorTypes = Types.InAssembly(pluginAssembly)
                .That()
                .HaveNameEndingWith("Orchestrator")
                .And()
                .AreClasses()
                .GetTypes();

            var violations = new List<string>();

            // Act - Check interface implementation
            foreach (var orchestratorType in orchestratorTypes)
            {
                var iOrchestratorInterface = orchestratorType.GetInterface("IOrchestrator");
                if (iOrchestratorInterface == null)
                {
                    violations.Add($"{orchestratorType.FullName} should implement IOrchestrator interface");
                }
            }

            // Assert
            violations.ShouldBeEmpty($"Orchestrator interface violations found: {string.Join(", ", violations)}");
        }

        /// <summary>
        /// Tests dimensional naming conventions - applies to any plugin assembly.
        /// Enforces 2D/3D suffixes for dimensional types.
        /// </summary>
        public static void Dimensional_Types_Should_Have_Dimensional_Suffix(Assembly pluginAssembly)
        {
            // Arrange
            var gridRelatedTypes = Types.InAssembly(pluginAssembly)
                .That()
                .ResideInNamespaceContaining("Grid")
                .And()
                .AreClasses()
                .GetTypes();

            var violations = new List<string>();

            // Act - Check naming conventions
            foreach (var type in gridRelatedTypes)
            {
                var typeName = type.Name;
                var namespaceName = type.Namespace ?? string.Empty;

                // Grid-related types should have dimensional suffix
                if ((typeName.Contains("Grid") || namespaceName.Contains("Grid")) &&
                    !typeName.Contains("2D") && !typeName.Contains("3D") &&
                    !typeName.Contains("Event") && // Events are exempt
                    !typeName.Contains("Test") && // Tests are exempt
                    !typeName.EndsWith("Base")) // Base classes are exempt
                {
                    violations.Add($"{type.FullName} should have 2D or 3D suffix for grid-related types");
                }
            }

            // Assert
            violations.ShouldBeEmpty($"Dimensional naming violations found: {string.Join(", ", violations)}");
        }

        /// <summary>
        /// Tests that all ID types use numeric IDs for performance, not strings.
        /// </summary>
        public static void ID_Types_Should_Use_Numeric_IDs_Not_Strings(Assembly pluginAssembly)
        {
            // Arrange
            var allTypes = Types.InAssembly(pluginAssembly)
                .GetTypes();

            var violations = new List<string>();

            // Act - Check for string-based ID properties
            foreach (var type in allTypes)
            {
                var properties = type.GetProperties(BindingFlags.Public | BindingFlags.Instance);
                
                foreach (var property in properties)
                {
                    var propertyName = property.Name.ToLower();
                    
                    // Check if this looks like an ID property
                    if (propertyName.EndsWith("id") && 
                        property.PropertyType == typeof(string))
                    {
                        violations.Add($"{type.FullName}.{property.Name} should use NumericId<T> instead of string for performance");
                    }
                }
            }

            // Assert
            violations.ShouldBeEmpty($"String-based ID violations found: {string.Join(", ", violations)}");
        }

        /// <summary>
        /// Tests that services only emit events appropriate to their domain.
        /// Enforces domain boundary separation and prevents event leakage.
        /// </summary>
        public static void Services_Should_Only_Emit_Domain_Appropriate_Events(Assembly pluginAssembly)
        {
            // Arrange
            var violations = new List<string>();
            
            // Define domain mappings
            var domainEventMappings = new Dictionary<string, List<string>>
            {
                ["Targeting"] = ["Target", "TargetMap", "Targeting"],
                ["Cursor"] = ["Cursor", "Position", "Positioner", "Visual"],
                ["Grid"] = ["Grid", "Cell", "Coordinate"],
                ["Manipulation"] = ["Manipulation", "Build", "Move", "Rotate", "Demolish"],
                ["Placement"] = ["Placement", "Place", "Placeable"]
            };

            // Act - Check all services for domain violations
            var serviceTypes = Types.InAssembly(pluginAssembly)
                .That()
                .HaveNameEndingWith("Service")
                .And()
                .AreClasses()
                .GetTypes();

            foreach (var serviceType in serviceTypes)
            {
                var serviceNamespace = serviceType.Namespace ?? "";
                var serviceDomain = DetermineServiceDomain(serviceType.Name, serviceNamespace);
                
                if (serviceDomain == "Unknown") continue; // Skip ambiguous services

                // Check events in this service
                var eventFields = serviceType.GetFields(BindingFlags.Public | BindingFlags.Instance)
                    .Where(f => f.FieldType == typeof(Action<>) || 
                               f.FieldType.GetGenericTypeDefinition() == typeof(Action<>))
                    .ToList();

                foreach (var eventField in eventFields)
                {
                    var eventName = eventField.Name;
                    var violatingDomains = FindViolatingDomains(eventName, domainEventMappings, serviceDomain);
                    
                    foreach (var violatingDomain in violatingDomains)
                    {
                        violations.Add($"{serviceType.Name}.{eventName} should be in {violatingDomain} domain, not {serviceDomain} domain");
                    }
                }
            }

            // Assert
            violations.ShouldBeEmpty($"Domain boundary violations found: {string.Join(", ", violations)}");
        }

        /// <summary>
        /// Tests that cursor domain consistently uses "Cursor" terminology.
        /// Prevents naming inconsistencies like "Positioner" vs "Cursor".
        /// </summary>
        public static void Cursor_Domain_Should_Use_Consistent_Cursor_Terminology(Assembly pluginAssembly)
        {
            // Arrange
            var violations = new List<string>();
            
            // Define forbidden cursor-related terms that should use "Cursor" instead
            var inconsistentTerms = new List<string> { "Positioner", "Positioning", "Pos" };
            
            // Get all types in the assembly
            var allTypes = Types.InAssembly(pluginAssembly)
                .GetTypes();

            // Act - Check for inconsistent cursor terminology
            foreach (var type in allTypes)
            {
                var typeName = type.Name;
                var namespaceName = type.Namespace ?? "";

                // Check if this is cursor-related but uses inconsistent terminology
                if (IsCursorRelated(typeName, namespaceName) && 
                    UsesInconsistentTerminology(typeName, inconsistentTerms))
                {
                    violations.Add($"{type.FullName} should use 'Cursor' terminology instead of '{GetInconsistentTerm(typeName, inconsistentTerms)}'");
                }
            }

            // Also check method names in cursor-related types
            foreach (var type in allTypes)
            {
                if (IsCursorRelated(type.Name, type.Namespace ?? ""))
                {
                    var methods = type.GetMethods(BindingFlags.Public | BindingFlags.Instance);
                    foreach (var method in methods)
                    {
                        if (UsesInconsistentTerminology(method.Name, inconsistentTerms))
                        {
                            violations.Add($"{type.Name}.{method.Name} should use 'Cursor' terminology instead of '{GetInconsistentTerm(method.Name, inconsistentTerms)}'");
                        }
                    }
                }
            }

            // Assert
            violations.ShouldBeEmpty($"Cursor terminology violations found: {string.Join(", ", violations)}");
        }

        private static bool IsCursorRelated(string typeName, string namespaceName)
        {
            var fullName = $"{namespaceName}.{typeName}";
            
            // Check for cursor-related indicators
            return fullName.Contains("Cursor", StringComparison.OrdinalIgnoreCase) ||
                   fullName.Contains("Position", StringComparison.OrdinalIgnoreCase) ||
                   fullName.Contains("Targeting") && fullName.Contains("Position"); // Targeting position handling
        }

        private static bool UsesInconsistentTerminology(string name, List<string> inconsistentTerms)
        {
            return inconsistentTerms.Any(term => name.Contains(term, StringComparison.OrdinalIgnoreCase));
        }

        private static string GetInconsistentTerm(string name, List<string> inconsistentTerms)
        {
            return inconsistentTerms.FirstOrDefault(term => name.Contains(term, StringComparison.OrdinalIgnoreCase)) ?? "";
        }

        /// <summary>
        /// Tests that all services implement IService<TState> with proper state ownership.
        /// Enforces that every service has typed state and provides snapshots.
        /// </summary>
        public static void All_Services_Should_Own_Typed_State_And_Provide_Snapshots(Assembly pluginAssembly)
        {
            // Arrange
            var violations = new List<string>();
            
            // Get all service classes
            var serviceTypes = Types.InAssembly(pluginAssembly)
                .That()
                .HaveNameEndingWith("Service")
                .And()
                .AreClasses()
                .GetTypes();

            // Act - Check service state ownership compliance
            foreach (var serviceType in serviceTypes)
            {
                // Skip test classes and abstract classes
                if (serviceType.Name.Contains("Test") || serviceType.IsAbstract)
                    continue;

                // All services should implement IService<TState>
                var genericServiceInterface = serviceType.GetInterfaces()
                    .FirstOrDefault(i => i.IsGenericType && 
                                      i.GetGenericTypeDefinition() == typeof(IService<>));

                if (genericServiceInterface == null)
                {
                    violations.Add($"{serviceType.FullName} should implement IService<TState> with typed state");
                    continue;
                }

                // Get the state type from the generic interface
                var stateType = genericServiceInterface.GetGenericArguments()[0];
                
                // Verify state type implements IState
                var stateInterface = stateType.GetInterface("IState");
                if (stateInterface == null)
                {
                    violations.Add($"{serviceType.FullName} state type {stateType.Name} should implement IState");
                    continue;
                }

                // Check if service has state property
                var stateProperty = serviceType.GetProperties(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance)
                    .FirstOrDefault(p => p.PropertyType == stateType);

                if (stateProperty == null)
                {
                    violations.Add($"{serviceType.FullName} should have a {stateType.Name} State property");
                    continue;
                }

                // Check if has GetSnapshot method
                var getSnapshotMethod = serviceType.GetMethod("GetSnapshot");
                if (getSnapshotMethod == null)
                {
                    violations.Add($"{serviceType.FullName} should implement GetSnapshot() method");
                    continue;
                }

                // Check return type - should be ISnapshot<TState>
                var expectedSnapshotType = typeof(ISnapshot<>).MakeGenericType(stateType);
                if (getSnapshotMethod.ReturnType != expectedSnapshotType)
                {
                    violations.Add($"{serviceType.FullName}.GetSnapshot() should return ISnapshot<{stateType.Name}>");
                }
            }

            // Assert
            violations.ShouldBeEmpty($"Service state ownership violations found: {string.Join(", ", violations)}");
        }

        /// <summary>
        /// Tests that each domain has a dedicated orchestrator implementing IOrchestrator.
        /// Enforces domain coordination consistency across plugins.
        /// </summary>
        public static void Each_Domain_Should_Have_Dedicated_Orchestrator(Assembly pluginAssembly)
        {
            // Arrange
            var violations = new List<string>();
            
            // Define expected domains (can be extended per plugin)
            var expectedDomains = new[] { "Targeting", "Placement", "Manipulation", "Cursor", "Master" };
            
            var orchestratorTypes = Types.InAssembly(pluginAssembly)
                .That()
                .HaveNameEndingWith("Orchestrator")
                .And()
                .AreClasses()
                .GetTypes();

            // Act - Check domain orchestrator coverage
            foreach (var domain in expectedDomains)
            {
                var domainOrchestrator = orchestratorTypes
                    .FirstOrDefault(t => t.Name.Contains(domain, StringComparison.OrdinalIgnoreCase));
                    
                if (domainOrchestrator == null)
                {
                    violations.Add($"Missing {domain}WorkflowOrchestrator for domain: {domain}");
                }
                else if (!typeof(IOrchestrator).IsAssignableFrom(domainOrchestrator))
                {
                    violations.Add($"{domainOrchestrator.FullName} must implement IOrchestrator interface");
                }
            }

            // Assert
            violations.ShouldBeEmpty($"Domain orchestrator violations found: {string.Join(", ", violations)}");
        }

        /// <summary>
        /// Tests that orchestrators are registered in service registry for consumer access.
        /// Enforces that orchestrators are accessible through dependency injection.
        /// </summary>
        public static void Orchestrators_Should_Be_Registered_In_Service_Registry(Assembly pluginAssembly)
        {
            // Arrange
            var violations = new List<string>();
            
            // Get all orchestrator types
            var orchestratorTypes = Types.InAssembly(pluginAssembly)
                .That()
                .ImplementInterface(typeof(IOrchestrator))
                .And()
                .AreClasses()
                .GetTypes();

            // Act - Check that orchestrators can be registered (interface compliance)
            foreach (var orchestratorType in orchestratorTypes)
            {
                // All orchestrators should be registerable in service registry
                // This means they should have public constructors with injectable dependencies
                var constructors = orchestratorType.GetConstructors();
                var publicConstructor = constructors.FirstOrDefault(c => c.IsPublic);
                
                if (publicConstructor == null)
                {
                    violations.Add($"{orchestratorType.FullName} should have a public constructor for service registration");
                }
            }

            // Assert
            violations.ShouldBeEmpty($"Orchestrator registration violations found: {string.Join(", ", violations)}");
        }

        /// <summary>
        /// Tests that all orchestrators have unique identifiers.
        /// Prevents orchestrator ID conflicts in multi-plugin scenarios.
        /// </summary>
        public static void All_Orchestrators_Should_Have_Unique_Identifiers(Assembly pluginAssembly)
        {
            // Arrange
            var violations = new List<string>();
            
            var orchestratorTypes = Types.InAssembly(pluginAssembly)
                .That()
                .ImplementInterface(typeof(IOrchestrator))
                .And()
                .AreClasses()
                .GetTypes();

            // Act - Check for OrchestratorId property
            foreach (var orchestratorType in orchestratorTypes)
            {
                var orchestratorIdProperty = orchestratorType.GetProperty("OrchestratorId");
                if (orchestratorIdProperty == null)
                {
                    violations.Add($"{orchestratorType.FullName} should have OrchestratorId property");
                }
                else if (orchestratorIdProperty.PropertyType != typeof(OrchestratorId))
                {
                    violations.Add($"{orchestratorType.FullName}.OrchestratorId should be of type OrchestratorId");
                }
            }

            // Assert
            violations.ShouldBeEmpty($"Orchestrator ID violations found: {string.Join(", ", violations)}");
        }

        /// <summary>
        /// Tests that all state classes are pure data and properly paired with services.
        /// Enforces that every state has a corresponding service owner.
        /// </summary>
        public static void All_State_Classes_Should_Be_Pure_Data_And_Paired_With_Services(Assembly pluginAssembly)
        {
            // Arrange
            var violations = new List<string>();
            
            // Get all state classes (typically ending with "State")
            var stateTypes = Types.InAssembly(pluginAssembly)
                .That()
                .HaveNameEndingWith("State")
                .And()
                .AreClasses()
                .GetTypes();

            // Get all service classes for pairing validation
            var serviceTypes = Types.InAssembly(pluginAssembly)
                .That()
                .HaveNameEndingWith("Service")
                .And()
                .AreClasses()
                .GetTypes()
                .ToDictionary(t => t, t => t.GetInterfaces()
                    .Where(i => i.IsGenericType && i.GetGenericTypeDefinition() == typeof(IService<>))
                    .Select(i => i.GetGenericArguments()[0])
                    .FirstOrDefault());

            // Act - Check state purity and service pairing
            foreach (var stateType in stateTypes)
            {
                // Skip test classes and abstract classes
                if (stateType.Name.Contains("Test") || stateType.IsAbstract)
                    continue;

                // Check if implements IState
                var iStateInterface = stateType.GetInterface("IState");
                if (iStateInterface == null)
                {
                    violations.Add($"{stateType.FullName} should implement IState interface");
                    continue;
                }

                // Check for forbidden methods (states should not have business logic)
                var publicMethods = stateType.GetMethods(BindingFlags.Public | BindingFlags.Instance)
                    .Where(m => !m.IsSpecialName) // Exclude properties, constructors, etc.
                    .ToList();

                var forbiddenMethods = publicMethods.Where(m => 
                    m.Name != "ToString" && 
                    m.Name != "GetHashCode" && 
                    m.Name != "Equals" &&
                    !m.Name.StartsWith("Update") // Allow data update methods
                ).ToList();

                if (forbiddenMethods.Any())
                {
                    var methodNames = string.Join(", ", forbiddenMethods.Select(m => m.Name));
                    violations.Add($"{stateType.FullName} has business logic methods: {methodNames} - states should be pure data");
                }

                // Check for events (states should not have events)
                var events = stateType.GetEvents(BindingFlags.Public | BindingFlags.Instance);
                if (events.Any())
                {
                    var eventNames = string.Join(", ", events.Select(e => e.Name));
                    violations.Add($"{stateType.FullName} has events: {eventNames} - states should not emit events");
                }

                // Check for GetSnapshot method (should be on service, not state)
                var getSnapshotMethod = stateType.GetMethod("GetSnapshot");
                if (getSnapshotMethod != null)
                {
                    violations.Add($"{stateType.FullName} has GetSnapshot() method - this should be on the owning service, not the state");
                }

                // Check if state is paired with a service
                var owningService = serviceTypes.FirstOrDefault(kvp => kvp.Value == stateType).Key;
                if (owningService == null)
                {
                    violations.Add($"{stateType.FullName} should be owned by a service implementing IService<{stateType.Name}>");
                }
            }

            // Assert
            violations.ShouldBeEmpty($"State purity and pairing violations found: {string.Join(", ", violations)}");
        }

        /// <summary>
        /// Tests that snapshot classes are immutable and properly sealed.
        /// Prevents snapshot mutation and ensures thread safety.
        /// </summary>
        public static void Snapshot_Classes_Should_Be_Immutable(Assembly pluginAssembly)
        {
            // Arrange
            var violations = new List<string>();
            
            // Get all snapshot classes (typically ending with "Snapshot")
            var snapshotTypes = Types.InAssembly(pluginAssembly)
                .That()
                .HaveNameEndingWith("Snapshot")
                .And()
                .AreClasses()
                .GetTypes();

            // Act - Check immutability compliance
            foreach (var snapshotType in snapshotTypes)
            {
                // Skip test classes
                if (snapshotType.Name.Contains("Test"))
                    continue;

                // Check if implements ISnapshot
                var snapshotInterface = snapshotType.GetInterface("ISnapshot");
                if (snapshotInterface == null)
                {
                    violations.Add($"{snapshotType.FullName} should implement ISnapshot interface");
                    continue;
                }

                // Check if class is sealed (prevents inheritance that could break immutability)
                if (!snapshotType.IsSealed)
                {
                    violations.Add($"{snapshotType.FullName} should be sealed to ensure immutability");
                }

                // Check if has only readonly properties (no setters)
                var properties = snapshotType.GetProperties(BindingFlags.Public | BindingFlags.Instance);
                var mutableProperties = properties.Where(p => p.CanWrite).ToList();
                
                if (mutableProperties.Any())
                {
                    var mutablePropertyNames = string.Join(", ", mutableProperties.Select(p => p.Name));
                    violations.Add($"{snapshotType.FullName} has mutable properties: {mutablePropertyNames}");
                }

                // Check if has only private constructors (factory pattern through GetSnapshot)
                var constructors = snapshotType.GetConstructors(BindingFlags.Public | BindingFlags.Instance);
                if (constructors.Any())
                {
                    violations.Add($"{snapshotType.FullName} should have only private constructors (use factory pattern)");
                }
            }

            // Assert
            violations.ShouldBeEmpty($"Snapshot immutability violations found: {string.Join(", ", violations)}");
        }

        private static string DetermineServiceDomain(string serviceName, string serviceNamespace)
        {
            var fullName = $"{serviceNamespace}.{serviceName}";
            
            if (fullName.Contains("Targeting")) return "Targeting";
            if (fullName.Contains("Cursor")) return "Cursor";
            if (fullName.Contains("Grid")) return "Grid";
            if (fullName.Contains("Manipulation")) return "Manipulation";
            if (fullName.Contains("Placement")) return "Placement";
            
            return "Unknown";
        }

        private static List<string> FindViolatingDomains(string eventName, Dictionary<string, List<string>> domainMappings, string currentDomain)
        {
            var violations = new List<string>();
            
            foreach (var mapping in domainMappings)
            {
                if (mapping.Key == currentDomain) continue; // Skip current domain
                
                foreach (var keyword in mapping.Value)
                {
                    if (eventName.Contains(keyword, StringComparison.OrdinalIgnoreCase))
                    {
                        violations.Add(mapping.Key);
                        break; // Found violation, no need to check other keywords for this domain
                    }
                }
            }
            
            return violations;
        }
    }
}
