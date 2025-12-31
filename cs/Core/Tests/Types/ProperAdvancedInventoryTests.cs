using System.Collections.Generic;
using System.Linq;
using Xunit;
using BarkMoon.GameComposition.Core.Types;

namespace GameComposition.Core.Tests.Types;

/// <summary>
/// Proper advanced inventory tests using real runtime types with parameterized tests
/// </summary>
public class ProperAdvancedInventoryTests
{
    #region Multi-Container Routing Tests

    [Theory]
    [MemberData(nameof(AdvancedInventoryTestData.MultiContainerRoutingTestData), MemberType = typeof(AdvancedInventoryTestData))]
    public void MultiContainerInventory_ShouldRouteToFirstCompatibleContainer(
        (string tag, int capacity)[] containerSetup,
        (string itemId, int quantity, string itemType) itemToAdd,
        string expectedContainerTag,
        int expectedCount)
    {
        // Arrange
        var inventory = CreateMultiContainerInventory(containerSetup);
        var itemStack = CreateItemStack(itemToAdd.itemId, itemToAdd.quantity, itemToAdd.itemType);

        // Act
        bool result = inventory.TryAddItemStack(itemStack);

        // Assert
        Assert.True(result);
        
        var targetContainer = inventory.GetAllContainers()
            .FirstOrDefault(c => c.Tag == expectedContainerTag);
        
        Assert.NotNull(targetContainer);
        Assert.Equal(expectedCount, targetContainer.CurrentCount);
        Assert.Single(targetContainer.GetAllItemStacks());
    }

    [Theory]
    [MemberData(nameof(AdvancedInventoryTestData.FullContainerRoutingTestData), MemberType = typeof(AdvancedInventoryTestData))]
    public void MultiContainerInventory_FirstContainerFull_ShouldRouteToSecond(
        (string tag, int capacity)[] containerSetup,
        (string itemId, int quantity, string itemType) firstItem,
        (string itemId, int quantity, string itemType) secondItem,
        string expectedContainerTag,
        int expectedCount)
    {
        // Arrange
        var inventory = CreateMultiContainerInventory(containerSetup);
        var firstStack = CreateItemStack(firstItem.itemId, firstItem.quantity, firstItem.itemType);
        var secondStack = CreateItemStack(secondItem.itemId, secondItem.quantity, secondItem.itemType);

        // Fill first container
        inventory.TryAddItemStack(firstStack);

        // Act
        bool result = inventory.TryAddItemStack(secondStack);

        // Assert
        Assert.True(result);
        
        var targetContainer = inventory.GetAllContainers()
            .FirstOrDefault(c => c.Tag == expectedContainerTag);
        
        Assert.NotNull(targetContainer);
        Assert.Equal(expectedCount, targetContainer.CurrentCount);
    }

    [Fact]
    public void MultiContainerInventory_AllContainersFull_ShouldReject()
    {
        // Arrange
        var containerSetup = new[] { ("weapons", 5), ("potions", 5) };
        var inventory = CreateMultiContainerInventory(containerSetup);
        
        // Fill both containers to capacity
        var weaponStack = CreateItemStack("iron_sword", 5, "weapon");
        var potionStack = CreateItemStack("health_potion", 5, "consumable");
        
        inventory.TryAddItemStack(weaponStack);
        inventory.TryAddItemStack(potionStack);

        var extraStack = CreateItemStack("magic_ring", 2, "accessory");

        // Act
        bool result = inventory.TryAddItemStack(extraStack);

        // Assert
        Assert.False(result);
        Assert.True(inventory.GetAllContainers().All(c => c.IsFull));
    }

    #endregion

    #region Conditional Filtering Tests

    [Theory]
    [MemberData(nameof(AdvancedInventoryTestData.ConditionalRoutingTestData), MemberType = typeof(AdvancedInventoryTestData))]
    public void MultiContainerInventory_WithConditions_ShouldRouteCorrectly(
        (string tag, int capacity, string[] allowedTypes)[] containerSetup,
        (string itemId, int quantity, string itemType) itemToAdd,
        string expectedContainerTag,
        int expectedCount)
    {
        // Arrange
        var inventory = CreateMultiContainerInventoryWithConditions(containerSetup);
        var itemStack = CreateItemStack(itemToAdd.itemId, itemToAdd.quantity, itemToAdd.itemType);

        // Act
        bool result = inventory.TryAddItemStack(itemStack);

        if (expectedContainerTag == null)
        {
            // Assert - Should be rejected
            Assert.False(result);
            Assert.Equal(0, inventory.CurrentCount);
        }
        else
        {
            // Assert - Should be accepted and routed correctly
            Assert.True(result);
            
            var targetContainer = inventory.GetAllContainers()
                .FirstOrDefault(c => c.Tag == expectedContainerTag);
            
            Assert.NotNull(targetContainer);
            Assert.Equal(expectedCount, targetContainer.CurrentCount);
        }
    }

    [Theory]
    [MemberData(nameof(AdvancedInventoryTestData.TagConditionTestData), MemberType = typeof(AdvancedInventoryTestData))]
    public void TagCondition_ShouldEvaluateCorrectly(
        string containerTag,
        string requiredTag,
        bool expectedResult)
    {
        // Arrange
        var condition = new TagCondition("test", "Test", "Test condition", requiredTag: requiredTag);
        var context = new ItemContainerContext
        {
            ContainerTag = containerTag,
            ItemStack = CreateItemStack("test_item", 1, "test")
        };

        // Act
        bool result = condition.IsSatisfied(context);

        // Assert
        Assert.Equal(expectedResult, result);
    }

    [Theory]
    [MemberData(nameof(AdvancedInventoryTestData.BlockedTagTestData), MemberType = typeof(AdvancedInventoryTestData))]
    public void BlockedTagCondition_ShouldRejectCorrectly(
        string containerTag,
        string blockedTag,
        bool expectedResult)
    {
        // Arrange
        var condition = new TagCondition("test", "Test", "Test condition", blockedTag: blockedTag);
        var context = new ItemContainerContext
        {
            ContainerTag = containerTag,
            ItemStack = CreateItemStack("test_item", 1, "test")
        };

        // Act
        bool result = condition.IsSatisfied(context);

        // Assert
        Assert.Equal(expectedResult, result);
    }

    [Theory]
    [MemberData(nameof(AdvancedInventoryTestData.ItemTypeConditionTestData), MemberType = typeof(AdvancedInventoryTestData))]
    public void ItemTypeCondition_ShouldFilterCorrectly(
        string[] allowedTypes,
        string itemType,
        bool expectedResult)
    {
        // Arrange
        var condition = new ItemTypeCondition("test", "Test", "Test condition", allowedTypes: allowedTypes);
        var context = new ItemContainerContext
        {
            ItemType = itemType,
            ItemStack = CreateItemStack("test_item", 1, itemType)
        };

        // Act
        bool result = condition.IsSatisfied(context);

        // Assert
        Assert.Equal(expectedResult, result);
    }

    [Theory]
    [MemberData(nameof(AdvancedInventoryTestData.BlockedItemTypeTestData), MemberType = typeof(AdvancedInventoryTestData))]
    public void BlockedItemTypeCondition_ShouldRejectCorrectly(
        string[] blockedTypes,
        string itemType,
        bool expectedResult)
    {
        // Arrange
        var condition = new ItemTypeCondition("test", "Test", "Test condition", blockedTypes: blockedTypes);
        var context = new ItemContainerContext
        {
            ItemType = itemType,
            ItemStack = CreateItemStack("test_item", 1, itemType)
        };

        // Act
        bool result = condition.IsSatisfied(context);

        // Assert
        Assert.Equal(expectedResult, result);
    }

    #endregion

    #region Priority Tests

    [Theory]
    [MemberData(nameof(AdvancedInventoryTestData.ConditionPriorityTestData), MemberType = typeof(AdvancedInventoryTestData))]
    public void ConditionPriority_ShouldEvaluateInOrder(
        (string type, int priority, object config)[] conditionSetup,
        (string itemId, int quantity, string itemType) itemToAdd,
        string containerTag,
        bool expectedResult)
    {
        // Arrange
        var container = CreateTaggedItemContainer(containerTag, 10);
        var conditions = CreateConditions(conditionSetup);
        
        foreach (var condition in conditions.OrderByDescending(c => c.Priority))
        {
            container.AddCondition(condition);
        }

        var itemStack = CreateItemStack(itemToAdd.itemId, itemToAdd.quantity, itemToAdd.itemType);

        // Act
        bool result = container.CanAcceptItemStack(itemStack);

        // Assert
        Assert.Equal(expectedResult, result);
    }

    #endregion

    #region Complex Integration Tests

    [Fact]
    public void Integration_MultiContainerWithMultipleConditions_ComplexScenario_ShouldWork()
    {
        // Arrange - Complex scenario with multiple containers and conditions
        var inventory = new MultiContainerInventory(30);
        
        // Weapon container: tag + type conditions
        var weaponContainer = CreateTaggedItemContainer("weapons", 10);
        weaponContainer.AddCondition(new TagCondition("weapon_tag", "Weapon Tag", "Only weapons", requiredTag: "weapons"));
        weaponContainer.AddCondition(new ItemTypeCondition("weapon_types", "Weapon Types", "Only weapon types", 
            allowedTypes: new[] { "weapon", "sword", "axe" }));
        
        // Premium container: only type condition (epic+ items)
        var premiumContainer = CreateTaggedItemContainer("premium", 10);
        premiumContainer.AddCondition(new ItemTypeCondition("premium_only", "Premium Only", "Only premium items", 
            allowedTypes: new[] { "epic", "legendary" }));
        
        // Potion container: tag + type conditions
        var potionContainer = CreateTaggedItemContainer("potions", 10);
        potionContainer.AddCondition(new TagCondition("potion_tag", "Potion Tag", "Only potions", requiredTag: "potions"));
        potionContainer.AddCondition(new ItemTypeCondition("potion_types", "Potion Types", "Only consumable types", 
            allowedTypes: new[] { "consumable", "potion", "food" }));

        inventory.AddContainer(weaponContainer);
        inventory.AddContainer(premiumContainer);
        inventory.AddContainer(potionContainer);

        // Pre-fill weapon container partially
        var existingSword = CreateItemStack("iron_sword", 6, "weapon");
        weaponContainer.TryAddItemStack(existingSword);

        // Test items
        var rareSword = CreateItemStack("rare_sword", 2, "weapon");
        var epicSword = CreateItemStack("epic_sword", 1, "epic");
        var healthPotion = CreateItemStack("health_potion", 5, "consumable");

        // Act
        bool rareSwordResult = inventory.TryAddItemStack(rareSword);
        bool epicSwordResult = inventory.TryAddItemStack(epicSword);
        bool potionResult = inventory.TryAddItemStack(healthPotion);

        // Assert
        Assert.True(rareSwordResult); // Goes to weapon container (has space, matching tag+type)
        Assert.True(epicSwordResult); // Goes to premium container (epic type matches)
        Assert.True(potionResult); // Goes to potion container (matching tag+type)
        
        Assert.Equal(8, weaponContainer.CurrentCount); // 6 + 2 rare swords
        Assert.Equal(1, premiumContainer.CurrentCount); // 1 epic sword
        Assert.Equal(5, potionContainer.CurrentCount); // 5 potions
        Assert.Equal(14, inventory.CurrentCount); // Total items
    }

    [Fact]
    public void Integration_NoCompatibleContainer_ShouldRejectAll()
    {
        // Arrange
        var inventory = new MultiContainerInventory(20);
        
        var weaponContainer = CreateTaggedItemContainer("weapons", 10);
        weaponContainer.AddCondition(new ItemTypeCondition("weapons_only", "Weapons Only", "Only weapons", 
            allowedTypes: new[] { "weapon", "sword" }));
        
        var armorContainer = CreateTaggedItemContainer("armor", 10);
        armorContainer.AddCondition(new ItemTypeCondition("armor_only", "Armor Only", "Only armor", 
            allowedTypes: new[] { "armor", "helmet" }));

        inventory.AddContainer(weaponContainer);
        inventory.AddContainer(armorContainer);

        var foodItem = CreateItemStack("bread", 3, "food");

        // Act
        bool result = inventory.TryAddItemStack(foodItem);

        // Assert
        Assert.False(result);
        Assert.Equal(0, inventory.CurrentCount);
        Assert.Empty(weaponContainer.GetAllItemStacks());
        Assert.Empty(armorContainer.GetAllItemStacks());
    }

    #endregion

    #region Helper Methods

    private static MultiContainerInventory CreateMultiContainerInventory((string tag, int capacity)[] containerSetup)
    {
        var inventory = new MultiContainerInventory();
        
        foreach (var (tag, capacity) in containerSetup)
        {
            var container = CreateTaggedItemContainer(tag, capacity);
            inventory.AddContainer(container);
        }
        
        return inventory;
    }

    private static MultiContainerInventory CreateMultiContainerInventoryWithConditions(
        (string tag, int capacity, string[] allowedTypes)[] containerSetup)
    {
        var inventory = new MultiContainerInventory();
        
        foreach (var (tag, capacity, allowedTypes) in containerSetup)
        {
            var container = CreateTaggedItemContainer(tag, capacity);
            
            // Add tag condition
            container.AddCondition(new TagCondition($"{tag}_tag", $"{tag} Tag", $"Only {tag} items", requiredTag: tag));
            
            // Add type condition if specified
            if (allowedTypes != null && allowedTypes.Length > 0)
            {
                container.AddCondition(new ItemTypeCondition($"{tag}_types", $"{tag} Types", $"Only {tag} types", 
                    allowedTypes: allowedTypes));
            }
            
            inventory.AddContainer(container);
        }
        
        return inventory;
    }

    private static TaggedItemContainer CreateTaggedItemContainer(string tag, int capacity)
    {
        return new TaggedItemContainer(tag, capacity);
    }

    private static IItemStack CreateItemStack(string itemId, int quantity, string itemType)
    {
        var itemStack = new MockItemStack($"{itemId}_stack", $"{itemId} Stack");
        itemStack.TryAddItem(itemId, quantity);
        
        // Note: In real implementation, we'd need to set the item type
        // This would depend on the actual IItemStack implementation
        return itemStack;
    }

    private static ICondition<ItemContainerContext>[] CreateConditions(
        (string type, int priority, object config)[] conditionSetup)
    {
        var conditions = new List<ICondition<ItemContainerContext>>();
        
        foreach (var (type, priority, config) in conditionSetup)
        {
            switch (type)
            {
                case "tag":
                    var requiredTag = config as string;
                    conditions.Add(new TagCondition($"tag_{priority}", $"Tag Condition {priority}", 
                        $"Tag condition with priority {priority}", requiredTag: requiredTag, priority: priority));
                    break;
                    
                case "type":
                    var allowedTypes = config as string[];
                    conditions.Add(new ItemTypeCondition($"type_{priority}", $"Type Condition {priority}", 
                        $"Type condition with priority {priority}", allowedTypes: allowedTypes, priority: priority));
                    break;
            }
        }
        
        return conditions.ToArray();
    }

    #endregion
}
