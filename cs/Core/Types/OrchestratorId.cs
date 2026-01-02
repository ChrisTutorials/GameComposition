namespace BarkMoon.GameComposition.Core.Types
{
    /// <summary>
    /// Strongly-typed identifier for orchestrator instances.
    /// Uses high-performance numeric ID for optimal performance.
    /// </summary>
    public readonly record struct OrchestratorId(long Value)
    {
        /// <summary>
        /// Empty/unknown orchestrator ID (zero value).
        /// </summary>
        public static readonly OrchestratorId Empty = new(0);
        
        /// <summary>
        /// Creates a new unique orchestrator ID.
        /// </summary>
        public static OrchestratorId New() => new(System.Threading.Interlocked.Increment(ref _counter));
        
        private static long _counter = 1;
        
        /// <summary>
        /// Determines if this ID is empty (zero).
        /// </summary>
        public bool IsEmpty => Value == 0;
        
        /// <summary>
        /// Determines if this ID has a value (non-zero).
        /// </summary>
        public bool HasValue => Value != 0;
    }
}
