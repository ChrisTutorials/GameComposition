using BarkMoon.GameComposition.Core.Types;

namespace BarkMoon.GameComposition.Core.Utils
{
    /// <summary>
    /// Utility methods for direction and rotation.
    /// </summary>
    public static class DirectionUtils
    {
        /// <summary>
        /// Gets the grid delta (offset) for a given compass direction.
        /// </summary>
        public static Vector2I GetDirectionTileDelta(CompassDirection direction)
        {
            return direction switch
            {
                CompassDirection.North => new Vector2I(0, -1),
                CompassDirection.NorthEast => new Vector2I(1, -1),
                CompassDirection.East => new Vector2I(1, 0),
                CompassDirection.SouthEast => new Vector2I(1, 1),
                CompassDirection.South => new Vector2I(0, 1),
                CompassDirection.SouthWest => new Vector2I(-1, 1),
                CompassDirection.West => new Vector2I(-1, 0),
                CompassDirection.NorthWest => new Vector2I(-1, -1),
                _ => Vector2I.Zero
            };
        }
    }
}
