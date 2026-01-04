namespace BarkMoon.GameComposition.Core.Interfaces
{
    /// <summary>
    /// Interface for hybrid event adapters that provide both C# events and Godot signals with signal forwarding capabilities.
    /// 
    /// This interface extends IEventAdapter<T> to establish the contract for signal forwarding
    /// and conditional conversion patterns, ensuring consistent hybrid adapter behavior:
    /// - C# events emit Core ViewModels (maximum C# performance)
    /// - Godot signals emit Godot ViewModels (signal compatibility)
    /// - Conditional conversion only when signal subscribers exist
    /// 
    /// Implementations should be "dumb" adapters that only translate events,
    /// without owning services or containing business logic.
    /// </summary>
    /// <typeparam name="T">The type of event data transmitted (must be a struct ViewModel).</typeparam>
    public interface IHybridEventAdapter<T> : IEventAdapter<T> where T : struct
    {
        /// <summary>
        /// Checks whether the adapter has any Godot signal subscribers.
        /// Used for conditional conversion optimization - only convert Core ViewModels
        /// to Godot ViewModels when there are actual signal subscribers.
        /// </summary>
        /// <returns>True if there are Godot signal subscribers, false otherwise.</returns>
        bool HasGodotSignalSubscribers();

        /// <summary>
        /// Gets the current count of Godot signal subscribers.
        /// Useful for debugging and optimization decisions.
        /// </summary>
        int GodotSignalSubscriberCount { get; }
    }
}
