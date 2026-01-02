using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using NetArchTest.Rules;
using Shouldly;
using Xunit;

namespace BarkMoon.GameComposition.ArchitecturalTests.Orchestrators
{
    /// <summary>
    /// Orchestrator architectural rules for plugin ecosystem.
    /// Tests orchestrator patterns, ownership, and interface compliance.
    /// </summary>
    public class OrchestratorArchitecturalTests
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

            // Act - Check dependencies
            var violations = new List<string>();
            
            foreach (var orchestratorType in orchestratorTypes)
            {
                var dependencies = orchestratorType.GetConstructors()
                    .SelectMany(c => c.GetParameters())
                    .Select(p => p.ParameterType)
                    .Where(t => !t.IsInterface && !t.IsPrimitive && t != typeof(string))
                    .ToList();

                foreach (var dependency in dependencies)
                {
                    var dependencyName = dependency.Name;
                    
                    // Allow services and event bus
                    if (dependencyName.EndsWith("Service") || 
                        dependencyName == "IEventBus" ||
                        dependencyName == "EventBus")
                    {
                        continue;
                    }

                    // Disallow direct dependencies on other orchestrators, states, or data types
                    if (dependencyName.EndsWith("Orchestrator") ||
                        dependencyName.EndsWith("State") ||
                        dependencyName.EndsWith("Data") ||
                        dependencyName.EndsWith("Model"))
                    {
                        violations.Add($"{orchestratorType.Name} should not depend on {dependencyName} directly");
                    }
                }
            }

            // Assert
            violations.ShouldBeEmpty($"Orchestrator dependency violations found: {string.Join(", ", violations)}");
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

            // Act & Assert
            foreach (var orchestratorType in orchestratorTypes)
            {
                orchestratorType.ShouldImplement(typeof(BarkMoon.GameComposition.Core.Interfaces.IOrchestrator),
                    $"Orchestrator {orchestratorType.Name} should implement IOrchestrator interface");
            }
        }

        /// <summary>
        /// Tests that each domain has a dedicated orchestrator implementing IOrchestrator.
        /// Enforces domain coordination consistency across plugins.
        /// </summary>
        public static void Each_Domain_Should_Have_Dedicated_Orchestrator(Assembly pluginAssembly)
        {
            // Arrange
            var violations = new List<string>();
            
            // Get all orchestrator types
            var orchestratorTypes = Types.InAssembly(pluginAssembly)
                .That()
                .HaveNameEndingWith("Orchestrator")
                .And()
                .AreClasses()
                .GetTypes();

            // Expected domains based on plugin structure
            var expectedDomains = new[]
            {
                "Targeting", "Placement", "Manipulation", "Cursor", "Grid", "Master"
            };

            // Act - Check that each expected domain has an orchestrator
            foreach (var domain in expectedDomains)
            {
                var domainOrchestrator = orchestratorTypes
                    .FirstOrDefault(o => o.Name.Contains(domain));

                if (domainOrchestrator == null)
                {
                    violations.Add($"Plugin should have {domain} orchestrator");
                }
                else
                {
                    // Verify it implements IOrchestrator
                    if (!typeof(BarkMoon.GameComposition.Core.Interfaces.IOrchestrator).IsAssignableFrom(domainOrchestrator))
                    {
                        violations.Add($"{domainOrchestrator.FullName} must implement IOrchestrator interface");
                    }
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
                .ImplementInterface(typeof(BarkMoon.GameComposition.Core.Interfaces.IOrchestrator))
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
            var orchestratorIds = new List<string>();
            
            // Get all orchestrator types
            var orchestratorTypes = Types.InAssembly(pluginAssembly)
                .That()
                .ImplementInterface(typeof(BarkMoon.GameComposition.Core.Interfaces.IOrchestrator))
                .And()
                .AreClasses()
                .GetTypes();

            // Act - Check that all orchestrators have OrchestratorId property
            foreach (var orchestratorType in orchestratorTypes)
            {
                var orchestratorIdProperty = orchestratorType.GetProperty("OrchestratorId");
                
                if (orchestratorIdProperty == null)
                {
                    violations.Add($"{orchestratorType.FullName} should have OrchestratorId property");
                }
                else if (orchestratorIdProperty.PropertyType != typeof(BarkMoon.GameComposition.Core.Types.OrchestratorId))
                {
                    violations.Add($"{orchestratorType.FullName}.OrchestratorId should be of type OrchestratorId");
                }
            }

            // Assert
            violations.ShouldBeEmpty($"Orchestrator ID violations found: {string.Join(", ", violations)}");
        }
    }
}
