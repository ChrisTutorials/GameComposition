namespace BarkMoon.GameComposition.Core.Interfaces
{
    /// <summary>
    /// Interface for hybrid event adapters that provide both C# events and Godot signals.
    /// 
    /// This interface establishes the contract for all HybridEventAdapter implementations,
    /// ensuring consistent patterns for dual event emission:
    /// - C# events for high-performance C# consumption
    /// - Godot signals for GDScript compatibility
    /// 
    /// Implementations should be "dumb" adapters that only translate events,
    /// without owning services or containing business logic.
    /// </summary>
    public interface IEventAdapter
    {
        /// <summary>
        /// Gets whether the adapter is currently active and subscribed to events.
        /// </summary>
        bool IsActive { get; }

        /// <summary>
        /// Starts the adapter and begins event translation.
        /// </summary>
        void Start();

        /// <summary>
        /// Stops the adapter and ends event translation.
        /// </summary>
        void Stop();

        /// <summary>
        /// Performs cleanup of adapter resources.
        /// </summary>
        void Dispose();
    }

    /// <summary>
    /// Generic interface for hybrid event adapters with typed event data.
    /// 
    /// This provides type safety for the event data while maintaining
    /// the dual C# event / Godot signal pattern.
    /// </summary>
    /// <typeparam name="T">The type of event data transmitted.</typeparam>
    public interface IEventAdapter<T> : IEventAdapter
    {
        /// <summary>
        /// C# event for high-performance consumption by C# components.
        /// </summary>
        event System.EventHandler<T>? CSharpEvent;

        /// <summary>
        /// Godot signal name for GDScript compatibility.
        /// This should correspond to a [Signal] attribute on the implementing class.
        /// </summary>
        string GodotSignalName { get; }
    }
}
