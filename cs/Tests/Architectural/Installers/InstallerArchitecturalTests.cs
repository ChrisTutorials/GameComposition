using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
using NetArchTest.Rules;
using Shouldly;
using Xunit;

namespace BarkMoon.GameComposition.ArchitecturalTests.Installers
{
    /// <summary>
    /// Plugin installer architectural rules for ecosystem consistency.
    /// Tests installer patterns, registration order, and service lifetime compliance.
    /// </summary>
    public class InstallerArchitecturalTests
    {
        /// <summary>
        /// Tests that all plugins have installer classes implementing IPluginInstaller.
        /// Enforces consistent plugin installation pattern across the ecosystem.
        /// </summary>
        public static void All_Plugins_Should_Have_Installer_Class(Assembly pluginAssembly)
        {
            // Arrange
            var violations = new List<string>();
            
            // Get all installer classes (ending with "Installer")
            var installerTypes = Types.InAssembly(pluginAssembly)
                .That()
                .HaveNameEndingWith("Installer")
                .And()
                .AreClasses()
                .GetTypes();

            // Act - Check that at least one installer exists and implements IPluginInstaller
            if (!installerTypes.Any())
            {
                violations.Add($"Plugin {pluginAssembly.GetName().Name} should have at least one installer class ending with 'Installer'");
            }
            else
            {
                foreach (var installerType in installerTypes)
                {
                    // Skip test classes
                    if (installerType.Name.Contains("Test"))
                        continue;

                    // All installers should implement IPluginInstaller
                    if (!typeof(BarkMoon.GameComposition.Core.Interfaces.IPluginInstaller).IsAssignableFrom(installerType))
                    {
                        violations.Add($"{installerType.FullName} should implement IPluginInstaller interface");
                    }

                    // All installers should have ConfigureServices method
                    var configureMethod = installerType.GetMethod("ConfigureServices");
                    if (configureMethod == null)
                    {
                        violations.Add($"{installerType.FullName} should have ConfigureServices(ServiceRegistry) method");
                    }
                    else
                    {
                        // Verify method signature
                        var parameters = configureMethod.GetParameters();
                        if (parameters.Length != 1 || parameters[0].ParameterType != typeof(BarkMoon.GameComposition.Core.Services.DI.ServiceRegistry))
                        {
                            violations.Add($"{installerType.FullName}.ConfigureServices should have signature: void ConfigureServices(ServiceRegistry registry)");
                        }
                    }
                }
            }

            // Assert
            violations.ShouldBeEmpty($"Plugin installer violations found: {string.Join(", ", violations)}");
        }

        /// <summary>
        /// Tests that installers register services in proper order and categories.
        /// Enforces consistent registration pattern: Configuration → Services → Orchestrators → Event Bus.
        /// </summary>
        public static void Installers_Should_Follow_Registration_Order_Pattern(Assembly pluginAssembly)
        {
            // Arrange
            var violations = new List<string>();
            
            var installerTypes = Types.InAssembly(pluginAssembly)
                .That()
                .ImplementInterface(typeof(BarkMoon.GameComposition.Core.Interfaces.IPluginInstaller))
                .And()
                .AreClasses()
                .GetTypes();

            // Act - Analyze installer registration patterns
            foreach (var installerType in installerTypes)
            {
                // Skip test classes
                if (installerType.Name.Contains("Test"))
                    continue;

                // Get the ConfigureServices method
                var configureMethod = installerType.GetMethod("ConfigureServices");
                if (configureMethod == null) continue;

                // Create instance to analyze registration pattern
                try
                {
                    var installer = Activator.CreateInstance(installerType);
                    var registry = new TestServiceRegistry(); // Test registry to capture registration order
                    
                    configureMethod.Invoke(installer, new object[] { registry });
                    
                    // Validate registration order
                    var registrationOrder = registry.GetRegistrationOrder();
                    ValidateRegistrationOrder(registrationOrder, installerType.Name, violations);
                }
                catch (Exception ex)
                {
                    violations.Add($"{installerType.FullName} failed to instantiate or execute: {ex.Message}");
                }
            }

            // Assert
            violations.ShouldBeEmpty($"Installer registration order violations found: {string.Join(", ", violations)}");
        }

        /// <summary>
        /// Tests that orchestrators are always registered as scoped services.
        /// Enforces proper orchestrator lifecycle management.
        /// </summary>
        public static void Orchestrators_Should_Be_Registered_As_Scoped(Assembly pluginAssembly)
        {
            // Arrange
            var violations = new List<string>();
            
            var installerTypes = Types.InAssembly(pluginAssembly)
                .That()
                .ImplementInterface(typeof(BarkMoon.GameComposition.Core.Interfaces.IPluginInstaller))
                .And()
                .AreClasses()
                .GetTypes();

            // Act - Check orchestrator registration patterns
            foreach (var installerType in installerTypes)
            {
                // Skip test classes
                if (installerType.Name.Contains("Test"))
                    continue;

                var configureMethod = installerType.GetMethod("ConfigureServices");
                if (configureMethod == null) continue;

                try
                {
                    var installer = Activator.CreateInstance(installerType);
                    var registry = new TestServiceRegistry();
                    
                    configureMethod.Invoke(installer, new object[] { registry });
                    
                    // Check that all orchestrators are registered as scoped
                    var orchestratorRegistrations = registry.GetRegistrations()
                        .Where(r => r.ServiceType.Name.EndsWith("Orchestrator"))
                        .ToList();
                    
                    foreach (var registration in orchestratorRegistrations)
                    {
                        if (registration.Lifetime != ServiceLifetime.Scoped)
                        {
                            violations.Add($"{installerType.FullName}: Orchestrator {registration.ServiceType.Name} should be registered as Scoped, not {registration.Lifetime}");
                        }
                    }
                }
                catch (Exception ex)
                {
                    violations.Add($"{installerType.FullName} failed to analyze: {ex.Message}");
                }
            }

            // Assert
            violations.ShouldBeEmpty($"Orchestrator registration violations found: {string.Join(", ", violations)}");
        }

        /// <summary>
        /// Tests that configurations are always registered as singleton services.
        /// Enforces proper configuration lifecycle management.
        /// </summary>
        public static void Configurations_Should_Be_Registered_As_Singleton(Assembly pluginAssembly)
        {
            // Arrange
            var violations = new List<string>();
            
            var installerTypes = Types.InAssembly(pluginAssembly)
                .That()
                .ImplementInterface(typeof(BarkMoon.GameComposition.Core.Interfaces.IPluginInstaller))
                .And()
                .AreClasses()
                .GetTypes();

            // Act - Check configuration registration patterns
            foreach (var installerType in installerTypes)
            {
                // Skip test classes
                if (installerType.Name.Contains("Test"))
                    continue;

                var configureMethod = installerType.GetMethod("ConfigureServices");
                if (configureMethod == null) continue;

                try
                {
                    var installer = Activator.CreateInstance(installerType);
                    var registry = new TestServiceRegistry();
                    
                    configureMethod.Invoke(installer, new object[] { registry });
                    
                    // Check that all configurations are registered as singleton
                    var configurationRegistrations = registry.GetRegistrations()
                        .Where(r => r.ServiceType.Name.EndsWith("Configuration"))
                        .ToList();
                    
                    foreach (var registration in configurationRegistrations)
                    {
                        if (registration.Lifetime != ServiceLifetime.Singleton)
                        {
                            violations.Add($"{installerType.FullName}: Configuration {registration.ServiceType.Name} should be registered as Singleton, not {registration.Lifetime}");
                        }
                    }
                }
                catch (Exception ex)
                {
                    violations.Add($"{installerType.FullName} failed to analyze: {ex.Message}");
                }
            }

            // Assert
            violations.ShouldBeEmpty($"Configuration registration violations found: {string.Join(", ", violations)}");
        }

        /// <summary>
        /// Tests that event bus is registered as singleton and only once per plugin.
        /// Enforces proper event bus sharing across orchestrators.
        /// </summary>
        public static void Event_Bus_Should_Be_Registered_As_Singleton_Once(Assembly pluginAssembly)
        {
            // Arrange
            var violations = new List<string>();
            
            var installerTypes = Types.InAssembly(pluginAssembly)
                .That()
                .ImplementInterface(typeof(BarkMoon.GameComposition.Core.Interfaces.IPluginInstaller))
                .And()
                .AreClasses()
                .GetTypes();

            // Act - Check event bus registration patterns
            foreach (var installerType in installerTypes)
            {
                // Skip test classes
                if (installerType.Name.Contains("Test"))
                    continue;

                var configureMethod = installerType.GetMethod("ConfigureServices");
                if (configureMethod == null) continue;

                try
                {
                    var installer = Activator.CreateInstance(installerType);
                    var registry = new TestServiceRegistry();
                    
                    configureMethod.Invoke(installer, new object[] { registry });
                    
                    // Check event bus registrations
                    var eventBusRegistrations = registry.GetRegistrations()
                        .Where(r => r.ServiceType == typeof(BarkMoon.GameComposition.Core.Interfaces.IEventBus) || 
                                   r.ServiceType.Name.Contains("EventBus"))
                        .ToList();
                    
                    if (eventBusRegistrations.Count == 0)
                    {
                        violations.Add($"{installerType.FullName}: Should register IEventBus as singleton");
                    }
                    else if (eventBusRegistrations.Count > 1)
                    {
                        violations.Add($"{installerType.FullName}: IEventBus should be registered only once, found {eventBusRegistrations.Count} registrations");
                    }
                    else
                    {
                        var registration = eventBusRegistrations.First();
                        if (registration.Lifetime != ServiceLifetime.Singleton)
                        {
                            violations.Add($"{installerType.FullName}: IEventBus should be registered as Singleton, not {registration.Lifetime}");
                        }
                    }
                }
                catch (Exception ex)
                {
                    violations.Add($"{installerType.FullName} failed to analyze: {ex.Message}");
                }
            }

            // Assert
            violations.ShouldBeEmpty($"Event bus registration violations found: {string.Join(", ", violations)}");
        }

        private static void ValidateRegistrationOrder(List<string> registrationOrder, string installerName, List<string> violations)
        {
            // Expected order: Configuration → Services → Orchestrators → Event Bus
            var configIndex = registrationOrder.FindIndex(r => r.Contains("Configuration"));
            var serviceIndex = registrationOrder.FindIndex(r => r.Contains("Service") && !r.Contains("Orchestrator"));
            var orchestratorIndex = registrationOrder.FindIndex(r => r.Contains("Orchestrator"));
            var eventBusIndex = registrationOrder.FindIndex(r => r.Contains("EventBus"));

            // Validate basic order constraints
            if (configIndex != -1 && serviceIndex != -1 && configIndex > serviceIndex)
            {
                violations.Add($"{installerName}: Configuration should be registered before Services");
            }

            if (serviceIndex != -1 && orchestratorIndex != -1 && serviceIndex > orchestratorIndex)
            {
                violations.Add($"{installerName}: Services should be registered before Orchestrators");
            }

            if (orchestratorIndex != -1 && eventBusIndex != -1 && orchestratorIndex > eventBusIndex)
            {
                violations.Add($"{installerName}: Orchestrators should be registered before Event Bus");
            }
        }
    }

    // Test helper classes for architectural validation
    public enum ServiceLifetime
    {
        Singleton,
        Scoped,
        Transient
    }

    public class ServiceRegistration
    {
        public Type ServiceType { get; set; }
        public Type ImplementationType { get; set; }
        public ServiceLifetime Lifetime { get; set; }
        public int Order { get; set; }
    }

    public class TestServiceRegistry : BarkMoon.GameComposition.Core.Services.DI.ServiceRegistry
    {
        private readonly List<ServiceRegistration> _registrations = new();
        private int _orderCounter = 0;

        public List<ServiceRegistration> GetRegistrations() => _registrations.ToList();
        public List<string> GetRegistrationOrder() => _registrations.Select(r => r.ServiceType.Name).ToList();

        // Override registration methods to capture order and details
        public new void RegisterSingleton<TService>(TService instance)
        {
            _registrations.Add(new ServiceRegistration
            {
                ServiceType = typeof(TService),
                ImplementationType = typeof(TService),
                Lifetime = ServiceLifetime.Singleton,
                Order = _orderCounter++
            });
        }

        public new void RegisterSingleton<TService, TImplementation>() where TImplementation : class, TService
        {
            _registrations.Add(new ServiceRegistration
            {
                ServiceType = typeof(TService),
                ImplementationType = typeof(TImplementation),
                Lifetime = ServiceLifetime.Singleton,
                Order = _orderCounter++
            });
        }

        public new void RegisterScoped<TService>()
        {
            _registrations.Add(new ServiceRegistration
            {
                ServiceType = typeof(TService),
                ImplementationType = typeof(TService),
                Lifetime = ServiceLifetime.Scoped,
                Order = _orderCounter++
            });
        }

        public new void RegisterScoped<TService, TImplementation>() where TImplementation : class, TService
        {
            _registrations.Add(new ServiceRegistration
            {
                ServiceType = typeof(TService),
                ImplementationType = typeof(TImplementation),
                Lifetime = ServiceLifetime.Scoped,
                Order = _orderCounter++
            });
        }
    }
}
