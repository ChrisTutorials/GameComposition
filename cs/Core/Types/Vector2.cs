using System;

namespace BarkMoon.GameComposition.Core.Types
{
    /// <summary>
    /// A 2D vector using floating-point coordinates (X, Y).
    /// Engine-agnostic implementation for 2D spatial calculations and grid positioning.
    /// </summary>
    /// <remarks>
    /// This is the foundational 2D vector type used across the framework for positioning,
    /// movement, and spatial calculations. Provides epsilon-based comparison for reliable
    /// floating-point operations across different platforms.
    /// </remarks>
    public struct Vector2 : IEquatable<Vector2>
    {
        /// <summary>The X coordinate component.</summary>
        public float X { get; set; }
        /// <summary>The Y coordinate component.</summary>
        public float Y { get; set; }

        /// <summary>Vector representing the origin (0, 0).</summary>
        public static readonly Vector2 Zero = new(0, 0);
        /// <summary>Vector representing unit scale (1, 1).</summary>
        public static readonly Vector2 One = new(1, 1);

        /// <summary>Vector pointing up (0, -1) in screen coordinates.</summary>
        public static readonly Vector2 Up = new(0, -1);
        /// <summary>Vector pointing down (0, 1) in screen coordinates.</summary>
        public static readonly Vector2 Down = new(0, 1);
        /// <summary>Vector pointing left (-1, 0) in screen coordinates.</summary>
        public static readonly Vector2 Left = new(-1, 0);
        /// <summary>Vector pointing right (1, 0) in screen coordinates.</summary>
        public static readonly Vector2 Right = new(1, 0);

        /// <summary>
        /// Creates a new Vector2 with the specified coordinates.
        /// </summary>
        /// <param name="x">The X coordinate.</param>
        /// <param name="y">The Y coordinate.</param>
        public Vector2(float x, float y) { X = x; Y = y; }

        /// <summary>
        /// Determines whether this Vector2 is equal to the specified object using floating-point epsilon comparison.
        /// </summary>
        /// <param name="obj">The object to compare with</param>
        /// <returns>true if the objects are equal; otherwise, false</returns>
        public override bool Equals(object? obj) => obj is Vector2 other && Equals(other);
        
        /// <summary>
        /// Determines whether this Vector2 is equal to the specified Vector2 using floating-point epsilon comparison.
        /// </summary>
        /// <param name="other">The Vector2 to compare with</param>
        /// <returns>true if the vectors are equal within epsilon; otherwise, false</returns>
        public bool Equals(Vector2 other) => Math.Abs(X - other.X) < float.Epsilon && Math.Abs(Y - other.Y) < float.Epsilon;
        /// <summary>
        /// Returns the hash code for this Vector2 instance.
        /// </summary>
        /// <returns>A hash code based on the X and Y components.</returns>
        public override int GetHashCode() => HashCode.Combine(X, Y);
        /// <summary>
        /// Determines if two Vector2 instances are equal using floating-point epsilon comparison.
        /// </summary>
        /// <param name="left">The first Vector2 instance.</param>
        /// <param name="right">The second Vector2 instance.</param>
        /// <returns>True if the vectors are approximately equal; otherwise false.</returns>
        public static bool operator ==(Vector2 left, Vector2 right) => left.Equals(right);
        /// <summary>
        /// Determines if two Vector2 instances are not equal using floating-point epsilon comparison.
        /// </summary>
        /// <param name="left">The first Vector2 instance.</param>
        /// <param name="right">The second Vector2 instance.</param>
        /// <returns>True if the vectors are not approximately equal; otherwise false.</returns>
        public static bool operator !=(Vector2 left, Vector2 right) => !left.Equals(right);
        /// <summary>
        /// Adds two Vector2 instances component-wise.
        /// </summary>
        /// <param name="a">The first vector.</param>
        /// <param name="b">The second vector.</param>
        /// <returns>A new Vector2 representing the sum.</returns>
        public static Vector2 operator +(Vector2 a, Vector2 b) => new(a.X + b.X, a.Y + b.Y);
        /// <summary>
        /// Subtracts two Vector2 instances component-wise.
        /// </summary>
        /// <param name="a">The first vector (minuend).</param>
        /// <param name="b">The second vector (subtrahend).</param>
        /// <returns>A new Vector2 representing the difference (a - b).</returns>
        public static Vector2 operator -(Vector2 a, Vector2 b) => new(a.X - b.X, a.Y - b.Y);
        /// <summary>
        /// Multiplies a Vector2 instance by a scalar value.
        /// </summary>
        /// <param name="a">The vector to scale.</param>
        /// <param name="scalar">The scalar multiplier.</param>
        /// <returns>A new Vector2 representing the scaled vector.</returns>
        public static Vector2 operator *(Vector2 a, float scalar) => new(a.X * scalar, a.Y * scalar);
        /// <summary>
        /// Returns a string representation of the Vector2 with formatted coordinates.
        /// </summary>
        /// <returns>A string in the format "(X.XX, Y.YY)" with two decimal places.</returns>
        public override string ToString() => $"({X:F2}, {Y:F2})";
        /// <summary>
        /// Converts this Vector2 to a Vector2I by rounding the coordinates to integers.
        /// </summary>
        /// <returns>A new Vector2I with rounded coordinates.</returns>
        /// <remarks>
        /// Used for converting floating-point positions to grid coordinates.
        /// </remarks>
        public Vector2I ToVector2I() => new((int)Math.Round(X), (int)Math.Round(Y));

        /// <summary>
        /// Calculates the magnitude (length) of this Vector2.
        /// </summary>
        /// <returns>The Euclidean length of the vector.</returns>
        /// <remarks>
        /// Used for distance calculations and magnitude comparisons.
        /// </remarks>
        public float Length() => (float)Math.Sqrt(X * X + Y * Y);
        /// <summary>
        /// Calculates the squared magnitude of this Vector2.
        /// </summary>
        /// <returns>The squared length of the vector.</returns>
        /// <remarks>
        /// More efficient than Length() for comparisons where actual distance isn't needed.
        /// </remarks>
        public float LengthSquared() => X * X + Y * Y;

        /// <summary>
        /// Calculates the distance to another Vector2.
        /// </summary>
        /// <param name="other">The other Vector2.</param>
        /// <returns>The Euclidean distance between the vectors.</returns>
        public float DistanceTo(Vector2 other) => (this - other).Length();
        /// <summary>
        /// Calculates the squared distance to another Vector2.
        /// </summary>
        /// <param name="other">The other Vector2.</param>
        /// <returns>The squared distance between the vectors.</returns>
        /// <remarks>
        /// More efficient than DistanceTo() for comparisons where actual distance isn't needed.
        /// </remarks>
        public float DistanceSquaredTo(Vector2 other) => (this - other).LengthSquared();

        /// <summary>
        /// Returns a normalized version of this Vector2 (unit length).
        /// </summary>
        /// <returns>A new Vector2 with length 1.0, or Zero if this vector has zero length.</returns>
        /// <remarks>
        /// Used for direction calculations and vector normalization.
        /// </remarks>
        public Vector2 Normalized()
        {
            float len = Length();
            return len > 0 ? this * (1.0f / len) : Zero;
        }
    }
}
