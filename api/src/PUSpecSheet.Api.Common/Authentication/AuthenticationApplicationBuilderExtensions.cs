using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Identity.Web;

namespace PUSpecSheet.Api.Common.Authentication;

public static class AuthenticationApplicationBuilderExtensions
{
    public static IHostApplicationBuilder AddIdentityWebApiAuthentication(this IHostApplicationBuilder builder)
    {
        builder.Services.AddScoped<UserAuthenticator>();

        builder.Services
           .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
           .AddMicrosoftIdentityWebApi(builder.Configuration);

        builder.Services.Configure<JwtBearerOptions>(JwtBearerDefaults.AuthenticationScheme, options =>
        {
            options.Events = new JwtBearerEvents
            {
                OnMessageReceived = JwtBearerEventHandlers.QueryBearerTokenMessageHandlerAsync,
                OnTokenValidated = async (context) =>
                {
                    var authenticator = context.HttpContext.RequestServices.GetRequiredService<UserAuthenticator>();
                    await authenticator.Authenticate(context);
                }
            };
        });

        return builder;
    }
}
