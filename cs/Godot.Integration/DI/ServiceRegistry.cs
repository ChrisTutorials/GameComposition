using System;
using Godot.Integration.Settings;
using Godot.Integration.Sessions;

namespace Godot.Integration.DI;

public sealed class ServiceRegistry : IDisposable
{
    private readonly GameComposition.Core.Services.DI.ServiceRegistry _inner;

    public ServiceRegistry()
    {
        _inner = new GameComposition.Core.Services.DI.ServiceRegistry();
    }

    public void Dispose() => _inner.Dispose();

    public bool IsRegistered<T>() where T : class => _inner.IsRegistered<T>();

    public bool TryGetService<T>(out T? service) where T : class => _inner.TryGetService(out service);

    public T GetService<T>() where T : class => _inner.GetService<T>();

    public void RegisterSingleton<T>(T instance) where T : class => _inner.RegisterSingleton(instance);

    public void RegisterSingleton<TAbstraction, TImplementation>(TImplementation instance)
        where TAbstraction : class
        where TImplementation : class, TAbstraction
        => _inner.RegisterSingleton<TAbstraction, TImplementation>(instance);

    public void RegisterFactory<T>(Func<T> factory) where T : class => _inner.RegisterFactory(factory);

    public void RegisterScoped<T>(Func<T> factory) where T : class => _inner.RegisterScoped(factory);

    public bool TryGetUserScopeProfile(out UserScopeProfile profile)
    {
        if (_inner.TryGetService<GameComposition.Core.Interfaces.IUserScopeProfileProvider>(out var provider)
            && provider != null)
        {
            var p = provider.GetProfile();
            profile = new UserScopeProfile(new UserId(p.UserId.Value), p.Name);
            return true;
        }

        profile = UserScopeProfile.Default;
        return false;
    }

    public bool TryGetSettingsProvider<TSettings>(out ISettingsProvider<TSettings>? provider)
        where TSettings : class
    {
        if (_inner.TryGetService<GameComposition.Core.Settings.ISettingsProvider<TSettings>>(out var p)
            && p != null)
        {
            provider = new SettingsProviderAdapter<TSettings>(p);
            return true;
        }

        provider = null;
        return false;
    }

    public void EnsureStaticSettingsProvider<TSettings>(TSettings settings)
        where TSettings : class
    {
        if (!_inner.TryGetService<GameComposition.Core.Settings.ISettingsProvider<TSettings>>(out _))
        {
            _inner.RegisterSingleton<GameComposition.Core.Settings.ISettingsProvider<TSettings>>(
                new GameComposition.Core.Settings.StaticSettingsProvider<TSettings>(settings));
        }
    }

    private sealed class SettingsProviderAdapter<TSettings> : ISettingsProvider<TSettings>
        where TSettings : class
    {
        private readonly GameComposition.Core.Settings.ISettingsProvider<TSettings> _innerProvider;

        public SettingsProviderAdapter(GameComposition.Core.Settings.ISettingsProvider<TSettings> innerProvider)
        {
            _innerProvider = innerProvider;
        }

        public TSettings GetSettings() => _innerProvider.GetSettings();
    }
}
