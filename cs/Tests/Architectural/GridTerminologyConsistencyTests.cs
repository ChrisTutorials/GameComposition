using NetArchTest.Rules;
using Shouldly;
using System.Linq;
using System.Reflection;
using BarkMoon.GameComposition.Core.Types;
using Xunit;

// Explicitly alias the NetArchTest Types class to avoid conflict
using ArchTypes = NetArchTest.Rules.Types;

namespace BarkMoon.GameComposition.Core.Tests.Architectural
{
    /// <summary>
    /// Architectural tests that enforce grid terminology consistency across the ecosystem.
    /// Ensures that grid-based positioning uses clear, unambiguous naming conventions.
    /// </summary>
    public class GridTerminologyConsistencyTests
    {
        [Fact]
        [Trait("Category", "Architectural")]
        public void Grid_Positioning_Should_Use_GridTile_Not_GridPosition()
        {
            // Arrange
            var assemblies = LoadAllPluginAssemblies();
            var gridPositioningTypes = new List<Type>();

            // Act
            foreach (var assembly in assemblies)
            {
                var types = assembly.GetTypes()
                    .Where(t => t.IsClass || t.IsValueType)
                    .Where(t => t.GetProperties().Any(p => p.Name.Contains("Grid")))
                    .Where(t => t.GetFields().Any(f => f.Name.Contains("Grid")))
                    .ToList();

                gridPositioningTypes.AddRange(types);
            }

            // Assert
            var violations = new List<string>();

            foreach (var type in gridPositioningTypes)
            {
                // Check properties
                var gridProperties = type.GetProperties()
                    .Where(p => p.Name.Contains("Grid"))
                    .ToList();

                foreach (var property in gridProperties)
                {
                    if (property.Name.Contains("GridPosition"))
                    {
                        violations.Add($"{type.FullName}.{property.Name}: Property should use 'GridTile' instead of 'GridPosition' for discrete grid coordinates");
                    }
                }

                // Check fields
                var gridFields = type.GetFields()
                    .Where(f => f.Name.Contains("Grid"))
                    .ToList();

                foreach (var field in gridFields)
                {
                    if (field.Name.Contains("GridPosition"))
                    {
                        violations.Add($"{type.FullName}.{field.Name}: Field should use 'GridTile' instead of 'GridPosition' for discrete grid coordinates");
                    }
                }

                // Check record struct parameters
                if (type.IsValueType && type.Name.Contains("Snapshot"))
                {
                    var constructors = type.GetConstructors();
                    foreach (var constructor in constructors)
                    {
                        var parameters = constructor.GetParameters();
                        foreach (var parameter in parameters)
                        {
                            if (parameter.Name.Contains("GridPosition"))
                            {
                                violations.Add($"{type.FullName} constructor parameter '{parameter.Name}': Should use 'GridTile' for discrete grid coordinates");
                            }
                        }
                    }
                }
            }

            violations.ShouldBeEmpty($"Found {violations.Count} grid terminology violations:\n{string.Join("\n", violations)}");
        }

        [Fact]
        [Trait("Category", "Architectural")]
        public void Grid_Types_Should_Use_Vector2I_For_Discrete_Positions()
        {
            // Arrange
            var assemblies = LoadAllPluginAssemblies();
            var violations = new List<string>();

            // Act
            foreach (var assembly in assemblies)
            {
                var types = assembly.GetTypes()
                    .Where(t => t.IsClass || t.IsValueType)
                    .Where(t => t.GetProperties().Any(p => p.Name.Contains("GridTile")))
                    .ToList();

                foreach (var type in types)
                {
                    var gridTileProperties = type.GetProperties()
                        .Where(p => p.Name.Contains("GridTile"))
                        .ToList();

                    foreach (var property in gridTileProperties)
                    {
                        if (property.PropertyType != typeof(Vector2I) && 
                            property.PropertyType.Name != "Vector2I")
                        {
                            violations.Add($"{type.FullName}.{property.Name}: GridTile properties should use Vector2I for discrete grid coordinates, found {property.PropertyType.Name}");
                        }
                    }

                    var gridTileFields = type.GetFields()
                        .Where(f => f.Name.Contains("GridTile"))
                        .ToList();

                    foreach (var field in gridTileFields)
                    {
                        if (field.FieldType != typeof(Vector2I) && 
                            field.FieldType.Name != "Vector2I")
                        {
                            violations.Add($"{type.FullName}.{field.Name}: GridTile fields should use Vector2I for discrete grid coordinates, found {field.FieldType.Name}");
                        }
                    }
                }
            }

            // Assert
            violations.ShouldBeEmpty($"Found {violations.Count} grid tile type violations:\n{string.Join("\n", violations)}");
        }

        [Fact]
        [Trait("Category", "Architectural")]
        public void World_Positioning_Should_Use_Vector2_For_Continuous_Positions()
        {
            // Arrange
            var assemblies = LoadAllPluginAssemblies();
            var violations = new List<string>();

            // Act
            foreach (var assembly in assemblies)
            {
                var types = assembly.GetTypes()
                    .Where(t => t.IsClass || t.IsValueType)
                    .Where(t => t.GetProperties().Any(p => p.Name.Contains("WorldPosition")))
                    .ToList();

                foreach (var type in types)
                {
                    var worldPositionProperties = type.GetProperties()
                        .Where(p => p.Name.Contains("WorldPosition"))
                        .ToList();

                    foreach (var property in worldPositionProperties)
                    {
                        if (property.PropertyType != typeof(Vector2) && 
                            property.PropertyType.Name != "Vector2")
                        {
                            violations.Add($"{type.FullName}.{property.Name}: WorldPosition properties should use Vector2 for continuous world coordinates, found {property.PropertyType.Name}");
                        }
                    }

                    var worldPositionFields = type.GetFields()
                        .Where(f => f.Name.Contains("WorldPosition"))
                        .ToList();

                    foreach (var field in worldPositionFields)
                    {
                        if (field.FieldType != typeof(Vector2) && 
                            field.FieldType.Name != "Vector2")
                        {
                            violations.Add($"{type.FullName}.{field.Name}: WorldPosition fields should use Vector2 for continuous world coordinates, found {field.FieldType.Name}");
                        }
                    }
                }
            }

            // Assert
            violations.ShouldBeEmpty($"Found {violations.Count} world position type violations:\n{string.Join("\n", violations)}");
        }

        #region Helper Methods

        private static List<Assembly> LoadAllPluginAssemblies()
        {
            var assemblies = new List<Assembly>();
            var baseDir = Path.Combine(Directory.GetCurrentDirectory(), "..", "..", "..", "..", "..", "..", "..", "..", "plugins");

            // Load GameComposition.Core
            var gameCompositionPath = Path.Combine(baseDir, "framework", "GameComposition", "cs", "Core", "bin", "Debug", "net10.0", "BarkMoon.GameComposition.Core.dll");
            if (File.Exists(gameCompositionPath))
            {
                assemblies.Add(Assembly.LoadFrom(gameCompositionPath));
            }

            // Load GridPlacement.Core
            var gridPlacementPath = Path.Combine(baseDir, "gameplay", "GridPlacement", "cs", "Core", "bin", "Debug", "net10.0", "BarkMoon.GridPlacement.Core.dll");
            if (File.Exists(gridPlacementPath))
            {
                assemblies.Add(Assembly.LoadFrom(gridPlacementPath));
            }

            return assemblies;
        }

        #endregion
    }
}
