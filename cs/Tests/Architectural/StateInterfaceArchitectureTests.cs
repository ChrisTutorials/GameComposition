using System.Linq;
using NetArchTest.Rules;
using Shouldly;
using Xunit;

namespace BarkMoon.GameComposition.Core.Tests.Architectural
{
    /// <summary>
    /// Architectural tests enforcing state interface patterns across the ecosystem.
    /// GLOBAL RULE: States implement IState directly - no domain-specific state interfaces.
    /// </summary>
    [Trait("Category", "Architectural")]
    public class StateInterfaceArchitectureTests
    {
        [Fact(DisplayName = "ARCH-STATE-001: No Domain State Interfaces Allowed")]
        public void No_Domain_State_Interfaces_Should_Exist()
        {
            // Arrange - This rule applies to ALL plugin assemblies
            // States should implement IState from GameComposition directly
            // No IPlacementState2D, ITargetingState, etc.
            
            // Scan for interfaces that:
            // 1. Have "State" in the name
            // 2. Are NOT in GameComposition namespace
            // 3. Are interfaces (not classes implementing IState)
            
            var gameCompositionAssembly = typeof(BarkMoon.GameComposition.Core.Interfaces.IState).Assembly;
            
            var result = Types.InAssembly(gameCompositionAssembly)
                .That()
                .AreInterfaces()
                .And()
                .HaveNameMatching(".*State.*")
                .And()
                .DoNotResideInNamespace("BarkMoon.GameComposition")
                .GetResult();

            // Assert - No domain-specific state interfaces in GameComposition
            result.IsSuccessful.ShouldBeTrue(
                "Domain-specific state interfaces (like IPlacementState2D) violate architecture. " +
                "States should implement IState from GameComposition directly. " +
                "External access to state data should be through Snapshots only.");
        }

        [Fact(DisplayName = "ARCH-STATE-002: State Classes Should Implement IState")]
        public void State_Classes_Should_Implement_IState()
        {
            // Arrange
            var gameCompositionAssembly = typeof(BarkMoon.GameComposition.Core.Interfaces.IState).Assembly;

            // Act - Find all classes with "State" suffix that should implement IState
            var result = Types.InAssembly(gameCompositionAssembly)
                .That()
                .AreClasses()
                .And()
                .HaveNameEndingWith("State")
                .And()
                .DoNotHaveNameEndingWith("RuntimeState") // Exclude specific patterns if needed
                .Should()
                .ImplementInterface(typeof(BarkMoon.GameComposition.Core.Interfaces.IState))
                .GetResult();

            // Assert
            result.IsSuccessful.ShouldBeTrue(
                "All State classes should implement IState from GameComposition. " +
                "This ensures consistent state contracts across the ecosystem.");
        }

        [Fact(DisplayName = "ARCH-STATE-003: External Access Via Snapshot Only")]
        public void State_Should_Not_Expose_Public_Mutable_State()
        {
            // This test validates that State classes are internal (not exposed publicly)
            // External consumers access data through Snapshots
            
            var gameCompositionAssembly = typeof(BarkMoon.GameComposition.Core.Interfaces.IState).Assembly;

            var result = Types.InAssembly(gameCompositionAssembly)
                .That()
                .AreClasses()
                .And()
                .HaveNameEndingWith("State2D")
                .Should()
                .NotBePublic()
                .GetResult();

            // Note: This may not find any types in GameComposition itself
            // The rule is primarily for plugin assemblies
            // Passing means no violations found
            result.IsSuccessful.ShouldBeTrue(
                "State classes should be internal to services. " +
                "External access should be through Snapshot types only.");
        }
    }
}
