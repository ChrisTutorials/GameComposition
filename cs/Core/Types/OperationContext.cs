using System;
using System.Collections.Generic;

namespace BarkMoon.GameComposition.Core.Types;

/// <summary>
/// Base class for operation contexts providing standardized metadata
/// for cross-plugin operations and event handling.
/// 
/// This enables plugins to pass consistent context information for
/// operations like crafting, trading, dropping, and building without
/// requiring custom context implementations.
/// </summary>
public abstract class OperationContext
{
    /// <summary>
    /// The entity or object performing the operation
    /// </summary>
    public object? Source { get; set; }
    
    /// <summary>
    /// The position where the operation is occurring
    /// </summary>
    public Vector2? Position { get; set; }
    
    /// <summary>
    /// The level or tier of the source entity
    /// </summary>
    public int Level { get; set; } = 1;
    
    /// <summary>
    /// General modifier affecting the operation (luck, skill, etc.)
    /// </summary>
    public float Modifier { get; set; } = 1.0f;
    
    /// <summary>
    /// Additional context data that doesn't belong in specific properties
    /// </summary>
    public Dictionary<string, object> Data { get; set; } = new();
    
    /// <summary>
    /// The current game time (if applicable)
    /// </summary>
    public DateTime? GameTime { get; set; }
    
    /// <summary>
    /// Whether this operation is critical or has special importance
    /// </summary>
    public bool IsCritical { get; set; }
    
    /// <summary>
    /// Unique identifier for this operation instance
    /// </summary>
    public string OperationId { get; set; } = Guid.NewGuid().ToString();
    
    /// <summary>
    /// Timestamp when this context was created
    /// </summary>
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;
    
    /// <summary>
    /// Gets a value from the context data
    /// </summary>
    /// <typeparam name="T">The type of value to get</typeparam>
    /// <param name="key">The data key</param>
    /// <param name="defaultValue">Default value if key not found</param>
    /// <returns>The value or default</returns>
    public T GetData<T>(string key, T defaultValue = default!)
    {
        if (Data.TryGetValue(key, out var value) && value is T typedValue)
        {
            return typedValue;
        }
        return defaultValue;
    }
    
    /// <summary>
    /// Checks if a key exists in the context data
    /// </summary>
    /// <param name="key">The data key</param>
    /// <returns>True if the key exists</returns>
    public bool HasData(string key)
    {
        return Data.ContainsKey(key);
    }
    
    /// <summary>
    /// Tries to get a typed value from the context data
    /// </summary>
    /// <typeparam name="T">The type of value to get</typeparam>
    /// <param name="key">The data key</param>
    /// <param name="value">Output value</param>
    /// <returns>True if the value was found and converted</returns>
    public bool TryGetData<T>(string key, out T value)
    {
        if (Data.TryGetValue(key, out var raw) && raw is T typed)
        {
            value = typed;
            return true;
        }
        
        value = default!;
        return false;
    }
    
    /// <summary>
    /// Sets a value in the context data
    /// </summary>
    /// <param name="key">The data key</param>
    /// <param name="value">The value to set</param>
    public void SetData(string key, object value)
    {
        Data[key] = value;
    }
    
    /// <summary>
    /// Removes a value from the context data
    /// </summary>
    /// <param name="key">The data key to remove</param>
    /// <returns>True if the key was removed</returns>
    public bool RemoveData(string key)
    {
        return Data.Remove(key);
    }
    
    /// <summary>
    /// Clears all context data
    /// </summary>
    public void ClearData()
    {
        Data.Clear();
    }
    
    /// <summary>
    /// Creates a copy of this context
    /// </summary>
    /// <returns>A new context with copied values</returns>
    public abstract OperationContext Clone();
    
    /// <summary>
    /// Validates that this context is properly configured
    /// </summary>
    /// <returns>True if the context is valid</returns>
    public virtual bool IsValid()
    {
        return !string.IsNullOrEmpty(OperationId) && Timestamp != default;
    }
    
    /// <summary>
    /// Returns a string representation of this operation context.
    /// </summary>
    /// <returns>A string showing the context type, operation ID, and position</returns>
    public override string ToString()
    {
        return $"{GetType().Name} {OperationId} at {Position}";
    }
}
