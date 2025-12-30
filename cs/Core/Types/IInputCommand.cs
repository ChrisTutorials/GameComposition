using System;

namespace BarkMoon.GameComposition.Core.Types
{
    /// <summary>
    /// Defines the contract for input commands across all plugins.
    /// Provides standardized input command structure for external consumers.
    /// </summary>
    public interface IInputCommand
    {
        /// <summary>
        /// Core command type for generic operations.
        /// </summary>
        CoreInputCommandType CoreType { get; }
        
        /// <summary>
        /// Plugin-specific extension command type.
        /// Null when using core command type.
        /// </summary>
        string? ExtensionType { get; }
        
        /// <summary>
        /// Timestamp when the command was created.
        /// </summary>
        double Timestamp { get; }
    }
}
