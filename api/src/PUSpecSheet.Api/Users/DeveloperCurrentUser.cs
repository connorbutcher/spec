using PUSpecSheet.Application.Users;
using PUSpecSheet.Domain.Users;

namespace PUSpecSheet.Api.Users;

/// <summary>
/// The user the request was signed in as, read from its claims. Until sign-in is added that is the seeded
/// developer user (see <see cref="DeveloperAuthenticationHandler"/>), who is also who work outside a
/// request runs as, such as seeding at start-up. Real authentication only has to issue the same claim.
/// </summary>
public sealed class DeveloperCurrentUser(IHttpContextAccessor httpContext) : ICurrentUser
{
    public int UserId => httpContext.HttpContext?.User.FindUserId() ?? WellKnownUsers.DeveloperId;
}
