namespace GameComposition.Core.Types;

using GameUserSessions.Core;

/// <summary>
/// Identifies a user scope and provides a human-readable scope name.
/// </summary>
/// <param name="UserId">The user identifier.</param>
/// <param name="Name">A display-friendly name for the user scope.</param>
public readonly record struct UserScopeProfile(GameUserSessions.Core.UserId UserId, string Name)
{
    /// <summary>
    /// Default profile used when no explicit user identity is provided.
    /// </summary>
    public static UserScopeProfile Default => new(GameUserSessions.Core.UserId.Empty, "User");
}
