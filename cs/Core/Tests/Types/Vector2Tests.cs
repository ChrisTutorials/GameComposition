using System;
using Xunit;
using BarkMoon.GameComposition.Core.Types;

namespace BarkMoon.GameComposition.Core.Tests.Types
{
    public class Vector2Tests
    {
        [Fact]
        public void Vector2_DefaultConstructor_CreatesZeroVector()
        {
            // Act
            var vector = new Vector2();

            // Assert
            Assert.Equal(0.0f, vector.X);
            Assert.Equal(0.0f, vector.Y);
        }

        [Fact]
        public void Vector2_ParameterizedConstructor_SetsCorrectValues()
        {
            // Arrange
            float x = 3.14f;
            float y = 2.71f;

            // Act
            var vector = new Vector2(x, y);

            // Assert
            Assert.Equal(x, vector.X);
            Assert.Equal(y, vector.Y);
        }

        [Theory]
        [InlineData(0.0f, 0.0f, 0.0f)]
        [InlineData(3.0f, 4.0f, 5.0f)]
        [InlineData(-3.0f, -4.0f, 5.0f)]
        [InlineData(1.0f, 0.0f, 1.0f)]
        [InlineData(0.0f, 1.0f, 1.0f)]
        public void Vector2_Length_ReturnsCorrectValue(float x, float y, float expected)
        {
            // Arrange
            var vector = new Vector2(x, y);

            // Act
            var length = vector.Length();

            // Assert
            Assert.Equal(expected, length, 0.001f);
        }

        [Theory]
        [InlineData(3.0f, 4.0f, 25.0f)]
        [InlineData(-3.0f, -4.0f, 25.0f)]
        [InlineData(0.0f, 0.0f, 0.0f)]
        [InlineData(1.0f, 1.0f, 2.0f)]
        public void Vector2_LengthSquared_ReturnsCorrectValue(float x, float y, float expected)
        {
            // Arrange
            var vector = new Vector2(x, y);

            // Act
            var lengthSquared = vector.LengthSquared();

            // Assert
            Assert.Equal(expected, lengthSquared, 0.001f);
        }

        [Theory]
        [InlineData(3.0f, 4.0f, 0.6f, 0.8f)]
        [InlineData(-3.0f, 4.0f, -0.6f, 0.8f)]
        [InlineData(0.0f, 0.0f, 0.0f, 0.0f)]
        [InlineData(1.0f, 0.0f, 1.0f, 0.0f)]
        public void Vector2_Normalized_ReturnsCorrectValue(float x, float y, float expectedX, float expectedY)
        {
            // Arrange
            var vector = new Vector2(x, y);

            // Act
            var normalized = vector.Normalized();

            // Assert
            Assert.Equal(expectedX, normalized.X, 0.001f);
            Assert.Equal(expectedY, normalized.Y, 0.001f);
        }

        [Theory]
        [InlineData(1.0f, 2.0f, 3.0f, 4.0f, 4.0f, 6.0f)]
        [InlineData(-1.0f, -2.0f, 3.0f, 4.0f, 2.0f, 2.0f)]
        [InlineData(0.0f, 0.0f, 5.0f, 7.0f, 5.0f, 7.0f)]
        public void Vector2_Addition_ReturnsCorrectResult(float x1, float y1, float x2, float y2, float expectedX, float expectedY)
        {
            // Arrange
            var a = new Vector2(x1, y1);
            var b = new Vector2(x2, y2);

            // Act
            var result = a + b;

            // Assert
            Assert.Equal(expectedX, result.X, 0.001f);
            Assert.Equal(expectedY, result.Y, 0.001f);
        }

        [Theory]
        [InlineData(3.0f, 4.0f, 1.0f, 2.0f, 2.0f, 2.0f)]
        [InlineData(-1.0f, -2.0f, 3.0f, 4.0f, -4.0f, -6.0f)]
        [InlineData(5.0f, 7.0f, 0.0f, 0.0f, 5.0f, 7.0f)]
        public void Vector2_Subtraction_ReturnsCorrectResult(float x1, float y1, float x2, float y2, float expectedX, float expectedY)
        {
            // Arrange
            var a = new Vector2(x1, y1);
            var b = new Vector2(x2, y2);

            // Act
            var result = a - b;

            // Assert
            Assert.Equal(expectedX, result.X, 0.001f);
            Assert.Equal(expectedY, result.Y, 0.001f);
        }

        [Theory]
        [InlineData(2.0f, 3.0f, 2.0f, 4.0f, 6.0f)]
        [InlineData(-2.0f, 3.0f, -2.0f, 4.0f, -6.0f)]
        [InlineData(0.0f, 5.0f, 3.0f, 0.0f, 15.0f)]
        public void Vector2_ScalarMultiplication_ReturnsCorrectResult(float x, float y, float scalar, float expectedX, float expectedY)
        {
            // Arrange
            var vector = new Vector2(x, y);

            // Act
            var result = vector * scalar;

            // Assert
            Assert.Equal(expectedX, result.X, 0.001f);
            Assert.Equal(expectedY, result.Y, 0.001f);
        }

        [Theory]
        [InlineData(1.0f, 2.0f, 3.0f, 4.0f, 2.828f)]
        [InlineData(-1.0f, 2.0f, 3.0f, -4.0f, 7.211f)]
        [InlineData(0.0f, 0.0f, 5.0f, 7.0f, 8.602f)]
        public void Vector2_DistanceTo_ReturnsCorrectResult(float x1, float y1, float x2, float y2, float expected)
        {
            // Arrange
            var a = new Vector2(x1, y1);
            var b = new Vector2(x2, y2);

            // Act
            var distance = a.DistanceTo(b);

            // Assert
            Assert.Equal(expected, distance, 0.001f);
        }

        [Theory]
        [InlineData(1.0f, 2.0f, 3.0f, 4.0f, 8.0f)]
        [InlineData(-1.0f, -2.0f, 3.0f, 4.0f, 52.0f)]
        [InlineData(3.0f, 4.0f, 1.0f, 2.0f, 8.0f)]
        public void Vector2_DistanceSquaredTo_ReturnsCorrectResult(float x1, float y1, float x2, float y2, float expected)
        {
            // Arrange
            var a = new Vector2(x1, y1);
            var b = new Vector2(x2, y2);

            // Act
            var distanceSquared = a.DistanceSquaredTo(b);

            // Assert
            Assert.Equal(expected, distanceSquared, 0.001f);
        }

        [Theory]
        [InlineData(1.0f, 2.0f, 1.0f, 2.0f, true)]
        [InlineData(1.0f, 2.0f, 1.0f, 3.0f, false)]
        [InlineData(-1.0f, -2.0f, -1.0f, -2.0f, true)]
        [InlineData(0.0f, 0.0f, 0.0f, 0.0f, true)]
        public void Vector2_Equality_ReturnsCorrectResult(float x1, float y1, float x2, float y2, bool expected)
        {
            // Arrange
            var a = new Vector2(x1, y1);
            var b = new Vector2(x2, y2);

            // Act
            var areEqual = a == b;

            // Assert
            Assert.Equal(expected, areEqual);
        }

        [Theory]
        [InlineData(1.0f, 2.0f, 1.0f, 2.0f, false)]
        [InlineData(1.0f, 2.0f, 1.0f, 3.0f, true)]
        [InlineData(-1.0f, -2.0f, -1.0f, -2.0f, false)]
        [InlineData(0.0f, 0.0f, 0.0f, 0.0f, false)]
        public void Vector2_Inequality_ReturnsCorrectResult(float x1, float y1, float x2, float y2, bool expected)
        {
            // Arrange
            var a = new Vector2(x1, y1);
            var b = new Vector2(x2, y2);

            // Act
            var areNotEqual = a != b;

            // Assert
            Assert.Equal(expected, areNotEqual);
        }

        [Fact]
        public void Vector2_ToString_ReturnsCorrectFormat()
        {
            // Arrange
            var vector = new Vector2(1.23f, 4.56f);

            // Act
            var result = vector.ToString();

            // Assert
            Assert.Contains("1.23", result);
            Assert.Contains("4.56", result);
        }

        [Fact]
        public void Vector2_ToVector2I_ReturnsCorrectValues()
        {
            // Arrange
            var vector = new Vector2(1.7f, 2.3f);

            // Act
            var vector2I = vector.ToVector2I();

            // Assert
            Assert.Equal(2, vector2I.X);
            Assert.Equal(2, vector2I.Y);
        }

        [Fact]
        public void Vector2_Zero_ReturnsZeroVector()
        {
            // Act
            var zero = Vector2.Zero;

            // Assert
            Assert.Equal(0.0f, zero.X);
            Assert.Equal(0.0f, zero.Y);
        }

        [Fact]
        public void Vector2_One_ReturnsOneVector()
        {
            // Act
            var one = Vector2.One;

            // Assert
            Assert.Equal(1.0f, one.X);
            Assert.Equal(1.0f, one.Y);
        }

        [Fact]
        public void Vector2_Up_ReturnsUpVector()
        {
            // Act
            var up = Vector2.Up;

            // Assert
            Assert.Equal(0.0f, up.X);
            Assert.Equal(-1.0f, up.Y);
        }

        [Fact]
        public void Vector2_Down_ReturnsDownVector()
        {
            // Act
            var down = Vector2.Down;

            // Assert
            Assert.Equal(0.0f, down.X);
            Assert.Equal(1.0f, down.Y);
        }

        [Fact]
        public void Vector2_Left_ReturnsLeftVector()
        {
            // Act
            var left = Vector2.Left;

            // Assert
            Assert.Equal(-1.0f, left.X);
            Assert.Equal(0.0f, left.Y);
        }

        [Fact]
        public void Vector2_Right_ReturnsRightVector()
        {
            // Act
            var right = Vector2.Right;

            // Assert
            Assert.Equal(1.0f, right.X);
            Assert.Equal(0.0f, right.Y);
        }
    }
}
