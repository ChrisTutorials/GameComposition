using System;

namespace BarkMoon.GameComposition.Core.Utils
{
    /// <summary>
    /// Pure C# math utilities to replace engine-specific math classes (Mathf).
    /// Provides AOT-compatible math operations for Unity and Godot.
    /// </summary>
    public static class MathUtils
    {
        /// <summary>
        /// Constrains an integer value within the specified range [min, max].
        /// </summary>
        /// <param name="value">The value to constrain.</param>
        /// <param name="min">The minimum allowed value (inclusive).</param>
        /// <param name="max">The maximum allowed value (inclusive).</param>
        /// <returns>The constrained value: min if value < min, max if value > max, otherwise value.</returns>
        /// <remarks>
        /// This is a fundamental utility for value normalization across the framework.
        /// Used for UI element bounds, animation parameters, and data validation.
        /// Engine-agnostic implementation ensures consistent behavior across Unity and Godot.
        /// </remarks>
        public static int Clamp(int value, int min, int max) => value < min ? min : (value > max ? max : value);
        
        /// <summary>
        /// Constrains a floating-point value within the specified range [min, max].
        /// </summary>
        /// <param name="value">The value to constrain.</param>
        /// <param name="min">The minimum allowed value (inclusive).</param>
        /// <param name="max">The maximum allowed value (inclusive).</param>
        /// <returns>The constrained value: min if value < min, max if value > max, otherwise value.</returns>
        /// <remarks>
        /// This is a fundamental utility for floating-point normalization across the framework.
        /// Used for alpha values, color components, physics calculations, and interpolation weights.
        /// Engine-agnostic implementation ensures consistent behavior across Unity and Godot.
        /// </remarks>
        public static float Clamp(float value, float min, float max) => value < min ? min : (value > max ? max : value);
        
        /// <summary>
        /// Constrains a double-precision value within the specified range [min, max].
        /// </summary>
        /// <param name="value">The value to constrain.</param>
        /// <param name="min">The minimum allowed value (inclusive).</param>
        /// <param name="max">The maximum allowed value (inclusive).</param>
        /// <returns>The constrained value: min if value < min, max if value > max, otherwise value.</returns>
        /// <remarks>
        /// This is a fundamental utility for high-precision value normalization across the framework.
        /// Used for scientific calculations, precise timing, and financial computations.
        /// Engine-agnostic implementation ensures consistent behavior across Unity and Godot.
        /// </remarks>
        public static double Clamp(double value, double min, double max) => value < min ? min : (value > max ? max : value);

        /// <summary>
        /// Returns the greater of two integer values.
        /// </summary>
        /// <param name="a">The first integer value.</param>
        /// <param name="b">The second integer value.</param>
        /// <returns>The larger of the two values.</returns>
        /// <remarks>
        /// This is a fundamental comparison utility used throughout the framework.
        /// Used for boundary calculations, size determinations, and threshold comparisons.
        /// Engine-agnostic implementation ensures consistent behavior across Unity and Godot.
        /// </remarks>
        public static int Max(int a, int b) => a > b ? a : b;

        /// <summary>
        /// Returns the greater of two floating-point values.
        /// </summary>
        /// <param name="a">The first floating-point value.</param>
        /// <param name="b">The second floating-point value.</param>
        /// <returns>The larger of the two values.</returns>
        /// <remarks>
        /// This is a fundamental comparison utility used throughout the framework.
        /// Used for distance calculations, magnitude comparisons, and physics computations.
        /// Engine-agnostic implementation ensures consistent behavior across Unity and Godot.
        /// </remarks>
        public static float Max(float a, float b) => a > b ? a : b;

        /// <summary>
        /// Returns the greater of two double-precision values.
        /// </summary>
        /// <param name="a">The first double-precision value.</param>
        /// <param name="b">The second double-precision value.</param>
        /// <returns>The larger of the two values.</returns>
        /// <remarks>
        /// This is a fundamental comparison utility used throughout the framework.
        /// Used for high-precision calculations, scientific computations, and timing comparisons.
        /// Engine-agnostic implementation ensures consistent behavior across Unity and Godot.
        /// </remarks>
        public static double Max(double a, double b) => a > b ? a : b;

        /// <summary>
        /// Returns the smaller of two integer values.
        /// </summary>
        /// <param name="a">The first integer value.</param>
        /// <param name="b">The second integer value.</param>
        /// <returns>The smaller of the two values.</returns>
        /// <remarks>
        /// Used for boundary calculations and size constraints across the framework.
        /// </remarks>
        public static int Min(int a, int b) => a < b ? a : b;

        /// <summary>
        /// Returns the smaller of two floating-point values.
        /// </summary>
        /// <param name="a">The first floating-point value.</param>
        /// <param name="b">The second floating-point value.</param>
        /// <returns>The smaller of the two values.</returns>
        /// <remarks>
        /// Used for distance calculations and physics constraints.
        /// </remarks>
        public static float Min(float a, float b) => a < b ? a : b;

        /// <summary>
        /// Returns the smaller of two double-precision values.
        /// </summary>
        /// <param name="a">The first double-precision value.</param>
        /// <param name="b">The second double-precision value.</param>
        /// <returns>The smaller of the two values.</returns>
        /// <remarks>
        /// Used for high-precision calculations and timing constraints.
        /// </remarks>
        public static double Min(double a, double b) => a < b ? a : b;

        /// <summary>
        /// Linearly interpolates between two floating-point values.
        /// </summary>
        /// <param name="from">The start value.</param>
        /// <param name="to">The end value.</param>
        /// <param name="weight">The interpolation weight (0.0 = from, 1.0 = to).</param>
        /// <returns>The interpolated value.</returns>
        /// <remarks>
        /// Used for animations, transitions, and smooth value changes.
        /// </remarks>
        public static float Lerp(float from, float to, float weight) => from + (to - from) * weight;
        /// <summary>
        /// Linearly interpolates between two double-precision values.
        /// </summary>
        /// <param name="from">The start value.</param>
        /// <param name="to">The end value.</param>
        /// <param name="weight">The interpolation weight (0.0 = from, 1.0 = to).</param>
        /// <returns>The interpolated value.</returns>
        /// <remarks>
        /// Used for high-precision animations and scientific calculations.
        /// </remarks>
        public static double Lerp(double from, double to, double weight) => from + (to - from) * weight;

        /// <summary>
        /// Returns the absolute value of a floating-point number.
        /// </summary>
        /// <param name="value">The value to make absolute.</param>
        /// <returns>The non-negative absolute value.</returns>
        /// <remarks>
        /// Used for distance calculations and magnitude comparisons.
        /// </remarks>
        public static float Abs(float value) => value < 0 ? -value : value;
        /// <summary>
        /// Returns the absolute value of an integer.
        /// </summary>
        /// <param name="value">The value to make absolute.</param>
        /// <returns>The non-negative absolute value.</returns>
        /// <remarks>
        /// Used for index calculations and count operations.
        /// </remarks>
        public static int Abs(int value) => value < 0 ? -value : value;

        /// <summary>
        /// Returns the sign of an integer (-1, 0, or 1).
        /// </summary>
        /// <param name="value">The value to check.</param>
        /// <returns>-1 if negative, 0 if zero, 1 if positive.</returns>
        /// <remarks>
        /// Used for direction calculations and value comparisons.
        /// </remarks>
        public static int Sign(int value) => value > 0 ? 1 : (value < 0 ? -1 : 0);
        /// <summary>
        /// Returns the sign of a floating-point number (-1.0, 0.0, or 1.0).
        /// </summary>
        /// <param name="value">The value to check.</param>
        /// <returns>-1.0 if negative, 0.0 if zero, 1.0 if positive.</returns>
        /// <remarks>
        /// Used for vector normalization and physics calculations.
        /// </remarks>
        public static float Sign(float value) => value > 0 ? 1f : (value < 0 ? -1f : 0f);

        /// <summary>
        /// Rounds a floating-point value to the nearest integer.
        /// </summary>
        /// <param name="value">The value to round.</param>
        /// <returns>The rounded integer value.</returns>
        /// <remarks>
        /// Used for grid positioning and UI coordinate calculations.
        /// </remarks>
        public static int RoundToInt(float value) => (int)Math.Round(value);
        /// <summary>
        /// Rounds a floating-point value down to the nearest integer.
        /// </summary>
        /// <param name="value">The value to floor.</param>
        /// <returns>The floored integer value.</returns>
        /// <remarks>
        /// Used for array indexing and boundary calculations.
        /// </remarks>
        public static int FloorToInt(float value) => (int)Math.Floor(value);
        /// <summary>
        /// Rounds a floating-point value up to the nearest integer.
        /// </summary>
        /// <param name="value">The value to ceil.</param>
        /// <returns>The ceiled integer value.</returns>
        /// <remarks>
        /// Used for capacity calculations and size requirements.
        /// </remarks>
        public static int CeilToInt(float value) => (int)Math.Ceiling(value);

        /// <summary>
        /// Checks if two floating-point values are approximately equal.
        /// </summary>
        /// <param name="a">The first value.</param>
        /// <param name="b">The second value.</param>
        /// <param name="epsilon">The tolerance for comparison.</param>
        /// <returns>True if the values differ by less than epsilon.</returns>
        /// <remarks>
        /// Used for floating-point comparisons where precision matters.
        /// </remarks>
        public static bool Approximately(float a, float b, float epsilon = 0.00001f) => Math.Abs(a - b) < epsilon;

        /// <summary>
        /// Converts degrees to radians.
        /// </summary>
        /// <param name="degrees">The angle in degrees.</param>
        /// <returns>The angle in radians.</returns>
        /// <remarks>
        /// Used for trigonometric calculations and angle conversions.
        /// </remarks>
        public static float DegToRad(float degrees) => degrees * (float)(Math.PI / 180.0);
        /// <summary>
        /// Converts radians to degrees.
        /// </summary>
        /// <param name="radians">The angle in radians.</param>
        /// <returns>The angle in degrees.</returns>
        /// <remarks>
        /// Used for UI display and human-readable angle representations.
        /// </remarks>
        public static float RadToDeg(float radians) => radians * (float)(180.0 / Math.PI);
    }
}
