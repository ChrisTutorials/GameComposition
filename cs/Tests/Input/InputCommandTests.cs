using System;
using BarkMoon.GameComposition.Core.Types;
using Xunit;

namespace GameComposition.Core.Tests.Input
{
    /// <summary>
    /// Comprehensive tests for the InputCommand enum + extensions pattern.
    /// Validates type safety, flexibility, and plugin extensibility.
    /// </summary>
    public class InputCommandTests
    {
        [Fact]
        public void InputCommand_CoreCommand_HasTypeSafety()
        {
            // Arrange & Act
            var command = InputCommand.Core(CoreInputCommandType.ActionStart);
            
            // Assert
            Assert.Equal(CoreInputCommandType.ActionStart, command.CoreType);
            Assert.Null(command.ExtensionType);
            Assert.True(command.IsCoreCommand);
            Assert.False(command.IsExtensionCommand);
            Assert.Equal("ActionStart", command.GetEffectiveType());
        }

        [Fact]
        public void InputCommand_ExtensionCommand_HasFlexibility()
        {
            // Arrange & Act
            var command = InputCommand.Extension("Plugin.Custom.Command");
            
            // Assert
            Assert.Equal(CoreInputCommandType.Unknown, command.CoreType);
            Assert.Equal("Plugin.Custom.Command", command.ExtensionType);
            Assert.False(command.IsCoreCommand);
            Assert.True(command.IsExtensionCommand);
            Assert.Equal("Plugin.Custom.Command", command.GetEffectiveType());
        }

        [Fact]
        public void InputCommand_HybridCommand_PrefersExtension()
        {
            // Arrange & Act
            var command = new InputCommand(
                CoreType: CoreInputCommandType.ActionStart,
                ExtensionType: "Plugin.Custom.Command"
            );
            
            // Assert
            Assert.Equal(CoreInputCommandType.ActionStart, command.CoreType);
            Assert.Equal("Plugin.Custom.Command", command.ExtensionType);
            Assert.True(command.IsCoreCommand);
            Assert.True(command.IsExtensionCommand);
            Assert.Equal("Plugin.Custom.Command", command.GetEffectiveType());
        }

        [Theory]
        [InlineData(CoreInputCommandType.Unknown)]
        [InlineData(CoreInputCommandType.NavigateStart)]
        [InlineData(CoreInputCommandType.ActionEnd)]
        [InlineData(CoreInputCommandType.Shutdown)]
        public void InputCommand_AllCoreEnumValues_AreValid(CoreInputCommandType coreType)
        {
            // Arrange & Act
            var command = InputCommand.Core(coreType);
            
            // Assert
            Assert.Equal(coreType, command.CoreType);
            Assert.NotNull(command.GetEffectiveType());
            Assert.NotEmpty(command.GetEffectiveType());
        }

        [Fact]
        public void InputCommand_Timestamp_SetCorrectly()
        {
            // Arrange
            var timestamp = DateTime.UtcNow.Ticks / 10000.0;
            
            // Act
            var command = InputCommand.Core(CoreInputCommandType.Initialize, timestamp);
            
            // Assert
            Assert.Equal(timestamp, command.Timestamp);
        }

        [Fact]
        public void InputCommand_DefaultValues_AreCorrect()
        {
            // Arrange & Act
            var command = new InputCommand();
            
            // Assert
            Assert.Equal(CoreInputCommandType.Unknown, command.CoreType);
            Assert.Null(command.ExtensionType);
            Assert.Equal(0, command.Timestamp);
            Assert.Equal("Unknown", command.GetEffectiveType());
        }

        [Fact]
        public void InputCommand_InterfaceImplementation_IsComplete()
        {
            // Arrange
            IInputCommand command = InputCommand.Core(CoreInputCommandType.ActionStart);
            
            // Act & Assert
            Assert.Equal(CoreInputCommandType.ActionStart, command.CoreType);
            Assert.Null(command.ExtensionType);
            Assert.Equal(0, command.Timestamp);
        }

        [Fact]
        public void InputCommand_RecordEquality_WorksCorrectly()
        {
            // Arrange
            var command1 = InputCommand.Core(CoreInputCommandType.ActionStart, 123);
            var command2 = InputCommand.Core(CoreInputCommandType.ActionStart, 123);
            var command3 = InputCommand.Core(CoreInputCommandType.ActionEnd, 123);
            
            // Act & Assert
            Assert.Equal(command1, command2);
            Assert.NotEqual(command1, command3);
        }

        [Fact]
        public void InputCommand_ExtensionNaming_FollowsConvention()
        {
            // Arrange & Act
            var extensionCommands = new[]
            {
                "Plugin.Manipulation.Start",
                "Plugin.Placement.End",
                "Plugin.Grid.Select",
                "Plugin.Targeting.Acquire"
            };
            
            // Assert
            foreach (var commandType in extensionCommands)
            {
                Assert.Contains('.', commandType);
                
                var command = InputCommand.Extension(commandType);
                Assert.Equal(commandType, command.GetEffectiveType());
            }
        }

        [Fact]
        public void InputCommand_GenericNaming_IsDomainAgnostic()
        {
            // Arrange & Act - Verify enum names are generic
            var coreCommands = Enum.GetValues<CoreInputCommandType>();
            
            // Assert - No domain-specific names in core enum
            var domainSpecificNames = new[] { "Manipulation", "Placement", "Grid", "Targeting", "Inventory", "Combat" };
            
            foreach (CoreInputCommandType command in coreCommands)
            {
                var commandName = command.ToString();
                
                foreach (var specificName in domainSpecificNames)
                {
                    Assert.DoesNotContain(specificName, commandName, StringComparison.OrdinalIgnoreCase);
                }
            }
        }
    }
}
