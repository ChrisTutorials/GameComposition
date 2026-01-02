using System;
using System.Linq;
using System.Reflection;
using NetArchTest.Rules;
using Shouldly;
using Xunit;
using Xunit.Abstractions;

namespace BarkMoon.GameComposition.Core.Tests.Architectural
{
    /// <summary>
    /// Microsoft.Extensions usage tests using NetArchTests.enhancededition + Shouldly.
    /// Validates proper usage of Microsoft.Extensions instead of custom implementations.
    /// </summary>
    public class MicrosoftExtensionsUsageTests
    {
        private readonly ITestOutputHelper _output;
        private readonly Assembly _gameCompositionAssembly;

        public MicrosoftExtensionsUsageTests(ITestOutputHelper output)
        {
            _output = output;
            _gameCompositionAssembly = Assembly.GetAssembly(typeof(GameComposition.Core.Services.CacheService))
                ?? throw new InvalidOperationException("Could not load GameComposition assembly");
        }

        [Fact]
        public void Should_Use_Microsoft_Extensions_ObjectPool_Directly()
        {
            // Arrange & Act - NetArchTests validation
            var customPools = Types.InAssembly(_gameCompositionAssembly)
                .That()
                .AreClasses()
                .And()
                .AreNotAbstract()
                .And()
                .HaveNameContaining("Pool")
                .And()
                .DoNotHaveNameStartingWith("Microsoft")
                .GetTypes();

            var violations = new List<string>();

            // Check if custom pools reimplement ObjectPool functionality
            foreach (var type in customPools)
            {
                var methods = type.GetMethods(BindingFlags.Public | BindingFlags.Instance)
                    .Where(m => !m.IsSpecialName)
                    .ToList();

                var hasGet = methods.Any(m => m.Name.Contains("Get"));
                var hasReturn = methods.Any(m => m.Name.Contains("Return"));

                if (hasGet && hasReturn)
                {
                    violations.Add($"{type.Name} appears to reimplement ObjectPool - use Microsoft.Extensions.ObjectPool instead");
                }
            }

            // Assert - Shouldy validation
            if (violations.Any())
            {
                _output.WriteLine("Custom pool implementations that should use Microsoft.Extensions:");
                foreach (var violation in violations)
                {
                    _output.WriteLine($"  - {violation}");
                }
                _output.WriteLine("Recommendation: Use Microsoft.Extensions.ObjectPool.ObjectPool<T>");
            }

            violations.ShouldBeEmpty("Should use Microsoft.Extensions.ObjectPool instead of custom implementations");
        }

        [Fact]
        public void Should_Use_Microsoft_Extensions_Caching_Directly()
        {
            // Arrange & Act - NetArchTests validation
            var customCaches = Types.InAssembly(_gameCompositionAssembly)
                .That()
                .AreClasses()
                .And()
                .AreNotAbstract()
                .And()
                .HaveNameContaining("Cache")
                .And()
                .DoNotHaveNameStartingWith("Microsoft")
                .GetTypes();

            var violations = new List<string>();

            // Check if custom caches reimplement MemoryCache functionality
            foreach (var type in customCaches)
            {
                var methods = type.GetMethods(BindingFlags.Public | BindingFlags.Instance)
                    .Where(m => !m.IsSpecialName)
                    .ToList();

                var hasGetOrCreate = methods.Any(m => m.Name.Contains("GetOrCreate"));
                var hasRemove = methods.Any(m => m.Name.Contains("Remove"));
                var hasKeyParameters = methods.Any(m => m.GetParameters().Any(p => p.ParameterType == typeof(string)));

                if (hasGetOrCreate && hasRemove && hasKeyParameters)
                {
                    violations.Add($"{type.Name} appears to reimplement MemoryCache - use Microsoft.Extensions.Caching.Memory instead");
                }
            }

            // Assert - Shouldy validation
            if (violations.Any())
            {
                _output.WriteLine("Custom cache implementations that should use Microsoft.Extensions:");
                foreach (var violation in violations)
                {
                    _output.WriteLine($"  - {violation}");
                }
                _output.WriteLine("Recommendation: Use Microsoft.Extensions.Caching.Memory.IMemoryCache");
            }

            violations.ShouldBeEmpty("Should use Microsoft.Extensions.Caching.Memory instead of custom implementations");
        }

        [Fact]
        public void Should_Use_Microsoft_Extensions_DependencyInjection_Directly()
        {
            // Arrange & Act - NetArchTests validation
            var customDiTypes = Types.InAssembly(_gameCompositionAssembly)
                .That()
                .AreClasses()
                .And()
                .AreNotAbstract()
                .And()
                .HaveNameContaining("Service")
                .Or()
                .HaveNameContaining("Registry")
                .Or()
                .HaveNameContaining("Provider")
                .And()
                .DoNotHaveNameStartingWith("Microsoft")
                .GetTypes();

            var violations = new List<string>();

            // Check if custom DI types reimplement DI functionality
            foreach (var type in customDiTypes)
            {
                var methods = type.GetMethods(BindingFlags.Public | BindingFlags.Instance)
                    .Where(m => !m.IsSpecialName)
                    .ToList();

                var hasGetService = methods.Any(m => m.Name.Contains("GetService"));
                var hasRegister = methods.Any(m => m.Name.Contains("Register") || m.Name.Contains("Add"));
                var hasGenericConstraints = methods.Any(m => m.IsGenericMethod);

                if (hasGetService && (hasRegister || hasGenericConstraints))
                {
                    violations.Add($"{type.Name} appears to reimplement DI - use Microsoft.Extensions.DependencyInjection instead");
                }
            }

            // Assert - Shouldy validation
            if (violations.Any())
            {
                _output.WriteLine("Custom DI implementations that should use Microsoft.Extensions:");
                foreach (var violation in violations)
                {
                    _output.WriteLine($"  - {violation}");
                }
                _output.WriteLine("Recommendation: Use Microsoft.Extensions.DependencyInjection.IServiceProvider");
            }

            violations.ShouldBeEmpty("Should use Microsoft.Extensions.DependencyInjection instead of custom implementations");
        }

        [Theory]
        [InlineData("Microsoft.Extensions.ObjectPool")]
        [InlineData("Microsoft.Extensions.Caching.Memory")]
        [InlineData("Microsoft.Extensions.DependencyInjection")]
        public void Should_Prefer_Microsoft_Extensions_Over_Custom_Implementations(string microsoftNamespace)
        {
            // Arrange & Act - NetArchTests validation
            var microsoftTypes = Types.InAssembly(_gameCompositionAssembly)
                .That()
                .ResideInNamespaceStartingWith(microsoftNamespace)
                .GetTypes();

            var customTypes = Types.InAssembly(_gameCompositionAssembly)
                .That()
                .AreClasses()
                .And()
                .AreNotAbstract()
                .And()
                .DoNotResideInNamespaceStartingWith("Microsoft")
                .And()
                .DoNotResideInNamespaceContaining("Tests")
                .GetTypes();

            var violations = new List<string>();

            // Check if custom types duplicate Microsoft.Extensions functionality
            foreach (var customType in customTypes)
            {
                var customMethods = customType.GetMethods(BindingFlags.Public | BindingFlags.Instance)
                    .Where(m => !m.IsSpecialName)
                    .Select(m => m.Name)
                    .ToHashSet();

                foreach (var microsoftType in microsoftTypes)
                {
                    var microsoftMethods = microsoftType.GetMethods(BindingFlags.Public | BindingFlags.Instance)
                        .Where(m => !m.IsSpecialName)
                        .Select(m => m.Name)
                        .ToHashSet();

                    var overlap = customMethods.Intersect(microsoftMethods).ToList();
                    
                    // If >50% method overlap, likely unnecessary duplication
                    if (overlap.Count > Math.Min(customMethods.Count, microsoftMethods.Count) * 0.5)
                    {
                        violations.Add($"{customType.Name} duplicates functionality from {microsoftType.Name}");
                    }
                }
            }

            // Assert - Shouldy validation
            if (violations.Any())
            {
                _output.WriteLine($"Custom implementations that duplicate {microsoftNamespace}:");
                foreach (var violation in violations)
                {
                    _output.WriteLine($"  - {violation}");
                }
                _output.WriteLine($"Recommendation: Use {microsoftNamespace} directly instead of custom implementations");
            }

            // Allow some custom implementations but warn about duplicates
            violations.Count.ShouldBeLessThanOrEqualTo(2, 
                $"Too many potential duplications of {microsoftNamespace}: {violations.Count}");
        }

        [Fact]
        public void Should_Not_Have_Unnecessary_Abstraction_Layers()
        {
            // Arrange & Act - NetArchTests validation
            var abstractionTypes = Types.InAssembly(_gameCompositionAssembly)
                .That()
                .AreClasses()
                .And()
                .AreNotAbstract()
                .And()
                .HaveNameEndingWith("Service")
                .Or()
                .HaveNameEndingWith("Adapter")
                .Or()
                .HaveNameEndingWith("Wrapper")
                .GetTypes();

            var violations = new List<string>();

            // Check for unnecessary abstractions
            foreach (var type in abstractionTypes)
            {
                var constructor = type.GetConstructors().FirstOrDefault();
                if (constructor != null)
                {
                    var parameters = constructor.GetParameters();
                    
                    // Check if this type just wraps a single Microsoft.Extensions type
                    if (parameters.Length == 1)
                    {
                        var paramType = parameters[0].ParameterType;
                        if (paramType.Namespace?.StartsWith("Microsoft.Extensions") == true)
                        {
                            var methods = type.GetMethods(BindingFlags.Public | BindingFlags.Instance)
                                .Where(m => !m.IsSpecialName)
                                .ToList();

                            // Check if most methods just delegate to the wrapped type
                            var delegationCount = methods.Count(m =>
                            {
                                var mParams = m.GetParameters();
                                return mParams.Length == 1 && 
                                       mParams[0].ParameterType == paramType &&
                                       m.ReturnType == mParams[0].ParameterType;
                            });

                            if (delegationCount > methods.Count * 0.7) // 70% delegation
                            {
                                violations.Add($"{type.Name} appears to be unnecessary wrapper around {paramType.Name}");
                            }
                        }
                    }
                }
            }

            // Assert - Shouldy validation
            if (violations.Any())
            {
                _output.WriteLine("Unnecessary abstraction layers found:");
                foreach (var violation in violations)
                {
                    _output.WriteLine($"  - {violation}");
                }
                _output.WriteLine("Recommendation: Use Microsoft.Extensions types directly");
            }

            // Allow some abstractions but warn about excessive ones
            violations.Count.ShouldBeLessThanOrEqualTo(2, 
                $"Too many unnecessary abstractions: {violations.Count}");
        }

        [Fact]
        public void Microsoft_Extensions_Packages_Should_Be_Properly_Referenced()
        {
            // Arrange & Act - NetArchTests validation
            var microsoftTypes = Types.InAssembly(_gameCompositionAssembly)
                .That()
                .ResideInNamespaceStartingWith("Microsoft.Extensions")
                .GetTypes();

            // Assert - Shouldy validation
            // Should have Microsoft.Extensions types available
            microsoftTypes.ShouldNotBeEmpty("Should reference Microsoft.Extensions packages");

            // Should have the key Microsoft.Extensions types we expect
            var hasObjectPool = microsoftTypes.Any(t => t.Name.Contains("ObjectPool"));
            var hasMemoryCache = microsoftTypes.Any(t => t.Name.Contains("MemoryCache"));
            var hasServiceProvider = microsoftTypes.Any(t => t.Name.Contains("ServiceProvider"));

            hasObjectPool.ShouldBeTrue("Should have Microsoft.Extensions.ObjectPool available");
            hasMemoryCache.ShouldBeTrue("Should have Microsoft.Extensions.Caching.Memory available");
            hasServiceProvider.ShouldBeTrue("Should have Microsoft.Extensions.DependencyInjection available");

            _output.WriteLine($"Microsoft.Extensions types found: {microsoftTypes.Count}");
            _output.WriteLine("  - ObjectPool types available");
            _output.WriteLine("  - MemoryCache types available");
            _output.WriteLine("  - ServiceProvider types available");
        }
    }
}
