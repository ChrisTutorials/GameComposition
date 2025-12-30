using System;
using System.Collections.Generic;

namespace BarkMoon.GameComposition.Core.Events;

/// <summary>
/// Base class for transaction events providing standardized transaction data
/// for cross-plugin event handling and analytics.
/// 
/// This enables plugins to emit consistent transaction events for
/// crafting, trading, dropping, and other operations without
/// requiring custom event implementations.
/// </summary>
public abstract class TransactionEvent : ServiceEvent
{
    /// <summary>
    /// Unique identifier for this transaction
    /// </summary>
    public string TransactionId { get; set; } = Guid.NewGuid().ToString();
    
    /// <summary>
    /// ID of the actor performing the transaction
    /// </summary>
    public string ActorId { get; set; } = string.Empty;
    
    /// <summary>
    /// ID of the target (shop, inventory, etc.)
    /// </summary>
    public string TargetId { get; set; } = string.Empty;
    
    /// <summary>
    /// ID of the item being transacted
    /// </summary>
    public string ItemId { get; set; } = string.Empty;
    
    /// <summary>
    /// Quantity of items in the transaction
    /// </summary>
    public int Quantity { get; set; }
    
    /// <summary>
    /// Type of transaction (craft, trade, drop, etc.)
    /// </summary>
    public string TransactionType { get; set; } = string.Empty;
    
    /// <summary>
    /// Additional transaction metadata
    /// </summary>
    public Dictionary<string, object> Metadata { get; set; } = new();
    
    /// <summary>
    /// Creates a new transaction event
    /// </summary>
    protected TransactionEvent() : base("TransactionSystem")
    {
    }
    
    /// <summary>
    /// Creates a new transaction event with specified values
    /// </summary>
    /// <param name="transactionId">Transaction ID</param>
    /// <param name="actorId">Actor ID</param>
    /// <param name="targetId">Target ID</param>
    /// <param name="itemId">Item ID</param>
    /// <param name="quantity">Quantity</param>
    /// <param name="transactionType">Transaction type</param>
    protected TransactionEvent(
        string transactionId,
        string actorId,
        string targetId,
        string itemId,
        int quantity,
        string transactionType) : base("TransactionSystem")
    {
        TransactionId = transactionId;
        ActorId = actorId;
        TargetId = targetId;
        ItemId = itemId;
        Quantity = quantity;
        TransactionType = transactionType;
    }
    
    /// <summary>
    /// Gets metadata value
    /// </summary>
    /// <typeparam name="T">Type of value</typeparam>
    /// <param name="key">Metadata key</param>
    /// <param name="defaultValue">Default value</param>
    /// <returns>Metadata value or default</returns>
    public T GetMetadata<T>(string key, T defaultValue = default!)
    {
        if (Metadata.TryGetValue(key, out var value) && value is T typedValue)
        {
            return typedValue;
        }
        return defaultValue;
    }
    
    /// <summary>
    /// Sets metadata value
    /// </summary>
    /// <param name="key">Metadata key</param>
    /// <param name="value">Metadata value</param>
    public void SetMetadata(string key, object value)
    {
        Metadata[key] = value;
    }
    
    /// <summary>
    /// Checks if metadata contains a key
    /// </summary>
    /// <param name="key">Metadata key</param>
    /// <returns>True if key exists</returns>
    public bool HasMetadata(string key)
    {
        return Metadata.ContainsKey(key);
    }
    
    public override string ToString()
    {
        return $"{TransactionType}: {ActorId} -> {TargetId} ({ItemId} x{Quantity}) [{TransactionId}]";
    }
}

/// <summary>
/// Event emitted when a transaction completes successfully
/// </summary>
public class CompletedTransactionEvent : TransactionEvent
{
    /// <summary>
    /// Price per unit (for trading transactions)
    /// </summary>
    public float PricePerUnit { get; set; }
    
    /// <summary>
    /// Total price (Quantity * PricePerUnit)
    /// </summary>
    public float TotalPrice => Quantity * PricePerUnit;
    
    /// <summary>
    /// Currency used for the transaction
    /// </summary>
    public string Currency { get; set; } = "gold";
    
    /// <summary>
    /// Whether this was a critical transaction
    /// </summary>
    public bool IsCritical { get; set; } = false;
    
    /// <summary>
    /// Creates a new completed transaction event
    /// </summary>
    public CompletedTransactionEvent() { }
    
    /// <summary>
    /// Creates a new completed transaction event with specified values
    /// </summary>
    /// <param name="transactionId">Transaction ID</param>
    /// <param name="actorId">Actor ID</param>
    /// <param name="targetId">Target ID</param>
    /// <param name="itemId">Item ID</param>
    /// <param name="quantity">Quantity</param>
    /// <param name="pricePerUnit">Price per unit</param>
    /// <param name="currency">Currency</param>
    /// <param name="transactionType">Transaction type</param>
    public CompletedTransactionEvent(
        string transactionId,
        string actorId,
        string targetId,
        string itemId,
        int quantity,
        float pricePerUnit,
        string currency = "gold",
        string transactionType = "trade")
        : base(transactionId, actorId, targetId, itemId, quantity, transactionType)
    {
        PricePerUnit = pricePerUnit;
        Currency = currency;
    }
    
    public override string ToString()
    {
        return $"COMPLETED {TransactionType}: {ActorId} -> {TargetId} ({ItemId} x{Quantity}) for {TotalPrice} {Currency} [{TransactionId}]";
    }
}

/// <summary>
/// Event emitted when a transaction fails
/// </summary>
public class FailedTransactionEvent : TransactionEvent
{
    /// <summary>
    /// Reason for the failure
    /// </summary>
    public string Reason { get; set; } = string.Empty;
    
    /// <summary>
    /// Error code for the failure
    /// </summary>
    public string ErrorCode { get; set; } = string.Empty;
    
    /// <summary>
    /// Whether this failure can be retried
    /// </summary>
    public bool CanRetry { get; set; } = false;
    
    /// <summary>
    /// Suggested retry delay in seconds
    /// </summary>
    public float RetryDelaySeconds { get; set; } = 0f;
    
    /// <summary>
    /// Creates a new failed transaction event
    /// </summary>
    public FailedTransactionEvent() { }
    
    /// <summary>
    /// Creates a new failed transaction event with specified values
    /// </summary>
    /// <param name="transactionId">Transaction ID</param>
    /// <param name="actorId">Actor ID</param>
    /// <param name="targetId">Target ID</param>
    /// <param name="itemId">Item ID</param>
    /// <param name="quantity">Quantity</param>
    /// <param name="reason">Failure reason</param>
    /// <param name="errorCode">Error code</param>
    /// <param name="transactionType">Transaction type</param>
    public FailedTransactionEvent(
        string transactionId,
        string actorId,
        string targetId,
        string itemId,
        int quantity,
        string reason,
        string errorCode = "",
        string transactionType = "trade")
        : base(transactionId, actorId, targetId, itemId, quantity, transactionType)
    {
        Reason = reason;
        ErrorCode = errorCode;
    }
    
    public override string ToString()
    {
        return $"FAILED {TransactionType}: {ActorId} -> {TargetId} ({ItemId} x{Quantity}) - {Reason} [{TransactionId}]";
    }
}

/// <summary>
/// Event emitted when a transaction is initiated (before completion)
/// </summary>
public class InitiatedTransactionEvent : TransactionEvent
{
    /// <summary>
    /// Estimated duration in seconds
    /// </summary>
    public float EstimatedDurationSeconds { get; set; } = 0f;
    
    /// <summary>
    /// Whether this transaction can be cancelled
    /// </summary>
    public bool CanCancel { get; set; } = true;
    
    /// <summary>
    /// Priority of this transaction
    /// </summary>
    public int Priority { get; set; } = 0;
    
    /// <summary>
    /// Creates a new initiated transaction event
    /// </summary>
    public InitiatedTransactionEvent() { }
    
    /// <summary>
    /// Creates a new initiated transaction event with specified values
    /// </summary>
    /// <param name="transactionId">Transaction ID</param>
    /// <param name="actorId">Actor ID</param>
    /// <param name="targetId">Target ID</param>
    /// <param name="itemId">Item ID</param>
    /// <param name="quantity">Quantity</param>
    /// <param name="estimatedDurationSeconds">Estimated duration</param>
    /// <param name="transactionType">Transaction type</param>
    public InitiatedTransactionEvent(
        string transactionId,
        string actorId,
        string targetId,
        string itemId,
        int quantity,
        float estimatedDurationSeconds = 0f,
        string transactionType = "trade")
        : base(transactionId, actorId, targetId, itemId, quantity, transactionType)
    {
        EstimatedDurationSeconds = estimatedDurationSeconds;
    }
    
    public override string ToString()
    {
        return $"INITIATED {TransactionType}: {ActorId} -> {TargetId} ({ItemId} x{Quantity}) [{TransactionId}]";
    }
}
