using System.IO;
using System.Linq;
using System.Xml.Linq;
using Shouldly;
using Xunit;
using Xunit.Abstractions;

namespace BarkMoon.GameComposition.Tests.Architectural
{
    /// <summary>
    /// SINGLE SOURCE OF TRUTH for dependency architecture validation.
    /// Enforces package version consistency, prevents duplicates, and validates architectural boundaries.
    /// Replaces: DynamicDependencyVersionConsistencyTests, SafeDependencyVersionConsistencyTests, RuntimeDependencyArchitectureTests
    /// </summary>
    public class SolutionDependencyArchitectureTests
    {
        private readonly string _solutionRoot = "g:\\dev\\game";
        private readonly ITestOutputHelper _output;

        // Single Source of Truth - Approved Package Versions
        private static readonly Dictionary<string, string> ApprovedVersions = new()
        {
            // Core Testing Infrastructure
            ["Microsoft.NET.Test.Sdk"] = "17.14.1",
            ["xunit"] = "2.9.3",
            ["xunit.runner.visualstudio"] = "3.1.4",
            ["Shouldly"] = "4.2.1",
            ["NetArchTest.Rules"] = "1.3.2",
            ["Moq"] = "4.20.69",
            
            // Code Coverage
            ["coverlet.collector"] = "6.0.4",
            ["Coverlet.msbuild"] = "6.0.2",
            
            // Architectural Testing
            ["YamlDotNet"] = "13.7.1",
            
            // Microsoft Extensions (Infrastructure)
            ["Microsoft.Extensions.ObjectPool"] = "10.0.1",
            ["Microsoft.Extensions.Caching.Memory"] = "10.0.1",
            ["Microsoft.Extensions.DependencyInjection"] = "10.0.1",
            ["Microsoft.Extensions.DependencyInjection.Abstractions"] = "10.0.1",
            ["Microsoft.Extensions.Configuration"] = "10.0.1",
            ["Microsoft.Extensions.Configuration.Abstractions"] = "10.0.1",
            ["Microsoft.Extensions.Logging"] = "10.0.1",
            ["Microsoft.Extensions.Logging.Abstractions"] = "10.0.1",
            ["Microsoft.Extensions.Options"] = "10.0.1",
            ["Microsoft.Extensions.Options.ConfigurationExtensions"] = "10.0.1",
            ["Microsoft.Extensions.Hosting"] = "10.0.1",
            ["Microsoft.Extensions.Hosting.Abstractions"] = "10.0.1"
        };

        // Deprecated versions to forbid
        private static readonly Dictionary<string, string[]> DeprecatedVersions = new()
        {
            ["xunit"] = new[] { "2.6.2", "2.9.2" },
            ["xunit.runner.visualstudio"] = new[] { "2.5.3", "2.8.2" },
            ["Microsoft.NET.Test.Sdk"] = new[] { "17.8.0", "17.11.1" },
            ["coverlet.collector"] = new[] { "6.0.2" }
        };

        public SolutionDependencyArchitectureTests(ITestOutputHelper output)
        {
            _output = output;
        }

        [Fact(DisplayName = "SOL-001: Solution Should Not Have Duplicate Package References Across Projects")]
        [Trait("Category", "Architectural")]
        public void Solution_Should_Not_Have_Duplicate_Package_References_Across_Projects()
        {
            // Arrange
            var projectFiles = GetKnownProjectPaths();
            var allPackages = new Dictionary<string, List<string>>();
            var violations = new List<string>();

            _output.WriteLine($"✅ Analyzing {projectFiles.Count} known projects for package consistency");

            // Act - Collect all package references across all projects
            foreach (var projectFile in projectFiles)
            {
                if (!File.Exists(projectFile))
                {
                    _output.WriteLine($"⚠️ Skipping missing project: {projectFile}");
                    continue;
                }

                try
                {
                    var projectContent = File.ReadAllText(projectFile);
                    var projectXml = XDocument.Parse(projectContent);
                    
                    var packages = projectXml.Descendants("PackageReference")
                        .Where(e => e.Attribute("Include") != null)
                        .Select(e => new 
                        { 
                            Name = e.Attribute("Include")?.Value,
                            Version = e.Attribute("Version")?.Value,
                            Project = Path.GetFileName(projectFile)
                        })
                        .ToList();

                    foreach (var package in packages)
                    {
                        if (!allPackages.ContainsKey(package.Name))
                            allPackages[package.Name] = new List<string>();
                        
                        allPackages[package.Name].Add($"{package.Project} ({package.Version ?? "no-version"})");
                    }
                }
                catch (Exception ex)
                {
                    _output.WriteLine($"⚠️ Error parsing {projectFile}: {ex.Message}");
                    continue;
                }
            }

            // Check for version inconsistencies
            foreach (var packageGroup in allPackages)
            {
                var versions = packageGroup.Value
                    .Select(p => p.Contains('(') ? p.Split('(').Last().Trim(')') : "no-version")
                    .Distinct()
                    .ToList();

                if (versions.Count > 1)
                {
                    violations.Add($"Package '{packageGroup.Key}' has inconsistent versions: {string.Join(", ", versions)}");
                }
            }

            // Check for packages used in many projects (might indicate central management needed)
            var widelyUsedPackages = allPackages
                .Where(kvp => kvp.Value.Count > 3)
                .Select(kvp => $"Package '{kvp.Key}' used in {kvp.Value.Count} projects: {string.Join(", ", kvp.Value.Take(3))}...")
                .ToList();

            // Report findings
            if (widelyUsedPackages.Any())
            {
                _output.WriteLine("📊 Widely used packages (candidates for central management):");
                foreach (var warning in widelyUsedPackages)
                {
                    _output.WriteLine($"  - {warning}");
                }
            }

            // Assert - No version inconsistencies allowed
            violations.ShouldBeEmpty($"Found package version inconsistencies: {string.Join(", ", violations)}");
        }

        [Fact(DisplayName = "SOL-002: All Projects Should Use Approved Infrastructure Versions")]
        [Trait("Category", "Architectural")]
        public void All_Projects_Should_Use_Approved_Infrastructure_Versions()
        {
            // Arrange
            var projectFiles = GetKnownProjectPaths();
            var violations = new List<string>();

            _output.WriteLine($"✅ Validating infrastructure versions across {projectFiles.Count} projects");

            // Act - Check each project against approved versions
            foreach (var projectFile in projectFiles)
            {
                if (!File.Exists(projectFile)) continue;

                try
                {
                    var projectContent = File.ReadAllText(projectFile);
                    var projectXml = XDocument.Parse(projectContent);
                    
                    var packages = projectXml.Descendants("PackageReference")
                        .Where(e => e.Attribute("Include") != null && e.Attribute("Version") != null)
                        .ToDictionary(
                            e => e.Attribute("Include")?.Value ?? "",
                            e => e.Attribute("Version")?.Value ?? ""
                        );

                    foreach (var package in packages)
                    {
                        if (ApprovedVersions.TryGetValue(package.Key, out var approvedVersion))
                        {
                            if (package.Value != approvedVersion)
                            {
                                violations.Add($"{Path.GetFileName(projectFile)}: {package.Key} = {package.Value} (should be {approvedVersion})");
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    _output.WriteLine($"⚠️ Error analyzing {projectFile}: {ex.Message}");
                }
            }

            // Report
            _output.WriteLine($"Found {violations.Count} version violations");
            foreach (var violation in violations.Take(10))
            {
                _output.WriteLine($"  ❌ {violation}");
            }
            if (violations.Count > 10)
            {
                _output.WriteLine($"  ... and {violations.Count - 10} more violations");
            }

            // Assert
            violations.ShouldBeEmpty("All projects should use approved infrastructure versions");
        }

        [Fact(DisplayName = "SOL-003: No Deprecated Package Versions Should Exist")]
        [Trait("Category", "Architectural")]
        public void No_Deprecated_Package_Versions_Should_Exist()
        {
            // Arrange
            var projectFiles = GetKnownProjectPaths();
            var deprecatedViolations = new List<string>();

            _output.WriteLine($"✅ Checking for deprecated versions in {projectFiles.Count} projects");

            // Act
            foreach (var projectFile in projectFiles)
            {
                if (!File.Exists(projectFile)) continue;

                try
                {
                    var projectContent = File.ReadAllText(projectFile);
                    var projectXml = XDocument.Parse(projectContent);
                    
                    var packages = projectXml.Descendants("PackageReference")
                        .Where(e => e.Attribute("Include") != null && e.Attribute("Version") != null)
                        .ToDictionary(
                            e => e.Attribute("Include")?.Value ?? "",
                            e => e.Attribute("Version")?.Value ?? ""
                        );
                    
                    foreach (var package in packages)
                    {
                        if (DeprecatedVersions.TryGetValue(package.Key, out var deprecated))
                        {
                            if (deprecated.Contains(package.Value))
                            {
                                var approvedVersion = ApprovedVersions[package.Key];
                                deprecatedViolations.Add($"{Path.GetFileName(projectFile)}: {package.Key} = {package.Value} (deprecated, should be {approvedVersion})");
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    _output.WriteLine($"⚠️ Error checking deprecated versions in {projectFile}: {ex.Message}");
                }
            }

            // Report
            _output.WriteLine($"Found {deprecatedViolations.Count} deprecated package violations");
            foreach (var violation in deprecatedViolations)
            {
                _output.WriteLine($"  ⚠️ {violation}");
            }

            // Assert
            deprecatedViolations.ShouldBeEmpty("No deprecated package versions should exist");
        }

        [Fact(DisplayName = "SOL-004: Solution Should Not Have Cyclical Dependencies Between Plugins")]
        [Trait("Category", "Architectural")]
        public void Solution_Should_Not_Have_Cyclical_Dependencies_Between_Plugins()
        {
            // Arrange
            var projectFiles = GetKnownProjectPaths();
            var projectReferences = new Dictionary<string, List<string>>();
            var pluginGroups = new Dictionary<string, List<string>>();
            
            // Build reference graph and group by plugin
            foreach (var projectFile in projectFiles)
            {
                var projectName = Path.GetFileNameWithoutExtension(projectFile);
                var pluginName = ExtractPluginName(projectFile);
                
                if (!pluginGroups.ContainsKey(pluginName))
                    pluginGroups[pluginName] = new List<string>();
                pluginGroups[pluginName].Add(projectName);

                try
                {
                    var projectContent = File.ReadAllText(projectFile);
                    var projectXml = XDocument.Parse(projectContent);
                    
                    var references = projectXml.Descendants("ProjectReference")
                        .Where(e => e.Attribute("Include") != null)
                        .Select(e => Path.GetFileNameWithoutExtension(e.Attribute("Include")?.Value))
                        .ToList();
                    
                    projectReferences[projectName] = references;
                }
                catch (Exception ex)
                {
                    _output.WriteLine($"⚠️ Error parsing references for {projectFile}: {ex.Message}");
                    continue;
                }
            }

            // Act - Detect cycles between plugins
            var pluginDependencies = new Dictionary<string, List<string>>();
            foreach (var plugin in pluginGroups.Keys)
            {
                var dependencies = new HashSet<string>();
                foreach (var project in pluginGroups[plugin])
                {
                    if (projectReferences.ContainsKey(project))
                    {
                        foreach (var reference in projectReferences[project])
                        {
                            var refPlugin = ExtractPluginNameFromProject(reference);
                            if (refPlugin != plugin && !string.IsNullOrEmpty(refPlugin))
                            {
                                dependencies.Add(refPlugin);
                            }
                        }
                    }
                }
                pluginDependencies[plugin] = dependencies.ToList();
            }

            // Check for cycles between plugins
            var visited = new HashSet<string>();
            var recursionStack = new HashSet<string>();
            var cycles = new List<string>();

            foreach (var plugin in pluginDependencies.Keys)
            {
                if (HasCycle(plugin, pluginDependencies, visited, recursionStack))
                {
                    cycles.Add(plugin);
                }
            }

            // Assert
            cycles.ShouldBeEmpty($"Found cyclical dependencies between plugins: {string.Join(", ", cycles)}");
        }

        [Fact(DisplayName = "SOL-005: Plugins Should Not Depend On Implementation Details Of Other Plugins")]
        [Trait("Category", "Architectural")]
        public void Plugins_Should_Not_Depend_On_Implementation_Details_Of_Other_Plugins()
        {
            // Arrange
            var projectFiles = GetKnownProjectPaths();
            var violations = new List<string>();

            // Act - Check for cross-plugin implementation dependencies
            foreach (var projectFile in projectFiles)
            {
                var projectName = Path.GetFileNameWithoutExtension(projectFile);
                var currentPlugin = ExtractPluginName(projectFile);
                
                try
                {
                    var projectContent = File.ReadAllText(projectFile);
                    var projectXml = XDocument.Parse(projectContent);
                    
                    var references = projectXml.Descendants("ProjectReference")
                        .Where(e => e.Attribute("Include") != null)
                        .Select(e => new 
                        { 
                            Project = e.Attribute("Include")?.Value,
                            Name = Path.GetFileNameWithoutExtension(e.Attribute("Include")?.Value)
                        })
                        .ToList();

                    foreach (var reference in references)
                    {
                        var refPlugin = ExtractPluginNameFromProject(reference.Name);
                        
                        // Check if referencing implementation details of another plugin
                        if (refPlugin != currentPlugin && !string.IsNullOrEmpty(refPlugin))
                        {
                            // Check if it's a Core reference (allowed) vs Implementation (not allowed)
                            if (!reference.Name.EndsWith(".Core") && !reference.Name.Contains("Tests"))
                            {
                                violations.Add($"{projectName} ({currentPlugin}) references implementation of {refPlugin}: {reference.Name}");
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    _output.WriteLine($"⚠️ Error checking implementation dependencies for {projectFile}: {ex.Message}");
                    continue;
                }
            }

            // Assert
            violations.ShouldBeEmpty($"Found plugins depending on implementation details of other plugins: {string.Join(", ", violations)}");
        }

        [Fact(DisplayName = "SOL-006: Framework Projects Should Not Depend On Gameplay Plugins")]
        [Trait("Category", "Architectural")]
        public void Framework_Projects_Should_Not_Depend_On_Gameplay_Plugins()
        {
            // Arrange
            var projectFiles = GetKnownProjectPaths();
            var violations = new List<string>();

            // Act - Check for framework -> gameplay dependencies
            foreach (var projectFile in projectFiles)
            {
                if (!projectFile.Contains("framework")) continue;

                var projectName = Path.GetFileNameWithoutExtension(projectFile);
                
                try
                {
                    var projectContent = File.ReadAllText(projectFile);
                    var projectXml = XDocument.Parse(projectContent);
                    
                    var references = projectXml.Descendants("ProjectReference")
                        .Where(e => e.Attribute("Include") != null)
                        .Select(e => e.Attribute("Include")?.Value)
                        .Where(r => r.Contains("gameplay"))
                        .ToList();

                    foreach (var reference in references)
                    {
                        violations.Add($"{projectName} (framework) references gameplay plugin: {reference}");
                    }
                }
                catch (Exception ex)
                {
                    _output.WriteLine($"⚠️ Error checking framework dependencies for {projectFile}: {ex.Message}");
                    continue;
                }
            }

            // Assert
            violations.ShouldBeEmpty($"Found framework projects depending on gameplay plugins: {string.Join(", ", violations)}");
        }

        [Fact(DisplayName = "SOL-007: Test Projects Should Not Reference Other Test Projects")]
        [Trait("Category", "Architectural")]
        public void Test_Projects_Should_Not_Reference_Other_Test_Projects()
        {
            // Arrange
            var projectFiles = GetKnownProjectPaths();
            var violations = new List<string>();

            // Act - Check for test -> test dependencies
            foreach (var projectFile in projectFiles)
            {
                var projectName = Path.GetFileNameWithoutExtension(projectFile);
                if (!projectName.Contains("Tests")) continue;

                try
                {
                    var projectContent = File.ReadAllText(projectFile);
                    var projectXml = XDocument.Parse(projectContent);
                    
                    var testReferences = projectXml.Descendants("ProjectReference")
                        .Where(e => e.Attribute("Include") != null)
                        .Select(e => e.Attribute("Include")?.Value)
                        .Where(r => r.Contains("Tests"))
                        .ToList();

                    foreach (var reference in testReferences)
                    {
                        violations.Add($"{projectName} references other test project: {reference}");
                    }
                }
                catch (Exception ex)
                {
                    _output.WriteLine($"⚠️ Error checking test references for {projectFile}: {ex.Message}");
                    continue;
                }
            }

            // Assert
            violations.ShouldBeEmpty($"Found test projects referencing other test projects: {string.Join(", ", violations)}");
        }

        #region Safe Helper Methods (No External Processes)

        private List<string> GetKnownProjectPaths()
        {
            // SAFE: Hardcoded known project paths - no recursive filesystem scanning
            return new List<string>
            {
                // Framework Projects
                @"g:\dev\game\plugins\framework\GameComposition\cs\Core\GameComposition.Core.csproj",
                @"g:\dev\game\plugins\framework\GameComposition\cs\Godot\GameComposition.Godot.csproj",
                @"g:\dev\game\plugins\framework\GameComposition\cs\Tests\GameComposition.Core.Tests.csproj",
                @"g:\dev\game\plugins\framework\GameComposition\cs\Tests\GameComposition.ArchitecturalTests.csproj",
                @"g:\dev\game\plugins\framework\GameComposition\cs\Tests\GameComposition.PerformanceTests.csproj",
                
                // GridPlacement Projects
                @"g:\dev\game\plugins\gameplay\GridPlacement\cs\Core\GridPlacement.Core.csproj",
                @"g:\dev\game\plugins\gameplay\GridPlacement\cs\Tests\GridPlacement.Core.Tests.csproj",
                @"g:\dev\game\plugins\gameplay\GridPlacement\cs\Tools\ArchitectureToolkit\ArchitectureToolkit.csproj",
                
                // Demo Projects
                @"g:\dev\game\demos\grid_building_dev\godot_cs\GridBuilding.Demo.CSharp.Tests.csproj"
            };
        }

        #endregion

        #region Helper Methods

        private string ExtractPluginName(string projectPath)
        {
            var parts = projectPath.Split(Path.DirectorySeparatorChar);
            var pluginsIndex = Array.IndexOf(parts, "plugins");
            
            if (pluginsIndex >= 0 && pluginsIndex + 2 < parts.Length)
            {
                return parts[pluginsIndex + 2]; // plugins/framework/GameComposition or plugins/gameplay/GridPlacement
            }
            
            var demosIndex = Array.IndexOf(parts, "demos");
            if (demosIndex >= 0 && demosIndex + 1 < parts.Length)
            {
                return parts[demosIndex + 1]; // demos/grid_building_dev
            }
            
            return "Unknown";
        }

        private string ExtractPluginNameFromProject(string projectName)
        {
            // Extract plugin name from project name
            if (projectName.Contains("GameComposition")) return "GameComposition";
            if (projectName.Contains("GridPlacement")) return "GridPlacement";
            if (projectName.Contains("GameUserSessions")) return "GameUserSessions";
            
            return "Unknown";
        }

        private bool HasCycle(string plugin, Dictionary<string, List<string>> dependencies, 
                            HashSet<string> visited, HashSet<string> recursionStack)
        {
            visited.Add(plugin);
            recursionStack.Add(plugin);

            if (dependencies.ContainsKey(plugin))
            {
                foreach (var dependency in dependencies[plugin])
                {
                    if (!visited.Contains(dependency))
                    {
                        if (HasCycle(dependency, dependencies, visited, recursionStack))
                            return true;
                    }
                    else if (recursionStack.Contains(dependency))
                    {
                        return true;
                    }
                }
            }

            recursionStack.Remove(plugin);
            return false;
        }

        #endregion
    }
}
