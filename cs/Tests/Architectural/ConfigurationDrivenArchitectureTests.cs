using Xunit;
using NetArchTest.Rules;
using Shouldly;
using System.Linq;
using System.Reflection;
using System.Collections.Generic;
using BarkMoon.GameComposition.Tests.Common;

// Explicitly alias the NetArchTest Types class to avoid conflict
using ArchTypes = NetArchTest.Rules;

namespace BarkMoon.GameComposition.Tests.Architectural
{
    /// <summary>
    /// Configuration-driven architectural tests that enforce rules through YML configuration.
    /// Replaces hardcoded rules with maintainable configuration-driven approach.
    /// </summary>
    public class ConfigurationDrivenArchitectureTests
    {
        private static readonly ArchitectureConfig _config = ArchitectureConfigLoader.LoadConfig();

        [Fact(DisplayName = "CONFIG-001: GlobalEventBus Pattern Should Be Prohibited")]
        [Trait("Category", "Architectural")]
        public void GlobalEventBus_Pattern_Should_Be_Prohibited()
        {
            // Arrange - Get configuration
            var globalEventBusConfig = _config.ProhibitedPatterns.GlobalEventBus;
            if (!globalEventBusConfig.Enabled)
                return; // Skip if rule is disabled

            var allViolations = new List<string>();

            // Act - Check each target assembly
            foreach (var assemblyName in globalEventBusConfig.TestAssemblies)
            {
                try
                {
                    var assembly = LoadAssemblyByName(assemblyName);
                    if (assembly == null) continue;

                    var violations = CheckGlobalEventBusViolations(assembly, globalEventBusConfig);
                    allViolations.AddRange(violations);
                }
                catch (Exception ex)
                {
                    allViolations.Add($"Failed to load assembly {assemblyName}: {ex.Message}");
                }
            }

            // Assert - No violations should exist
            if (allViolations.Count > 0)
            {
                var errorMessage = $"{globalEventBusConfig.Message}\nViolations:\n{string.Join("\n", allViolations)}";
                throw new InvalidOperationException(errorMessage);
            }
        }

        [Fact(DisplayName = "CONFIG-002: Weak Typing Should Be Prohibited")]
        [Trait("Category", "Architectural")]
        public void Weak_Typing_Should_Be_Prohibited()
        {
            // Arrange
            var weakTypingConfig = _config.ProhibitedPatterns.WeakTyping;
            if (!weakTypingConfig.Enabled)
                return; // Skip if rule is disabled

            var assemblies = ArchitecturalTestHelpers.GetAllAssemblies();
            var allViolations = new List<string>();

            // Act - Check each assembly for weak typing violations
            foreach (var assembly in assemblies)
            {
                var violations = CheckWeakTypingViolations(assembly, weakTypingConfig);
                allViolations.AddRange(violations);
            }

            // Assert - No violations should exist
            if (allViolations.Count > 0)
            {
                var errorMessage = $"{weakTypingConfig.Message}\nViolations:\n{string.Join("\n", allViolations)}";
                throw new InvalidOperationException(errorMessage);
            }
        }

        [Fact(DisplayName = "CONFIG-003: Custom Implementations Should Be Prohibited")]
        [Trait("Category", "Architectural")]
        public void Custom_Implementations_Should_Be_Prohibited()
        {
            // Arrange
            var customImplConfig = _config.ProhibitedPatterns.CustomImplementations;
            if (!customImplConfig.Enabled)
                return; // Skip if rule is disabled

            var allViolations = new List<string>();

            // Act - Check each target assembly
            foreach (var assemblyName in customImplConfig.TargetAssemblies)
            {
                try
                {
                    var assembly = LoadAssemblyByName(assemblyName);
                    if (assembly == null) continue;

                    var violations = CheckCustomImplementationViolations(assembly, customImplConfig);
                    allViolations.AddRange(violations);
                }
                catch (Exception ex)
                {
                    allViolations.Add($"Failed to load assembly {assemblyName}: {ex.Message}");
                }
            }

            // Assert - No violations should exist
            if (allViolations.Count > 0)
            {
                var errorMessage = $"{customImplConfig.Message}\nViolations:\n{string.Join("\n", allViolations)}";
                throw new InvalidOperationException(errorMessage);
            }
        }

        [Fact(DisplayName = "CONFIG-004: Engine Dependencies Should Be Prohibited")]
        [Trait("Category", "Architectural")]
        public void Engine_Dependencies_Should_Be_Prohibited()
        {
            // Arrange
            var engineDepConfig = _config.ProhibitedPatterns.EngineDependencies;
            if (!engineDepConfig.Enabled)
                return; // Skip if rule is disabled

            var allViolations = new List<string>();

            // Act - Check each target assembly
            foreach (var assemblyName in engineDepConfig.TargetAssemblies)
            {
                try
                {
                    var assembly = LoadAssemblyByName(assemblyName);
                    if (assembly == null) continue;

                    var violations = CheckEngineDependencyViolations(assembly, engineDepConfig);
                    allViolations.AddRange(violations);
                }
                catch (Exception ex)
                {
                    allViolations.Add($"Failed to load assembly {assemblyName}: {ex.Message}");
                }
            }

            // Assert - No violations should exist
            if (allViolations.Count > 0)
            {
                var errorMessage = $"{engineDepConfig.Message}\nViolations:\n{string.Join("\n", allViolations)}";
                throw new InvalidOperationException(errorMessage);
            }
        }

        private static Assembly? LoadAssemblyByName(string assemblyName)
        {
            // Try to load from current directory first
            var assemblyPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, $"{assemblyName}.dll");
            if (File.Exists(assemblyPath))
                return Assembly.LoadFrom(assemblyPath);

            // Try to load by name
            try
            {
                return Assembly.Load(assemblyName);
            }
            catch
            {
                return null;
            }
        }

        private static List<string> CheckGlobalEventBusViolations(Assembly assembly, GlobalEventBusConfig config)
        {
            var violations = new List<string>();

            // Check for GlobalEventBus classes (prohibited pattern)
            var globalEventBusClasses = ArchTypes.InAssembly(assembly)
                .That()
                .HaveNameContaining(config.Pattern, StringComparison.OrdinalIgnoreCase)
                .And()
                .AreClasses()
                .GetTypes().ToArray();

            foreach (var globalEventBusClass in globalEventBusClasses)
            {
                // Skip allowed domain-specific event buses
                if (config.AllowedDomainSpecific.Any(allowed => 
                    globalEventBusClass.Name.Contains(allowed, StringComparison.OrdinalIgnoreCase)))
                    continue;

                violations.Add(
                    $"Assembly {assembly.GetName().Name}: GlobalEventBus pattern {globalEventBusClass.Name} is prohibited. {config.Message}");
            }

            return violations;
        }

        private static List<string> CheckWeakTypingViolations(Assembly assembly, WeakTypingConfig config)
        {
            var violations = new List<string>();

            // Check for Dictionary<string, object> usage in target namespaces
            var targetTypes = new List<Type>();

            foreach (var nsPattern in config.TargetNamespaces)
            {
                var types = ArchTypes.InAssembly(assembly)
                    .That()
                    .ResideInNamespace(nsPattern)
                    .Or()
                    .HaveNameContainingAny(config.NamePatterns.ToArray())
                    .GetTypes();

                targetTypes.AddRange(types);
            }

            foreach (var type in targetTypes)
            {
                // Check for Dictionary<string, object> fields or properties
                var dictionaryFields = type.GetFields(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance)
                    .Where(f => f.FieldType.Name.Contains("Dictionary") && 
                               f.FieldType.GetGenericArguments().Length == 2 &&
                               f.FieldType.GetGenericArguments()[0].Name == "String" &&
                               f.FieldType.GetGenericArguments()[1].Name == "Object");

                var dictionaryProperties = type.GetProperties(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance)
                    .Where(p => p.PropertyType.Name.Contains("Dictionary") && 
                               p.PropertyType.GetGenericArguments().Length == 2 &&
                               p.PropertyType.GetGenericArguments()[0].Name == "String" &&
                               p.PropertyType.GetGenericArguments()[1].Name == "Object");

                foreach (var field in dictionaryFields)
                {
                    violations.Add($"Type {type.FullName} has prohibited Dictionary<string, object> field: {field.Name}");
                }

                foreach (var property in dictionaryProperties)
                {
                    violations.Add($"Type {type.FullName} has prohibited Dictionary<string, object> property: {property.Name}");
                }
            }

            return violations;
        }

        private static List<string> CheckCustomImplementationViolations(Assembly assembly, CustomImplementationsConfig config)
        {
            var violations = new List<string>();

            // Check for prohibited custom implementation names
            foreach (var prohibitedName in config.ProhibitedNames)
            {
                var types = ArchTypes.InAssembly(assembly)
                    .That()
                    .HaveNameContaining(prohibitedName, StringComparison.OrdinalIgnoreCase)
                    .GetTypes().ToArray();

                foreach (var type in types)
                {
                    violations.Add($"Assembly {assembly.GetName().Name}: Custom implementation {type.Name} is prohibited. {config.Message}");
                }
            }

            return violations;
        }

        private static List<string> CheckEngineDependencyViolations(Assembly assembly, EngineDependenciesConfig config)
        {
            var violations = new List<string>();

            // Check for engine dependencies in referenced assemblies
            var referencedAssemblies = assembly.GetReferencedAssemblies();
            
            foreach (var prohibitedDep in config.ProhibitedDependencies)
            {
                var engineReferences = referencedAssemblies
                    .Where(ra => ra.Name.Contains(prohibitedDep, StringComparison.OrdinalIgnoreCase))
                    .ToArray();

                foreach (var engineRef in engineReferences)
                {
                    // Skip allowed exceptions
                    if (config.AllowedExceptions.Any(exception => 
                        engineRef.Name.Contains(exception, StringComparison.OrdinalIgnoreCase)))
                        continue;

                    violations.Add($"Assembly {assembly.GetName().Name} has prohibited engine dependency: {engineRef.Name}. {config.Message}");
                }
            }

            return violations;
        }
    }
}
