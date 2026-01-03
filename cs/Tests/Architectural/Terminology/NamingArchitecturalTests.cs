using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
using NetArchTest.Rules;
using Shouldly;
using Xunit;

namespace BarkMoon.GameComposition.ArchitecturalTests.Naming
{
    /// <summary>
    /// Naming convention architectural rules for plugin ecosystem.
    /// Tests dimensional suffixes, snapshot naming, and domain terminology consistency.
    /// </summary>
    public class NamingArchitecturalTests
    {
        /// <summary>
        /// Tests dimensional naming conventions - applies to any plugin assembly.
        /// Enforces 2D/3D suffixes for dimensional types.
        /// </summary>
        public static void Dimensional_Types_Should_Have_Dimensional_Suffix(Assembly pluginAssembly)
        {
            // Arrange
            var violations = new List<string>();
            
            // Get all types that might be dimensional
            var gridRelatedTypes = Types.InAssembly(pluginAssembly)
                .That()
                .ResideInNamespace("*.Grid*")
                .Or()
                .ResideInNamespace("*.Targeting*")
                .Or()
                .ResideInNamespace("*.Cursor*")
                .GetTypes();

            // Act - Check dimensional suffix compliance
            foreach (var type in gridRelatedTypes)
            {
                var typeName = type.Name;
                
                // Skip test types
                if (typeName.Contains("Test"))
                    continue;

                // Check if type deals with dimensional concepts but lacks suffix
                var hasDimensionalConcept = typeName.Contains("Grid") || 
                                          typeName.Contains("Position") || 
                                          typeName.Contains("Coordinate") || 
                                          typeName.Contains("Cursor") ||
                                          typeName.Contains("Target");

                if (hasDimensionalConcept && 
                    !typeName.EndsWith("2D") && 
                    !typeName.EndsWith("3D") &&
                    !typeName.EndsWith("Service") && // Services can be dimension-agnostic
                    !typeName.EndsWith("State") &&   // States can be dimension-agnostic
                    !typeName.EndsWith("Data"))      // Data can be dimension-agnostic
                {
                    violations.Add($"{type.FullName} appears to be dimensional but lacks 2D/3D suffix");
                }
            }

            // Assert
            violations.ShouldBeEmpty($"Dimensional naming violations found: {string.Join(", ", violations)}");
        }

        /// <summary>
        /// Tests that all ID types use numeric IDs for performance, not strings.
        /// </summary>
        public static void ID_Types_Should_Use_Numeric_IDs_Not_Strings(Assembly pluginAssembly)
        {
            // Arrange
            var violations = new List<string>();
            
            // Get all ID types
            var idTypes = Types.InAssembly(pluginAssembly)
                .That()
                .HaveNameEndingWith("Id")
                .And()
                .AreClasses()
                .GetTypes();

            // Act - Check ID type implementations
            foreach (var idType in idTypes)
            {
                // Skip test types
                if (idType.Name.Contains("Test"))
                    continue;

                // Check for string-based ID properties
                var properties = idType.GetProperties();
                foreach (var property in properties)
                {
                    if (property.PropertyType == typeof(string) && 
                        (property.Name.Contains("Id") || property.Name.Contains("ID")))
                    {
                        violations.Add($"{idType.FullName}.{property.Name} should use numeric ID type, not string");
                    }
                }

                // Check for string-based constructor parameters
                var constructors = idType.GetConstructors();
                foreach (var constructor in constructors)
                {
                    var parameters = constructor.GetParameters();
                    foreach (var parameter in parameters)
                    {
                        if (parameter.ParameterType == typeof(string) && 
                            (parameter.Name?.Contains("Id") == true || parameter.Name?.Contains("ID") == true))
                        {
                            violations.Add($"{idType.FullName} constructor parameter {parameter.Name} should use numeric ID type, not string");
                        }
                    }
                }
            }

            // Assert
            violations.ShouldBeEmpty($"ID type violations found: {string.Join(", ", violations)}");
        }

        /// <summary>
        /// Tests that cursor domain consistently uses "Cursor" terminology.
        /// Prevents naming inconsistencies like "Positioner" vs "Cursor".
        /// </summary>
        public static void Cursor_Domain_Should_Use_Consistent_Cursor_Terminology(Assembly pluginAssembly)
        {
            // Arrange
            var violations = new List<string>();
            
            // Get all cursor-related types
            var cursorTypes = Types.InAssembly(pluginAssembly)
                .That()
                .ResideInNamespace("*.Cursor*")
                .Or()
                .HaveNameContaining("Cursor")
                .GetTypes();

            // Act - Check for consistent terminology
            var forbiddenTerms = new[] { "Positioner", "Positioning", "Pointer", "Selector" };

            foreach (var type in cursorTypes)
            {
                // Skip test types
                if (type.Name.Contains("Test"))
                    continue;

                var typeName = type.Name;
                var typeNamespace = type.Namespace ?? "";

                // Check for forbidden terminology
                foreach (var forbiddenTerm in forbiddenTerms)
                {
                    if (typeName.Contains(forbiddenTerm) || typeNamespace.Contains(forbiddenTerm))
                    {
                        violations.Add($"{type.FullName} uses forbidden term '{forbiddenTerm}' - use 'Cursor' terminology instead");
                    }
                }
            }

            // Assert
            violations.ShouldBeEmpty($"Cursor terminology violations found: {string.Join(", ", violations)}");
        }

        /// <summary>
        /// Tests that snapshot classes follow proper naming convention: Domain + Snapshot + Dimension.
        /// Enforces consistent naming patterns across all snapshot classes.
        /// </summary>
        public static void Snapshot_Classes_Should_Follow_Proper_Naming_Convention(Assembly pluginAssembly)
        {
            // Arrange
            var violations = new List<string>();
            
            // Get all snapshot classes (ending with "Snapshot")
            var snapshotTypes = Types.InAssembly(pluginAssembly)
                .That()
                .HaveNameEndingWith("Snapshot")
                .And()
                .AreClasses()
                .GetTypes();

            // Act - Check naming convention compliance
            foreach (var snapshotType in snapshotTypes)
            {
                // Skip test classes
                if (snapshotType.Name.Contains("Test"))
                    continue;

                var typeName = snapshotType.Name;
                
                // Check for forbidden patterns
                var forbiddenPatterns = new[]
                {
                    "StateSnapshot", // Should be just "Snapshot"
                    "SnapshotState", // Wrong order
                    "State2DSnapshot", // Wrong order, should be "Snapshot2D"
                    "State3DSnapshot", // Wrong order, should be "Snapshot3D"
                };

                foreach (var pattern in forbiddenPatterns)
                {
                    if (typeName.Contains(pattern))
                    {
                        violations.Add($"{snapshotType.FullName} contains forbidden pattern '{pattern}'. Snapshot names should follow pattern: Domain + Snapshot + Dimension (e.g., 'TargetingSnapshot2D', 'PlacementSnapshot')");
                        break;
                    }
                }

                // Verify correct pattern: Domain + Snapshot + [2D|3D]
                // Examples: TargetingSnapshot2D, PlacementSnapshot, GridSnapshot3D
                if (!Regex.IsMatch(typeName, @"^[A-Za-z]+Snapshot(2D|3D)?$") && 
                    !Regex.IsMatch(typeName, @"^[A-Za-z]+Snapshot$"))
                {
                    violations.Add($"{snapshotType.FullName} doesn't follow naming pattern. Expected: Domain + Snapshot + optional Dimension (e.g., 'TargetingSnapshot2D', 'PlacementSnapshot')");
                }
            }

            // Assert
            violations.ShouldBeEmpty($"Snapshot naming convention violations found: {string.Join(", ", violations)}");
        }

        /// <summary>
        /// Tests that service classes with dimensional suffixes have the suffix at the end.
        /// Enforces that "2D" and "3D" are always suffixes, not in the middle.
        /// </summary>
        public static void Service_Classes_Should_Have_Dimensional_Suffix_At_End(Assembly pluginAssembly)
        {
            // Arrange
            var violations = new List<string>();
            
            // Get all service classes (ending with "Service")
            var serviceTypes = Types.InAssembly(pluginAssembly)
                .That()
                .HaveNameEndingWith("Service")
                .And()
                .AreClasses()
                .GetTypes();

            // Act - Check dimensional suffix placement
            foreach (var serviceType in serviceTypes)
            {
                // Skip test classes
                if (serviceType.Name.Contains("Test"))
                    continue;

                var typeName = serviceType.Name;
                
                // Check for dimensional indicators in wrong position
                if (typeName.Contains("2D") || typeName.Contains("3D"))
                {
                    // Dimensional suffix should be at the very end
                    if (!typeName.EndsWith("2D") && !typeName.EndsWith("3D"))
                    {
                        violations.Add($"{serviceType.FullName} has dimensional indicator not at end. '{typeName}' should end with '2D' or '3D'");
                    }
                }
            }

            // Assert
            violations.ShouldBeEmpty($"Service dimensional suffix violations found: {string.Join(", ", violations)}");
        }

        /// <summary>
        /// Tests that state classes with dimensional suffixes have the suffix at the end.
        /// Enforces that "2D" and "3D" are always suffixes, not in the middle.
        /// </summary>
        public static void State_Classes_Should_Have_Dimensional_Suffix_At_End(Assembly pluginAssembly)
        {
            // Arrange
            var violations = new List<string>();
            
            // Get all state classes (ending with "State")
            var stateTypes = Types.InAssembly(pluginAssembly)
                .That()
                .HaveNameEndingWith("State")
                .And()
                .AreClasses()
                .GetTypes();

            // Act - Check dimensional suffix placement
            foreach (var stateType in stateTypes)
            {
                // Skip test classes
                if (stateType.Name.Contains("Test"))
                    continue;

                var typeName = stateType.Name;
                
                // Check for dimensional indicators in wrong position
                if (typeName.Contains("2D") || typeName.Contains("3D"))
                {
                    // Dimensional suffix should be at the very end
                    if (!typeName.EndsWith("2D") && !typeName.EndsWith("3D"))
                    {
                        violations.Add($"{stateType.FullName} has dimensional indicator not at end. '{typeName}' should end with '2D' or '3D'");
                    }
                }
            }

            // Assert
            violations.ShouldBeEmpty($"State dimensional suffix violations found: {string.Join(", ", violations)}");
        }
    }
}
