using System;

namespace BarkMoon.GameComposition.Core.Types
{
    public struct Vector2 : IEquatable<Vector2>
    {
        public float X;
        public float Y;

        public static readonly Vector2 Zero = new(0, 0);
        public static readonly Vector2 One = new(1, 1);

        public static readonly Vector2 Up = new(0, -1);
        public static readonly Vector2 Down = new(0, 1);
        public static readonly Vector2 Left = new(-1, 0);
        public static readonly Vector2 Right = new(1, 0);

        public Vector2(float x, float y) { X = x; Y = y; }

        public override bool Equals(object? obj) => obj is Vector2 other && Equals(other);
        public bool Equals(Vector2 other) => Math.Abs(X - other.X) < float.Epsilon && Math.Abs(Y - other.Y) < float.Epsilon;
        public override int GetHashCode() => HashCode.Combine(X, Y);
        public static bool operator ==(Vector2 left, Vector2 right) => left.Equals(right);
        public static bool operator !=(Vector2 left, Vector2 right) => !left.Equals(right);
        public static Vector2 operator +(Vector2 a, Vector2 b) => new(a.X + b.X, a.Y + b.Y);
        public static Vector2 operator -(Vector2 a, Vector2 b) => new(a.X - b.X, a.Y - b.Y);
        public static Vector2 operator *(Vector2 a, float scalar) => new(a.X * scalar, a.Y * scalar);
        public override string ToString() => $"({X:F2}, {Y:F2})";
        public Vector2I ToVector2I() => new((int)Math.Round(X), (int)Math.Round(Y));

        public float Length() => (float)Math.Sqrt(X * X + Y * Y);
        public Vector2 Normalized()
        {
            float len = Length();
            return len > 0 ? this * (1.0f / len) : Zero;
        }
    }
}
