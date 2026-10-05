using System.Globalization;
using System.Security.Claims;
using System.Text.Encodings.Web;
using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Options;
using PUSpecSheet.Application.Users;

namespace PUSpecSheet.Api.Users;

/// <summary>
/// Signs every request in as the current user (the seeded developer user) until real sign-in is added,
/// so authorization has someone to allow or refuse. What that user may do still comes from their roles.
/// </summary>
public sealed class DeveloperAuthenticationHandler(
    IOptionsMonitor<DeveloperAuthenticationOptions> options,
    ILoggerFactory logger,
    UrlEncoder encoder,
    ICurrentUser currentUser)
    : AuthenticationHandler<DeveloperAuthenticationOptions>(options, logger, encoder)
{
    public const string SchemeName = "Developer";

    protected override Task<AuthenticateResult> HandleAuthenticateAsync()
    {
        var userId = currentUser.UserId.ToString(CultureInfo.InvariantCulture);
        var identity = new ClaimsIdentity([new Claim(ClaimTypes.NameIdentifier, userId)], SchemeName);
        var ticket = new AuthenticationTicket(new ClaimsPrincipal(identity), SchemeName);

        return Task.FromResult(AuthenticateResult.Success(ticket));
    }
}
