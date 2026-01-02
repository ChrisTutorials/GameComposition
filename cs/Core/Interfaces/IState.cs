using Microsoft.Extensions.ObjectPool;

namespace BarkMoon.GameComposition.Core.Interfaces
{
    /// <summary>
    /// Enhanced marker interface for state objects (Layer 1/2 Data).
    /// State = Pure Data Only - no business logic allowed.
    /// 
    /// <para>
    /// <strong>Performance Requirements:</strong>
    /// Implementations should use Microsoft.Extensions.ObjectPool for collections to eliminate allocations
    /// in high-frequency scenarios. This ensures consistent frame rates and reduces GC pressure.
    /// </para>
    /// 
    /// <para>
    /// <strong>Template Pattern:</strong>
    /// Think of state objects like Godot packed scenes - they should be reusable templates
    /// that can be quickly instantiated and reset rather than created from scratch.
    /// </para>
    /// 
    /// <para>
    /// <strong>Required Methods for Pooling:</strong>
    /// - <c>Reset()</c>: Clear all data and return to "factory fresh" state
    /// - <c>Initialize(ObjectPool)</c>: Set up Microsoft.Extensions.ObjectPool collection pools
    /// </para>
    /// 
    /// <para>
    /// <strong>State Lifecycle Properties:</strong>
    /// - <c>IsInitialized</c>: Whether the state has been properly initialized
    /// - <c>IsReady</c>: Whether the state is ready for operations (domain-specific)
    /// </para>
    /// </summary>
    public interface IState
    {
        /// <summary>
        /// Gets whether the state is ready for operations.
        /// This is domain-specific and indicates the state has all required data for its purpose.
        /// </summary>
        bool IsReady { get; }
    }
}
