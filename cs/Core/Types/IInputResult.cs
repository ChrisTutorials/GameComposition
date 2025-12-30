using System;

namespace BarkMoon.GameComposition.Core.Types
{
    /// <summary>
    /// Defines the contract for input processing results across all plugins.
    /// Provides standardized result structure for external consumers.
    /// </summary>
    public interface IInputResult
    {
        /// <summary>
        /// Whether the input was handled successfully.
        /// </summary>
        bool Handled { get; }
        
        /// <summary>
        /// Error message if handling failed, null if successful.
        /// </summary>
        string? ErrorMessage { get; }
    }
}
