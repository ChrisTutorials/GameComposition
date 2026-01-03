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
    /// Parameterized architectural tests that work across plugins and domains.
    /// Uses configuration-driven approach for maximum flexibility.
    /// </summary>
    public class ParameterizedArchitectureTests
    {
        private static readonly ArchitectureConfig _config = ArchitectureConfigLoader.LoadConfig();

        public static TheoryData<string, string> DomainBoundaryTestData =>
            GenerateDomainBoundaryTestData();

        public static TheoryData<string, string> ServiceLocationTestData =>
            GenerateServiceLocationTestData();

        public static TheoryData<string, string> StateMutabilityTestData =>
            GenerateStateMutabilityTestData();

        [Theory(DisplayName = "ARCH-008: Domain Boundary Enforcement")]
        [Trait("Category", "Architectural")]
        [MemberData(nameof(DomainBoundaryTestData))]
        public void Domain_Boundary_Enforcement_Should_Work(string sourceDomain, string targetDomain)
        {
            // Arrange
            var sourceTypes = GetTypesInDomain(sourceDomain);
            var violations = new List<string>();

            // Act
            foreach (var sourceType in sourceTypes)
            {
                var dependencies = GetDomainDependencies(sourceType);
                var violatingDependencies = dependencies
                    .Where(d => IsInDomain(d, targetDomain))
                    .ToList();

                foreach (var violation in violatingDependencies)
                {
                    var dependencyKey = $"{sourceDomain}->{targetDomain}";
                    if (!_config.AllowedCrossDomainDependencies.Contains(dependencyKey))
                    {
                        violations.Add(
                            $"Domain boundary violation: {sourceType.FullName} (domain: {sourceDomain}) " +
                            $"depends on {violation.FullName} (domain: {targetDomain})");
                    }
                }
            }

            // Assert
            violations.ShouldBeEmpty(
                $"Domain boundary violations found between {sourceDomain} and {targetDomain}:\n" +
                $"{string.Join("\n", violations)}");
        }

        [Theory(DisplayName = "ARCH-009: Service Location Validation")]
        [Trait("Category", "Architectural")]
        [MemberData(nameof(ServiceLocationTestData))]
        public void Service_Location_Should_Be_Consistent(string pluginName, string expectedServicePattern)
        {
            // Arrange
            var pluginServices = GetServicesInPlugin(pluginName);

            // Act & Assert
            foreach (var serviceType in pluginServices)
            {
                var namespaceParts = serviceType.Namespace?.Split('.') ?? Array.Empty<string>();
                var hasServicesNamespace = namespaceParts.Contains("Services");
                
                hasServicesNamespace.ShouldBeTrue(
                    $"Service '{serviceType.FullName}' in plugin '{pluginName}' " +
                    $"should reside in a 'Services' namespace. Current: '{serviceType.Namespace}'");

                // Check for proper service interface
                var hasServiceInterface = serviceType.GetInterfaces()
                    .Any(i => _config.RequiredServiceInterfaces.Any(pattern => 
                        pattern.Contains("*") ? i.Name.Contains(pattern.Replace("*", "")) : i.Name == pattern));

                hasServiceInterface.ShouldBeTrue(
                    $"Service '{serviceType.FullName}' should implement a service interface. " +
                    $"Found: {string.Join(", ", serviceType.GetInterfaces().Select(i => i.Name))}");
            }
        }

        [Theory(DisplayName = "ARCH-010: State Mutability Patterns")]
        [Trait("Category", "Architectural")]
        [MemberData(nameof(StateMutabilityTestData))]
        public void State_Mutability_Should_Follow_Patterns(string pluginName, string statePattern)
        {
            // Arrange
            var stateTypes = GetStatesInPlugin(pluginName, statePattern);
            var violations = new List<string>();

            // Act
            foreach (var stateType in stateTypes)
            {
                var isStruct = stateType.IsValueType;
                var isReadOnlyStruct = isStruct && stateType.GetCustomAttribute<IsReadOnlyAttribute>() != null;
                var hasImmutableInterface = stateType.GetInterfaces().Any(i => i.Name.Contains("Immutable"));
                var hasSnapshotSuffix = stateType.Name.EndsWith("Snapshot");

                // Different rules for different state types
                if (hasSnapshotSuffix)
                {
                    // Snapshots should be immutable
                    if (!isReadOnlyStruct && !hasImmutableInterface)
                    {
                        violations.Add(
                            $"Snapshot '{stateType.FullName}' should be immutable. " +
                            $"Consider making it a readonly struct or implementing an immutable interface.");
                    }
                }
                else if (stateType.Name.EndsWith("State"))
                {
                    // State can be mutable but should be clearly marked
                    var hasPublicSetters = stateType.GetProperties()
                        .Any(p => p.CanWrite && p.SetMethod?.IsPublic == true);

                    if (hasPublicSetters && !isReadOnlyStruct && !hasImmutableInterface)
                    {
                        violations.Add(
                            $"State '{stateType.FullName}' has public setters but is not marked as mutable. " +
                            $"Consider adding a mutable interface or making it immutable.");
                    }
                }
            }

            // Assert
            violations.ShouldBeEmpty(
                $"State mutability violations in plugin '{pluginName}' for pattern '{statePattern}':\n" +
                $"{string.Join("\n", violations)}");
        }

        [Fact(DisplayName = "ARCH-011: Cross-Plugin Dependency Validation")]
        [Trait("Category", "Architectural")]
        public void Cross_Plugin_Dependencies_Should_Be_Valid()
        {
            // Arrange
            var plugins = GetAvailablePlugins();
            var violations = new List<string>();

            // Act
            foreach (var plugin in plugins)
            {
                var pluginTypes = GetTypesInPlugin(plugin);
                
                foreach (var type in pluginTypes)
                {
                    var dependencies = GetAssemblyDependencies(type);
                    
                    foreach (var dependency in dependencies)
                    {
                        var dependencyPlugin = GetPluginFromAssembly(dependency);
                        
                        if (!string.IsNullOrEmpty(dependencyPlugin) && dependencyPlugin != plugin)
                        {
                            // Check if this cross-plugin dependency is allowed
                            var dependencyKey = $"{plugin}->{dependencyPlugin}";
                            if (!_config.AllowedCrossDomainDependencies.Contains(dependencyKey))
                            {
                                violations.Add(
                                    $"Cross-plugin dependency violation: {type.FullName} (plugin: {plugin}) " +
                                    $"depends on {dependency.FullName} (plugin: {dependencyPlugin})");
                            }
                        }
                    }
                }
            }

            // Assert
            violations.ShouldBeEmpty(
                $"Cross-plugin dependency violations found:\n{string.Join("\n", violations)}");
        }

        [Fact(DisplayName = "ARCH-012: Assembly Naming Consistency")]
        [Trait("Category", "Architectural")]
        public void Assembly_Naming_Should_Be_Consistent()
        {
            // Arrange
            var assemblies = GetRelevantAssemblies();
            var violations = new List<string>();

            // Act
            foreach (var assembly in assemblies)
            {
                var assemblyName = assembly.GetName().Name;
                
                // Check for proper naming patterns
                var hasValidPrefix = assemblyName.StartsWith("BarkMoon.") || 
                                  assemblyName.StartsWith("GameComposition.") ||
                                  assemblyName.StartsWith("GridPlacement.");
                
                var hasValidSuffix = assemblyName.EndsWith(".Core") ||
                                  assemblyName.EndsWith(".Tests") ||
                                  assemblyName.EndsWith(".Tools");

                if (!hasValidPrefix)
                {
                    violations.Add($"Assembly '{assemblyName}' should have a valid prefix (BarkMoon., GameComposition., or GridPlacement.)");
                }

                if (assemblyName.EndsWith(".Core") && !hasValidPrefix)
                {
                    violations.Add($"Core assembly '{assemblyName}' should have proper company prefix");
                }
            }

            // Assert
            violations.ShouldBeEmpty(
                $"Assembly naming violations found:\n{string.Join("\n", violations)}");
        }

        #region Test Data Generation

        private static TheoryData<string, string> GenerateDomainBoundaryTestData()
        {
            var data = new TheoryData<string, string>();
            var domains = _config.DomainPatterns.ToList();

            // Generate all possible domain pairs
            for (int i = 0; i < domains.Count; i++)
            {
                for (int j = 0; j < domains.Count; j++)
                {
                    if (i != j)
                    {
                        data.Add(domains[i], domains[j]);
                    }
                }
            }

            return data;
        }

        private static TheoryData<string, string> GenerateServiceLocationTestData()
        {
            var data = new TheoryData<string, string>();
            var plugins = GetAvailablePlugins();

            foreach (var plugin in plugins)
            {
                foreach (var pattern in _config.ServicePatterns)
                {
                    data.Add(plugin, pattern);
                }
            }

            return data;
        }

        private static TheoryData<string, string> GenerateStateMutabilityTestData()
        {
            var data = new TheoryData<string, string>();
            var plugins = GetAvailablePlugins();

            foreach (var plugin in plugins)
            {
                foreach (var pattern in _config.StatePatterns)
                {
                    data.Add(plugin, pattern);
                }
            }

            return data;
        }

        #endregion

        #region Helper Methods

        private static IEnumerable<Type> GetTypesInDomain(string domain)
        {
            var assemblies = GetRelevantAssemblies();
            return assemblies
                .SelectMany(a => a.GetTypes())
                .Where(t => !t.IsInterface && !t.IsEnum &&
                       (t.Namespace?.Contains(domain) == true))
                .ToList();
        }

        private static IEnumerable<Type> GetServicesInPlugin(string pluginName)
        {
            var assemblies = GetRelevantAssemblies();
            return assemblies
                .Where(a => a.GetName().Name.Contains(pluginName))
                .SelectMany(a => a.GetTypes())
                .Where(t => t.IsClass && !t.IsAbstract &&
                       _config.ServicePatterns.Any(pattern => 
                           pattern.Contains("*") ? t.Name.Contains(pattern.Replace("*", "")) : t.Name == pattern))
                .ToList();
        }

        private static IEnumerable<Type> GetStatesInPlugin(string pluginName, string pattern)
        {
            var assemblies = GetRelevantAssemblies();
            return assemblies
                .Where(a => a.GetName().Name.Contains(pluginName))
                .SelectMany(a => a.GetTypes())
                .Where(t => !t.IsInterface && !t.IsEnum &&
                       (pattern.Contains("*") ? t.Name.Contains(pattern.Replace("*", "")) : t.Name == pattern))
                .ToList();
        }

        private static IEnumerable<Type> GetTypesInPlugin(string pluginName)
        {
            var assemblies = GetRelevantAssemblies();
            return assemblies
                .Where(a => a.GetName().Name.Contains(pluginName))
                .SelectMany(a => a.GetTypes())
                .Where(t => !t.IsInterface && !t.IsEnum)
                .ToList();
        }

        private static IEnumerable<Assembly> GetRelevantAssemblies()
        {
            var assemblies = new List<Assembly>();
            
            // Add current assembly
            assemblies.Add(Assembly.GetExecutingAssembly());
            
            // Auto-discover relevant assemblies
            var currentDir = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location) ?? "";
            var dllFiles = Directory.GetFiles(currentDir, "*.dll")
                .Where(f => Path.GetFileName(f).Contains("Core") && 
                           !Path.GetFileName(f).Contains("Tests") &&
                           !Path.GetFileName(f).Contains("Tools"));

            foreach (var dllFile in dllFiles)
            {
                try
                {
                    var assembly = Assembly.LoadFrom(dllFile);
                    assemblies.Add(assembly);
                }
                catch
                {
                    // Ignore assemblies that can't be loaded
                }
            }

            return assemblies.Where(a => a != null).Distinct();
        }

        private static List<string> GetAvailablePlugins()
        {
            var assemblies = GetRelevantAssemblies();
            return assemblies
                .Select(a => ExtractPluginName(a.GetName().Name))
                .Where(name => !string.IsNullOrEmpty(name))
                .Distinct()
                .ToList();
        }

        private static string ExtractPluginName(string assemblyName)
        {
            if (assemblyName.Contains("GameComposition")) return "GameComposition";
            if (assemblyName.Contains("GridPlacement")) return "GridPlacement";
            if (assemblyName.Contains("ItemDrops")) return "ItemDrops";
            if (assemblyName.Contains("WorldTime")) return "WorldTime";
            return string.Empty;
        }

        private static bool IsInDomain(Type type, string domain)
        {
            return type.Namespace?.Contains(domain) == true;
        }

        private static IEnumerable<Type> GetDomainDependencies(Type type)
        {
            return type.GetFields()
                .Concat(type.GetProperties().Select(p => p.PropertyType))
                .Where(f => f is FieldInfo fi ? fi.IsPublic : (f as PropertyInfo)?.GetMethod?.IsPublic == true)
                .Select(f => f is FieldInfo fi ? fi.FieldType : (f as PropertyInfo)?.PropertyType)
                .Where(t => t != null && !t.IsPrimitive && t.Namespace != null)
                .Distinct();
        }

        private static IEnumerable<Assembly> GetAssemblyDependencies(Type type)
        {
            return type.GetReferencedAssemblies()
                .Select(Assembly.Load)
                .Where(a => a != null);
        }

        private static string GetPluginFromAssembly(Assembly assembly)
        {
            return ExtractPluginName(assembly.GetName().Name);
        }

        #endregion
    }
}
