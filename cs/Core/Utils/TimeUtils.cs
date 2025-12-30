using System;

namespace BarkMoon.GameComposition.Core.Utils
{
    /// <summary>
    /// Pure C# time utilities to replace engine-specific time classes.
    /// Provides AOT-compatible time operations for Unity and Godot.
    /// </summary>
    public static class TimeUtils
    {
        /// <summary>
        /// Gets the current Unix timestamp in seconds.
        /// Equivalent to Godot's Time.GetUnixTimeFromSystem().
        /// </summary>
        public static double GetUnixTime()
        {
            return DateTimeOffset.UtcNow.ToUnixTimeSeconds();
        }

        /// <summary>
        /// Gets the current Unix timestamp in milliseconds.
        /// </summary>
        public static long GetUnixTimeMilliseconds()
        {
            return DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
        }

        /// <summary>
        /// Gets the current Unix timestamp with fractional seconds.
        /// More precise than GetUnixTime().
        /// </summary>
        public static double GetUnixTimePrecise()
        {
            return DateTimeOffset.UtcNow.ToUnixTimeMilliseconds() / 1000.0;
        }

        /// <summary>
        /// Gets the current time as a formatted string.
        /// </summary>
        public static string GetDateTimeString(string format = "yyyy-MM-dd HH:mm:ss")
        {
            return DateTime.Now.ToString(format);
        }

        /// <summary>
        /// Gets the current UTC time as a formatted string.
        /// </summary>
        public static string GetDateTimeStringUtc(string format = "yyyy-MM-dd HH:mm:ss")
        {
            return DateTime.UtcNow.ToString(format);
        }

        /// <summary>
        /// Converts Unix timestamp to DateTime.
        /// </summary>
        public static DateTime FromUnixTime(double unixTime)
        {
            return DateTimeOffset.FromUnixTimeSeconds((long)unixTime).DateTime;
        }

        /// <summary>
        /// Gets elapsed time in seconds since a Unix timestamp.
        /// </summary>
        public static double GetElapsedSecondsSince(double startTime)
        {
            return GetUnixTimePrecise() - startTime;
        }

        /// <summary>
        /// Checks if a duration has passed since a given timestamp.
        /// </summary>
        public static bool HasDurationPassed(double startTime, double durationSeconds)
        {
            return GetElapsedSecondsSince(startTime) >= durationSeconds;
        }
    }
}
