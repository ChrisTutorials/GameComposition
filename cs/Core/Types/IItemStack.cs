namespace GameComposition.Core.Types;

/// <summary>
/// Core interface representing a stack of items in the game composition framework.
/// Provides standardized stack operations for cross-plugin compatibility.
/// Designed to work with IItem interface for type-safe item management.
/// </summary>
public interface IItemStack
{
    /// <summary>
    /// The item type contained in this stack.
    /// All items in a stack must be of the same type for compatibility.
    /// </summary>
    IItem Item { get; }
    
    /// <summary>
    /// Current quantity of items in the stack.
    /// Must be between 0 and MaxStackSize.
    /// </summary>
    int Quantity { get; set; }
    
    /// <summary>
    /// Maximum quantity this stack can hold.
    /// Determined by the item's MaxStackSize property.
    /// </summary>
    int MaxStackSize { get; }
    
    /// <summary>
    /// Total weight of all items in the stack.
    /// Calculated as Quantity * Item.Weight.
    /// </summary>
    float TotalWeight { get; }
    
    /// <summary>
    /// Total value of all items in the stack.
    /// Calculated as Quantity * Item.Value.
    /// </summary>
    float TotalValue { get; }
    
    /// <summary>
    /// Whether the stack is empty (Quantity == 0).
    /// </summary>
    bool IsEmpty { get; }
    
    /// <summary>
    /// Whether the stack is full (Quantity == MaxStackSize).
    /// </summary>
    bool IsFull { get; }
    
    /// <summary>
    /// Checks if the specified quantity can be added to this stack.
    /// </summary>
    /// <param name="quantity">Quantity to check</param>
    /// <returns>True if the quantity can be added without exceeding MaxStackSize</returns>
    bool CanAdd(int quantity);
    
    /// <summary>
    /// Attempts to add the specified quantity to this stack.
    /// </summary>
    /// <param name="quantity">Quantity to add</param>
    /// <returns>True if the quantity was added successfully</returns>
    bool TryAdd(int quantity);
    
    /// <summary>
    /// Attempts to remove the specified quantity from this stack.
    /// </summary>
    /// <param name="quantity">Quantity to remove</param>
    /// <param name="removedStack">Output stack containing the removed items if successful</param>
    /// <returns>True if the quantity was removed successfully</returns>
    bool TryRemove(int quantity, out IItemStack? removedStack);
    
    /// <summary>
    /// Creates a new stack with the specified quantity split from this stack.
    /// Does not modify the original stack.
    /// </summary>
    /// <param name="quantity">Quantity to split</param>
    /// <returns>New stack with the specified quantity, or null if invalid</returns>
    IItemStack? Split(int quantity);
    
    /// <summary>
    /// Checks if this stack can be merged with another stack.
    /// Stacks can be merged if they contain compatible item types.
    /// </summary>
    /// <param name="other">Other stack to check</param>
    /// <returns>True if the stacks can be merged</returns>
    bool CanMerge(IItemStack other);
    
    /// <summary>
    /// Attempts to merge another stack into this stack.
    /// The other stack will be emptied if the merge is successful.
    /// </summary>
    /// <param name="other">Stack to merge into this one</param>
    /// <returns>True if the merge was successful</returns>
    bool TryMerge(IItemStack other);
}
