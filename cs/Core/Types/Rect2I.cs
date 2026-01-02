using System;

namespace BarkMoon.GameComposition.Core.Types
{
    /// <summary>
    /// A 2D axis-aligned bounding box using integer coordinates.
    /// Engine-agnostic implementation for grid area calculations.
    /// </summary>
    public struct Rect2I : IEquatable<Rect2I>
    {
        /// <summary>Gets or sets the position (top-left corner) of the rectangle.</summary>
        public Vector2I Position;
        
        /// <summary>Gets or sets the size (width and height) of the rectangle.</summary>
        public Vector2I Size;

        /// <summary>Represents a zero-sized rectangle at the origin.</summary>
        public static readonly Rect2I Zero = new(Vector2I.Zero, Vector2I.Zero);

        /// <summary>Gets or sets the X coordinate of the rectangle's position.</summary>
        public int X { get => Position.X; set => Position.X = value; }
        
        /// <summary>Gets or sets the Y coordinate of the rectangle's position.</summary>
        public int Y { get => Position.Y; set => Position.Y = value; }
        
        /// <summary>Gets or sets the width of the rectangle.</summary>
        public int Width { get => Size.X; set => Size.X = value; }
        
        /// <summary>Gets or sets the height of the rectangle.</summary>
        public int Height { get => Size.Y; set => Size.Y = value; }

        /// <summary>Gets the X coordinate of the rectangle's right edge.</summary>
        public int EndX => Position.X + Size.X;
        
        /// <summary>Gets the Y coordinate of the rectangle's bottom edge.</summary>
        public int EndY => Position.Y + Size.Y;
        
        /// <summary>Gets the position of the rectangle's bottom-right corner.</summary>
        public Vector2I End => new(Position.X + Size.X, Position.Y + Size.Y);

        /// <summary>
        /// Initializes a new Rect2I with the specified position and size.
        /// </summary>
        /// <param name="position">The top-left corner position</param>
        /// <param name="size">The width and height</param>
        public Rect2I(Vector2I position, Vector2I size)
        {
            Position = position;
            Size = size;
        }

        /// <summary>
        /// Initializes a new Rect2I with the specified coordinates and dimensions.
        /// </summary>
        /// <param name="x">The X coordinate of the top-left corner</param>
        /// <param name="y">The Y coordinate of the top-left corner</param>
        /// <param name="width">The width of the rectangle</param>
        /// <param name="height">The height of the rectangle</param>
        public Rect2I(int x, int y, int width, int height)
        {
            Position = new Vector2I(x, y);
            Size = new Vector2I(width, height);
        }

        /// <summary>
        /// Determines whether the specified point is contained within this rectangle.
        /// </summary>
        /// <param name="point">The point to test</param>
        /// <returns>true if the point is contained; otherwise, false</returns>
        public bool Contains(Vector2I point)
        {
            return point.X >= Position.X && point.X < EndX && 
                   point.Y >= Position.Y && point.Y < EndY;
        }

        /// <summary>
        /// Determines whether this rectangle intersects with another rectangle.
        /// </summary>
        /// <param name="other">The other rectangle to test</param>
        /// <returns>true if the rectangles intersect; otherwise, false</returns>
        public bool Intersects(Rect2I other)
        {
            return Position.X < other.EndX && EndX > other.Position.X &&
                   Position.Y < other.EndY && EndY > other.Position.Y;
        }

        /// <summary>
        /// Determines whether this rectangle is equal to the specified object.
        /// </summary>
        /// <param name="obj">The object to compare with</param>
        /// <returns>true if the objects are equal; otherwise, false</returns>
        public override bool Equals(object? obj) => obj is Rect2I other && Equals(other);
        
        /// <summary>
        /// Determines whether this rectangle is equal to the specified Rect2I.
        /// </summary>
        /// <param name="other">The rectangle to compare with</param>
        /// <returns>true if the rectangles are equal; otherwise, false</returns>
        public bool Equals(Rect2I other) => Position == other.Position && Size == other.Size;
        
        /// <summary>
        /// Returns the hash code for this rectangle.
        /// </summary>
        /// <returns>A hash code based on position and size</returns>
        public override int GetHashCode() => HashCode.Combine(Position, Size);

        /// <summary>
        /// Determines whether two rectangles are equal.
        /// </summary>
        /// <param name="left">The left rectangle</param>
        /// <param name="right">The right rectangle</param>
        /// <returns>true if the rectangles are equal; otherwise, false</returns>
        public static bool operator ==(Rect2I left, Rect2I right) => left.Equals(right);
        
        /// <summary>
        /// Determines whether two rectangles are not equal.
        /// </summary>
        /// <param name="left">The left rectangle</param>
        /// <param name="right">The right rectangle</param>
        /// <returns>true if the rectangles are not equal; otherwise, false</returns>
        public static bool operator !=(Rect2I left, Rect2I right) => !left.Equals(right);

        /// <summary>
        /// Returns a string representation of this rectangle.
        /// </summary>
        /// <returns>A string in the format "[Pos: {Position}, Size: {Size}]"</returns>
        public override string ToString() => $"[Pos: {Position}, Size: {Size}]";
    }
}
