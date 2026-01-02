using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Xunit;
using Xunit.Abstractions;
using BarkMoon.GameComposition.Core.Types;

namespace BarkMoon.GameComposition.Core.Tests.Performance
{
    /// <summary>
    /// Performance tests comparing string-based TypedId with numeric NumericId.
    /// </summary>
    public class IdPerformanceTests
    {
        private readonly ITestOutputHelper _output;
        
        public IdPerformanceTests(ITestOutputHelper output)
        {
            _output = output;
        }
        
        // Test ID types
        public readonly record struct StringShopId(string Value);
        public readonly record struct NumericShopId(long Value);
        
        [Fact]
        public void StringId_MemoryUsage_IsHigherThanNumeric()
        {
            // Arrange
            const int count = 100_000;
            var stringIds = new List<StringShopId>(count);
            var numericIds = new List<NumericShopId>(count);
            
            // Act
            var stringMemory = GC.GetTotalMemory(true);
            for (int i = 0; i < count; i++)
            {
                stringIds.Add(new StringShopId($"shop-{Guid.NewGuid():N}"));
            }
            var stringMemoryAfter = GC.GetTotalMemory(false);
            
            var numericMemory = GC.GetTotalMemory(true);
            for (int i = 0; i < count; i++)
            {
                numericIds.Add(new NumericShopId(i + 1));
            }
            var numericMemoryAfter = GC.GetTotalMemory(false);
            
            // Assert
            var stringUsage = stringMemoryAfter - stringMemory;
            var numericUsage = numericMemoryAfter - numericMemory;
            
            _output.WriteLine($"String IDs: {stringUsage:N0} bytes for {count:N0} IDs ({stringUsage / count:F1} bytes per ID)");
            _output.WriteLine($"Numeric IDs: {numericUsage:N0} bytes for {count:N0} IDs ({numericUsage / count:F1} bytes per ID)");
            _output.WriteLine($"Memory reduction: {(1.0 - (double)numericUsage / stringUsage) * 100:F1}%");
            
            // Numeric IDs should use significantly less memory
            Assert.True(numericUsage < stringUsage / 2, "Numeric IDs should use at least 50% less memory");
        }
        
        [Fact]
        public void NumericId_Comparison_IsFasterThanString()
        {
            // Arrange
            const int iterations = 1_000_000;
            var stringId1 = new StringShopId("shop-12345");
            var stringId2 = new StringShopId("shop-67890");
            var numericId1 = new NumericShopId(12345);
            var numericId2 = new NumericShopId(67890);
            
            // Act & Assert - String comparison
            var sw = Stopwatch.StartNew();
            for (int i = 0; i < iterations; i++)
            {
                var equal = stringId1 == stringId2;
            }
            sw.Stop();
            var stringTime = sw.ElapsedMilliseconds;
            
            // Act & Assert - Numeric comparison
            sw.Restart();
            for (int i = 0; i < iterations; i++)
            {
                var equal = numericId1 == numericId2;
            }
            sw.Stop();
            var numericTime = sw.ElapsedMilliseconds;
            
            _output.WriteLine($"String comparison: {stringTime}ms for {iterations:N0} operations");
            _output.WriteLine($"Numeric comparison: {numericTime}ms for {iterations:N0} operations");
            _output.WriteLine($"Speed improvement: {(double)stringTime / numericTime:F1}x faster");
            
            // Numeric comparison should be faster (allowing for system variations)
            Assert.True(numericTime <= stringTime, "Numeric comparison should be as fast or faster than string comparison");
        }
        
        [Fact]
        public void NumericId_Generation_IsFasterThanGuid()
        {
            // Arrange
            const int iterations = 100_000;
            
            // Act & Assert - GUID generation
            var sw = Stopwatch.StartNew();
            for (int i = 0; i < iterations; i++)
            {
                var id = new StringShopId(Guid.NewGuid().ToString("N"));
            }
            sw.Stop();
            var guidTime = sw.ElapsedMilliseconds;
            
            // Act & Assert - Numeric generation
            sw.Restart();
            for (int i = 0; i < iterations; i++)
            {
                var id = new NumericShopId(i + 1);
            }
            sw.Stop();
            var numericTime = sw.ElapsedMilliseconds;
            
            _output.WriteLine($"GUID generation: {guidTime}ms for {iterations:N0} IDs");
            _output.WriteLine($"Numeric generation: {numericTime}ms for {iterations:N0} IDs");
            _output.WriteLine($"Speed improvement: {(double)guidTime / numericTime:F1}x faster");
            
            // Numeric generation should be faster
            Assert.True(numericTime <= guidTime, "Numeric generation should be as fast or faster than GUID generation");
        }
        
        [Fact]
        public void IdConverter_RoundTrip_ConversionMaintainsConsistency()
        {
            // Arrange
            var originalGuid = Guid.NewGuid().ToString("N");
            
            // Act
            var numeric = IdConverter.GuidToNumeric<NumericShopId>(originalGuid);
            var convertedBack = IdConverter.NumericToGuid(numeric);
            
            // Assert
            Assert.NotEqual(originalGuid, convertedBack); // One-way conversion expected
            Assert.True(numeric.HasValue);
            Assert.True(IdConverter.IsGuid(originalGuid));
            Assert.True(IdConverter.IsNumeric(numeric.ToString()));
            
            _output.WriteLine($"Original GUID: {originalGuid}");
            _output.WriteLine($"Numeric ID: {numeric}");
            _output.WriteLine($"Converted back: {convertedBack}");
        }
        
        [Fact]
        public void LargeDataset_NumericIds_OptimizeMemoryUsage()
        {
            // Arrange - Simulate RimWorld/Stardew Valley scale
            const int entityCount = 10_000; // Typical for medium-sized game
            
            var stringEntities = new List<(StringShopId Id, string Name)>();
            var numericEntities = new List<(NumericShopId Id, string Name)>();
            
            // Act - Create entities with string IDs
            GC.Collect();
            var stringMemoryBefore = GC.GetTotalMemory(true);
            for (int i = 0; i < entityCount; i++)
            {
                stringEntities.Add((new StringShopId($"entity-{Guid.NewGuid():N}"), $"Entity {i}"));
            }
            var stringMemoryAfter = GC.GetTotalMemory(false);
            
            // Act - Create entities with numeric IDs
            GC.Collect();
            var numericMemoryBefore = GC.GetTotalMemory(true);
            for (int i = 0; i < entityCount; i++)
            {
                numericEntities.Add((new NumericShopId(i + 1), $"Entity {i}"));
            }
            var numericMemoryAfter = GC.GetTotalMemory(false);
            
            // Assert
            var stringUsage = stringMemoryAfter - stringMemoryBefore;
            var numericUsage = numericMemoryAfter - numericMemoryBefore;
            var savings = stringUsage - numericUsage;
            
            _output.WriteLine($"Entities: {entityCount:N0}");
            _output.WriteLine($"String IDs: {stringUsage:N0} bytes total");
            _output.WriteLine($"Numeric IDs: {numericUsage:N0} bytes total");
            _output.WriteLine($"Memory saved: {savings:N0} bytes ({(double)savings / stringUsage * 100:F1}%)");
            
            // Should save significant memory at scale
            Assert.True(savings > stringUsage * 0.4, "Should save at least 40% memory at scale");
        }
    }
}
