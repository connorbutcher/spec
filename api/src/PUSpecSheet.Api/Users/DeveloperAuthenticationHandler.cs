using System.Globalization;
using System.Security.Claims;
using System.Text.Encodings.Web;
using Microsoft.AspNetCore.Authentication;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using PUSpecSheet.Data;
using PUSpecSheet.Domain.Users;

namespace PUSpecSheet.Api.Users;

/// <summary>
/// Signs every request in as the seeded developer user until real sign-in is added, so authorization has
/// someone to allow or refuse. What that user may do still comes from their roles.
/// <para>
/// With <see cref="DeveloperAuthenticationOptions.AllowUserSwitching"/> on, a request can name another
/// user by user name, in the <see cref="UserHeader"/> header or, for a WebSocket that can't send headers,
/// the <see cref="UserQuery"/> query parameter.
/// </para>
/// </summary>
public sealed class DeveloperAuthenticationHandler(
    IOptionsMonitor<DeveloperAuthenticationOptions> options,
    ILoggerFactory logger,
    UrlEncoder encoder,
    PuSpecSheetDbContext db)
    : AuthenticationHandler<DeveloperAuthenticationOptions>(options, logger, encoder)
{
    public const string SchemeName = "Developer";

    public const string UserHeader = "X-Developer-User";

    public const string UserQuery = "developerUser";

    protected override async Task<AuthenticateResult> HandleAuthenticateAsync()
    {
        var userId = await ResolveUserIdAsync();
        if (userId is null)
        {
            return AuthenticateResult.Fail("That developer user doesn't exist.");
        }

        var identity = new ClaimsIdentity(
            [new Claim(ClaimTypes.NameIdentifier, userId.Value.ToString(CultureInfo.InvariantCulture))],
            SchemeName);
        return AuthenticateResult.Success(new AuthenticationTicket(new ClaimsPrincipal(identity), SchemeName));
    }

    private async Task<int?> ResolveUserIdAsync()
    {
        var userName = Options.AllowUserSwitching
            ? Request.Headers[UserHeader].FirstOrDefault() ?? Request.Query[UserQuery].FirstOrDefault()
            : null;
        if (string.IsNullOrWhiteSpace(userName))
        {
            return WellKnownUsers.DeveloperId;
        }

        return await db.Users
            .Where(user => user.UserName == userName && user.IsActive)
            .Select(user => (int?)user.Id)
            .SingleOrDefaultAsync(Context.RequestAborted);
    }
}
