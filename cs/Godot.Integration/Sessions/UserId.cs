using System;

namespace Godot.Integration.Sessions;

public readonly record struct UserId(Guid Value)
{
    public static UserId Empty => new(Guid.Empty);

    public static UserId New() => new(Guid.NewGuid());

    public bool IsEmpty => Value == Guid.Empty;
}
