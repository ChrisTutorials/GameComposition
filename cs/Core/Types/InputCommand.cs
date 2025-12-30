using System;

namespace BarkMoon.GameComposition.Core.Types
{
    /// <summary>
    /// Standard implementation of input command for cross-plugin compatibility.
    /// Provides type-safe core commands with flexible plugin extensions.
    /// </summary>
    public record InputCommand(
        CoreInputCommandType CoreType = CoreInputCommandType.Unknown,
        string? ExtensionType = null,
        double Timestamp = 0
    ) : IInputCommand
    {
        /// <summary>
        /// Gets the effective command type for routing decisions.
        /// Core types take precedence over extensions.
        /// </summary>
        public string GetEffectiveType() => 
            ExtensionType ?? CoreType.ToString();
        
        /// <summary>
        /// Whether this is a core system command.
        /// </summary>
        public bool IsCoreCommand => CoreType != CoreInputCommandType.Unknown;
        
        /// <summary>
        /// Whether this is a plugin-specific extension command.
        /// </summary>
        public bool IsExtensionCommand => !string.IsNullOrEmpty(ExtensionType);
        
        /// <summary>
        /// Creates a core command with type safety.
        /// </summary>
        public static InputCommand Core(CoreInputCommandType coreType, double timestamp = 0) =>
            new(coreType, null, timestamp);
        
        /// <summary>
        /// Creates a plugin extension command with flexibility.
        /// </summary>
        public static InputCommand Extension(string extensionType, double timestamp = 0) =>
            new(CoreInputCommandType.Unknown, extensionType, timestamp);
    }
}
