using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Text;
using System.Text.Json;

namespace FSH.Framework.Caching;

public sealed class HybridCacheService : ICacheService
{
    private static readonly Encoding Utf8 = Encoding.UTF8;
    private static readonly JsonSerializerOptions JsonOpts = new() { PropertyNamingPolicy = JsonNamingPolicy.CamelCase };

    private readonly IMemoryCache _memoryCache;
    private readonly IDistributedCache _distributedCache;
    private readonly ILogger<HybridCacheService> _logger;
    private readonly CachingOptions _opts;

    public HybridCacheService(
        IMemoryCache memoryCache,
        IDistributedCache distributedCache,
        ILogger<HybridCacheService> logger,
        IOptions<CachingOptions> opts)
    {
        ArgumentNullException.ThrowIfNull(opts);

        _memoryCache = memoryCache;
        _distributedCache = distributedCache;
        _logger = logger;
        _opts = opts.Value;
    }

    public async Task<T?> GetItemAsync<T>(string key, CancellationToken ct = default)
    {
        key = Normalize(key);
        try
        {
            // Check L1 cache first (memory)
            if (_memoryCache.TryGetValue(key, out T? memoryValue))
            {
                _logger.LogDebug("Cache hit in memory for {Key}", key);
                return memoryValue;
            }

            // Fall back to L2 cache (distributed)
            var bytes = await _distributedCache.GetAsync(key, ct).ConfigureAwait(false);
            if (bytes is null || bytes.Length == 0) return default;

            var value = JsonSerializer.Deserialize<T>(Utf8.GetString(bytes), JsonOpts);
            
            // Populate L1 cache from L2
            if (value is not null)
            {
                var expiration = GetMemoryCacheExpiration();
                _memoryCache.Set(key, value, expiration);
                _logger.LogDebug("Populated memory cache from distributed cache for {Key}", key);
            }

            return value;
        }
        catch (Exception ex) when (ex is not OperationCanceledException)
        {
            _logger.LogWarning(ex, "Cache get failed for {Key}", key);
            return default;
        }
    }

    public async Task SetItemAsync<T>(string key, T value, TimeSpan? sliding = default, CancellationToken ct = default)
    {
        key = Normalize(key);
        try
        {
            var bytes = Utf8.GetBytes(JsonSerializer.Serialize(value, JsonOpts));
            await _distributedCache.SetAsync(key, bytes, BuildDistributedEntryOptions(sliding), ct).ConfigureAwait(false);
            
            // Also set in memory cache
            var expiration = GetMemoryCacheExpiration();
            _memoryCache.Set(key, value, expiration);
            
            _logger.LogDebug("Cached {Key} in both memory and distributed caches", key);
        }
        catch (Exception ex) when (ex is not OperationCanceledException)
        {
            _logger.LogWarning(ex, "Cache set failed for {Key}", key);
        }
    }

    public async Task RemoveItemAsync(string key, CancellationToken ct = default)
    {
        key = Normalize(key);
        try
        {
            // Remove from both caches
            _memoryCache.Remove(key);
            await _distributedCache.RemoveAsync(key, ct).ConfigureAwait(false);
            _logger.LogDebug("Removed {Key} from both caches", key);
        }
        catch (Exception ex) when (ex is not OperationCanceledException)
        {
            _logger.LogWarning(ex, "Cache remove failed for {Key}", key);
        }
    }

    public async Task RefreshItemAsync(string key, CancellationToken ct = default)
    {
        key = Normalize(key);
        try
        {
            await _distributedCache.RefreshAsync(key, ct).ConfigureAwait(false);
            _logger.LogDebug("Refreshed {Key}", key);
        }
        catch (Exception ex) when (ex is not OperationCanceledException)
        {
            _logger.LogWarning(ex, "Cache refresh failed for {Key}", key);
        }
    }

    public T? GetItem<T>(string key) => GetItemAsync<T>(key).GetAwaiter().GetResult();
    public void SetItem<T>(string key, T value, TimeSpan? sliding = default) => SetItemAsync(key, value, sliding).GetAwaiter().GetResult();
    public void RemoveItem(string key) => RemoveItemAsync(key).GetAwaiter().GetResult();
    public void RefreshItem(string key) => RefreshItemAsync(key).GetAwaiter().GetResult();

    private DistributedCacheEntryOptions BuildDistributedEntryOptions(TimeSpan? sliding)
    {
        var o = new DistributedCacheEntryOptions();

        if (sliding.HasValue)
            o.SetSlidingExpiration(sliding.Value);
        else if (_opts.DefaultSlidingExpiration.HasValue)
            o.SetSlidingExpiration(_opts.DefaultSlidingExpiration.Value);

        if (_opts.DefaultAbsoluteExpiration.HasValue)
            o.SetAbsoluteExpiration(_opts.DefaultAbsoluteExpiration.Value);

        return o;
    }

    private MemoryCacheEntryOptions GetMemoryCacheExpiration()
    {
        var options = new MemoryCacheEntryOptions();

        // Use shorter expiration for memory cache (faster refresh from distributed cache)
        var slidingExpiration = _opts.DefaultSlidingExpiration ?? TimeSpan.FromMinutes(1);
        options.SetSlidingExpiration(TimeSpan.FromSeconds(slidingExpiration.TotalSeconds * 0.8)); // 80% of distributed cache expiration

        return options;
    }

    private string Normalize(string key)
    {
        if (string.IsNullOrWhiteSpace(key)) throw new ArgumentNullException(nameof(key));
        var prefix = _opts.KeyPrefix ?? string.Empty;
        if (prefix.Length == 0)
        {
            return key;
        }

        return key.StartsWith(prefix, StringComparison.Ordinal)
            ? key
            : prefix + key;
    }
}
