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
    /// YAGNI violation tests using NetArchTests.enhancededition + Shouldly.
    /// Catches unnecessary abstractions and over-engineering patterns.
    /// </summary>
    public class YagniViolationTests
    {
        private readonly ITestOutputHelper _output;
        private readonly Assembly _gameCompositionAssembly;

        public YagniViolationTests(ITestOutputHelper output)
        {
            _output = output;
            _gameCompositionAssembly = Assembly.GetAssembly(typeof(GameComposition.Core.Services.CacheService))
                ?? throw new InvalidOperationException("Could not load GameComposition assembly");
        }

        [Fact]
        public void Services_Should_Not_Have_Unnecessary_Wrapper_Layers()
        {
            // Arrange & Act - NetArchTests validation
            var serviceTypes = Types.InAssembly(_gameCompositionAssembly)
                .That()
                .AreClasses()
                .And()
                .HaveNameEndingWith("Service")
                .And()
                .AreNotAbstract()
                .GetTypes();

            var violations = new List<string>();

            // Check for simple delegation patterns using reflection
            foreach (var serviceType in serviceTypes)
            {
                var methods = serviceType.GetMethods(BindingFlags.Public | BindingFlags.Instance)
                    .Where(m => !m.IsSpecialName);

                foreach (var method in methods)
                {
                    var body = method.GetMethodBody();
                    if (body != null && body.GetILAsByteArray().Length < 20)
                    {
                        var parameters = method.GetParameters();
                        if (parameters.Length == 1)
                        {
                            var paramType = parameters[0].ParameterType;
                            if (paramType.Name.Contains("Service") || paramType.Name.Contains("Pool"))
                            {
                                violations.Add($"{serviceType.Name}.{method.Name} appears to be unnecessary wrapper");
                            }
                        }
                    }
                }
            }

            // Assert - Shouldy validation
            if (violations.Any())
            {
                _output.WriteLine("YAGNI violations found:");
                foreach (var violation in violations)
                {
                    _output.WriteLine($"  - {violation}");
                }
                _output.WriteLine($"WARNING: {violations.Count} potential YAGNI violations found");
            }

            // Allow warnings but don't fail the build
            violations.Count.ShouldBeLessThanOrEqualTo(2, 
                $"Too many YAGNI violations: {violations.Count}. Consider simplifying service architecture.");
        }

        [Fact]
        public void Performance_Services_Should_Be_Focused_And_Simple()
        {
            // Arrange & Act - NetArchTests validation
            var performanceServices = Types.InAssembly(_gameCompositionAssembly)
                .That()
                .AreClasses()
                .And()
                .AreNotAbstract()
                .And()
                .HaveNameContaining("Performance")
                .Or()
                .HaveNameContaining("Optimization")
                .GetTypes();

            var violations = new List<string>();

            foreach (var serviceType in performanceServices)
            {
                var methods = serviceType.GetMethods(BindingFlags.Public | BindingFlags.Instance)
                    .Where(m => !m.IsSpecialName);

                // Check if service combines multiple concerns
                var concernCount = 0;
                if (methods.Any(m => m.Name.Contains("Cache"))) concernCount++;
                if (methods.Any(m => m.Name.Contains("Pool"))) concernCount++;
                if (methods.Any(m => m.Name.Contains("Object"))) concernCount++;

                if (concernCount > 1)
                {
                    violations.Add($"{serviceType.Name} combines {concernCount} different concerns - consider separating");
                }

                // Check for too many methods
                if (methods.Count() > 10)
                {
                    violations.Add($"{serviceType.Name} has {methods.Count()} public methods - may be over-engineered");
                }
            }

            // Assert - Shouldy validation
            if (violations.Any())
            {
                _output.WriteLine("Performance service YAGNI violations:");
                foreach (var violation in violations)
                {
                    _output.WriteLine($"  - {violation}");
                }
                _output.WriteLine("Recommendation: Keep performance services focused on single concern");
            }

            // Fail if we have complex performance services
            violations.Count.ShouldBeLessThanOrEqualTo(1, 
                $"Too many YAGNI violations in performance services: {violations.Count}");
        }

        [Fact]
        public void Service_Dependencies_Should_Be_Minimal_And_Focused()
        {
            // Arrange & Act - NetArchTests validation
            var serviceTypes = Types.InAssembly(_gameCompositionAssembly)
                .That()
                .AreClasses()
                .And()
                .HaveNameEndingWith("Service")
                .And()
                .AreNotAbstract()
                .GetTypes();

            var violations = new List<string>();

            foreach (var serviceType in serviceTypes)
            {
                var constructor = serviceType.GetConstructors().FirstOrDefault();
                if (constructor != null)
                {
                    var parameters = constructor.GetParameters();
                    
                    // Too many dependencies may indicate over-engineering
                    if (parameters.Length > 3)
                    {
                        violations.Add($"{serviceType.Name} has {parameters.Length} dependencies - may be over-engineered");
                    }

                    // Check for dependency chains
                    var serviceDependencies = parameters
                        .Where(p => p.ParameterType.Name.EndsWith("Service"))
                        .ToList();

                    if (serviceDependencies.Count > 1)
                    {
                        violations.Add($"{serviceType.Name} depends on {serviceDependencies.Count} other services - consider refactoring");
                    }
                }
            }

            // Assert - Shouldy validation
            if (violations.Any())
            {
                _output.WriteLine("Service dependency YAGNI violations:");
                foreach (var violation in violations)
                {
                    _output.WriteLine($"  - {violation}");
                }
                _output.WriteLine("Recommendation: Keep service dependencies minimal (≤3)");
            }

            // Warn about complex dependency graphs
            violations.Count.ShouldBeLessThanOrEqualTo(3, 
                $"Too many service dependency issues: {violations.Count}");
        }

        [Theory]
        [InlineData("CacheService", 5)]
        [InlineData("ObjectPoolingService", 8)]
        public void Specific_Services_Should_Stay_Focused(string serviceName, int maxMethodCount)
        {
            // Arrange & Act - NetArchTests validation
            var serviceType = Types.InAssembly(_gameCompositionAssembly)
                .That()
                .HaveName(serviceName)
                .And()
                .AreClasses()
                .GetTypes()
                .FirstOrDefault();

            serviceType.ShouldNotBeNull($"Service {serviceName} should exist");

            var publicMethods = serviceType.GetMethods(BindingFlags.Public | BindingFlags.Instance)
                .Where(m => !m.IsSpecialName)
                .ToList();

            // Assert - Shouldy validation
            if (publicMethods.Count > maxMethodCount)
            {
                _output.WriteLine($"{serviceName} has {publicMethods.Count} public methods:");
                foreach (var method in publicMethods)
                {
                    _output.WriteLine($"  - {method.Name}({string.Join(", ", method.GetParameters().Select(p => p.ParameterType.Name))})");
                }
                _output.WriteLine($"Expected: ≤{maxMethodCount} methods");
            }

            publicMethods.Count.ShouldBeLessThanOrEqualTo(maxMethodCount,
                $"{serviceName} should have ≤{maxMethodCount} public methods, but has {publicMethods.Count}");
        }

        [Fact]
        public void Wrapper_Services_Should_Not_Just_Delegate_To_Microsoft_Extensions()
        {
            // Arrange & Act - NetArchTests validation
            var wrapperServices = Types.InAssembly(_gameCompositionAssembly)
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

            foreach (var serviceType in wrapperServices)
            {
                var constructor = serviceType.GetConstructors().FirstOrDefault();
                if (constructor != null)
                {
                    var parameters = constructor.GetParameters();
                    
                    // Check if this service just wraps a single Microsoft.Extensions type
                    if (parameters.Length == 1)
                    {
                        var paramType = parameters[0].ParameterType;
                        if (paramType.Namespace?.StartsWith("Microsoft.Extensions") == true)
                        {
                            var methods = serviceType.GetMethods(BindingFlags.Public | BindingFlags.Instance)
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
                                violations.Add($"{serviceType.Name} appears to be unnecessary wrapper around {paramType.Name}");
                            }
                        }
                    }
                }
            }

            // Assert - Shouldy validation
            if (violations.Any())
            {
                _output.WriteLine("Unnecessary wrapper services found:");
                foreach (var violation in violations)
                {
                    _output.WriteLine($"  - {violation}");
                }
                _output.WriteLine("Recommendation: Use Microsoft.Extensions types directly");
            }

            // Allow some wrappers but warn about excessive ones
            violations.Count.ShouldBeLessThanOrEqualTo(2, 
                $"Too many unnecessary wrapper services: {violations.Count}");
        }
    }
}
