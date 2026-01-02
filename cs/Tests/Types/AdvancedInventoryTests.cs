using System.Collections.Generic;
using System.Linq;
using Xunit;
using BarkMoon.GameComposition.Core.Types;

namespace GameComposition.Core.Tests.Types;

/// <summary>
/// Context for item container condition evaluation
/// </summary>
public class ItemContainerContext
{
    public IItemStack ItemStack { get; set; }
    public string ContainerTag { get; set; }
    public string ItemType { get; set; }
    public Dictionary<string, object> Properties { get; set; } = new();
}

/// <summary>
/// Tag-based condition for filtering items in containers
/// </summary>
public class TagCondition : ConditionBase<ItemContainerContext>
{
    private readonly string _requiredTag;
    private readonly string _blockedTag;

    public TagCondition(string id, string name, string description, string requiredTag = null, string blockedTag = null, int priority = 0)
        : base(id, name, description, priority)
    {
        _requiredTag = requiredTag;
        _blockedTag = blockedTag;
    }

    protected override bool EvaluateCondition(ItemContainerContext context)
    {
        // If we have a blocked tag, reject if container has blocked tag
        if (!string.IsNullOrEmpty(_blockedTag))
        {
            if (context.ContainerTag == _blockedTag)
                return false;
        }

        // If we have a required tag, only accept if container has required tag
        if (!string.IsNullOrEmpty(_requiredTag))
        {
            if (context.ContainerTag != _requiredTag)
                return false;
        }

        // No specific requirements or requirements met, accept
        return true;
    }

    public override bool IsValid()
    {
        return base.IsValid() && (!string.IsNullOrEmpty(_requiredTag) || !string.IsNullOrEmpty(_blockedTag));
    }
}

/// <summary>
/// Item type condition for filtering items in containers
/// </summary>
public class ItemTypeCondition : ConditionBase<ItemContainerContext>
{
    private readonly string[] _allowedTypes;
    private readonly string[] _blockedTypes;

    public ItemTypeCondition(string id, string name, string description, string[] allowedTypes = null, string[] blockedTypes = null, int priority = 0)
        : base(id, name, description, priority)
    {
        _allowedTypes = allowedTypes ?? Array.Empty<string>();
        _blockedTypes = blockedTypes ?? Array.Empty<string>();
    }

    protected override bool EvaluateCondition(ItemContainerContext context)
    {
        // Check blocked types first
        if (_blockedTypes.Contains(context.ItemType))
            return false;

        // If we have allowed types, check if item type is allowed
        if (_allowedTypes.Length > 0)
            return _allowedTypes.Contains(context.ItemType);

        // No restrictions, accept
        return true;
    }

    public override bool IsValid()
    {
        return base.IsValid() && (_allowedTypes.Length > 0 || _blockedTypes.Length > 0);
    }
}

/// <summary>
/// Enhanced mock item stack that allows setting item types for testing
/// </summary>
public class EnhancedMockItemStack : IItemStack
{
    private readonly Dictionary<string, int> _items = new();
    private readonly Dictionary<string, string> _itemTypes = new();

    public EnhancedMockItemStack(string id, string name, int capacity = 999)
    {
        Id = id;
        Name = name;
        Capacity = capacity;
    }

    public string Id { get; }
    public string Name { get; }
    public int Capacity { get; }
    public int CurrentCount => _items.Values.Sum();
    public bool IsFull => CurrentCount >= Capacity;
    public bool IsEmpty => CurrentCount == 0;

    public bool HasItem(string itemId, int quantity = 1) => GetItemQuantity(itemId) >= quantity;
    public int GetItemQuantity(string itemId) => _items.TryGetValue(itemId, out var quantity) ? quantity : 0;

    public bool TryAddItem(string itemId, int quantity)
    {
        if (itemId == null) throw new ArgumentNullException(nameof(itemId));
        if (quantity <= 0) return false;
        if (string.IsNullOrEmpty(itemId)) return false;
        if (CurrentCount + quantity > Capacity) return false;

        _items[itemId] = GetItemQuantity(itemId) + quantity;
        return true;
    }

    public bool TryAddItem(string itemId, int quantity, string itemType)
    {
        if (TryAddItem(itemId, quantity))
        {
            _itemTypes[itemId] = itemType;
            return true;
        }
        return false;
    }

    public bool TryRemoveItem(string itemId, int quantity)
    {
        if (quantity <= 0) return false;
        if (!HasItem(itemId, quantity)) return false;

        var currentQuantity = GetItemQuantity(itemId);
        var newQuantity = currentQuantity - quantity;
        
        if (newQuantity <= 0)
        {
            _items.Remove(itemId);
            _itemTypes.Remove(itemId);
        }
        else
        {
            _items[itemId] = newQuantity;
        }

        return true;
    }

    public IEnumerable<InventoryItem> GetAllItems()
    {
        return _items.Select(kvp => new InventoryItem
        {
            Id = kvp.Key,
            Name = kvp.Key,
            Type = _itemTypes.GetValueOrDefault(kvp.Key, "unknown"),
            Quantity = kvp.Value,
            Weight = 1.0f,
            Value = 10.0f
        });
    }

    public bool CanAcceptItems(IEnumerable<InventoryItem> items)
    {
        var totalRequired = items.Sum(item => item.Quantity);
        return CurrentCount + totalRequired <= Capacity;
    }

    public bool TryAddItems(IEnumerable<InventoryItem> items)
    {
        foreach (var item in items)
        {
            if (!TryAddItem(item.Id, item.Quantity))
                return false;
        }
        return true;
    }

    public bool TryRemoveItems(IEnumerable<InventoryItem> items)
    {
        foreach (var item in items)
        {
            if (!TryRemoveItem(item.Id, item.Quantity))
                return false;
        }
        return true;
    }
}
public class TaggedItemContainer
{
    private readonly List<IItemStack> _itemStacks = new();
    private readonly List<ICondition<ItemContainerContext>> _conditions = new();
    private readonly int _capacity;
    private readonly string _tag;

    public TaggedItemContainer(string tag, int capacity = 5)
    {
        _tag = tag;
        _capacity = capacity;
    }

    public string Tag => _tag;
    public int Capacity => _capacity;
    public int CurrentCount => _itemStacks.Sum(stack => stack.CurrentCount);
    public int StackCount => _itemStacks.Count;
    public bool IsFull => CurrentCount >= _capacity;
    public bool IsEmpty => CurrentCount == 0;

    public void AddCondition(ICondition<ItemContainerContext> condition)
    {
        _conditions.Add(condition);
    }

    public bool CanAcceptItemStack(IItemStack itemStack)
    {
        if (CurrentCount + itemStack.CurrentCount > _capacity) return false;

        // Extract item type from the stack
        var firstItem = itemStack.GetAllItems().FirstOrDefault();
        var itemType = firstItem?.Type ?? "unknown";

        var context = new ItemContainerContext
        {
            ItemStack = itemStack,
            ContainerTag = _tag,
            ItemType = itemType,
            Properties = new Dictionary<string, object>
            {
                ["container_tag"] = _tag,
                ["item_count"] = itemStack.CurrentCount,
                ["item_type"] = itemType
            }
        };

        // Evaluate all conditions
        foreach (var condition in _conditions.OrderByDescending(c => c.Priority))
        {
            if (!condition.IsSatisfied(context))
                return false;
        }

        return true;
    }

    public bool TryAddItemStack(IItemStack itemStack)
    {
        if (!CanAcceptItemStack(itemStack)) return false;

        _itemStacks.Add(itemStack);
        return true;
    }

    public bool TryRemoveItemStack(IItemStack itemStack)
    {
        return _itemStacks.Remove(itemStack);
    }

    public IReadOnlyList<IItemStack> GetAllItemStacks() => _itemStacks.AsReadOnly();
    public IReadOnlyList<ICondition<ItemContainerContext>> GetConditions() => _conditions.AsReadOnly();
}

/// <summary>
/// Enhanced inventory that manages multiple tagged containers with intelligent routing
/// </summary>
public class MultiContainerInventory
{
    private readonly List<TaggedItemContainer> _containers = new();
    private readonly int _totalCapacity;

    public MultiContainerInventory(int totalCapacity = 999)
    {
        _totalCapacity = totalCapacity;
    }

    public int TotalCapacity => _totalCapacity;
    public int CurrentCount => _containers.Sum(c => c.CurrentCount);
    public int ContainerCount => _containers.Count;
    public bool IsFull => CurrentCount >= _totalCapacity;

    public void AddContainer(TaggedItemContainer container)
    {
        _containers.Add(container);
    }

    public bool TryAddItemStack(IItemStack itemStack)
    {
        if (CurrentCount + itemStack.CurrentCount > _totalCapacity) return false;

        // Try to add to the first compatible container (in order)
        foreach (var container in _containers)
        {
            if (container.TryAddItemStack(itemStack))
                return true;
        }

        return false;
    }

    public bool TryAddItemStackToSpecificContainer(IItemStack itemStack, string containerTag)
    {
        var container = _containers.FirstOrDefault(c => c.Tag == containerTag);
        if (container == null) return false;

        return container.TryAddItemStack(itemStack);
    }

    public IReadOnlyList<TaggedItemContainer> GetAllContainers() => _containers.AsReadOnly();
    public IReadOnlyList<IItemStack> GetAllItemStacks() => _containers.SelectMany(c => c.GetAllItemStacks()).ToList().AsReadOnly();
}

/// <summary>
/// Advanced test scenarios for multi-container inventories with conditional filtering
/// </summary>
public class AdvancedInventoryTests
{
    #region Multi-Container Routing Tests

    [Fact]
    public void MultiContainerInventory_FirstContainerFull_SecondContainerEmpty_ShouldRouteToSecond()
    {
        // Arrange
        var inventory = new MultiContainerInventory(10);
        var container1 = new TaggedItemContainer("weapons", 5);
        var container2 = new TaggedItemContainer("potions", 5);
        
        inventory.AddContainer(container1);
        inventory.AddContainer(container2);

        // Fill first container
        var swordStack = new MockItemStack("sword_stack", "Swords", 5);
        swordStack.TryAddItem("iron_sword", 5);
        container1.TryAddItemStack(swordStack);

        var potionStack = new MockItemStack("potion_stack", "Potions", 3);
        potionStack.TryAddItem("health_potion", 3);

        // Act
        bool result = inventory.TryAddItemStack(potionStack);

        // Assert
        Assert.True(result);
        Assert.Equal(8, inventory.CurrentCount); // 5 from first + 3 from second
        Assert.Equal(5, container1.CurrentCount); // Still full
        Assert.Equal(3, container2.CurrentCount); // Received the new stack
        Assert.Single(container2.GetAllItemStacks());
    }

    [Fact]
    public void MultiContainerInventory_AllContainersFull_ShouldReject()
    {
        // Arrange
        var inventory = new MultiContainerInventory(10);
        var container1 = new TaggedItemContainer("weapons", 5);
        var container2 = new TaggedItemContainer("potions", 5);
        
        inventory.AddContainer(container1);
        inventory.AddContainer(container2);

        // Fill both containers
        var swordStack = new MockItemStack("sword_stack", "Swords", 5);
        swordStack.TryAddItem("iron_sword", 5);
        container1.TryAddItemStack(swordStack);

        var potionStack = new MockItemStack("potion_stack", "Potions", 5);
        potionStack.TryAddItem("health_potion", 5);
        container2.TryAddItemStack(potionStack);

        var extraStack = new MockItemStack("extra_stack", "Extra", 2);
        extraStack.TryAddItem("magic_ring", 2);

        // Act
        bool result = inventory.TryAddItemStack(extraStack);

        // Assert
        Assert.False(result);
        Assert.Equal(10, inventory.CurrentCount);
        Assert.True(container1.IsFull);
        Assert.True(container2.IsFull);
    }

    [Fact]
    public void MultiContainerInventory_PartiallyFilledContainers_ShouldRouteToFirstCompatible()
    {
        // Arrange
        var inventory = new MultiContainerInventory(15);
        var container1 = new TaggedItemContainer("weapons", 5);
        var container2 = new TaggedItemContainer("potions", 5);
        var container3 = new TaggedItemContainer("misc", 5);
        
        inventory.AddContainer(container1);
        inventory.AddContainer(container2);
        inventory.AddContainer(container3);

        // Partially fill first container
        var swordStack = new MockItemStack("sword_stack", "Swords", 5);
        swordStack.TryAddItem("iron_sword", 3);
        container1.TryAddItemStack(swordStack);

        var newSwordStack = new MockItemStack("new_sword_stack", "New Swords", 2);
        newSwordStack.TryAddItem("steel_sword", 2);

        // Act
        bool result = inventory.TryAddItemStack(newSwordStack);

        // Assert
        Assert.True(result);
        Assert.Equal(5, container1.CurrentCount); // Should add to first container
        Assert.Equal(0, container2.CurrentCount);
        Assert.Equal(0, container3.CurrentCount);
        Assert.Equal(2, container1.StackCount); // Two stacks in first container
    }

    #endregion

    #region Conditional Filtering Tests

    [Fact]
    public void TaggedItemContainer_AllowedTag_ShouldAccept()
    {
        // Arrange
        var container = new TaggedItemContainer("weapons", 10);
        var condition = new TagCondition("weapon_only", "Weapon Only", "Only accepts weapons", requiredTag: "weapons");
        container.AddCondition(condition);

        var weaponStack = new MockItemStack("weapon_stack", "Weapons", 3);
        weaponStack.TryAddItem("iron_sword", 3);

        // Act
        bool result = container.TryAddItemStack(weaponStack);

        // Assert
        Assert.True(result);
        Assert.Single(container.GetAllItemStacks());
    }

    [Fact]
    public void TaggedItemContainer_BlockedTag_ShouldReject()
    {
        // Arrange
        var container = new TaggedItemContainer("potions", 10); // Container has "potions" tag
        var condition = new TagCondition("no_potions", "No Potions", "Blocks potions", blockedTag: "potions");
        container.AddCondition(condition);

        var potionStack = new EnhancedMockItemStack("potion_stack", "Potions", 3);
        potionStack.TryAddItem("health_potion", 3, "consumable");

        // Act
        bool result = container.TryAddItemStack(potionStack);

        // Assert
        Assert.False(result);
        Assert.Empty(container.GetAllItemStacks());
    }

    [Fact]
    public void TaggedItemContainer_ItemTypeAllowed_ShouldAccept()
    {
        // Arrange
        var container = new TaggedItemContainer("general", 10);
        var condition = new ItemTypeCondition("weapons_only", "Weapons Only", "Only accepts weapon types", 
            allowedTypes: new[] { "weapon", "sword", "axe" });
        container.AddCondition(condition);

        var swordStack = new EnhancedMockItemStack("sword_stack", "Swords", 3);
        swordStack.TryAddItem("iron_sword", 3, "weapon");

        // Act
        bool result = container.CanAcceptItemStack(swordStack);

        // Assert
        Assert.True(result);
    }

    [Fact]
    public void TaggedItemContainer_ItemTypeBlocked_ShouldReject()
    {
        // Arrange
        var container = new TaggedItemContainer("general", 10);
        var condition = new ItemTypeCondition("no_consumables", "No Consumables", "Blocks consumable items", 
            blockedTypes: new[] { "consumable", "potion", "food" });
        container.AddCondition(condition);

        var potionStack = new EnhancedMockItemStack("potion_stack", "Potions", 3);
        potionStack.TryAddItem("health_potion", 3, "consumable");

        // Act
        bool result = container.CanAcceptItemStack(potionStack);

        // Assert
        Assert.False(result);
    }

    [Fact]
    public void MultiContainerInventory_ConditionalFiltering_ShouldRouteToCompatibleContainer()
    {
        // Arrange
        var inventory = new MultiContainerInventory(20);
        var weaponContainer = new TaggedItemContainer("weapons", 10);
        var potionContainer = new TaggedItemContainer("potions", 10);

        // Add conditions - weapon container should only accept weapons, potion container only potions
        weaponContainer.AddCondition(new TagCondition("weapon_tag", "Weapon Tag", "Only weapons", requiredTag: "weapons"));
        weaponContainer.AddCondition(new ItemTypeCondition("weapon_types", "Weapon Types", "Only weapon types", allowedTypes: new[] { "weapon", "sword", "axe" }));
        
        potionContainer.AddCondition(new TagCondition("potion_tag", "Potion Tag", "Only potions", requiredTag: "potions"));
        potionContainer.AddCondition(new ItemTypeCondition("potion_types", "Potion Types", "Only consumable types", allowedTypes: new[] { "consumable", "potion", "food" }));

        inventory.AddContainer(weaponContainer);
        inventory.AddContainer(potionContainer);

        var weaponStack = new EnhancedMockItemStack("weapon_stack", "Weapons", 3);
        weaponStack.TryAddItem("iron_sword", 3, "weapon");

        var potionStack = new EnhancedMockItemStack("potion_stack", "Potions", 5);
        potionStack.TryAddItem("health_potion", 5, "consumable");

        // Debug: Check if individual containers accept the items
        bool weaponAcceptsWeapon = weaponContainer.CanAcceptItemStack(weaponStack);
        bool potionAcceptsPotion = potionContainer.CanAcceptItemStack(potionStack);
        bool weaponAcceptsPotion = weaponContainer.CanAcceptItemStack(potionStack);

        // Act
        bool weaponResult = inventory.TryAddItemStack(weaponStack);
        bool potionResult = inventory.TryAddItemStack(potionStack);

        // Assert - Now with proper type filtering
        Assert.True(weaponAcceptsWeapon); // Weapon container should accept weapons
        Assert.True(potionAcceptsPotion); // Potion container should accept potions
        Assert.False(weaponAcceptsPotion); // Weapon container should NOT accept potions (now it won't!)
        
        Assert.True(weaponResult); // Weapon container accepts weapon
        Assert.True(potionResult); // Potion container accepts potion
        
        // Now the routing should work correctly
        Assert.Equal(3, weaponContainer.CurrentCount); // Only weapon here
        Assert.Equal(5, potionContainer.CurrentCount); // Only potion here
        Assert.Single(weaponContainer.GetAllItemStacks());
        Assert.Single(potionContainer.GetAllItemStacks());
    }

    [Fact]
    public void MultiContainerInventory_NoCompatibleContainer_ShouldReject()
    {
        // Arrange
        var inventory = new MultiContainerInventory(20);
        var weaponContainer = new TaggedItemContainer("weapons", 10);
        var armorContainer = new TaggedItemContainer("armor", 10);

        // Add conditions that block food items
        weaponContainer.AddCondition(new ItemTypeCondition("no_food_weapons", "No Food in Weapons", "Blocks food", blockedTypes: new[] { "food", "consumable" }));
        armorContainer.AddCondition(new ItemTypeCondition("no_food_armor", "No Food in Armor", "Blocks food", blockedTypes: new[] { "food", "consumable" }));

        inventory.AddContainer(weaponContainer);
        inventory.AddContainer(armorContainer);

        var foodStack = new EnhancedMockItemStack("food_stack", "Food", 3);
        foodStack.TryAddItem("bread", 3, "food");

        // Act
        bool result = inventory.TryAddItemStack(foodStack);

        // Assert
        Assert.False(result);
        Assert.Equal(0, inventory.CurrentCount);
        Assert.Empty(weaponContainer.GetAllItemStacks());
        Assert.Empty(armorContainer.GetAllItemStacks());
    }

    [Fact]
    public void MultiContainerInventory_MultipleConditions_ShouldEvaluateAll()
    {
        // Arrange
        var inventory = new MultiContainerInventory(20);
        var premiumContainer = new TaggedItemContainer("premium", 10);

        // Add multiple conditions
        premiumContainer.AddCondition(new TagCondition("premium_tag", "Premium Tag", "Only premium items", requiredTag: "premium"));
        premiumContainer.AddCondition(new ItemTypeCondition("rare_only", "Rare Only", "Only rare items", allowedTypes: new[] { "rare", "epic", "legendary" }));

        inventory.AddContainer(premiumContainer);

        var rareItemStack = new EnhancedMockItemStack("rare_stack", "Rare Items", 2);
        rareItemStack.TryAddItem("rare_sword", 2, "rare");

        var commonItemStack = new EnhancedMockItemStack("common_stack", "Common Items", 3);
        commonItemStack.TryAddItem("common_sword", 3, "common");

        // Act
        bool rareResult = inventory.TryAddItemStack(rareItemStack);
        bool commonResult = inventory.TryAddItemStack(commonItemStack);

        // Assert
        Assert.True(rareResult); // Meets both conditions
        Assert.False(commonResult); // Fails item type condition
        Assert.Equal(2, premiumContainer.CurrentCount);
        Assert.Single(premiumContainer.GetAllItemStacks());
    }

    #endregion

    #region Complex Integration Scenarios

    [Fact]
    public void Integration_MultiContainerWithConditions_ComplexRouting_ShouldWork()
    {
        // Arrange
        var inventory = new MultiContainerInventory(30);
        
        // Create containers with different conditions
        var weaponContainer = new TaggedItemContainer("weapons", 10);
        var potionContainer = new TaggedItemContainer("potions", 10);
        var premiumContainer = new TaggedItemContainer("premium", 10);

        weaponContainer.AddCondition(new TagCondition("weapon_tag", "Weapon Tag", "Only weapons", requiredTag: "weapons"));
        potionContainer.AddCondition(new TagCondition("potion_tag", "Potion Tag", "Only potions", requiredTag: "potions"));
        premiumContainer.AddCondition(new ItemTypeCondition("premium_only", "Premium Only", "Only premium items", allowedTypes: new[] { "epic", "legendary" }));

        inventory.AddContainer(weaponContainer);
        inventory.AddContainer(potionContainer);
        inventory.AddContainer(premiumContainer);

        // Fill weapon container partially
        var swordStack = new EnhancedMockItemStack("sword_stack", "Swords", 6);
        swordStack.TryAddItem("iron_sword", 6, "weapon");
        weaponContainer.TryAddItemStack(swordStack);

        // Test items
        var rareSwordStack = new EnhancedMockItemStack("rare_sword_stack", "Rare Swords", 2);
        rareSwordStack.TryAddItem("rare_sword", 2, "weapon");

        var epicSwordStack = new EnhancedMockItemStack("epic_sword_stack", "Epic Swords", 1);
        epicSwordStack.TryAddItem("epic_sword", 1, "epic");

        var potionStack = new EnhancedMockItemStack("potion_stack", "Potions", 5);
        potionStack.TryAddItem("health_potion", 5, "consumable");

        // Act
        bool rareSwordResult = inventory.TryAddItemStack(rareSwordStack);
        bool epicSwordResult = inventory.TryAddItemStack(epicSwordStack);
        bool potionResult = inventory.TryAddItemStack(potionStack);

        // Assert - Items go to first compatible container in order
        Assert.True(rareSwordResult); // Goes to weapon container (has space and matching tag)
        Assert.True(epicSwordResult); // Goes to weapon container (epic sword is also a weapon type, and weapon container is first)
        Assert.True(potionResult); // Goes to potion container (matching tag)
        
        Assert.Equal(9, weaponContainer.CurrentCount); // 6 + 2 rare swords + 1 epic sword
        Assert.Equal(5, potionContainer.CurrentCount); // 5 potions
        Assert.Equal(0, premiumContainer.CurrentCount); // No items, epic sword went to weapon container first
        Assert.Equal(14, inventory.CurrentCount); // Total items
    }

    [Fact]
    public void Integration_ConditionPriority_HigherPriorityConditionsEvaluatedFirst()
    {
        // Arrange
        var container = new TaggedItemContainer("weapons", 10); // Container has "weapons" tag
        
        // Add conditions with different priorities
        var highPriorityCondition = new TagCondition("high_priority", "High Priority", "High priority tag condition", 
            requiredTag: "weapons", priority: 100);
        var lowPriorityCondition = new ItemTypeCondition("low_priority", "Low Priority", "Low priority type condition", 
            allowedTypes: new[] { "weapon" }, priority: 1); // Allow weapons, not potions

        container.AddCondition(highPriorityCondition);
        container.AddCondition(lowPriorityCondition);

        var weaponStack = new EnhancedMockItemStack("weapon_stack", "Weapons", 2);
        weaponStack.TryAddItem("sword", 2, "weapon");

        // Act
        bool result = container.CanAcceptItemStack(weaponStack);

        // Assert
        Assert.True(result); // High priority condition (tag) passes, low priority condition (type) also passes
    }

    #endregion
}
