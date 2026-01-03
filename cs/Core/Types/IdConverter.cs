using System;
using System.Security.Cryptography;
using System.Text;

namespace BarkMoon.GameComposition.Core.Types
{
    /// <summary>
    /// Conversion utilities for seamless integration between string and numeric IDs.
    /// 
    /// Provides bidirectional conversion between GUID-based string IDs and
    /// high-performance numeric IDs while maintaining type safety.
    /// </summary>
    /// <remarks>
    /// Use cases:
    /// - External APIs: Convert GUIDs to numeric for internal storage
    /// - Database migration: Map existing string keys to numeric keys
    /// - Plugin compatibility: Bridge between different ID systems
    /// </remarks>
    public static class IdConverter
    {
        /// <summary>
        /// Converts a GUID string to a numeric ID using deterministic hashing.
        /// </summary>
        /// <typeparam name="T">The type parameter for the numeric ID.</typeparam>
        /// <param name="guidString">The GUID string to convert.</param>
        /// <returns>A numeric ID representing the GUID.</returns>
        /// <exception cref="ArgumentNullException">Thrown when guidString is null.</exception>
        /// <exception cref="FormatException">Thrown when guidString is not a valid GUID format.</exception>
        public static NumericId<T> GuidToNumeric<T>(string guidString)
        {
            ArgumentNullException.ThrowIfNull(guidString);
            
            // Validate GUID format
            if (!Guid.TryParse(guidString, out _))
                throw new FormatException($"Invalid GUID format: {guidString}");
            
            // Use deterministic hash to convert GUID to 64-bit number
            var hash = SHA256.HashData(Encoding.UTF8.GetBytes(guidString));
            var bytes = new byte[8];
            Array.Copy(hash, 0, bytes, 0, 8);
            
            // Ensure positive value for database compatibility
            var value = Math.Abs(BitConverter.ToInt64(bytes, 0));
            return new NumericId<T>(value);
        }
        
        /// <summary>
        /// Converts a numeric ID back to a GUID string representation.
        /// Note: This is a one-way conversion - original GUID cannot be recovered.
        /// </summary>
        /// <typeparam name="T">The type parameter of the numeric ID.</typeparam>
        /// <param name="numericId">The numeric ID to convert.</param>
        /// <returns>A GUID string derived from the numeric ID.</returns>
        public static string NumericToGuid<T>(NumericId<T> numericId)
        {
            // Convert numeric ID back to a deterministic GUID
            var bytes = BitConverter.GetBytes(numericId.Value);
            var hash = SHA256.HashData(bytes);
            
            // Take first 16 bytes for GUID
            var guidBytes = new byte[16];
            Array.Copy(hash, 0, guidBytes, 0, 16);
            
            return new Guid(guidBytes).ToString("N");
        }
        
        /// <summary>
        /// Attempts to convert a GUID string to a numeric ID.
        /// </summary>
        /// <typeparam name="T">The type parameter for the numeric ID.</typeparam>
        /// <param name="guidString">The GUID string to convert.</param>
        /// <param name="result">When this method returns, contains the converted numeric ID if successful.</param>
        /// <returns>True if conversion succeeded; otherwise false.</returns>
        public static bool TryGuidToNumeric<T>(string guidString, out NumericId<T> result)
        {
            result = default;
            
            if (string.IsNullOrEmpty(guidString))
            {
                result = NumericId<T>.Empty;
                return true;
            }
            
            try
            {
                result = GuidToNumeric<T>(guidString);
                return true;
            }
            catch (FormatException)
            {
                return false;
            }
            catch (ArgumentNullException)
            {
                return false;
            }
        }
        
        /// <summary>
        /// Determines if a string is a valid GUID format.
        /// </summary>
        /// <param name="value">The string to validate.</param>
        /// <returns>True if the string is a valid GUID; otherwise false.</returns>
        public static bool IsGuid(string value)
        {
            return !string.IsNullOrEmpty(value) && Guid.TryParse(value, out _);
        }
        
        /// <summary>
        /// Determines if a string is a valid numeric ID format.
        /// </summary>
        /// <param name="value">The string to validate.</param>
        /// <returns>True if the string is a valid numeric ID; otherwise false.</returns>
        public static bool IsNumeric(string value)
        {
            return !string.IsNullOrEmpty(value) && long.TryParse(value, out _);
        }
        
        /// <summary>
        /// Creates a numeric ID from a database sequence value.
        /// </summary>
        /// <typeparam name="T">The type parameter for the numeric ID.</typeparam>
        /// <param name="sequenceValue">The database sequence value.</param>
        /// <returns>A numeric ID with the specified value.</returns>
        public static NumericId<T> FromSequence<T>(long sequenceValue)
        {
            return new NumericId<T>(sequenceValue);
        }
        
        /// <summary>
        /// Extracts the sequence value from a numeric ID for database storage.
        /// </summary>
        /// <typeparam name="T">The type parameter of the numeric ID.</typeparam>
        /// <param name="numericId">The numeric ID.</param>
        /// <returns>The underlying sequence value.</returns>
        public static long ToSequence<T>(NumericId<T> numericId)
        {
            return numericId.Value;
        }
    }
}
