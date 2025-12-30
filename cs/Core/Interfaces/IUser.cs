using System;
using System.Threading.Tasks;
using BarkMoon.GameComposition.Core.Types;

namespace BarkMoon.GameComposition.Core.Interfaces
{
    /// <summary>
    /// Universal user interface for all plugins in the GameComposition ecosystem.
    /// Provides the foundational user identity contract that all plugins can depend on.
    /// </summary>
    public interface IUser
    {
        /// <summary>
        /// Unique identifier for the user.
        /// </summary>
        UserId Id { get; }
        
        /// <summary>
        /// Display name of the user.
        /// </summary>
        string Name { get; }
        
        /// <summary>
        /// User profile data including preferences and metadata.
        /// </summary>
        IUserProfile Profile { get; }
        
        /// <summary>
        /// When the user was created.
        /// </summary>
        DateTime CreatedAt { get; }
        
        /// <summary>
        /// Whether the user is currently active.
        /// </summary>
        bool IsActive { get; }
    }

    /// <summary>
    /// Universal user session interface representing a user's active session.
    /// </summary>
    public interface IUserSession
    {
        /// <summary>
        /// The user associated with this session.
        /// </summary>
        IUser User { get; }
        
        /// <summary>
        /// When the session started.
        /// </summary>
        DateTime StartedAt { get; }
        
        /// <summary>
        /// Last activity timestamp for the session.
        /// </summary>
        DateTime LastActivity { get; }
        
        /// <summary>
        /// Current status of the session.
        /// </summary>
        SessionStatus Status { get; }
    }

    /// <summary>
    /// Universal user profile interface for user preferences and metadata.
    /// </summary>
    public interface IUserProfile
    {
        /// <summary>
        /// User preferences and settings.
        /// </summary>
        System.Collections.Generic.Dictionary<string, object> Preferences { get; }
        
        /// <summary>
        /// User-specific settings and configuration.
        /// </summary>
        System.Collections.Generic.Dictionary<string, object> Settings { get; }
        
        /// <summary>
        /// Additional user metadata.
        /// </summary>
        System.Collections.Generic.Dictionary<string, object> Metadata { get; }
    }

    /// <summary>
    /// Universal user manager interface for user lifecycle operations.
    /// </summary>
    public interface IUserManager
    {
        /// <summary>
        /// Creates a new user with the specified name and profile.
        /// </summary>
        Task<IUser> CreateUserAsync(string name, IUserProfile profile);
        
        /// <summary>
        /// Gets a user by their unique identifier.
        /// </summary>
        Task<IUser?> GetUserAsync(UserId userId);
        
        /// <summary>
        /// Gets all currently active users.
        /// </summary>
        Task<System.Collections.Generic.IEnumerable<IUser>> GetActiveUsersAsync();
        
        /// <summary>
        /// Deactivates a user (marks as inactive).
        /// </summary>
        Task<bool> DeactivateUserAsync(UserId userId);
    }

    /// <summary>
    /// Universal session manager interface for user session lifecycle.
    /// </summary>
    public interface ISessionManager
    {
        /// <summary>
        /// Creates a new session for the specified user.
        /// </summary>
        Task<IUserSession> CreateSessionAsync(IUser user);
        
        /// <summary>
        /// Gets the active session for a user.
        /// </summary>
        Task<IUserSession?> GetSessionAsync(UserId userId);
        
        /// <summary>
        /// Ends a user's session.
        /// </summary>
        Task<bool> EndSessionAsync(UserId userId);
        
        /// <summary>
        /// Gets all currently active sessions.
        /// </summary>
        Task<System.Collections.Generic.IEnumerable<IUserSession>> GetActiveSessionsAsync();
    }

    /// <summary>
    /// Session status enumeration.
    /// </summary>
    public enum SessionStatus
    {
        /// <summary>
        /// Session is active and running.
        /// </summary>
        Active,
        
        /// <summary>
        /// Session is paused or suspended.
        /// </summary>
        Paused,
        
        /// <summary>
        /// Session has ended.
        /// </summary>
        Ended,
        
        /// <summary>
        /// Session encountered an error.
        /// </summary>
        Error
    }
}
