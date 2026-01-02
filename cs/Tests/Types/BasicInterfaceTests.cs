using Xunit;
using GameComposition.Core.Types;

namespace GameComposition.Core.Tests.Types;

/// <summary>
/// Basic tests to verify IItem and IItemStack interfaces work together.
/// Tests the fundamental contract compatibility without complex implementations.
/// </summary>
public class BasicInterfaceTests
{
    [Fact]
    public void IItem_Interface_ShouldHaveRequiredProperties()
    {
        // This test verifies the IItem interface contract
        // Since we're testing interfaces, we verify the design through reflection
        
        var itemType = typeof(IItem);
        
        // Verify required properties exist
        Assert.NotNull(itemType.GetProperty("TypeId"));
        Assert.NotNull(itemType.GetProperty("Id"));
        Assert.NotNull(itemType.GetProperty("Name"));
        Assert.NotNull(itemType.GetProperty("Description"));
        Assert.NotNull(itemType.GetProperty("MaxStackSize"));
        Assert.NotNull(itemType.GetProperty("Weight"));
        Assert.NotNull(itemType.GetProperty("Value"));
        Assert.NotNull(itemType.GetProperty("Properties"));
    }

    [Fact]
    public void IItemStack_Interface_ShouldHaveRequiredProperties()
    {
        // This test verifies the IItemStack interface contract
        
        var stackType = typeof(IItemStack);
        
        // Verify required properties exist
        Assert.NotNull(stackType.GetProperty("Item"));
        Assert.NotNull(stackType.GetProperty("Quantity"));
        Assert.NotNull(stackType.GetProperty("MaxStackSize"));
        Assert.NotNull(stackType.GetProperty("TotalWeight"));
        Assert.NotNull(stackType.GetProperty("TotalValue"));
        Assert.NotNull(stackType.GetProperty("IsEmpty"));
        Assert.NotNull(stackType.GetProperty("IsFull"));
        
        // Verify required methods exist
        Assert.NotNull(stackType.GetMethod("CanAdd"));
        Assert.NotNull(stackType.GetMethod("TryAdd"));
        Assert.NotNull(stackType.GetMethod("TryRemove"));
        Assert.NotNull(stackType.GetMethod("Split"));
        Assert.NotNull(stackType.GetMethod("CanMerge"));
        Assert.NotNull(stackType.GetMethod("TryMerge"));
    }

    [Fact]
    public void ItemTypeId_ShouldHaveCoreTypes()
    {
        // Verify core types are registered
        var weaponType = ItemTypeId.FromId(1);
        var armorType = ItemTypeId.FromId(2);
        var consumableType = ItemTypeId.FromId(3);
        
        Assert.Equal("Weapon", weaponType.Name);
        Assert.Equal("Armor", armorType.Name);
        Assert.Equal("Consumable", consumableType.Name);
        
        // Verify specific types
        var swordType = ItemTypeId.FromId(10);
        var potionType = ItemTypeId.FromId(20);
        
        Assert.Equal("Sword", swordType.Name);
        Assert.Equal("Potion", potionType.Name);
    }

    [Fact]
    public void ItemTypeId_ShouldSupportPluginRegistration()
    {
        // Test plugin registration capability
        var customType = ItemTypeId.Register("CustomPluginItem");
        
        Assert.Equal("CustomPluginItem", customType.Name);
        Assert.True(customType.Value >= 1000);
        
        // Verify lookup works
        var foundType = ItemTypeId.FromName("CustomPluginItem");
        Assert.Equal(customType, foundType);
    }

    [Fact]
    public void ItemTypeId_IsCompatible_ShouldWork()
    {
        var sword1 = ItemTypeId.FromId(10);
        var sword2 = ItemTypeId.FromId(10);
        var potion = ItemTypeId.FromId(20);
        
        Assert.True(sword1.IsCompatible(sword2));
        Assert.False(sword1.IsCompatible(potion));
    }
}
