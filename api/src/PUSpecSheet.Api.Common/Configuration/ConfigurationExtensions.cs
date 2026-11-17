using Microsoft.Extensions.Configuration;

namespace PUSpecSheet.Api.Common.Configuration;

public static class ConfigurationExtensions
{
    public static string GetCacheKeyPrefix(this IConfiguration configuration)
    {
        return configuration["CacheKeyPrefix"] ?? string.Empty;
    }
}
