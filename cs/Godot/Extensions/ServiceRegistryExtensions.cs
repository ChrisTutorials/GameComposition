using System;
using BarkMoon.GameComposition.Core.Services.DI;
using BarkMoon.GameComposition.Core.Interfaces;
using BarkMoon.GameComposition.Core.Settings;
using BarkMoon.GameComposition.Core.Types;

namespace BarkMoon.GameComposition.Godot.Extensions
{
    /// <summary>
    /// Godot-specific extension methods for the Core ServiceRegistry.
    /// Provides convenience methods for accessing common framework services.
    /// </summary>
    public static class ServiceRegistryExtensions
    {
        /// <summary>
        /// Tries to get the user scope profile from the registry.
        /// </summary>
        public static bool TryGetUserScopeProfile(this ServiceRegistry registry, out UserScopeProfile profile)
        {
            if (registry.TryGetService<IUserScopeProfileProvider>(out var provider) && provider != null)
            {
                profile = provider.GetProfile();
                return true;
            }

            profile = UserScopeProfile.Default;
            return false;
        }

        /// <summary>
        /// Tries to get a settings provider for a specific settings type.
        /// </summary>
        public static bool TryGetSettingsProvider<TSettings>(this ServiceRegistry registry, out ISettingsProvider<TSettings>? provider)
            where TSettings : class
        {
            return registry.TryGetService<ISettingsProvider<TSettings>>(out provider);
        }

        /// <summary>
        /// Ensures a static settings provider is registered for the given settings instance if one doesn't already exist.
        /// </summary>
        public static void EnsureStaticSettingsProvider<TSettings>(this ServiceRegistry registry, TSettings settings)
            where TSettings : class
        {
            if (!registry.TryGetService<ISettingsProvider<TSettings>>(out _))
            {
                registry.RegisterSingleton<ISettingsProvider<TSettings>>(new StaticSettingsProvider<TSettings>(settings));
            }
        }
    }
}

