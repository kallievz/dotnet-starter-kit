using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using StackExchange.Redis;

namespace FSH.Framework.Caching;

public static class Extensions
{
    public static IServiceCollection AddHeroCaching(this IServiceCollection services, IConfiguration configuration)
    {
        ArgumentNullException.ThrowIfNull(configuration);

        services
            .AddOptions<CachingOptions>()
            .BindConfiguration(nameof(CachingOptions));

        // Always add memory cache for L1
        services.AddMemoryCache();

        var cacheOptions = configuration.GetSection(nameof(CachingOptions)).Get<CachingOptions>();
        if (cacheOptions == null || string.IsNullOrEmpty(cacheOptions.Redis))
        {
            // If no Redis, use memory cache for L2 as well
            services.AddDistributedMemoryCache();
            services.AddTransient<ICacheService, HybridCacheService>();
            return services;
        }

        // Use Redis for L2 cache
        services.AddStackExchangeRedisCache(options =>
        {
            var config = ConfigurationOptions.Parse(cacheOptions.Redis);
            config.AbortOnConnectFail = true;

            options.ConfigurationOptions = config;
        });

        // Register hybrid cache service
        services.AddTransient<ICacheService, HybridCacheService>();

        return services;
    }
}