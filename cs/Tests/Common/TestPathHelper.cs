using System;
using System.IO;

namespace BarkMoon.GameComposition.Tests.Common
{
    /// <summary>
    /// Single Source of Truth (SSOT) for test path resolution and directory navigation patterns.
    /// Eliminates hardcoded path duplication across test files.
    /// </summary>
    public static class TestPathHelper
    {
        /// <summary>
        /// Gets the base directory for plugin assemblies.
        /// Uses consistent path resolution across all test files.
        /// </summary>
        /// <returns>Full path to the plugins directory</returns>
        public static string GetPluginBaseDirectory()
        {
            // Navigate from test output directory to plugins directory
            var currentDir = Directory.GetCurrentDirectory();
            // Current: G:\dev\game\plugins\framework\GameComposition\cs\Tests\Architectural\bin\ArchitecturalTests\net9.0
            // Target:   G:\dev\game\plugins
            return Path.GetFullPath(Path.Combine(currentDir, "..", "..", "..", "..", "..", ".."));
        }

        /// <summary>
        /// Gets the test output directory (current directory).
        /// </summary>
        /// <returns>Current test directory</returns>
        public static string GetTestDirectory() => Directory.GetCurrentDirectory();

        /// <summary>
        /// Gets the framework base directory.
        /// </summary>
        /// <returns>Full path to the framework directory</returns>
        public static string GetFrameworkDirectory()
        {
            return Path.Combine(GetPluginBaseDirectory(), "framework");
        }

        /// <summary>
        /// Gets the gameplay base directory.
        /// </summary>
        /// <returns>Full path to the gameplay directory</returns>
        public static string GetGameplayDirectory()
        {
            return Path.Combine(GetPluginBaseDirectory(), "gameplay");
        }

        /// <summary>
        /// Gets the GameComposition Core build directory.
        /// </summary>
        /// <param name="configuration">Build configuration (default: Debug)</param>
        /// <param name="targetFramework">Target framework (default: net9.0)</param>
        /// <returns>Full path to the GameComposition.Core build directory</returns>
        public static string GetGameCompositionCoreBuildDirectory(string configuration = "Debug", string targetFramework = "net9.0")
        {
            return Path.Combine(
                GetFrameworkDirectory(),
                "GameComposition", "cs", "Core", "bin", 
                configuration, targetFramework
            );
        }

        /// <summary>
        /// Gets the GridPlacement Core build directory.
        /// </summary>
        /// <param name="configuration">Build configuration (default: Debug)</param>
        /// <param name="targetFramework">Target framework (default: net9.0)</param>
        /// <returns>Full path to the GridPlacement.Core build directory</returns>
        public static string GetGridPlacementCoreBuildDirectory(string configuration = "Debug", string targetFramework = "net9.0")
        {
            return Path.Combine(
                GetGameplayDirectory(),
                "GridPlacement", "cs", "Core", "bin", 
                configuration, targetFramework
            );
        }

        /// <summary>
        /// Gets the GridPlacement Godot build directory.
        /// </summary>
        /// <param name="configuration">Build configuration (default: Debug)</param>
        /// <param name="targetFramework">Target framework (default: net9.0)</param>
        /// <returns>Full path to the GridPlacement.Godot build directory</returns>
        public static string GetGridPlacementGodotBuildDirectory(string configuration = "Debug", string targetFramework = "net9.0")
        {
            return Path.Combine(
                GetGameplayDirectory(),
                "GridPlacement", "cs", "Godot", "bin", 
                configuration, targetFramework
            );
        }

        /// <summary>
        /// Gets the full path to a specific assembly file.
        /// </summary>
        /// <param name="assemblyName">Name of the assembly (e.g., "GameComposition.Core")</param>
        /// <param name="configuration">Build configuration (default: Debug)</param>
        /// <param name="targetFramework">Target framework (default: net9.0)</param>
        /// <returns>Full path to the assembly file</returns>
        public static string GetAssemblyPath(string assemblyName, string configuration = "Debug", string targetFramework = "net9.0")
        {
            var assemblyFileName = $"BarkMoon.{assemblyName}.dll";
            
            return assemblyName switch
            {
                "GameComposition.Core" => Path.Combine(GetGameCompositionCoreBuildDirectory(configuration, targetFramework), assemblyFileName),
                "GridPlacement.Core" => Path.Combine(GetGridPlacementCoreBuildDirectory(configuration, targetFramework), assemblyFileName),
                "GridPlacement.Godot" => Path.Combine(GetGridPlacementGodotBuildDirectory(configuration, targetFramework), assemblyFileName),
                _ => throw new ArgumentException($"Unknown assembly name: {assemblyName}")
            };
        }

        /// <summary>
        /// Validates that a directory exists, creating it if specified.
        /// </summary>
        /// <param name="path">Directory path to validate</param>
        /// <param name="createIfMissing">Whether to create the directory if it doesn't exist</param>
        /// <returns>True if the directory exists or was created</returns>
        public static bool ValidateDirectory(string path, bool createIfMissing = false)
        {
            if (Directory.Exists(path))
                return true;

            if (createIfMissing)
            {
                Directory.CreateDirectory(path);
                return true;
            }

            return false;
        }

        /// <summary>
        /// Validates that a file exists.
        /// </summary>
        /// <param name="path">File path to validate</param>
        /// <returns>True if the file exists</returns>
        public static bool ValidateFile(string path) => File.Exists(path);
    }
}
