using System.Collections.Generic;

namespace BarkMoon.GameComposition.Core.Types;

/// <summary>
/// Basic inventory interface for cross-plugin compatibility.
/// Provides fundamental inventory operations without requiring premium plugins.
/// 
/// This interface enables free cross-plugin compatibility between ArtisanCraft,
/// TradingSystem, ItemDrops, and other plugins without creating artificial paywalls.
/// 
/// Premium plugins can implement this interface and offer additional features
/// through extended interfaces.
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
    /// Checks if the inventory contains a specific item
    /// </summary>
    /// <param name="itemId">The item ID to check</param>
    /// <param name="quantity">Minimum quantity required</param>
    /// <returns>True if the item exists with sufficient quantity</returns>
    bool HasItem(string itemId, int quantity = 1);
    
    /// <summary>
    /// Gets the quantity of a specific item in the inventory
    /// </summary>
    /// <param name="itemId">The item ID to check</param>
    /// <returns>The quantity of the item, or 0 if not found</returns>
    int GetItemQuantity(string itemId);
    
    /// <summary>
    /// Attempts to add items to the inventory
    /// </summary>
    /// <param name="itemId">The item ID to add</param>
    /// <param name="quantity">Quantity to add</param>
    /// <returns>True if items were added successfully</returns>
    bool TryAddItem(string itemId, int quantity);
    
    /// <summary>
    /// Attempts to remove items from the inventory
    /// </summary>
    /// <param name="itemId">The item ID to remove</param>
    /// <param name="quantity">Quantity to remove</param>
    /// <returns>True if items were removed successfully</returns>
    bool TryRemoveItem(string itemId, int quantity);
    
    /// <summary>
    /// Gets all items in the inventory
    /// </summary>
    /// <returns>Collection of all inventory items</returns>
    IEnumerable<InventoryItem> GetAllItems();
    
    /// <summary>
    /// Checks if the inventory can accept the specified items
    /// </summary>
    /// <param name="items">Items to check</param>
    /// <returns>True if all items can be added</returns>
    bool CanAcceptItems(IEnumerable<InventoryItem> items);
    
    /// <summary>
    /// Attempts to add multiple items to the inventory
    /// </summary>
    /// <param name="items">Items to add</param>
    /// <returns>True if all items were added successfully</returns>
    bool TryAddItems(IEnumerable<InventoryItem> items);
    
    /// <summary>
    /// Attempts to remove multiple items from the inventory
    /// </summary>
    /// <param name="items">Items to remove</param>
    /// <returns>True if all items were removed successfully</returns>
    bool TryRemoveItems(IEnumerable<InventoryItem> items);
}
