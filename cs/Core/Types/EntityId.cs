using System;

namespace BarkMoon.GameComposition.Core.Types
{
    /// <summary>
    /// Strongly-typed identifier for domain entities/objects.
    /// Represents a unique reference to game objects, placements, sessions,
    /// or any non-user domain concept that needs type-safe identification.
    /// 
    /// ## Usage
    /// Use <see cref="EntityId"/> for identifying "things" in the game world:
    /// - Placed objects on the grid
    /// - Placement sessions
    /// - Grid maps
    /// - Any domain object requiring unique identification
    /// 
    /// ## Distinction from UserId
    /// - <see cref="UserId"/>: Identifies actors (who is performing actions)
    /// - <see cref="EntityId"/>: Identifies objects (what is being acted upon)
    /// </summary>
    public readonly record struct EntityId(Guid Value)
    {
        /// <summary>
        /// Represents an uninitialized or "no entity" identifier.
        /// </summary>
        public static readonly EntityId Empty = new EntityId(Guid.Empty);

        /// <summary>
        /// Creates a new entity identifier with a freshly generated GUID value.
        /// </summary>
        public static EntityId New() => new EntityId(Guid.NewGuid());

        /// <summary>
        /// Parses a string representation into an <see cref="EntityId"/>, or returns
        /// <see cref="Empty"/> when the value is invalid.
        /// </summary>
        /// <param name="value">String form of the GUID value to parse.</param>
        /// <returns>A corresponding <see cref="EntityId"/> or <see cref="Empty"/>.</returns>
        public static EntityId FromString(string value)
        {
            if (string.IsNullOrWhiteSpace(value))
                return Empty;

            return Guid.TryParse(value, out var guid)
                ? new EntityId(guid)
                : Empty;
        }

        /// <summary>
        /// Converts the underlying GUID value to its string representation.
        /// </summary>
        /// <returns>String form of the wrapped GUID value.</returns>
        public override string ToString() => Value.ToString();

        /// <summary>
        /// Gets a value indicating whether this identifier equals <see cref="Empty"/>.
        /// </summary>
        public bool IsEmpty => Value == Guid.Empty;
    }
}
