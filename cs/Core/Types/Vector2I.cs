using System;

namespace BarkMoon.GameComposition.Core.Types
{
    /// <summary>
    /// Represents a 2D integer vector for grid-based positioning and calculations.
    /// Provides essential vector operations for grid coordinates and tile-based positioning.
    /// </summary>
    public struct Vector2I : IEquatable<Vector2I>
    {
        /// <summary>
        /// Gets or sets the X coordinate (horizontal position).
        /// </summary>
        public int X;
        
        /// <summary>
        /// Gets or sets the Y coordinate (vertical position).
        /// </summary>
        public int Y;

        /// <summary>
        /// Represents a zero vector (0, 0).
        /// </summary>
        public static readonly Vector2I Zero = new(0, 0);
        
        /// <summary>
        /// Represents a unit vector (1, 1).
        /// </summary>
        public static readonly Vector2I One = new(1, 1);
        
        /// <summary>
        /// Represents the upward direction (0, -1).
        /// </summary>
        public static readonly Vector2I Up = new(0, -1);
        
        /// <summary>
        /// Represents the downward direction (0, 1).
        /// </summary>
        public static readonly Vector2I Down = new(0, 1);
        
        /// <summary>
        /// Represents the leftward direction (-1, 0).
        /// </summary>
        public static readonly Vector2I Left = new(-1, 0);
        
        /// <summary>
        /// Represents the rightward direction (1, 0).
        /// </summary>
        public static readonly Vector2I Right = new(1, 0);

        /// <summary>
        /// Initializes a new Vector2I with the specified coordinates.
        /// </summary>
        /// <param name="x">The X coordinate</param>
        /// <param name="y">The Y coordinate</param>
        public Vector2I(int x, int y) { X = x; Y = y; }

        /// <summary>
        /// Determines whether this vector is equal to the specified object.
        /// </summary>
        /// <param name="obj">The object to compare with</param>
        /// <returns>true if the objects are equal; otherwise, false</returns>
        public override bool Equals(object? obj) => obj is Vector2I other && Equals(other);
        
        /// <summary>
        /// Determines whether this vector is equal to the specified Vector2I.
        /// </summary>
        /// <param name="other">The vector to compare with</param>
        /// <returns>true if the vectors are equal; otherwise, false</returns>
        public bool Equals(Vector2I other) => X == other.X && Y == other.Y;
        
        /// <summary>
        /// Returns the hash code for this vector.
        /// </summary>
        /// <returns>A hash code for the current Vector2I</returns>
        public override int GetHashCode() => HashCode.Combine(X, Y);
        
        /// <summary>
        /// Determines whether two vectors are equal.
        /// </summary>
        /// <param name="left">The left vector</param>
        /// <param name="right">The right vector</param>
        /// <returns>true if the vectors are equal; otherwise, false</returns>
        public static bool operator ==(Vector2I left, Vector2I right) => left.Equals(right);
        
        /// <summary>
        /// Determines whether two vectors are not equal.
        /// </summary>
        /// <param name="left">The left vector</param>
        /// <param name="right">The right vector</param>
        /// <returns>true if the vectors are not equal; otherwise, false</returns>
        public static bool operator !=(Vector2I left, Vector2I right) => !left.Equals(right);
        
        /// <summary>
        /// Adds two vectors together.
        /// </summary>
        /// <param name="a">The first vector</param>
        /// <param name="b">The second vector</param>
        /// <returns>The sum of the two vectors</returns>
        public static Vector2I operator +(Vector2I a, Vector2I b) => new(a.X + b.X, a.Y + b.Y);
        
        /// <summary>
        /// Subtracts one vector from another.
        /// </summary>
        /// <param name="a">The first vector</param>
        /// <param name="b">The second vector</param>
        /// <returns>The difference of the two vectors</returns>
        public static Vector2I operator -(Vector2I a, Vector2I b) => new(a.X - b.X, a.Y - b.Y);
        
        /// <summary>
        /// Multiplies a vector by a scalar.
        /// </summary>
        /// <param name="a">The vector</param>
        /// <param name="scalar">The scalar value</param>
        /// <returns>The scaled vector</returns>
        public static Vector2I operator *(Vector2I a, int scalar) => new(a.X * scalar, a.Y * scalar);
        
        /// <summary>
        /// Returns a string representation of this vector.
        /// </summary>
        /// <returns>A string in the format "(X, Y)"</returns>
        public override string ToString() => $"({X}, {Y})";
        
        /// <summary>
        /// Converts this integer vector to a floating-point vector.
        /// </summary>
        /// <returns>A Vector2 with the same coordinates</returns>
        public Vector2 ToVector2() => new(X, Y);

        /// <summary>
        /// Calculates the length (magnitude) of this vector.
        /// </summary>
        /// <returns>The Euclidean length of the vector</returns>
        public float Length() => (float)Math.Sqrt(X * X + Y * Y);
        
        /// <summary>
        /// Calculates the squared length of this vector.
        /// More efficient than Length() when only relative comparisons are needed.
        /// </summary>
        /// <returns>The squared length of the vector</returns>
        public int LengthSquared() => X * X + Y * Y;

        /// <summary>
        /// Calculates the distance to another vector.
        /// </summary>
        /// <param name="other">The other vector</param>
        /// <returns>The Euclidean distance between the vectors</returns>
        public float DistanceTo(Vector2I other) => (this - other).Length();
        
        /// <summary>
        /// Calculates the squared distance to another vector.
        /// More efficient than DistanceTo() when only relative comparisons are needed.
        /// </summary>
        /// <param name="other">The other vector</param>
        /// <returns>The squared distance between the vectors</returns>
        public int DistanceSquaredTo(Vector2I other) => (this - other).LengthSquared();
    }
}
