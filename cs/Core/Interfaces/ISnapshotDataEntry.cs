namespace BarkMoon.GameComposition.Core.Interfaces;

/// <summary>
/// Represents a single data entry within a snapshot.
/// </summary>
public interface ISnapshotDataEntry
{
    /// <summary>
    /// Gets the unique identifier for this data entry.
    /// </summary>
    string Id { get; }

    /// <summary>
    /// Gets the value of the data entry.
    /// </summary>
    object? Value { get; }
}
