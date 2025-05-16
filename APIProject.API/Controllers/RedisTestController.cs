using APIProject.Application.Interfaces;
using Microsoft.AspNetCore.Mvc;
using System.Text.Json;

namespace APIProject.API.Controllers
{
    [Route("api/redis-test")]
    [ApiController]
    public class RedisTestController : ControllerBase
    {
        private readonly ICacheService _cacheService;
        private readonly ILogger<RedisTestController> _logger;

        public RedisTestController(ICacheService cacheService, ILogger<RedisTestController> logger)
        {
            _cacheService = cacheService;
            _logger = logger;
        }

        [HttpGet("status")]
        public async Task<IActionResult> GetStatus()
        {
            try
            {
                string testKey = "redis-test-key";
                string testValue = $"Test value at {DateTime.UtcNow}";

                _logger.LogInformation("Setting test value in Redis...");
                await _cacheService.SetAsync(testKey, testValue, TimeSpan.FromMinutes(5));

                _logger.LogInformation("Reading test value from Redis...");
                string? retrievedValue = await _cacheService.GetAsync<string>(testKey);

                bool exists = await _cacheService.ExistsAsync(testKey);

                var result = new
                {
                    Status = "Connected",
                    TestKeyExists = exists,
                    OriginalValue = testValue,
                    RetrievedValue = retrievedValue,
                    CurrentTime = DateTime.UtcNow
                };

                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error connecting to Redis");
                return StatusCode(500, new { Status = "Error", Message = "Failed to connect to Redis", Error = ex.Message });
            }
        }

        [HttpPost("set")]
        public async Task<IActionResult> SetValue([FromBody] CacheEntryDto cacheEntry)
        {
            try
            {
                await _cacheService.SetAsync(
                    cacheEntry.Key, 
                    cacheEntry.Value, 
                    cacheEntry.ExpiryMinutes.HasValue 
                        ? TimeSpan.FromMinutes(cacheEntry.ExpiryMinutes.Value) 
                        : null);

                return Ok(new { Message = $"Value set for key: {cacheEntry.Key}" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error setting value in Redis");
                return StatusCode(500, new { Status = "Error", Message = "Failed to set value in Redis", Error = ex.Message });
            }
        }

        [HttpGet("get/{key}")]
        public async Task<IActionResult> GetValue(string key)
        {
            try
            {
                var value = await _cacheService.GetAsync<object>(key);
                if (value == null)
                {
                    return NotFound(new { Message = $"No value found for key: {key}" });
                }

                return Ok(new { Key = key, Value = value });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting value from Redis");
                return StatusCode(500, new { Status = "Error", Message = "Failed to get value from Redis", Error = ex.Message });
            }
        }

        [HttpDelete("delete/{key}")]
        public async Task<IActionResult> DeleteValue(string key)
        {
            try
            {
                bool exists = await _cacheService.ExistsAsync(key);
                if (!exists)
                {
                    return NotFound(new { Message = $"No value found for key: {key}" });
                }

                await _cacheService.RemoveAsync(key);
                return Ok(new { Message = $"Value removed for key: {key}" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting value from Redis");
                return StatusCode(500, new { Status = "Error", Message = "Failed to delete value from Redis", Error = ex.Message });
            }
        }

        public class CacheEntryDto
        {
            public string Key { get; set; } = string.Empty;
            public object Value { get; set; } = default!;
            public int? ExpiryMinutes { get; set; }
        }
    }
}