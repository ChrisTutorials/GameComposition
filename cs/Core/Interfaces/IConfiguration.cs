using System.Collections.Generic;

namespace BarkMoon.GameComposition.Core.Interfaces
{
    /// <summary>
    /// Base interface for all framework and domain configurations.
    /// Provides validation and self-check capabilities.
    /// </summary>
    public interface IConfiguration
    {
        /// <summary>
        /// Validates the current configuration state.
        /// </summary>
        /// <returns>True if the configuration is valid; otherwise, false.</returns>
        bool IsValid();

        /// <summary>
        /// Validates the configuration and returns a list of error messages.
        /// </summary>
        /// <returns>A list of validation error messages. Empty if valid.</returns>
        List<string> Validate();
    }
}
