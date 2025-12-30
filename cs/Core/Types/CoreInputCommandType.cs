using System;

namespace BarkMoon.GameComposition.Core.Types
{
    /// <summary>
    /// Generic core input command types for cross-plugin compatibility.
    /// Provides type-safe command handling for common system operations.
    /// </summary>
    public enum CoreInputCommandType
    {
        Unknown = 0,
        
        // Navigation/Selection
        NavigateStart,
        NavigateEnd,
        SelectStart,
        SelectEnd,
        
        // Action/Interaction
        ActionStart,
        ActionEnd,
        Activate,
        Deactivate,
        
        // State Changes
        StateChange,
        ModeChange,
        ContextChange,
        
        // System Events
        Initialize,
        Shutdown,
        Reset,
        
        // Data Operations
        DataCreate,
        DataUpdate,
        DataDelete,
        
        // Input Events
        InputReceived,
        InputProcessed,
        InputCancelled
    }
}
