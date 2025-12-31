using System.Collections.Generic;

namespace GameComposition.Core.Types;

/// <summary>
/// Core interface representing an individual item in the game composition framework.
/// Provides the universal contract for all items across plugins and systems.
/// </summary>
public interface IItem
{
    /// <summary>
    /// Unique type identifier for categorization and filtering.
    /// Used for type-safe item classification across the plugin ecosystem.
    /// </summary>
    ItemTypeId TypeId { get; }
    
    /// <summary>
    /// Unique identifier for this specific item instance.
    /// Should be unique within the item's type scope.
    /// </summary>
    string Id { get; }
    
    /// <summary>
    /// Human-readable name of the item.
    /// Display name shown to users in UI and tooltips.
    /// </summary>
    string Name { get; }
    
    /// <summary>
    /// Detailed description of the item.
    /// Used for tooltips, help text, and item information panels.
    /// </summary>
    string Description { get; }
    
    /// <summary>
    /// Maximum quantity that can be stacked together.
    /// Value of 1 means item cannot be stacked.
    /// </summary>
    int MaxStackSize { get; }
    
    /// <summary>
    /// Weight of a single unit of this item.
    /// Used for inventory capacity calculations and encumbrance.
    /// </summary>
    float Weight { get; }
    
    /// <summary>
    /// Base value of a single unit of this item.
    /// Used for trading, crafting costs, and economic calculations.
    /// </summary>
    float Value { get; }
    
    /// <summary>
    /// Plugin-specific properties and metadata.
    /// Allows plugins to extend item data without breaking the core interface.
    /// Key should be descriptive, value can be any serializable object.
    /// </summary>
    Dictionary<string, object> Properties { get; }
}
