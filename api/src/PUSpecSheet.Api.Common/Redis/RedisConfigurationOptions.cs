using Microsoft.Extensions.Configuration;
using StackExchange.Redis;

namespace PUSpecSheet.Api.Common.Redis;

public static class RedisConfigurationOptions
{
    public static ConfigurationOptions GetConfigurationOptions(IConfigurationSection configurationSection)
    {
        var commands = new Dictionary<string, string?>
        {
            {"cluster", null }, //Disable cluster commands
            {"config", null } // Disable config commands
        };

        var endpoint = EndPointCollection.TryParse(configurationSection.GetRequiredValue<string>("Endpoint")) ??
                       throw new InvalidOperationException("Invalid Redis endpoint configuration");

        var channelPrefix = configurationSection.GetValue<string>("ChannelPrefix");
        if (string.IsNullOrWhiteSpace(channelPrefix))
        {
            return new ConfigurationOptions
            {
                EndPoints = new EndPointCollection([endpoint]),
                User = configurationSection.GetRequiredValue<string>("Auth:User"),
                Password = configurationSection.GetRequiredValue<string>("Auth:Password"),
                DefaultDatabase = 0,
                CommandMap = CommandMap.Create(commands)
            };
        }

        return new ConfigurationOptions
        {
            EndPoints = new EndPointCollection([endpoint]),
            User = configurationSection.GetRequiredValue<string>("Auth:User"),
            Password = configurationSection.GetRequiredValue<string>("Auth:Password"),
            DefaultDatabase = 0,
            ChannelPrefix = new RedisChannel(channelPrefix, RedisChannel.PatternMode.Literal),
            CommandMap = CommandMap.Create(commands)
        };
    }

    private static T GetRequiredValue<T>(this IConfiguration configuration, string key)
    {
        var value = configuration.GetValue<T>(key);

        return value is null ?
            throw new InvalidOperationException($"Configuration value for key '{key}' is required but not found") :
            value;
    }
}
