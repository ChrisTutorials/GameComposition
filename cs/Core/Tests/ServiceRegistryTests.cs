using System;
using GameComposition.Core.Settings;
using GameComposition.Core.Services.DI;
using GameComposition.Core.Types;
using Xunit;

namespace GameComposition.Core.Tests;

public sealed class ServiceRegistryTests
{
    private sealed class DisposableService : IDisposable
    {
        public bool IsDisposed { get; private set; }
        public void Dispose() => IsDisposed = true;
    }

    private sealed class ThrowingDisposableService : IDisposable
    {
        public bool WasDisposed { get; private set; }

        public void Dispose()
        {
            WasDisposed = true;
            throw new InvalidOperationException("boom");
        }
    }

    private sealed class FactoryService
    {
    }

    private sealed class ScopedService
    {
    }

    private sealed class HealthService : IServiceHealthCheck
    {
        public ServiceHealthStatus Status { get; set; } = new() { IsHealthy = true };
        public ServiceHealthStatus CheckHealth() => Status;
    }

    [Fact]
    public void RegisterSingleton_Duplicate_Throws()
    {
        var registry = new ServiceRegistry();
        registry.RegisterSingleton(new object());

        Assert.Throws<ServiceRegistrationException>(() => registry.RegisterSingleton(new object()));
    }

    [Fact]
    public void GetService_NotRegistered_Throws()
    {
        var registry = new ServiceRegistry();
        Assert.Throws<ServiceNotRegisteredException>(() => registry.GetService<object>());
    }

    [Fact]
    public void RegisterFactory_Duplicate_Throws()
    {
        var registry = new ServiceRegistry();
        registry.RegisterFactory(() => new FactoryService());

        Assert.Throws<ServiceRegistrationException>(() => registry.RegisterFactory(() => new FactoryService()));
    }

    [Fact]
    public void RegisterScoped_Duplicate_Throws()
    {
        var registry = new ServiceRegistry();
        registry.RegisterScoped(() => new ScopedService());

        Assert.Throws<ServiceRegistrationException>(() => registry.RegisterScoped(() => new ScopedService()));
    }

    [Fact]
    public void GetService_Factory_CreatesNewInstanceEachCall()
    {
        var registry = new ServiceRegistry();
        registry.RegisterFactory(() => new FactoryService());

        var a = registry.GetService<FactoryService>();
        var b = registry.GetService<FactoryService>();

        Assert.NotSame(a, b);
    }

    [Fact]
    public void GetService_Factory_WhenFactoryThrows_WrapsInServiceRegistrationException()
    {
        var registry = new ServiceRegistry();
        registry.RegisterFactory<FactoryService>(() => throw new InvalidOperationException("factory blew up"));

        Assert.Throws<ServiceRegistrationException>(() => registry.GetService<FactoryService>());
    }

    [Fact]
    public void TryGetService_Factory_WhenFactoryThrows_ReturnsFalse()
    {
        var registry = new ServiceRegistry();
        registry.RegisterFactory<FactoryService>(() => throw new InvalidOperationException("factory blew up"));

        var ok = registry.TryGetService<FactoryService>(out var service);

        Assert.False(ok);
        Assert.Null(service);
    }

    [Fact]
    public void ValidateServices_WhenMissing_Throws()
    {
        var registry = new ServiceRegistry();
        registry.RegisterSingleton("hi");

        Assert.Throws<InvalidOperationException>(() => registry.ValidateServices(typeof(string), typeof(int)));
    }

    [Fact]
    public void Clear_RemovesServices_AndDisposesDisposables()
    {
        var registry = new ServiceRegistry();
        var disposable = new DisposableService();
        registry.RegisterSingleton(disposable);
        registry.RegisterFactory(() => new FactoryService());
        registry.RegisterScoped(() => new ScopedService());

        registry.Clear();

        Assert.True(disposable.IsDisposed);
        Assert.Throws<ServiceNotRegisteredException>(() => registry.GetService<string>());
        Assert.Throws<ServiceNotRegisteredException>(() => registry.GetService<FactoryService>());
    }

    [Fact]
    public void Dispose_WhenDisposableThrows_DoesNotThrow()
    {
        var registry = new ServiceRegistry();
        var throwing = new ThrowingDisposableService();
        registry.RegisterSingleton(throwing);

        registry.Dispose();

        Assert.True(throwing.WasDisposed);
    }

    [Fact]
    public void AfterDispose_ThrowsObjectDisposedException()
    {
        var registry = new ServiceRegistry();
        registry.RegisterSingleton("hi");
        registry.Dispose();

        Assert.Throws<ObjectDisposedException>(() => registry.GetService<string>());
        Assert.Throws<ObjectDisposedException>(() => registry.TryGetService<string>(out _));
        Assert.Throws<ObjectDisposedException>(() => registry.IsRegistered<string>());
        Assert.Throws<ObjectDisposedException>(() => registry.GetStatistics());
        Assert.Throws<ObjectDisposedException>(() => registry.PerformHealthChecks());
        Assert.Throws<ObjectDisposedException>(() => registry.CreateScope());
    }

    [Fact]
    public void ScopedService_InScope_IsCached()
    {
        var registry = new ServiceRegistry();
        registry.RegisterScoped(() => new object());

        using var scope = registry.CreateScope();
        var a = scope.GetService<object>();
        var b = scope.GetService<object>();

        Assert.Same(a, b);
    }

    [Fact]
    public void ScopedService_AcrossScopes_IsNotCached()
    {
        var registry = new ServiceRegistry();
        registry.RegisterScoped(() => new object());

        object a;
        using (var scopeA = registry.CreateScope())
            a = scopeA.GetService<object>();

        object b;
        using (var scopeB = registry.CreateScope())
            b = scopeB.GetService<object>();

        Assert.NotSame(a, b);
    }

    [Fact]
    public void Dispose_DisposesRegisteredDisposables()
    {
        var registry = new ServiceRegistry();
        var disposable = new DisposableService();
        registry.RegisterSingleton(disposable);

        registry.Dispose();

        Assert.True(disposable.IsDisposed);
    }

    [Fact]
    public void PerformHealthChecks_ReturnsUnhealthyWhenAnyServiceUnhealthy()
    {
        var registry = new ServiceRegistry();
        var svc = new HealthService { Status = new ServiceHealthStatus { IsHealthy = false, Message = "bad" } };
        registry.RegisterSingleton<IServiceHealthCheck, HealthService>(svc);

        var result = registry.PerformHealthChecks();

        Assert.False(result.IsHealthy);
        Assert.Single(result.UnhealthyServices);
    }

    [Fact]
    public void GetStatistics_ReturnsExpectedCounts()
    {
        var registry = new ServiceRegistry();
        registry.RegisterSingleton("hi");
        registry.RegisterFactory(() => new FactoryService());
        registry.RegisterScoped(() => new ScopedService());

        var stats = registry.GetStatistics();

        Assert.Equal(1, stats.SingletonCount);
        Assert.Equal(1, stats.FactoryCount);
        Assert.Equal(1, stats.ScopedFactoryCount);
        Assert.Equal(3, stats.TotalServices);
    }

    [Fact]
    public void ServiceScope_GetService_WhenNotRegistered_ThrowsInvalidOperationException()
    {
        var registry = new ServiceRegistry();

        using var scope = registry.CreateScope();
        Assert.Throws<InvalidOperationException>(() => scope.GetService(typeof(object)));
    }

    [Fact]
    public void ServiceScope_GetService_WhenDisposed_ThrowsObjectDisposedException()
    {
        var registry = new ServiceRegistry();
        registry.RegisterScoped(() => new object());

        var scope = registry.CreateScope();
        scope.Dispose();

        Assert.Throws<ObjectDisposedException>(() => scope.GetService<object>());
    }

    [Fact]
    public void ServiceScope_Dispose_SwallowsDisposableErrors()
    {
        var registry = new ServiceRegistry();
        registry.RegisterScoped(() => new ThrowingDisposableService());

        var scope = registry.CreateScope();
        _ = scope.GetService<ThrowingDisposableService>();

        scope.Dispose();
    }

    [Fact]
    public void UserId_FromString_WhenWhitespace_ReturnsEmpty()
    {
        var id = UserId.FromString("  ");
        Assert.True(id.IsEmpty);
        Assert.Equal(UserId.Empty, id);
    }

    [Fact]
    public void UserId_FromString_WhenInvalid_ReturnsEmpty()
    {
        var id = UserId.FromString("not-a-guid");
        Assert.True(id.IsEmpty);
        Assert.Equal(UserId.Empty, id);
    }

    [Fact]
    public void UserId_FromString_WhenValid_Parses()
    {
        var guid = Guid.NewGuid();
        var id = UserId.FromString(guid.ToString());

        Assert.False(id.IsEmpty);
        Assert.Equal(guid, id.Value);
        Assert.Equal(guid.ToString(), id.ToString());
    }

    [Fact]
    public void UserScopeProfile_Default_IsStable()
    {
        var profile = UserScopeProfile.Default;

        Assert.Equal(UserId.Empty, profile.UserId);
        Assert.Equal("User", profile.Name);
    }

    [Fact]
    public void StaticSettingsProvider_ReturnsSameSettingsInstance()
    {
        var settings = new object();
        ISettingsProvider<object> provider = new StaticSettingsProvider<object>(settings);

        var resolved = provider.GetSettings();

        Assert.Same(settings, resolved);
    }
}
