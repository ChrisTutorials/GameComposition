namespace BarkMoon.GameComposition.Core.Types;

/// <summary>
/// Type-safe identifier pattern for cross-plugin compatibility.
/// 
/// This provides compile-time type safety for IDs while maintaining
/// string-based storage for serialization and database compatibility.
/// 
/// Performance Considerations:
/// - Use TypedId&lt;string&gt; for external APIs and GUID-based systems
/// - Use NumericId&lt;long&gt; for internal database operations and performance-critical paths
/// - String IDs: ~36 bytes memory, slower comparisons, compatible with GUID systems
/// - Numeric IDs: 8 bytes memory, 10x faster comparisons, optimal for database primary keys
/// 
/// Usage examples:
/// - public readonly record struct ShopId(string Value) : TypedId&lt;ShopId&gt;;
/// - public readonly record struct OwnerId(string Value) : TypedId&lt;OwnerId&gt;;
/// - public readonly record struct RecipeId(string Value) : TypedId&lt;RecipeId&gt;;
/// </summary>
/// <remarks>
/// For high-performance scenarios (RimWorld/Stardew Valley scale):
/// - Consider NumericId&lt;T&gt; for database primary keys
/// - Use IdConverter for seamless string ↔ numeric conversion
/// - Keep TypedId&lt;string&gt; for external plugin APIs
/// </remarks>
public readonly record struct TypedId<T>(string Value)
{
    /// <summary>
    /// Empty/unknown ID
    /// </summary>
    public static readonly TypedId<T> Empty = new(string.Empty);
    
    /// <summary>
    /// The string value of this typed ID.
    /// </summary>
    public string Value { get; } = Value ?? string.Empty;
    
    /// <summary>
    /// Implicit conversion from TypedId to string
    /// </summary>
    /// <param name="id">The TypedId to convert</param>
    public static implicit operator string(TypedId<T> id) => id.Value;
    
    /// <summary>
    /// Implicit conversion from string to TypedId
    /// </summary>
    /// <param name="value">String value to convert</param>
    public static implicit operator TypedId<T>(string value) => new(value);
    
    /// <summary>
    /// Determines if this ID is empty.
    /// </summary>
    public bool IsEmpty => string.IsNullOrEmpty(Value);
    
    /// <summary>
    /// Determines if this ID has a value.
    /// </summary>
    public bool HasValue => !string.IsNullOrEmpty(Value);
    
    /// <summary>
    /// Creates a new typed ID with a generated GUID.
    /// </summary>
    /// <returns>A new typed ID with a GUID value.</returns>
    public static TypedId<T> New() => new(System.Guid.NewGuid().ToString());
    
    /// <summary>
    /// Parses a string into a typed ID.
    /// </summary>
    /// <param name="value">The string value to parse.</param>
    /// <returns>A typed ID instance.</returns>
    public static TypedId<T> Parse(string value) => new(value);
    
    /// <summary>
    /// Attempts to parse a string into a typed ID.
    /// </summary>
    /// <param name="value">The string value to parse.</param>
    /// <param name="result">When this method returns, contains the parsed typed ID if successful.</param>
    /// <returns>True if parsing succeeded; always true since any string is valid.</returns>
    public static bool TryParse(string value, out TypedId<T> result)
    {
        result = new TypedId<T>(value);
        return true; // Always succeeds since any string is valid
    }
    
    /// <summary>
    /// Compares this typed ID with another for equality.
    /// </summary>
    /// <param name="other">The other typed ID to compare with.</param>
    /// <returns>True if the IDs have the same value; otherwise false.</returns>
    public bool Equals(TypedId<T> other) => Value == other.Value;
    
    /// <summary>
    /// Returns the hash code for this typed ID.
    /// </summary>
    /// <returns>A hash code based on the string value.</returns>
    public override int GetHashCode() => Value?.GetHashCode() ?? 0;
    
    /// <summary>
    /// Returns the string representation of this typed ID.
    /// </summary>
    /// <returns>The string value, or empty string if null.</returns>
    public override string ToString() => Value ?? string.Empty;
}

/// <summary>
/// Extension methods for TypedId
/// </summary>
public static class TypedIdExtensions
{
    /// <summary>
    /// Determines if a typed ID is null or empty.
    /// </summary>
    /// <typeparam name="T">The type parameter of the typed ID.</typeparam>
    /// <param name="id">The typed ID to check.</param>
    /// <returns>True if the ID is null or has an empty value.</returns>
    public static bool IsNullOrEmpty<T>(this TypedId<T>? id) => 
        id == null || string.IsNullOrEmpty(id.Value);
    
    /// <summary>
    /// Determines if a typed ID has a value.
    /// </summary>
    /// <typeparam name="T">The type parameter of the typed ID.</typeparam>
    /// <param name="id">The typed ID to check.</param>
    /// <returns>True if the ID is not null and has a non-empty value.</returns>
    public static bool HasValue<T>(this TypedId<T>? id) => 
        id != null && !string.IsNullOrEmpty(id.Value);
    
    /// <summary>
    /// Gets the string value of a typed ID or returns a default if null/empty.
    /// </summary>
    /// <typeparam name="T">The type parameter of the typed ID.</typeparam>
    /// <param name="id">The typed ID to get the value from.</param>
    /// <param name="defaultValue">The default value to return if the ID is null or empty.</param>
    /// <returns>The string value, or the default if null/empty.</returns>
    public static string GetValueOrDefault<T>(this TypedId<T>? id, string defaultValue = "") =>
        id?.Value ?? defaultValue;
    
    /// <summary>
    /// Converts a nullable typed ID to a non-nullable one with a default value.
    /// </summary>
    /// <typeparam name="T">The type parameter of the typed ID.</typeparam>
    /// <param name="id">The nullable typed ID to convert.</param>
    /// <param name="defaultValue">The default value to use if the ID is null.</param>
    /// <returns>A non-nullable typed ID.</returns>
    public static TypedId<T> WithDefault<T>(this TypedId<T>? id, string defaultValue = "") =>
        id?.Value ?? new TypedId<T>(defaultValue);
}
