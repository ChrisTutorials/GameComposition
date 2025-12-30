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

        public static bool operator ==(Vector3 left, Vector3 right) => left.Equals(right);
        public static bool operator !=(Vector3 left, Vector3 right) => !left.Equals(right);

        public static Vector3 operator +(Vector3 a, Vector3 b) => new Vector3(a.X + b.X, a.Y + b.Y, a.Z + b.Z);
        public static Vector3 operator -(Vector3 a, Vector3 b) => new Vector3(a.X - b.X, a.Y - b.Y, a.Z - b.Z);
        public static Vector3 operator *(Vector3 a, float scalar) => new Vector3(a.X * scalar, a.Y * scalar, a.Z * scalar);
        
        public override string ToString() => $"({X:F2}, {Y:F2}, {Z:F2})";
    }
}

