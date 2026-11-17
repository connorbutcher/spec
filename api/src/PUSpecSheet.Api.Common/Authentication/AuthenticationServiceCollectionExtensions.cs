using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Extensions.DependencyInjection;

namespace PUSpecSheet.Api.Common.Authentication;

public static class AuthenticationServiceCollectionExtensions
{
    public static IServiceCollection AddPUSpecSheetAuthentication(this IServiceCollection services)
    {
        services
            .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
            .AddScheme<JwtBearerOptions, UserAuthenticationHandler>(JwtBearerDefaults.AuthenticationScheme, options =>
            {
                options.Events = new JwtBearerEvents
                {
                    OnMessageReceived = JwtBearerEventHandlers.QueryBearerTokenMessageHandlerAsync
                };
            })
            .AddJwtBearer("");

        return services;
    }
}
