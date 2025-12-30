using BarkMoon.GameComposition.Core.Types;
namespace BarkMoon.GameComposition.Godot.Settings;

public interface ISettingsProvider<out TSettings>
    where TSettings : class
{
    TSettings GetSettings();
}

