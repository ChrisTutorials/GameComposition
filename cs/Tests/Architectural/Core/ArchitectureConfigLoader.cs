using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;

namespace BarkMoon.GameComposition.Tests.Architectural
{
    /// <summary>
    /// Configuration loader for architectural tests.
    /// Supports both YAML (preferred) and JSON configuration formats.
    /// </summary>
    public static class ArchitectureConfigLoader
    {
        private static ArchitectureConfig? _cachedConfig;

        /// <summary>
        /// Loads architecture configuration from file or returns default.
        /// </summary>
        /// <returns>Architecture configuration</returns>
        public static ArchitectureConfig LoadConfig()
        {
            if (_cachedConfig != null)
                return _cachedConfig;

            var configPath = GetConfigFilePath();
            
            if (File.Exists(configPath))
            {
                try
                {
                    // Try YAML first (if we add YAML support later)
                    if (configPath.EndsWith(".yaml", StringComparison.OrdinalIgnoreCase))
                    {
                        _cachedConfig = LoadYamlConfig(configPath);
                    }
                    else
                    {
                        _cachedConfig = LoadJsonConfig(configPath);
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Warning: Failed to load architecture config from {configPath}: {ex.Message}");
                    _cachedConfig = GetDefaultConfig();
                }
            }
            else
            {
                Console.WriteLine($"Info: Architecture config file not found at {configPath}, using defaults");
                _cachedConfig = GetDefaultConfig();
            }

            return _cachedConfig;
        }

        private static string GetConfigFilePath()
        {
            // Look for config file in several locations
            var baseDir = AppDomain.CurrentDomain.BaseDirectory;
            var possiblePaths = new[]
            {
                Path.Combine(baseDir, "architecture-config.yaml"),
                Path.Combine(baseDir, "architecture-config.json"),
                Path.Combine(baseDir, "Architectural", "architecture-config.yaml"),
                Path.Combine(baseDir, "Architectural", "architecture-config.json")
            };

            return possiblePaths.FirstOrDefault(File.Exists) ?? possiblePaths[0];
        }

        private static ArchitectureConfig LoadYamlConfig(string configPath)
        {
            // For now, parse YAML manually (simple key-value pairs)
            // In future, we could add a YAML parser library
            var lines = File.ReadAllLines(configPath);
            var config = new ArchitectureConfig();

            var currentSection = "";
            for (int i = 0; i < lines.Length; i++)
            {
                var line = lines[i];
                var trimmed = line.Trim();
                
                // Skip comments and empty lines
                if (string.IsNullOrEmpty(trimmed) || trimmed.StartsWith("#"))
                    continue;

                // Section headers
                if (trimmed.EndsWith(":"))
                {
                    currentSection = trimmed.Replace(":", "").ToLowerInvariant();
                    continue;
                }

                // Key-value pairs
                if (trimmed.Contains(":"))
                {
                    var parts = trimmed.Split(new[] { ':' }, 2);
                    var key = parts[0].Trim().Replace("-", "").ToLowerInvariant();
                    var value = parts[1].Trim();

                    switch (currentSection)
                    {
                        case "allowed_cross_domain_dependencies":
                            if (value.StartsWith("-"))
                                config.AllowedCrossDomainDependencies.Add(value.TrimStart('-', ' ').Trim('"'));
                            break;

                        case "allowed_namespace_violations":
                            if (value.StartsWith("-"))
                                config.AllowedNamespaceViolations.Add(value.TrimStart('-', ' ').Trim('"'));
                            break;

                        case "required_service_interfaces":
                            if (value.StartsWith("-"))
                                config.RequiredServiceInterfaces.Add(value.TrimStart('-', ' ').Trim('"'));
                            break;

                        case "domain_patterns":
                            if (value.StartsWith("-"))
                                config.DomainPatterns.Add(value.TrimStart('-', ' ').Trim('"'));
                            break;

                        case "service_patterns":
                            if (value.StartsWith("-"))
                                config.ServicePatterns.Add(value.TrimStart('-', ' ').Trim('"'));
                            break;

                        case "state_patterns":
                            if (value.StartsWith("-"))
                                config.StatePatterns.Add(value.TrimStart('-', ' ').Trim('"'));
                            break;

                        case "event_patterns":
                            if (value.StartsWith("-"))
                                config.EventPatterns.Add(value.TrimStart('-', ' ').Trim('"'));
                            break;
                            
                        // Enhanced prohibited patterns parsing
                        case "prohibited_patterns":
                            ParseProhibitedPatterns(config, lines, ref i);
                            break;
                            
                        case "strong_typing":
                            ParseStrongTyping(config, lines, ref i);
                            break;
                            
                        case "service_based_architecture":
                            ParseServiceBasedArchitecture(config, lines, ref i);
                            break;
                            
                        case "event_architecture":
                            ParseEventArchitecture(config, lines, ref i);
                            break;
                            
                        case "object_service_architecture":
                            ParseObjectServiceArchitecture(config, lines, ref i);
                            break;
                    }
                }
            }

            return config;
        }

        private static void ParseProhibitedPatterns(ArchitectureConfig config, string[] lines, ref int i)
        {
            var currentSubSection = "";
            
            for (i++; i < lines.Length; i++)
            {
                var line = lines[i].Trim();
                if (string.IsNullOrEmpty(line) || line.StartsWith("#")) continue;
                
                if (line.EndsWith(":"))
                {
                    currentSubSection = line.Replace(":", "").ToLowerInvariant();
                    continue;
                }
                
                if (line.Contains(":"))
                {
                    var parts = line.Split(new[] { ':' }, 2);
                    var key = parts[0].Trim().Replace("-", "").ToLowerInvariant();
                    var value = parts[1].Trim();
                    
                    switch (currentSubSection)
                    {
                        case "global_event_bus":
                            ParseGlobalEventBus(config.ProhibitedPatterns.GlobalEventBus, key, value);
                            break;
                        case "weak_typing":
                            ParseWeakTyping(config.ProhibitedPatterns.WeakTyping, key, value);
                            break;
                        case "custom_implementations":
                            ParseCustomImplementations(config.ProhibitedPatterns.CustomImplementations, key, value);
                            break;
                        case "engine_dependencies":
                            ParseEngineDependencies(config.ProhibitedPatterns.EngineDependencies, key, value);
                            break;
                    }
                }
                else if (line.StartsWith("  ") && !line.Contains(":"))
                {
                    // Handle list items
                    var listItem = line.TrimStart('-', ' ').Trim();
                    if (!string.IsNullOrEmpty(listItem))
                    {
                        switch (currentSubSection)
                        {
                            case "global_event_bus":
                                if (listItem.StartsWith("test_assemblies:") || listItem.StartsWith("allowed_domain_specific:"))
                                    continue; // Skip subsection headers
                                config.ProhibitedPatterns.GlobalEventBus.TestAssemblies.Add(listItem.Trim('"'));
                                break;
                            case "weak_typing":
                                if (listItem.StartsWith("target_namespaces:") || listItem.StartsWith("name_patterns:"))
                                    continue;
                                if (listItem.StartsWith("target_namespaces"))
                                    config.ProhibitedPatterns.WeakTyping.TargetNamespaces.Add(listItem.Trim('"'));
                                else if (listItem.StartsWith("name_patterns"))
                                    config.ProhibitedPatterns.WeakTyping.NamePatterns.Add(listItem.Trim('"'));
                                break;
                        }
                    }
                }
                else if (!line.StartsWith("  "))
                {
                    // End of prohibited_patterns section
                    i--; // Back up one line
                    break;
                }
            }
        }
        
        private static void ParseGlobalEventBus(GlobalEventBusConfig config, string key, string value)
        {
            switch (key)
            {
                case "enabled":
                    config.Enabled = bool.Parse(value);
                    break;
                case "pattern":
                    config.Pattern = value.Trim('"');
                    break;
                case "message":
                    config.Message = value.Trim('"');
                    break;
            }
        }
        
        private static void ParseWeakTyping(WeakTypingConfig config, string key, string value)
        {
            switch (key)
            {
                case "enabled":
                    config.Enabled = bool.Parse(value);
                    break;
                case "pattern":
                    config.Pattern = value.Trim('"');
                    break;
                case "message":
                    config.Message = value.Trim('"');
                    break;
            }
        }
        
        private static void ParseCustomImplementations(CustomImplementationsConfig config, string key, string value)
        {
            switch (key)
            {
                case "enabled":
                    config.Enabled = bool.Parse(value);
                    break;
                case "message":
                    config.Message = value.Trim('"');
                    break;
            }
        }
        
        private static void ParseEngineDependencies(EngineDependenciesConfig config, string key, string value)
        {
            switch (key)
            {
                case "enabled":
                    config.Enabled = bool.Parse(value);
                    break;
                case "message":
                    config.Message = value.Trim('"');
                    break;
            }
        }
        
        private static void ParseStrongTyping(ArchitectureConfig config, string[] lines, ref int i)
        {
            // Implementation for strong typing section parsing
            // Similar structure to ParseProhibitedPatterns
            for (i++; i < lines.Length; i++)
            {
                var line = lines[i].Trim();
                if (string.IsNullOrEmpty(line) || line.StartsWith("#")) continue;
                
                if (!line.StartsWith("  ") && !line.Contains(":"))
                {
                    i--; // Back up one line
                    break;
                }
            }
        }
        
        private static void ParseServiceBasedArchitecture(ArchitectureConfig config, string[] lines, ref int i)
        {
            // Implementation for service-based architecture section parsing
            for (i++; i < lines.Length; i++)
            {
                var line = lines[i].Trim();
                if (string.IsNullOrEmpty(line) || line.StartsWith("#")) continue;
                
                if (!line.StartsWith("  ") && !line.Contains(":"))
                {
                    i--; // Back up one line
                    break;
                }
            }
        }
        
        private static void ParseEventArchitecture(ArchitectureConfig config, string[] lines, ref int i)
        {
            // Implementation for event architecture section parsing
            for (i++; i < lines.Length; i++)
            {
                var line = lines[i].Trim();
                if (string.IsNullOrEmpty(line) || line.StartsWith("#")) continue;
                
                if (!line.StartsWith("  ") && !line.Contains(":"))
                {
                    i--; // Back up one line
                    break;
                }
            }
        }
        
        private static void ParseObjectServiceArchitecture(ArchitectureConfig config, string[] lines, ref int i)
        {
            var currentSubSection = "";
            
            for (i++; i < lines.Length; i++)
            {
                var line = lines[i].Trim();
                if (string.IsNullOrEmpty(line) || line.StartsWith("#")) continue;
                
                if (line.EndsWith(":"))
                {
                    currentSubSection = line.Replace(":", "").ToLowerInvariant();
                    continue;
                }
                
                if (line.Contains(":"))
                {
                    var parts = line.Split(new[] { ':' }, 2);
                    var key = parts[0].Trim().Replace("-", "").ToLowerInvariant();
                    var value = parts[1].Trim();
                    
                    switch (currentSubSection)
                    {
                        case "framework_interface":
                            ParseFrameworkInterface(config.ObjectServiceArchitecture.FrameworkInterface, key, value);
                            break;
                        case "plugin_implementations":
                            ParsePluginImplementations(config.ObjectServiceArchitecture.PluginImplementations, key, value);
                            break;
                        case "engine_agnostic_naming":
                            ParseEngineAgnosticNaming(config.ObjectServiceArchitecture.EngineAgnosticNaming, key, value);
                            break;
                        case "required_interface_methods":
                            ParseRequiredInterfaceMethods(config.ObjectServiceArchitecture.RequiredInterfaceMethods, key, value);
                            break;
                        case "engine_types_in_interface":
                            ParseEngineTypesInInterface(config.ObjectServiceArchitecture.EngineTypesInInterface, key, value);
                            break;
                        case "engine_types_in_implementation":
                            ParseEngineTypesInImplementation(config.ObjectServiceArchitecture.EngineTypesInImplementation, key, value);
                            break;
                        case "migration_violations":
                            ParseMigrationViolations(config.ObjectServiceArchitecture.MigrationViolations, key, value);
                            break;
                    }
                }
                else if (line.StartsWith("  ") && !line.Contains(":"))
                {
                    // Handle list items
                    var listItem = line.TrimStart('-', ' ').Trim();
                    if (!string.IsNullOrEmpty(listItem))
                    {
                        switch (currentSubSection)
                        {
                            case "engine_agnostic_naming":
                                if (listItem.StartsWith("prohibited_terms:") || listItem.StartsWith("required_terms:"))
                                    continue;
                                if (listItem.StartsWith("prohibited"))
                                    config.ObjectServiceArchitecture.EngineAgnosticNaming.ProhibitedTerms.Add(listItem.Trim('"'));
                                else if (listItem.StartsWith("required"))
                                    config.ObjectServiceArchitecture.EngineAgnosticNaming.RequiredTerms.Add(listItem.Trim('"'));
                                break;
                            case "required_interface_methods":
                                config.ObjectServiceArchitecture.RequiredInterfaceMethods.RequiredMethods.Add(listItem.Trim('"'));
                                break;
                            case "engine_types_in_interface":
                                if (listItem.StartsWith("prohibited_types:") || listItem.StartsWith("allowed_types:"))
                                    continue;
                                if (listItem.StartsWith("prohibited"))
                                    config.ObjectServiceArchitecture.EngineTypesInInterface.ProhibitedTypes.Add(listItem.Trim('"'));
                                else if (listItem.StartsWith("allowed"))
                                    config.ObjectServiceArchitecture.EngineTypesInInterface.AllowedTypes.Add(listItem.Trim('"'));
                                break;
                            case "engine_types_in_implementation":
                                if (listItem.StartsWith("allowed_implementation_types:"))
                                    continue;
                                config.ObjectServiceArchitecture.EngineTypesInImplementation.AllowedImplementationTypes.Add(listItem.Trim('"'));
                                break;
                            case "migration_violations":
                                if (listItem.StartsWith("deprecated_patterns:"))
                                    continue;
                                config.ObjectServiceArchitecture.MigrationViolations.DeprecatedPatterns.Add(listItem.Trim('"'));
                                break;
                        }
                    }
                }
                else if (!line.StartsWith("  "))
                {
                    // End of object_service_architecture section
                    i--; // Back up one line
                    break;
                }
            }
        }
        
        private static void ParseFrameworkInterface(FrameworkInterfaceConfig config, string key, string value)
        {
            switch (key)
            {
                case "enabled":
                    config.Enabled = bool.Parse(value);
                    break;
                case "interfacename":
                    config.InterfaceName = value.Trim('"');
                    break;
                case "expectedlocation":
                    config.ExpectedLocation = value.Trim('"');
                    break;
                case "namespacepattern":
                    config.NamespacePattern = value.Trim('"');
                    break;
            }
        }
        
        private static void ParsePluginImplementations(PluginImplementationsConfig config, string key, string value)
        {
            switch (key)
            {
                case "enabled":
                    config.Enabled = bool.Parse(value);
                    break;
                case "implementationpattern":
                    config.ImplementationPattern = value.Trim('"');
                    break;
                case "expectedlocation":
                    config.ExpectedLocation = value.Trim('"');
                    break;
                case "namespacepattern":
                    config.NamespacePattern = value.Trim('"');
                    break;
            }
        }
        
        private static void ParseEngineAgnosticNaming(EngineAgnosticNamingConfig config, string key, string value)
        {
            switch (key)
            {
                case "enabled":
                    config.Enabled = bool.Parse(value);
                    break;
                case "message":
                    config.Message = value.Trim('"');
                    break;
            }
        }
        
        private static void ParseRequiredInterfaceMethods(RequiredInterfaceMethodsConfig config, string key, string value)
        {
            switch (key)
            {
                case "enabled":
                    config.Enabled = bool.Parse(value);
                    break;
                case "message":
                    config.Message = value.Trim('"');
                    break;
            }
        }
        
        private static void ParseEngineTypesInInterface(EngineTypesInInterfaceConfig config, string key, string value)
        {
            switch (key)
            {
                case "enabled":
                    config.Enabled = bool.Parse(value);
                    break;
                case "message":
                    config.Message = value.Trim('"');
                    break;
            }
        }
        
        private static void ParseEngineTypesInImplementation(EngineTypesInImplementationConfig config, string key, string value)
        {
            switch (key)
            {
                case "enabled":
                    config.Enabled = bool.Parse(value);
                    break;
                case "message":
                    config.Message = value.Trim('"');
                    break;
            }
        }
        
        private static void ParseMigrationViolations(MigrationViolationsConfig config, string key, string value)
        {
            switch (key)
            {
                case "enabled":
                    config.Enabled = bool.Parse(value);
                    break;
                case "migrationmessage":
                    config.MigrationMessage = value.Trim('"');
                    break;
                case "alloweduntil":
                    config.AllowedUntil = value.Trim('"');
                    break;
            }
        }
        
        private static ArchitectureConfig LoadJsonConfig(string configPath)
        {
            var json = File.ReadAllText(configPath);
            return JsonSerializer.Deserialize<ArchitectureConfig>(json) ?? GetDefaultConfig();
        }

        private static ArchitectureConfig GetDefaultConfig()
        {
            return new ArchitectureConfig
            {
                AllowedCrossDomainDependencies = new HashSet<string>
                {
                    "Input->Targeting",
                    "Targeting->Manipulation", 
                    "Manipulation->Placement",
                    "Workflows->Input",
                    "Workflows->Targeting",
                    "Workflows->Manipulation",
                    "Workflows->Placement"
                },
                AllowedNamespaceViolations = new HashSet<string>
                {
                    "Legacy.*",
                    "Internal.*"
                },
                RequiredServiceInterfaces = new HashSet<string>
                {
                    "IService",
                    "I*Service"
                },
                DomainPatterns = new HashSet<string>
                {
                    "Targeting", "Manipulation", "Placement", "Grid", 
                    "Input", "Workflows", "Validation", "Events"
                },
                ServicePatterns = new HashSet<string>
                {
                    "*Service", "*Service", "*Adapter", "*Workflow", "*Orchestrator"
                },
                StatePatterns = new HashSet<string>
                {
                    "*State", "*Snapshot", "*Context", "*Data"
                },
                EventPatterns = new HashSet<string>
                {
                    "*Event", "*EventArgs"
                }
            };
        }
    }

    /// <summary>
    /// Configuration model for architectural tests.
    /// Supports comprehensive rule enforcement across plugins and domains.
    /// </summary>
    public class ArchitectureConfig
    {
        // Existing properties...
        public HashSet<string> AllowedCrossDomainDependencies { get; set; } = new();
        public HashSet<string> AllowedNamespaceViolations { get; set; } = new();
        public HashSet<string> RequiredServiceInterfaces { get; set; } = new();
        public HashSet<string> DomainPatterns { get; set; } = new();
        public HashSet<string> ServicePatterns { get; set; } = new();
        public HashSet<string> StatePatterns { get; set; } = new();
        public HashSet<string> EventPatterns { get; set; } = new();
        
        // Enhanced prohibited patterns support
        public ProhibitedPatternsConfig ProhibitedPatterns { get; set; } = new();
        public StrongTypingConfig StrongTyping { get; set; } = new();
        public ServiceBasedArchitectureConfig ServiceBasedArchitecture { get; set; } = new();
        public EventArchitectureConfig EventArchitecture { get; set; } = new();
        public ObjectServiceArchitectureConfig ObjectServiceArchitecture { get; set; } = new();
    }
    
    /// <summary>
    /// Configuration for prohibited patterns.
    /// </summary>
    public class ProhibitedPatternsConfig
    {
        public GlobalEventBusConfig GlobalEventBus { get; set; } = new();
        public WeakTypingConfig WeakTyping { get; set; } = new();
        public CustomImplementationsConfig CustomImplementations { get; set; } = new();
        public EngineDependenciesConfig EngineDependencies { get; set; } = new();
    }
    
    public class GlobalEventBusConfig
    {
        public bool Enabled { get; set; }
        public string Pattern { get; set; } = "*GlobalEventBus";
        public string Message { get; set; } = "GlobalEventBus pattern is prohibited. Use IEventDispatcher injection instead.";
        public List<string> AllowedDomainSpecific { get; set; } = new();
        public List<string> TestAssemblies { get; set; } = new();
    }
    
    public class WeakTypingConfig
    {
        public bool Enabled { get; set; }
        public string Pattern { get; set; } = "Dictionary<string, object>";
        public string Message { get; set; } = "Dictionary<string, object> is prohibited for domain data. Use strongly typed classes instead.";
        public List<string> AllowedContexts { get; set; } = new();
        public List<string> TargetNamespaces { get; set; } = new();
        public List<string> NamePatterns { get; set; } = new();
    }
    
    public class CustomImplementationsConfig
    {
        public bool Enabled { get; set; }
        public List<string> ProhibitedNames { get; set; } = new();
        public string Message { get; set; } = "Custom implementations are prohibited. Use Microsoft.Extensions.* instead.";
        public List<string> TargetAssemblies { get; set; } = new();
    }
    
    public class EngineDependenciesConfig
    {
        public bool Enabled { get; set; }
        public List<string> ProhibitedDependencies { get; set; } = new();
        public string Message { get; set; } = "Engine dependencies are prohibited in Core packages. Core must be engine-agnostic.";
        public List<string> TargetAssemblies { get; set; } = new();
        public List<string> AllowedExceptions { get; set; } = new();
    }
    
    /// <summary>
    /// Configuration for strong typing rules.
    /// </summary>
    public class StrongTypingConfig
    {
        public ProhibitObjectTypingConfig ProhibitObjectTyping { get; set; } = new();
        public RequiredReturnTypesConfig RequiredReturnTypes { get; set; } = new();
    }
    
    public class ProhibitObjectTypingConfig
    {
        public bool Enabled { get; set; }
        public List<string> TargetInterfaces { get; set; } = new();
        public List<string> ProhibitedTypes { get; set; } = new();
        public string Message { get; set; } = "Object typing is prohibited in service interfaces. Use strong types instead.";
    }
    
    public class RequiredReturnTypesConfig
    {
        public bool Enabled { get; set; }
        public List<string> InterfacePatterns { get; set; } = new();
        public List<string> ProhibitedReturnTypes { get; set; } = new();
        public string Message { get; set; } = "Service methods must return strong types, not object.";
    }
    
    /// <summary>
    /// Configuration for Service-Based Architecture rules.
    /// </summary>
    public class ServiceBasedArchitectureConfig
    {
        public CoreGodotSeparationConfig CoreGodotSeparation { get; set; } = new();
        public AdapterPatternConfig AdapterPattern { get; set; } = new();
        public ServicePatternConfig ServicePattern { get; set; } = new();
    }
    
    public class CoreGodotSeparationConfig
    {
        public bool Enabled { get; set; }
        public List<string> CorePatterns { get; set; } = new();
        public List<string> GodotPatterns { get; set; } = new();
    }
    
    public class AdapterPatternConfig
    {
        public bool Enabled { get; set; }
        public List<string> AdapterPatterns { get; set; } = new();
        public string BridgeDirection { get; set; } = "EngineToCore";
    }
    
    public class ServicePatternConfig
    {
        public bool Enabled { get; set; }
        public List<string> ServicePatterns { get; set; } = new();
        public string Layer { get; set; } = "Core";
    }
    
    /// <summary>
    /// Configuration for Event architecture rules.
    /// </summary>
    public class EventArchitectureConfig
    {
        public InterfacesInCoreConfig InterfacesInCore { get; set; } = new();
        public DispatcherInjectionConfig DispatcherInjection { get; set; } = new();
    }
    
    public class InterfacesInCoreConfig
    {
        public bool Enabled { get; set; }
        public List<string> CoreNamespaces { get; set; } = new();
    }
    
    public class DispatcherInjectionConfig
    {
        public bool Enabled { get; set; }
        public List<string> AllowedInterfaces { get; set; } = new();
    }
    
    /// <summary>
    /// Configuration for ObjectService architecture rules.
    /// </summary>
    public class ObjectServiceArchitectureConfig
    {
        public FrameworkInterfaceConfig FrameworkInterface { get; set; } = new();
        public PluginImplementationsConfig PluginImplementations { get; set; } = new();
        public EngineAgnosticNamingConfig EngineAgnosticNaming { get; set; } = new();
        public RequiredInterfaceMethodsConfig RequiredInterfaceMethods { get; set; } = new();
        public EngineTypesInInterfaceConfig EngineTypesInInterface { get; set; } = new();
        public EngineTypesInImplementationConfig EngineTypesInImplementation { get; set; } = new();
        public MigrationViolationsConfig MigrationViolations { get; set; } = new();
    }
    
    public class FrameworkInterfaceConfig
    {
        public bool Enabled { get; set; }
        public string InterfaceName { get; set; } = "IObjectService";
        public string ExpectedLocation { get; set; } = "GameComposition.Core.Services";
        public string NamespacePattern { get; set; } = "BarkMoon.GameComposition.Core.Services";
    }
    
    public class PluginImplementationsConfig
    {
        public bool Enabled { get; set; }
        public string ImplementationPattern { get; set; } = "*ObjectService";
        public string ExpectedLocation { get; set; } = "*.Godot.*.Services";
        public string NamespacePattern { get; set; } = "BarkMoon.*.Godot.*.Services";
    }
    
    public class EngineAgnosticNamingConfig
    {
        public bool Enabled { get; set; }
        public List<string> ProhibitedTerms { get; set; } = new();
        public List<string> RequiredTerms { get; set; } = new();
        public string Message { get; set; } = "ObjectService must use engine-agnostic terminology. Avoid engine-specific terms like 'Scene', 'GameObject', or 'Actor'.";
    }
    
    public class RequiredInterfaceMethodsConfig
    {
        public bool Enabled { get; set; }
        public List<string> RequiredMethods { get; set; } = new();
        public string Message { get; set; } = "IObjectService must implement all required object lifecycle methods.";
    }
    
    public class EngineTypesInInterfaceConfig
    {
        public bool Enabled { get; set; }
        public List<string> ProhibitedTypes { get; set; } = new();
        public List<string> AllowedTypes { get; set; } = new();
        public string Message { get; set; } = "IObjectService interface must use only 'object' types to remain engine-agnostic. Engine-specific types should only be in implementations.";
    }
    
    public class EngineTypesInImplementationConfig
    {
        public bool Enabled { get; set; }
        public List<string> AllowedImplementationTypes { get; set; } = new();
        public string Message { get; set; } = "ObjectService implementations can use engine-specific types for their target engine.";
    }
    
    public class MigrationViolationsConfig
    {
        public bool Enabled { get; set; }
        public List<string> DeprecatedPatterns { get; set; } = new();
        public string MigrationMessage { get; set; } = "ISceneService is deprecated. Migrate to IObjectService for engine-agnostic object management.";
        public string AllowedUntil { get; set; } = "2025-02-01";
    }
}
