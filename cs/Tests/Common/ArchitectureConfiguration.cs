using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using YamlDotNet.Serialization;

namespace BarkMoon.GameComposition.Tests.Common
{
    /// <summary>
    /// Configuration model for architectural tests loaded from YAML.
    /// Provides externalized configuration to reduce compilation churn.
    /// </summary>
    public class ArchitectureConfiguration
    {
        public ProhibitedPatterns ProhibitedPatterns { get; set; } = new();
        public EventArchitecture EventArchitecture { get; set; } = new();
        public ServiceBasedArchitecture ServiceBasedArchitecture { get; set; } = new();
        public TestConfiguration TestConfiguration { get; set; } = new();
        public List<string> AllowedCrossDomainDependencies { get; set; } = new();
        public List<string> AllowedNamespaceViolations { get; set; } = new();
        public List<string> RequiredServiceInterfaces { get; set; } = new();
    }

    public class ProhibitedPatterns
    {
        public GlobalEventBusRule GlobalEventBus { get; set; } = new();
        public WeakTypingRule WeakTyping { get; set; } = new();
    }

    public class GlobalEventBusRule
    {
        public bool Enabled { get; set; } = true;
        public string Pattern { get; set; } = "*GlobalEventBus";
        public string Message { get; set; } = "GlobalEventBus pattern is prohibited. Use IEventDispatcher injection instead.";
        public List<string> AllowedDomainSpecific { get; set; } = new();
    }

    public class WeakTypingRule
    {
        public bool Enabled { get; set; } = true;
        public string Pattern { get; set; } = "Dictionary<string, object>";
        public string Message { get; set; } = "Dictionary<string, object> is prohibited for domain data. Use strongly typed classes instead.";
        public List<string> AllowedContexts { get; set; } = new();
    }

    public class EventArchitecture
    {
        public InterfacesInCoreRule InterfacesInCore { get; set; } = new();
        public DispatcherInjectionRule DispatcherInjection { get; set; } = new();
    }

    public class InterfacesInCoreRule
    {
        public bool Enabled { get; set; } = true;
        public List<string> CoreNamespaces { get; set; } = new();
    }

    public class DispatcherInjectionRule
    {
        public bool Enabled { get; set; } = true;
        public List<string> AllowedInterfaces { get; set; } = new();
    }

    public class ServiceBasedArchitecture
    {
        public CoreGodotSeparationRule CoreGodotSeparation { get; set; } = new();
        public AdapterPatternRule AdapterPattern { get; set; } = new();
        public ServicePatternRule ServicePattern { get; set; } = new();
    }

    public class CoreGodotSeparationRule
    {
        public bool Enabled { get; set; } = true;
        public List<string> CorePatterns { get; set; } = new();
        public List<string> GodotPatterns { get; set; } = new();
    }

    public class AdapterPatternRule
    {
        public bool Enabled { get; set; } = true;
        public List<string> AdapterPatterns { get; set; } = new();
        public string BridgeDirection { get; set; } = "EngineToCore";
    }

    public class ServicePatternRule
    {
        public bool Enabled { get; set; } = true;
        public List<string> ServicePatterns { get; set; } = new();
        public string Layer { get; set; } = "Core";
    }

    public class TestConfiguration
    {
        public bool ParallelExecution { get; set; } = true;
        public bool FailFast { get; set; } = false;
        public int MaxViolationsPerTest { get; set; } = 10;
    }

    /// <summary>
    /// Loads architecture configuration from YAML file using YamlDotNet.
    /// Falls back to defaults if file is not found.
    /// </summary>
    public static class ArchitectureConfigurationLoader
    {
        private static ArchitectureConfiguration? _cachedConfig;

        public static ArchitectureConfiguration LoadConfiguration()
        {
            if (_cachedConfig != null)
                return _cachedConfig;

            var configPath = GetConfigurationPath();
            
            if (File.Exists(configPath))
            {
                try
                {
                    var yamlContent = File.ReadAllText(configPath);
                    var deserializer = new DeserializerBuilder()
                        .IgnoreUnmatchedProperties()
                        .Build();
                    
                    _cachedConfig = deserializer.Deserialize<ArchitectureConfiguration>(yamlContent);
                    
                    // Validate loaded configuration
                    ValidateConfiguration(_cachedConfig);
                }
                catch (Exception ex)
                {
                    throw new InvalidOperationException($"Failed to load architecture configuration from {configPath}: {ex.Message}", ex);
                }
            }
            else
            {
                _cachedConfig = CreateDefaultConfiguration();
            }

            return _cachedConfig;
        }

        private static string GetConfigurationPath()
        {
            var assemblyLocation = System.Reflection.Assembly.GetExecutingAssembly().Location;
            var assemblyDir = Path.GetDirectoryName(assemblyLocation) ?? "";
            return Path.Combine(assemblyDir, "architecture-config.yaml");
        }

        private static void ValidateConfiguration(ArchitectureConfiguration config)
        {
            if (config?.ProhibitedPatterns?.GlobalEventBus == null)
                throw new InvalidOperationException("Configuration must contain ProhibitedPatterns.GlobalEventBus section");
            
            if (config?.TestConfiguration == null)
                throw new InvalidOperationException("Configuration must contain TestConfiguration section");
        }

        private static ArchitectureConfiguration CreateDefaultConfiguration()
        {
            return new ArchitectureConfiguration
            {
                ProhibitedPatterns = new ProhibitedPatterns
                {
                    GlobalEventBus = new GlobalEventBusRule
                    {
                        Enabled = true,
                        Pattern = "*GlobalEventBus",
                        Message = "GlobalEventBus pattern is prohibited. Use IEventDispatcher injection instead.",
                        AllowedDomainSpecific = new List<string>
                        {
                            "PlacementEventBus",
                            "GridEventBus",
                            "TargetingEventBus",
                            "InputEventBus"
                        }
                    },
                    WeakTyping = new WeakTypingRule
                    {
                        Enabled = true,
                        Pattern = "Dictionary<string, object>",
                        Message = "Dictionary<string, object> is prohibited for domain data. Use strongly typed classes instead.",
                        AllowedContexts = new List<string> { "UI.*", "External.*" }
                    }
                },
                EventArchitecture = new EventArchitecture
                {
                    InterfacesInCore = new InterfacesInCoreRule
                    {
                        Enabled = true,
                        CoreNamespaces = new List<string> { "*.Core.*", "BarkMoon.*.Core.*" }
                    },
                    DispatcherInjection = new DispatcherInjectionRule
                    {
                        Enabled = true,
                        AllowedInterfaces = new List<string> { "IEventDispatcher", "IEventBus" }
                    }
                },
                ServiceBasedArchitecture = new ServiceBasedArchitecture
                {
                    CoreGodotSeparation = new CoreGodotSeparationRule
                    {
                        Enabled = true,
                        CorePatterns = new List<string> { "*.Core.*", "BarkMoon.*.Core.*" },
                        GodotPatterns = new List<string> { "*.Godot.*", "BarkMoon.*.Godot.*", "*Adapter.cs" }
                    },
                    AdapterPattern = new AdapterPatternRule
                    {
                        Enabled = true,
                        AdapterPatterns = new List<string> { "*Adapter", "Godot*Adapter" },
                        BridgeDirection = "EngineToCore"
                    },
                    ServicePattern = new ServicePatternRule
                    {
                        Enabled = true,
                        ServicePatterns = new List<string> { "*Service", "*Router", "*Workflow", "*Orchestrator" },
                        Layer = "Core"
                    }
                },
                TestConfiguration = new TestConfiguration
                {
                    ParallelExecution = true,
                    FailFast = false,
                    MaxViolationsPerTest = 10
                }
            };
        }

        public static void ResetCache()
        {
            _cachedConfig = null;
        }
    }
}
