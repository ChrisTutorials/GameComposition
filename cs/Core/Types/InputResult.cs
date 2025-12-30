using System;

namespace BarkMoon.GameComposition.Core.Types
{
    /// <summary>
    /// Standard implementation of input result for cross-plugin compatibility.
    /// Provides immutable result structure with error handling.
    /// </summary>
    public record InputResult(
        bool Handled,
        string? ErrorMessage = null
    ) : IInputResult;
}
