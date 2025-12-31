using System;

namespace BarkMoon.GameComposition.Core.Interfaces
{
    /// <summary>
    /// Interface for objects that can copy their state to and from other instances.
    /// Provides a standardized way to transfer configuration state between objects
    /// without requiring external mapping libraries or complex reflection.
    /// </summary>
    public interface ICopyable
    {
        /// <summary>
        /// Copies state from another configuration into this instance.
        /// Used for populating this object with data from a source configuration.
        /// </summary>
        /// <param name="source">Source configuration to copy from.</param>
        void CopyFrom(IConfiguration source);

        /// <summary>
        /// Copies state from this instance to another configuration.
        /// Used for transferring this object's data to a target configuration.
        /// </summary>
        /// <param name="target">Target configuration to copy to.</param>
        void CopyTo(IConfiguration target);
    }
}
