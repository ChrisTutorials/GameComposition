using System;
using Xunit;
using BarkMoon.GameComposition.Core.Utils;

namespace BarkMoon.GameComposition.Core.Tests.Utils
{
    public class MathUtilsTests
    {
        [Theory]
        [InlineData(5, 1, 10, 5)]
        [InlineData(0, 1, 10, 1)]
        [InlineData(15, 1, 10, 10)]
        [InlineData(-5, -10, 10, -5)]
        [InlineData(-15, -10, 10, -10)]
        public void Clamp_Int_ReturnsCorrectValue(int value, int min, int max, int expected)
        {
            // Act
            var result = MathUtils.Clamp(value, min, max);

            // Assert
            Assert.Equal(expected, result);
        }

        [Theory]
        [InlineData(5.0f, 1.0f, 10.0f, 5.0f)]
        [InlineData(0.0f, 1.0f, 10.0f, 1.0f)]
        [InlineData(15.0f, 1.0f, 10.0f, 10.0f)]
        [InlineData(-5.0f, -10.0f, 10.0f, -5.0f)]
        [InlineData(-15.0f, -10.0f, 10.0f, -10.0f)]
        public void Clamp_Float_ReturnsCorrectValue(float value, float min, float max, float expected)
        {
            // Act
            var result = MathUtils.Clamp(value, min, max);

            // Assert
            Assert.Equal(expected, result);
        }

        [Theory]
        [InlineData(5.5, 1.5, 10.5, 5.5)]
        [InlineData(0.5, 1.5, 10.5, 1.5)]
        [InlineData(15.5, 1.5, 10.5, 10.5)]
        [InlineData(-5.5, -10.5, 10.5, -5.5)]
        [InlineData(-15.5, -10.5, 10.5, -10.5)]
        public void Clamp_Double_ReturnsCorrectValue(double value, double min, double max, double expected)
        {
            // Act
            var result = MathUtils.Clamp(value, min, max);

            // Assert
            Assert.Equal(expected, result);
        }

        [Theory]
        [InlineData(3, 7, 7)]
        [InlineData(7, 3, 7)]
        [InlineData(5, 5, 5)]
        [InlineData(-3, -7, -3)]
        [InlineData(0, 5, 5)]
        public void Max_Int_ReturnsCorrectValue(int a, int b, int expected)
        {
            // Act
            var result = MathUtils.Max(a, b);

            // Assert
            Assert.Equal(expected, result);
        }

        [Theory]
        [InlineData(3.0f, 7.0f, 7.0f)]
        [InlineData(7.0f, 3.0f, 7.0f)]
        [InlineData(5.0f, 5.0f, 5.0f)]
        [InlineData(-3.0f, -7.0f, -3.0f)]
        [InlineData(0.0f, 5.0f, 5.0f)]
        public void Max_Float_ReturnsCorrectValue(float a, float b, float expected)
        {
            // Act
            var result = MathUtils.Max(a, b);

            // Assert
            Assert.Equal(expected, result);
        }

        [Theory]
        [InlineData(3.0, 7.0, 7.0)]
        [InlineData(7.0, 3.0, 7.0)]
        [InlineData(5.0, 5.0, 5.0)]
        [InlineData(-3.0, -7.0, -3.0)]
        [InlineData(0.0, 5.0, 5.0)]
        public void Max_Double_ReturnsCorrectValue(double a, double b, double expected)
        {
            // Act
            var result = MathUtils.Max(a, b);

            // Assert
            Assert.Equal(expected, result);
        }

        [Theory]
        [InlineData(3, 7, 3)]
        [InlineData(7, 3, 3)]
        [InlineData(5, 5, 5)]
        [InlineData(-3, -7, -7)]
        [InlineData(0, 5, 0)]
        public void Min_Int_ReturnsCorrectValue(int a, int b, int expected)
        {
            // Act
            var result = MathUtils.Min(a, b);

            // Assert
            Assert.Equal(expected, result);
        }

        [Theory]
        [InlineData(3.0f, 7.0f, 3.0f)]
        [InlineData(7.0f, 3.0f, 3.0f)]
        [InlineData(5.0f, 5.0f, 5.0f)]
        [InlineData(-3.0f, -7.0f, -7.0f)]
        [InlineData(0.0f, 5.0f, 0.0f)]
        public void Min_Float_ReturnsCorrectValue(float a, float b, float expected)
        {
            // Act
            var result = MathUtils.Min(a, b);

            // Assert
            Assert.Equal(expected, result);
        }

        [Theory]
        [InlineData(3.0, 7.0, 3.0)]
        [InlineData(7.0, 3.0, 3.0)]
        [InlineData(5.0, 5.0, 5.0)]
        [InlineData(-3.0, -7.0, -7.0)]
        [InlineData(0.0, 5.0, 0.0)]
        public void Min_Double_ReturnsCorrectValue(double a, double b, double expected)
        {
            // Act
            var result = MathUtils.Min(a, b);

            // Assert
            Assert.Equal(expected, result);
        }

        [Theory]
        [InlineData(0.0f, 10.0f, 0.0f, 0.0f)]
        [InlineData(0.0f, 10.0f, 0.5f, 5.0f)]
        [InlineData(0.0f, 10.0f, 1.0f, 10.0f)]
        [InlineData(10.0f, 0.0f, 0.5f, 5.0f)]
        [InlineData(-5.0f, 5.0f, 0.5f, 0.0f)]
        public void Lerp_Float_ReturnsCorrectValue(float from, float to, float weight, float expected)
        {
            // Act
            var result = MathUtils.Lerp(from, to, weight);

            // Assert
            Assert.Equal(expected, result, 0.001f);
        }

        [Theory]
        [InlineData(0.0, 10.0, 0.0, 0.0)]
        [InlineData(0.0, 10.0, 0.5, 5.0)]
        [InlineData(0.0, 10.0, 1.0, 10.0)]
        [InlineData(10.0, 0.0, 0.5, 5.0)]
        [InlineData(-5.0, 5.0, 0.5, 0.0)]
        public void Lerp_Double_ReturnsCorrectValue(double from, double to, double weight, double expected)
        {
            // Act
            var result = MathUtils.Lerp(from, to, weight);

            // Assert
            Assert.Equal(expected, result, 0.001);
        }

        [Theory]
        [InlineData(5.0f, 5.0f)]
        [InlineData(-5.0f, 5.0f)]
        [InlineData(0.0f, 0.0f)]
        [InlineData(3.14f, 3.14f)]
        [InlineData(-3.14f, 3.14f)]
        public void Abs_Float_ReturnsCorrectValue(float value, float expected)
        {
            // Act
            var result = MathUtils.Abs(value);

            // Assert
            Assert.Equal(expected, result);
        }

        [Theory]
        [InlineData(5, 5)]
        [InlineData(-5, 5)]
        [InlineData(0, 0)]
        [InlineData(42, 42)]
        [InlineData(-42, 42)]
        public void Abs_Int_ReturnsCorrectValue(int value, int expected)
        {
            // Act
            var result = MathUtils.Abs(value);

            // Assert
            Assert.Equal(expected, result);
        }

        [Theory]
        [InlineData(5, 1)]
        [InlineData(-5, -1)]
        [InlineData(0, 0)]
        [InlineData(42, 1)]
        [InlineData(-42, -1)]
        public void Sign_Int_ReturnsCorrectValue(int value, int expected)
        {
            // Act
            var result = MathUtils.Sign(value);

            // Assert
            Assert.Equal(expected, result);
        }

        [Theory]
        [InlineData(5.0f, 1.0f)]
        [InlineData(-5.0f, -1.0f)]
        [InlineData(0.0f, 0.0f)]
        [InlineData(3.14f, 1.0f)]
        [InlineData(-3.14f, -1.0f)]
        public void Sign_Float_ReturnsCorrectValue(float value, float expected)
        {
            // Act
            var result = MathUtils.Sign(value);

            // Assert
            Assert.Equal(expected, result);
        }

        [Theory]
        [InlineData(3.2f, 3)]
        [InlineData(3.7f, 4)]
        [InlineData(-3.2f, -3)]
        [InlineData(-3.7f, -4)]
        [InlineData(0.5f, 0)]
        public void RoundToInt_ReturnsCorrectValue(float value, int expected)
        {
            // Act
            var result = MathUtils.RoundToInt(value);

            // Assert
            Assert.Equal(expected, result);
        }

        [Theory]
        [InlineData(3.2f, 3)]
        [InlineData(3.7f, 3)]
        [InlineData(-3.2f, -4)]
        [InlineData(-3.7f, -4)]
        [InlineData(0.5f, 0)]
        public void FloorToInt_ReturnsCorrectValue(float value, int expected)
        {
            // Act
            var result = MathUtils.FloorToInt(value);

            // Assert
            Assert.Equal(expected, result);
        }

        [Theory]
        [InlineData(3.2f, 4)]
        [InlineData(3.7f, 4)]
        [InlineData(-3.2f, -3)]
        [InlineData(-3.7f, -3)]
        [InlineData(0.5f, 1)]
        public void CeilToInt_ReturnsCorrectValue(float value, int expected)
        {
            // Act
            var result = MathUtils.CeilToInt(value);

            // Assert
            Assert.Equal(expected, result);
        }

        [Theory]
        [InlineData(1.0f, 1.0f, true)]
        [InlineData(1.0f, 1.000001f, true)]
        [InlineData(1.0f, 1.00002f, false)]
        [InlineData(-1.0f, -1.000001f, true)]
        [InlineData(0.0f, 0.0f, true)]
        public void Approximately_ReturnsCorrectResult(float a, float b, bool expected)
        {
            // Act
            var result = MathUtils.Approximately(a, b);

            // Assert
            Assert.Equal(expected, result);
        }

        [Fact]
        public void Approximately_CustomEpsilon_UsesCorrectThreshold()
        {
            // Arrange
            float a = 1.0f;
            float b = 1.05f;
            float epsilon = 0.1f;

            // Act
            var result = MathUtils.Approximately(a, b, epsilon);

            // Assert
            Assert.True(result);
        }

        [Theory]
        [InlineData(0.0f, 0.0f)]
        [InlineData(90.0f, (float)(Math.PI / 2.0))]
        [InlineData(180.0f, (float)Math.PI)]
        [InlineData(360.0f, (float)(2.0 * Math.PI))]
        [InlineData(-90.0f, (float)(-Math.PI / 2.0))]
        public void DegToRad_ReturnsCorrectValue(float degrees, float expected)
        {
            // Act
            var result = MathUtils.DegToRad(degrees);

            // Assert
            Assert.Equal(expected, result, 0.001f);
        }

        [Theory]
        [InlineData(0.0f, 0.0f)]
        [InlineData((float)(Math.PI / 2.0), 90.0f)]
        [InlineData((float)Math.PI, 180.0f)]
        [InlineData((float)(2.0 * Math.PI), 360.0f)]
        [InlineData((float)(-Math.PI / 2.0), -90.0f)]
        public void RadToDeg_ReturnsCorrectValue(float radians, float expected)
        {
            // Act
            var result = MathUtils.RadToDeg(radians);

            // Assert
            Assert.Equal(expected, result, 0.001f);
        }

        [Fact]
        public void DegToRad_RadToDeg_RoundTrip_ReturnsOriginalValue()
        {
            // Arrange
            float originalDegrees = 45.0f;

            // Act
            float radians = MathUtils.DegToRad(originalDegrees);
            float backToDegrees = MathUtils.RadToDeg(radians);

            // Assert
            Assert.Equal(originalDegrees, backToDegrees, 0.001f);
        }
    }
}
