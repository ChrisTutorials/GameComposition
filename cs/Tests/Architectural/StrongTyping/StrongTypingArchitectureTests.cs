using System.Linq;
using System.Reflection;
using NetArchTest.Rules;
using Shouldly;
using Xunit;
using BarkMoon.GameComposition.Tests.Common;

// Explicitly alias the NetArchTest Types class to avoid conflict
using ArchTypes = NetArchTest.Rules.Types;

namespace BarkMoon.GameComposition.Tests.Architectural
{
    /// <summary>
    /// Architectural tests enforcing strong typing principles across the entire ecosystem.
    /// These tests validate that we use proper interfaces and concrete types instead of object/object?.
    /// </summary>
    public class StrongTypingArchitectureTests
    {
        [Fact(DisplayName = "001GC: Domain Data Contracts Should Use Strong Types Not Dictionary")]
        [Trait("Category", "Architectural")]
        public void Domain_Data_Contracts_Should_Use_Strong_Types_Not_Dictionary()
        {
            // Arrange - Use SSOT helper for assembly loading
            var assemblies = ArchitecturalTestHelpers.GetAllAssemblies()
                .Where(a => a.GetName().Name.Contains("GridPlacement"))
                .ToArray();

            foreach (var assembly in assemblies)
            {
                if (assembly == null) continue;

                // Act - Find data contract types
                var dataContractTypes = Types.InAssembly(assembly)
                    .That()
                    .ResideInNamespace("*.Data.*")
                    .Or()
                    .HaveNameContaining("Entry")
                    .Or()
                    .HaveNameContaining("Metadata")
                    .Or()
                    .HaveNameContaining("Contract")
                    .Or()
                    .HaveNameContaining("Record")
                    .GetTypes();

                // Assert - Check for Dictionary<string,object> usage
                foreach (var type in dataContractTypes)
                {
                    var dictionaryUsages = type.GetFields()
                        .Concat(type.GetProperties())
                        .Where(member => member.FieldType?.Name == "Dictionary`2" || 
                                       member.PropertyType?.Name == "Dictionary`2")
                        .ToList();

                    var violations = dictionaryUsages.Where(member =>
                    {
                        var fieldType = member.FieldType ?? member.PropertyType;
                        if (fieldType?.IsGenericType == true)
                        {
                            var genericArgs = fieldType.GetGenericArguments();
                            return genericArgs.Length == 2 && 
                                   genericArgs[0].FullName == "System.String" && 
                                   genericArgs[1].FullName == "System.Object";
                        }
                        return false;
                    }).ToList();

                    violations.ShouldBeEmpty(
                        $"Type {type.FullName} should use strong typing instead of Dictionary<string,object>. " +
                        $"Found {violations.Count} violations: {string.Join(", ", violations.Select(v => v.Name))}");
                }
            }
        }

        [Fact(DisplayName = "002GC: Service Methods Should Not Return Object Types")]
        [Trait("Category", "Architectural")]
        public void Service_Methods_Should_Not_Return_Object_Types()
        {
            // Arrange
            var assemblies = new[]
            {
                typeof(BarkMoon.GridPlacement.Core.Services.Placement.PlacementService).Assembly,
                typeof(BarkMoon.GridPlacement.Core.Services.Targeting.TargetingService2D).Assembly,
                // Add other plugin assemblies as they're created
            };

            foreach (var assembly in assemblies)
            {
                if (assembly == null) continue;

                // Act - Find service types
                var serviceTypes = Types.InAssembly(assembly)
                    .That()
                    .ResideInNamespace("*.Services.*")
                    .And()
                    .ImplementInterface("IService")
                    .GetTypes();

                // Assert - Check for object return types
                foreach (var serviceType in serviceTypes)
                {
                    var methods = serviceType.GetMethods()
                        .Where(m => m.IsPublic && !m.IsSpecialName);

                    var violations = methods.Where(method =>
                    {
                        var returnType = method.ReturnType;
                        return returnType == typeof(object) || returnType == typeof(object?);
                    }).ToList();

                    violations.ShouldBeEmpty(
                        $"Service {serviceType.FullName} should not return object/object? types. " +
                        $"Found {violations.Count} violations: {string.Join(", ", violations.Select(v => $"{v.Name} -> {v.ReturnType.Name}"))}");
                }
            }
        }

        [Fact(DisplayName = "003GC: Service Method Parameters Should Not Use Object Types")]
        [Trait("Category", "Architectural")]
        public void Service_Method_Parameters_Should_Not_Use_Object_Types()
        {
            // Arrange
            var assemblies = new[]
            {
                typeof(BarkMoon.GridPlacement.Core.Services.Placement.PlacementService).Assembly,
                typeof(BarkMoon.GridPlacement.Core.Services.Targeting.TargetingService2D).Assembly,
                // Add other plugin assemblies as they're created
            };

            foreach (var assembly in assemblies)
            {
                if (assembly == null) continue;

                // Act - Find service types
                var serviceTypes = Types.InAssembly(assembly)
                    .That()
                    .ResideInNamespace("*.Services.*")
                    .And()
                    .ImplementInterface("IService")
                    .GetTypes();

                // Assert - Check for object parameter types
                foreach (var serviceType in serviceTypes)
                {
                    var methods = serviceType.GetMethods()
                        .Where(m => m.IsPublic && !m.IsSpecialName);

                    var violations = new List<(string MethodName, string ParameterName, string ParameterType)>();

                    foreach (var method in methods)
                    {
                        var objectParams = method.GetParameters()
                            .Where(p => p.ParameterType == typeof(object) || p.ParameterType == typeof(object?));

                        foreach (var param in objectParams)
                        {
                            violations.Add((method.Name, param.Name!, param.ParameterType.Name));
                        }
                    }

                    violations.ShouldBeEmpty(
                        $"Service {serviceType.FullName} should not use object/object? parameters. " +
                        $"Found {violations.Count} violations: {string.Join(", ", violations.Select(v => $"{v.MethodName}({v.ParameterName}: {v.ParameterType})"))}");
                }
            }
        }

        [Fact(DisplayName = "004GC: Public APIs Should Use Strong Types Not Object")]
        [Trait("Category", "Architectural")]
        public void Public_APIs_Should_Use_Strong_Types_Not_Object()
        {
            // Arrange
            var assemblies = new[]
            {
                typeof(BarkMoon.GridPlacement.Core.Placement.Data.PlacementEntry).Assembly,
                typeof(BarkMoon.GridPlacement.Core.Targeting.Types.TargetingSnapshot2D).Assembly,
                // Add other plugin assemblies as they're created
            };

            foreach (var assembly in assemblies)
            {
                if (assembly == null) continue;

                // Act - Find public API types (interfaces, public classes)
                var publicApiTypes = Types.InAssembly(assembly)
                    .That()
                    .ArePublic()
                    .Or()
                    .AreInterfaces()
                    .GetTypes();

                // Assert - Check for object usage in public members
                foreach (var type in publicApiTypes)
                {
                    var publicMembers = type.GetMembers()
                        .Where(m => m is FieldInfo fi && fi.IsPublic ||
                                   m is PropertyInfo pi && pi.GetMethod?.IsPublic == true ||
                                   m is MethodInfo mi && mi.IsPublic)
                        .ToList();

                    var violations = new List<string>();

                    foreach (var member in publicMembers)
                    {
                        switch (member)
                        {
                            case FieldInfo field:
                                if (field.FieldType == typeof(object) || field.FieldType == typeof(object?))
                                    violations.Add($"{member.Name} (field: {field.FieldType.Name})");
                                break;
                            case PropertyInfo property:
                                if (property.PropertyType == typeof(object) || property.PropertyType == typeof(object?))
                                    violations.Add($"{member.Name} (property: {property.PropertyType.Name})");
                                break;
                            case MethodInfo method:
                                if (method.ReturnType == typeof(object) || method.ReturnType == typeof(object?))
                                    violations.Add($"{member.Name} (return: {method.ReturnType.Name})");
                                
                                var objectParams = method.GetParameters()
                                    .Where(p => p.ParameterType == typeof(object) || p.ParameterType == typeof(object?));
                                foreach (var param in objectParams)
                                    violations.Add($"{member.Name} (param: {param.Name}: {param.ParameterType.Name})");
                                break;
                        }
                    }

                    violations.ShouldBeEmpty(
                        $"Public API {type.FullName} should use strong types instead of object/object?. " +
                        $"Found {violations.Count} violations: {string.Join(", ", violations)}");
                }
            }
        }

        [Fact(DisplayName = "005GC: Event Data Should Use Strong Types Not Object")]
        [Trait("Category", "Architectural")]
        public void Event_Data_Should_Use_Strong_Types_Not_Object()
        {
            // Arrange
            var assemblies = new[]
            {
                typeof(BarkMoon.GridPlacement.Core.Manipulation.Events.ManipulationInputEvent).Assembly,
                // Add other plugin assemblies as they're created
            };

            foreach (var assembly in assemblies)
            {
                if (assembly == null) continue;

                // Act - Find event types
                var eventTypes = Types.InAssembly(assembly)
                    .That()
                    .ResideInNamespace("*.Events.*")
                    .And()
                    .Inherit("ServiceEvent")
                    .GetTypes();

                // Assert - Check for object usage in event properties
                foreach (var eventType in eventTypes)
                {
                    var properties = eventType.GetProperties()
                        .Where(p => p.CanRead && p.GetIndexParameters().Length == 0);

                    var violations = properties.Where(p =>
                        p.PropertyType == typeof(object) || p.PropertyType == typeof(object?)).ToList();

                    violations.ShouldBeEmpty(
                        $"Event {eventType.FullName} should use strong types instead of object/object?. " +
                        $"Found {violations.Count} violations: {string.Join(", ", violations.Select(v => $"{v.Name}: {v.PropertyType.Name}"))}");
                }
            }
        }

        [Fact(DisplayName = "006GC: Configuration Classes Should Use Strong Types Not Object")]
        [Trait("Category", "Architectural")]
        public void Configuration_Classes_Should_Use_Strong_Types_Not_Object()
        {
            // Arrange
            var assemblies = new[]
            {
                typeof(BarkMoon.GridPlacement.Core.Configuration.GridPlacementConfiguration).Assembly,
                // Add other plugin assemblies as they're created
            };

            foreach (var assembly in assemblies)
            {
                if (assembly == null) continue;

                // Act - Find configuration types
                var configTypes = Types.InAssembly(assembly)
                    .That()
                    .ResideInNamespace("*.Configuration.*")
                    .And()
                    .HaveNameContaining("Configuration")
                    .GetTypes();

                // Assert - Check for object usage in configuration properties
                foreach (var configType in configTypes)
                {
                    var properties = configType.GetProperties()
                        .Where(p => p.CanRead && p.CanWrite && p.GetIndexParameters().Length == 0);

                    var violations = properties.Where(p =>
                        p.PropertyType == typeof(object) || p.PropertyType == typeof(object?)).ToList();

                    violations.ShouldBeEmpty(
                        $"Configuration {configType.FullName} should use strong types instead of object/object?. " +
                        $"Found {violations.Count} violations: {string.Join(", ", violations.Select(v => $"{v.Name}: {v.PropertyType.Name}"))}");
                }
            }
        }

        [Fact(DisplayName = "007GC: State Classes Should Use Strong Types Not Object")]
        [Trait("Category", "Architectural")]
        public void State_Classes_Should_Use_Strong_Types_Not_Object()
        {
            // Arrange
            var assemblies = new[]
            {
                typeof(BarkMoon.GridPlacement.Core.State.Placement.PlacementState2D).Assembly,
                typeof(BarkMoon.GridPlacement.Core.State.Targeting.TargetingState2D).Assembly,
                // Add other plugin assemblies as they're created
            };

            foreach (var assembly in assemblies)
            {
                if (assembly == null) continue;

                // Act - Find state types
                var stateTypes = Types.InAssembly(assembly)
                    .That()
                    .ResideInNamespace("*.State.*")
                    .And()
                    .ImplementInterface("IState")
                    .GetTypes();

                // Assert - Check for object usage in state fields/properties
                foreach (var stateType in stateTypes)
                {
                    var members = stateType.GetFields()
                        .Concat<FieldInfo>(stateType.GetProperties())
                        .Where(m => !m.IsStatic);

                    var violations = members.Where(m =>
                    {
                        var memberType = m is FieldInfo fi ? fi.FieldType : ((PropertyInfo)m).PropertyType;
                        return memberType == typeof(object) || memberType == typeof(object?);
                    }).ToList();

                    violations.ShouldBeEmpty(
                        $"State {stateType.FullName} should use strong types instead of object/object?. " +
                        $"Found {violations.Count} violations: {string.Join(", ", violations.Select(v => $"{v.Name}: {v.MemberType.Name}"))}");
                }
            }
        }
    }
}
