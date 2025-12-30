using System;

namespace BarkMoon.GameComposition.Core.Types
{
    /// <summary>
    /// Strongly-typed identifier for a user of GridBuilding systems.
    /// Represents "who" is issuing commands in a session, independent of
    /// building ownership or engine-specific player representations.
    /// </summary>
    public readonly record struct UserId(Guid Value)
    {
        /// <summary>
        /// Represents an uninitialized or "no user" identifier.
        /// </summary>
        public static readonly UserId Empty = new UserId(Guid.Empty);

        /// <summary>
        /// Creates a new user identifier with a freshly generated GUID value.
        /// </summary>
        public static UserId New() => new UserId(Guid.NewGuid());

        /// <summary>
        /// Parses a string representation into a <see cref="UserId"/>, or returns
        /// <see cref="Empty"/> when the value is invalid.
        /// </summary>
        /// <param name="value">String form of the GUID value to parse.</param>
        /// <returns>A corresponding <see cref="UserId"/> or <see cref="Empty"/>.</returns>
        public static UserId FromString(string value)
        {
            if (string.IsNullOrWhiteSpace(value))
                return Empty;

            return Guid.TryParse(value, out var guid)
                ? new UserId(guid)
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

