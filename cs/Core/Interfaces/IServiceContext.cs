namespace GameComposition.Core.Interfaces
{
    /// <summary>
    /// Interface for service context to enable Godot-free dependency injection
    /// </summary>
    public interface IServiceContext
    {
        /// <summary>
        /// Name of the context for debugging
        /// </summary>
        string Name { get; }
        
        /// <summary>
        /// Gets the parent context if available
        /// </summary>
        IServiceContext? Parent { get; }
    }
}
