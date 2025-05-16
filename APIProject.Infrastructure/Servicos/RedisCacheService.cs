using APIProject.Application.Interfaces;
using APIProject.Infrastructure.Configuracoes;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Options;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace APIProject.Infrastructure.Servicos
{
    public class RedisCacheService : ICacheService
    {
        private readonly IDistributedCache _distributedCache;
        private readonly RedisConfiguracoes _redisConfig;
        private readonly JsonSerializerOptions _jsonOptions;

        public RedisCacheService(IDistributedCache distributedCache, IOptions<RedisConfiguracoes> redisConfig)
        {
            _distributedCache = distributedCache;
            _redisConfig = redisConfig.Value;

            // Configurar JsonSerializerOptions para usar construtores sem parâmetros
            _jsonOptions = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true,
                WriteIndented = false,
                DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
                ReferenceHandler = ReferenceHandler.Preserve
            };
        }

        public async Task<T?> GetAsync<T>(string key)
        {
            string? cachedValue = await _distributedCache.GetStringAsync(key);

            if (string.IsNullOrEmpty(cachedValue))
                return default;

            try
            {
                return JsonSerializer.Deserialize<T>(cachedValue, _jsonOptions);
            }
            catch (Exception ex)
            {
                // Log erro e retornar null
                Console.WriteLine($"Erro ao deserializar do cache: {ex.Message}");
                return default;
            }
        }

        public async Task SetAsync<T>(string key, T value, TimeSpan? expiry = null)
        {
            try
            {
                string serializedValue = JsonSerializer.Serialize(value, _jsonOptions);

                var options = new DistributedCacheEntryOptions
                {
                    AbsoluteExpirationRelativeToNow = expiry ?? TimeSpan.FromMinutes(_redisConfig.AbsoluteExpirationInMinutes),
                    SlidingExpiration = TimeSpan.FromMinutes(_redisConfig.SlidingExpirationInMinutes)
                };

                await _distributedCache.SetStringAsync(key, serializedValue, options);
            }
            catch (Exception ex)
            {
                // Log erro mas não falha
                Console.WriteLine($"Erro ao serializar para o cache: {ex.Message}");
            }
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

            try
            {
                // Se o cache falhou, vamos buscar do banco de dados
                T newValue = await factory();

                // Tentar armazenar no cache, mas se falhar, apenas retorna o valor
                await SetAsync(key, newValue, expiry);

                return newValue;
            }
            catch (Exception ex)
            {
                // Em caso de erro grave, log e rethrow
                Console.WriteLine($"Erro grave ao executar factory: {ex.Message}");
                throw;
            }
        }
    }
}