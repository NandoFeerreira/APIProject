using APIProject.Application.Interfaces;
using APIProject.Infrastructure.Configuracoes;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Options;
using System.Text.Json;

namespace APIProject.Infrastructure.Servicos
{
    public class RedisCacheService : ICacheService
    {
        private readonly IDistributedCache _distributedCache;
        private readonly RedisConfiguracoes _redisConfig;

        public RedisCacheService(IDistributedCache distributedCache, IOptions<RedisConfiguracoes> redisConfig)
        {
            _distributedCache = distributedCache;
            _redisConfig = redisConfig.Value;
        }

        public async Task<T?> GetAsync<T>(string key)
        {
            string? cachedValue = await _distributedCache.GetStringAsync(key);
            
            if (string.IsNullOrEmpty(cachedValue))
                return default;

            return JsonSerializer.Deserialize<T>(cachedValue);
        }

        public async Task SetAsync<T>(string key, T value, TimeSpan? expiry = null)
        {
            string serializedValue = JsonSerializer.Serialize(value);
            
            var options = new DistributedCacheEntryOptions
            {
                AbsoluteExpirationRelativeToNow = expiry ?? TimeSpan.FromMinutes(_redisConfig.AbsoluteExpirationInMinutes),
                SlidingExpiration = TimeSpan.FromMinutes(_redisConfig.SlidingExpirationInMinutes)
            };

            await _distributedCache.SetStringAsync(key, serializedValue, options);
        }

        public async Task RemoveAsync(string key)
        {
            await _distributedCache.RemoveAsync(key);
        }

        public async Task<bool> ExistsAsync(string key)
        {
            return await GetAsync<object>(key) != null;
        }

        public async Task<T> GetOrCreateAsync<T>(string key, Func<Task<T>> factory, TimeSpan? expiry = null)
        {
            T? cachedValue = await GetAsync<T>(key);
            
            if (cachedValue != null)
                return cachedValue;

            T newValue = await factory();
            await SetAsync(key, newValue, expiry);
            return newValue;
        }
    }
}