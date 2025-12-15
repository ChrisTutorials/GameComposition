namespace GameComposition.Core.Settings;

/// <summary>
/// Provides settings of type <typeparamref name="TSettings"/> to runtime services.
/// </summary>
/// <typeparam name="TSettings">The settings type.</typeparam>
public interface ISettingsProvider<TSettings>
{
    /// <summary>
    /// Returns the current settings value.
    /// </summary>
    /// <returns>The settings value.</returns>
    TSettings GetSettings();
}

/// <summary>
/// Simple settings provider that always returns the same instance.
/// </summary>
/// <typeparam name="TSettings">The settings type.</typeparam>
/// <param name="settings">The settings value to return.</param>
public sealed class StaticSettingsProvider<TSettings>(TSettings settings) : ISettingsProvider<TSettings>
{
    /// <summary>
    /// Returns the configured settings value.
    /// </summary>
    /// <returns>The settings value.</returns>
    public TSettings GetSettings() => settings;
}
