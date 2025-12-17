using System;

namespace Godot.Integration.Settings;

public sealed class StaticSettingsProvider<TSettings> : ISettingsProvider<TSettings>
    where TSettings : class
{
    private readonly TSettings _settings;

    public StaticSettingsProvider(TSettings settings)
    {
        _settings = settings ?? throw new ArgumentNullException(nameof(settings));
    }

    public TSettings GetSettings() => _settings;
}
