using System.Security.Claims;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Extensions.Configuration;
using PUSpecSheet.Api.Common.Context;

namespace PUSpecSheet.Api.Common.Authentication;

public sealed class UserAuthenticator
{
    private readonly IUserContext _userContext;
    private readonly IConfiguration _configuration;

    public UserAuthenticator(IUserContext userContext, IConfiguration configuration)
    {
        _userContext = userContext;
        _configuration = configuration;
    }

    public async Task Authenticate(TokenValidatedContext tokenValidatedContext)
    {
        if (tokenValidatedContext.Principal?.Identity is not ClaimsIdentity claimsIdentity || !claimsIdentity.IsAuthenticated)
        {
            tokenValidatedContext.Fail("Could not find ClaimsIdentity");
            return;
        }

        AuthenticationHelper.CheckAndHandleRobots(_configuration, claimsIdentity);
        var user = await AuthenticationHelper.TryGetUserAsync(_userContext, claimsIdentity, tokenValidatedContext.HttpContext.RequestAborted);
        if (user == null)
        {
            tokenValidatedContext.Fail("Could not find user from ObjectID.");
            return;
        }

        return;
    }
}
