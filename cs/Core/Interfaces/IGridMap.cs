using System;
using BarkMoon.GameComposition.Core.Types;

namespace BarkMoon.GameComposition.Core.Interfaces
{
    /// <summary>
    /// Core interface for grid-based map representations.
    /// Focuses on grid coordinate systems and spatial operations without rendering concerns.
    /// This is a cross-plugin game-level concept for grid-based gameplay mechanics.
    /// 
    /// Key Principles:
    /// - Grid is the domain (coordinate system, placement logic)
    /// - TileMap is an implementation detail (rendering)
    /// - No rendering or engine-specific concepts
    /// </summary>
    public interface IGridMap
    {
        /// <summary>
        /// Unique identifier for this grid map instance.
        /// </summary>
        string Id { get; }

        /// <summary>
        /// Human-readable name for this grid map.
        /// </summary>
        string Name { get; }

        /// <summary>
        /// Size of the grid in coordinates (width, height).
        /// </summary>
        Vector2I Size { get; }

        /// <summary>
        /// Size of individual grid cells in world units.
        /// </summary>
        Vector2I CellSize { get; }

        /// <summary>
        /// Indicates whether the grid map is properly initialized and ready for use.
        /// </summary>
        bool IsValid { get; }

        /// <summary>
        /// Converts world coordinates to grid coordinates.
        /// </summary>
        /// <param name="worldPos">World position in units</param>
        /// <returns>Grid position in cell coordinates</returns>
        Vector2I WorldToGrid(Vector2 worldPos);

        /// <summary>
        /// Converts grid coordinates to world coordinates.
        /// </summary>
        /// <param name="gridPos">Grid position in cell coordinates</param>
        /// <returns>World position in units</returns>
        Vector2 GridToWorld(Vector2I gridPos);

        /// <summary>
        /// Checks if a grid position is valid within this grid bounds.
        /// </summary>
        /// <param name="gridPos">Grid position to validate</param>
        /// <returns>True if position is within bounds, false otherwise</returns>
        bool IsValidPosition(Vector2I gridPos);
    }
}
