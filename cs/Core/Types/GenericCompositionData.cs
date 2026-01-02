using System.Collections.Generic;

namespace BarkMoon.GameComposition.Core.Types;

/// <summary>
/// Generic implementation of ICompositionData using a dictionary.
/// Use this only when dynamic data is strictly required. Prefer strongly-typed implementations.
/// </summary>
public record GenericCompositionData(Dictionary<string, object> Data) : ICompositionData
{
    public GenericCompositionData() : this(new Dictionary<string, object>()) { }
    
    public object? this[string key]
    {
        get => Data.TryGetValue(key, out var value) ? value : null;
        set { if (value != null) Data[key] = value; else Data.Remove(key); }
    }
}
