using GameComposition.Core.Types;

namespace GameComposition.Core.Interfaces;

/// <summary>
/// Provides the user-scoped profile used to create or identify a user service scope.
/// </summary>
public interface IUserScopeProfileProvider
{
    /// <summary>
    /// Returns the current user scope profile.
    /// </summary>
    /// <returns>The user scope profile.</returns>
    UserScopeProfile GetProfile();
}
