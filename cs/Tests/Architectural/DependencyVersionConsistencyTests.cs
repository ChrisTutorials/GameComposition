using System.IO;
using System.Linq;
using System.Xml.Linq;
using Shouldly;
using Xunit;
using Xunit.Abstractions;

namespace BarkMoon.GameComposition.Tests.Architectural
{
    /// <summary>
    /// Enforces single source of truth for dependency versions across all projects.
    /// Prevents version drift and ensures consistent infrastructure packages.
    /// </summary>
    public class DependencyVersionConsistencyTests
    {
        private readonly ITestOutputHelper _output;

        // Single Source of Truth - Centralized dependency versions
        private static readonly Dictionary<string, string> ApprovedVersions = new()
        {
            // Core Testing Infrastructure
            ["Microsoft.NET.Test.Sdk"] = "17.14.1",
            ["xunit"] = "2.9.3",
            ["xunit.runner.visualstudio"] = "3.1.4",
            ["Shouldly"] = "4.2.1",
            ["NetArchTest.Rules"] = "1.3.2",
            
            // Code Coverage
            ["coverlet.collector"] = "6.0.4",
            ["Coverlet.msbuild"] = "6.0.2",
            
            // Mocking Frameworks
            ["Moq"] = "4.20.69",
            
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

        // Projects that should be validated
        private static readonly string[] ProjectPaths = new[]
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

        public DependencyVersionConsistencyTests(ITestOutputHelper output)
        {
            _output = output;
        }

        [Fact(DisplayName = "DV001: All Projects Should Use Approved Infrastructure Versions")]
        public void All_Projects_Should_Use_Approved_Infrastructure_Versions()
        {
            // Arrange
            var violations = new List<string>();
            var projectAnalyses = new List<ProjectAnalysis>();

            // Act
            foreach (var projectPath in ProjectPaths)
            {
                if (!File.Exists(projectPath))
                {
                    _output.WriteLine($"Skipping missing project: {projectPath}");
                    continue;
                }

                var analysis = AnalyzeProject(projectPath);
                projectAnalyses.Add(analysis);

                // Check for version violations
                foreach (var package in analysis.PackageReferences)
                {
                    if (ApprovedVersions.TryGetValue(package.Key, out var approvedVersion))
                    {
                        if (package.Value != approvedVersion)
                        {
                            violations.Add($"{Path.GetFileName(projectPath)}: {package.Key} = {package.Value} (should be {approvedVersion})");
                        }
                    }
                }
            }

            // Report summary
            _output.WriteLine($"Analyzed {projectAnalyses.Count} projects");
            _output.WriteLine($"Found {violations.Count} version violations");

            foreach (var analysis in projectAnalyses)
            {
                _output.WriteLine($"\n{Path.GetFileName(analysis.ProjectPath)}:");
                foreach (var pkg in analysis.PackageReferences.OrderBy(x => x.Key))
                {
                    var status = ApprovedVersions.ContainsKey(pkg.Key) ? "✅" : "⚠️";
                    _output.WriteLine($"  {status} {pkg.Key} = {pkg.Value}");
                }
            }

            // Assert
            if (violations.Any())
            {
                var violationMessage = "Dependency version violations found:\n" + string.Join("\n", violations);
                throw new Exception(violationMessage);
            }

            violations.ShouldBeEmpty("All projects should use approved infrastructure versions");
        }

        [Fact(DisplayName = "DV002: All Test Projects Should Use Consistent XUnit Versions")]
        public void All_Test_Projects_Should_Use_Consistent_XUnit_Versions()
        {
            // Arrange
            var testProjects = ProjectPaths.Where(p => p.Contains("Tests") || p.Contains("test")).ToArray();
            var xunitViolations = new List<string>();

            // Act
            foreach (var projectPath in testProjects)
            {
                if (!File.Exists(projectPath)) continue;

                var analysis = AnalyzeProject(projectPath);
                var xunitPackages = analysis.PackageReferences
                    .Where(pr => pr.Key.StartsWith("xunit", StringComparison.OrdinalIgnoreCase))
                    .ToList();

                foreach (var package in xunitPackages)
                {
                    if (ApprovedVersions.TryGetValue(package.Key, out var approvedVersion))
                    {
                        if (package.Value != approvedVersion)
                        {
                            xunitViolations.Add($"{Path.GetFileName(projectPath)}: {package.Key} = {package.Value} (should be {approvedVersion})");
                        }
                    }
                }
            }

            // Report
            _output.WriteLine($"XUnit consistency check for {testProjects.Length} test projects");
            _output.WriteLine($"Found {xunitViolations.Count} XUnit version violations");

            // Assert
            xunitViolations.ShouldBeEmpty("All test projects should use consistent XUnit versions");
        }

        [Fact(DisplayName = "DV003: All Projects Should Use Latest Approved Versions")]
        public void All_Projects_Should_Use_Latest_Approved_Versions()
        {
            // Arrange
            var outdatedPackages = new List<string>();

            // Act
            foreach (var projectPath in ProjectPaths)
            {
                if (!File.Exists(projectPath)) continue;

                var analysis = AnalyzeProject(projectPath);
                
                foreach (var package in analysis.PackageReferences)
                {
                    if (ApprovedVersions.TryGetValue(package.Key, out var approvedVersion))
                    {
                        // Check if project is using older version
                        if (IsOlderVersion(package.Value, approvedVersion))
                        {
                            outdatedPackages.Add($"{Path.GetFileName(projectPath)}: {package.Key} = {package.Value} (should be upgraded to {approvedVersion})");
                        }
                    }
                }
            }

            // Report
            _output.WriteLine($"Outdated package check across all projects");
            _output.WriteLine($"Found {outdatedPackages.Count} outdated packages");

            // Assert
            outdatedPackages.ShouldBeEmpty("All projects should use latest approved versions");
        }

        [Fact(DisplayName = "DV004: Framework Dependencies Should Be Consistent")]
        public void Framework_Dependencies_Should_Be_Consistent()
        {
            // Arrange
            var frameworkProjects = ProjectPaths.Where(p => p.Contains("framework")).ToArray();
            var frameworkViolations = new List<string>();
            var frameworkPackageVersions = new Dictionary<string, HashSet<string>>();

            // Act - Collect all versions used for each package
            foreach (var projectPath in frameworkProjects)
            {
                if (!File.Exists(projectPath)) continue;

                var analysis = AnalyzeProject(projectPath);
                
                foreach (var package in analysis.PackageReferences)
                {
                    if (!frameworkPackageVersions.ContainsKey(package.Key))
                    {
                        frameworkPackageVersions[package.Key] = new HashSet<string>();
                    }
                    frameworkPackageVersions[package.Key].Add(package.Value);
                }
            }

            // Check for inconsistencies
            foreach (var kvp in frameworkPackageVersions)
            {
                if (kvp.Value.Count > 1)
                {
                    frameworkViolations.Add($"{kvp.Key} has inconsistent versions: {string.Join(", ", kvp.Value.Order())}");
                }
            }

            // Report
            _output.WriteLine($"Framework dependency consistency check");
            _output.WriteLine($"Found {frameworkViolations.Count} framework inconsistencies");

            // Assert
            frameworkViolations.ShouldBeEmpty("Framework dependencies should be consistent across all framework projects");
        }

        [Fact(DisplayName = "DV005: No Deprecated Package Versions Should Exist")]
        public void No_Deprecated_Package_Versions_Should_Exist()
        {
            // Arrange
            var deprecatedVersions = new Dictionary<string, string[]>
            {
                ["xunit"] = new[] { "2.6.2", "2.9.2" }, // Older versions
                ["xunit.runner.visualstudio"] = new[] { "2.5.3", "2.8.2" }, // Older versions
                ["Microsoft.NET.Test.Sdk"] = new[] { "17.8.0", "17.11.1" }, // Older versions
                ["coverlet.collector"] = new[] { "6.0.2" } // Older version
            };

            var deprecatedViolations = new List<string>();

            // Act
            foreach (var projectPath in ProjectPaths)
            {
                if (!File.Exists(projectPath)) continue;

                var analysis = AnalyzeProject(projectPath);
                
                foreach (var package in analysis.PackageReferences)
                {
                    if (deprecatedVersions.TryGetValue(package.Key, out var deprecated))
                    {
                        if (deprecated.Contains(package.Value))
                        {
                            var approvedVersion = ApprovedVersions[package.Key];
                            deprecatedViolations.Add($"{Path.GetFileName(projectPath)}: {package.Key} = {package.Value} (deprecated, should be {approvedVersion})");
                        }
                    }
                }
            }

            // Report
            _output.WriteLine($"Deprecated package version check");
            _output.WriteLine($"Found {deprecatedViolations.Count} deprecated package versions");

            // Assert
            deprecatedViolations.ShouldBeEmpty("No deprecated package versions should exist");
        }

        private ProjectAnalysis AnalyzeProject(string projectPath)
        {
            var doc = XDocument.Load(projectPath);
            var packageReferences = doc.Descendants("PackageReference")
                .ToDictionary(
                    pr => pr.Attribute("Include")?.Value ?? "",
                    pr => pr.Attribute("Version")?.Value ?? ""
                );

            return new ProjectAnalysis
            {
                ProjectPath = projectPath,
                PackageReferences = packageReferences
            };
        }

        private bool IsOlderVersion(string current, string approved)
        {
            // Simple version comparison (could be enhanced with proper semantic version parsing)
            var currentParts = current.Split('.').Select(int.Parse).ToArray();
            var approvedParts = approved.Split('.').Select(int.Parse).ToArray();

            for (int i = 0; i < Math.Min(currentParts.Length, approvedParts.Length); i++)
            {
                if (currentParts[i] < approvedParts[i])
                    return true;
                if (currentParts[i] > approvedParts[i])
                    return false;
            }

            return currentParts.Length < approvedParts.Length;
        }

        private class ProjectAnalysis
        {
            public string ProjectPath { get; set; } = string.Empty;
            public Dictionary<string, string> PackageReferences { get; set; } = new();
        }
    }
}
