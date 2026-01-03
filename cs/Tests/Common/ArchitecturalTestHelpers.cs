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
        public static object ForAssembly(Assembly assembly)
        {
            return ArchTypes.InAssembly(assembly);
        }

        /// <summary>
        /// Creates NetArchTest rule starting points for all assemblies.
        /// </summary>
        /// <returns>Collection of rule builders for all assemblies</returns>
        public static IEnumerable<object> ForAllAssemblies()
        {
            return GetAllAssemblies().Select(assembly => ForAssembly(assembly));
        }

        /// <summary>
        /// Creates NetArchTest rule starting points for core assemblies only.
        /// </summary>
        /// <returns>Collection of rule builders for core assemblies</returns>
        public static IEnumerable<object> ForCoreAssemblies()
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
        /// Validates that all assemblies follow the specified namespace convention.
        /// </summary>
        /// <param name="namespacePrefix">Required namespace prefix</param>
        /// <param name="assemblies">Assemblies to validate (null for all)</param>
        public static void ValidateNamespaceConvention(string namespacePrefix, Assembly[]? assemblies = null)
        {
            var targetAssemblies = assemblies ?? GetAllAssemblies();
            var results = targetAssemblies.Select(assembly => 
                ForAssembly(assembly)
                    .Should()
                    .ResideInNamespaceStartingWith(namespacePrefix)
                    .GetResult()
            ).ToList();

            foreach (var (result, index) in results.Select((r, i) => (r, i)))
            {
                result.IsSuccessful.ShouldBeTrue(
                    $"Assembly {targetAssemblies[index].GetName().Name} should follow {namespacePrefix} namespace convention");
            }
        }

        /// <summary>
        /// Validates that core assemblies do not depend on Godot assemblies.
        /// </summary>
        public static void ValidateCoreAssembliesDoNotDependOnGodot()
        {
            var coreAssemblies = GetCoreAssemblies();
            var godotAssemblies = GetAssembliesWithGodot().Except(coreAssemblies).ToArray();

            foreach (var coreAssembly in coreAssemblies)
            {
                var result = ForAssembly(coreAssembly)
                    .Should()
                    .NotHaveDependencyOnAny(godotAssemblies.Select(a => a.GetName().Name).ToArray())
                    .GetResult();

                result.IsSuccessful.ShouldBeTrue(
                    $"Core assembly {coreAssembly.GetName().Name} should not depend on Godot assemblies");
            }
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
        /// Asserts that a condition is true with a descriptive error message.
        /// </summary>
        /// <param name="condition">Condition to validate</param>
        /// <param name="message">Error message if condition is false</param>
        public static void AssertArchitecturalRule(bool condition, string message)
        {
            condition.ShouldBeTrue(message);
        }
    }
}
