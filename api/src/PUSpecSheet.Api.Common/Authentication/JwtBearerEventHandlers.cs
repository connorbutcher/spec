using System.Net;
using Microsoft.AspNetCore.Authentication.JwtBearer;

namespace PUSpecSheet.Api.Common.Authentication;

public static class JwtBearerEventHandlers
{
    public static Task QueryBearerTokenMessageHandlerAsync(this MessageReceivedContext context)
    {
        if (!context.Request.Query.TryGetValue("access_token", out var values) || values.Count == 0)
        {
            return Task.CompletedTask;
        }

        if (values.Count > 1)
        {
            return HandleFailure(context, HttpStatusCode.Unauthorized,
                $"Only one 'access_token' query string parameter can be defined. However, {values.Count:N0} were included in the request.");
        }

        var token = values.Single();

        if (string.IsNullOrWhiteSpace(token))
        {
            return HandleFailure(context, HttpStatusCode.Unauthorized,
                "The 'access_token' query string parameter was defined, but a value to represent the token was not included.");
        }

        context.Token = token;
        return Task.CompletedTask;
    }

    private static Task HandleFailure(MessageReceivedContext context, HttpStatusCode statusCode, string errorMessage)
    {
        context.Response.StatusCode = (int)statusCode;
        context.Fail(errorMessage);
        return Task.CompletedTask;
    }
}
