using System.Collections.Generic;
using Xunit;
using GameComposition.Core.Types;

namespace GameComposition.Core.Tests.Types;

/// <summary>
/// Basic tests for TypedId item system interfaces.
/// Verifies interface compatibility and core functionality.
/// </summary>
public class TypedIdItemSystemTests
{
    #region ItemTypeId Tests

    [Fact]
    public void ItemTypeId_CoreTypes_ShouldBePreRegistered()
    {
        // Arrange & Act & Assert
        Assert.Equal("Weapon", ItemTypeId.FromId(1).Name);
        Assert.Equal("Armor", ItemTypeId.FromId(2).Name);
        Assert.Equal("Consumable", ItemTypeId.FromId(3).Name);
        Assert.Equal("Sword", ItemTypeId.FromId(10).Name);
        Assert.Equal("Potion", ItemTypeId.FromId(20).Name);
        Assert.Equal("Common", ItemTypeId.FromId(100).Name);
        Assert.Equal("Epic", ItemTypeId.FromId(103).Name);
    }

    [Fact]
    public void ItemTypeId_RegisterNewType_ShouldWork()
    {
        // Arrange
        const string typeName = "MagicItem";
        const int typeId = 2000;

        // Act
        var registeredType = ItemTypeId.Register(typeId, typeName);

        // Assert
        Assert.Equal(typeId, registeredType.Value);
        Assert.Equal(typeName, registeredType.Name);
        Assert.Equal(registeredType, ItemTypeId.FromId(typeId));
        Assert.Equal(registeredType, ItemTypeId.FromName(typeName));
    }

    [Fact]
    public void ItemTypeId_RegisterWithAutoId_ShouldWork()
    {
        // Arrange
        const string typeName = "AutoItem";

        // Act
        var registeredType = ItemTypeId.Register(typeName);

        // Assert
        Assert.Equal(typeName, registeredType.Name);
        Assert.True(registeredType.Value >= 1000); // Auto-assigned IDs start at 1000
        Assert.Equal(registeredType, ItemTypeId.FromName(typeName));
    }

    [Fact]
    public void ItemTypeId_RegisterDuplicate_ShouldThrow()
    {
        // Arrange
        const string typeName = "Duplicate";
        const int typeId = 3000;
        ItemTypeId.Register(typeId, typeName);

        // Act & Assert
        Assert.Throws<System.ArgumentException>(() => ItemTypeId.Register(typeId, typeName));
        Assert.Throws<System.ArgumentException>(() => ItemTypeId.Register(typeId, "DifferentName"));
        Assert.Throws<System.ArgumentException>(() => ItemTypeId.Register(9999, typeName));
    }

    [Theory]
    [InlineData(0, "None")]
    [InlineData(999, "None")]
    [InlineData(-1, "None")]
    public void ItemTypeId_FromIdNotFound_ShouldReturnNone(int id, string expectedName)
    {
        // Arrange & Act
        var result = ItemTypeId.FromId(id);

        // Assert
        Assert.Equal(expectedName, result.Name);
        Assert.Equal(0, result.Value);
    }

    [Fact]
    public void ItemTypeId_IsCompatible_ShouldWork()
    {
        // Arrange
        var swordType1 = ItemTypeId.FromId(10); // Sword
        var swordType2 = ItemTypeId.FromId(10); // Sword (same)
        var potionType = ItemTypeId.FromId(20); // Potion (different)

        // Act & Assert
        Assert.True(swordType1.IsCompatible(swordType2));
        Assert.False(swordType1.IsCompatible(potionType));
    }

    #endregion

    #region Interface Compatibility Tests

    [Fact]
    public void IItem_IItemStack_Compatibility_ShouldWork()
    {
        // This test verifies that the interfaces are designed to work together
        // Since we're testing interfaces only, we verify the contract compatibility
        
        // Arrange - Create mock implementations that follow the interface contracts
        var mockItem = new MockItem(ItemTypeId.FromId(10), "test_sword", "Test Sword");
        var mockStack = new MockItemStack(mockItem, 5);

        // Act & Assert - Verify interface compatibility
        Assert.Equal(mockItem.TypeId, mockStack.Item.TypeId);
        Assert.Equal(mockItem.MaxStackSize, mockStack.MaxStackSize);
        Assert.True(mockStack.Quantity <= mockStack.MaxStackSize);
        Assert.Equal(mockItem.Weight * mockStack.Quantity, mockStack.TotalWeight);
        Assert.Equal(mockItem.Value * mockStack.Quantity, mockStack.TotalValue);
    }

    [Theory]
    [InlineData(1, 5, true)]  // Can add 5 to empty stack with capacity 10 (1+5=6 <= 10)
    [InlineData(1, 9, true)]  // Can add 9 to empty stack with capacity 10 (1+9=10 <= 10)
    [InlineData(1, 10, false)] // Cannot add 10 to stack with capacity 10 (1+10=11 > 10)
    [InlineData(5, 5, true)]  // Can add 5 to half-full stack with capacity 10 (5+5=10 <= 10)
    [InlineData(5, 6, false)] // Cannot add 6 to half-full stack with capacity 10 (5+6=11 > 10)
    public void IItemStack_CanAdd_ShouldWorkCorrectly(int initialQuantity, int addQuantity, bool expectedResult)
    {
        // Arrange
        var mockItem = new MockItem(ItemTypeId.FromId(10), "test_sword", "Test Sword") { MaxStackSize = 10 };
        var mockStack = new MockItemStack(mockItem, initialQuantity);

        // Debug info
        System.Diagnostics.Debug.WriteLine($"Test: initial={initialQuantity}, add={addQuantity}, expected={expectedResult}");
        System.Diagnostics.Debug.WriteLine($"Actual: quantity={mockStack.Quantity}, max={mockStack.MaxStackSize}, canAdd={mockStack.CanAdd(addQuantity)}");

        // Act
        var result = mockStack.CanAdd(addQuantity);

        // Assert
        Assert.Equal(expectedResult, result);
    }

    [Fact]
    public void IItemStack_TryAdd_ValidQuantity_ShouldSucceed()
    {
        // Arrange
        var mockItem = new MockItem(ItemTypeId.FromId(10), "test_sword", "Test Sword") { MaxStackSize = 10 };
        var mockStack = new MockItemStack(mockItem, 3);

        // Act
        var result = mockStack.TryAdd(2);

        // Assert
        Assert.True(result);
        Assert.Equal(5, mockStack.Quantity);
    }

    [Fact]
    public void IItemStack_TryAdd_ExceedCapacity_ShouldFail()
    {
        // Arrange
        var mockItem = new MockItem(ItemTypeId.FromId(10), "test_sword", "Test Sword") { MaxStackSize = 5 };
        var mockStack = new MockItemStack(mockItem, 3);

        // Act
        var result = mockStack.TryAdd(3); // Would make quantity 6, exceeding capacity 5

        // Assert
        Assert.False(result);
        Assert.Equal(3, mockStack.Quantity); // Quantity unchanged
    }

    [Fact]
    public void IItemStack_CanMerge_CompatibleTypes_ShouldReturnTrue()
    {
        // Arrange
        var swordType = ItemTypeId.FromId(10);
        var item1 = new MockItem(swordType, "sword1", "Sword 1") { MaxStackSize = 10 };
        var item2 = new MockItem(swordType, "sword2", "Sword 2") { MaxStackSize = 10 };
        var stack1 = new MockItemStack(item1, 3);
        var stack2 = new MockItemStack(item2, 4);

        // Act
        var canMerge = stack1.CanMerge(stack2);

        // Assert
        Assert.True(canMerge);
    }

    [Fact]
    public void IItemStack_CanMerge_IncompatibleTypes_ShouldReturnFalse()
    {
        // Arrange
        var swordItem = new MockItem(ItemTypeId.FromId(10), "sword", "Sword") { MaxStackSize = 10 };
        var potionItem = new MockItem(ItemTypeId.FromId(20), "potion", "Potion") { MaxStackSize = 20 };
        var swordStack = new MockItemStack(swordItem, 3);
        var potionStack = new MockItemStack(potionItem, 5);

        // Act
        var canMerge = swordStack.CanMerge(potionStack);

        // Assert
        Assert.False(canMerge);
    }

    #endregion

    #region Mock Implementations for Testing

    /// <summary>
    /// Mock implementation of IItem for testing interface compatibility.
    /// This demonstrates how plugins should implement the IItem interface.
    /// </summary>
    private class MockItem : IItem
    {
        public ItemTypeId TypeId { get; }
        public string Id { get; }
        public string Name { get; }
        public string Description { get; }
        public int MaxStackSize { get; set; }
        public float Weight { get; set; }
        public float Value { get; set; }
        public Dictionary<string, object> Properties { get; }

        public MockItem(ItemTypeId typeId, string id, string name)
        {
            TypeId = typeId;
            Id = id;
            Name = name;
            Description = $"Mock {name}";
            MaxStackSize = 10; // Default
            Weight = 1.0f; // Default
            Value = 10.0f; // Default
            Properties = new Dictionary<string, object>();
        }
    }

    /// <summary>
    /// Mock implementation of IItemStack for testing interface compatibility.
    /// This demonstrates how plugins should implement the IItemStack interface.
    /// </summary>
    private class MockItemStack : IItemStack
    {
        public IItem Item { get; }
        public int Quantity { get; set; }
        public int MaxStackSize => Item.MaxStackSize;
        public float TotalWeight => Quantity * Item.Weight;
        public float TotalValue => Quantity * Item.Value;
        public bool IsEmpty => Quantity <= 0;
        public bool IsFull => Quantity >= MaxStackSize;

        public MockItemStack(IItem item, int quantity)
        {
            Item = item;
            Quantity = Math.Max(0, quantity); // Don't clamp in constructor, let tests set exact values
        }

        public bool CanAdd(int quantity)
        {
            return Quantity + quantity <= MaxStackSize && quantity > 0;
        }

        public bool TryAdd(int quantity)
        {
            if (CanAdd(quantity))
            {
                Quantity += quantity;
                return true;
            }
            return false;
        }

        public bool TryRemove(int quantity, out IItemStack? removedStack)
        {
            if (quantity <= 0 || quantity > Quantity)
            {
                removedStack = null;
                return false;
            }

            removedStack = new MockItemStack(Item, quantity);
            Quantity -= quantity;
            return true;
        }

        public IItemStack? Split(int quantity)
        {
            if (quantity <= 0 || quantity > Quantity)
                return null;

            return new MockItemStack(Item, quantity);
        }

        public bool CanMerge(IItemStack other)
        {
            return Item.TypeId.IsCompatible(other.Item.TypeId) && 
                   Quantity + other.Quantity <= MaxStackSize;
        }

        public bool TryMerge(IItemStack other)
        {
            if (!CanMerge(other))
                return false;

            Quantity += other.Quantity;
            // Empty the other stack
            other = new MockItemStack(other.Item, 0);
            return true;
        }
    }

    #endregion
}
