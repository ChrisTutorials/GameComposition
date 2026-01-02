using System;

namespace BarkMoon.GameComposition.Core.Types
{
    /// <summary>
    /// A 3D vector using floating-point coordinates (X, Y, Z).
    /// Engine-agnostic implementation for world-space calculations.
    /// </summary>
    public struct Vector3 : IEquatable<Vector3>
    {
        public float X;
        public float Y;
        public float Z;

        public static readonly Vector3 Zero = new Vector3(0, 0, 0);
        public static readonly Vector3 One = new Vector3(1, 1, 1);

        public Vector3(float x, float y, float z)
        {
            X = x;
            Y = y;
            Z = z;
        }

        public override bool Equals(object? obj) => obj is Vector3 other && Equals(other);
        public bool Equals(Vector3 other) => 
            Math.Abs(X - other.X) < float.Epsilon && 
            Math.Abs(Y - other.Y) < float.Epsilon && 
            Math.Abs(Z - other.Z) < float.Epsilon;
            
        public override int GetHashCode() => HashCode.Combine(X, Y, Z);

        /// <summary>
        /// Determines if two Vector3 instances are equal using floating-point epsilon comparison.
        /// </summary>
        /// <param name="left">The first Vector3 instance.</param>
        /// <param name="right">The second Vector3 instance.</param>
        /// <returns>True if the vectors are approximately equal; otherwise false.</returns>
        /// <remarks>
        /// This operator uses epsilon-based comparison to handle floating-point precision issues.
        /// Essential for reliable 3D spatial calculations across different platforms.
        /// </remarks>
        public static bool operator ==(Vector3 left, Vector3 right) => left.Equals(right);
        /// <summary>
        /// Determines if two Vector3 instances are not equal using floating-point epsilon comparison.
        /// </summary>
        /// <param name="left">The first Vector3 instance.</param>
        /// <param name="right">The second Vector3 instance.</param>
        /// <returns>True if the vectors are not approximately equal; otherwise false.</returns>
        /// <remarks>
        /// This operator uses epsilon-based comparison to handle floating-point precision issues.
        /// Essential for reliable 3D spatial calculations across different platforms.
        /// </remarks>
        public static bool operator !=(Vector3 left, Vector3 right) => !left.Equals(right);

        /// <summary>
        /// Adds two Vector3 instances component-wise.
        /// </summary>
        /// <param name="a">The first vector.</param>
        /// <param name="b">The second vector.</param>
        /// <returns>A new Vector3 representing the sum of the input vectors.</returns>
        /// <remarks>
        /// This operation is fundamental for vector arithmetic in 3D space calculations.
        /// Used for position translation, velocity addition, and force composition.
        /// </remarks>
        public static Vector3 operator +(Vector3 a, Vector3 b) => new Vector3(a.X + b.X, a.Y + b.Y, a.Z + b.Z);
        /// <summary>
        /// Subtracts two Vector3 instances component-wise.
        /// </summary>
        /// <param name="a">The first vector (minuend).</param>
        /// <param name="b">The second vector (subtrahend).</param>
        /// <returns>A new Vector3 representing the difference (a - b).</returns>
        /// <remarks>
        /// This operation is fundamental for vector arithmetic in 3D space calculations.
        /// Used for direction calculation, distance computation, and relative positioning.
        /// </remarks>
        public static Vector3 operator -(Vector3 a, Vector3 b) => new Vector3(a.X - b.X, a.Y - b.Y, a.Z - b.Z);
        /// <summary>
        /// Multiplies a Vector3 instance by a scalar value.
        /// </summary>
        /// <param name="a">The vector to scale.</param>
        /// <param name="scalar">The scalar multiplier.</param>
        /// <returns>A new Vector3 representing the scaled vector.</returns>
        /// <remarks>
        /// This operation is fundamental for vector scaling in 3D space calculations.
        /// Used for velocity scaling, force magnitude adjustment, and distance scaling.
        /// </remarks>
        public static Vector3 operator *(Vector3 a, float scalar) => new Vector3(a.X * scalar, a.Y * scalar, a.Z * scalar);
        
        /// <summary>
        /// Returns a string representation of the Vector3 with formatted coordinates.
        /// </summary>
        /// <returns>A string in the format "(X.XX, Y.YY, Z.ZZ)" with two decimal places.</returns>
        /// <remarks>
        /// Provides human-readable output for debugging and logging purposes.
        /// The fixed decimal format ensures consistent string representation across platforms.
        /// </remarks>
        public override string ToString() => $"({X:F2}, {Y:F2}, {Z:F2})";
    }
}

