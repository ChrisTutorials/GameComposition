using NetArchTest.Rules;
using Shouldly;
using System.Linq;
using System.Reflection;
using System.Collections.Generic;

namespace BarkMoon.GameComposition.Core.Tests.Architectural
{
    /// <summary>
    /// Architectural tests that enforce consistent ID typing across all plugins.
    /// All domain objects must use GameComposition.Core numeric ID types, not strings.
    /// </summary>
    public class IdTypeConsistencyTests
    {
        /// <summary>
        /// Rule 1: Data contracts (records/DTOs) must not use string for ID fields.
        /// They should use GameComposition.Core numeric ID types (UserId, NumericId, etc.).
        /// </summary>
        [Fact]
        public void Data_Contracts_Must_Use_Numeric_ID_Types_Not_Strings()
        {
            // Arrange - Test all loaded assemblies
            var assemblies = new[]
            {
                Assembly.LoadFrom("../../../Core/bin/Debug/net10.0/BarkMoon.GameComposition.Core.dll"),
                // Add other plugin assemblies as needed
            };

            foreach (var assembly in assemblies)
            {
                if (!System.IO.File.Exists(assembly.Location))
                    continue; // Skip if assembly doesn't exist

                // Find all data contract types (records, DTOs)
                var dataContractTypes = Types.InAssembly(assembly)
                    .That()
                    .AreClasses()
                    .Or()
                    .AreValueTypes()
                    .And()
                    .HaveNameEndingWith("Entry")
                    .Or()
                    .HaveNameEndingWith("Data")
                    .Or()
                    .HaveNameEndingWith("Dto")
                    .Or()
                    .HaveNameEndingWith("Contract")
                    .GetTypes();

                // Act & Assert
                foreach (var type in dataContractTypes)
                {
                    var properties = type.GetProperties(BindingFlags.Public | BindingFlags.Instance);
                    var fields = type.GetFields(BindingFlags.Public | BindingFlags.Instance);

                    // Check properties
                    foreach (var property in properties)
                    {
                        // Skip if it's not an ID field
                        if (!property.Name.Contains("Id") && !property.Name.Contains("Key"))
                            continue;

                        // Should not use string for IDs
                        property.PropertyType.ShouldNotBe(typeof(string),
                            $"Data contract {type.Name}.{property.Name} should not use string. Use GameComposition.Core numeric ID types like UserId, NumericId, etc.");

                        // Should use GameComposition.Core ID types
                        IsGameCompositionIdType(property.PropertyType).ShouldBeTrue(
                            $"Data contract {type.Name}.{property.Name} should use GameComposition.Core ID types. Current type: {property.PropertyType.Name}");
                    }

                    // Check fields
                    foreach (var field in fields)
                    {
                        // Skip if it's not an ID field
                        if (!field.Name.Contains("Id") && !field.Name.Contains("Key"))
                            continue;

                        // Should not use string for IDs
                        field.FieldType.ShouldNotBe(typeof(string),
                            $"Data contract {type.Name}.{field.Name} should not use string. Use GameComposition.Core numeric ID types like UserId, NumericId, etc.");

                        // Should use GameComposition.Core ID types
                        IsGameCompositionIdType(field.FieldType).ShouldBeTrue(
                            $"Data contract {type.Name}.{field.Name} should use GameComposition.Core ID types. Current type: {field.FieldType.Name}");
                    }
                }
            }
        }

        /// <summary>
        /// Rule 2: State classes must use GameComposition.Core ID types, not strings.
        /// </summary>
        [Fact]
        public void State_Classes_Must_Use_Numeric_ID_Types_Not_Strings()
        {
            // Arrange
            var assemblies = new[]
            {
                Assembly.LoadFrom("../../../Core/bin/Debug/net10.0/BarkMoon.GameComposition.Core.dll"),
                // Add plugin assemblies as needed
            };

            foreach (var assembly in assemblies)
            {
                if (!System.IO.File.Exists(assembly.Location))
                    continue;

                var stateTypes = Types.InAssembly(assembly)
                    .That()
                    .HaveNameEndingWith("State")
                    .And()
                    .AreClasses()
                    .And()
                    .AreNotAbstract()
                    .And()
                    .AreNotInterfaces()
                    .GetTypes();

                // Act & Assert
                foreach (var stateType in stateTypes)
                {
                    var properties = stateType.GetProperties(BindingFlags.Public | BindingFlags.Instance);
                    var fields = stateType.GetFields(BindingFlags.NonPublic | BindingFlags.Instance);

                    // Check public properties
                    foreach (var property in properties)
                    {
                        if (!property.Name.Contains("Id") && !property.Name.Contains("Key"))
                            continue;

                        property.PropertyType.ShouldNotBe(typeof(string),
                            $"State {stateType.Name}.{property.Name} should not use string. Use GameComposition.Core numeric ID types.");

                        IsGameCompositionIdType(property.PropertyType).ShouldBeTrue(
                            $"State {stateType.Name}.{property.Name} should use GameComposition.Core ID types. Current: {property.PropertyType.Name}");
                    }

                    // Check private fields
                    foreach (var field in fields)
                    {
                        if (!field.Name.Contains("Id") && !field.Name.Contains("Key"))
                            continue;

                        field.FieldType.ShouldNotBe(typeof(string),
                            $"State {stateType.Name} field '{field.Name}' should not use string. Use GameComposition.Core numeric ID types.");

                        IsGameCompositionIdType(field.FieldType).ShouldBeTrue(
                            $"State {stateType.Name} field '{field.Name}' should use GameComposition.Core ID types. Current: {field.FieldType.Name}");
                    }
                }
            }
        }

        /// <summary>
        /// Rule 3: Service method parameters must use GameComposition.Core ID types, not strings.
        /// </summary>
        [Fact]
        public void Service_Methods_Must_Use_Numeric_ID_Types_Not_Strings()
        {
            // Arrange
            var assemblies = new[]
            {
                Assembly.LoadFrom("../../../Core/bin/Debug/net10.0/BarkMoon.GameComposition.Core.dll"),
                // Add plugin assemblies as needed
            };

            foreach (var assembly in assemblies)
            {
                if (!System.IO.File.Exists(assembly.Location))
                    continue;

                var serviceTypes = Types.InAssembly(assembly)
                    .That()
                    .HaveNameEndingWith("Service")
                    .And()
                    .AreClasses()
                    .And()
                    .AreNotAbstract()
                    .And()
                    .AreNotInterfaces()
                    .GetTypes();

                // Act & Assert
                foreach (var serviceType in serviceTypes)
                {
                    var methods = serviceType.GetMethods(BindingFlags.Public | BindingFlags.Instance);

                    foreach (var method in methods)
                    {
                        if (method.IsSpecialName) // Skip properties
                            continue;

                        var parameters = method.GetParameters();

                        foreach (var param in parameters)
                        {
                            if (!param.Name.Contains("Id") && !param.Name.Contains("Key"))
                                continue;

                            param.ParameterType.ShouldNotBe(typeof(string),
                                $"Service {serviceType.Name}.{method.Name}() parameter '{param.Name}' should not use string. Use GameComposition.Core numeric ID types.");

                            IsGameCompositionIdType(param.ParameterType).ShouldBeTrue(
                                $"Service {serviceType.Name}.{method.Name}() parameter '{param.Name}' should use GameComposition.Core ID types. Current: {param.ParameterType.Name}");
                        }
                    }
                }
            }
        }

        private static bool IsGameCompositionIdType(Type type)
        {
            // Check for GameComposition.Core ID types
            var gameCompositionAssembly = typeof(BarkMoon.GameComposition.Core.Types.UserId).Assembly;
            
            // Check if type is from GameComposition.Core and has "Id" in name
            return type.Assembly == gameCompositionAssembly && 
                   (type.Name.Contains("Id") || type.Name.Contains("Key")) &&
                   !type.IsEnum;
        }
    }
}
