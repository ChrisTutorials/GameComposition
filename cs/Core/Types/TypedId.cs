namespace BarkMoon.GameComposition.Core.Types;

/// <summary>
/// Type-safe identifier pattern for cross-plugin compatibility.
/// 
/// This provides compile-time type safety for IDs while maintaining
/// string-based storage for serialization and database compatibility.
/// 
/// Usage examples:
/// - public readonly record struct ShopId(string Value) : TypedId<ShopId>;
/// - public readonly record struct OwnerId(string Value) : TypedId<OwnerId>;
/// - public readonly record struct RecipeId(string Value) : TypedId<RecipeId>;
/// </summary>
public readonly record struct TypedId<T>(string Value)
{
    /// <summary>
    /// Empty/unknown ID
    /// </summary>
    public static readonly TypedId<T> Empty = new(string.Empty);
    
    /// <summary>
    /// Implicit conversion from string
    /// </summary>
    /// <param name="value">String value</param>
    public static implicit operator string(TypedId<T> id) => id.Value;
    
    /// <summary>
    /// Implicit conversion to string
    /// </summary>
    /// <param name="value">String value</param>
    public static implicit operator TypedId<T>(string value) => new(value);
    
    /// <summary>
    /// Checks if the ID is empty or null
    /// </summary>
    public bool IsEmpty => string.IsNullOrEmpty(Value);
    
    /// <summary>
    /// Checks if the ID has a value
    /// </summary>
    public bool HasValue => !string.IsNullOrEmpty(Value);
    
    /// <summary>
    /// Creates a new typed ID with a generated GUID
    /// </summary>
    /// <returns>New typed ID with GUID value</returns>
    public static TypedId<T> New() => new(System.Guid.NewGuid().ToString());
    
    /// <summary>
    /// Parses a string into a typed ID
    /// </summary>
    /// <param name="value">String value to parse</param>
    /// <returns>Typed ID</returns>
    public static TypedId<T> Parse(string value) => new(value);
    
    /// <summary>
    /// Tries to parse a string into a typed ID
    /// </summary>
    /// <param name="value">String value to parse</param>
    /// <param name="result">Output typed ID</param>
    /// <returns>True if parsing succeeded</returns>
    public static bool TryParse(string value, out TypedId<T> result)
    {
        result = new TypedId<T>(value);
        return true; // Always succeeds since any string is valid
    }
    
    /// <summary>
    /// Compares two typed IDs for equality
    /// </summary>
    /// <param name="other">Other typed ID</param>
    /// <returns>True if equal</returns>
    public bool Equals(TypedId<T> other) => Value == other.Value;
    
    /// <summary>
    /// Gets the hash code for this typed ID
    /// </summary>
    /// <returns>Hash code</returns>
    public override int GetHashCode() => Value?.GetHashCode() ?? 0;
    
    /// <summary>
    /// Returns the string representation of this typed ID
    /// </summary>
    /// <returns>String value</returns>
    public override string ToString() => Value ?? string.Empty;
}

/// <summary>
/// Extension methods for TypedId
/// </summary>
public static class TypedIdExtensions
{
    /// <summary>
    /// Checks if a typed ID is null or empty
    /// </summary>
    /// <typeparam name="T">Type parameter</typeparam>
    /// <param name="id">Typed ID to check</param>
    /// <returns>True if null or empty</returns>
    public static bool IsNullOrEmpty<T>(this TypedId<T>? id) => 
        id == null || string.IsNullOrEmpty(id.Value);
    
    /// <summary>
    /// Checks if a typed ID has a value
    /// </summary>
    /// <typeparam name="T">Type parameter</typeparam>
    /// <param name="id">Typed ID to check</param>
    /// <returns>True if has value</returns>
    public static bool HasValue<T>(this TypedId<T>? id) => 
        id != null && !string.IsNullOrEmpty(id.Value);
    
    /// <summary>
    /// Gets the string value or default if null/empty
    /// </summary>
    /// <typeparam name="T">Type parameter</typeparam>
    /// <param name="id">Typed ID</param>
    /// <param name="defaultValue">Default value</param>
    /// <returns>String value or default</returns>
    public static string GetValueOrDefault<T>(this TypedId<T>? id, string defaultValue = "") =>
        id?.Value ?? defaultValue;
    
    /// <summary>
    /// Converts a nullable typed ID to a non-nullable one with default
    /// </summary>
    /// <typeparam name="T">Type parameter</typeparam>
    /// <param name="id">Nullable typed ID</param>
    /// <param name="defaultValue">Default value if null</param>
    /// <returns>Non-nullable typed ID</returns>
    public static TypedId<T> WithDefault<T>(this TypedId<T>? id, string defaultValue = "") =>
        id?.Value ?? new TypedId<T>(defaultValue);
}
