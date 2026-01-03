using System;
using System.Threading;

namespace BarkMoon.GameComposition.Core.Types
{
    /// <summary>
    /// High-performance numeric identifier for internal database operations.
    /// 
    /// This provides type-safe integer IDs optimized for database performance
    /// and memory efficiency. Use for internal primary keys and high-performance
    /// scenarios where string IDs would cause performance issues.
    /// 
    /// Usage examples:
    /// - public readonly record struct ShopId(long Value) : NumericId<ShopId>;
    /// - public readonly record struct OwnerId(long Value) : NumericId<OwnerId>;
    /// - public readonly record struct RecipeId(long Value) : NumericId<RecipeId>;
    /// </summary>
    /// <remarks>
    /// Performance characteristics:
    /// - Memory: 8 bytes vs ~36 bytes for string GUIDs
    /// - Comparison: ~5ns vs ~50ns for string comparison
    /// - Database: Native integer primary keys with optimal indexing
    /// - GC: Zero allocations for value types
    /// 
    /// Static factory methods are intentionally on the generic type for discoverability
    /// and type safety, following established .NET patterns like Guid.NewGuid().
    /// </remarks>
    #pragma warning disable CA1000 // Static members on generic types are intentional for factory patterns
    public readonly record struct NumericId<T>(long Value)
    {
        private static long _counter = 1;
        
        /// <summary>
        /// Empty/unknown ID (zero value).
        /// </summary>
        public static readonly NumericId<T> Empty = new(0);
        
        /// <summary>
        /// The numeric value of this ID.
        /// </summary>
        public long Value { get; } = Value;
        
        /// <summary>
        /// Determines if this ID is empty (zero).
        /// </summary>
        public bool IsEmpty => Value == 0;
        
        /// <summary>
        /// Determines if this ID has a value (non-zero).
        /// </summary>
        public bool HasValue => Value != 0;
        
        /// <summary>
        /// Creates a new numeric ID with auto-incremented value.
        /// Thread-safe for concurrent ID generation.
        /// </summary>
        /// <returns>A new numeric ID with a unique positive value.</returns>
        public static NumericId<T> New() => new(Interlocked.Increment(ref _counter));
        
        /// <summary>
        /// Parses a string into a numeric ID.
        /// </summary>
        /// <param name="value">The string value to parse.</param>
        /// <returns>A numeric ID instance.</returns>
        /// <exception cref="FormatException">Thrown when the string is not a valid long.</exception>
        public static NumericId<T> Parse(string value)
        {
            if (string.IsNullOrEmpty(value))
                return Empty;
            return new NumericId<T>(long.Parse(value, System.Globalization.CultureInfo.InvariantCulture));
        }
        
        /// <summary>
        /// Attempts to parse a string into a numeric ID.
        /// </summary>
        /// <param name="value">The string value to parse.</param>
        /// <param name="result">When this method returns, contains the parsed numeric ID if successful.</param>
        /// <returns>True if parsing succeeded; otherwise false.</returns>
        public static bool TryParse(string value, out NumericId<T> result)
        {
            if (string.IsNullOrEmpty(value))
            {
                result = Empty;
                return true;
            }
            
            if (long.TryParse(value, out var parsed))
            {
                result = new NumericId<T>(parsed);
                return true;
            }
            
            result = Empty;
            return false;
        }
        
        /// <summary>
        /// Implicit conversion from long to NumericId.
        /// </summary>
        /// <param name="value">The long value.</param>
        public static implicit operator NumericId<T>(long value) => new(value);
        
        /// <summary>
        /// Implicit conversion from NumericId to long.
        /// </summary>
        /// <param name="id">The numeric ID.</param>
        public static implicit operator long(NumericId<T> id) => id.Value;
        
        /// <summary>
        /// Compares this numeric ID with another for equality.
        /// </summary>
        /// <param name="other">The other numeric ID to compare with.</param>
        /// <returns>True if the IDs have the same value; otherwise false.</returns>
        public bool Equals(NumericId<T> other) => Value == other.Value;
        
        /// <summary>
        /// Compares if left numeric ID is less than right.
        /// </summary>
        /// <param name="left">The first numeric ID.</param>
        /// <param name="right">The second numeric ID.</param>
        /// <returns>True if left is less than right; otherwise false.</returns>
        public static bool operator <(NumericId<T> left, NumericId<T> right) => left.Value < right.Value;
        
        /// <summary>
        /// Compares if left numeric ID is greater than right.
        /// </summary>
        /// <param name="left">The first numeric ID.</param>
        /// <param name="right">The second numeric ID.</param>
        /// <returns>True if left is greater than right; otherwise false.</returns>
        public static bool operator >(NumericId<T> left, NumericId<T> right) => left.Value > right.Value;
        
        /// <summary>
        /// Compares if left numeric ID is less than or equal to right.
        /// </summary>
        /// <param name="left">The first numeric ID.</param>
        /// <param name="right">The second numeric ID.</param>
        /// <returns>True if left is less than or equal to right; otherwise false.</returns>
        public static bool operator <=(NumericId<T> left, NumericId<T> right) => left.Value <= right.Value;
        
        /// <summary>
        /// Compares if left numeric ID is greater than or equal to right.
        /// </summary>
        /// <param name="left">The first numeric ID.</param>
        /// <param name="right">The second numeric ID.</param>
        /// <returns>True if left is greater than or equal to right; otherwise false.</returns>
        public static bool operator >=(NumericId<T> left, NumericId<T> right) => left.Value >= right.Value;
        
        /// <summary>
        /// Returns the string representation of this numeric ID.
        /// </summary>
        /// <returns>The numeric value as a string.</returns>
        public override string ToString() => Value.ToString(System.Globalization.CultureInfo.InvariantCulture);
    }
    #pragma warning restore CA1000
}
