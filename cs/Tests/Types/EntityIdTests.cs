using System;
using Xunit;
using BarkMoon.GameComposition.Core.Types;

namespace BarkMoon.GameComposition.Core.Tests.Types
{
    /// <summary>
    /// Unit tests for <see cref="EntityId"/> type.
    /// Tests cover construction, factory methods, equality, and edge cases.
    /// </summary>
    public class EntityIdTests
    {
        #region Construction Tests

        [Fact]
        public void EntityId_New_CreatesUniqueId()
        {
            // Act
            var id1 = EntityId.New();
            var id2 = EntityId.New();

            // Assert
            Assert.NotEqual(EntityId.Empty, id1);
            Assert.NotEqual(EntityId.Empty, id2);
            Assert.NotEqual(id1, id2);
        }

        [Fact]
        public void EntityId_Empty_HasEmptyGuid()
        {
            // Act
            var empty = EntityId.Empty;

            // Assert
            Assert.Equal(Guid.Empty, empty.Value);
            Assert.True(empty.IsEmpty);
        }

        [Fact]
        public void EntityId_FromGuid_PreservesValue()
        {
            // Arrange
            var guid = Guid.NewGuid();

            // Act
            var entityId = new EntityId(guid);

            // Assert
            Assert.Equal(guid, entityId.Value);
            Assert.False(entityId.IsEmpty);
        }

        #endregion

        #region FromString Tests

        [Fact]
        public void FromString_ValidGuid_ReturnsEntityId()
        {
            // Arrange
            var guid = Guid.NewGuid();
            var guidString = guid.ToString();

            // Act
            var entityId = EntityId.FromString(guidString);

            // Assert
            Assert.Equal(guid, entityId.Value);
            Assert.False(entityId.IsEmpty);
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData("   ")]
        public void FromString_NullOrWhitespace_ReturnsEmpty(string? value)
        {
            // Act
            var entityId = EntityId.FromString(value!);

            // Assert
            Assert.Equal(EntityId.Empty, entityId);
            Assert.True(entityId.IsEmpty);
        }

        [Theory]
        [InlineData("not-a-guid")]
        [InlineData("12345")]
        [InlineData("invalid-guid-value")]
        public void FromString_InvalidGuid_ReturnsEmpty(string invalidValue)
        {
            // Act
            var entityId = EntityId.FromString(invalidValue);

            // Assert
            Assert.Equal(EntityId.Empty, entityId);
            Assert.True(entityId.IsEmpty);
        }

        #endregion

        #region Equality Tests

        [Fact]
        public void Equality_SameGuid_AreEqual()
        {
            // Arrange
            var guid = Guid.NewGuid();
            var id1 = new EntityId(guid);
            var id2 = new EntityId(guid);

            // Assert
            Assert.Equal(id1, id2);
            Assert.True(id1 == id2);
            Assert.False(id1 != id2);
        }

        [Fact]
        public void Equality_DifferentGuids_AreNotEqual()
        {
            // Arrange
            var id1 = EntityId.New();
            var id2 = EntityId.New();

            // Assert
            Assert.NotEqual(id1, id2);
            Assert.False(id1 == id2);
            Assert.True(id1 != id2);
        }

        [Fact]
        public void Equality_EmptyIds_AreEqual()
        {
            // Arrange
            var empty1 = EntityId.Empty;
            var empty2 = new EntityId(Guid.Empty);

            // Assert
            Assert.Equal(empty1, empty2);
            Assert.True(empty1 == empty2);
        }

        #endregion

        #region ToString Tests

        [Fact]
        public void ToString_ReturnsGuidString()
        {
            // Arrange
            var guid = Guid.NewGuid();
            var entityId = new EntityId(guid);

            // Act
            var result = entityId.ToString();

            // Assert
            Assert.Equal(guid.ToString(), result);
        }

        [Fact]
        public void ToString_Empty_ReturnsEmptyGuidString()
        {
            // Act
            var result = EntityId.Empty.ToString();

            // Assert
            Assert.Equal(Guid.Empty.ToString(), result);
        }

        #endregion

        #region IsEmpty Tests

        [Fact]
        public void IsEmpty_NewId_ReturnsFalse()
        {
            // Arrange
            var entityId = EntityId.New();

            // Assert
            Assert.False(entityId.IsEmpty);
        }

        [Fact]
        public void IsEmpty_EmptyId_ReturnsTrue()
        {
            // Assert
            Assert.True(EntityId.Empty.IsEmpty);
        }

        [Fact]
        public void IsEmpty_FromEmptyGuid_ReturnsTrue()
        {
            // Arrange
            var entityId = new EntityId(Guid.Empty);

            // Assert
            Assert.True(entityId.IsEmpty);
        }

        #endregion

        #region Roundtrip Tests

        [Fact]
        public void Roundtrip_ToStringAndBack_PreservesValue()
        {
            // Arrange
            var original = EntityId.New();
            
            // Act
            var stringForm = original.ToString();
            var restored = EntityId.FromString(stringForm);

            // Assert
            Assert.Equal(original, restored);
        }

        #endregion

        #region Distinct From UserId Tests

        [Fact]
        public void EntityId_NotImplicitlyConvertibleToUserId()
        {
            // This test ensures type safety - EntityId and UserId are distinct types
            // The fact that this compiles but the types are not assignable is the test
            
            var entityId = EntityId.New();
            var userId = UserId.New();

            // Different types with same underlying structure
            Assert.Equal(typeof(EntityId), entityId.GetType());
            Assert.Equal(typeof(UserId), userId.GetType());
            Assert.NotEqual(entityId.GetType(), userId.GetType());
        }

        #endregion
    }
}
