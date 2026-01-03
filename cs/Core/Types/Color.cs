using System;

namespace BarkMoon.GameComposition.Core.Types
{
    /// <summary>
    /// A color using RGBA components (R, G, B, A).
    /// Engine-agnostic implementation.
    /// </summary>
    public struct Color : IEquatable<Color>
    {
        public float R { get; set; }
        public float G { get; set; }
        public float B { get; set; }
        public float A { get; set; }

        public static readonly Color Black = new Color(0, 0, 0, 1);
        public static readonly Color White = new Color(1, 1, 1, 1);
        public static readonly Color Red = new Color(1, 0, 0, 1);
        public static readonly Color Green = new Color(0, 1, 0, 1);
        public static readonly Color Blue = new Color(0, 0, 1, 1);
        public static readonly Color Transparent = new Color(0, 0, 0, 0);

        public Color(float r, float g, float b, float a = 1.0f)
        {
            R = r;
            G = g;
            B = b;
            A = a;
        }

        public override bool Equals(object obj) => obj is Color other && Equals(other);
        public bool Equals(Color other) => 
            System.Math.Abs(R - other.R) < 0.001f && 
            System.Math.Abs(G - other.G) < 0.001f && 
            System.Math.Abs(B - other.B) < 0.001f && 
            System.Math.Abs(A - other.A) < 0.001f;

        public override int GetHashCode() => HashCode.Combine(R, G, B, A);

        public static bool operator ==(Color left, Color right) => left.Equals(right);
        public static bool operator !=(Color left, Color right) => !left.Equals(right);

        public override string ToString() => $"RGBA({R:F2}, {G:F2}, {B:F2}, {A:F2})";
    }
}

