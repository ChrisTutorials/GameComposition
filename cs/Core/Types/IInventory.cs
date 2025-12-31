using System.Collections.Generic;

namespace GameComposition.Core.Types;

/// <summary>
/// Basic inventory interface for managing collections of item stacks.
/// Provides inventory-level operations and capacity management.
/// </summary>
public interface IInventory
{
    /// <summary>
    /// Unique identifier for this inventory
    /// </summary>
    string Id { get; }
    
    /// <summary>
    /// Display name for this inventory
    /// </summary>
    string Name { get; }
    
    /// <summary>
    /// Maximum number of items this inventory can hold
    /// </summary>
    int Capacity { get; }
    
    /// <summary>
    /// Current number of items in the inventory
    /// </summary>
    int CurrentCount { get; }
    
    /// <summary>
    /// Whether the inventory is full
    /// </summary>
    bool IsFull { get; }
    
    /// <summary>
    /// Whether the inventory is empty
    /// </summary>
    bool IsEmpty { get; }
    
    /// <summary>
    /// Adds an item stack to this inventory
    /// </summary>
    /// <param name="itemStack">The item stack to add</param>
    /// <returns>True if the item stack was added successfully</returns>
    bool TryAddItemStack(IItemStack itemStack);
    
    /// <summary>
    /// Removes an item stack from this inventory
    /// </summary>
    /// <param name="itemStack">The item stack to remove</param>
    /// <returns>True if the item stack was removed successfully</returns>
    bool TryRemoveItemStack(IItemStack itemStack);
    
    /// <summary>
    /// Gets all item stacks in this inventory
    /// </summary>
    /// <returns>Collection of all item stacks</returns>
    IReadOnlyList<IItemStack> GetAllItemStacks();
    
    /// <summary>
    /// Checks if the inventory can accept the specified item stack
    /// </summary>
    /// <param name="itemStack">Item stack to check</param>
    /// <returns>True if the item stack can be added</returns>
    bool CanAcceptItemStack(IItemStack itemStack);
}