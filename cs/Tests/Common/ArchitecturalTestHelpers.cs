using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using BarkMoon.GameComposition.Tests.Common;

// Explicitly alias the NetArchTest Types class to avoid conflict
using ArchTypes = NetArchTest.Rules;

namespace BarkMoon.GameComposition.Tests.Common
{
    /// <summary>
    /// Static utility helpers for architectural tests.
    /// Provides common patterns and setup without requiring inheritance.
    /// Follows DRY principles and enables composition over inheritance.
    /// </summary>
    public static class ArchitecturalTestHelpers
    {
        /// <summary>
        /// Gets all relevant assemblies for architectural testing.
        /// </summary>
        public static Assembly[] GetAllAssemblies() => TestAssemblyHelper.GetAllRelevantAssemblies();

        /// <summary>
        /// Gets all relevant assemblies for architectural testing (alias).
        /// </summary>
        public static Assembly[] GetAllRelevantAssemblies() => TestAssemblyHelper.GetAllRelevantAssemblies();

        /// <summary>
        /// Gets core assemblies only (excludes Godot assemblies).
        /// </summary>
        public static Assembly[] GetCoreAssemblies() => TestAssemblyHelper.GetCoreAssemblies();

        /// <summary>
        /// Gets assemblies including Godot assemblies for front-end testing.
        /// </summary>
        public static Assembly[] GetAssembliesWithGodot() => TestAssemblyHelper.GetAssembliesWithGodot();

        /// <summary>
        /// Creates a NetArchTest rule starting point for the specified assembly.
        /// </summary>
        /// <param name="assembly">Assembly to create rule for</param>
        /// <returns>NetArchTest rule builder</returns>
        public static ArchTypes.Types ForAssembly(Assembly assembly)
        {
            return ArchTypes.Types.InAssembly(assembly);
        }

        /// <summary>
        /// Creates NetArchTest rule starting points for all assemblies.
        /// </summary>
        /// <returns>Collection of rule builders for all assemblies</returns>
        public static IEnumerable<ArchTypes.Types> ForAllAssemblies()
        {
            return GetAllAssemblies().Select(assembly => ForAssembly(assembly));
        }

        /// <summary>
        /// Creates NetArchTest rule starting points for core assemblies only.
        /// </summary>
        /// <returns>Collection of rule builders for core assemblies</returns>
        public static IEnumerable<ArchTypes.Types> ForCoreAssemblies()
        {
            return GetCoreAssemblies().Select(assembly => ForAssembly(assembly));
        }

        /// <summary>
        /// Finds all types in all assemblies that match the specified predicate.
        /// </summary>
        /// <param name="typePredicate">Predicate to match types</param>
        /// <returns>Collection of matching types</returns>
        public static IEnumerable<Type> FindTypesInAllAssemblies(Func<Type, bool> typePredicate)
        {
            return GetAllAssemblies().SelectMany(assembly => assembly.GetTypes().Where(typePredicate));
        }

        /// <summary>
        /// Finds all types in core assemblies that match the specified predicate.
        /// </summary>
        /// <param name="typePredicate">Predicate to match types</param>
        /// <returns>Collection of matching types</returns>
        public static IEnumerable<Type> FindTypesInCoreAssemblies(Func<Type, bool> typePredicate)
        {
            return GetCoreAssemblies().SelectMany(assembly => assembly.GetTypes().Where(typePredicate));
        }

        /// <summary>
        /// Finds all types with names ending with the specified suffix.
        /// </summary>
        /// <param name="suffix">Suffix to match</param>
        /// <param name="includeAbstract">Whether to include abstract types</param>
        /// <returns>Collection of matching types</returns>
        public static IEnumerable<Type> FindTypesWithSuffix(string suffix, bool includeAbstract = false)
        {
            return FindTypesInAllAssemblies(type => 
                type.Name.EndsWith(suffix, StringComparison.OrdinalIgnoreCase) &&
                (includeAbstract || !type.IsAbstract) &&
                type.IsClass);
        }

        /// <summary>
        /// Finds all types with names starting with the specified prefix.
        /// </summary>
        /// <param name="prefix">Prefix to match</param>
        /// <param name="includeAbstract">Whether to include abstract types</param>
        /// <returns>Collection of matching types</returns>
        public static IEnumerable<Type> FindTypesWithPrefix(string prefix, bool includeAbstract = false)
        {
            return FindTypesInAllAssemblies(type => 
                type.Name.StartsWith(prefix, StringComparison.OrdinalIgnoreCase) &&
                (includeAbstract || !type.IsAbstract) &&
                type.IsClass);
        }

        /// <summary>
        /// Finds all types that implement the specified interface.
        /// </summary>
        /// <typeparam name="TInterface">Interface type to find implementations for</typeparam>
        /// <param name="includeAbstract">Whether to include abstract implementations</param>
        /// <returns>Collection of types implementing the interface</returns>
        public static IEnumerable<Type> FindTypesImplementing<TInterface>(bool includeAbstract = false)
        {
            var interfaceType = typeof(TInterface);
            return FindTypesInAllAssemblies(type => 
                interfaceType.IsAssignableFrom(type) &&
                type != interfaceType &&
                (includeAbstract || !type.IsAbstract) &&
                type.IsClass);
        }

        /// <summary>
        /// Gets a descriptive name for an assembly.
        /// </summary>
        /// <param name="assembly">Assembly to get name for</param>
        /// <returns>Descriptive assembly name</returns>
        public static string GetAssemblyDisplayName(Assembly assembly)
        {
            var name = assembly.GetName().Name;
            return name.Replace("BarkMoon.", "").Replace(".Core", "").Replace(".Godot", "");
        }

        /// <summary>
        /// Loads YAML configuration for architectural tests.
        /// </summary>
        /// <param name="configFileName">Configuration file name</param>
        /// <returns>Architecture configuration</returns>
        public static ArchitectureConfiguration LoadYamlConfig(string configFileName = "architecture-config.yaml")
        {
            return ArchitectureConfigurationLoader.LoadConfiguration();
        }

        /// <summary>
        /// Finds an interface by name across assemblies.
        /// </summary>
        /// <param name="assemblies">Assemblies to search</param>
        /// <param name="interfaceName">Interface name to find</param>
        /// <param name="namespaceFilter">Optional namespace filter</param>
        /// <returns>Interface type or null if not found</returns>
        public static Type? FindInterfaceByName(Assembly[] assemblies, string interfaceName, string? namespaceFilter = null)
        {
            return assemblies
                .SelectMany(assembly => assembly.GetTypes())
                .FirstOrDefault(type => 
                    type.IsInterface && 
                    type.Name == interfaceName &&
                    (namespaceFilter == null || type.Namespace?.Contains(namespaceFilter) == true));
        }

        /// <summary>
        /// Finds a class by name across assemblies.
        /// </summary>
        /// <param name="assemblies">Assemblies to search</param>
        /// <param name="className">Class name to find</param>
        /// <param name="namespaceFilter">Optional namespace filter</param>
        /// <returns>Class type or null if not found</returns>
        public static Type? FindClassByName(Assembly[] assemblies, string className, string? namespaceFilter = null)
        {
            return assemblies
                .SelectMany(assembly => assembly.GetTypes())
                .FirstOrDefault(type => 
                    !type.IsInterface && 
                    type.Name == className &&
                    (namespaceFilter == null || type.Namespace?.Contains(namespaceFilter) == true));
        }

        /// <summary>
        /// Finds classes by pattern across assemblies.
        /// </summary>
        /// <param name="assemblies">Assemblies to search</param>
        /// <param name="pattern">Pattern to match</param>
        /// <param name="namespaceFilter">Optional namespace filter</param>
        /// <returns>Collection of matching classes</returns>
        public static IEnumerable<Type> FindClassesByPattern(Assembly[] assemblies, string pattern, string? namespaceFilter = null)
        {
            return assemblies
                .SelectMany(assembly => assembly.GetTypes())
                .Where(type => 
                    !type.IsInterface && 
                    type.Name.EndsWith(pattern.Replace("*", ""), StringComparison.OrdinalIgnoreCase) &&
                    (namespaceFilter == null || type.Namespace?.Contains(namespaceFilter) == true));
        }

        /// <summary>
        /// Finds types by pattern across assemblies.
        /// </summary>
        /// <param name="assemblies">Assemblies to search</param>
        /// <param name="patterns">Patterns to match</param>
        /// <param name="namespaceFilter">Optional namespace filter</param>
        /// <returns>Collection of matching types</returns>
        public static IEnumerable<Type> FindTypesByPattern(Assembly[] assemblies, string[] patterns, string? namespaceFilter = null)
        {
            return assemblies
                .SelectMany(assembly => assembly.GetTypes())
                .Where(type => 
                    patterns.Any(pattern => type.Name.EndsWith(pattern.Replace("*", ""), StringComparison.OrdinalIgnoreCase)) &&
                    (namespaceFilter == null || type.Namespace?.Contains(namespaceFilter) == true));
        }

        /// <summary>
        /// Checks if a type inherits from a specified interface.
        /// </summary>
        /// <param name="type">Type to check</param>
        /// <param name="interfaceName">Interface name to check for</param>
        /// <returns>True if type inherits from interface</returns>
        public static bool CheckInheritance(Type type, string interfaceName)
        {
            return type.GetInterfaces().Any(iface => iface.Name == interfaceName);
        }

        /// <summary>
        /// Checks if a type implements a specified interface.
        /// </summary>
        /// <param name="type">Type to check</param>
        /// <param name="interfaceName">Interface name to check for</param>
        /// <returns>True if type implements interface</returns>
        public static bool CheckInterfaceImplementation(Type type, string interfaceName)
        {
            return type.GetInterfaces().Any(iface => iface.Name == interfaceName);
        }

        /// <summary>
        /// Checks inheritance violations for a collection of types.
        /// </summary>
        /// <param name="types">Types to check</param>
        /// <param name="requiredBaseType">Required base type name</param>
        /// <returns>Collection of violation messages</returns>
        public static List<string> CheckInheritanceViolations(IEnumerable<Type> types, string requiredBaseType)
        {
            var violations = new List<string>();
            
            foreach (var type in types)
            {
                var baseTypeName = type.BaseType?.Name;
                if (baseTypeName != requiredBaseType)
                {
                    violations.Add($"{type.Name} inherits from {baseTypeName ?? "null"} instead of {requiredBaseType}");
                }
            }
            
            return violations;
        }

        /// <summary>
        /// Checks naming violations for a collection of types.
        /// </summary>
        /// <param name="types">Types to check</param>
        /// <param name="namingConfig">Naming configuration</param>
        /// <returns>Collection of violation messages</returns>
        public static List<string> CheckNamingViolations(IEnumerable<Type> types, object namingConfig)
        {
            var violations = new List<string>();
            
            foreach (var type in types)
            {
                // Interfaces should start with 'I'
                if (type.IsInterface && !type.Name.StartsWith("I"))
                {
                    violations.Add($"{type.Name} - Interface should start with 'I'");
                }

                // Classes should not start with 'I' (except ValidationResult)
                if (!type.IsInterface && type.Name.StartsWith("I") && type.Name != "ValidationResult")
                {
                    violations.Add($"{type.Name} - Class should not start with 'I'");
                }

                // Result classes should end with 'Result' (operation result pattern)
                if (!type.IsInterface && type.Name.EndsWith("Report") && type.Name != "PlacementReport")
                {
                    violations.Add($"{type.Name} - Report classes should follow operation result pattern with 'Result' suffix");
                }
            }
            
            return violations;
        }
    }
}
