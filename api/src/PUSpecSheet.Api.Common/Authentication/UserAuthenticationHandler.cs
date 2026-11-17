using System.Security.Claims;
using System.Text.Encodings.Web;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using PUSpecSheet.Api.Common.Context;

namespace PUSpecSheet.Api.Common.Authentication;

public class UserAuthenticationHandler : JwtBearerHandler
{
    private readonly IUserContext _userContext;
    private readonly IConfiguration _configuration;

    public UserAuthenticationHandler(
        IUserContext userContext,
        IOptionsMonitor<JwtBearerOptions> options,
        IConfiguration configuration,
        ILoggerFactory logger,
        UrlEncoder encoder)
        : base(options, logger, encoder)
    {
        _userContext = userContext;
        _configuration = configuration;
    }

    protected override async Task<AuthenticateResult> HandleAuthenticateAsync()
    {
        var jwtAuthResult = await base.HandleAuthenticateAsync();

        if (!jwtAuthResult.Succeeded)
        {
            return jwtAuthResult;
        }

        if (jwtAuthResult.Principal.Identity is not ClaimsIdentity claimsIdentity || !claimsIdentity.IsAuthenticated)
        {
            return AuthenticateResult.Fail($"Could not find ClaimsIdentity");
        }

        AuthenticationHelper.CheckAndHandleRobots(_configuration, claimsIdentity);

        var user = await AuthenticationHelper.TryGetUserAsync(_userContext, claimsIdentity, Request.HttpContext.RequestAborted);
        if (user == null)
        {
            return AuthenticateResult.Fail($"Could not find user from ObjectID.");
        }

        var authTicket = new AuthenticationTicket(jwtAuthResult.Principal, Scheme.Name);

        return AuthenticateResult.Success(authTicket);
    }
}
