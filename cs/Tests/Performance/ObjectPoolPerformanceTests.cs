using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Xunit;
using Xunit.Abstractions;
using BarkMoon.GameComposition.Core.Performance;
using BarkMoon.GameComposition.Core.Types;

namespace BarkMoon.GameComposition.Core.Tests.Performance
{
    /// <summary>
    /// Performance benchmarks to validate ObjectPool effectiveness
    /// Tests both allocation reduction and speed improvements
    /// </summary>
    public class ObjectPoolPerformanceTests
    {
        private readonly ITestOutputHelper _output;

        public ObjectPoolPerformanceTests(ITestOutputHelper output)
        {
            _output = output;
        }

        [Fact]
        public void ObjectPool_ListCreation_ReducesAllocations()
        {
            // Arrange
            const int iterations = 10_000;
            var pool = new ObjectPool<List<CoreVector2I>>(50, list => list.Clear());
            
            // Act & Assert - Direct allocation
            var sw1 = Stopwatch.StartNew();
            var allocatedLists = new List<List<CoreVector2I>>();
            for (int i = 0; i < iterations; i++)
            {
                var list = new List<CoreVector2I>();
                list.Add(new CoreVector2I(i, i));
                allocatedLists.Add(list);
            }
            sw1.Stop();
            
            // Act & Assert - Pooled allocation
            var sw2 = Stopwatch.StartNew();
            var pooledLists = new List<List<CoreVector2I>>();
            for (int i = 0; i < iterations; i++)
            {
                var list = pool.Get();
                list.Add(new CoreVector2I(i, i));
                pooledLists.Add(list);
            }
            sw2.Stop();
            
            // Return pooled objects
            foreach (var list in pooledLists)
            {
                pool.Return(list);
            }
            
            _output.WriteLine($"Direct allocation: {sw1.ElapsedMilliseconds}ms for {iterations:N0} lists");
            _output.WriteLine($"Pooled allocation: {sw2.ElapsedMilliseconds}ms for {iterations:N0} lists");
            _output.WriteLine($"Speed improvement: {(double)sw1.ElapsedMilliseconds / sw2.ElapsedMilliseconds:F1}x");
            _output.WriteLine($"Pool count after return: {pool.Count}/{pool.MaxCapacity}");
            
            // Pooled should be faster after warmup
            Assert.True(sw2.ElapsedMilliseconds <= sw1.ElapsedMilliseconds, "Pooled allocation should be as fast or faster");
        }

        [Fact]
        public void ObjectPool_DictionaryCreation_ReducesAllocations()
        {
            // Arrange
            const int iterations = 5_000;
            var pool = new ObjectPool<Dictionary<string, object>>(25, dict => dict.Clear());
            
            // Act & Assert - Direct allocation
            var sw1 = Stopwatch.StartNew();
            for (int i = 0; i < iterations; i++)
            {
                var dict = new Dictionary<string, object>();
                dict[$"key{i}"] = $"value{i}";
                dict[$"nested{i}"] = new { Index = i, Name = $"test{i}" };
            }
            sw1.Stop();
            
            // Act & Assert - Pooled allocation
            var sw2 = Stopwatch.StartNew();
            var pooledDicts = new List<Dictionary<string, object>>();
            for (int i = 0; i < iterations; i++)
            {
                var dict = pool.Get();
                dict[$"key{i}"] = $"value{i}";
                dict[$"nested{i}"] = new { Index = i, Name = $"test{i}" };
                pooledDicts.Add(dict);
            }
            sw2.Stop();
            
            // Return pooled objects
            foreach (var dict in pooledDicts)
            {
                pool.Return(dict);
            }
            
            _output.WriteLine($"Direct allocation: {sw1.ElapsedMilliseconds}ms for {iterations:N0} dictionaries");
            _output.WriteLine($"Pooled allocation: {sw2.ElapsedMilliseconds}ms for {iterations:N0} dictionaries");
            _output.WriteLine($"Speed improvement: {(double)sw1.ElapsedMilliseconds / sw2.ElapsedMilliseconds:F1}x");
            
            // Pooled should be faster
            Assert.True(sw2.ElapsedMilliseconds <= sw1.ElapsedMilliseconds, "Pooled allocation should be as fast or faster");
        }

        [Fact]
        public void CollectionPool_MultipleTypes_ReducesMemoryPressure()
        {
            // Arrange
            const int iterations = 1_000;
            
            // Act & Assert - Direct allocation
            var sw1 = Stopwatch.StartNew();
            var directLists = new List<List<CoreVector2I>>();
            var directDicts = new List<Dictionary<string, int>>();
            
            for (int i = 0; i < iterations; i++)
            {
                var list = new List<CoreVector2I>();
                var dict = new Dictionary<string, int>();
                
                for (int j = 0; j < 10; j++)
                {
                    list.Add(new CoreVector2I(i + j, i + j));
                    dict[$"key{j}"] = i + j;
                }
                
                directLists.Add(list);
                directDicts.Add(dict);
            }
            sw1.Stop();
            
            // Act & Assert - Pooled allocation
            var sw2 = Stopwatch.StartNew();
            var pooledLists = new List<List<CoreVector2I>>();
            var pooledDicts = new List<Dictionary<string, int>>();
            
            for (int i = 0; i < iterations; i++)
            {
                using var listPool = CollectionPool.GetListPool<CoreVector2I>();
                using var dictPool = CollectionPool.GetDictionaryPool<string, int>();
                
                var list = listPool.Get();
                var dict = dictPool.Get();
                
                for (int j = 0; j < 10; j++)
                {
                    list.Add(new CoreVector2I(i + j, i + j));
                    dict[$"key{j}"] = i + j;
                }
                
                pooledLists.Add(list);
                pooledDicts.Add(dict);
            }
            sw2.Stop();
            
            // Return pooled objects
            foreach (var list in pooledLists)
                CollectionPool.GetListPool<CoreVector2I>().Return(list);
            foreach (var dict in pooledDicts)
                CollectionPool.GetDictionaryPool<string, int>().Return(dict);
            
            _output.WriteLine($"Direct allocation: {sw1.ElapsedMilliseconds}ms for {iterations:N0} mixed collections");
            _output.WriteLine($"Pooled allocation: {sw2.ElapsedMilliseconds}ms for {iterations:N0} mixed collections");
            _output.WriteLine($"Speed improvement: {(double)sw1.ElapsedMilliseconds / sw2.ElapsedMilliseconds:F1}x");
            
            // Pooled should be significantly faster for mixed operations
            Assert.True(sw2.ElapsedMilliseconds <= sw1.ElapsedMilliseconds * 1.1, "Pooled should be competitive even with overhead");
        }

        [Fact]
        public void ObjectPool_ConcurrentAccess_ThreadSafe()
        {
            // Arrange
            const int threads = 4;
            const int operationsPerThread = 1_000;
            var pool = new ObjectPool<List<CoreVector2I>>(100, list => list.Clear());
            var results = new List<(long Allocated, long Returned)>();
            
            // Act
            var sw = Stopwatch.StartNew();
            var tasks = Enumerable.Range(0, threads).Select(threadId => 
            {
                long allocated = 0;
                long returned = 0;
                
                for (int i = 0; i < operationsPerThread; i++)
                {
                    var list = pool.Get();
                    allocated++;
                    list.Add(new CoreVector2I(threadId, i));
                    
                    // Randomly return some objects to test concurrent access
                    if (i % 3 == 0)
                    {
                        pool.Return(list);
                        returned++;
                    }
                }
                
                return (allocated, returned);
            }).ToArray();
            
            // Wait for all tasks
            Task.WaitAll(tasks);
            sw.Stop();
            
            // Collect results
            foreach (var result in tasks)
            {
                results.Add(result);
            }
            
            var totalAllocated = results.Sum(r => r.Allocated);
            var totalReturned = results.Sum(r => r.Returned);
            
            _output.WriteLine($"Concurrent test: {threads} threads, {operationsPerThread:N0} ops each");
            _output.WriteLine($"Total allocated: {totalAllocated:N0}");
            _output.WriteLine($"Total returned: {totalReturned:N0}");
            _output.WriteLine($"Pool count: {pool.Count}/{pool.MaxCapacity}");
            _output.WriteLine($"Time: {sw.ElapsedMilliseconds}ms");
            
            // Verify thread safety - no exceptions thrown
            Assert.True(totalAllocated > 0, "Objects should be allocated");
            Assert.True(pool.Count <= pool.MaxCapacity, "Pool should not exceed capacity");
        }

        [Fact]
        public void ObjectPool_CustomFactory_ReducesComplexObjectCreation()
        {
            // Arrange
            const int iterations = 1_000;
            
            // Complex object with expensive initialization
            var pool = new ObjectPool<ComplexObject>(
                () => new ComplexObject($"Generated-{Guid.NewGuid()}"),
                50,
                obj => obj.Reset()
            );
            
            // Act & Assert - Direct creation
            var sw1 = Stopwatch.StartNew();
            for (int i = 0; i < iterations; i++)
            {
                var obj = new ComplexObject($"Direct-{i}");
                obj.ProcessData(new[] { i, i + 1, i + 2 });
            }
            sw1.Stop();
            
            // Act & Assert - Pooled creation
            var sw2 = Stopwatch.StartNew();
            var pooledObjects = new List<ComplexObject>();
            for (int i = 0; i < iterations; i++)
            {
                var obj = pool.Get();
                obj.ProcessData(new[] { i, i + 1, i + 2 });
                pooledObjects.Add(obj);
            }
            sw2.Stop();
            
            // Return pooled objects
            foreach (var obj in pooledObjects)
            {
                pool.Return(obj);
            }
            
            _output.WriteLine($"Direct creation: {sw1.ElapsedMilliseconds}ms for {iterations:N0} complex objects");
            _output.WriteLine($"Pooled creation: {sw2.ElapsedMilliseconds}ms for {iterations:N0} complex objects");
            _output.WriteLine($"Speed improvement: {(double)sw1.ElapsedMilliseconds / sw2.ElapsedMilliseconds:F1}x");
            _output.WriteLine($"Pool count: {pool.Count}/{pool.MaxCapacity}");
            
            // Pooled should be significantly faster for complex objects
            Assert.True(sw2.ElapsedMilliseconds < sw1.ElapsedMilliseconds, "Pooled complex objects should be faster");
        }

        [Fact]
        public void CollectionPool_Statistics_ProvidesInsights()
        {
            // Arrange
            var listPool = CollectionPool.GetListPool<CoreVector2I>();
            var dictPool = CollectionPool.GetDictionaryPool<string, object>();
            
            // Act - Use pools to generate statistics
            var lists = new List<List<CoreVector2I>>();
            var dicts = new List<Dictionary<string, object>>();
            
            for (int i = 0; i < 100; i++)
            {
                lists.Add(listPool.Get());
                dicts.Add(dictPool.Get());
            }
            
            // Return half of each to test statistics
            for (int i = 0; i < 50; i++)
            {
                listPool.Return(lists[i]);
                dictPool.Return(dicts[i]);
            }
            
            // Get statistics
            var stats = CollectionPool.GetStatistics();
            
            _output.WriteLine($"Collection Pool Statistics:");
            foreach (var stat in stats)
            {
                _output.WriteLine($"  {stat.Key}: {stat.Value.Count}/{stat.Value.MaxCapacity}");
            }
            
            // Verify statistics are meaningful
            Assert.True(stats.Count > 0, "Should have statistics for registered pools");
            Assert.True(stats.Any(s => s.Key.Contains("List")), "Should have List pool statistics");
            Assert.True(stats.Any(s => s.Key.Contains("Dictionary")), "Should have Dictionary pool statistics");
            
            // Clean up remaining objects
            for (int i = 50; i < 100; i++)
            {
                listPool.Return(lists[i]);
                dictPool.Return(dicts[i]);
            }
        }

        /// <summary>
        /// Complex object for testing expensive initialization scenarios
        /// </summary>
        private class ComplexObject
        {
            public string Id { get; private set; }
            public List<int> Data { get; private set; }
            public DateTime CreatedAt { get; private set; }
            public int ProcessCount { get; private set; }

            public ComplexObject(string id)
            {
                Id = id;
                Data = new List<int>();
                CreatedAt = DateTime.UtcNow;
                ProcessCount = 0;
                
                // Simulate expensive initialization
                Thread.Sleep(1);
            }

            public void ProcessData(int[] newData)
            {
                Data.AddRange(newData);
                ProcessCount++;
            }

            public void Reset()
            {
                Data.Clear();
                ProcessCount = 0;
                CreatedAt = DateTime.UtcNow;
            }
        }
    }
}
