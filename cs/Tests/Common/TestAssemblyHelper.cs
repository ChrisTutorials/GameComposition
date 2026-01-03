using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;

namespace BarkMoon.GameComposition.Tests.Common
{
    /// <summary>
    /// Single Source of Truth (SSOT) for test assembly discovery and loading patterns.
    /// Eliminates duplication across architectural tests and provides consistent assembly loading.
    /// </summary>
    public static class TestAssemblyHelper
    {
        private static readonly Dictionary<AssemblyType, Assembly[]> _cachedAssemblies = new();

        /// <summary>
        /// Types of assembly collections for different test scenarios.
        /// </summary>
        public enum AssemblyType
        {
            /// <summary>
            /// Core assemblies only (GameComposition.Core, GridPlacement.Core)
            /// </summary>
            CoreOnly,
            
            /// <summary>
            /// Core + Godot assemblies for front-end node testing
            /// </summary>
            CoreWithGodot,
            
            /// <summary>
            /// All available assemblies for comprehensive cross-domain testing
            /// </summary>
            All
        }

        /// <summary>
        /// Gets cached assemblies for the specified type.
        /// </summary>
        /// <param name="assemblyType">Type of assembly collection to retrieve</param>
        /// <returns>Array of loaded assemblies</returns>
        public static Assembly[] GetAssemblies(AssemblyType assemblyType)
        {
            if (_cachedAssemblies.TryGetValue(assemblyType, out var cached))
                return cached;

            var assemblies = LoadAssemblies(assemblyType).ToArray();
            _cachedAssemblies[assemblyType] = assemblies;
            return assemblies;
        }

        /// <summary>
        /// Gets all relevant assemblies for architectural testing.
        /// </summary>
        /// <returns>Array of loaded assemblies</returns>
        public static Assembly[] GetAllRelevantAssemblies() => GetAssemblies(AssemblyType.All);

        /// <summary>
        /// Gets core assemblies only (excludes Godot assemblies).
        /// </summary>
        /// <returns>Array of core assemblies</returns>
        public static Assembly[] GetCoreAssemblies() => GetAssemblies(AssemblyType.CoreOnly);

        /// <summary>
        /// Gets assemblies including Godot assemblies for front-end testing.
        /// </summary>
        /// <returns>Array of assemblies including Godot</returns>
        public static Assembly[] GetAssembliesWithGodot() => GetAssemblies(AssemblyType.CoreWithGodot);

        /// <summary>
        /// Loads assemblies based on the specified type.
        /// </summary>
        /// <param name="assemblyType">Type of assemblies to load</param>
        /// <returns>Collection of loaded assemblies</returns>
        private static IEnumerable<Assembly> LoadAssemblies(AssemblyType assemblyType)
        {
            var configs = GetAssemblyConfigurations(assemblyType);
            var baseDir = TestPathHelper.GetPluginBaseDirectory();

            foreach (var config in configs)
            {
                var assemblyPath = Path.Combine(baseDir, config.RelativePath);
                if (File.Exists(assemblyPath))
                {
                    try
                    {
                        yield return Assembly.LoadFrom(assemblyPath);
                    }
                    catch (FileNotFoundException ex)
                    {
                        Console.WriteLine($"Warning: Failed to load assembly {config.Name} from {assemblyPath}: {ex.Message}");
                    }
                    catch (BadImageFormatException ex)
                    {
                        Console.WriteLine($"Warning: Assembly {config.Name} at {assemblyPath} is not a valid .NET assembly: {ex.Message}");
                    }
                }
                else
                {
                    Console.WriteLine($"Info: Assembly {config.Name} not found at {assemblyPath}");
                }
            }
        }

        /// <summary>
        /// Gets assembly configurations for the specified assembly type.
        /// </summary>
        /// <param name="assemblyType">Type of assemblies to configure</param>
        /// <returns>Array of assembly configurations</returns>
        private static AssemblyConfiguration[] GetAssemblyConfigurations(AssemblyType assemblyType)
        {
            return assemblyType switch
            {
                AssemblyType.CoreOnly => new[]
                {
                    new AssemblyConfiguration("GameComposition.Core", "framework/GameComposition/cs/Core/bin/Debug/net10.0/BarkMoon.GameComposition.Core.dll"),
                    new AssemblyConfiguration("GridPlacement.Core", "gameplay/GridPlacement/cs/Core/bin/Debug/net10.0/BarkMoon.GridPlacement.Core.dll")
                },
                
                AssemblyType.CoreWithGodot => new[]
                {
                    new AssemblyConfiguration("GameComposition.Core", "framework/GameComposition/cs/Core/bin/Debug/net10.0/BarkMoon.GameComposition.Core.dll"),
                    new AssemblyConfiguration("GridPlacement.Core", "gameplay/GridPlacement/cs/Core/bin/Debug/net10.0/BarkMoon.GridPlacement.Core.dll"),
                    new AssemblyConfiguration("GridPlacement.Godot", "gameplay/GridPlacement/cs/Godot/bin/Debug/net10.0/BarkMoon.GridPlacement.Godot.dll")
                },
                
                AssemblyType.All => new[]
                {
                    new AssemblyConfiguration("GameComposition.Core", "framework/GameComposition/cs/Core/bin/Debug/net10.0/BarkMoon.GameComposition.Core.dll"),
                    new AssemblyConfiguration("GridPlacement.Core", "gameplay/GridPlacement/cs/Core/bin/Debug/net10.0/BarkMoon.GridPlacement.Core.dll"),
                    new AssemblyConfiguration("GridPlacement.Godot", "gameplay/GridPlacement/cs/Godot/bin/Debug/net10.0/BarkMoon.GridPlacement.Godot.dll")
                    // Add more assemblies as they are created
                },
                
                _ => throw new ArgumentException($"Unknown assembly type: {assemblyType}")
            };
        }

        /// <summary>
        /// Configuration for a test assembly.
        /// </summary>
        /// <param name="name">Display name of the assembly</param>
        /// <param name="relativePath">Relative path from plugin base directory</param>
        private record AssemblyConfiguration(string Name, string RelativePath);
    }
}
