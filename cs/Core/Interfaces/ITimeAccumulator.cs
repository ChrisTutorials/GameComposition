using System;

namespace BarkMoon.GameComposition.Core.Interfaces
{
    /// <summary>
    /// Interface for accumulating delta time, allowing for headless simulation parity
    /// between engine-driven and pure C# execution environments.
    /// </summary>
    public interface ITimeAccumulator
    {
        /// <summary>
        /// Gets the current accumulated time in seconds.
        /// </summary>
        double TotalTimeSeconds { get; }

        /// <summary>
        /// Gets the last delta time processed in seconds.
        /// </summary>
        double LastDeltaSeconds { get; }

        /// <summary>
        /// Processes a step of time.
        /// </summary>
        /// <param name="delta">The time elapsed since the last step in seconds.</param>
        void ProcessStep(double delta);

        /// <summary>
        /// Resets the accumulator to zero.
        /// </summary>
        void Reset();
    }
}
