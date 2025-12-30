using BarkMoon.GameComposition.Core.Types;

namespace BarkMoon.GameComposition.Godot.Extensions
{
    /// <summary>
    /// Shared extension methods for translating between BarkMoon.GameComposition.Core and Godot types.
    /// Used by all plugins in the ecosystem for engine-agnostic marshaling.
    /// </summary>
    public static class FrameworkTypeMappingExtensions
    {
        #region Vector2 Extensions
        
        public static global::Godot.Vector2 ToGodot(this Vector2 v) => new(v.X, v.Y);
        public static Vector2 ToCore(this global::Godot.Vector2 v) => new(v.X, v.Y);
        
        #endregion
        
        #region Vector2I Extensions
        
        public static global::Godot.Vector2I ToGodot(this Vector2I v) => new(v.X, v.Y);
        public static Vector2I ToCore(this global::Godot.Vector2I v) => new(v.X, v.Y);
        
        #endregion
        
        #region Vector3 Extensions
        
        public static global::Godot.Vector3 ToGodot(this Vector3 v) => new(v.X, v.Y, v.Z);
        public static Vector3 ToCore(this global::Godot.Vector3 v) => new(v.X, v.Y, v.Z);
        
        #endregion
        
        #region Color Extensions
        
        public static global::Godot.Color ToGodot(this Color c) => new(c.R, c.G, c.B, c.A);
        public static Color ToCore(this global::Godot.Color c) => new(c.R, c.G, c.B, c.A);
        
        #endregion
        
        #region Rect2I Extensions
        
        public static global::Godot.Rect2I ToGodot(this Rect2I r) => new(r.Position.ToGodot(), r.Size.ToGodot());
        public static Rect2I ToCore(this global::Godot.Rect2I r) => new(r.Position.ToCore(), r.Size.ToCore());
        
        #endregion
    }
}

