using System;
using System.Collections.Generic;
using BarkMoon.GameComposition.Core.Interfaces;

namespace BarkMoon.GameComposition.Core.Configuration
{
    /// <summary>
    /// Hybrid configuration factory that combines interface contracts with centralized conversion utilities.
    /// Provides game-wide configuration management across all plugins while maintaining clean separation of concerns.
    /// 
    /// This factory works with any IConfiguration implementation, enabling seamless conversion between
    /// Core configurations and Godot resources without requiring external dependencies.
    /// </summary>
    public static class ConfigurationFactory
    {
        /// <summary>
        /// Converts between any two IConfiguration implementations using interface contracts.
        /// This method works across all plugins and configuration types in the game ecosystem.
        /// </summary>
        /// <typeparam name="TTarget">Target configuration type.</typeparam>
        /// <param name="source">Source configuration.</param>
        /// <returns>Converted configuration of target type.</returns>
        public static TTarget ConvertTo<TTarget>(IConfiguration source) where TTarget : class, IConfiguration
        {
            if (source == null)
                ArgumentNullException.ThrowIfNull(source);

            var target = Activator.CreateInstance<TTarget>();
            
            // Use the new CopyFrom/CopyTo interface contract
            source.CopyTo(target);
            
            return target;
        }

        /// <summary>
        /// Creates a deep copy of any IConfiguration implementation.
        /// </summary>
        /// <typeparam name="T">Configuration type.</typeparam>
        /// <param name="config">Configuration to copy.</param>
        /// <returns>Copied configuration.</returns>
        public static T Copy<T>(T config) where T : class, IConfiguration
        {
            if (config == null)
                ArgumentNullException.ThrowIfNull(config);

            return ConvertTo<T>(config);
        }

        /// <summary>
        /// Validates any IConfiguration implementation.
        /// </summary>
        /// <param name="config">Configuration to validate.</param>
        /// <returns>Validation result with errors and warnings.</returns>
        public static ConfigurationValidationResult Validate(IConfiguration config)
        {
            if (config == null)
                ArgumentNullException.ThrowIfNull(config);

            var errors = config.Validate();
            var isValid = config.IsValid();

            return new ConfigurationValidationResult
            {
                IsValid = isValid,
                Errors = errors,
                Warnings = isValid ? new List<string>() : new List<string> { "Configuration validation failed" }
            };
        }

        /// <summary>
        /// Creates a default instance of any IConfiguration implementation.
        /// </summary>
        /// <typeparam name="T">Configuration type.</typeparam>
        /// <returns>Default configuration instance.</returns>
        public static T CreateDefault<T>() where T : class, IConfiguration
        {
            return Activator.CreateInstance<T>();
        }

        /// <summary>
        /// Compares two IConfiguration implementations for equality.
        /// </summary>
        /// <param name="left">First configuration.</param>
        /// <param name="right">Second configuration.</param>
        /// <returns>True if configurations are equal.</returns>
        public static bool AreEqual(IConfiguration left, IConfiguration right)
        {
            if (ReferenceEquals(left, right))
                return true;

            if (left == null || right == null)
                return false;

            // Use reflection to compare properties
            var leftProperties = left.GetType().GetProperties();
            var rightProperties = right.GetType().GetProperties();

            foreach (var leftProp in leftProperties)
            {
                if (!leftProp.CanRead || !leftProp.CanWrite)
                    continue;

                var rightProp = right.GetType().GetProperty(leftProp.Name);
                if (rightProp == null || !rightProp.CanRead || !rightProp.CanWrite)
                    continue;

                var leftValue = leftProp.GetValue(left);
                var rightValue = rightProp.GetValue(right);

                if (!Equals(leftValue, rightValue))
                    return false;
            }

            return true;
        }

        /// <summary>
        /// Batch converts multiple configurations to the same target type.
        /// </summary>
        /// <typeparam name="TTarget">Target configuration type.</typeparam>
        /// <param name="sources">Source configurations to convert.</param>
        /// <returns>Converted configurations.</returns>
        public static IEnumerable<TTarget> ConvertBatch<TTarget>(IEnumerable<IConfiguration> sources) where TTarget : class, IConfiguration
        {
            if (sources == null)
                ArgumentNullException.ThrowIfNull(sources);

            foreach (var source in sources)
            {
                yield return ConvertTo<TTarget>(source);
            }
        }

        /// <summary>
        /// Creates a configuration preset with common values.
        /// </summary>
        /// <typeparam name="T">Configuration type.</typeparam>
        /// <param name="preset">Preset type to create.</param>
        /// <returns>Configuration with preset values.</returns>
        public static T CreatePreset<T>(ConfigurationPreset preset) where T : class, IConfiguration
        {
            var config = Activator.CreateInstance<T>();
            
            // Apply preset based on common configuration patterns
            switch (preset)
            {
                case ConfigurationPreset.Default:
                    // Use default constructor values
                    break;
                case ConfigurationPreset.Development:
                    ApplyDevelopmentPreset(config);
                    break;
                case ConfigurationPreset.Production:
                    ApplyProductionPreset(config);
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(preset), preset, null);
            }

            return config;
        }

        #region Private Helper Methods

        /// <summary>
        /// Applies development preset values.
        /// </summary>
        private static void ApplyDevelopmentPreset(IConfiguration config)
        {
            // Enable debug features if available
            var debugProp = config.GetType().GetProperty("ShowDebugInfo");
            if (debugProp != null && debugProp.CanWrite)
            {
                debugProp.SetValue(config, true);
            }
        }

        /// <summary>
        /// Applies production preset values.
        /// </summary>
        private static void ApplyProductionPreset(IConfiguration config)
        {
            // Disable debug features if available
            var debugProp = config.GetType().GetProperty("ShowDebugInfo");
            if (debugProp != null && debugProp.CanWrite)
            {
                debugProp.SetValue(config, false);
            }
        }

        #endregion
    }

    /// <summary>
    /// Result of configuration validation.
    /// </summary>
    public class ConfigurationValidationResult
    {
        public bool IsValid { get; set; }
        public List<string> Errors { get; set; } = new List<string>();
        public List<string> Warnings { get; set; } = new List<string>();

        /// <summary>
        /// Gets a summary of the validation result.
        /// </summary>
        public string Summary => IsValid ? "Valid" : $"Invalid ({Errors.Count} errors, {Warnings.Count} warnings)";
    }

    /// <summary>
    /// Common configuration presets.
    /// </summary>
    public enum ConfigurationPreset
    {
        Default,
        Development,
        Production
    }
}
