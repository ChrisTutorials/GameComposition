using System.Collections.Generic;
using System.Linq;
using Xunit;
using BarkMoon.GameComposition.Core.Types;

namespace GameComposition.Core.Tests.Types;

/// <summary>
/// Parameterized test data for item stack operations
/// </summary>
public class ItemStackTestData
{
    public static IEnumerable<object[]> ValidItemQuantities()
    {
        yield return new object[] { "health_potion", 1 };
        yield return new object[] { "health_potion", 5 };
        yield return new object[] { "health_potion", 10 };
        yield return new object[] { "mana_potion", 3 };
        yield return new object[] { "iron_sword", 1 };
        yield return new object[] { "gold_coin", 100 };
        yield return new object[] { "arrow", 50 };
    }

    public static IEnumerable<object[]> InvalidItemQuantities()
    {
        yield return new object[] { "health_potion", 0 };
        yield return new object[] { "health_potion", -1 };
        yield return new object[] { "health_potion", -10 };
    }

    public static IEnumerable<object[]> CapacityTestData()
    {
        yield return new object[] { 10, 5, true };   // capacity 10, adding 5, should succeed
        yield return new object[] { 10, 10, true };  // capacity 10, adding 10, should succeed
        yield return new object[] { 10, 11, false }; // capacity 10, adding 11, should fail
        yield return new object[] { 5, 6, false };   // capacity 5, adding 6, should fail
        yield return new object[] { 100, 99, true }; // capacity 100, adding 99, should succeed
    }

    public static IEnumerable<object[]> StackCapacityTestData()
    {
        yield return new object[] { 20, 5, 15, true };   // stack cap 20, current 5, adding 15, should succeed
        yield return new object[] { 20, 10, 10, true };  // stack cap 20, current 10, adding 10, should succeed
        yield return new object[] { 20, 15, 6, false };  // stack cap 20, current 15, adding 6, should fail
        yield return new object[] { 1, 0, 1, true };     // stack cap 1, current 0, adding 1, should succeed
        yield return new object[] { 1, 1, 1, false };    // stack cap 1, current 1, adding 1, should fail
    }
}

/// <summary>
/// Mock implementation of IItemStack for testing
/// </summary>
public class MockItemStack : IItemStack
{
    private readonly Dictionary<string, int> _items = new();

    public MockItemStack(string id, string name, int capacity = 999)
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

    public bool TryRemoveItem(string itemId, int quantity)
    {
        if (quantity <= 0) return false;
        if (!HasItem(itemId, quantity)) return false;

        var currentQuantity = GetItemQuantity(itemId);
        var newQuantity = currentQuantity - quantity;
        
        if (newQuantity <= 0)
            _items.Remove(itemId);
        else
            _items[itemId] = newQuantity;

        return true;
    }

    public IEnumerable<InventoryItem> GetAllItems()
    {
        return _items.Select(kvp => new InventoryItem
        {
            Id = kvp.Key,
            Name = kvp.Key,
            Type = "test",
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

/// <summary>
/// Mock implementation of IInventory for testing
/// </summary>
public class MockInventory : IInventory
{
    private readonly List<IItemStack> _itemStacks = new();

    public MockInventory(string id, string name, int capacity = 999)
    {
        Id = id;
        Name = name;
        Capacity = capacity;
    }

    public string Id { get; }
    public string Name { get; }
    public int Capacity { get; }
    public int CurrentCount => _itemStacks.Sum(stack => stack.CurrentCount);
    public bool IsFull => CurrentCount >= Capacity;
    public bool IsEmpty => CurrentCount == 0;

    public bool TryAddItemStack(IItemStack itemStack)
    {
        if (itemStack == null) throw new ArgumentNullException(nameof(itemStack));
        if (!CanAcceptItemStack(itemStack)) return false;
        
        // Check if we can merge with existing stack
        var existingStack = _itemStacks.FirstOrDefault(s => s.Id == itemStack.Id);
        if (existingStack != null)
        {
            // Try to merge quantities
            var totalQuantity = existingStack.CurrentCount + itemStack.CurrentCount;
            if (totalQuantity <= existingStack.Capacity)
            {
                return existingStack.TryAddItem(itemStack.GetAllItems().First().Id, itemStack.CurrentCount);
            }
        }

        // Add as new stack
        _itemStacks.Add(itemStack);
        return true;
    }

    public bool TryRemoveItemStack(IItemStack itemStack)
    {
        return _itemStacks.Remove(itemStack);
    }

    public IReadOnlyList<IItemStack> GetAllItemStacks() => _itemStacks.AsReadOnly();

    public bool CanAcceptItemStack(IItemStack itemStack)
    {
        if (itemStack == null) throw new ArgumentNullException(nameof(itemStack));
        
        // Check inventory capacity
        if (CurrentCount + itemStack.CurrentCount > Capacity) return false;

        // Check if we can merge with existing stack
        var existingStack = _itemStacks.FirstOrDefault(s => s.Id == itemStack.Id);
        if (existingStack != null)
        {
            var totalQuantity = existingStack.CurrentCount + itemStack.CurrentCount;
            return totalQuantity <= existingStack.Capacity;
        }

        return true;
    }
}

/// <summary>
/// Mock implementation of ItemContainer for testing
/// </summary>
public class MockItemContainer
{
    private readonly List<IItemStack> _itemStacks = new();
    private readonly int _capacity;

    public MockItemContainer(int capacity = 999)
    {
        _capacity = capacity;
    }

    public int CurrentCount => _itemStacks.Sum(stack => stack.CurrentCount);
    public bool IsFull => CurrentCount >= _capacity;
    public bool IsEmpty => CurrentCount == 0;

    public bool TryAddItemStack(IItemStack itemStack)
    {
        if (CurrentCount + itemStack.CurrentCount > _capacity) return false;
        
        _itemStacks.Add(itemStack);
        return true;
    }

    public bool TryRemoveItemStack(IItemStack itemStack)
    {
        return _itemStacks.Remove(itemStack);
    }

    public IReadOnlyList<IItemStack> GetAllItemStacks() => _itemStacks.AsReadOnly();
}

/// <summary>
/// Comprehensive test suite for item stack and inventory operations
/// </summary>
public class ItemStackInventoryTests
{
    #region ItemStack Direct Operations

    [Theory]
    [MemberData(nameof(ItemStackTestData.ValidItemQuantities), MemberType = typeof(ItemStackTestData))]
    public void ItemStack_TryAddItem_ValidQuantity_ShouldSucceed(string itemId, int quantity)
    {
        // Arrange
        var itemStack = new MockItemStack("test_stack", "Test Stack", 100);

        // Act
        bool result = itemStack.TryAddItem(itemId, quantity);

        // Assert
        Assert.True(result);
        Assert.Equal(quantity, itemStack.CurrentCount);
        Assert.True(itemStack.HasItem(itemId, quantity));
        Assert.Equal(quantity, itemStack.GetItemQuantity(itemId));
    }

    [Theory]
    [MemberData(nameof(ItemStackTestData.InvalidItemQuantities), MemberType = typeof(ItemStackTestData))]
    public void ItemStack_TryAddItem_InvalidQuantity_ShouldFail(string itemId, int quantity)
    {
        // Arrange
        var itemStack = new MockItemStack("test_stack", "Test Stack", 100);
        var initialCount = itemStack.CurrentCount;

        // Act
        bool result = itemStack.TryAddItem(itemId, quantity);

        // Assert
        Assert.False(result);
        Assert.Equal(initialCount, itemStack.CurrentCount);
        Assert.False(itemStack.HasItem(itemId));
    }

    [Theory]
    [MemberData(nameof(ItemStackTestData.CapacityTestData), MemberType = typeof(ItemStackTestData))]
    public void ItemStack_TryAddItem_RespectCapacity_ShouldHonorLimit(int capacity, int quantity, bool shouldSucceed)
    {
        // Arrange
        var itemStack = new MockItemStack("test_stack", "Test Stack", capacity);

        // Act
        bool result = itemStack.TryAddItem("test_item", quantity);

        // Assert
        if (shouldSucceed)
        {
            Assert.True(result);
            Assert.Equal(quantity, itemStack.CurrentCount);
        }
        else
        {
            Assert.False(result);
            Assert.Equal(0, itemStack.CurrentCount);
        }
    }

    [Theory]
    [MemberData(nameof(ItemStackTestData.StackCapacityTestData), MemberType = typeof(ItemStackTestData))]
    public void ItemStack_TryAddItem_ExistingStack_ShouldRespectStackCapacity(int stackCapacity, int currentQuantity, int addQuantity, bool shouldSucceed)
    {
        // Arrange
        var itemStack = new MockItemStack("test_stack", "Test Stack", stackCapacity);
        itemStack.TryAddItem("test_item", currentQuantity);

        // Act
        bool result = itemStack.TryAddItem("test_item", addQuantity);

        // Assert
        if (shouldSucceed)
        {
            Assert.True(result);
            Assert.Equal(currentQuantity + addQuantity, itemStack.CurrentCount);
        }
        else
        {
            Assert.False(result);
            Assert.Equal(currentQuantity, itemStack.CurrentCount);
        }
    }

    #endregion

    #region ItemContainer Operations

    [Fact]
    public void ItemContainer_AddItemStack_ShouldSucceed()
    {
        // Arrange
        var container = new MockItemContainer(100);
        var itemStack = new MockItemStack("potion_stack", "Health Potions", 50);
        itemStack.TryAddItem("health_potion", 10);

        // Act
        bool result = container.TryAddItemStack(itemStack);

        // Assert
        Assert.True(result);
        Assert.Equal(10, container.CurrentCount);
        Assert.Single(container.GetAllItemStacks());
    }

    [Fact]
    public void ItemContainer_AddItemStack_ExceedCapacity_ShouldFail()
    {
        // Arrange
        var container = new MockItemContainer(5);
        var itemStack = new MockItemStack("potion_stack", "Health Potions", 50);
        itemStack.TryAddItem("health_potion", 10);

        // Act
        bool result = container.TryAddItemStack(itemStack);

        // Assert
        Assert.False(result);
        Assert.Equal(0, container.CurrentCount);
        Assert.Empty(container.GetAllItemStacks());
    }

    [Fact]
    public void ItemContainer_AddMultipleItemStacks_ShouldTrackCorrectly()
    {
        // Arrange
        var container = new MockItemContainer(100);
        var potionStack = new MockItemStack("potion_stack", "Health Potions", 50);
        var swordStack = new MockItemStack("sword_stack", "Swords", 10);
        
        potionStack.TryAddItem("health_potion", 10);
        swordStack.TryAddItem("iron_sword", 2);

        // Act
        bool addPotions = container.TryAddItemStack(potionStack);
        bool addSwords = container.TryAddItemStack(swordStack);

        // Assert
        Assert.True(addPotions);
        Assert.True(addSwords);
        Assert.Equal(12, container.CurrentCount);
        Assert.Equal(2, container.GetAllItemStacks().Count());
    }

    #endregion

    #region Inventory Operations

    [Fact]
    public void Inventory_AddItemStack_ShouldSucceed()
    {
        // Arrange
        var inventory = new MockInventory("player_inventory", "Player Inventory", 100);
        var itemStack = new MockItemStack("potion_stack", "Health Potions", 50);
        itemStack.TryAddItem("health_potion", 10);

        // Act
        bool result = inventory.TryAddItemStack(itemStack);

        // Assert
        Assert.True(result);
        Assert.Equal(10, inventory.CurrentCount);
        Assert.Single(inventory.GetAllItemStacks());
    }

    [Fact]
    public void Inventory_AddItemStack_ExceedInventoryCapacity_ShouldFail()
    {
        // Arrange
        var inventory = new MockInventory("player_inventory", "Player Inventory", 5);
        var itemStack = new MockItemStack("potion_stack", "Health Potions", 50);
        itemStack.TryAddItem("health_potion", 10);

        // Act
        bool result = inventory.TryAddItemStack(itemStack);

        // Assert
        Assert.False(result);
        Assert.Equal(0, inventory.CurrentCount);
        Assert.Empty(inventory.GetAllItemStacks());
    }

    [Fact]
    public void Inventory_AddCompatibleStacks_ShouldMerge()
    {
        // Arrange
        var inventory = new MockInventory("player_inventory", "Player Inventory", 100);
        var stack1 = new MockItemStack("potion_stack", "Health Potions", 20);
        var stack2 = new MockItemStack("potion_stack", "Health Potions", 20);
        
        stack1.TryAddItem("health_potion", 10);
        stack2.TryAddItem("health_potion", 5);

        // Act
        bool addFirst = inventory.TryAddItemStack(stack1);
        bool addSecond = inventory.TryAddItemStack(stack2);

        // Assert
        Assert.True(addFirst);
        Assert.True(addSecond);
        Assert.Equal(15, inventory.CurrentCount);
        Assert.Single(inventory.GetAllItemStacks()); // Should merge into one stack
    }

    [Fact]
    public void Inventory_AddIncompatibleStacks_ShouldNotMerge()
    {
        // Arrange
        var inventory = new MockInventory("player_inventory", "Player Inventory", 100);
        var potionStack = new MockItemStack("potion_stack", "Health Potions", 20);
        var swordStack = new MockItemStack("sword_stack", "Swords", 10);
        
        potionStack.TryAddItem("health_potion", 10);
        swordStack.TryAddItem("iron_sword", 2);

        // Act
        bool addPotions = inventory.TryAddItemStack(potionStack);
        bool addSwords = inventory.TryAddItemStack(swordStack);

        // Assert
        Assert.True(addPotions);
        Assert.True(addSwords);
        Assert.Equal(12, inventory.CurrentCount);
        Assert.Equal(2, inventory.GetAllItemStacks().Count()); // Should remain separate
    }

    [Fact]
    public void Inventory_AddStack_ExceedStackCapacity_ShouldCreateNewStack()
    {
        // Arrange
        var inventory = new MockInventory("player_inventory", "Player Inventory", 100);
        var stack1 = new MockItemStack("potion_stack", "Health Potions", 15);
        var stack2 = new MockItemStack("potion_stack", "Health Potions", 15);
        
        stack1.TryAddItem("health_potion", 10);
        stack2.TryAddItem("health_potion", 10); // This would exceed stack capacity if merged

        // Act
        bool addFirst = inventory.TryAddItemStack(stack1);
        bool addSecond = inventory.TryAddItemStack(stack2);

        // Assert
        Assert.True(addFirst);
        Assert.False(addSecond); // Should fail because it would exceed stack capacity
        Assert.Equal(10, inventory.CurrentCount);
        Assert.Single(inventory.GetAllItemStacks());
    }

    #endregion

    #region Integration Scenarios

    [Fact]
    public void Integration_ContainerToInventoryTransfer_ShouldWork()
    {
        // Arrange
        var container = new MockItemContainer(100);
        var inventory = new MockInventory("player_inventory", "Player Inventory", 100);
        var itemStack = new MockItemStack("potion_stack", "Health Potions", 50);
        itemStack.TryAddItem("health_potion", 10);

        // Act
        bool addToContainer = container.TryAddItemStack(itemStack);
        bool transferToInventory = inventory.TryAddItemStack(itemStack);

        // Assert
        Assert.True(addToContainer);
        Assert.True(transferToInventory);
        Assert.Equal(10, inventory.CurrentCount);
        Assert.Single(inventory.GetAllItemStacks());
    }

    [Fact]
    public void Integration_MultipleContainersToInventory_ShouldRespectCapacity()
    {
        // Arrange
        var container1 = new MockItemContainer(50);
        var container2 = new MockItemContainer(50);
        var inventory = new MockInventory("player_inventory", "Player Inventory", 15); // Limited capacity
        
        var stack1 = new MockItemStack("potion_stack", "Health Potions", 50);
        var stack2 = new MockItemStack("sword_stack", "Swords", 10);
        
        stack1.TryAddItem("health_potion", 10);
        stack2.TryAddItem("iron_sword", 10);

        // Act
        container1.TryAddItemStack(stack1);
        container2.TryAddItemStack(stack2);
        
        bool addStack1 = inventory.TryAddItemStack(stack1);
        bool addStack2 = inventory.TryAddItemStack(stack2);

        // Assert
        Assert.True(addStack1); // First stack should fit
        Assert.False(addStack2); // Second stack should exceed capacity
        Assert.Equal(10, inventory.CurrentCount);
        Assert.Single(inventory.GetAllItemStacks());
    }

    [Fact]
    public void Integration_InventoryToContainerTransfer_ShouldWork()
    {
        // Arrange
        var inventory = new MockInventory("player_inventory", "Player Inventory", 100);
        var container = new MockItemContainer(100);
        var itemStack = new MockItemStack("potion_stack", "Health Potions", 50);
        itemStack.TryAddItem("health_potion", 10);

        // Act
        bool addToInventory = inventory.TryAddItemStack(itemStack);
        bool transferToContainer = container.TryAddItemStack(itemStack);

        // Assert
        Assert.True(addToInventory);
        Assert.True(transferToContainer);
        Assert.Equal(10, container.CurrentCount);
        Assert.Single(container.GetAllItemStacks());
    }

    #endregion

    #region Edge Cases and Error Handling

    [Fact]
    public void ItemStack_NullItemId_ShouldFail()
    {
        // Arrange
        var itemStack = new MockItemStack("test_stack", "Test Stack", 100);

        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => itemStack.TryAddItem(null!, 5));
    }

    [Fact]
    public void ItemStack_EmptyItemId_ShouldFail()
    {
        // Arrange
        var itemStack = new MockItemStack("test_stack", "Test Stack", 100);

        // Act
        bool result = itemStack.TryAddItem("", 5);

        // Assert
        Assert.False(result);
    }

    [Fact]
    public void Inventory_NullItemStack_ShouldFail()
    {
        // Arrange
        var inventory = new MockInventory("test_inventory", "Test Inventory", 100);

        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => inventory.TryAddItemStack(null!));
    }

    [Fact]
    public void Inventory_RemoveNonExistentStack_ShouldFail()
    {
        // Arrange
        var inventory = new MockInventory("test_inventory", "Test Inventory", 100);
        var itemStack = new MockItemStack("test_stack", "Test Stack", 50);

        // Act
        bool result = inventory.TryRemoveItemStack(itemStack);

        // Assert
        Assert.False(result);
        Assert.Empty(inventory.GetAllItemStacks());
    }

    #endregion
}
