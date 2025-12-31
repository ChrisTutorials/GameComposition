using System.Collections.Concurrent;

namespace GameComposition.Core.Types;

/// <summary>
/// Type-safe identifier for item types in the game composition framework.
/// Provides compile-time type safety while supporting runtime plugin registration.
/// </summary>
public readonly record struct ItemTypeId(int Value, string Name)
{
    /// <summary>
    /// Represents no item type (null/undefined).
    /// </summary>
    public static readonly ItemTypeId None = new(0, "None");
    
    /// <summary>
    /// Registry for tracking registered item types.
    /// Ensures type uniqueness across the plugin ecosystem.
    /// </summary>
    private static readonly ConcurrentDictionary<int, ItemTypeId> s_registry = new();
    
    /// <summary>
    /// Registry for name-based lookups.
    /// Allows plugins to find types by name.
    /// </summary>
    private static readonly ConcurrentDictionary<string, ItemTypeId> s_nameRegistry = new();
    
    /// <summary>
    /// Next available type ID for automatic registration.
    /// Starts at 1000 to leave room for core types.
    /// </summary>
    private static int s_nextId = 1000;
    
    /// <summary>
    /// Static constructor to register core item types.
    /// </summary>
    static ItemTypeId()
    {
        // Register core item types
        RegisterCoreType(1, "Weapon");
        RegisterCoreType(2, "Armor");
        RegisterCoreType(3, "Consumable");
        RegisterCoreType(4, "Material");
        RegisterCoreType(5, "Accessory");
        RegisterCoreType(6, "Tool");
        RegisterCoreType(7, "Quest");
        RegisterCoreType(8, "Currency");
        
        // Register specific weapon types
        RegisterCoreType(10, "Sword");
        RegisterCoreType(11, "Axe");
        RegisterCoreType(12, "Bow");
        RegisterCoreType(13, "Staff");
        RegisterCoreType(14, "Wand");
        
        // Register specific consumable types
        RegisterCoreType(20, "Potion");
        RegisterCoreType(21, "Food");
        RegisterCoreType(22, "Elixir");
        RegisterCoreType(23, "Scroll");
        
        // Register quality tiers
        RegisterCoreType(100, "Common");
        RegisterCoreType(101, "Uncommon");
        RegisterCoreType(102, "Rare");
        RegisterCoreType(103, "Epic");
        RegisterCoreType(104, "Legendary");
    }
    
    /// <summary>
    /// Registers a new item type for plugins.
    /// Thread-safe registration with conflict detection.
    /// </summary>
    /// <param name="value">Numeric ID for the type</param>
    /// <param name="name">Human-readable name for the type</param>
    /// <returns>Registered ItemTypeId</returns>
    /// <exception cref="ArgumentException">Thrown when type ID or name already exists</exception>
    public static ItemTypeId Register(int value, string name)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Item type name cannot be null or empty", nameof(name));
        
        var newType = new ItemTypeId(value, name);
        
        // Check for conflicts
        if (s_registry.ContainsKey(value))
            throw new ArgumentException($"Item type ID {value} is already registered", nameof(value));
        
        if (s_nameRegistry.ContainsKey(name))
            throw new ArgumentException($"Item type name '{name}' is already registered", nameof(name));
        
        // Register the type
        s_registry[value] = newType;
        s_nameRegistry[name] = newType;
        
        return newType;
    }
    
    /// <summary>
    /// Registers a new item type with automatic ID assignment.
    /// Recommended for plugins that don't need specific IDs.
    /// </summary>
    /// <param name="name">Human-readable name for the type</param>
    /// <returns>Registered ItemTypeId with auto-assigned ID</returns>
    public static ItemTypeId Register(string name)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Item type name cannot be null or empty", nameof(name));
        
        var value = System.Threading.Interlocked.Increment(ref s_nextId);
        return Register(value, name);
    }
    
    /// <summary>
    /// Gets an item type by its numeric ID.
    /// </summary>
    /// <param name="value">Numeric ID to look up</param>
    /// <returns>ItemTypeId if found, None if not found</returns>
    public static ItemTypeId FromId(int value)
    {
        return s_registry.TryGetValue(value, out var type) ? type : None;
    }
    
    /// <summary>
    /// Gets an item type by its name.
    /// </summary>
    /// <param name="name">Name to look up</param>
    /// <returns>ItemTypeId if found, None if not found</returns>
    public static ItemTypeId FromName(string name)
    {
        return s_nameRegistry.TryGetValue(name, out var type) ? type : None;
    }
    
    /// <summary>
    /// Gets all registered item types.
    /// </summary>
    /// <returns>ReadOnly collection of all registered ItemTypeId values</returns>
    public static System.Collections.Generic.IReadOnlyCollection<ItemTypeId> GetAllTypes()
    {
        return s_registry.Values.ToList().AsReadOnly();
    }
    
    /// <summary>
    /// Checks if this item type is compatible with another for stacking.
    /// Items are compatible if they have the same TypeId.
    /// </summary>
    /// <param name="other">Other ItemTypeId to compare</param>
    /// <returns>True if types are compatible for stacking</returns>
    public bool IsCompatible(ItemTypeId other)
    {
        return Value == other.Value;
    }
    
    /// <summary>
    /// Registers a core item type (internal use).
    /// Core types use predefined IDs and cannot be overridden.
    /// </summary>
    /// <param name="value">Predefined numeric ID</param>
    /// <param name="name">Type name</param>
    private static void RegisterCoreType(int value, string name)
    {
        var type = new ItemTypeId(value, name);
        s_registry[value] = type;
        s_nameRegistry[name] = type;
        
        // Update next ID if needed
        if (value >= s_nextId)
            s_nextId = value + 1;
    }
}
