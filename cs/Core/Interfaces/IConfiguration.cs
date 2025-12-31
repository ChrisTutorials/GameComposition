using System.Collections.Generic;

namespace BarkMoon.GameComposition.Core.Interfaces
{
    /// <summary>
    /// Base interface for all framework and domain configurations.
    /// Provides validation, copying, and self-check capabilities.
    /// 
    /// This interface combines validation and state copying capabilities
    /// to provide a complete configuration management contract.
    /// </summary>
    public interface IConfiguration : ICopyable
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
