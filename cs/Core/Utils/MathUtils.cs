using System;

namespace BarkMoon.GameComposition.Core.Utils
{
    /// <summary>
    /// Pure C# math utilities to replace engine-specific math classes (Mathf).
    /// Provides AOT-compatible math operations for Unity and Godot.
    /// </summary>
    public static class MathUtils
    {
        public static int Clamp(int value, int min, int max) => value < min ? min : (value > max ? max : value);
        public static float Clamp(float value, float min, float max) => value < min ? min : (value > max ? max : value);
        public static double Clamp(double value, double min, double max) => value < min ? min : (value > max ? max : value);

        public static int Max(int a, int b) => a > b ? a : b;
        public static float Max(float a, float b) => a > b ? a : b;
        public static double Max(double a, double b) => a > b ? a : b;

        public static int Min(int a, int b) => a < b ? a : b;
        public static float Min(float a, float b) => a < b ? a : b;
        public static double Min(double a, double b) => a < b ? a : b;

        public static float Lerp(float from, float to, float weight) => from + (to - from) * weight;
        public static double Lerp(double from, double to, double weight) => from + (to - from) * weight;

        public static float Abs(float value) => value < 0 ? -value : value;
        public static int Abs(int value) => value < 0 ? -value : value;

        public static int Sign(int value) => value > 0 ? 1 : (value < 0 ? -1 : 0);
        public static float Sign(float value) => value > 0 ? 1f : (value < 0 ? -1f : 0f);

        public static int RoundToInt(float value) => (int)Math.Round(value);
        public static int FloorToInt(float value) => (int)Math.Floor(value);
        public static int CeilToInt(float value) => (int)Math.Ceiling(value);

        public static bool Approximately(float a, float b, float epsilon = 0.00001f) => Math.Abs(a - b) < epsilon;

        public static float DegToRad(float degrees) => degrees * (float)(Math.PI / 180.0);
        public static float RadToDeg(float radians) => radians * (float)(180.0 / Math.PI);
    }
}
