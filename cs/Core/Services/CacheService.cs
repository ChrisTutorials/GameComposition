using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.ObjectPool;
using System.Collections.Concurrent;

namespace BarkMoon.GameComposition.Core.Services
{
    /// <summary>
    /// High-performance caching service using Microsoft.Extensions.Caching.Memory.
    /// Provides thread-safe memory caching with configurable expiration policies.
    /// </summary>
    public interface ICacheService
    {
        /// <summary>
        /// Gets a value from the cache or creates it if it doesn't exist.
        /// </summary>
        /// <typeparam name="T">The type of value to cache</typeparam>
        /// <param name="key">The cache key</param>
        /// <param name="factory">Factory function to create the value if not cached</param>
        /// <param name="expiration">Optional expiration time</param>
        /// <returns>The cached or newly created value</returns>
        T GetOrCreate<T>(string key, Func<T> factory, TimeSpan? expiration = null);

        /// <summary>
        /// Gets a value from the cache asynchronously or creates it if it doesn't exist.
        /// </summary>
        /// <typeparam name="T">The type of value to cache</typeparam>
        /// <param name="key">The cache key</param>
        /// <param name="factory">Async factory function to create the value if not cached</param>
        /// <param name="expiration">Optional expiration time</param>
        /// <returns>The cached or newly created value</returns>
        Task<T> GetOrCreateAsync<T>(string key, Func<Task<T>> factory, TimeSpan? expiration = null);

        /// <summary>
        /// Removes a value from the cache.
        /// </summary>
        /// <param name="key">The cache key to remove</param>
        /// <returns>True if the value was removed, false if it didn't exist</returns>
        bool Remove(string key);

        /// <summary>
        /// Checks if a key exists in the cache.
        /// </summary>
        /// <param name="key">The cache key to check</param>
        /// <returns>True if the key exists, false otherwise</returns>
        bool Contains(string key);

        /// <summary>
        /// Clears all cached values.
        /// </summary>
        void Clear();
    }

    /// <summary>
    /// Implementation of ICacheService using Microsoft.Extensions.Caching.Memory.
    /// Provides high-performance, thread-safe memory caching with expiration policies.
    /// </summary>
    public class CacheService : ICacheService
    {
        private readonly IMemoryCache _cache;
        private readonly ObjectPool<MemoryCacheEntryOptions> _optionsPool;

        public CacheService(IMemoryCache cache, ObjectPool<MemoryCacheEntryOptions> optionsPool)
        {
            _cache = cache ?? throw new ArgumentNullException(nameof(cache));
            _optionsPool = optionsPool ?? throw new ArgumentNullException(nameof(optionsPool));
        }

        /// <summary>
        /// Gets a value from the cache or creates it if it doesn't exist.
        /// </summary>
        public T GetOrCreate<T>(string key, Func<T> factory, TimeSpan? expiration = null)
        {
            if (string.IsNullOrEmpty(key))
                throw new ArgumentException("Cache key cannot be null or empty", nameof(key));

            if (factory == null)
                throw new ArgumentNullException(nameof(factory));

            var options = _optionsPool.Get();
            try
            {
                if (expiration.HasValue)
                {
                    options.AbsoluteExpirationRelativeToNow = expiration.Value;
                }

                return _cache.GetOrCreate(key, entry =>
                {
                    if (expiration.HasValue)
                    {
                        entry.AbsoluteExpirationRelativeToNow = expiration.Value;
                    }
                    return factory();
                });
            }
            finally
            {
                _optionsPool.Return(options);
            }
        }

        /// <summary>
        /// Gets a value from the cache asynchronously or creates it if it doesn't exist.
        /// </summary>
        public async Task<T> GetOrCreateAsync<T>(string key, Func<Task<T>> factory, TimeSpan? expiration = null)
        {
            if (string.IsNullOrEmpty(key))
                throw new ArgumentException("Cache key cannot be null or empty", nameof(key));

            if (factory == null)
                throw new ArgumentNullException(nameof(factory));

            var options = _optionsPool.Get();
            try
            {
                if (expiration.HasValue)
                {
                    options.AbsoluteExpirationRelativeToNow = expiration.Value;
                }

                return await _cache.GetOrCreateAsync(key, async entry =>
                {
                    if (expiration.HasValue)
                    {
                        entry.AbsoluteExpirationRelativeToNow = expiration.Value;
                    }
                    return await factory();
                });
            }
            finally
            {
                _optionsPool.Return(options);
            }
        }

        /// <summary>
        /// Removes a value from the cache.
        /// </summary>
        public bool Remove(string key)
        {
            if (string.IsNullOrEmpty(key))
                return false;

            _cache.Remove(key);
            return true;
        }

        /// <summary>
        /// Checks if a key exists in the cache.
        /// </summary>
        public bool Contains(string key)
        {
            if (string.IsNullOrEmpty(key))
                return false;

            return _cache.TryGetValue(key, out _);
        }

        /// <summary>
        /// Clears all cached values.
        /// </summary>
        public void Clear()
        {
            if (_cache is MemoryCache memoryCache)
            {
                memoryCache.Compact(1.0); // Compact to 0% size
            }
        }
    }

    /// <summary>
    /// Extension methods for registering caching services with DI container.
    /// </summary>
    public static class CacheServiceExtensions
    {
        /// <summary>
        /// Adds caching services to the service collection.
        /// </summary>
        /// <param name="services">The service collection</param>
        /// <param name="configureCache">Optional configuration for the memory cache</param>
        /// <returns>The service collection for chaining</returns>
        public static IServiceCollection AddCacheServices(
            this IServiceCollection services,
            Action<MemoryCacheOptions>? configureCache = null)
        {
            services.AddMemoryCache(configureCache);
            services.AddSingleton<ICacheService, CacheService>();
            
            // Register object pool for MemoryCacheEntryOptions to reduce allocations
            services.AddSingleton<ObjectPool<MemoryCacheEntryOptions>>(provider =>
            {
                var options = new DefaultPooledObjectPolicy<MemoryCacheEntryOptions>();
                return new DefaultObjectPool<MemoryCacheEntryOptions>(options);
            });

            return services;
        }
    }
}
