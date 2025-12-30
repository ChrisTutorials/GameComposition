using BarkMoon.GameComposition.Core.Types;
namespace BarkMoon.GameComposition.Godot.Sessions;

public readonly record struct UserScopeProfile(UserId UserId, string Name)
{
    public static UserScopeProfile Default => new(UserId.Empty, "User");
}

