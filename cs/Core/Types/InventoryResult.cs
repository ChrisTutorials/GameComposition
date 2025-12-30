using System.Collections.Generic;

namespace BarkMoon.GameComposition.Core.Types;

/// <summary>
/// Result of an inventory operation providing standardized success/failure
/// information for cross-plugin compatibility.
/// 
/// This enables plugins to return consistent operation results for
/// inventory operations without requiring custom result implementations.
/// </summary>
public class InventoryResult
{
    /// <summary>
    /// Whether the operation was successful
    /// </summary>
    public bool Success { get; set; }
    
    /// <summary>
    /// Error message if operation failed
    /// </summary>
    public string? ErrorMessage { get; set; }
    
    /// <summary>
    /// Error code for programmatic handling
    /// </summary>
    public string? ErrorCode { get; set; }
    
    /// <summary>
    /// Number of items actually processed
    /// </summary>
    public int ItemsProcessed { get; set; }
    
    /// <summary>
    /// Remaining capacity after operation
    /// </summary>
    public int RemainingCapacity { get; set; }
    
    /// <summary>
    /// Additional result data
    /// </summary>
    public Dictionary<string, object> Data { get; set; } = new();
    
    /// <summary>
    /// Items that were successfully processed
    /// </summary>
    public List<InventoryItem> ProcessedItems { get; set; } = new();
    
    /// <summary>
    /// Items that were rejected (if any)
    /// </summary>
    public List<InventoryItem> RejectedItems { get; set; } = new();
    
    /// <summary>
    /// Whether the operation was partially successful
    /// </summary>
    public bool IsPartialSuccess => Success && (RejectedItems.Count > 0 || ItemsProcessed < (Data.TryGetValue("RequestedItems", out var requested) && requested is int reqInt ? reqInt : 0));
    
    /// <summary>
    /// Whether the operation completely failed
    /// </summary>
    public bool IsCompleteFailure => !Success && ItemsProcessed == 0;
    
    /// <summary>
    /// Creates a successful result
    /// </summary>
    /// <param name="itemsProcessed">Number of items processed</param>
    /// <param name="remainingCapacity">Remaining capacity</param>
    /// <param name="processedItems">Items that were processed</param>
    /// <returns>Success result</returns>
    public static InventoryResult SuccessResult(
        int itemsProcessed = 0, 
        int remainingCapacity = 0, 
        IEnumerable<InventoryItem>? processedItems = null)
    {
        return new InventoryResult
        {
            Success = true,
            ItemsProcessed = itemsProcessed,
            RemainingCapacity = remainingCapacity,
            ProcessedItems = processedItems != null ? new List<InventoryItem>(processedItems) : new List<InventoryItem>()
        };
    }
    
    /// <summary>
    /// Creates a failure result
    /// </summary>
    /// <param name="errorMessage">Error message</param>
    /// <param name="errorCode">Error code</param>
    /// <param name="rejectedItems">Items that were rejected</param>
    /// <returns>Failure result</returns>
    public static InventoryResult FailureResult(
        string errorMessage, 
        string? errorCode = null, 
        IEnumerable<InventoryItem>? rejectedItems = null)
    {
        return new InventoryResult
        {
            Success = false,
            ErrorMessage = errorMessage,
            ErrorCode = errorCode,
            RejectedItems = rejectedItems != null ? new List<InventoryItem>(rejectedItems) : new List<InventoryItem>()
        };
    }
    
    /// <summary>
    /// Creates a partial success result
    /// </summary>
    /// <param name="itemsProcessed">Number of items processed</param>
    /// <param name="remainingCapacity">Remaining capacity</param>
    /// <param name="processedItems">Items that were processed</param>
    /// <param name="rejectedItems">Items that were rejected</param>
    /// <param name="message">Optional message</param>
    /// <returns>Partial success result</returns>
    public static InventoryResult PartialSuccessResult(
        int itemsProcessed,
        int remainingCapacity,
        IEnumerable<InventoryItem>? processedItems = null,
        IEnumerable<InventoryItem>? rejectedItems = null,
        string? message = null)
    {
        return new InventoryResult
        {
            Success = true,
            ItemsProcessed = itemsProcessed,
            RemainingCapacity = remainingCapacity,
            ProcessedItems = processedItems != null ? new List<InventoryItem>(processedItems) : new List<InventoryItem>(),
            RejectedItems = rejectedItems != null ? new List<InventoryItem>(rejectedItems) : new List<InventoryItem>(),
            ErrorMessage = message
        };
    }
    
    /// <summary>
    /// Gets data value
    /// </summary>
    /// <typeparam name="T">Type of value</typeparam>
    /// <param name="key">Data key</param>
    /// <param name="defaultValue">Default value</param>
    /// <returns>Data value or default</returns>
    public T GetData<T>(string key, T defaultValue = default!)
    {
        if (Data.TryGetValue(key, out var value) && value is T typedValue)
        {
            return typedValue;
        }
        return defaultValue;
    }
    
    /// <summary>
    /// Sets data value
    /// </summary>
    /// <param name="key">Data key</param>
    /// <param name="value">Data value</param>
    public void SetData(string key, object value)
    {
        Data[key] = value;
    }
    
    /// <summary>
    /// Checks if data contains a key
    /// </summary>
    /// <param name="key">Data key</param>
    /// <returns>True if key exists</returns>
    public bool HasData(string key)
    {
        return Data.ContainsKey(key);
    }
    
    /// <summary>
    /// Adds a processed item to the result
    /// </summary>
    /// <param name="item">Item to add</param>
    public void AddProcessedItem(InventoryItem item)
    {
        ProcessedItems.Add(item);
        ItemsProcessed += item.Quantity;
    }
    
    /// <summary>
    /// Adds a rejected item to the result
    /// </summary>
    /// <param name="item">Item to add</param>
    public void AddRejectedItem(InventoryItem item)
    {
        RejectedItems.Add(item);
    }
    
    /// <summary>
    /// Merges another result into this one
    /// </summary>
    /// <param name="other">Other result to merge</param>
    public void Merge(InventoryResult other)
    {
        Success = Success && other.Success;
        ItemsProcessed += other.ItemsProcessed;
        RemainingCapacity = other.RemainingCapacity;
        
        if (!string.IsNullOrEmpty(other.ErrorMessage) && string.IsNullOrEmpty(ErrorMessage))
        {
            ErrorMessage = other.ErrorMessage;
        }
        
        if (!string.IsNullOrEmpty(other.ErrorCode) && string.IsNullOrEmpty(ErrorCode))
        {
            ErrorCode = other.ErrorCode;
        }
        
        ProcessedItems.AddRange(other.ProcessedItems);
        RejectedItems.AddRange(other.RejectedItems);
        
        foreach (var kvp in other.Data)
        {
            Data[kvp.Key] = kvp.Value;
        }
    }
    
    public override string ToString()
    {
        var status = Success ? "SUCCESS" : "FAILURE";
        if (IsPartialSuccess) status = "PARTIAL";
        
        return $"{status}: {ItemsProcessed} items processed, {RemainingCapacity} capacity remaining" +
               (!string.IsNullOrEmpty(ErrorMessage) ? $" - {ErrorMessage}" : "");
    }
}
