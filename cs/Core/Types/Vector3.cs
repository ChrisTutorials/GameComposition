using System;

namespace BarkMoon.GameComposition.Core.Types
{
    /// <summary>
    /// A 3D vector using floating-point coordinates (X, Y, Z).
    /// Engine-agnostic implementation for world-space calculations.
    /// </summary>
    public struct Vector3 : IEquatable<Vector3>
    {
        /// <summary>Gets or sets the X coordinate component.</summary>
        public float X;
        
        /// <summary>Gets or sets the Y coordinate component.</summary>
        public float Y;
        
        /// <summary>Gets or sets the Z coordinate component.</summary>
        public float Z;

        /// <summary>Vector representing the origin (0, 0, 0).</summary>
        public static readonly Vector3 Zero = new Vector3(0, 0, 0);
        
        /// <summary>Vector representing unit scale (1, 1, 1).</summary>
        public static readonly Vector3 One = new Vector3(1, 1, 1);

        /// <summary>
        /// Initializes a new Vector3 with the specified coordinates.
        /// </summary>
        /// <param name="x">The X coordinate</param>
        /// <param name="y">The Y coordinate</param>
        /// <param name="z">The Z coordinate</param>
        public Vector3(float x, float y, float z)
        {
            X = x;
            Y = y;
            Z = z;
        }

        /// <summary>
        /// Determines whether this Vector3 is equal to the specified object using floating-point epsilon comparison.
        /// </summary>
        /// <param name="obj">The object to compare with</param>
        /// <returns>true if the objects are equal; otherwise, false</returns>
        public override bool Equals(object? obj) => obj is Vector3 other && Equals(other);
        
        /// <summary>
        /// Determines whether this Vector3 is equal to the specified Vector3 using floating-point epsilon comparison.
        /// </summary>
        /// <param name="other">The Vector3 to compare with</param>
        /// <returns>true if the vectors are equal within epsilon; otherwise, false</returns>
        public bool Equals(Vector3 other) => 
            Math.Abs(X - other.X) < float.Epsilon && 
            Math.Abs(Y - other.Y) < float.Epsilon && 
            Math.Abs(Z - other.Z) < float.Epsilon;
            
        /// <summary>
        /// Returns the hash code for this Vector3 instance.
        /// </summary>
        /// <returns>A hash code based on the X, Y, and Z components</returns>
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

