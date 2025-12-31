using System.Collections.Generic;
using BarkMoon.GameComposition.Core.Types;

namespace GameComposition.Core.Tests.Types;

/// <summary>
/// Test data factory for advanced inventory scenarios
/// </summary>
public static class AdvancedInventoryTestData
{
    #region Container Test Data

    public static IEnumerable<object[]> ValidContainerScenarios()
    {
        yield return new object[] { "weapons", 5, 3, "iron_sword", "weapon" };
        yield return new object[] { "potions", 10, 7, "health_potion", "consumable" };
        yield return new object[] { "armor", 8, 4, "steel_helmet", "armor" };
        yield return new object[] { "misc", 15, 12, "magic_ring", "accessory" };
    }

    public static IEnumerable<object[]> ContainerCapacityTestData()
    {
        yield return new object[] { 5, 3, true };  // Under capacity
        yield return new object[] { 5, 5, true };  // At capacity
        yield return new object[] { 5, 6, false }; // Over capacity
        yield return new object[] { 10, 9, true }; // Under capacity
        yield return new object[] { 10, 10, true }; // At capacity
        yield return new object[] { 10, 11, false }; // Over capacity
    }

    #endregion

    #region Condition Test Data

    public static IEnumerable<object[]> TagConditionTestData()
    {
        yield return new object[] { "weapons", "weapons", true };  // Required tag matches
        yield return new object[] { "weapons", "potions", false }; // Required tag doesn't match
        yield return new object[] { "potions", "potions", true };  // Required tag matches
        yield return new object[] { "potions", "weapons", false }; // Required tag doesn't match
    }

    public static IEnumerable<object[]> BlockedTagTestData()
    {
        yield return new object[] { "weapons", "potions", true };  // Not blocked
        yield return new object[] { "potions", "potions", false }; // Blocked tag
        yield return new object[] { "armor", "weapons", true };    // Not blocked
        yield return new object[] { "weapons", "weapons", false }; // Blocked tag
    }

    public static IEnumerable<object[]> ItemTypeConditionTestData()
    {
        yield return new object[] { new[] { "weapon", "sword" }, "weapon", true };  // Allowed type
        yield return new object[] { new[] { "weapon", "sword" }, "consumable", false }; // Not allowed
        yield return new object[] { new[] { "consumable", "potion" }, "potion", true }; // Allowed type
        yield return new object[] { new[] { "consumable", "potion" }, "weapon", false }; // Not allowed
    }

    public static IEnumerable<object[]> BlockedItemTypeTestData()
    {
        yield return new object[] { new[] { "consumable", "food" }, "weapon", true };  // Not blocked
        yield return new object[] { new[] { "consumable", "food" }, "potion", false }; // Blocked type
        yield return new object[] { new[] { "weapon", "sword" }, "armor", true };     // Not blocked
        yield return new object[] { new[] { "weapon", "sword" }, "sword", false };    // Blocked type
    }

    #endregion

    #region Multi-Container Test Data

    public static IEnumerable<object[]> MultiContainerRoutingTestData()
    {
        yield return new object[] { 
            new[] { ("weapons", 5), ("potions", 5) }, 
            ("iron_sword", 3, "weapon"), 
            "weapons", 
            3 
        };
        yield return new object[] { 
            new[] { ("weapons", 5), ("potions", 5) }, 
            ("health_potion", 4, "consumable"), 
            "potions", 
            4 
        };
        yield return new object[] { 
            new[] { ("weapons", 3), ("armor", 5), ("misc", 10) }, 
            ("steel_sword", 2, "weapon"), 
            "weapons", 
            2 
        };
    }

    public static IEnumerable<object[]> FullContainerRoutingTestData()
    {
        yield return new object[] { 
            new[] { ("weapons", 5), ("potions", 5) }, 
            ("iron_sword", 3, "weapon"), 
            ("health_potion", 4, "consumable"),
            "potions", // Should route to second container
            4 
        };
        yield return new object[] { 
            new[] { ("weapons", 5), ("potions", 5) }, 
            ("health_potion", 4, "consumable"),
            ("iron_sword", 3, "weapon"), 
            "weapons", // Should route to first container
            3 
        };
    }

    #endregion

    #region Complex Scenario Test Data

    public static IEnumerable<object[]> ConditionalRoutingTestData()
    {
        yield return new object[] {
            // Container setup: (tag, capacity, allowedTypes)
            new[] { ("weapons", 5, new[] { "weapon", "sword" }), 
                    ("potions", 5, new[] { "consumable", "potion" }) },
            // Item to add: (itemId, quantity, itemType)
            ("iron_sword", 3, "weapon"),
            // Expected container and count
            "weapons", 3
        };
        yield return new object[] {
            new[] { ("weapons", 5, new[] { "weapon", "sword" }), 
                    ("potions", 5, new[] { "consumable", "potion" }) },
            ("health_potion", 4, "consumable"),
            "potions", 4
        };
        yield return new object[] {
            new[] { ("weapons", 5, new[] { "weapon", "sword" }), 
                    ("armor", 5, new[] { "armor", "helmet" }) },
            ("magic_ring", 2, "accessory"),
            null, 0 // No compatible container
        };
    }

    #endregion

    #region Priority Test Data

    public static IEnumerable<object[]> ConditionPriorityTestData()
    {
        yield return new object[] {
            // Conditions: (type, priority, config)
            new (string, int, object)[] { ("tag", 100, "weapons"), ("type", 1, new[] { "weapon" }) },
            // Item: (itemId, quantity, itemType)
            ("iron_sword", 2, "weapon"),
            // Container tag
            "weapons",
            true // Should be accepted
        };
        yield return new object[] {
            new (string, int, object)[] { ("tag", 100, "weapons"), ("type", 1, new[] { "potion" }) },
            ("health_potion", 3, "consumable"),
            "weapons",
            false // Should be rejected (type condition fails)
        };
    }

    #endregion
}
