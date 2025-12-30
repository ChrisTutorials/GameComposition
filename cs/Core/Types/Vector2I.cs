using System;

namespace BarkMoon.GameComposition.Core.Types
{
    public struct Vector2I : IEquatable<Vector2I>
    {
        public int X;
        public int Y;

        public static readonly Vector2I Zero = new(0, 0);
        public static readonly Vector2I One = new(1, 1);
        public static readonly Vector2I Up = new(0, -1);
        public static readonly Vector2I Down = new(0, 1);
        public static readonly Vector2I Left = new(-1, 0);
        public static readonly Vector2I Right = new(1, 0);

        public Vector2I(int x, int y) { X = x; Y = y; }

        public override bool Equals(object? obj) => obj is Vector2I other && Equals(other);
        public bool Equals(Vector2I other) => X == other.X && Y == other.Y;
        public override int GetHashCode() => HashCode.Combine(X, Y);
        public static bool operator ==(Vector2I left, Vector2I right) => left.Equals(right);
        public static bool operator !=(Vector2I left, Vector2I right) => !left.Equals(right);
        public static Vector2I operator +(Vector2I a, Vector2I b) => new(a.X + b.X, a.Y + b.Y);
        public static Vector2I operator -(Vector2I a, Vector2I b) => new(a.X - b.X, a.Y - b.Y);
        public static Vector2I operator *(Vector2I a, int scalar) => new(a.X * scalar, a.Y * scalar);
        public override string ToString() => $"({X}, {Y})";
        public Vector2 ToVector2() => new(X, Y);

        public float Length() => (float)Math.Sqrt(X * X + Y * Y);
        public int LengthSquared() => X * X + Y * Y;

        public float DistanceTo(Vector2I other) => (this - other).Length();
        public int DistanceSquaredTo(Vector2I other) => (this - other).LengthSquared();
    }
}
