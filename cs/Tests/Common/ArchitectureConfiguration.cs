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
        public ViewModelRules ViewModelRules { get; set; } = new();
        public ResultsInterfaceHierarchy ResultsInterfaceHierarchy { get; set; } = new();
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

    /// <summary>
    /// Defines event architecture rules for architectural validation.
    /// </summary>
    public class EventArchitecture
    {
        /// <summary>
        /// Rules for interfaces in Core.
        /// </summary>
        public InterfacesInCoreRule InterfacesInCore { get; set; } = new();
        
        /// <summary>
        /// Rules for dispatcher injection.
        /// </summary>
        public DispatcherInjectionRule DispatcherInjection { get; set; } = new();
    }

    /// <summary>
    /// Defines rules for interfaces in Core namespace.
    /// </summary>
    public class InterfacesInCoreRule
    {
        /// <summary>
        /// Whether interfaces in Core rules are enforced.
        /// </summary>
        public bool Enabled { get; set; } = true;
        
        /// <summary>
        /// List of Core namespaces where interfaces should be located.
        /// </summary>
        public List<string> CoreNamespaces { get; set; } = new();
    }

    /// <summary>
    /// Defines rules for dispatcher injection validation.
    /// </summary>
    public class DispatcherInjectionRule
    {
        /// <summary>
        /// Whether dispatcher injection rules are enforced.
        /// </summary>
        public bool Enabled { get; set; } = true;
        
        /// <summary>
        /// List of allowed interfaces for dispatcher injection.
        /// </summary>
        public List<string> AllowedInterfaces { get; set; } = new();
    }

    /// <summary>
    /// Defines service-based architecture rules for architectural validation.
    /// </summary>
    public class ServiceBasedArchitecture
    {
        /// <summary>
        /// Core/Godot separation rules.
        /// </summary>
        public CoreGodotSeparationRule CoreGodotSeparation { get; set; } = new();
        
        /// <summary>
        /// Adapter pattern rules.
        /// </summary>
        public AdapterPatternRule AdapterPattern { get; set; } = new();
        
        /// <summary>
        /// Service pattern rules.
        /// </summary>
        public ServicePatternRule ServicePattern { get; set; } = new();
    }

    /// <summary>
    /// Defines Core/Godot separation rules for architectural validation.
    /// </summary>
    public class CoreGodotSeparationRule
    {
        /// <summary>
        /// Whether Core/Godot separation rules are enforced.
        /// </summary>
        public bool Enabled { get; set; } = true;
        
        /// <summary>
        /// List of Core namespace patterns.
        /// </summary>
        public List<string> CorePatterns { get; set; } = new();
        
        /// <summary>
        /// List of Godot namespace patterns.
        /// </summary>
        public List<string> GodotPatterns { get; set; } = new();
    }

    /// <summary>
    /// Defines adapter pattern rules for architectural validation.
    /// </summary>
    public class AdapterPatternRule
    {
        /// <summary>
        /// Whether adapter pattern rules are enforced.
        /// </summary>
        public bool Enabled { get; set; } = true;
        
        /// <summary>
        /// List of adapter patterns.
        /// </summary>
        public List<string> AdapterPatterns { get; set; } = new();
        
        /// <summary>
        /// Direction of the bridge (EngineToCore or CoreToEngine).
        /// </summary>
        public string BridgeDirection { get; set; } = "EngineToCore";
    }

    /// <summary>
    /// Defines service pattern rules for architectural validation.
    /// </summary>
    public class ServicePatternRule
    {
        /// <summary>
        /// Whether service pattern rules are enforced.
        /// </summary>
        public bool Enabled { get; set; } = true;
        
        /// <summary>
        /// List of service patterns.
        /// </summary>
        public List<string> ServicePatterns { get; set; } = new();
        
        /// <summary>
        /// Layer where services should be located.
        /// </summary>
        public string Layer { get; set; } = "Core";
    }

    /// <summary>
    /// Configuration for architectural test execution.
    /// </summary>
    public class TestConfiguration
    {
        /// <summary>
        /// Whether tests should run in parallel.
        /// </summary>
        public bool ParallelExecution { get; set; } = true;
        
        /// <summary>
        /// Whether tests should fail fast on first violation.
        /// </summary>
        public bool FailFast { get; set; } = false;
        
        /// <summary>
        /// Maximum number of violations to report per test.
        /// </summary>
        public int MaxViolationsPerTest { get; set; } = 10;
    }

    /// <summary>
    /// Loads architecture configuration from YAML file using YamlDotNet.
    /// Falls back to defaults if file is not found.
    /// </summary>
    public static class ArchitectureConfigurationLoader
    {
        private static ArchitectureConfiguration? _cachedConfig;

        /// <summary>
        /// Loads architecture configuration from YAML file.
        /// </summary>
        /// <returns>Loaded configuration or default if file not found</returns>
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

        /// <summary>
        /// Resets the cached configuration, forcing reload on next access.
        /// </summary>
        public static void ResetCache()
        {
            _cachedConfig = null;
        }
    }

    /// <summary>
    /// Defines ViewModel rules for architectural validation.
    /// </summary>
    public class ViewModelRules
    {
        /// <summary>
        /// Core ViewModel rules.
        /// </summary>
        public CoreViewModelRules CoreViewModels { get; set; } = new();
        
        /// <summary>
        /// Godot ViewModel rules.
        /// </summary>
        public GodotViewModelRules GodotViewModels { get; set; } = new();
        
        /// <summary>
        /// Converter rules.
        /// </summary>
        public ConverterRules Converters { get; set; } = new();
        
        /// <summary>
        /// Pairing rules.
        /// </summary>
        public PairingRules PairingRules { get; set; } = new();
        
        /// <summary>
        /// Test configuration.
        /// </summary>
        public ViewModelTestConfiguration TestConfiguration { get; set; } = new();
    }

    /// <summary>
    /// Defines rules for Core ViewModels in architectural validation.
    /// </summary>
    public class CoreViewModelRules
    {
        /// <summary>
        /// Whether Core ViewModels must be structs.
        /// </summary>
        public bool MustBeStruct { get; set; } = true;
        
        /// <summary>
        /// Whether Core ViewModels must be in Core namespace.
        /// </summary>
        public bool MustBeInCoreNamespace { get; set; } = true;
        
        /// <summary>
        /// Whether Core ViewModels must use Core types.
        /// </summary>
        public bool MustUseCoreTypes { get; set; } = true;
        
        /// <summary>
        /// List of forbidden member types.
        /// </summary>
        public List<string> ForbiddenMembers { get; set; } = new();
        
        /// <summary>
        /// List of allowed member types.
        /// </summary>
        public List<string> AllowedMembers { get; set; } = new();
        
        /// <summary>
        /// Naming pattern for Core ViewModels.
        /// </summary>
        public string NamingPattern { get; set; } = "*ViewModel";
        
        /// <summary>
        /// Namespace pattern for Core ViewModels.
        /// </summary>
        public string NamespacePattern { get; set; } = "BarkMoon.GridPlacement.Core.*";
        
        /// <summary>
        /// List of allowed type patterns.
        /// </summary>
        public List<string> AllowedTypePatterns { get; set; } = new();
    }

    /// <summary>
    /// Defines rules for Godot ViewModels in architectural validation.
    /// </summary>
    public class GodotViewModelRules
    {
        /// <summary>
        /// Whether Godot ViewModels must be structs.
        /// </summary>
        public bool MustBeStruct { get; set; } = true;
        
        /// <summary>
        /// Whether Godot ViewModels must be in Godot namespace.
        /// </summary>
        public bool MustBeInGodotNamespace { get; set; } = true;
        
        /// <summary>
        /// Whether Godot ViewModels must use Godot types.
        /// </summary>
        public bool MustUseGodotTypes { get; set; } = true;
        
        /// <summary>
        /// List of forbidden member types.
        /// </summary>
        public List<string> ForbiddenMembers { get; set; } = new();
        
        /// <summary>
        /// List of allowed member types.
        /// </summary>
        public List<string> AllowedMembers { get; set; } = new();
        
        /// <summary>
        /// Naming pattern for Godot ViewModels.
        /// </summary>
        public string NamingPattern { get; set; } = "*ViewModel";
        
        /// <summary>
        /// Namespace pattern for Godot ViewModels.
        /// </summary>
        public string NamespacePattern { get; set; } = "BarkMoon.GridPlacement.Godot.*";
        
        /// <summary>
        /// List of allowed type patterns.
        /// </summary>
        public List<string> AllowedTypePatterns { get; set; } = new();
    }

    /// <summary>
    /// Defines converter rules for Core/Godot ViewModel separation.
    /// </summary>
    public class ConverterRules
    {
        /// <summary>
        /// Whether converters must be classes (not interfaces).
        /// </summary>
        public bool MustBeClass { get; set; } = true;
        
        /// <summary>
        /// Whether converters must be in Godot namespace.
        /// </summary>
        public bool MustBeInGodotNamespace { get; set; } = true;
        
        /// <summary>
        /// Whether converters must have a Convert method.
        /// </summary>
        public bool MustHaveConvertMethod { get; set; } = true;
        
        /// <summary>
        /// Naming pattern for converter classes.
        /// </summary>
        public string NamingPattern { get; set; } = "*Converter";
        
        /// <summary>
        /// Required method name for conversion.
        /// </summary>
        public string RequiredMethod { get; set; } = "ConvertToGodot";
        
        /// <summary>
        /// Namespace pattern where converters should be located.
        /// </summary>
        public string NamespacePattern { get; set; } = "BarkMoon.GridPlacement.Godot.*";
    }

    /// <summary>
    /// Defines pairing rules for Core/Godot ViewModel separation.
    /// </summary>
    public class PairingRules
    {
        /// <summary>
        /// Whether Core/Godot pairing is required.
        /// </summary>
        public bool RequireCoreGodotPairs { get; set; } = true;
        
        /// <summary>
        /// Suffix for Core to Godot conversion.
        /// </summary>
        public string CoreToGodotSuffix { get; set; } = "Godot";
        
        /// <summary>
        /// Suffix for Godot to Core conversion.
        /// </summary>
        public string GodotToCoreSuffix { get; set; } = "Core";
        
        /// <summary>
        /// Message format for missing pair violations.
        /// </summary>
        public string MissingPairViolationMessage { get; set; } = "Missing {missing_type}ViewModel - {existing_type}ViewModel exists without {missing_type} counterpart";
    }

    /// <summary>
    /// Configuration for ViewModel architectural tests.
    /// </summary>
    public class ViewModelTestConfiguration
    {
        /// <summary>
        /// Expected number of violations for RED/GREEN testing.
        /// </summary>
        public int ExpectedViolations { get; set; } = 0;
        
        /// <summary>
        /// Format for violation messages.
        /// </summary>
        public string ViolationMessageFormat { get; set; } = "GREEN = Core/Godot separation with converters, RED = Missing Core/Godot pairing or converters";
        
        /// <summary>
        /// Success messages for test validation.
        /// </summary>
        public List<string> SuccessMessages { get; set; } = new();
        
        /// <summary>
        /// Failure messages for test validation.
        /// </summary>
        public List<string> FailureMessages { get; set; } = new();
    }

    /// <summary>
    /// Defines results interface hierarchy rules for architectural validation.
    /// </summary>
    public class ResultsInterfaceHierarchy
    {
        /// <summary>
        /// Interface inheritance rules.
        /// </summary>
        public InterfaceInheritanceRule InterfaceInheritance { get; set; } = new();
        
        /// <summary>
        /// Class implementation rules.
        /// </summary>
        public ClassImplementationRule ClassImplementation { get; set; } = new();
        
        /// <summary>
        /// Domain result rules.
        /// </summary>
        public DomainResultsRule DomainResults { get; set; } = new();
        
        /// <summary>
        /// Naming convention rules.
        /// </summary>
        public NamingConventionsRule NamingConventions { get; set; } = new();
        
        /// <summary>
        /// Expected violations for RED/GREEN testing.
        /// </summary>
        public ExpectedViolationsRule ExpectedViolations { get; set; } = new();
    }

    /// <summary>
    /// Defines interface inheritance rules for architectural validation.
    /// </summary>
    public class InterfaceInheritanceRule
    {
        /// <summary>
        /// Whether interface inheritance rules are enforced.
        /// </summary>
        public bool Enabled { get; set; } = true;
        
        /// <summary>
        /// List of required hierarchy items.
        /// </summary>
        public List<RequiredHierarchyItem> RequiredHierarchy { get; set; } = new();
    }

    /// <summary>
    /// Defines required hierarchy items for architectural validation.
    /// </summary>
    public class RequiredHierarchyItem
    {
        /// <summary>
        /// Interface name that must exist.
        /// </summary>
        public string Interface { get; set; } = "";
        
        /// <summary>
        /// Base class that the interface must inherit from.
        /// </summary>
        public string MustInheritFrom { get; set; } = "";
        
        /// <summary>
        /// Expected location of the interface.
        /// </summary>
        public string Location { get; set; } = "";
        
        /// <summary>
        /// Error message for violations.
        /// </summary>
        public string Message { get; set; } = "";
    }

    /// <summary>
    /// Defines rules for class implementation requirements.
    /// </summary>
    public class ClassImplementationRule
    {
        /// <summary>
        /// Whether class implementation rules are enforced.
        /// </summary>
        public bool Enabled { get; set; } = true;
        
        /// <summary>
        /// Name of the class that must implement the interface.
        /// </summary>
        public string Class { get; set; } = "";
        
        /// <summary>
        /// Interface that the class must implement.
        /// </summary>
        public string MustImplement { get; set; } = "";
        
        /// <summary>
        /// Expected location of the class.
        /// </summary>
        public string Location { get; set; } = "";
        
        /// <summary>
        /// Error message for violations.
        /// </summary>
        public string Message { get; set; } = "";
    }

    /// <summary>
    /// Defines rules for domain result classes.
    /// </summary>
    public class DomainResultsRule
    {
        /// <summary>
        /// Whether domain result rules are enforced.
        /// </summary>
        public bool Enabled { get; set; } = true;
        
        /// <summary>
        /// Pattern for result class names.
        /// </summary>
        public string Pattern { get; set; } = "*Result";
        
        /// <summary>
        /// Required base class for result classes.
        /// </summary>
        public string MustInheritFrom { get; set; } = "ValidationResult";
        
        /// <summary>
        /// Whether to exclude framework classes from validation.
        /// </summary>
        public bool ExcludeFrameworkClass { get; set; } = true;
        
        /// <summary>
        /// Error message for violations.
        /// </summary>
        public string Message { get; set; } = "";
    }

    /// <summary>
    /// Defines naming conventions for architectural elements.
    /// </summary>
    public class NamingConventionsRule
    {
        /// <summary>
        /// Whether naming conventions are enforced.
        /// </summary>
        public bool Enabled { get; set; } = true;
        
        /// <summary>
        /// Interface naming rules.
        /// </summary>
        public InterfaceNamingRule Interfaces { get; set; } = new();
        
        /// <summary>
        /// Class naming rules.
        /// </summary>
        public ClassNamingRule Classes { get; set; } = new();
    }

    /// <summary>
    /// Defines interface naming conventions.
    /// </summary>
    public class InterfaceNamingRule
    {
        /// <summary>
        /// Required prefix for interface names.
        /// </summary>
        public string MustStartWith { get; set; } = "I";
        
        /// <summary>
        /// Required suffix for interface names.
        /// </summary>
        public string MustEndWith { get; set; } = "Result";
    }

    /// <summary>
    /// Defines class naming conventions.
    /// </summary>
    public class ClassNamingRule
    {
        /// <summary>
        /// Prohibited prefix for class names.
        /// </summary>
        public string MustNotStartWith { get; set; } = "I";
        
        /// <summary>
        /// Required suffix for class names.
        /// </summary>
        public string MustEndWith { get; set; } = "Result";
        
        /// <summary>
        /// List of exceptions to naming rules.
        /// </summary>
        public List<string> Exceptions { get; set; } = new();
    }

    /// <summary>
    /// Defines expected violations for RED/GREEN testing scenarios.
    /// </summary>
    public class ExpectedViolationsRule
    {
        /// <summary>
        /// Current state of the architecture (RED = violations exist, GREEN = compliant).
        /// </summary>
        public string CurrentState { get; set; } = "RED";
        
        /// <summary>
        /// List of missing interfaces that need to be implemented.
        /// </summary>
        public List<string> MissingInterfaces { get; set; } = new();
        
        /// <summary>
        /// List of missing implementations that need to be added.
        /// </summary>
        public List<string> MissingImplementations { get; set; } = new();
        
        /// <summary>
        /// Instructions for fixing the identified violations.
        /// </summary>
        public string FixInstructions { get; set; } = "";
    }
}
