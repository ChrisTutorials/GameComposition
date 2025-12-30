using System.Collections.Generic;

namespace BarkMoon.GameComposition.Core.Types;

/// <summary>
/// Standard inventory item representation for cross-plugin compatibility.
/// 
/// This class provides a common item format that plugins can use to exchange
/// inventory data without requiring specific premium plugins.
/// 
/// Plugins can extend this with additional properties through the Properties
/// dictionary for domain-specific needs.
/// </summary>
public class InventoryItem
{
    /// <summary>
    /// Unique identifier for this item type
    /// </summary>
    public string Id { get; set; } = string.Empty;
    
    /// <summary>
    /// Type or category of the item (e.g., "weapon", "resource", "consumable")
    /// </summary>
    public string Type { get; set; } = string.Empty;
    
    /// <summary>
    /// Display name for this item
    /// </summary>
    public string Name { get; set; } = string.Empty;
    
    /// <summary>
    /// Description of this item
    /// </summary>
    public string Description { get; set; } = string.Empty;
    
    /// <summary>
    /// Quantity of this item
    /// </summary>
    public int Quantity { get; set; } = 1;
    
    /// <summary>
    /// Weight of a single item
    /// </summary>
    public float Weight { get; set; }
    
    /// <summary>
    /// Value of a single item
    /// </summary>
    public float Value { get; set; }
    
    /// <summary>
    /// Maximum stack size for this item (0 = unlimited)
    /// </summary>
    public int MaxStackSize { get; set; } = 1;
    
    /// <summary>
    /// Whether this item can be traded
    /// </summary>
    public bool IsTradeable { get; set; } = true;
    
    /// <summary>
    /// Whether this item can be dropped
    /// </summary>
    public bool IsDroppable { get; set; } = true;
    
    /// <summary>
    /// Whether this item can be destroyed
    /// </summary>
    public bool IsDestroyable { get; set; } = true;
    
    /// <summary>
    /// Total weight (Quantity * Weight)
    /// </summary>
    public float TotalWeight => Quantity * Weight;
    
    /// <summary>
    /// Total value (Quantity * Value)
    /// </summary>
    public float TotalValue => Quantity * Value;
    
    /// <summary>
    /// Additional item properties for plugin-specific data
    /// </summary>
    public Dictionary<string, object> Properties { get; set; } = new();
    
    /// <summary>
    /// Creates a new inventory item with default values
    /// </summary>
    public InventoryItem() { }
    
    /// <summary>
    /// Creates a new inventory item with specified values
    /// </summary>
    /// <param name="id">Item ID</param>
    /// <param name="type">Item type</param>
    /// <param name="name">Item name</param>
    /// <param name="quantity">Item quantity</param>
    /// <param name="weight">Item weight</param>
    /// <param name="value">Item value</param>
    public InventoryItem(string id, string type, string name, int quantity = 1, float weight = 0f, float value = 0f)
    {
        Id = id;
        Type = type;
        Name = name;
        Quantity = quantity;
        Weight = weight;
        Value = value;
    }
    
    /// <summary>
    /// Gets a property value from the properties dictionary
    /// </summary>
    /// <typeparam name="T">Type of the property</typeparam>
    /// <param name="key">Property key</param>
    /// <param name="defaultValue">Default value if not found</param>
    /// <returns>Property value or default</returns>
    public T GetProperty<T>(string key, T defaultValue = default!)
    {
        if (Properties.TryGetValue(key, out var value) && value is T typedValue)
        {
            return typedValue;
        }
        return defaultValue;
    }
    
    /// <summary>
    /// Sets a property value in the properties dictionary
    /// </summary>
    /// <param name="key">Property key</param>
    /// <param name="value">Property value</param>
    public void SetProperty(string key, object value)
    {
        Properties[key] = value;
    }
    
    /// <summary>
    /// Checks if a property exists in the properties dictionary
    /// </summary>
    /// <param name="key">Property key</param>
    /// <returns>True if property exists</returns>
    public bool HasProperty(string key)
    {
        return Properties.ContainsKey(key);
    }
    
    /// <summary>
    /// Creates a copy of this inventory item
    /// </summary>
    /// <returns>A new InventoryItem with copied values</returns>
    public InventoryItem Clone()
    {
        return new InventoryItem
        {
            Id = Id,
            Type = Type,
            Name = Name,
            Description = Description,
            Quantity = Quantity,
            Weight = Weight,
            Value = Value,
            MaxStackSize = MaxStackSize,
            IsTradeable = IsTradeable,
            IsDroppable = IsDroppable,
            IsDestroyable = IsDestroyable,
            Properties = new Dictionary<string, object>(Properties)
        };
    }
    
    /// <summary>
    /// Splits this item into two items with specified quantities
    /// </summary>
    /// <param name="quantity1">Quantity for the first item</param>
    /// <param name="quantity2">Quantity for the second item</param>
    /// <returns>Tuple of two items</returns>
    public (InventoryItem item1, InventoryItem item2) Split(int quantity1, int quantity2)
    {
        if (quantity1 + quantity2 != Quantity)
            throw new ArgumentException("Quantities must sum to original quantity");
        
        var item1 = Clone();
        item1.Quantity = quantity1;
        
        var item2 = Clone();
        item2.Quantity = quantity2;
        
        return (item1, item2);
    }
    
    /// <summary>
    /// Merges this item with another item of the same type
    /// </summary>
    /// <param name="other">Item to merge with</param>
    /// <returns>Merged item if successful, null if items cannot be merged</returns>
    public InventoryItem? Merge(InventoryItem other)
    {
        if (Id != other.Id || Type != other.Type)
            return null;
        
        if (MaxStackSize > 0 && Quantity + other.Quantity > MaxStackSize)
            return null;
        
        var merged = Clone();
        merged.Quantity = Quantity + other.Quantity;
        
        return merged;
    }
    
    public override string ToString()
    {
        return $"{Name} x{Quantity} ({Type})";
    }
}
