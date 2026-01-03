using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using NetArchTest.Rules;
using Shouldly;
using Xunit;

namespace BarkMoon.GameComposition.Tests.Architectural
{
    /// <summary>
    /// Generic cross-plugin architectural tests that work across domains and plugins.
    /// Uses configuration-driven approach for maximum flexibility.
    /// </summary>
    public class CrossPluginArchitectureTests
    {
        private static readonly ArchitectureConfig _config = LoadArchitectureConfig();

        [Fact(DisplayName = "ARCH-001: Services Should Reside In Services Namespace")]
        [Trait("Category", "Architectural")]
        public void Services_Should_Reside_In_Services_Namespace()
        {
            // Arrange
            var serviceTypes = GetServiceTypesAcrossPlugins();

            // Act & Assert
            foreach (var serviceType in serviceTypes)
            {
                var namespaceParts = serviceType.Namespace?.Split('.') ?? Array.Empty<string>();
                var hasServicesNamespace = namespaceParts.Contains("Services");
                
                hasServicesNamespace.ShouldBeTrue(
                    $"Service '{serviceType.FullName}' should reside in a 'Services' namespace. " +
                    $"Current namespace: '{serviceType.Namespace}'");
            }
        }

        [Fact(DisplayName = "ARCH-002: Services Should Implement IService Interface")]
        [Trait("Category", "Architectural")]
        public void Services_Should_Implement_IService_Interface()
        {
            // Arrange
            var serviceTypes = GetServiceTypesAcrossPlugins();

            // Act & Assert
            foreach (var serviceType in serviceTypes)
            {
                var hasIServiceInterface = serviceType.GetInterfaces()
                    .Any(i => i.Name.StartsWith("IService") || i.Name == "IService");

                hasIServiceInterface.ShouldBeTrue(
                    $"Service '{serviceType.FullName}' should implement an IService interface. " +
                    $"Found interfaces: {string.Join(", ", serviceType.GetInterfaces().Select(i => i.Name))}");
            }
        }

        [Fact(DisplayName = "ARCH-003: Domain Types Should Not Cross Domain Boundaries")]
        [Trait("Category", "Architectural")]
        public void Domain_Types_Should_Not_Cross_Domain_Boundaries()
        {
            // Arrange
            var domainTypes = GetDomainTypesAcrossPlugins();
            var violations = new List<string>();

            // Act & Assert
            foreach (var domainType in domainTypes)
            {
                var currentDomain = ExtractDomainFromNamespace(domainType.Namespace ?? "");
                var dependencies = GetDomainDependencies(domainType);

                foreach (var dependency in dependencies)
                {
                    var dependencyDomain = ExtractDomainFromNamespace(dependency.Namespace ?? "");
                    
                    if (!string.IsNullOrEmpty(currentDomain) && 
                        !string.IsNullOrEmpty(dependencyDomain) && 
                        currentDomain != dependencyDomain &&
                        !_config.AllowedCrossDomainDependencies.Contains($"{currentDomain}->{dependencyDomain}"))
                    {
                        violations.Add(
                            $"Domain type '{domainType.FullName}' (domain: {currentDomain}) " +
                            $"should not depend on '{dependency.FullName}' (domain: {dependencyDomain})");
                    }
                }
            }

            violations.ShouldBeEmpty($"Found {violations.Count} domain boundary violations:\n{string.Join("\n", violations)}");
        }

        [Fact(DisplayName = "ARCH-004: State Types Should Be Immutable Or Have Clear Mutability")]
        [Trait("Category", "Architectural")]
        public void State_Types_Should_Be_Immutable_Or_Have_Clear_Mutability()
        {
            // Arrange
            var stateTypes = GetStateTypesAcrossPlugins();
            var violations = new List<string>();

            // Act & Assert
            foreach (var stateType in stateTypes)
            {
                var isStruct = stateType.IsValueType;
                var isReadOnlyStruct = isStruct && stateType.GetCustomAttribute<IsReadOnlyAttribute>() != null;
                var hasImmutableInterface = stateType.GetInterfaces().Any(i => i.Name.Contains("Immutable"));
                var hasStateSuffix = stateType.Name.EndsWith("State");

                // State types should be either immutable or clearly marked as mutable
                if (!isReadOnlyStruct && !hasImmutableInterface && hasStateSuffix)
                {
                    var hasPublicSetters = stateType.GetProperties()
                        .Any(p => p.CanWrite && p.SetMethod?.IsPublic == true);

                    if (hasPublicSetters)
                    {
                        violations.Add(
                            $"State type '{stateType.FullName}' has public setters but is not marked as immutable. " +
                            $"Consider making it a readonly struct or implementing an immutable interface.");
                    }
                }
            }

            violations.ShouldBeEmpty($"Found {violations.Count} state mutability violations:\n{string.Join("\n", violations)}");
        }

        [Fact(DisplayName = "ARCH-005: Event Types Should Inherit From ServiceEvent")]
        [Trait("Category", "Architectural")]
        public void Event_Types_Should_Inherit_From_ServiceEvent()
        {
            // Arrange
            var eventTypes = GetEventTypesAcrossPlugins();
            var serviceEventType = typeof(BarkMoon.GameComposition.Core.Events.ServiceEvent);

            // Act & Assert
            foreach (var eventType in eventTypes)
            {
                var inheritsFromServiceEvent = serviceEventType.IsAssignableFrom(eventType);

                inheritsFromServiceEvent.ShouldBeTrue(
                    $"Event type '{eventType.FullName}' should inherit from ServiceEvent. " +
                    $"Current inheritance chain: {GetInheritanceChain(eventType)}");
            }
        }

        [Fact(DisplayName = "ARCH-006: Adapter Types Should Bridge Engine And Core")]
        [Trait("Category", "Architectural")]
        public void Adapter_Types_Should_Bridge_Engine_And_Core()
        {
            // Arrange
            var adapterTypes = GetAdapterTypesAcrossPlugins();
            var violations = new List<string>();

            // Act & Assert
            foreach (var adapterType in adapterTypes)
            {
                var hasCoreDependency = adapterType.GetReferencedAssemblies()
                    .Any(a => a.FullName.Contains("GameComposition.Core") || a.FullName.Contains("GridPlacement.Core"));
                
                var hasEngineDependency = adapterType.GetReferencedAssemblies()
                    .Any(a => a.FullName.Contains("Godot") || a.FullName.Contains("Engine"));

                if (!hasCoreDependency || !hasEngineDependency)
                {
                    violations.Add(
                        $"Adapter '{adapterType.FullName}' should bridge both engine and core layers. " +
                        $"Has Core: {hasCoreDependency}, Has Engine: {hasEngineDependency}");
                }
            }

            violations.ShouldBeEmpty($"Found {violations.Count} adapter violations:\n{string.Join("\n", violations)}");
        }

        [Fact(DisplayName = "ARCH-007: Workflow Types Should Coordinate Multiple Services")]
        [Trait("Category", "Architectural")]
        public void Workflow_Types_Should_Coordinate_Multiple_Services()
        {
            // Arrange
            var workflowTypes = GetWorkflowTypesAcrossPlugins();
            var violations = new List<string>();

            // Act & Assert
            foreach (var workflowType in workflowTypes)
            {
                var serviceDependencies = workflowType.GetFields()
                    .Concat(workflowType.GetProperties().Select(p => p.PropertyType))
                    .Where(f => f.Name.Contains("Service") || f.GetInterfaces().Any(i => i.Name.Contains("Service")))
                    .ToList();

                if (serviceDependencies.Count < 2)
                {
                    violations.Add(
                        $"Workflow '{workflowType.FullName}' should coordinate multiple services. " +
                        $"Found {serviceDependencies.Count} service dependencies.");
                }
            }

            violations.ShouldBeEmpty($"Found {violations.Count} workflow violations:\n{string.Join("\n", violations)}");
        }

        #region Helper Methods

        private static ArchitectureConfig LoadArchitectureConfig()
        {
            // For now, return default config. In future, this could load from YAML/JSON
            return new ArchitectureConfig
            {
                AllowedCrossDomainDependencies = new HashSet<string>
                {
                    "Input->Targeting",
                    "Targeting->Manipulation", 
                    "Manipulation->Placement",
                    "Workflows->Input",
                    "Workflows->Targeting",
                    "Workflows->Manipulation",
                    "Workflows->Placement"
                }
            };
        }

        private static IEnumerable<Type> GetServiceTypesAcrossPlugins()
        {
            var assemblies = GetRelevantAssemblies();
            return assemblies
                .SelectMany(a => a.GetTypes())
                .Where(t => t.IsClass && !t.IsAbstract && 
                       (t.Name.EndsWith("Service") || t.Name.EndsWith("Service") || 
                        t.GetInterfaces().Any(i => i.Name.Contains("Service"))))
                .ToList();
        }

        private static IEnumerable<Type> GetDomainTypesAcrossPlugins()
        {
            var assemblies = GetRelevantAssemblies();
            return assemblies
                .SelectMany(a => a.GetTypes())
                .Where(t => !t.IsInterface && !t.IsEnum &&
                       (t.Namespace?.Contains("Targeting") == true ||
                        t.Namespace?.Contains("Manipulation") == true ||
                        t.Namespace?.Contains("Placement") == true ||
                        t.Namespace?.Contains("Grid") == true))
                .ToList();
        }

        private static IEnumerable<Type> GetStateTypesAcrossPlugins()
        {
            var assemblies = GetRelevantAssemblies();
            return assemblies
                .SelectMany(a => a.GetTypes())
                .Where(t => (t.Name.EndsWith("State") || t.Name.EndsWith("Snapshot")) &&
                       !t.IsInterface)
                .ToList();
        }

        private static IEnumerable<Type> GetEventTypesAcrossPlugins()
        {
            var assemblies = GetRelevantAssemblies();
            return assemblies
                .SelectMany(a => a.GetTypes())
                .Where(t => t.Name.EndsWith("Event") && !t.IsInterface)
                .ToList();
        }

        private static IEnumerable<Type> GetAdapterTypesAcrossPlugins()
        {
            var assemblies = GetRelevantAssemblies();
            return assemblies
                .SelectMany(a => a.GetTypes())
                .Where(t => t.Name.Contains("Adapter") || t.Namespace?.Contains("Adapters") == true)
                .ToList();
        }

        private static IEnumerable<Type> GetWorkflowTypesAcrossPlugins()
        {
            var assemblies = GetRelevantAssemblies();
            return assemblies
                .SelectMany(a => a.GetTypes())
                .Where(t => (t.Name.Contains("Workflow") || t.Name.Contains("Orchestrator")) &&
                       !t.IsInterface)
                .ToList();
        }

        private static IEnumerable<Assembly> GetRelevantAssemblies()
        {
            var assemblies = new List<Assembly>();
            
            // Add current assembly
            assemblies.Add(Assembly.GetExecutingAssembly());
            
            // Add GameComposition.Core
            try
            {
                assemblies.Add(typeof(BarkMoon.GameComposition.Core.Types.TypedId<>).Assembly);
            }
            catch { /* Ignore if not available */ }
            
            // Add GridPlacement.Core
            try
            {
                assemblies.Add(typeof(BarkMoon.GridPlacement.Core.Placement.Data.PlacementEntry).Assembly);
            }
            catch { /* Ignore if not available */ }
            
            return assemblies.Where(a => a != null).Distinct();
        }

        private static string ExtractDomainFromNamespace(string namespaceName)
        {
            var parts = namespaceName.Split('.');
            var domainParts = new[] { "Targeting", "Manipulation", "Placement", "Grid", "Input", "Workflows" };
            
            foreach (var part in parts)
            {
                if (domainParts.Contains(part))
                    return part;
            }
            
            return string.Empty;
        }

        private static IEnumerable<Type> GetDomainDependencies(Type type)
        {
            return type.GetFields()
                .Concat(type.GetProperties().Select(p => p.PropertyType))
                .Where(f => f.IsPublic || (f is PropertyInfo pi && pi.GetMethod?.IsPublic == true))
                .Select(f => f is FieldInfo fi ? fi.FieldType : (f as PropertyInfo)?.PropertyType)
                .Where(t => t != null && !t.IsPrimitive && t.Namespace != null)
                .Distinct();
        }

        private static string GetInheritanceChain(Type type)
        {
            var chain = new List<string>();
            var current = type.BaseType;
            
            while (current != null)
            {
                chain.Add(current.Name);
                current = current.BaseType;
            }
            
            return string.Join(" -> ", chain);
        }

        #endregion
    }
}
