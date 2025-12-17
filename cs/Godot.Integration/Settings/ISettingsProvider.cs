namespace Godot.Integration.Settings;

public interface ISettingsProvider<out TSettings>
    where TSettings : class
{
    TSettings GetSettings();
}
