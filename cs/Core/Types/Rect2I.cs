using System;

namespace BarkMoon.GameComposition.Core.Types
{
    /// <summary>
    /// A 2D axis-aligned bounding box using integer coordinates.
    /// Engine-agnostic implementation for grid area calculations.
    /// </summary>
    public struct Rect2I : IEquatable<Rect2I>
    {
        public Vector2I Position;
        public Vector2I Size;

        public static readonly Rect2I Zero = new(Vector2I.Zero, Vector2I.Zero);

        public int X { get => Position.X; set => Position.X = value; }
        public int Y { get => Position.Y; set => Position.Y = value; }
        public int Width { get => Size.X; set => Size.X = value; }
        public int Height { get => Size.Y; set => Size.Y = value; }

        public int EndX => Position.X + Size.X;
        public int EndY => Position.Y + Size.Y;
        public Vector2I End => new(Position.X + Size.X, Position.Y + Size.Y);

        public Rect2I(Vector2I position, Vector2I size)
        {
            Position = position;
            Size = size;
        }

        public Rect2I(int x, int y, int width, int height)
        {
            Position = new Vector2I(x, y);
            Size = new Vector2I(width, height);
        }

        public bool Contains(Vector2I point)
        {
            return point.X >= Position.X && point.X < EndX && 
                   point.Y >= Position.Y && point.Y < EndY;
        }

        public bool Intersects(Rect2I other)
        {
            return Position.X < other.EndX && EndX > other.Position.X &&
                   Position.Y < other.EndY && EndY > other.Position.Y;
        }

        public override bool Equals(object? obj) => obj is Rect2I other && Equals(other);
        public bool Equals(Rect2I other) => Position == other.Position && Size == other.Size;
        public override int GetHashCode() => HashCode.Combine(Position, Size);

        public static bool operator ==(Rect2I left, Rect2I right) => left.Equals(right);
        public static bool operator !=(Rect2I left, Rect2I right) => !left.Equals(right);

        public override string ToString() => $"[Pos: {Position}, Size: {Size}]";
    }
}
