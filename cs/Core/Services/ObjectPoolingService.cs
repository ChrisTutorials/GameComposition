using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.ObjectPool;
using System.Collections.Concurrent;

namespace BarkMoon.GameComposition.Core.Services
{
    /// <summary>
    /// High-performance pooled object factory for frequently created objects.
    /// Reduces GC pressure by reusing objects through object pooling.
    /// </summary>
    /// <typeparam name="T">The type of object to pool</typeparam>
    public interface IObjectPoolFactory<T> where T : class
    {
        /// <summary>
        /// Gets an object from the pool or creates a new one if none are available.
        /// </summary>
        /// <returns>A pooled or newly created object</returns>
        T Acquire();

        /// <summary>
        /// Returns an object to the pool for reuse.
        /// </summary>
        /// <param name="obj">The object to return to the pool</param>
        void Return(T obj);
    }

    /// <summary>
    /// Implementation of IObjectPoolFactory using Microsoft.Extensions.ObjectPool.
    /// Provides high-performance object pooling with configurable policies.
    /// </summary>
    /// <typeparam name="T">The type of object to pool</typeparam>
    public class ObjectPoolFactory<T> : IObjectPoolFactory<T> where T : class
    {
        private readonly ObjectPool<T> _pool;

        public ObjectPoolFactory(ObjectPool<T> pool)
        {
            _pool = pool ?? throw new ArgumentNullException(nameof(pool));
        }

        /// <summary>
        /// Gets an object from the pool or creates a new one if none are available.
        /// </summary>
        public T Acquire() => _pool.Get();

        /// <summary>
        /// Returns an object to the pool for reuse.
        /// </summary>
        public void Return(T obj) => _pool.Return(obj);
    }

    /// <summary>
    /// Factory for creating commonly used pooled objects in GameComposition.
    /// Provides pre-configured pools for high-frequency object types.
    /// </summary>
    public class CommonObjectPools
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly ConcurrentDictionary<Type, object> _pools = new();

        public CommonObjectPools(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider ?? throw new ArgumentNullException(nameof(serviceProvider));
        }

        /// <summary>
        /// Gets a pooled object factory for the specified type.
        /// </summary>
        /// <typeparam name="T">The type of object to pool</typeparam>
        /// <returns>A pooled object factory</returns>
        public IObjectPoolFactory<T> GetPool<T>() where T : class
        {
            var type = typeof(T);
            
            if (_pools.TryGetValue(type, out var pool))
            {
                return (IObjectPoolFactory<T>)pool;
            }

            var factory = _serviceProvider.GetRequiredService<IObjectPoolFactory<T>>();
            _pools.TryAdd(type, factory);
            return factory;
        }

        /// <summary>
        /// Gets a pooled List{T} factory.
        /// </summary>
        /// <typeparam name="T">The element type</typeparam>
        /// <returns>A pooled List{T} factory</returns>
        public IObjectPoolFactory<List<T>> GetListPool<T>()
        {
            var type = typeof(List<T>);
            
            if (_pools.TryGetValue(type, out var pool))
            {
                return (IObjectPoolFactory<List<T>>)pool;
            }

            var factory = _serviceProvider.GetRequiredService<IObjectPoolFactory<List<T>>>();
            _pools.TryAdd(type, factory);
            return factory;
        }

        /// <summary>
        /// Gets a pooled Dictionary{TKey, TValue} factory.
        /// </summary>
        /// <typeparam name="TKey">The key type</typeparam>
        /// <typeparam name="TValue">The value type</typeparam>
        /// <returns>A pooled Dictionary{TKey, TValue} factory</returns>
        public IObjectPoolFactory<Dictionary<TKey, TValue>> GetDictionaryPool<TKey, TValue>()
            where TKey : notnull
        {
            var type = typeof(Dictionary<TKey, TValue>);
            
            if (_pools.TryGetValue(type, out var pool))
            {
                return (IObjectPoolFactory<Dictionary<TKey, TValue>>)pool;
            }

            var factory = _serviceProvider.GetRequiredService<IObjectPoolFactory<Dictionary<TKey, TValue>>>();
            _pools.TryAdd(type, factory);
            return factory;
        }
    }

    /// <summary>
    /// Extension methods for registering object pooling services with DI container.
    /// </summary>
    public static class ObjectPoolingExtensions
    {
        /// <summary>
        /// Adds object pooling services to the service collection.
        /// </summary>
        /// <param name="services">The service collection</param>
        /// <returns>The service collection for chaining</returns>
        public static IServiceCollection AddObjectPooling(this IServiceCollection services)
        {
            // Register common pool factories
            services.AddSingleton<IObjectPoolFactory<object>, ObjectPoolFactory<object>>();
            
            // Register List pools
            services.AddSingleton<IObjectPoolFactory<List<string>>, ObjectPoolFactory<List<string>>>();
            services.AddSingleton<IObjectPoolFactory<List<int>>, ObjectPoolFactory<List<int>>>();
            services.AddSingleton<IObjectPoolFactory<List<object>>, ObjectPoolFactory<List<object>>>();
            
            // Register Dictionary pools
            services.AddSingleton<IObjectPoolFactory<Dictionary<string, object>>, ObjectPoolFactory<Dictionary<string, object>>>();
            services.AddSingleton<IObjectPoolFactory<Dictionary<string, string>>, ObjectPoolFactory<Dictionary<string, string>>>();
            
            // Register CommonObjectPools
            services.AddSingleton<CommonObjectPools>();

            return services;
        }

        /// <summary>
        /// Adds a specific object pool for the given type.
        /// </summary>
        /// <typeparam name="T">The type to pool</typeparam>
        /// <param name="services">The service collection</param>
        /// <param name="policy">Optional custom pooling policy</param>
        /// <returns>The service collection for chaining</returns>
        public static IServiceCollection AddObjectPool<T>(
            this IServiceCollection services,
            IPooledObjectPolicy<T>? policy = null) where T : class, new()
        {
            var poolPolicy = policy ?? new DefaultPooledObjectPolicy<T>();
            services.AddSingleton<ObjectPool<T>>(provider => new DefaultObjectPool<T>(poolPolicy));
            services.AddSingleton<IObjectPoolFactory<T>>(provider => 
                new ObjectPoolFactory<T>(provider.GetRequiredService<ObjectPool<T>>()));

            return services;
        }

        /// <summary>
        /// Adds a List{T} object pool.
        /// </summary>
        /// <typeparam name="T">The element type</typeparam>
        /// <param name="services">The service collection</param>
        /// <returns>The service collection for chaining</returns>
        public static IServiceCollection AddListPool<T>(this IServiceCollection services)
        {
            var policy = new DefaultPooledObjectPolicy<List<T>>();
            
            services.AddSingleton<ObjectPool<List<T>>>(provider => new DefaultObjectPool<List<T>>(policy));
            services.AddSingleton<IObjectPoolFactory<List<T>>>(provider => 
                new ObjectPoolFactory<List<T>>(provider.GetRequiredService<ObjectPool<List<T>>>()));

            return services;
        }

        /// <summary>
        /// Adds a Dictionary{TKey, TValue} object pool.
        /// </summary>
        /// <typeparam name="TKey">The key type</typeparam>
        /// <typeparam name="TValue">The value type</typeparam>
        /// <param name="services">The service collection</param>
        /// <returns>The service collection for chaining</returns>
        public static IServiceCollection AddDictionaryPool<TKey, TValue>(
            this IServiceCollection services) where TKey : notnull
        {
            var policy = new DefaultPooledObjectPolicy<Dictionary<TKey, TValue>>();
            
            services.AddSingleton<ObjectPool<Dictionary<TKey, TValue>>>(provider => 
                new DefaultObjectPool<Dictionary<TKey, TValue>>(policy));
            services.AddSingleton<IObjectPoolFactory<Dictionary<TKey, TValue>>>(provider => 
                new ObjectPoolFactory<Dictionary<TKey, TValue>>(
                    provider.GetRequiredService<ObjectPool<Dictionary<TKey, TValue>>>()));

            return services;
        }
    }
}
