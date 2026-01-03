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
            foreach (var line in lines)
            {
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
                    }
                }
            }

            return config;
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
    /// </summary>
    public class ArchitectureConfig
    {
        public HashSet<string> AllowedCrossDomainDependencies { get; set; } = new();
        public HashSet<string> AllowedNamespaceViolations { get; set; } = new();
        public HashSet<string> RequiredServiceInterfaces { get; set; } = new();
        public HashSet<string> DomainPatterns { get; set; } = new();
        public HashSet<string> ServicePatterns { get; set; } = new();
        public HashSet<string> StatePatterns { get; set; } = new();
        public HashSet<string> EventPatterns { get; set; } = new();
    }
}
